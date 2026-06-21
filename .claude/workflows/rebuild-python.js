export const meta = {
  name: 'rebuild-python',
  description: 'Ground-up rebuild of libs/python design pages to world-class py3.15 (ADT collapse, AOP, unified rails, full .api-stacking) with high-signal prose, comment hygiene, and an adversarial redteam. args = optional folder scope (e.g. "geometry"); empty/"ALL" = all of libs/python.',
  phases: [
    { title: 'Discover', detail: 'list every design page under the target (recursive .planning specs)' },
    { title: 'Rebuild', detail: 'per page: rebuild(max) -> critique(xhigh) -> adversarial redteam(max), pooled at CAP=14' },
  ],
}

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages'], properties: { pages: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [HARNESS] -- bounded worker pool: steady <=cap concurrent, no burst ----------------
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async () => { while (next < items.length) { const i = next++; out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}
const CAP = 14

// --- [INPUT] -- args = optional scope under the language root (folder name or sub-path; empty/"ALL" = whole root) ---
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const ROOT = 'libs/python'
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SCOPE = (!rawScope || rawScope === 'ALL') ? '' : rawScope
const SWEEP = !SCOPE ? ROOT : (SCOPE === ROOT || SCOPE.indexOf(ROOT + '/') === 0) ? SCOPE : ROOT + '/' + SCOPE
const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }
const subOf = (p) => p.split('/.planning/').pop()

const LAW = [
  'Rasm monorepo, libs/python planning corpus (markdown specs of intended Python module designs). CLAUDE.md manifest law governs. DENSITY BAR: docs/stacks/python/ (README/language/shapes/surfaces-and-dispatch/rails-and-effects/algorithms/type-system/boundaries/runtime/system-apis) — author Python as dense, polymorphic, and rich as that bar. Cite ONLY members confirmed in the folder .api catalogs (libs/python/<folder>/.api/*.md + branch libs/python/.api/*.md).',
  'This is a FUNDAMENTAL GROUND-UP REBUILD of a planning-stage DESIGN PAGE, not a polish pass. Stop anchoring to naive/surface/junior/old-style Python.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items (report those in residual_high). If after real investigation the file is already correct, return verdict=clean — never invent edits to look busy.',
].join('\n')
const ULTRA = [
  'ULTRA-ADVANCED REBUILD MANDATE: COLLAPSE parallel classes/types/enums/constants 30-50% via ADT (tagged unions, discriminated msgspec/pydantic models, value objects with behavior) and DATA-DRIVEN op/case tables — no one-class-per-variant spam. AOP: cross-cutting concerns (retry, telemetry, validation, caching, receipts, fault rails) as decorators/aspects, not repeated inline or sibling classes. UNIFIED rails + UNIFIED pipelines + feature-arms-as-cases (never loose separate). FP/ROP/Expressions; typed Result/Option rails; total match with exhaustive assert_never; NEVER exception control flow in domain logic. Parameterize inputs AND outputs (no stringy/weak typing). Algorithmic logic; ZERO fragile hardcoding/coupling.',
  'STACK .api CAPABILITY (load-bearing): FIRST open the folder .api/ catalogs and mine them — they document the full ADVANCED surface of each package (combinators, hooks, native pipelines, discriminators, async mirrors) and how packages STACK. Then compose 4-5 admitted libraries into single dense operations woven as ONE rail (e.g. msgspec dec_hook -> pydantic discriminated union -> stamina retry_context -> opentelemetry span), NOT flat one-shot per-library uses. Use the MOST powerful members; reject surface-level single-feature subsets.',
  'PRESERVE all capability (densify, never delete functionality). Where a page is already dense, refine; where it is flat/naive, rebuild ground-up. Never regress correctness or boundary law.',
].join('\n')
const PATLAW = [
  'PY-VERSION LAW: target Python 3.15 on the full modern band (3.11/3.12/3.13/3.14/3.15) — advanced patterns ONLY, zero legacy idioms, IDENTICAL conventions across every folder and package.',
  'NEVER write `from __future__ import annotations`. NEVER use legacy typing: use PEP 585 builtin generics (`list[T]`, `dict[K, V]`, `tuple[...]`, `set[T]`) NOT `typing.List/Dict/Tuple/Set`; PEP 604 unions (`X | None`, `A | B`) NOT `Optional`/`Union`; PEP 695 type parameters (`class C[T]:`, `def f[T](...)`, `type Alias[T] = ...`) NOT `TypeVar` + `Generic`. Use `Self`, `override`, `TypeIs`/`TypeGuard`, `assert_never`, `ReadOnly`, `TypedDict` + `NotRequired`/`Required`, `LiteralString`, `enum.StrEnum`/`IntEnum`, and `@dataclass(slots=True, frozen=True)` or `msgspec.Struct`/pydantic models where each best fits.',
  'Prefer total `match`/structural pattern matching over if-chains, walrus where it tightens, exhaustive `assert_never` on closed unions, and modern facilities (PEP 750 t-strings, PEP 749 deferred annotations) where relevant. Keep every one of these choices CONSISTENT across all folders so the corpus reads as one ultra-advanced codebase.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package/folder owner strictly in its lane; internal code uses canonical names and shapes with mapping only at the edge; do not trample a sibling owner while densifying; respect the dependency direction of the workspace strata.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. Name the exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no task/process/session/history/proof/review comments, no docstring bloat. Densify names and types so comments are rarely needed; cut every low-value comment.'

const authorPrompt = (page) => [LAW, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ground-up REBUILD of ' + page + ' to the ULTRA-ADVANCED bar. Read the page, its sibling pages (for cross-page unification), and the .api/ catalogs it cites (mine the full advanced surface and STACK them). Collapse shapes via ADT + data tables, STACK .api capability into unified rails, apply AOP/decorators, parameterize I/O, py3.15 modern-only patterns. Write high-signal prose with all code backticked. Fix-in-place (read-then-extend, preserve capability). Report what you collapsed (count before->after). Return the fix-log.'].join('\n')
const critiquePrompt = (page) => [LAW, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: CONSTRUCTIVE CRITIQUE + FIX IN PLACE of ' + page + '. Is this the densest, richest, most world-class form at the docs/stacks/python bar? Push further: deeper ADT collapse, MORE .api-stacking (are the most powerful members and multi-library rails actually used, or surface-level?), tighter unified rails, AOP where features repeat, richer parameterization. ENFORCE py3.15 modern-only patterns (PEP 585/604/695; no `from __future__ import annotations`; no `typing.List/Optional/Union`/`TypeVar`+`Generic`) and cross-package consistency. ENFORCE prose + comment hygiene. EDIT the page. Return fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans], claim} object for any CROSS-FILE item you cannot fix from this one file (NO severity; the reconcile phase fixes all of them).'].join('\n')
const redteamPrompt = (page) => [LAW, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE of ' + page + '. You are a HOSTILE principal reviewer whose explicit goal is to REJECT this design. Assume it is naive, under-powered, or wrong until the page proves otherwise — the burden of proof is on the design, never on you. Open the cited .api/ catalogs and sibling pages, then ATTACK relentlessly: phantom .api members; flat one-shot library use that must STACK the advanced surface; exception control flow in domain logic; non-total match or missing `assert_never`; `from __future__ import annotations` or legacy typing (`typing.List/Dict/Optional/Union`, `TypeVar`+`Generic`) where PEP 585/604/695 is required; untyped/stringy/weak carriers; residual class/type/enum/constant spam an ADT would collapse; fragile hardcoding/coupling; cross-package pattern drift; missing input AND output parameterization for complex multi-varied use; unhandled long-tail/edge cases; prose bloat and un-backticked code. For EVERY weakness, state the concrete failure (what breaks, under which input/output/edge case, and why) and THEN repair it in place — no soft-pedalling, no could/should. Reject "good enough"; force the objectively densest, most powerful, most correct form, and run COUNTERFACTUALS (is there a fundamentally stronger design than the one taken?). Hold the highest bar of any stage. EDIT to repair every defect you raise. Return fix-log + residual_high — each item a {files: [every repo-relative path the cross-file fix spans], claim} object for a CROSS-FILE item you cannot fix from one file (NO severity — every finding counts equally and the reconcile phase addresses all; every single-file fix you make yourself).'].join('\n')

// --- [STAGES] ----------------------------------------------------------------------------
const STAGES = [
  { key: 'rebuild', build: authorPrompt, effort: 'max' },
  { key: 'crit', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processPage = async (w, tag) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(w.page), { label: st.key + ':' + folderOf(w.page) + ':' + subOf(w.page), phase: tag + folderOf(w.page), schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: 300000 })
    if (r) logs[st.key] = r
  }
  return { page: w.page, logs }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Discover')
const inv = await agent('List every design page under ' + SWEEP + ' — markdown specs at paths matching */.planning/**/*.md. Return each as a repo-relative path (e.g. ' + ROOT + '/<folder>/.planning/<sub>/<page>.md). Exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md. Use find; do not cd.', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const pending = ((inv && inv.pages) || []).filter(Boolean).map((p) => ({ page: p }))
const total = pending.length
log('Discover under ' + SWEEP + ': ' + total + ' design pages; pooling at CAP=' + CAP)

phase('Rebuild')
const done = (await pool(pending, CAP, (w) => processPage(w, 'Rebuild-'))).filter(Boolean)

// --- [RECONCILE] -- consume the cross-FILE residuals the per-page pass deferred -----------
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: x.files && x.files.length ? x.files : [page], claim: x.claim }
const allRes = []
for (const r of done) for (const st of ['crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, r.page)) }
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Rebuild: ' + done.length + '/' + total + ' pages; reconcile ' + uniq.length + ' residuals (crit+redteam, deduped) -> ' + clusters.length + ' clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pipeline(
    clusters,
    (cl) => agent([LAW, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', 'TASK: RECONCILE these cross-FILE residuals the critique AND red-team passes deferred. There is NO severity — treat EVERY residual as must-address. Read EVERY listed file. For each: if it is a real cross-file defect, FIX it in place (unify the shared type/seam/rail, repair the strata/boundary issue), preserving all capability and regressing no file; if a residual is FACTUALLY INCORRECT or not a real defect, leave it and say why in the summary — never silently skip a real one to avoid work. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', BOUNDARIES, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named files from disk and classify each residual: status "fixed" (real defect, now genuinely resolved), "invalid" (the claim is factually wrong / not a real defect — cite why), or "open" (real defect still NOT resolved). Default to "open" on any doubt for a real-looking defect; mark "invalid" ONLY when you can show the claim is wrong. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_residual = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual), ' + dropped.length + ' dropped as invalid')
return { root: ROOT, scope: SCOPE || 'ALL', complete: done.length, total: total, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped }
