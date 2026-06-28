export const meta = {
  name: 'element-api-authoring',
  whenToUse: 'After the manual Directory.Packages.props pin + restore, before the element build workflow.',
  description: 'Author the FULL .api catalog for every newly-admitted Rasm.Element-campaign package in the correct tier (shared libs/csharp/.api vs folder .api), update the folder README roster + csproj, then reconcile non-phantom + tier placement. Per ELEMENT-REBUILD-PLAN.md section 5.',
  phases: [
    { title: 'Author' },
    { title: 'Reconcile' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 8
const STAGGER_MS = 1500
const PLAN = 'ELEMENT-REBUILD-PLAN.md'
const API_EXEMPLAR = 'libs/csharp/.api/api-unitsnet.md'

// --- [INPUTS] ----------------------------------------------------------------------------

const PACKAGES = [
  { name: 'Marten', kind: 'nuget', version: '9.11.0', tier: 'folder', api: 'libs/csharp/Rasm.Persistence/.api/api-marten.md', readme: 'libs/csharp/Rasm.Persistence/README.md', csproj: 'libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj' },
  { name: 'Riok.Mapperly', kind: 'nuget', version: '4.3.1', tier: 'shared', api: 'libs/csharp/.api/api-mapperly.md', readme: 'libs/csharp/.planning/README.md', csproj: '' },
  { name: 'Generator.Equals', kind: 'nuget', version: '4.0.0', tier: 'shared', api: 'libs/csharp/.api/api-generator-equals.md', readme: 'libs/csharp/.planning/README.md', csproj: '' },
  { name: 'MiniExcel', kind: 'nuget', version: 'latest-pinned', tier: 'folder', api: 'libs/csharp/Rasm.Persistence/.api/api-miniexcel.md', readme: 'libs/csharp/Rasm.Persistence/README.md', csproj: 'libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj' },
  { name: 'bSDD REST client', kind: 'doc', version: '', tier: 'folder', api: 'libs/csharp/Rasm.Bim/.api/api-bsdd.md', readme: 'libs/csharp/Rasm.Bim/README.md', csproj: '' },
  { name: 'EC3 REST client', kind: 'doc', version: '', tier: 'folder', api: 'libs/csharp/Rasm.Compute/.api/api-ec3.md', readme: 'libs/csharp/Rasm.Compute/README.md', csproj: '' },
  { name: 'DuckDB INSTALL/LOAD pattern', kind: 'doc', version: '', tier: 'folder', api: 'libs/csharp/Rasm.Persistence/.api/api-duckdb.md', readme: '', csproj: '' },
]
const wanted = Array.isArray(args) ? args.map(String)
  : (typeof args === 'string' && args.trim()) ? [args.trim()]
  : null
const matches = (w, p) => p.name.toLowerCase().indexOf(w.toLowerCase()) === 0 || w.toLowerCase() === p.name.toLowerCase()
const unknown = wanted ? wanted.filter((w) => !PACKAGES.some((p) => matches(w, p))) : []
if (unknown.length) throw new Error('Unknown package selector(s): ' + unknown.join(', ') + '. Valid: ' + PACKAGES.map((p) => p.name).join(', '))
const TARGETS = wanted ? PACKAGES.filter((p) => wanted.some((w) => matches(w, p))) : PACKAGES

// --- [MODELS] ----------------------------------------------------------------------------

const AUTHOR_SCHEMA = { type: 'object', additionalProperties: false, required: ['package', 'api', 'tier', 'verdict', 'summary'],
  properties: {
    package: { type: 'string' }, api: { type: 'string' }, tier: { type: 'string', enum: ['shared', 'folder'] },
    verdict: { type: 'string', enum: ['authored', 'updated', 'skipped'] },
    members: { type: 'number' }, readmeUpdated: { type: 'boolean' }, csprojUpdated: { type: 'boolean' },
    residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } },
    summary: { type: 'string' } } }
const RECON_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'rows'],
  properties: { overall: { type: 'boolean' },
    rows: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['api', 'status'], properties: { api: { type: 'string' }, status: { type: 'string', enum: ['ok', 'phantom', 'wrong-tier', 'missing-roster', 'fixed'] }, note: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const BRIEF = [
  'CAMPAIGN: the Rasm.Element rebuild (ELEMENT-REBUILD-PLAN.md at repo root). READ that plan in FULL first — especially section 5 (PACKAGE ADMISSION ' +
    'REGISTER, which fixes each package -> tier -> .api path -> folder) and section 4E (how the package is composed). Central pins are ALREADY in ' +
    'Directory.Packages.props and restored (do NOT touch the manifest).',
  'TIER LAW: cross-cutting SUBSTRATE packages (used across folders) get their .api in the SHARED tier `libs/csharp/.api/`; folder-specific packages get ' +
    'their .api in that folder`s own `.api/`. The row you are given states the exact tier + path — honor it.',
  'AUTHORING LAW: author a FULL, transcription-complete .api catalog matching the EXISTING .api catalog convention (read ' + API_EXEMPLAR + ' and a ' +
    'sibling .api in the target tier as the format exemplar). ZERO placeholders/stubs/TODO; real signatures only. NO version-gating prose, NO provenance, ' +
    'NO freshness tails. For a `nuget` package: resolve the LIVE surface via Context7 (resolve-library-id -> query-docs) + the nuget MCP get_package_context, ' +
    'and VERIFY every member exists via `uv run python -m tools.assay api` against the RESTORED assembly — a member you cannot verify is a PHANTOM, delete it. ' +
    'For a `doc` package (a hand-thin REST client or a usage pattern like DuckDB INSTALL/LOAD): document the integration PATTERN + the exact endpoints/SQL/types, ' +
    'no phantom members.',
  'ROSTER + CSPROJ: if a README path is given, add/refresh the package`s roster row in it (folder README [02]-[DOMAIN_PACKAGES] grouping, or the shared ' +
    'branch registry `libs/csharp/.planning/README.md` for substrate) — match the existing roster format by example, no version pins in prose. If a csproj ' +
    'path is given, add the `<PackageReference Include="..." />` (no Version attribute — central management). A blank csproj/readme means that step is owned ' +
    'elsewhere (substrate csproj refs are wired by the build workflow; DuckDB is already in the Persistence roster) — skip it, do not invent.',
].join('\n')

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
const authorPrompt = (p) => [BRIEF, '',
  'TASK: author the .api catalog for package `' + p.name + '` (kind=' + p.kind + (p.version ? ', central pin ' + p.version : '') + ').',
  'TIER: ' + p.tier + '. WRITE the catalog to: ' + p.api + '.',
  (p.readme ? 'ROSTER: add/refresh the roster row in ' + p.readme + '.' : 'ROSTER: none (owned elsewhere) — skip.'),
  (p.csproj ? 'CSPROJ: add the PackageReference (no Version attr) to ' + p.csproj + '.' : 'CSPROJ: none (substrate refs wired by the build workflow, or already pinned) — skip.'),
  'Read ' + PLAN + ' section 5 + ' + API_EXEMPLAR + ' + a sibling .api in the ' + p.tier + ' tier FIRST. Mine the package to FULL useful depth (the deepest ' +
    'operator/combinator/generated surface it reaches), composed for downstream Rasm.Element consumers per the plan`s integration map. Write the files NOW; ' +
    'the returned object REPORTS edits already made. Report residual {files,claim} ONLY for a genuine cross-package item you cannot resolve from this one package.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Author')
const authored = (await pool(TARGETS, CAP, (p) => agent(authorPrompt(p), { label: 'api:' + p.name, phase: 'Author', schema: AUTHOR_SCHEMA, effort: 'high', stallMs: 300000 }))).filter(Boolean)

// --- [RECONCILE]

phase('Reconcile')
const recon = await agent([BRIEF, '',
  'TASK: ADVERSARIAL RECONCILE of the authored .api set. For EACH catalog below, read it from disk and verify: (1) NON-PHANTOM — every cited member ' +
    'verifiably exists (spot-check via `uv run python -m tools.assay api` for nuget packages); (2) CORRECT TIER — substrate in `libs/csharp/.api/`, ' +
    'folder-specific in the folder `.api/`; (3) ROSTER present where required; (4) NO placeholder/version-gating/provenance prose. FIX any hit in place ' +
    '(delete phantoms, move a wrong-tier catalog, add a missing roster row). Return one row per catalog with its status.',
  'TARGETS:\n' + JSON.stringify(TARGETS.map((p) => {
    const result = authored.find((a) => a.package === p.name || a.api === p.api)
    return { package: p.name, api: p.api, tier: p.tier, verdict: result?.verdict ?? 'missing' }
  }), null, 1)].join('\n'),
  { label: 'reconcile', phase: 'Reconcile', schema: RECON_SCHEMA, effort: 'xhigh', stallMs: 300000 })

return { authored: authored.map((a) => ({ package: a.package, api: a.api, tier: a.tier, verdict: a.verdict, members: a.members })), reconcile: recon, count: authored.length }
