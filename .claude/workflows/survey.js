export const meta = {
  name: 'survey',
  whenToUse: 'Deep-research the modern external packages a target planning folder is missing — packages that REPLACE hand-rolled design-page capability or ADD genuine domain capability — then execute end to end in one run: central admission with gates, full-depth .api catalogs, registry closure, and immediate holistic integration into the design pages. args = a planning folder path, an array of paths, or {targets}; target lanes run CONCURRENTLY — only the Admit stage (the central-manifest writer) serializes across targets.',
  description: 'Package survey-and-integrate over one target planning folder per lane. Scout (read-only, on gpt-5.5 dispatched through a sonnet codex wrapper; the CODEX flag false restores native opus) maps the folder — admitted packages, hand-rolled capability an ecosystem package owns, domain gaps against the bleeding-edge state of the art — and emits bounded research facets. Research fan (gpt-5.5 codex wrappers with live web search enabled, parallel) hunts the best-in-class modern package per facet, self-validating the admission gate (best-of, platform, newest stable, license, modern packaging, no-dup) with verified versions and members, writing its full candidate dossier to a per-lane report file and returning a thin receipt. ONE admission writer (fable) reads every research report IN FULL from disk, consolidates adversarially, hand-edits the central manifest + owning project registry + folder README bidirectionally (adds, and ripple-removes superseded packages), runs the restore/lock gate with the toolchain fallback, self-heals, and reverts what cannot resolve. Catalog writers (fable, parallel) author the .api catalogs at FULL depth — decompile/feed-verified members, [STACKING], homed to the owning tier (folder or language root). Mapper fan (gpt-5.5 codex wrappers, read-only) then reads ALL planning-folder pages plus the landed catalogs (new first) and the language-root tier, writing information maps to report files — locations, verified members, integration shapes as fact, never prescriptions — and returning thin receipts. ONE fable executor reads the map reports from disk and implements the whole integration: new pages/sub-folders where the capability demands an owner, existing pages improved and extended in place, holistic composition never tacked-on rows, index-doc closure, every ripple in the same pass. All target lanes run CONCURRENTLY under one agent-level slot cap (CAP=14); the Admit stage alone serializes across targets, and shared-tier catalogs of one language route through one serialized writer so concurrent lanes never collide on the language-root .api files. The scout hand-roll census feeds every Research facet and the Integrate executor; Scout, Admit, and Integrate each carry one bounded re-attempt. Nothing follows the executor; cold-verify runs separately when wanted.',
  phases: [
    { title: 'Scout', detail: 'one read-only gpt-5.5 lane per target (codex wrapper): folder map, hand-roll census, domain gaps, bounded research facets' },
    { title: 'Research', detail: 'one gpt-5.5 lane per facet (codex wrapper with live web search), parallel under the pool cap: best-in-class modern candidates, gate self-validated, versions/licenses/members verified, dossier to disk + thin receipt' },
    { title: 'Admit', detail: 'one writer, serialized across targets: reads research reports from disk, adversarial consolidation, central manifest + registry + README bidirectional edits, restore/lock gate, self-heal, revert on failure', model: 'fable' },
    { title: 'Catalog', detail: 'parallel writers: full-depth .api catalogs for every admitted package, verified members, [STACKING], owning-tier homing; shared-tier catalogs of one language route through one serialized writer', model: 'fable' },
    { title: 'Map', detail: 'read-only gpt-5.5 mappers (codex wrappers) over all planning pages + both .api tiers, new catalogs first: information maps to disk + thin receipts, never prescriptions' },
    { title: 'Integrate', detail: 'one executor: reads map reports from disk, then the whole integration in place — new owners where demanded, existing pages grown, index docs closed, ripples in-pass', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 14
const STAGGER_MS = 1500
const STALL = 300000
const EXEC_STALL = 480000
const MAP_SLICE = 5 // planning pages per mapper
const CATALOG_BATCH = 2 // admitted packages per catalog writer
const CODEX = true // scout/research/map lanes run on gpt-5.5 via the codex wrapper; false restores native opus lanes
const CODEX_DIR = '.claude/scratch/survey' // wrapper task/schema/report files, one triple per lane

// --- [INPUTS] ----------------------------------------------------------------------------

const normTarget = (t) => String(t).trim().replace(/\/+$/, '').replace(/\/\.planning$/, '')
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const isObj = !!argsIn && typeof argsIn === 'object' && !Array.isArray(argsIn)
const rawTargets = Array.isArray(argsIn) ? argsIn
  : (isObj && Array.isArray(argsIn.targets)) ? argsIn.targets
  : (isObj && typeof argsIn.targets === 'string') ? [argsIn.targets]
  : (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn]
  : []
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))]

// --- [MODELS] ----------------------------------------------------------------------------

// Scout is the run's one INLINE codex payload: facets fan the Research stage and pages slice the Map
// stage, so the typed JSON must travel through structured output; failure is ok=false + `failure`.
const SCOUT_SCHEMA = { type: 'object', additionalProperties: false, required: ['ok', 'failure', 'domain', 'packages', 'pages', 'facets', 'handRolls'], properties: {
  ok: { type: 'boolean' }, failure: { type: 'string' },
  domain: { type: 'string' },
  packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'version', 'role'], properties: {
    name: { type: 'string' }, version: { type: 'string' }, role: { type: 'string' } } } },
  handRolls: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability', 'evidence'], properties: {
    capability: { type: 'string' }, evidence: { type: 'string' } } } },
  pages: { type: 'array', items: { type: 'string' } }, // real listing of the folder design pages
  facets: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['id', 'direction', 'gap', 'mandate'], properties: {
    id: { type: 'string' }, direction: { type: 'string' }, gap: { type: 'string' }, mandate: { type: 'string' } } } } } }

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR = { type: 'object', additionalProperties: false, required: ['path', 'line', 'role', 'note'], properties: {
  path: { type: 'string' }, line: { type: 'integer' },
  role: { type: 'string', enum: ['state', 'ruling', 'catalog', 'counterpart', 'absence'] },
  note: { type: 'string' } } }

const COVERAGE = { type: 'object', additionalProperties: false, required: ['requested', 'read', 'skipped', 'unverified'], properties: {
  requested: { type: 'array', items: { type: 'string' } },
  read: { type: 'array', items: { type: 'string' } },
  skipped: { type: 'array', items: { type: 'string' } },
  unverified: { type: 'array', items: { type: 'string' } } } }

const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['facet', 'candidates', 'coverage'], properties: {
  facet: { type: 'string' },
  candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'fills', 'ok', 'version', 'license', 'bestOf', 'platformOk', 'newest', 'licenseOk', 'modernPkg', 'notDup', 'dupOf', 'alternativesConsidered', 'evidence', 'members', 'files', 'anchors'], properties: {
    package: { type: 'string' }, fills: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' },
    bestOf: { type: 'boolean' }, platformOk: { type: 'boolean' }, newest: { type: 'boolean' }, licenseOk: { type: 'boolean' },
    modernPkg: { type: 'boolean' }, notDup: { type: 'boolean' }, dupOf: { type: 'string' }, ok: { type: 'boolean' },
    alternativesConsidered: { type: 'string' }, evidence: { type: 'string' },
    members: { type: 'array', items: { type: 'string' } }, // verified member spellings backing the capability claim
    files: { type: 'array', items: { type: 'string' } }, // repo files the admission writer must open: census sites, overlapping catalogs, manifest rows
    anchors: { type: 'array', items: ANCHOR } } } }, // repo-side coordinates only; web truth stays in `evidence`
  coverage: COVERAGE } }

const ADMIT_SCHEMA = { type: 'object', additionalProperties: false, required: ['admitted', 'skipped', 'files', 'green', 'summary'], properties: {
  admitted: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'version', 'catalog', 'license', 'replaces'], properties: {
    package: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' },
    catalog: { type: 'string' }, // canonical .api path at the OWNING tier the Catalog stage fills
    replaces: { type: 'string' } } } },
  skipped: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: {
    package: { type: 'string' }, why: { type: 'string' } } } },
  files: { type: 'array', items: { type: 'string' } }, green: { type: 'boolean' }, summary: { type: 'string' } } }

const CATALOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'summary', 'phantomsDropped'], properties: {
  files: { type: 'array', items: { type: 'string' } }, phantomsDropped: { type: 'array', items: { type: 'string' } },
  summary: { type: 'string' } } }

const MAP_SCHEMA = { type: 'object', additionalProperties: false, required: ['entries', 'coverage', 'summary'], properties: {
  entries: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'kind', 'files', 'info', 'anchors', 'members'], properties: {
    target: { type: 'string' }, kind: { type: 'string', enum: ['state', 'stacking', 'gap', 'ripple', 'registry'] },
    files: { type: 'array', items: { type: 'string' } }, // files the executor must open for this entry
    info: { type: 'string' }, // the fact: current shape, integration shape, gap — prose truth, zero prescriptions
    anchors: { type: 'array', items: ANCHOR }, // exact coordinates backing the fact
    members: { type: 'array', items: { type: 'string' } } } } }, // verified member spellings backing a stacking entry
  coverage: COVERAGE, summary: { type: 'string' } } }

// Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + count + headline travel inline.
const RECEIPT = { type: 'object', additionalProperties: false, required: ['ok', 'report', 'entries', 'headline', 'failure'], properties: {
  ok: { type: 'boolean' }, report: { type: 'string' }, entries: { type: 'integer' },
  headline: { type: 'string' }, failure: { type: 'string' } } }

const FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'built', 'beyond', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  built: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  beyond: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LANG = {
  cs: { key: 'cs', root: 'libs/csharp', stack: 'docs/stacks/csharp',
    manifest: '`Directory.Packages.props` (central pins, label-grouped, one-line maintenance comments) + `Directory.Build.props` (net10.0 floor, osx-arm64)',
    registry: 'the owning `.csproj` PackageReference rows and the folder README package sections',
    gate: '`uv run --frozen python -m tools.assay static --project <csproj>` (one JSON Envelope on stdout); assay unavailable: `dotnet restore` + ' +
      '`dotnet build` at the same green criterion',
    runtime: 'PLATFORM: osx-arm64 on net10.0 — managed AnyCPU, an osx-arm64 native asset, or a Forge-provisioned native substrate; reject ' +
      'win-only/x64-only/dead-on-arm. For a multi-target package decompile the lib/<tfm> a net10 consumer actually binds (assay default ' +
      'resolution can pick a non-bound TFM whose surface differs — set DOTNET_ROOT and run `ilspycmd <pkg>/lib/<consumer-tfm>/<asm>.dll ' +
      '-t <FQN>` when in doubt); never document a member from a non-bound TFM.' },
  py: { key: 'py', root: 'libs/python', stack: 'docs/stacks/python',
    manifest: '`pyproject.toml` (dependencies / dependency-groups, lean unpinned names by default)',
    registry: 'the folder README package sections',
    gate: '`uv lock` + `uv sync` + import-verification of the new modules + `ruff check` at the same green criterion',
    runtime: 'RUNTIME: the workspace floor is CPython 3.15 on osx-arm64; admissibility is decided by an ACTUAL `uv lock` + `uv sync` + import ' +
      'on cp315, never wheel-presence alone — a pure-Python/cp315-clean wheel or a native sdist that source-builds via the Forge scientific ' +
      'toolchain is admitted un-gated; a `python_version` marker is honest only for a real reported cp315 build failure. Verify members ' +
      'against the actually-installed distribution.' },
  ts: { key: 'ts', root: 'libs/typescript', stack: 'docs/stacks/typescript',
    manifest: '`pnpm-workspace.yaml` (catalog)',
    registry: 'the folder README package sections',
    gate: '`pnpm install` + `pnpm -r build` (or `tsc -p`) over the affected packages at the same green criterion',
    runtime: 'RUNTIME: verify members against the published types in node_modules `.d.ts` declarations; a member absent from the published types is a phantom.' },
}
const langOf = (t) => t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null

// --- [OPERATIONS] ------------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// Agent-level slot scheduler: CAP agents in flight across ALL target lanes, staggered launch,
// work-conserving backfill the moment a slot frees. The single governor for every agent call.
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

// Serial write chains — first lane to arrive goes first; the slot is acquired INSIDE the chained
// thunk, so a queued lane never holds a slot while waiting its turn.
const makeChain = () => { let tail = Promise.resolve(); return (fn) => { const p = tail.then(fn, fn); tail = p.then(() => undefined, () => undefined); return p } }
const admitSerial = makeChain() // ONE central-manifest writer at a time across all targets
const sharedSerial = { cs: makeChain(), py: makeChain(), ts: makeChain() } // ONE language-root .api writer at a time per language
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }

// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// CODEX_DIR, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns a thin RECEIPT — the product stays on disk for the
// consuming writer. It never does, edits, judges, or relays the work. `web` inserts live web search.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')

const codexCore = (base, rpt, schema, writes, web) => [
  '(1) Files FIRST, with the WRITE TOOL — never a shell heredoc and never a relative path (cwd drift and heredoc quoting land files where codex cannot find them, killing every launch on a missing schema file). From the repository root (your starting cwd): mkdir -p ' + CODEX_DIR + '; purge stale lane artifacts (a leftover report would READY instantly with last run\'s data): rm -f ' + base + '-report.json ' + base + '-stderr.log; Write the TASK block below verbatim to ' + base + '-task.md; Write this JSON ' +
    'Schema exactly to ' + base + '-schema.json — both paths resolved ABSOLUTE under the repository root: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call from the repo root, which FIRST verifies the files: test -s ' + base + '-task.md && test -s ' + base + '-schema.json || echo FILES-MISSING — on FILES-MISSING redo (1), NEVER launch without both. THEN the command below VERBATIM, never retyped or reflowed (every token matters: dropping </dev/null makes codex block forever on stdin, zero-CPU, no report): ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral -c mcp_servers={} ' +
    (web ? '-c web_search="live" ' : '') + '--output-schema ' + base + '-schema.json -o ' + base + '-report.json ' +
    '"Do the task in ' + base + '-task.md from the repository root. Final message: JSON per the output schema." ' +
    '</dev/null >/dev/null 2>' + base + '-stderr.log &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rptPat + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rptPat + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). LIVENESS IS NOT HEALTH: after the 4th RUNNING poll (~20 min wall) the run is WEDGED, not slow — kill it (pkill -f "' + rptPat + '") and go to (4) as GONE. Cap at 7 poll calls total.']

const codexPrompt = (label, task, schema, writes, web, head) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1) // self-excluding pgrep/pkill pattern
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and return a thin ' +
    'RECEIPT for its on-disk report. Never perform, edit, judge, soften, summarize, or RELAY the work itself.',
  ...codexCore(base, rpt, schema, writes, web),
  '(4) READY: do NOT relay the report body through your output — build the MECHANICAL headline with jq (never your own ' +
    'judgment): entries=$(jq \'' + head.arr + ' | length\' ' + base + '-report.json); kinds=$(jq -r \'' + head.kind + ' | group_by(.) | map("\\(.[0])x\\(length)") | join(",")\' ' + base + '-report.json). ' +
    'Return the RECEIPT: ok=true, report=' + base + '-report.json, entries=that count, headline="<entries> ' + head.unit + ' | <kinds>", failure empty. ' +
    'GONE with no report: tail -5 ' + base + '-stderr.log FIRST — that tail IS the crash reason; relaunch the (2) command once (detached, never ' +
    'foreground) and resume polling; a second GONE returns ok=false, entries=0, report and headline empty, failure=the stderr tail in one line.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}

// Scout is the run's one INLINE codex lane: its payload is orchestration input, so the typed report
// JSON travels through structured output verbatim; failure is ok=false + `failure`, never sentinels.
const codexInline = (label, task, schema, writes, web) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1) // self-excluding pgrep/pkill pattern
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and relay ' +
    'its typed answer VERBATIM. Never perform, edit, judge, soften, or summarize the task yourself.',
  ...codexCore(base, rpt, schema, writes, web),
  '(4) READY: return the report-file JSON through your structured output VERBATIM, unchanged. GONE with no report: ' +
    'tail -5 ' + base + '-stderr.log FIRST — that tail IS the crash reason; relaunch the (2) command once (detached, never ' +
    'foreground) and resume polling; a second GONE returns the schema shape with `ok` false, `failure` = the stderr tail ' +
    'in one line, every array empty, and every other string empty.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}

// jq headline bits per receipt product: mechanical counts by gate/kind, never lane judgment.
const HEAD = {
  research: { arr: '.candidates', kind: '[.candidates[] | if .ok then "gated" else "rejected" end]', unit: 'candidates' },
  map: { arr: '.entries', kind: '[.entries[].kind]', unit: 'entries' },
}

// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, native opus otherwise.
// The roster row carries `scope` from the ORCHESTRATOR (never the lane's self-report) so a failed lane's
// uncovered territory is exact even when the lane died before writing anything.
const recon = (task, o) => (CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes, !!o.web, o.head),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: STALL })
  : agent(task + '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
    CODEX_DIR + '/' + fileTag(o.label) + '-report.json (Write tool, absolute path under the repo root): ' +
    JSON.stringify(o.schema) + ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
    { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: RECEIPT, stallMs: STALL })
).then((r) => ({ lane: o.label, scope: o.scope || [], ok: !!(r && r.ok && r.report), report: (r && r.report) || '',
  entries: (r && r.entries) || 0, headline: (r && r.headline) || '', failure: (r && r.failure) || (r ? '' : 'lane died') }))
const scoutLane = (task, o) => CODEX
  ? agent(codexInline(o.label, task, o.schema, !!o.writes, !!o.web),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: o.schema, stallMs: STALL })
  : agent(task, { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: o.schema, stallMs: STALL })

// --- [SHARED_BLOCKS] ---------------------------------------------------------------------

const CTX = (t, L) => 'Rasm monorepo, planning phase — the work product is design pages, index docs, central manifests, and .api ' +
  'catalogs; no source files land. Target planning folder: ' + t + ' (design pages under ' + t + '/.planning/, folder catalogs ' +
  'under ' + t + '/.api/, shared substrate catalogs under ' + L.root + '/.api/). Central pins live in ' + L.manifest + ' — never a ' +
  'per-package manifest. All .md prose follows docs/standards/style-guide.md: declarative agent-facing law, no provenance, no ' +
  'process narration, no hedges. Never run git commit.'

const MEMBER_TRUTH = (L) => 'MEMBER TRUTH — verify external members via `uv run --frozen python -m tools.assay api` ' +
  '(decompile/reflection); when the assay rail is unavailable or errors, truth routes through the fallback tier: both .api tiers, ' +
  'the nuget MCP (feed truth: newest version, license, TFMs, deprecation), Context7 for the official surface, exa/tavily source ' +
  'reads. A package, version, or member no tier can verify is a PHANTOM: never survey it, never cite it, never admit it. ' + L.runtime

const ADMISSION_GATE = 'ADMISSION GATE — every candidate passes ALL of: BEST-OF (the strongest package for the gap, real ' +
  'alternatives compared, never the first found); PLATFORM (resolves on the workspace floor); NEWEST (current stable named, ' +
  'actively maintained); LICENSE (OSS under any OSI license, or a commercial grant that is free with full access — any fee, ' +
  'subscription, seat cap, usage tier, or eval-only grant is REJECTED; state the exact SPDX/grant); MODERN PACKAGING (current ' +
  'TFM/abi/wheel era; reject abandoned or outdated-framework-only artifacts); NO-DUP (does not duplicate an admitted package or a ' +
  'sibling candidate — when two overlap, keep the single best and set dupOf on the loser). The gate is a FLOOR: hunt disqualifiers ' +
  'beyond it — dead or vulnerable transitive dependencies, native supply-chain rot, eval-gated features behind an OSS shell — and ' +
  'any found disqualifier is evidence for ok=false. Default ok=false on any unproven condition.'

const ADDITION_LAW = 'ADDITION LAW — aggressive, zero hedging: admit a package whenever it provides unique, modern capability ' +
  'appropriate to the folder domain, judged for five times today\'s demands. DECLINE only for a real reason: old/unmaintained/' +
  'low-quality, a strictly stronger alternative already admitted, or out of the folder domain. Never decline for lack of a ' +
  'CURRENT consumer — planned consumers are real design pressure. Never add for its own sake, never add a thin wrapper over an ' +
  'admitted surface, and only remove a package when another admitted package provably covers every site it serves (cite the ' +
  'subsumer and the sites); a replacement must be strictly more modern/capable/maintained, never a lateral move. A justified ' +
  'admission is executed FULLY in this run — manifest + registry + README + full-depth catalog + real design integration — never ' +
  'recorded as future or deferred work.'

const CATALOG_LAW = (L) => 'CATALOG LAW — a catalog is homed to its OWNING tier: a folder-domain package lives at the folder ' +
  'tier, a cross-folder substrate package at ' + L.root + '/.api/. House format: header (package / version / license / floor), ' +
  'member sections grouped by concern with backticked symbols + signatures + a consumer/boundary note, and a [STACKING] section ' +
  'stating how the package composes with the admitted substrate rails and its sibling domain packages into single dense rails. ' +
  'Full advanced surface at operator depth — real members only, verified per MEMBER TRUTH; a member you cannot verify is dropped ' +
  'and reported, never kept.'

const INFO_LAW = 'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current shape at each ' +
  'site, verified member spellings, integration shapes as fact. The executor decides how to build; a map entry that tells it ' +
  'what to write instead of what is true is a defect. ENTRY FORM: `info` is prose truth; `anchors` carry one coordinate per ' +
  'row (role names what it proves; `note` is the shortest literal witness under 20 words, or empty when path+line suffice; an ' +
  '`absence` anchor names where the expected thing was searched and not found); `files` lists what the executor must open for ' +
  'the entry. An underutilized-capability entry is INVENTORY, never instruction: verified members, current usage anchors, the ' +
  'concept that admits it — the executor decides whether it composes. COVERAGE is part of the product: `requested` = your ' +
  'assigned scope, `read` = what you actually full-read, `skipped`/`unverified` = what you did not reach — an honest skip ' +
  'beats a silent one.'

const WRITE_LAW = 'WRITE FULLY — every fix you identify you make NOW via Edit/Write; a fix-log reports edits already made, ' +
  'never a to-do or a hedge. Every ripple your edit exposes is YOURS in the same pass — any project file, both seam ends, ' +
  'consumer sites, index docs. Landed sibling work is composed as found; a conflict resolves to the stronger form, never a revert.'

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length) {
  log('No targets — pass a planning folder path, an array of paths, or {targets}. Empty args is a no-op.')
  return { targets: [], lanes: [] }
}

const badLang = TARGETS.filter((t) => !langOf(t))

if (badLang.length) {
  log('Targets must live under libs/csharp | libs/python | libs/typescript. Got: ' + JSON.stringify(badLang))
  return { targets: TARGETS, lanes: [] }
}

// All target lanes run CONCURRENTLY; the slot scheduler is the only concurrency governor. The six
// phases are declared up front — concurrent lanes route every agent to its group via the per-call
// phase option and never race the global phase(). Only Admit serializes across lanes (admitSerial);
// shared-tier catalogs of one language serialize through sharedSerial so lanes never collide on
// the language-root .api files.
phase('Scout')
phase('Research')
phase('Admit')
phase('Catalog')
phase('Map')
phase('Integrate')

const lane = async (t) => {
  const L = LANG[langOf(t)]
  const tag = t.split('/').pop()

  // --- [SCOUT]
  const scoutPrompt = [CTX(t, L), MEMBER_TRUTH(L), ADDITION_LAW,
    'TASK: HOSTILE READ-ONLY SCOUT (investigate, do NOT edit). Map ' + t + ' against real disk, never memory: `ls`/`fd` the ' +
    'folder tree and BOTH .api tiers, then FULL-read the README, the project/registry surface, every folder-tier catalog, every ' +
    'design page under ' + t + '/.planning/, the substrate-tier catalogs the folder references, and the folder\'s central ' +
    'manifest rows. Return: `domain` (1-2 sentences); `packages` (every admitted package with version and a hostile role call); ' +
    '`handRolls` (each capability the design pages implement by hand that a real ecosystem package owns, with a verified ' +
    'page-section evidence pointer); `pages` (the real listing of every design page); `facets` — 4 to 8 non-overlapping research ' +
    'directions that together cover the folder\'s highest-value package gaps, judged against the bleeding-edge state of the art ' +
    'on both naivety axes (COVERAGE: the folder models a thin slice of its domain; APPROACH: hand-rolled enumeration where a ' +
    'package-backed generator should own the space). Each facet is all its researcher receives — pack the mandate with the ' +
    'admitted-adjacent packages the candidate must NOT duplicate and the seams it must integrate with; the facet is a pointer, ' +
    'never a ceiling. A completed scout sets `ok` true with `failure` empty; a scout that cannot complete sets `ok` false with ' +
    'the one-line reason in `failure` and every array empty.'].join('\n\n')
  const scoutOpts = { label: 'scout:' + tag, phase: 'Scout', schema: SCOUT_SCHEMA }
  // One bounded re-attempt: a dead or failed scout silently no-ops the whole lane.
  let scout = await slot(() => scoutLane(scoutPrompt, scoutOpts))
  if (!(scout && scout.ok)) scout = await slot(() => scoutLane(scoutPrompt, { ...scoutOpts, label: 'scout:' + tag + ':retry' }))
  const facets = ((scout && scout.facets) || []).filter((f) => f && f.id)
  const pages = ((scout && scout.pages) || []).filter(Boolean)
  const handRolls = ((scout && scout.handRolls) || []).filter(Boolean)
  log(tag + ' scout: ' + facets.length + ' facet(s), ' + pages.length + ' page(s), ' + handRolls.length + ' hand-roll(s)' +
    (scout && !scout.ok && scout.failure ? ' — FAILED: ' + scout.failure : ''))
  if (!facets.length) return { target: t, admitted: 0, note: 'no research facets' }

  // --- [RESEARCH]
  const research = (await Promise.all(facets.map((fc) => slot(() => recon([CTX(t, L), MEMBER_TRUTH(L), ADMISSION_GATE, ADDITION_LAW,
    'TASK: RESEARCH one facet (read-only, then self-validate). facet=' + fc.id + ' · direction=' + fc.direction + ' · gap=' +
    fc.gap + (fc.mandate ? ' · mandate=' + fc.mandate : '') + '. Find the best-in-class MODERN package(s) that close this gap ' +
    'for the folder domain. The facet is your initial pointer, never a ceiling: re-derive the landscape yourself and sweep the ' +
    'full space it names — the first-found candidate is the presumed-naive pick until the real alternatives are attacked and ' +
    'the strongest named. Reject a thin-slice package covering one corner when a full-space owner exists; prefer one ' +
    'parameterized owner over a roster of point solutions. The folder\'s scout-verified HAND-ROLL CENSUS — capability the ' +
    'design pages implement by hand that a real package owns — is: ' + JSON.stringify(handRolls) + '; a candidate that owns ' +
    'one of these census sites outranks one that merely adjoins the gap. Web research for landscape/newest/maintenance/license; registry ' +
    'truth for versions and licenses; members per MEMBER TRUTH. Exclude anything already admitted unless it is a strictly ' +
    'stronger replacement (then set `fills` to name what it replaces). Self-validate each candidate against the ADMISSION GATE ' +
    'and return the gate fields with evidence and the alternatives compared. CANDIDATE FORM: `members` = the verified member ' +
    'spellings backing the capability claim; repo-side facts carry coordinates — the census site a candidate owns, the admitted ' +
    'catalog or manifest row it overlaps — as `anchors` (role names what each proves; `note` = shortest literal witness under ' +
    '20 words, or empty; an `absence` anchor names where the expected thing was searched and not found) with `files` = the repo ' +
    'files the admission writer must open; web truth stays in `evidence`. COVERAGE is part of the product: `requested` = your ' +
    'facet scope, `read` = the registries, sources, and repo surfaces you actually consulted, `skipped`/`unverified` = what you ' +
    'did not reach — an honest skip beats a silent one. Write nothing.'].join('\n\n'),
    { label: 'research:' + tag + ':' + fc.id, phase: 'Research', schema: RESEARCH_SCHEMA, web: true, head: HEAD.research,
      scope: [fc.id + ' | ' + fc.direction + ' | ' + fc.gap] }))))).filter(Boolean)
  const researched = research.filter((r) => r.ok)
  const candTotal = researched.reduce((n, r) => n + r.entries, 0)
  const unresearched = research.filter((r) => !r.ok).flatMap((r) => r.scope.map((sc) => ({ lane: r.lane, scope: sc })))
  log(tag + ' research: ' + researched.length + '/' + facets.length + ' facet report(s), ' + candTotal + ' candidate(s)' +
    (unresearched.length ? ' — FAILED: ' + research.filter((r) => !r.ok).map((r) => r.lane).join(', ') : ''))
  if (!researched.length) return { target: t, admitted: 0, note: 'no research reports landed' }

  // --- [ADMIT]
  // Serialized across lanes: one central-manifest writer at a time. Research products stay on disk;
  // the writer consumes the roster's report files, never an inlined payload.
  const admitPrompt = [CTX(t, L), MEMBER_TRUTH(L), ADMISSION_GATE, ADDITION_LAW, WRITE_LAW,
    'TASK: ADMISSION WRITER — you are the run\'s only central-manifest writer while you hold the serial window. The research ' +
    'products are ON DISK, one report file per facet (ROSTER below; consume only lanes with ok=true). CONSUMPTION: (a) ' +
    'UNRESEARCHED facets below got no research coverage — they admit nothing this run; never back-fill them from memory; ' +
    '(b) read every ok report IN FULL from disk before deciding anything — candidates overlap across facets, cluster by ' +
    'package as you read; (c) consolidate ADVERSARIALLY: an on-disk ok=true candidate is a claim to re-derive, never a fact — ' +
    're-check each against the central manifest truth (a dup with an admitted row, a phantom version, or domain drift kills ' +
    'the claim), re-open the repo `anchors`/`files` a candidate cites, resolve every remaining overlap across facets to the ' +
    'single best, and drop anything outside the folder domain. THEN execute each surviving ' +
    'admission NOW: (a) the central pin in ' + L.manifest + ' at the newest stable version, hand-edited in the matching ' +
    'label group; (b) ' + L.registry + ', bidirectionally — a replacement named in `fills` ripple-removes the superseded ' +
    'package\'s pin, registry row, README mention, and catalog; (c) the restore/lock gate: ' + L.gate + ' — self-heal a red ' +
    'gate in place (wrong group, missing transitive floor pin, nonexistent version, stale reference); a package that ' +
    'genuinely cannot resolve is REVERTED entirely and reported under `skipped` with the reason. For each admitted package ' +
    'report `catalog` = the canonical .api path at its OWNING tier (folder-domain package -> ' + t + '/.api/; cross-folder ' +
    'substrate -> ' + L.root + '/.api/), matching the sibling naming convention. `green` is true only when the final gate is ' +
    'clean after your repairs. Admit only candidates present in the on-disk reports; never fabricate a package, version, or ' +
    'member beyond them. UNRESEARCHED: ' + JSON.stringify(unresearched) + ' ROSTER: ' + JSON.stringify(research)].join('\n\n')
  const admitOpts = { label: 'admit:' + tag, phase: 'Admit', model: 'fable', effort: 'high', schema: ADMIT_SCHEMA, stallMs: EXEC_STALL }
  // One bounded re-attempt inside the serial window: a dead admit drops the lane's whole admission.
  const admit = await admitSerial(async () => (await slot(() => agent(admitPrompt, admitOpts)))
    || (await slot(() => agent(admitPrompt, { ...admitOpts, label: 'admit:' + tag + ':retry' }))))
  const admitted = ((admit && admit.admitted) || []).filter((a) => a && a.package)
  log(tag + ' admit: ' + admitted.length + ' admitted, ' + (((admit && admit.skipped) || []).length) + ' skipped, green=' + !!(admit && admit.green))
  if (!admitted.length) return { target: t, admitted: 0, green: !!(admit && admit.green), note: (admit && admit.summary) || 'nothing admitted' }

  // --- [CATALOG]
  // Folder-tier batches fan freely; shared-tier catalogs route through this language's ONE serialized writer.
  const catalogPrompt = (batch) => [CTX(t, L), MEMBER_TRUTH(L), CATALOG_LAW(L), WRITE_LAW,
    'TASK: AUTHOR the full-depth .api catalog for each of these admitted packages, at the exact `catalog` path each carries: ' +
    JSON.stringify(batch) + '. Read the sibling catalogs at the owning tier first for the house convention, then write each ' +
    'catalog complete: the full advanced surface at operator depth, every member verified per MEMBER TRUTH (a member no tier ' +
    'verifies is dropped and listed in `phantomsDropped`), the [STACKING] section wiring the package into the substrate rails ' +
    'and its sibling domain packages. Sibling writers land catalogs concurrently — compose theirs as found on disk.'].join('\n\n')
  const catalogOpts = (lbl) => ({ label: lbl, phase: 'Catalog', model: 'fable', effort: 'high', schema: CATALOG_SCHEMA, stallMs: STALL })
  const sharedTier = admitted.filter((a) => String(a.catalog || '').indexOf(L.root + '/.api/') === 0)
  const folderTier = admitted.filter((a) => String(a.catalog || '').indexOf(L.root + '/.api/') !== 0)
  const catalogTasks = chunk(folderTier, CATALOG_BATCH).map((batch, i) => slot(() => agent(catalogPrompt(batch), catalogOpts('catalog:' + tag + ':b' + i))))
  if (sharedTier.length) catalogTasks.push(sharedSerial[L.key](() => slot(() => agent(catalogPrompt(sharedTier), catalogOpts('catalog:' + tag + ':shared')))))
  const catalogs = (await Promise.all(catalogTasks)).filter(Boolean)
  const catalogFiles = catalogs.flatMap((c) => c.files || [])
  log(tag + ' catalog: ' + catalogFiles.length + ' catalog file(s) authored')

  // --- [MAP]
  const slices = pages.length ? chunk(pages, MAP_SLICE) : [[]]
  const maps = (await Promise.all(slices.map((s, i) => slot(() => recon([CTX(t, L), MEMBER_TRUTH(L), INFO_LAW,
    'TASK: INTEGRATION MAP, slice ' + i + ' (read-only). The run just admitted: ' + JSON.stringify(admitted) + '. Read the NEW ' +
    'catalogs FIRST (' + JSON.stringify(catalogFiles) + '), then each of these design pages IN FULL from CURRENT disk: ' +
    JSON.stringify(s) + ', then every other catalog (folder tier and ' + L.root + '/.api/) the pages cite. Return entries: ' +
    'where each new capability lands (`stacking` — the page whose concept admits it, verified member spellings, the ' +
    'integration shape as fact), what each page currently hand-rolls that an admitted package now owns (`state`/`gap` with ' +
    'exact anchors), where a new page or sub-folder is warranted because no existing page owns the concept (`gap`), and every ' +
    'registry/index surface the integration will touch (`registry`/`ripple`).'].join('\n\n'),
    { label: 'map:' + tag + ':s' + i, phase: 'Map', schema: MAP_SCHEMA, head: HEAD.map, scope: s }))))).filter(Boolean)
  const mapped = maps.filter((r) => r.ok)
  const mapTotal = mapped.reduce((n, r) => n + r.entries, 0)
  const unmapped = maps.filter((r) => !r.ok).flatMap((r) => r.scope.map((sc) => ({ lane: r.lane, scope: sc })))
  log(tag + ' map: ' + mapTotal + ' entr(ies) across ' + mapped.length + '/' + maps.length + ' mapper(s)' +
    (unmapped.length ? ' — FAILED: ' + maps.filter((r) => !r.ok).map((r) => r.lane).join(', ') : ''))

  // --- [INTEGRATE]
  const integratePrompt = [CTX(t, L), MEMBER_TRUTH(L), WRITE_LAW,
    'TASK: INTEGRATION EXECUTOR (WRITER — you are the run\'s LAST agent for this target; nothing follows you; full write ' +
    'authority over the folder, its index docs, and any file a ripple exposes). Read the ' + L.stack + '/ doctrine at source ' +
    '(README and every page it routes) — it is the bar. The map REPORT FILES are your reconnaissance — information, not ' +
    'instructions. CONSUMPTION: (a) UNMAPPED pages below get your own cold read FIRST — a failed mapper\'s territory is ' +
    'yours directly; (b) read every ok map report IN FULL from disk; entries overlap across slices — dedupe by target as ' +
    'you read; (c) each entry\'s anchors are jump coordinates — re-open every anchor behind an edit (mandatory); ' +
    'navigation-only entries re-verify only when touched; spot-verify what you build on and hunt past the maps on your own ' +
    'authority. IMPLEMENT the whole integration NOW: replace ' +
    'hand-rolled capability with the admitted packages at every mapped site, grow existing owners in place (a case, row, ' +
    'field, or operation — reshaped as if always carried, never a tacked-on mention), author a new page or sub-folder ' +
    'ground-up where the mapped capability demands an owner no page carries, weave beyond-map underutilized capability the ' +
    'catalogs expose, and close the folder README/ARCHITECTURE index docs so the landed state is truthfully reflected. ' +
    'Every ripple in the same pass, both seam ends. ADMITTED: ' + JSON.stringify(admitted) + '. HAND-ROLLS (the scout census ' +
    'of page-local reimplementation the admission now owns — every census site replaced at its evidence anchor, none left ' +
    'half-migrated): ' + JSON.stringify(handRolls) + '. UNMAPPED: ' + JSON.stringify(unmapped) + ' MAP ROSTER: ' + JSON.stringify(maps)].join('\n\n')
  const integrateOpts = { label: 'integrate:' + tag, phase: 'Integrate', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: EXEC_STALL }
  // One bounded re-attempt: a dead executor leaves the lane admitted but unintegrated.
  const fix = (await slot(() => agent(integratePrompt, integrateOpts)))
    || (await slot(() => agent(integratePrompt, { ...integrateOpts, label: 'integrate:' + tag + ':retry' })))
  return { target: t, admitted: admitted.length, skipped: (admit && admit.skipped) || [], green: !!(admit && admit.green),
    catalogs: catalogFiles, mapEntries: mapTotal,
    failedLanes: research.concat(maps).filter((r) => !r.ok).map((r) => r.lane),
    built: (fix && fix.built && fix.built.length) || 0,
    beyond: (fix && fix.beyond && fix.beyond.length) || 0, summary: (fix && fix.summary) || (fix ? '' : 'integrate agent died twice') }
}

const lanes = (await Promise.all(TARGETS.map((t) => lane(t).then((r) => r, () => ({ target: t, admitted: 0, note: 'lane crashed — inspect the run journal' }))))).filter(Boolean)

return { targets: TARGETS, lanes }
