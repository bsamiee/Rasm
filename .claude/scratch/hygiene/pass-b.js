export const meta = {
  name: 'pass-b-sanity',
  description: 'Repo-wide index-doc sanity across all three language branches: truthful README/ARCHITECTURE rebuilds from disk-derived facts, canonical seam vocabulary, 160-column fence/card gates run to clean',
  phases: [
    { title: 'Census', detail: 'per branch: sonnet fact-derivation from disk + gate finding lists', model: 'sonnet' },
    { title: 'Refine', detail: 'one writer per folder: README/ARCHITECTURE rebuilt truthful, fences gated clean', model: 'fable' },
    { title: 'Mirrors', detail: 'one terminal fable: cross-branch seam mirror consistency + final gate proof', model: 'fable' },
  ],
}

// --- [CONSTANTS] ---
const GATE = 'python3 .claude/scratch/hygiene/fence-linelen.py'
const STALL = 300000
const FOLDERS = [
  { root: 'libs/csharp/Rasm', branch: 'cs' },
  { root: 'libs/csharp/Rasm.Element', branch: 'cs' },
  { root: 'libs/csharp/Rasm.Materials', branch: 'cs' },
  { root: 'libs/csharp/Rasm.Bim', branch: 'cs' },
  { root: 'libs/csharp/Rasm.Fabrication', branch: 'cs' },
  { root: 'libs/csharp/Rasm.AppHost', branch: 'cs' },
  { root: 'libs/csharp/Rasm.AppUi', branch: 'cs' },
  { root: 'libs/csharp/Rasm.Compute', branch: 'cs' },
  { root: 'libs/python/artifacts', branch: 'py' },
  { root: 'libs/python/compute', branch: 'py' },
  { root: 'libs/python/data', branch: 'py' },
  { root: 'libs/python/geometry', branch: 'py' },
  { root: 'libs/python/runtime', branch: 'py' },
  { root: 'libs/typescript/core', branch: 'ts' },
  { root: 'libs/typescript/data', branch: 'ts' },
  { root: 'libs/typescript/iac', branch: 'ts' },
  { root: 'libs/typescript/runtime', branch: 'ts' },
  { root: 'libs/typescript/security', branch: 'ts' },
  { root: 'libs/typescript/ui', branch: 'ts' },
]
const BRANCHES = [
  { root: 'libs/csharp/.planning', branch: 'cs' },
  { root: 'libs/python/.planning', branch: 'py' },
  { root: 'libs/typescript/.planning', branch: 'ts' },
  { root: 'libs/.planning', branch: 'core' },
]

// --- [SHARED] ---
const CTX = 'Rasm monorepo, planning phase. Repo-wide index-doc sanity pass: every README.md and ARCHITECTURE.md in libs/ becomes truthful, dense, and gate-clean. ' +
  'ONE FENCE: libs/csharp/Rasm.Persistence/** has a live campaign writer — never edit under it; a fact or mirror row that needs a Persistence-side edit is recorded as a residual, never touched. ' +
  'SCOPE IS TIGHT: README.md, ARCHITECTURE.md, and gate-named fence lines in .planning pages — .api/ catalogs, manifests, csproj files, and design-page content are OUT of scope entirely; this is a fast focused pass, never a rebuild. ' +
  'LAW AT SOURCE: the seam grammar, the closed [KIND] vocabulary, the 160-column fence-line law, and the package-card law live at libs/.planning/README.md — read them there. ' +
  'All prose follows docs/standards/style-guide.md: declarative agent-facing law, no meta narrative, no hedging, no session or process references, no fragile enumerations (file/folder counts that rot). Never run git commit.'
const CENSUS = { type: 'object', additionalProperties: false, required: ['facts', 'gate', 'summary'], properties: {
  facts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['root', 'fact'], properties: {
    root: { type: 'string' }, fact: { type: 'string' } } } },
  gate: { type: 'array', items: { type: 'string' } },
  summary: { type: 'string' } } }
const APPLIED = { type: 'object', additionalProperties: false, required: ['applied', 'report'], properties: {
  applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  residuals: { type: 'array', items: { type: 'string' } },
  report: { type: 'string' } } }

const censusPrompt = (b) => CTX + '\n\nTASK: read-only census over the ' + b + ' scope. For every folder in scope derive the REAL facts from disk, never from the current doc prose: the actual .planning sub-domain topology and page set, the actual seam rows each ARCHITECTURE carries with their [KIND] tags and mirror endpoints (name every row whose mirror is absent, mismatched, or non-canonical), the actual package registries vs the folder README groups, and every stale/wrong/meta/hedging prose site in the README/ARCHITECTURE pair (anchors mandatory). Then run `' + GATE + ' <file>` on each folder\'s README.md, ARCHITECTURE.md, and every .planning page, and return the finding lines in `gate`. Facts are evidence for the refine writers — terse, anchored, one line each.'
const refinePrompt = (f, facts, gate) => CTX + '\n\nTASK: index-doc refine for ' + f.root + ' (WRITER — you own that folder\'s README.md, ARCHITECTURE.md, and the codemap/seams fences plus over-cap fence lines inside its .planning pages; nothing outside the folder except recording residuals). CENSUS FACTS: ' + JSON.stringify(facts) + '\nGATE FINDINGS: ' + JSON.stringify(gate) + '\n' +
  'Rebuild the prose TRUTHFUL first, format second: (1) ARCHITECTURE — the codemap matches real disk topology; every tree-line comment is rebuilt as high-signal what/why for agents (the owning concern and its load-bearing law), never code-body detail, never neutral filler; the seams fence carries only real, canonically-tagged, both-ends-mirrored rows (a mirror needing a Persistence-side or other-fenced edit is a residual). (2) README — router matches real pages; package cards conform to the dash-led one-line law; prose leads with the controlling rule, zero meta/hedging/stale content. (3) .planning pages — fix ONLY fence-scope defects the gate names (rebuild the comment to fit, never truncate contracts) and any seam row your ARCHITECTURE corrections ripple into; page design content is untouched. (4) GATE: run `' + GATE + ' <file>` on every file you edit until zero findings. Meaning is preserved everywhere; density rises, demand never weakens.'
const mirrorPrompt = (reports) => CTX + '\n\nTASK: cross-branch mirror close (WRITER over every libs/ ARCHITECTURE seams fence outside the Persistence fence). (1) Every cross-folder and cross-language seam row agrees with its mirror — same [KIND], same shared-shape naming, both endpoints real files; disagreements resolve to the landed-disk truth (read the endpoint pages, never guess); rows whose mirror sits inside the Persistence fence are recorded residuals. (2) FINAL PROOF: run `' + GATE + ' libs` and fix every remaining finding outside the fence; report the terminal finding count with the fenced remainder enumerated. REPORTS: ' + JSON.stringify(reports)

// --- [COMPOSITION] ---
phase('Census')
const scopes = ['cs', 'py', 'ts', 'core']
const censuses = await parallel(scopes.map((b) => () => agent(
  censusPrompt(b === 'core' ? 'libs/.planning core (the Tier-0 docs)' : b + ' branch (' + FOLDERS.filter((f) => f.branch === b).map((f) => f.root).join(', ') + ' + ' + BRANCHES.find((x) => x.branch === b).root + ')'),
  { label: 'census:' + b, phase: 'Census', model: 'sonnet', effort: 'medium', schema: CENSUS, stallMs: STALL })))
const factsFor = (root) => censuses.filter(Boolean).flatMap((c) => (c.facts || []).filter((x) => x.root.indexOf(root) === 0))
const gateFor = (root) => censuses.filter(Boolean).flatMap((c) => (c.gate || []).filter((g) => g.indexOf(root) === 0))
phase('Refine')
const units = FOLDERS.concat(BRANCHES)
const refined = await parallel(units.map((f) => () => agent(refinePrompt(f, factsFor(f.root), gateFor(f.root)),
  { label: 'refine:' + f.root.split('/').slice(-2).join('/'), phase: 'Refine', model: 'fable', effort: 'high', schema: APPLIED, stallMs: STALL })))
phase('Mirrors')
const reports = refined.filter(Boolean).map((r) => ({ report: r.report, residuals: r.residuals || [] }))
const close = await agent(mirrorPrompt(reports), { label: 'mirrors', phase: 'Mirrors', model: 'fable', effort: 'high', schema: APPLIED, stallMs: STALL })
return { census: censuses.filter(Boolean).map((c) => c.summary), refined: reports.length,
  residuals: reports.flatMap((r) => r.residuals).concat((close && close.residuals) || []),
  close: close && close.report }
