export const meta = {
  name: 'tmp-artifacts-api-complete',
  whenToUse: 'Complete every stub/thin/new .api catalog under libs/python/artifacts to full integration-shaped capability before the artifacts ground-up rebuild campaign.',
  description: 'Artifacts-scoped .api completion: classify libs/python/artifacts/.api by line-count into stub / new-package / thin / rich, BUILD every non-rich catalog (1 agent per file) to FULL first-class integration-shaped capability — full advanced surface AND how it STACKS onto BOTH .api tiers (the shared libs/python/.api rails + the folder catalogs) into single dense rails — verified against real members via assay api, then reconcile cross-catalog residuals. The ~45 already-rich catalogs are skipped. Reads the campaign brief libs/python/artifacts/.planning/_REBUILD_BRIEF.md for the admit/replace roster and the categorical-best / no-overlap mandate. Temporary campaign workflow; takes no args.',
  phases: [
    { title: 'API-Discover', detail: 'classify every libs/python/artifacts/.api catalog by line-count; emit the non-rich build set, skip the rich' },
    { title: 'API-Build', detail: 'per catalog (1 agent/file): extract-full -> integration-shape -> harden adversarially, verify via assay api, stack both .api tiers, pooled at CAP' },
    { title: 'Reconcile', detail: 'union-find cluster cross-catalog residuals -> fix(max) -> adversarial verify(xhigh)' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const API_DIR = 'libs/python/artifacts/.api'
const BRIEF = 'libs/python/artifacts/.planning/_REBUILD_BRIEF.md'

// --- [INPUTS] ----------------------------------------------------------------------------

// scope is fixed to the artifacts catalog set; args are ignored (campaign-locked target)

// --- [MODELS] ----------------------------------------------------------------------------

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['build', 'skipped'], properties: { build: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['file', 'klass'], properties: { file: { type: 'string' }, klass: { type: 'string', enum: ['stub', 'new', 'thin'] }, lines: { type: 'integer' }, pkg: { type: 'string' } } } }, skipped: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['built', 'refined', 'clean'] }, members_verified: { type: 'string' }, stacked: { type: 'string' }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/python/artifacts. .api catalogs are agent-facing declarative records of a package useful surface that the .planning DESIGN PAGES compose against. ' +
    'CLAUDE.md DEPENDENCY_POLICY: mine each admitted package to its FULL useful capability; prefer ecosystem primitives over reinvention; internalize capability ' +
    'into canonical owners; treat dependencies as first-class implementation surfaces. House .api format: header (package / version / license / build-floor / marker ' +
    'or cp-gate / target), then member sections grouped by concern, with backticked symbols + signatures + a consumer/boundary note keyed to the owning .planning page. ' +
    'NO provenance/process narration, NO freshness tails. Cite REAL members only — verify via `uv run --frozen python -m tools.assay api resolve <pkg>` (assay api owns ' +
    'external-artifact reflection over installed Python distributions and node_modules per CLAUDE.md OWNER_ROUTING); fall back to the package source / official surface / ' +
    'Context7 ONLY if reflection is blocked (a new package not yet installed, a cp-gated package absent on this interpreter), tagging the catalog accordingly. READ ' +
    'tools/assay/README.md FIRST for the assay api-arm contract (its resolve/decompile/reflection invocation, supported artifact kinds, JSON output shape) so you drive ' +
    'it correctly rather than guessing flags.',
  'CAMPAIGN BRIEF — READ ' + BRIEF + ' FIRST: it fixes the admit/replace roster (which package is the categorical-best owner of each concern, which older package it ' +
    'supersedes), the no-overlap mandate, and the both-`.api`-tier stacking law. A new catalog documents the surface of a package the brief admits; tag a superseded ' +
    'package the brief flags for removal, and shape every catalog so the owning .planning page reaches the package at full depth.',
  'MANDATE — INTEGRATION-SHAPED, NOT SURFACE-LEVEL: a built .api documents (a) the package full ADVANCED surface (combinators, hooks, native pipelines, discriminators, ' +
    'async mirrors — not just the basic members), AND (b) the INTEGRATION patterns the dense design composes — how this library STACKS into single rails across BOTH ' +
    'tiers: the SHARED/UNIVERSAL `libs/python/.api/*.md` rails (expression Result/Option, msgspec/pydantic discriminated models, beartype validation, stamina retry, ' +
    'structlog + opentelemetry spans, anyio structured concurrency, numpy) layered ON TOP OF the folder-specific `libs/python/artifacts/.api/*.md` domain packages — so ' +
    'a catalog documents stacking ONTO the universal rails, never a flat folder-only subset. The catalog GUIDES the rebuild toward first-class stacked usage. Reject ' +
    'surface-level member lists and thin rename framing.',
  'WRITE-FULLY MANDATE: every correction you identify you MUST make NOW via Edit/Write directly in the .api file — the structured fix-log is a REPORT of edits ALREADY ' +
    'MADE, never a to-do list or a would/should hedge; leave nothing behind except genuine cross-CATALOG items (report those in residual). A `(placeholder)` stub or a ' +
    'thin catalog is NEVER acceptable output — build it to a full sibling-grade catalog. Only return verdict=clean for a catalog already mature and correct; never invent edits.',
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
const nameOf = (p) => p.split('/').pop().replace(/\.md$/, '')
const buildPrompt = (t) => [
  LAW, '',
  'TASK: BUILD the .api catalog ' + t.file + ' (classified `' + t.klass + '`) to FULL first-class, integration-shaped capability — a `stub`/`new` catalog is authored from ' +
    'scratch to a full sibling-grade catalog, a `thin` catalog is deepened (read-then-extend, never shrink real content). Run the 3-lens write: (1) EXTRACT-FULL — confirm ' +
    'the package and document its full useful ADVANCED surface (combinators/hooks/async mirrors/discriminators/native pipelines), not the basic subset; for a NEW package ' +
    'resolve its real surface via `uv run --frozen python -m tools.assay api resolve` (or Context7 / the official surface if it is not yet installed or is cp-gated absent ' +
    'here, tagging the catalog). (2) INTEGRATION-SHAPE — structure to document how this lib STACKS across BOTH .api tiers (the shared libs/python/.api rails ON TOP OF the ' +
    'folder packages) into single dense rails, and which .planning page(s) own it per the brief roster. (3) HARDEN — as a HOSTILE reviewer who assumes the catalog is ' +
    'naive until proven otherwise, adversarially remove every phantom member, wrong floor/marker/cp-gate/license/target, surface-level framing, and un-stacked ' +
    'single-feature framing; close any gap a consuming design page genuinely needs (a specific member/signature it composes). Match the house format of a rich sibling ' +
    'catalog in the same folder. Return the fix-log (file + verdict + members_verified + stacked + residual — each residual a {files: [the catalog paths a cross-CATALOG ' +
    'fix must also touch], claim} object; everything within THIS catalog you fix yourself).',
].join('\n')
const processFile = async (t) => {
  const r = await agent(buildPrompt(t), { label: 'api:' + nameOf(t.file), phase: 'API-Build', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  return r ? { file: t.file, log: r } : null
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('API-Discover')
const inv = await agent('Classify every .api catalog under ' + API_DIR + '/ by line-count. List the directory and `wc -l` each `*.md`. Classify each: `stub` (a ' +
  'placeholder / 1-3 line file), `new` (a freshly-created placeholder for a package newly admitted this campaign — treat any 1-3 line file whose package is in the ' + BRIEF + ' ' +
  'admit roster as `new`), `thin` (a real but under-built catalog, roughly under ~60 substantive lines or missing the both-tier stacking + advanced surface), or `rich` (a ' +
  'full integration-shaped catalog, roughly 150+ lines — SKIP these). Read ' + BRIEF + ' to know the admit roster. Return: build — one entry per NON-rich catalog {file: ' +
  'repo-relative path, klass: stub|new|thin, lines, pkg: the package id}; skipped — the repo-relative paths of the rich catalogs. Use find/ls/wc; do not cd; do not edit ' +
  'anything.', { label: 'discover', phase: 'API-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const BUILD = ((inv && inv.build) || []).filter((t) => t && t.file)
const SKIPPED = ((inv && inv.skipped) || []).filter(Boolean)
log('API discover under ' + API_DIR + ': ' + BUILD.length + ' to build (' + BUILD.filter((t) => t.klass === 'new').length + ' new, ' + BUILD.filter((t) => t.klass === 'stub').length +
  ' stub, ' + BUILD.filter((t) => t.klass === 'thin').length + ' thin), ' + SKIPPED.length + ' rich skipped; pooling at CAP=' + CAP)
if (!BUILD.length) { log('No non-rich catalogs to build — all artifacts .api catalogs are already rich.'); return { dir: API_DIR, build: 0, skipped: SKIPPED.length } }

// --- [API_BUILD]
phase('API-Build')
const done = (await pool(BUILD, CAP, processFile)).filter(Boolean)

// --- [RECONCILE]
const allRes = []
for (const r of done) if (r.log && r.log.residual) for (const x of r.log.residual) allRes.push(typeof x === 'string' ? { files: [r.file], claim: x } : { files: x.files && x.files.length ? x.files : [r.file], claim: x.claim })
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('API build: ' + done.length + '/' + BUILD.length + ' catalogs; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent([LAW, '', 'TASK: RECONCILE these cross-CATALOG residuals the per-file pass deferred. There is NO severity — address EVERY residual. Read EVERY listed ' +
      '.api catalog, make the cross-catalog fix in place (add the depended-on member/signature, align the both-tier stacking note across catalogs, resolve a roster ' +
      'overlap the brief flags), verify members via `uv run --frozen python -m tools.assay api resolve`, never shrink real content. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix:' + i, phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 })
    if (!fix) return null
    const verify = await agent([LAW, '', 'TASK: ADVERSARIAL COMPLETENESS VERIFY. A reconcile agent claims to have fixed the cross-catalog residuals below. Read the named ' +
      'catalogs from disk and prove, per claim: status "fixed" (real defect, now resolved), "invalid" (the claim is factually wrong — cite why), or "open" (real defect ' +
      'still NOT resolved). Default "open" on any doubt; "invalid" only when provably wrong. One verdict per claim plus overall. Claims:\n' + JSON.stringify(cl, null, 1) + '\nCatalogs touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_residual = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual), ' + dropped.length + ' dropped as invalid')
return { dir: API_DIR, build: BUILD.length, complete: done.length, skipped: SKIPPED.length, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped }
