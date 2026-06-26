export const meta = {
  name: 'resolve-residuals',
  whenToUse: 'Resolve cross-file hard residuals a rebuild run could not close, with research and adversarial verification.',
  description: 'Manual cross-file HARD-RESIDUAL resolver, language-agnostic: investigate/research each open reconcile residual a rebuild-* run could not close (read every spanned page + the right .api tier + assay api for member claims + domain research), decide the canonical resolution, implement the design-doc changes across ALL spanned pages at the owning language bar (cs/py/ts), then adversarially verify. Hostile stance: assume the residual is real and the prior code naive/illusory; resolve deferred decisions with research rather than punting; realization-gate items are signature-locked in the owning design page (never realized as source). args = the hard residuals to resolve — an array, or {residuals:[...]}, or a rebuild-* run\'s {hard_residual:[...]} return; each item a {files,claim} object, a {id,claim,hint} object, or a bare claim string. Empty = no-op.',
  phases: [
    { title: 'Investigate', detail: 'per residual: read spanned pages + the right .api tier + assay/domain research -> a precise resolution plan (read-only)' },
    { title: 'Implement', detail: 'per file-cluster: apply every resolution across the spanned design pages, fix-in-place at the owning language bar' },
    { title: 'Verify', detail: 'per residual: adversarially confirm it is implemented, correct, and consistent across pages' },
  ],
}

const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'decision', 'files'], properties: { id: { type: 'string' }, claim: { type: 'string' }, decision: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, api_members: { type: 'array', items: { type: 'string' } }, edits: { type: 'array', items: { type: 'string' } }, open_question: { type: 'string' } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['resolved', 'partial', 'clean'] }, applied: { type: 'array', items: { type: 'string' } }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'status', 'summary'], properties: { id: { type: 'string' }, status: { type: 'string', enum: ['resolved', 'open'] }, evidence: { type: 'string' }, summary: { type: 'string' } } }

// --- [HARNESS] -- ramped bounded worker pool ---------------------------------------------
const STAGGER_MS = 1500
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
const clusterByFiles = (plans) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const p of plans) { (p.files || []).forEach(add); for (let i = 1; i < (p.files || []).length; i++) parent.set(find(p.files[i]), find(p.files[0])) }
  const by = new Map()
  for (const p of plans) { const root = (p.files && p.files.length) ? find(p.files[0]) : p.id; (by.get(root) || by.set(root, []).get(root)).push(p) }
  return [...by.values()]
}
const CAP = 10

// --- [INPUT] -- args = an array / {residuals:[...]} / a rebuild {hard_residual:[...]}; items are {files,claim} | {id,claim,hint} | string ---
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const RAW = Array.isArray(input) ? input : (input && Array.isArray(input.residuals)) ? input.residuals : (input && Array.isArray(input.hard_residual)) ? input.hard_residual : []
const toResidual = (x, i) => typeof x === 'string' ? { id: 'R' + (i + 1), claim: x, files: [], hint: '' } : { id: x.id || ('R' + (i + 1)), claim: x.claim || '', files: Array.isArray(x.files) ? x.files : [], hint: x.hint || '' }
const RESIDUALS = RAW.map(toResidual).filter((r) => r.claim)

// --- [DOCTRINE] -- language-routed design-page bar + hostile residual-resolution mandate ---
const DESIGN_BAR = [
  'Rasm monorepo planning corpus (markdown specs; code fences are the product, no source yet). CLAUDE.md manifest + WORKSPACE_LAW strata govern. LANGUAGE ROUTING — hold every edit to the OWNING language bar of the file you edit, by its root: `libs/csharp/**` -> docs/stacks/csharp/ + the relevant domain/ shard ([Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject] ADT collapse, LanguageExt `Fin`/`Validation`/`Option`/`Eff` rails, two-weave AOP, total generated `Switch`, the `tools/cs-analyzer` doctrine gate); `libs/python/**` -> docs/stacks/python/ (py3.15 PEP 585/604/695, `frozendict` builtin, `TypedDict`/Pydantic payloads, expression `Result`/`Option`, `anyio`, `stamina`, closed-fault `@tagged_union`); `libs/typescript/**` -> docs/stacks/typescript/ + coding-ts (Effect-TS rails, `Schema`-first boundaries, branded/nominal types, exhaustive discriminated unions, zero `any`/`throw`/`enum`). Read the operative pages for the file language before editing; every fence holds that bar as fact.',
  'STACK .api CAPABILITY by the file language: C# uses the per-package `<pkg>/.api/*.md` catalogs + the universal Thinktecture/LanguageExt rails (no central tier; Geometry catalogs live at `libs/csharp/Rasm/.api/`); Python mines BOTH tiers — the shared `libs/python/.api/*.md` AND the folder `libs/python/<folder>/.api/*.md`; TypeScript mines the shared `libs/typescript/.api/*.md` Effect/Schema/React tier AND the area `libs/typescript/<area>/.api/*.md`. Cite ONLY members confirmed in those catalogs; maximize the shared/universal rails, never only the folder set.',
  'PROSE + COMMENTS: high-signal design-spec prose per docs/standards/style-guide.md — lead with the controlling contract, one idea per paragraph, no hedges/provenance/process narration; BACKTICK every symbol/type/member/path. Keep canonical `# --- [LABEL]` section dividers; comment only where intent is not obvious; zero low-value comments. Prose that ASSERTS capability the fence does not implement is a defect.',
  'BOUNDARY LAW: keep every package/folder owner in its lane and on its stratum; internal code uses canonical names with mapping only at the edge; respect the workspace strata (depend strictly upward; no host-type leak into a host-neutral owner); do not trample a sibling owner while resolving a residual.',
].join('\n')
const ADVERSARIAL = 'HOSTILE STANCE: assume the residual is REAL and the spanned code is naive, junior, or ILLUSORY until proven otherwise — the burden of proof is on the code, never on you. "Already fine"/"mature"/"good enough" is rejected; treat dense confident-looking code as a prime suspect for hollow/decorative complexity, and disbelieve every claim the page makes about itself until verified against the real domain + the catalogued package surface. Resolve the residual PROPERLY and FULLY, never punt it.'
const RESIDUAL_LAW = [
  'YOU RESOLVE CROSS-FILE HARD RESIDUALS: each residual is a real, open, cross-FILE defect or deferred decision the per-page rebuild + in-run reconcile could not close. Resolve it PROPERLY and FULLY — add the functionality/feature/axis, unify the ambiguous contract, lock the signature, close the capability gap — across EVERY design page it spans, leaving the corpus internally consistent.',
  'DEFERRED DECISIONS ARE RESOLVED, NOT PUNTED: where a residual is a design decision (a new policy axis, a consumer wiring, a canonical contract, a capability extension), make the STRONGEST principled choice the domain + standards admit (research it; choose the most polymorphic, parameterized, future-ready form) and IMPLEMENT it. Only leave an `open_question` when the choice genuinely requires an operator product decision you cannot ground.',
  'REALIZATION-GATE ITEMS STAY IN THE DESIGN PAGE: where a residual notes an owner is "not yet realized as source", the resolution is to SIGNATURE-LOCK the exact contract on the owning DESIGN PAGE so consumers cite it consistently — NEVER create `.cs`/`.py`/`.ts` source (that is the deferred implement pass).',
  'RESEARCH MANDATE — no guessing: where a claim hinges on a real package/host member, VERIFY it via `uv run python -m tools.assay api` over the right artifact band (host DLLs / NuGet / Python distributions / node_modules; READ tools/assay/README.md FIRST for the api-arm contract and the correct invocation, and for a gated/companion Python band the correct interpreter). Where a claim catalogs missing members, WRITE them into the owning `.api/*.md` catalog — resolutions MAY edit `.api` catalog files as well as design pages. Where a claim hinges on a domain/math contract, research the real convention before deciding. Cite only verified members; a member you cannot verify is a phantom — do not assert it.',
  'WRITE-FULLY: every edit you identify you MUST make NOW via Edit/Write; the fix-log REPORTS edits already made. Leave behind only a genuine still-cross-file item (residual) or a true operator decision (open_question).',
].join('\n')

const filesLine = (r) => (r.files && r.files.length) ? '\nSPANNED FILES (read every one): ' + JSON.stringify(r.files) : ''
const investigatePrompt = (r) => [DESIGN_BAR, '', ADVERSARIAL, '', RESIDUAL_LAW, '', 'TASK (INVESTIGATE — read-only, produce the resolution PLAN, edit NOTHING): residual `' + r.id + '`.\nCLAIM: ' + r.claim + filesLine(r) + (r.hint ? '\nWHERE TO LOOK: ' + r.hint : '') + '\nRead EVERY design page the claim spans + its siblings + the relevant `.api` catalogs (the right tier for the file language). Where the claim hinges on a real package/host member, VERIFY via `uv run python -m tools.assay api` (read tools/assay/README.md first). Where it hinges on a domain/math contract, research the real convention. Then DECIDE the canonical resolution (the strongest principled design the owning language bar admits; resolve deferred decisions, do not punt). Return the PLAN: `id`, the `decision` (what the canonical resolution IS), the exact repo-relative `files` to edit, the precise `edits` per file, the verified `.api` `api_members` to cite, and an `open_question` ONLY if it needs a true operator product decision.'].join('\n')
const implementPrompt = (cluster) => {
  const files = [...new Set(cluster.flatMap((p) => p.files || []))]
  return [DESIGN_BAR, '', ADVERSARIAL, '', RESIDUAL_LAW, '', 'TASK (IMPLEMENT — fix-in-place across these design pages at the owning language bar): apply EVERY resolution plan below to the spanned pages, preserving all capability and regressing no page. Unify each cross-file contract/seam CONSISTENTLY across all pages it touches; cite only verified `.api` members; signature-lock (never create source). Files in this cluster: ' + JSON.stringify(files) + '\nPLANS:\n' + JSON.stringify(cluster, null, 1) + '\nReturn the fix-log (files edited + verdict + applied), with `residual` only for a genuine still-cross-file item you could not close from these files.'].join('\n')
}
const verifyPrompt = (p) => [DESIGN_BAR, '', ADVERSARIAL, '', 'TASK (ADVERSARIAL VERIFY): residual `' + p.id + '`. Read the spanned design pages from disk and prove whether the resolution is ACTUALLY done: implemented (not hedged or merely described), correct (math/domain/`.api`-grounded — re-check any cited member against the catalog/`assay api`), and CONSISTENT across every page it spans. Status `resolved` ONLY if genuinely complete and correct; else `open` with exactly what remains. Default to `open` on doubt.\nCLAIM: ' + (p.claim || '') + '\nINTENDED RESOLUTION: ' + (p.decision || '') + '\nFILES: ' + JSON.stringify(p.files || [])].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
if (!RESIDUALS.length) { log('resolve-residuals: no residuals passed (args = array / {residuals:[...]} / a rebuild {hard_residual:[...]}). No-op.'); return { residuals: 0, resolved: [], open: [], failed_investigate: [], open_questions: [] } }

phase('Investigate')
log('Investigating ' + RESIDUALS.length + ' hard residuals')
const investigateOne = (r) => agent(investigatePrompt(r), { label: 'investigate:' + r.id, phase: 'Investigate', schema: PLAN_SCHEMA, effort: 'max', stallMs: 300000 }).then((p) => p ? { ...p, id: p.id || r.id, claim: p.claim || r.claim, files: (p.files && p.files.length) ? p.files : r.files } : null)
const firstPass = await pool(RESIDUALS, CAP, investigateOne)
// ONE bounded re-attempt for any residual whose investigate returned no plan: a transient agent death must never silently lose a residual.
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
  residuals: RESIDUALS.length,
  clusters: clusters.length,
  resolved: verify.filter((v) => v.status === 'resolved').map((v) => v.id),
  open: open.map((v) => ({ id: v.id, remains: v.summary })),
  failed_investigate,
  open_questions: plans.filter((p) => p.open_question).map((p) => ({ id: p.id, q: p.open_question })),
}
