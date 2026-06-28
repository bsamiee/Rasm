export const meta = {
  name: 'harden-api-batch',
  description: 'One-off 2nd-pass HARDEN over an explicit HARDCODED set of already-full .api catalogs (the 51 the args-dropped focused run never reached): adversarial harden (remove phantom members / wrong floor-marker-target / surface framing, deepen cross-package stacking, verify members) PLUS strip the stale banned version-gating META prose. Targets are hardcoded in the script because the Workflow scriptPath args-drop bug makes args untrustworthy, so this takes NO args. BATCH=4, CAP=8, single-agent harden, then union-find cross-catalog reconcile.',
  whenToUse: 'Re-run the harden over committed/modified .api catalogs after an args-dropped focused run only hardened the untracked subset.',
  phases: [
    { title: 'API-Harden', detail: 'per batch of the hardcoded set: adversarial harden + strip stale version-gating meta, pooled at CAP=8' },
    { title: 'Reconcile', detail: 'union-find cluster cross-catalog residuals -> fix -> adversarial completeness verify' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 8
const BATCH = 4
const STAGGER_MS = 1500
const FILES = [
  'libs/csharp/Rasm.AppHost/.api/api-tensors.md',
  'libs/csharp/Rasm.Bim/.api/api-brickschema-net.md',
  'libs/csharp/Rasm.Bim/.api/api-dotbim.md',
  'libs/csharp/Rasm.Bim/.api/api-dragonfly-schema.md',
  'libs/csharp/Rasm.Bim/.api/api-honeybee-schema.md',
  'libs/csharp/Rasm.Bim/.api/api-nts-geojson4stj.md',
  'libs/csharp/Rasm.Bim/.api/api-nts-geopackage.md',
  'libs/csharp/Rasm.Bim/.api/api-openstudio.md',
  'libs/csharp/Rasm.Bim/.api/api-structuralanalysisformat.md',
  'libs/csharp/Rasm.Bim/.api/api-vividorange-cases.md',
  'libs/csharp/Rasm.Bim/.api/api-vividorange-countries.md',
  'libs/csharp/Rasm.Bim/.api/api-vividorange-loads.md',
  'libs/csharp/Rasm.Bim/.api/api-vividorange-stages.md',
  'libs/csharp/Rasm.Bim/.api/api-xbim-cobieexpress.md',
  'libs/csharp/Rasm.Compute/.api/api-brief-finite-element.md',
  'libs/csharp/Rasm.Compute/.api/api-fealite2d-plotting.md',
  'libs/csharp/Rasm.Compute/.api/api-fealite2d.md',
  'libs/csharp/Rasm.Fabrication/.api/api-dstv-net.md',
  'libs/csharp/Rasm.Fabrication/.api/api-hashing.md',
  'libs/csharp/Rasm.Materials/.api/api-messagepack.md',
  'libs/csharp/Rasm.Materials/.api/api-vividorange-uncertainties.md',
  'libs/csharp/Rasm.Persistence/.api/api-ara3d-bimopenschema.md',
  'libs/csharp/Rasm.Persistence/.api/api-highperformance.md',
  'libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md',
  'libs/python/data/.api/brightway2.md',
  'libs/python/data/.api/bw-processing.md',
  'libs/python/data/.api/bw2calc.md',
  'libs/python/data/.api/bw2data.md',
  'libs/python/data/.api/bw2io.md',
  'libs/python/data/.api/epdx.md',
  'libs/python/data/.api/laspy.md',
  'libs/python/data/.api/obspec-utils.md',
  'libs/python/data/.api/olca-ipc.md',
  'libs/python/data/.api/openepd.md',
  'libs/python/data/.api/premise.md',
  'libs/python/geometry/.api/dragonfly-core.md',
  'libs/python/geometry/.api/dragonfly-energy.md',
  'libs/python/geometry/.api/honeybee-core.md',
  'libs/python/geometry/.api/honeybee-energy-standards.md',
  'libs/python/geometry/.api/honeybee-energy.md',
  'libs/python/geometry/.api/honeybee-openstudio.md',
  'libs/python/geometry/.api/honeybee-standards.md',
  'libs/python/geometry/.api/ladybug-comfort.md',
  'libs/python/geometry/.api/ladybug-core.md',
  'libs/python/geometry/.api/ladybug-geometry.md',
  'libs/python/geometry/.api/lbt-recipes.md',
  'libs/python/geometry/.api/pollination-handlers.md',
  'libs/python/geometry/.api/queenbee.md',
  'libs/python/runtime/.api/lbt-recipes.md',
  'libs/python/runtime/.api/pollination-handlers.md',
  'libs/python/runtime/.api/queenbee.md',
]

// --- [MODELS] ----------------------------------------------------------------------------
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['hardened', 'clean'] }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. .api catalogs are agent-facing declarative records of a package useful surface that DESIGN PAGES compose against. CLAUDE.md ' +
    'DEPENDENCY_POLICY: mine each admitted package to its FULL useful capability; prefer ecosystem primitives over reinvention; treat dependencies ' +
    'as first-class implementation surfaces. House .api format: header (package / version / license / build-floor / marker or target), then member ' +
    'sections grouped by concern, backticked symbols + signatures + a consumer/boundary note. NO provenance/process narration, NO freshness tails. ' +
    'Cite REAL members only — verify via `uv run python -m tools.assay api resolve <pkg>` (assay api owns external-artifact reflection over host ' +
    'DLLs, NuGet, installed Python distributions, and node_modules). MANY previously companion-gated Python packages (the LBT + Brightway families) ' +
    'are NOW INSTALLED in the cp315 env and ARE assay-reflectable — reflect them; fall back to source/official surface only where reflection is ' +
    'genuinely blocked. READ tools/assay/README.md FIRST for the assay api-arm contract.',
  'MANDATE — INTEGRATION-SHAPED, NOT SURFACE-LEVEL: a catalog documents (a) the package full ADVANCED surface (combinators, hooks, native ' +
    'pipelines, discriminators, async mirrors), AND (b) the INTEGRATION patterns the dense design composes — how this library STACKS with the other ' +
    'admitted libs into single rails, INCLUDING the shared/universal tier (`libs/python/.api/` for Python; C# has no central tier and keeps per-folder ' +
    'copies). Reject surface-level member lists.',
  'STRIP VERSION-GATING META: gate/wheel/floor/companion-gating RATIONALE is BANNED in .api files and is now mostly STALE (the python infra was ' +
    'de-gated). DELETE every such line — "COMPANION-GATED", "Manifest-pinned python_version<3.15", "no cpXXX wheel", "the cluster pins <3.15", ' +
    '"lifts once wheels land", build-floor rationale, marker-saga prose. Keep ONLY a minimal one-clause evidence tag where the package is genuinely ' +
    'NOT installed in the active env (e.g. "members source-verified against vX") — no reasoning about why. NEVER re-introduce any version-pinning or ' +
    'wheel-availability commentary. This is mandatory on EVERY catalog you touch.',
  'WRITE-FULLY MANDATE: every correction you identify you MUST make NOW via Edit directly in the .api file — the structured fix-log REPORTS edits ' +
    'ALREADY MADE, never a to-do list. If a catalog is already mature, correct, and carries no stale gate meta, return verdict=clean. A cross-CATALOG ' +
    'item you cannot fix from this batch goes in `residual` as a {files, claim} object.',
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
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }
const hardenPrompt = (files) => [LAW, '',
  'TASK: 2nd-pass ADVERSARIAL HARDEN of these ALREADY-FULL .api catalogs (NOT stubs) — read each from disk and attack it as a HOSTILE reviewer who ' +
    'assumes it is naive until proven: ' + files.join(', '),
  'For EACH file: (1) VERIFY every documented member exists (assay api resolve; the LBT/Brightway families are now installed so reflect them, do not ' +
    'leave them source-only) and DELETE every phantom; (2) fix wrong floor/marker/target, missing license/ABI/runtime flag, surface-level/naive ' +
    'framing, and un-stacked single-feature framing — deepen to the integration-shaped, cross-library-stacked form; (3) STRIP all stale/banned ' +
    'version-gating / wheel / floor / companion-gating RATIONALE prose per the LAW (replace a now-false "companion-gated python_version<3.15" claim ' +
    'with nothing, or a minimal "source-verified against vX" only where genuinely uninstalled); (4) close any member gap a consuming design page ' +
    'needs. Never shrink real content. Return the fix-log (files + verdict hardened|clean + residual — each residual a {files:[catalog paths a ' +
    'cross-catalog fix must also touch], claim} object; everything within these catalogs you fix yourself).'].join('\n')
const processBatch = async (w) => {
  const r = await agent(hardenPrompt(w.files), { label: 'harden:' + w.files[0].split('/.api/')[0].split('/').pop() + '+' + (w.files.length - 1), phase: 'API-Harden', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  return r ? { files: w.files, log: r } : null
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('API-Harden')
const pending = chunk(FILES, BATCH).map((files) => ({ files }))
log('harden-api-batch: ' + FILES.length + ' catalogs in ' + pending.length + ' batches; pooling at CAP=' + CAP)
const done = (await pool(pending, CAP, processBatch)).filter(Boolean)

const allRes = []
for (const r of done) if (r.log && r.log.residual) for (const x of r.log.residual) allRes.push(typeof x === 'string' ? { files: r.files, claim: x } : { files: x.files && x.files.length ? x.files : r.files, claim: x.claim })
const uniq = [...new Map(allRes.map((r) => [r.files.join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('harden-api-batch: ' + done.length + '/' + pending.length + ' batches; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pipeline(
    clusters,
    (cl) => agent([LAW, '', 'TASK: RECONCILE these cross-CATALOG residuals the per-batch harden deferred. Address EVERY residual: read each listed ' +
      'catalog, make the cross-catalog fix in place (add the depended-on member, align the stacking note, strip any stale gate meta), verify members ' +
      'via assay api resolve, never shrink real content. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', 'TASK: ADVERSARIAL COMPLETENESS VERIFY. Read the named catalogs from disk and prove, per claim: "fixed" ' +
      '(real defect, now resolved), "invalid" (claim factually wrong — cite why), or "open" (real defect still NOT resolved). Default "open" on doubt. ' +
      'Claims:\n' + JSON.stringify(cl, null, 1) + '\nCatalogs touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_residual = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('harden-api-batch reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open, ' + dropped.length + ' dropped as invalid')
return { targets: FILES.length, batches: pending.length, complete: done.length, clusters: clusters.length, hard_residual, dropped }
