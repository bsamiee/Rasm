export const meta = {
  name: 'survey',
  whenToUse: 'Deep-research the modern external packages a target planning folder is missing — packages that REPLACE hand-rolled design-page capability or ADD genuine domain capability — then execute end to end in one run: central admission with gates, full-depth .api catalogs, registry closure, and immediate holistic integration into the design pages. args = a planning folder path, an array of paths, or {targets}; multiple targets run sequentially (the central manifest has one writer at a time).',
  description: 'Package survey-and-integrate over one target planning folder per lane. Scout (opus, read-only) maps the folder — admitted packages, hand-rolled capability an ecosystem package owns, domain gaps against the bleeding-edge state of the art — and emits bounded research facets. Research fan (opus, parallel) hunts the best-in-class modern package per facet, self-validating the admission gate (best-of, platform, newest stable, license, modern packaging, no-dup) with verified versions and members. ONE admission writer (fable) consolidates adversarially, hand-edits the central manifest + owning project registry + folder README bidirectionally (adds, and ripple-removes superseded packages), runs the restore/lock gate with the toolchain fallback, self-heals, and reverts what cannot resolve. Catalog writers (fable, parallel) author the .api catalogs at FULL depth — decompile/feed-verified members, [STACKING], homed to the owning tier (folder or language root). Mapper fan (opus, read-only) then reads ALL planning-folder pages plus the landed catalogs (new first) and the language-root tier, returning information maps — locations, verified members, integration shapes as fact, never prescriptions. ONE fable executor implements the whole integration: new pages/sub-folders where the capability demands an owner, existing pages improved and extended in place, holistic composition never tacked-on rows, index-doc closure, every ripple in the same pass. Nothing follows the executor; cold-verify runs separately when wanted.',
  phases: [
    { title: 'Scout', detail: 'one read-only agent per target: folder map, hand-roll census, domain gaps, bounded research facets', model: 'opus' },
    { title: 'Research', detail: 'one agent per facet, parallel under the pool cap: best-in-class modern candidates, gate self-validated, versions/licenses/members verified', model: 'opus' },
    { title: 'Admit', detail: 'one writer: adversarial consolidation, central manifest + registry + README bidirectional edits, restore/lock gate, self-heal, revert on failure', model: 'fable' },
    { title: 'Catalog', detail: 'parallel writers: full-depth .api catalogs for every admitted package, verified members, [STACKING], owning-tier homing', model: 'fable' },
    { title: 'Map', detail: 'read-only mappers over all planning pages + both .api tiers, new catalogs first: information maps, never prescriptions', model: 'opus' },
    { title: 'Integrate', detail: 'one executor: the whole integration in place — new owners where demanded, existing pages grown, index docs closed, ripples in-pass', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const STALL = 300000
const EXEC_STALL = 480000
const MAP_SLICE = 5 // planning pages per mapper
const CATALOG_BATCH = 2 // admitted packages per catalog writer

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

const SCOUT_SCHEMA = { type: 'object', additionalProperties: false, required: ['domain', 'packages', 'pages', 'facets'], properties: {
  domain: { type: 'string' },
  packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name'], properties: {
    name: { type: 'string' }, version: { type: 'string' }, role: { type: 'string' } } } },
  handRolls: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability', 'evidence'], properties: {
    capability: { type: 'string' }, evidence: { type: 'string' } } } },
  pages: { type: 'array', items: { type: 'string' } }, // real listing of the folder design pages
  facets: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['id', 'direction', 'gap'], properties: {
    id: { type: 'string' }, direction: { type: 'string' }, gap: { type: 'string' }, mandate: { type: 'string' } } } } } }
const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['facet', 'candidates'], properties: {
  facet: { type: 'string' },
  candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'fills', 'ok'], properties: {
    package: { type: 'string' }, fills: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' },
    bestOf: { type: 'boolean' }, platformOk: { type: 'boolean' }, newest: { type: 'boolean' }, licenseOk: { type: 'boolean' },
    modernPkg: { type: 'boolean' }, notDup: { type: 'boolean' }, dupOf: { type: 'string' }, ok: { type: 'boolean' },
    alternativesConsidered: { type: 'string' }, evidence: { type: 'string' } } } } } }
const ADMIT_SCHEMA = { type: 'object', additionalProperties: false, required: ['admitted', 'skipped', 'files', 'green', 'summary'], properties: {
  admitted: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'version', 'catalog'], properties: {
    package: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' },
    catalog: { type: 'string' }, // canonical .api path at the OWNING tier the Catalog stage fills
    replaces: { type: 'string' } } } },
  skipped: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: {
    package: { type: 'string' }, why: { type: 'string' } } } },
  files: { type: 'array', items: { type: 'string' } }, green: { type: 'boolean' }, summary: { type: 'string' } } }
const CATALOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, phantomsDropped: { type: 'array', items: { type: 'string' } },
  summary: { type: 'string' } } }
const MAP_SCHEMA = { type: 'object', additionalProperties: false, required: ['entries', 'summary'], properties: {
  entries: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'kind', 'info'], properties: {
    target: { type: 'string' }, kind: { type: 'string', enum: ['state', 'stacking', 'gap', 'ripple', 'registry'] },
    info: { type: 'string' }, members: { type: 'array', items: { type: 'string' } } } } },
  summary: { type: 'string' } } }
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
    gate: '`uv run --frozen python -m tools.assay static --project <csproj>` (one JSON Envelope on stdout); assay unavailable: `dotnet restore` + `dotnet build` at the same green criterion',
    runtime: 'PLATFORM: osx-arm64 on net10.0 — managed AnyCPU, an osx-arm64 native asset, or a Forge-provisioned native substrate; reject win-only/x64-only/dead-on-arm. For a multi-target package decompile the lib/<tfm> a net10 consumer actually binds (assay default resolution can pick a non-bound TFM whose surface differs — set DOTNET_ROOT and run `ilspycmd <pkg>/lib/<consumer-tfm>/<asm>.dll -t <FQN>` when in doubt); never document a member from a non-bound TFM.' },
  py: { key: 'py', root: 'libs/python', stack: 'docs/stacks/python',
    manifest: '`pyproject.toml` (dependencies / dependency-groups, lean unpinned names by default)',
    registry: 'the folder README package sections',
    gate: '`uv lock` + `uv sync` + import-verification of the new modules + `ruff check` at the same green criterion',
    runtime: 'RUNTIME: the workspace floor is CPython 3.15 on osx-arm64; admissibility is decided by an ACTUAL `uv lock` + `uv sync` + import on cp315, never wheel-presence alone — a pure-Python/cp315-clean wheel or a native sdist that source-builds via the Forge scientific toolchain is admitted un-gated; a `python_version` marker is honest only for a real reported cp315 build failure. Verify members against the actually-installed distribution.' },
  ts: { key: 'ts', root: 'libs/typescript', stack: 'docs/stacks/typescript',
    manifest: '`pnpm-workspace.yaml` (catalog)',
    registry: 'the folder README package sections',
    gate: '`pnpm install` + `pnpm -r build` (or `tsc -p`) over the affected packages at the same green criterion',
    runtime: 'RUNTIME: verify members against the published types in node_modules `.d.ts` declarations; a member absent from the published types is a phantom.' },
}
const langOf = (t) => t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null

// --- [OPERATIONS] ------------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  let gate = Promise.resolve()
  const launch = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  const run = async () => { while (next < items.length) { const i = next++; await launch(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }

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
  'what to write instead of what is true is a defect.'
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

// Targets run sequentially: the central manifest has exactly one writer at a time.
const lanes = []
for (const t of TARGETS) {
  const L = LANG[langOf(t)]
  const tag = t.split('/').pop()

  phase('Scout')
  const scout = await agent([CTX(t, L), MEMBER_TRUTH(L), ADDITION_LAW,
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
    'never a ceiling.'].join('\n\n'),
    { label: 'scout:' + tag, phase: 'Scout', model: 'opus', effort: 'high', schema: SCOUT_SCHEMA, stallMs: STALL })
  const facets = ((scout && scout.facets) || []).filter((f) => f && f.id)
  const pages = ((scout && scout.pages) || []).filter(Boolean)
  log(tag + ' scout: ' + facets.length + ' facet(s), ' + pages.length + ' page(s), ' + (((scout && scout.handRolls) || []).length) + ' hand-roll(s)')
  if (!facets.length) { lanes.push({ target: t, admitted: 0, note: 'no research facets' }); continue }

  phase('Research')
  const research = (await pool(facets, CAP, (fc) => agent([CTX(t, L), MEMBER_TRUTH(L), ADMISSION_GATE, ADDITION_LAW,
    'TASK: RESEARCH one facet (read-only, then self-validate). facet=' + fc.id + ' · direction=' + fc.direction + ' · gap=' +
    fc.gap + (fc.mandate ? ' · mandate=' + fc.mandate : '') + '. Find the best-in-class MODERN package(s) that close this gap ' +
    'for the folder domain. The facet is your initial pointer, never a ceiling: re-derive the landscape yourself and sweep the ' +
    'full space it names — the first-found candidate is the presumed-naive pick until the real alternatives are attacked and ' +
    'the strongest named. Reject a thin-slice package covering one corner when a full-space owner exists; prefer one ' +
    'parameterized owner over a roster of point solutions. Web research for landscape/newest/maintenance/license; registry ' +
    'truth for versions and licenses; members per MEMBER TRUTH. Exclude anything already admitted unless it is a strictly ' +
    'stronger replacement (then set `fills` to name what it replaces). Self-validate each candidate against the ADMISSION GATE ' +
    'and return the gate fields with evidence and the alternatives compared. Write nothing.'].join('\n\n'),
    { label: 'research:' + tag + ':' + fc.id, phase: 'Research', model: 'opus', effort: 'high', schema: RESEARCH_SCHEMA, stallMs: STALL }))).filter(Boolean)
  const okCount = research.reduce((n, r) => n + ((r.candidates || []).filter((c) => c && c.ok).length), 0)
  log(tag + ' research: ' + research.length + '/' + facets.length + ' facet(s) returned, ' + okCount + ' gated candidate(s)')

  phase('Admit')
  const admit = await agent([CTX(t, L), MEMBER_TRUTH(L), ADMISSION_GATE, ADDITION_LAW, WRITE_LAW,
    'TASK: ADMISSION WRITER — you are the run\'s only central-manifest writer. The research payload is inlined below; ' +
    'consolidate ADVERSARIALLY: an inlined ok=true is a claim to re-derive, never a fact — re-check each against the central ' +
    'manifest truth (a dup with an admitted row, a phantom version, or domain drift kills the claim), resolve every remaining ' +
    'overlap across facets to the single best, and drop anything outside the folder domain. THEN execute each surviving ' +
    'admission NOW: (a) the central pin in ' + L.manifest + ' at the newest stable version, hand-edited in the matching ' +
    'label group; (b) ' + L.registry + ', bidirectionally — a replacement named in `fills` ripple-removes the superseded ' +
    'package\'s pin, registry row, README mention, and catalog; (c) the restore/lock gate: ' + L.gate + ' — self-heal a red ' +
    'gate in place (wrong group, missing transitive floor pin, nonexistent version, stale reference); a package that ' +
    'genuinely cannot resolve is REVERTED entirely and reported under `skipped` with the reason. For each admitted package ' +
    'report `catalog` = the canonical .api path at its OWNING tier (folder-domain package -> ' + t + '/.api/; cross-folder ' +
    'substrate -> ' + L.root + '/.api/), matching the sibling naming convention. `green` is true only when the final gate is ' +
    'clean after your repairs. RESEARCH PAYLOAD (consolidate only candidates present here; never fabricate a package, ' +
    'version, or member beyond it):\n' + JSON.stringify(research, null, 1)].join('\n\n'),
    { label: 'admit:' + tag, phase: 'Admit', model: 'fable', effort: 'high', schema: ADMIT_SCHEMA, stallMs: EXEC_STALL })
  const admitted = ((admit && admit.admitted) || []).filter((a) => a && a.package)
  log(tag + ' admit: ' + admitted.length + ' admitted, ' + (((admit && admit.skipped) || []).length) + ' skipped, green=' + !!(admit && admit.green))
  if (!admitted.length) { lanes.push({ target: t, admitted: 0, green: !!(admit && admit.green), note: (admit && admit.summary) || 'nothing admitted' }); continue }

  phase('Catalog')
  const catalogs = (await pool(chunk(admitted, CATALOG_BATCH), CAP, (batch, i) => agent([CTX(t, L), MEMBER_TRUTH(L), CATALOG_LAW(L), WRITE_LAW,
    'TASK: AUTHOR the full-depth .api catalog for each of these admitted packages, at the exact `catalog` path each carries: ' +
    JSON.stringify(batch) + '. Read the sibling catalogs at the owning tier first for the house convention, then write each ' +
    'catalog complete: the full advanced surface at operator depth, every member verified per MEMBER TRUTH (a member no tier ' +
    'verifies is dropped and listed in `phantomsDropped`), the [STACKING] section wiring the package into the substrate rails ' +
    'and its sibling domain packages. Sibling writers land catalogs concurrently — compose theirs as found on disk.'].join('\n\n'),
    { label: 'catalog:' + tag + ':b' + i, phase: 'Catalog', model: 'fable', effort: 'high', schema: CATALOG_SCHEMA, stallMs: STALL }))).filter(Boolean)
  const catalogFiles = catalogs.flatMap((c) => c.files || [])
  log(tag + ' catalog: ' + catalogFiles.length + ' catalog file(s) authored')

  phase('Map')
  const slices = pages.length ? chunk(pages, MAP_SLICE) : [[]]
  const maps = (await pool(slices, CAP, (s, i) => agent([CTX(t, L), MEMBER_TRUTH(L), INFO_LAW,
    'TASK: INTEGRATION MAP, slice ' + i + ' (read-only). The run just admitted: ' + JSON.stringify(admitted) + '. Read the NEW ' +
    'catalogs FIRST (' + JSON.stringify(catalogFiles) + '), then each of these design pages IN FULL from CURRENT disk: ' +
    JSON.stringify(s) + ', then every other catalog (folder tier and ' + L.root + '/.api/) the pages cite. Return entries: ' +
    'where each new capability lands (`stacking` — the page whose concept admits it, verified member spellings, the ' +
    'integration shape as fact), what each page currently hand-rolls that an admitted package now owns (`state`/`gap` with ' +
    'exact anchors), where a new page or sub-folder is warranted because no existing page owns the concept (`gap`), and every ' +
    'registry/index surface the integration will touch (`registry`/`ripple`).'].join('\n\n'),
    { label: 'map:' + tag + ':s' + i, phase: 'Map', model: 'opus', effort: 'high', schema: MAP_SCHEMA, stallMs: STALL }))).filter(Boolean)
  const entries = maps.flatMap((m) => m.entries || [])
  log(tag + ' map: ' + entries.length + ' entr(ies) from ' + maps.length + ' mapper(s)')

  phase('Integrate')
  const fix = await agent([CTX(t, L), MEMBER_TRUTH(L), WRITE_LAW,
    'TASK: INTEGRATION EXECUTOR (WRITER — you are the run\'s LAST agent for this target; nothing follows you; full write ' +
    'authority over the folder, its index docs, and any file a ripple exposes). Read the ' + L.stack + '/ doctrine at source ' +
    '(README and every page it routes) — it is the bar. The maps below are reconnaissance — information, not instructions; ' +
    'spot-verify what you build on and hunt past them on your own authority. IMPLEMENT the whole integration NOW: replace ' +
    'hand-rolled capability with the admitted packages at every mapped site, grow existing owners in place (a case, row, ' +
    'field, or operation — reshaped as if always carried, never a tacked-on mention), author a new page or sub-folder ' +
    'ground-up where the mapped capability demands an owner no page carries, weave beyond-map underutilized capability the ' +
    'catalogs expose, and close the folder README/ARCHITECTURE index docs so the landed state is truthfully reflected. ' +
    'Every ripple in the same pass, both seam ends. ADMITTED: ' + JSON.stringify(admitted) + '. MAPS: ' + JSON.stringify(entries)].join('\n\n'),
    { label: 'integrate:' + tag, phase: 'Integrate', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: EXEC_STALL })
  lanes.push({ target: t, admitted: admitted.length, skipped: (admit && admit.skipped) || [], green: !!(admit && admit.green),
    catalogs: catalogFiles, mapEntries: entries.length, built: (fix && fix.built && fix.built.length) || 0,
    beyond: (fix && fix.beyond && fix.beyond.length) || 0, summary: (fix && fix.summary) || '' })
}
return { targets: TARGETS, lanes }
