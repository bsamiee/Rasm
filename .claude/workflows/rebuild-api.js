export const meta = {
  name: 'rebuild-api',
  whenToUse: 'Rebuild every .api catalog under a target root to full integration-shaped capability.',
  description: 'Rebuild every .api catalog under a target root to FULL first-class, integration-shaped capability — document each package full advanced surface AND how packages STACK into single dense rails, verified against real members. Language-agnostic: members verified via assay api over host DLLs / NuGet / Python distributions / node_modules. args = optional scope (e.g. "libs/python" or "libs/csharp/Rasm.Bim"); empty = all of libs.',
  phases: [
    { title: 'API-Discover', detail: 'list every .api catalog file under the target' },
    { title: 'API-Rebuild', detail: 'per small batch: extract-full -> refine-integration -> harden adversarially, pooled at CAP=14' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const BATCH = 4 // .api files per agent — deep enough per file, many agents for parallelism
const STAGGER_MS = 1500

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. .api catalogs are agent-facing declarative records of a package useful surface that DESIGN PAGES compose against. CLAUDE.md ' +
    'DEPENDENCY_POLICY: mine each admitted package to its FULL useful capability; prefer ecosystem primitives over reinvention; internalize ' +
    'capability into canonical owners; treat dependencies as first-class implementation surfaces. House .api format: header (package / version / ' +
    'license / build-floor / marker or target), then member sections grouped by concern, backticked symbols + signatures + a consumer/boundary ' +
    'note. NO provenance/process narration, NO freshness tails. Cite REAL members only — verify via `uv run --frozen python -m tools.assay api ' +
    'resolve <pkg>` (assay api owns external-artifact reflection over host DLLs, NuGet, installed Python distributions, and node_modules ' +
    'declarations per CLAUDE.md OWNER_ROUTING); fall back to the package source/official surface only if reflection is blocked, tagging the ' +
    'catalog accordingly. READ tools/assay/README.md FIRST for the assay api-arm contract (its resolve/decompile/reflection invocation, supported ' +
    'artifact kinds, and JSON output shape) so you drive it correctly rather than guessing flags.',
  'MANDATE — INTEGRATION-SHAPED, NOT SURFACE-LEVEL: a rebuilt .api documents (a) the package full ADVANCED surface (combinators, hooks, native ' +
    'pipelines, discriminators, async mirrors — not just the basic members), AND (b) the INTEGRATION patterns the dense design should compose — ' +
    'how this library STACKS with the other admitted libs into single rails (e.g. a decode hook feeding a discriminated model under a retry ' +
    'context with a telemetry span) — INCLUDING the SHARED/UNIVERSAL catalog tier where the language has one (`libs/python/.api/` for Python: ' +
    'expression/msgspec/pydantic/pydantic-settings/stamina/beartype/structlog/opentelemetry/anyio; `libs/typescript/.api/` for TypeScript: ' +
    'effect/effect-platform/Schema/react), so a folder/area catalog documents stacking ONTO those universal rails, not only its sibling-folder ' +
    'libs. C# has NO central tier — its universals are Thinktecture/LanguageExt, and the `libs/csharp/Rasm/.api/` catalogs serve the ' +
    '`Rasm/Geometry` planning effort. The catalog GUIDES the rebuild toward first-class, stacked usage. Reject surface-level member lists.',
  'WRITE-FULLY MANDATE: every correction you identify you MUST make NOW via Edit/Write directly in the .api file — the structured fix-log is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list or would/should hedge; leave nothing behind. If a catalog is already mature and correct, ' +
    'return verdict=clean — never invent edits.',
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
const rebuildPrompt = (files) => [
  LAW, '',
  'TASK: REBUILD these .api catalogs to FULL first-class, integration-shaped capability (fix-in-place, read-then-extend; never shrink real content): ' + files.join(', ' +
    ''),
  'For EACH file run the same 3-lens write: (1) EXTRACT-FULL — confirm the package and document its full useful ADVANCED surface ' +
    '(combinators/hooks/async mirrors/discriminators/native pipelines), not the basic subset; (2) REFINE/REFACTOR — restructure to ' +
    'integration-shaped, documenting how this lib STACKS with sibling admitted libs into single dense rails; (3) HARDEN — as a HOSTILE reviewer ' +
    'who assumes the catalog is naive until proven otherwise, adversarially remove every phantom member, wrong floor/marker/target, ' +
    'surface-level/naive framing, missing license/ABI/runtime flag, and un-stacked single-feature framing. Verify members via `uv run --frozen ' +
    'python -m tools.assay api resolve`. Also close any gap a consuming design page genuinely needs (a specific member/signature the design ' +
    'composes). Return the fix-log (files + verdict + residual — each residual a {files: [the catalog paths a cross-CATALOG fix must also touch], ' +
    'claim} object; everything within these catalogs you fix yourself).',
].join('\n')
const processBatch = async (w, tag) => {
  const r = await agent(rebuildPrompt(w.files), { label: 'api:' + w.files[0].split('/.api/')[0].split('/').pop() + '+' + (w.files.length - 1), phase: tag, schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  return r ? { files: w.files, log: r } : null
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('API-Discover')
const inv = await agent('List every .api catalog file under ' + SWEEP + ' — markdown files at paths matching */.api/*.md (and any nested .api ' +
  'subdirs). Return each as a repo-relative path. If none exist, return an empty list. Use find; do not cd.', { label: 'discover', phase: 'API-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const FILES = ((inv && inv.files) || []).filter(Boolean)
const pending = chunk(FILES, BATCH).map((files) => ({ files }))
const totalFiles = FILES.length
const totalBatches = pending.length
log('API discover under ' + SWEEP + ': ' + totalFiles + ' catalogs in ' + totalBatches + ' batches; pooling at CAP=' + CAP)

// --- [API_REBUILD]
phase('API-Rebuild')
const done = (await pool(pending, CAP, (w) => processBatch(w, 'API-Rebuild'))).filter(Boolean)

// --- [RECONCILE]
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
log('API rebuild: ' + done.length + '/' + totalBatches + ' batches (' + totalFiles + ' catalogs); reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' ' +
  'clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pipeline(
    clusters,
    (cl) => agent([LAW, '', 'TASK: RECONCILE these cross-CATALOG residuals the per-batch pass deferred. There is NO severity — address EVERY ' +
      'residual. Read EVERY listed .api catalog, make the cross-catalog fix in place (add the depended-on member/signature, align the stacking ' +
      'note across catalogs), verify members via `uv run --frozen python -m tools.assay api resolve`, never shrink real content. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', 'TASK: ADVERSARIAL COMPLETENESS VERIFY. A reconcile agent claims to have fixed the cross-catalog ' +
      'residuals below. Read the named catalogs from disk and prove, per claim, each: status "fixed" (real defect, now resolved), "invalid" (the ' +
      'claim is factually wrong — cite why), or "open" (real defect still NOT resolved). Default "open" on any doubt; "invalid" only when provably ' +
      'wrong. One verdict per claim plus overall. Claims:\n' + JSON.stringify(cl, null, 1) + '\nCatalogs touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_residual = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual), ' + dropped.length + ' dropped as invalid')
return { scope: SWEEP, catalogs: totalFiles, batches: totalBatches, complete: done.length, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped }
