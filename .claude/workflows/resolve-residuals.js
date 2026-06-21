export const meta = {
  name: 'resolve-residuals',
  description: 'Focused cross-file residual resolution over libs/python design pages: investigate/research each open reconcile residual (read every spanned page + both-tier .api + assay api for member claims + domain research), decide the canonical resolution, implement the design-doc changes across ALL spanned pages at the rebuild-python bar, then adversarially verify. Deferred design decisions are RESOLVED with research; realization-gate items are signature-locked in the owning design page (never realized as .py). args = optional {residuals:[{id,claim,hint}]} override; default = the 7 open compute residuals.',
  phases: [
    { title: 'Investigate', detail: 'per residual: read spanned pages + both-tier .api + assay/domain research -> a precise resolution plan (read-only)' },
    { title: 'Implement', detail: 'per file-cluster: apply every resolution across the spanned design pages, fix-in-place at the rebuild bar' },
    { title: 'Verify', detail: 'per residual: adversarially confirm it is implemented, correct, and consistent across pages' },
  ],
}

const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'decision', 'files'], properties: { id: { type: 'string' }, claim: { type: 'string' }, decision: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, api_members: { type: 'array', items: { type: 'string' } }, edits: { type: 'array', items: { type: 'string' } }, open_question: { type: 'string' } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['resolved', 'partial', 'clean'] }, applied: { type: 'array', items: { type: 'string' } }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'status', 'summary'], properties: { id: { type: 'string' }, status: { type: 'string', enum: ['resolved', 'open'] }, evidence: { type: 'string' }, summary: { type: 'string' } } }

// --- [HARNESS] -- ramped bounded worker pool ---------------------------------------------
const STAGGER_MS = 1500
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async (slot) => {
    if (slot) await new Promise((res) => setTimeout(res, slot * STAGGER_MS))
    while (next < items.length) { const i = next++; out[i] = await worker(items[i], i) }
  }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, (_, slot) => run(slot)))
  return out
}
const clusterByFiles = (plans) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const p of plans) { (p.files || []).forEach(add); for (let i = 1; i < (p.files || []).length; i++) parent.set(find(p.files[i]), find(p.files[0])) }
  const by = new Map()
  for (const p of plans) { const root = (p.files && p.files.length) ? find(p.files[0]) : p.id; (by.get(root) || by.set(root, []).get(root)).push(p) }
  return [...by.values()]
}
const CAP = 9
const ROOT = 'libs/python'

// --- [INPUT] -- args = optional {residuals:[...]} or an array; default = the 7 open compute residuals ---
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const DEFAULT_RESIDUALS = [
  { id: 'R6-spectral-contract', claim: 'SpectralReadout.fold is fed Welch PSD power by signal.md but amplitude `xp.abs(spectrum)` by transform.md\'s Fourier path, into one parameter named `magnitude`; the owner contract is ambiguous. DECIDE one canonical contract in the OWNER transform.md (rename `magnitude` to a power/amplitude-explicit spine, or have signal.md pass `sqrt(pxx)`), and apply it consistently across transform.md and signal.md.', hint: 'compute analysis transform.md (owner) + signal.md; SpectralReadout.fold' },
  { id: 'R8-hilbert-portability', claim: 'transform.md `_analytic` runs `scipy.signal.hilbert(x)` on the resolved `xp` under blanket Array-API portability prose, but scipy.signal.hilbert is NOT Array-API-aware (jax-skipped, dask-partial). Scope the `xp` portability prose to `scipy.fft`, and treat the hilbert analytic arm as numpy-resident with an explicit numpy floor/caveat.', hint: 'compute analysis transform.md; scipy.signal.hilbert vs scipy.fft Array-API support' },
  { id: 'R15-content-key', claim: 'convex.md `_seed_arrays`/`_convex_key` and design.md `_design_key` concatenate problem-data arrays under a coarse fmt (collision risk); apply the SAME slot-index/shape discriminant program.md already adopted, OR add an explicit structural non-collidability confirmation if the buffers cannot collide.', hint: 'compute solvers/convex.md + optimization/design.md + program.md (the slot-index/shape discriminant exemplar)' },
  { id: 'R20-argnums-consumer', claim: 'The `argnums` differentiated-argument witness on Differentiation/DiffReceipt has no current or planned autodiff-Jacobian consumer. DECIDE across sensitivity.md + study.md whether any consumer is intended: either wire a real consumer (the strongest design) or drop the witness; make the cross-file decision and apply it.', hint: 'compute solvers/sensitivity.md + experiments/study.md; argnums on Differentiation/DiffReceipt' },
  { id: 'R22-bracket-axis', claim: 'TOL_ONLY 1-D bracketing solvers (Bisection/GoldenSearch) in nonlinear.md need a bracket interval threaded into the optimistix solve entry as `options={"lower":..., "upper":...}`, but the run() closure passes no `options=`. Add a `bracket: tuple[float, float] | None` axis on SolverPolicy plus the matching `_route_cells`/entry adjustment that forwards it as optimistix `options`.', hint: 'compute solvers/nonlinear.md; SolverPolicy + _route_cells + optimistix options' },
  { id: 'R26-runtime-signatures', claim: 'The runtime owners `boundary`, `receipted`, `RuntimeRail`, and the `Redaction` policy that every solver page imports are specified only in ARCHITECTURE.md (codemap), not signature-locked on the runtime owner DESIGN PAGE; solver pages assume identical signatures unverifiably. SIGNATURE-LOCK the exact signatures on the runtime owner design page so the solver pages cite them consistently. The `.py` realization is DEFERRED to the implement pass — do NOT create `.py`; lock the spec only.', hint: 'runtime owner design page(s) for boundary/receipted/RuntimeRail/Redaction + runtime ARCHITECTURE.md + the compute solver pages that import them' },
  { id: 'R29-lineax-results', claim: 'linear.md cannot reduce vmapped `Solution.result` via `RESULTS.promote(...).name` the way nonlinear.md/differential.md do, because `lineax.RESULTS.promote` is not catalogued. REFLECT the real `lineax.RESULTS` members via assay api on the gated band, catalog them in `.api/lineax.md`, then either upgrade linear.md to the same batched-RESULTS reduction or document the divergence as intentional WITH the reflected evidence.', hint: 'compute solvers/linear.md + nonlinear.md + differential.md + libs/python/compute/.api/lineax.md; lineax.RESULTS.promote' },
]
const RESIDUALS = Array.isArray(input) ? input : (input && Array.isArray(input.residuals)) ? input.residuals : DEFAULT_RESIDUALS

// --- [DOCTRINE] -- design-page bar (matches rebuild-python) + residual-resolution mandate ---
const DESIGN_BAR = [
  'Rasm monorepo, ' + ROOT + ' planning corpus (markdown specs of intended Python module designs; code fences are the product, no .py yet). CLAUDE.md manifest law governs. DENSITY BAR: docs/stacks/python/ (README/language/shapes/surfaces-and-dispatch/rails-and-effects/algorithms/type-system/boundaries/runtime/system-apis) — every edit holds Python as dense, polymorphic, and rich as that bar.',
  'STACK .api CAPABILITY across BOTH tiers: the shared/universal branch catalogs `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*, protobuf, grpcio) AND the folder-specific `libs/python/<folder>/.api/*.md`. Cite ONLY members confirmed in those catalogs; maximize the shared/universal rails, never only the folder set.',
  'PY-VERSION LAW: Python 3.15 modern band only — PEP 585 builtin generics, PEP 604 unions, PEP 695 type parameters; NEVER `from __future__ import annotations`, NEVER `typing.List/Dict/Optional/Union`/`TypeVar`+`Generic`. Total `match` with `assert_never`; typed `Result`/`Option` rails; NEVER exception control flow in domain logic. ADT collapse (tagged unions, discriminated models, value objects with behavior) + data-driven dispatch tables; AOP for cross-cutting concerns; parameterize inputs AND outputs.',
  'PROSE + COMMENTS: high-signal design-spec prose per docs/standards/style-guide.md — lead with the controlling contract, one idea per paragraph, no hedges/provenance/process narration; BACKTICK every symbol/type/member/path. Keep canonical `# --- [LABEL]` section dividers; comment only where intent is not obvious; zero low-value comments.',
  'BOUNDARY LAW: keep every package/folder owner in its lane; internal code uses canonical names with mapping only at the edge; respect the workspace strata; do not trample a sibling owner while resolving a residual.',
].join('\n')
const RESIDUAL_LAW = [
  'YOU RESOLVE CROSS-FILE RESIDUALS: each residual is a real, open, cross-FILE defect or deferred decision the per-page rebuild + reconcile could not close. Resolve it PROPERLY and FULLY — add the functionality/feature/axis, unify the ambiguous contract, lock the signature — across EVERY design page it spans, leaving the corpus internally consistent.',
  'DEFERRED DECISIONS ARE RESOLVED, NOT PUNTED: where a residual is a design decision (a new policy axis, an autodiff consumer, a canonical contract), make the STRONGEST principled choice the domain + standards admit (research it; apply the 7-point density rubric; choose the most polymorphic, parameterized, future-ready form) and IMPLEMENT it. Only leave an `open_question` when the choice genuinely requires an operator product decision you cannot ground.',
  'REALIZATION-GATE ITEMS STAY IN THE DESIGN PAGE: where a residual notes an owner is "not yet realized as .py", the resolution is to SIGNATURE-LOCK the exact contract on the owning DESIGN PAGE so consumers cite it consistently — NEVER create `.py` source (that is the deferred implement pass).',
  'RESEARCH MANDATE — no guessing: where a claim hinges on a real package member, VERIFY it via `uv run --frozen python -m tools.assay api resolve <pkg>` (READ tools/assay/README.md FIRST for the api-arm contract and the correct invocation). For a GATED/companion package (a `python_version<\'3.15\'` or companion-band marker — e.g. cadquery-ocp, topologicpy, open3d, compas, manifold3d, pye57), resolve members on the CORRECT companion interpreter band per tools/assay/README.md; if the companion genuinely cannot resolve a member, document the gap explicitly in the catalog WITH the band caveat rather than leaving a vague hedge or asserting a phantom member. Where a claim catalogs missing members, WRITE them into the owning `.api/*.md` catalog — resolutions MAY edit `.api` catalog files as well as design pages. Where a claim hinges on a domain/math contract (e.g. spectral power vs amplitude), research the real convention before deciding. Cite only verified members.',
  'WRITE-FULLY: every edit you identify you MUST make NOW via Edit/Write; the fix-log REPORTS edits already made. Leave behind only a genuine still-cross-file item (residual) or a true operator decision (open_question).',
].join('\n')

const investigatePrompt = (r) => [DESIGN_BAR, '', RESIDUAL_LAW, '', 'TASK (INVESTIGATE — read-only, produce the resolution PLAN, edit NOTHING): residual `' + r.id + '`.\nCLAIM: ' + r.claim + '\nWHERE TO LOOK: ' + (r.hint || '') + '\nRead EVERY design page the claim spans + its siblings + the relevant `.api` catalogs (both tiers). Where the claim hinges on a real package member, VERIFY via `uv run --frozen python -m tools.assay api resolve` (read tools/assay/README.md first). Where it hinges on a domain/math contract, research the real convention. Then DECIDE the canonical resolution (strongest principled design; resolve deferred decisions, do not punt). Return the PLAN: `id`, the `decision` (what the canonical resolution IS), the exact repo-relative `files` to edit, the precise `edits` per file, the verified `.api` `api_members` to cite, and an `open_question` ONLY if it needs a true operator product decision.'].join('\n')
const implementPrompt = (cluster) => {
  const files = [...new Set(cluster.flatMap((p) => p.files || []))]
  return [DESIGN_BAR, '', RESIDUAL_LAW, '', 'TASK (IMPLEMENT — fix-in-place across these design pages): apply EVERY resolution plan below to the spanned pages at the full design bar, preserving all capability and regressing no page. Unify each cross-file contract/seam CONSISTENTLY across all pages it touches; cite only verified `.api` members; signature-lock (never create `.py`). Files in this cluster: ' + JSON.stringify(files) + '\nPLANS:\n' + JSON.stringify(cluster, null, 1) + '\nReturn the fix-log (files edited + verdict + applied), with `residual` only for a genuine still-cross-file item you could not close from these files.'].join('\n')
}
const verifyPrompt = (p) => [DESIGN_BAR, '', 'TASK (ADVERSARIAL VERIFY): residual `' + p.id + '`. Read the spanned design pages from disk and prove whether the resolution is ACTUALLY done: implemented (not hedged or merely described), correct (math/domain/`.api`-grounded — re-check any cited member), and CONSISTENT across every page it spans. Status `resolved` ONLY if genuinely complete and correct; else `open` with exactly what remains. Default to `open` on doubt.\nCLAIM: ' + (p.claim || '') + '\nINTENDED RESOLUTION: ' + (p.decision || '') + '\nFILES: ' + JSON.stringify(p.files || [])].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Investigate')
log('Investigating ' + RESIDUALS.length + ' residuals')
const investigateOne = (r) => agent(investigatePrompt(r), { label: 'investigate:' + r.id, phase: 'Investigate', schema: PLAN_SCHEMA, effort: 'max', stallMs: 300000 }).then((p) => p ? { ...p, id: p.id || r.id, claim: p.claim || r.claim } : null)
const firstPass = await pool(RESIDUALS, CAP, investigateOne)
// ONE bounded re-attempt for any residual whose investigate returned no plan: a transient agent death (connection closed mid-response) must never silently lose a residual. Death-recovery, not speculative retry.
const deadIdx = firstPass.map((p, i) => (p ? -1 : i)).filter((i) => i >= 0)
if (deadIdx.length) { log('Investigate produced no plan for ' + deadIdx.length + ' residual(s) — one re-attempt: ' + deadIdx.map((i) => RESIDUALS[i].id).join(',')); const re = await pool(deadIdx.map((i) => RESIDUALS[i]), CAP, investigateOne); deadIdx.forEach((i, k) => { firstPass[i] = re[k] }) }
const plans = firstPass.filter(Boolean)
const failed_investigate = RESIDUALS.filter((_, i) => !firstPass[i]).map((r) => r.id)
const clusters = clusterByFiles(plans)
log('Investigate: ' + plans.length + '/' + RESIDUALS.length + ' plans' + (failed_investigate.length ? ' — STILL FAILED after re-attempt (surfaced, not lost): ' + failed_investigate.join(',') : '') + ' -> ' + clusters.length + ' file-clusters')

phase('Implement')
const impl = (await pool(clusters, CAP, (cl) => agent(implementPrompt(cl), { label: 'implement:' + cl.map((p) => p.id).join('+').slice(0, 40), phase: 'Implement', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 }).then((r) => r ? { cluster: cl, log: r } : null))).filter(Boolean)
log('Implement: ' + impl.length + '/' + clusters.length + ' clusters applied')

phase('Verify')
const verify = (await pool(plans, CAP, (p) => agent(verifyPrompt(p), { label: 'verify:' + p.id, phase: 'Verify', schema: VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
const open = verify.filter((v) => v.status === 'open')
log('Verify: ' + (verify.length - open.length) + '/' + verify.length + ' resolved; ' + open.length + ' still open')

return {
  root: ROOT,
  residuals: RESIDUALS.length,
  clusters: clusters.length,
  resolved: verify.filter((v) => v.status === 'resolved').map((v) => v.id),
  open: open.map((v) => ({ id: v.id, remains: v.summary })),
  failed_investigate,
  open_questions: plans.filter((p) => p.open_question).map((p) => ({ id: p.id, q: p.open_question })),
}
