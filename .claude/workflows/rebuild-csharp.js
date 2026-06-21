export const meta = {
  name: 'rebuild-csharp',
  description: 'Ground-up rebuild of libs/csharp design pages to world-class modern C# (strata-correct, [Union]/SmartEnum/ValueObject ADT collapse, ROP/typed rails, AOP, source-generated owners) with FULL compliance to docs/stacks/csharp/ + the relevant domain/ shard, high-signal prose, comment hygiene, and an adversarial redteam. args = optional package scope (e.g. "Rasm.Bim"); empty/"ALL" = all of libs/csharp.',
  phases: [
    { title: 'Discover', detail: 'list every design page under the target (recursive .planning specs)' },
    { title: 'Rebuild', detail: 'per page: rebuild(max) -> critique(xhigh) -> adversarial redteam(max), pooled at CAP=14' },
  ],
}

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages'], properties: { pages: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [HARNESS] -- bounded worker pool: steady <=cap concurrent, no burst ----------------
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
const CAP = 14

// --- [INPUT] -- args = optional scope under the language root (package name or sub-path; empty/"ALL" = whole root) ---
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const ROOT = 'libs/csharp'
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SCOPE = (!rawScope || rawScope === 'ALL') ? '' : rawScope
const SWEEP = !SCOPE ? ROOT : (SCOPE === ROOT || SCOPE.indexOf(ROOT + '/') === 0) ? SCOPE : ROOT + '/' + SCOPE
const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }
const subOf = (p) => p.split('/.planning/').pop()

const LAW = [
  'Rasm monorepo, libs/csharp planning corpus (markdown specs of intended C# package designs). CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward; a host-neutral owner only where a non-Rhino runtime consumes the contract). C# PLANNING-HOMING: under `libs/csharp/Rasm` the active planning effort is `Rasm/Geometry` — its design pages live at `libs/csharp/Rasm/Geometry/.planning/**` while its governing `ARCHITECTURE.md`/`IDEAS.md`/`TASKLOG.md`/`README.md` and `.api/` catalogs live at the `libs/csharp/Rasm/` package ROOT (one level UP from `Geometry/`); read those Rasm-root docs + `.api/` as the governing context and shared capability tier for the Geometry pages, and never trample the mature siblings `Analysis`/`Domain`/`Vectors` (not planning targets).',
  'MANDATORY STANDARDS COMPLIANCE: every page MUST meet docs/stacks/csharp/ (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) AND the specialized docs/stacks/csharp/domain/ shard(s) relevant to the page concern (compute, concurrency, data-interchange, diagnostics, durability, interaction, persistence, postgres, resilience, runtime, transport, validation, visuals). READ the relevant shard(s) and conform exactly — this is a hard gate, not advisory. Cite only host/NuGet members confirmed via `uv run python -m tools.assay api`; back bridge claims with EvidenceCertificate + reviewed ReferenceEvidence.',
  'This is a FUNDAMENTAL GROUND-UP REBUILD of a planning-stage DESIGN PAGE, not a polish pass. Even where current C# quality is "good", push to the objectively strongest form the standards admit.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items (report those in residual_high). If after real investigation the file is already correct, return verdict=clean — never invent edits to look busy.',
].join('\n')
const ULTRA = [
  'ULTRA-ADVANCED REBUILD MANDATE: COLLAPSE >=3 parallel types / sibling factory methods / repeated switch arms / single-call private helpers into ONE polymorphic owner IN THE SAME FILE via `[Union]` discriminated unions, `SmartEnum<T>` with delegate behavior, readonly-struct/record `[ValueObject]`s, source-generated case families, fold algebra, and data tables — never extract a new file to reduce LOC, never delete capability. AOP: cross-cutting concerns (retry, telemetry, validation, caching, receipts, fault rails) as decorators/aspects.',
  'STACK CAPABILITY: FIRST mine the package `.api/*.md` catalogs (the curated, integration-shaped capability surface at `libs/csharp/<Package>/.api/`; for the `Rasm/Geometry` effort they live at `libs/csharp/Rasm/.api/`) AND the universal Thinktecture / LanguageExt rails — C# has NO central `.api/` tier, so the universals are Thinktecture/LanguageExt plus full docs/stacks/csharp doctrine. There is NO fixed package count: compose EVERY relevant host API + admitted NuGet package + catalog member into single dense owners woven as ONE rail (source-generated owners, fold algebra, data tables), ALWAYS layering the universal Thinktecture/LanguageExt rails onto the domain packages, NOT flat one-shot per-API uses. Capture host/platform surfaces into focused local owners; verify members with `uv run python -m tools.assay api` (host DLLs, NuGet). Maximize the catalogs + universal rails wherever relevant. Use the MOST powerful members; reject surface-level subsets.',
  'PRESERVE all capability (densify, never delete functionality). Where a page is already dense, refine; where it is flat/naive, rebuild ground-up. Never regress correctness or boundary/strata law.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely — NEVER weak/unbounded/erased types where the language can express the domain; NEVER exception control flow in domain logic (use typed error rails / Result / ROP and the route recovery patterns); NEVER imperative branching where a bounded vocabulary, dispatch table, generated switch, match, or fold owns the variation; NEVER mutable accumulation for domain transforms (use immutable folds, projections, collection combinators). Total switch with exhaustiveness; typed algorithm receipts (NEVER a generic IReceipt/ledger) when fields carry route/status/sampling/solver/spectral/mesh/extraction/benchmark/host evidence.',
  'Latest stable C#/.NET to the metal: primary constructors, collection expressions, `params` collections, list/relational/logical pattern matching, required members, file-scoped types, nullable reference types enforced, generic math / static abstract members where they fit. Treat analyzer diagnostics as architecture pressure (fix true positives, refine false positives, no ceremony suppressions). Apply the docs/stacks/csharp file-organization and section-order law (`[Union]`/`[SmartEnum]`/`[ValueObject]` and generated case families stay inside the declaring owner block).',
  'Keep conventions IDENTICAL across every package; place each package on its canonical stratum and depend strictly upward. One canonical semantic name per bounded concept; arity/filter/provider/modality live in request shape, case, or policy row, not parallel `Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` names.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package owner strictly in its lane and on its stratum; geometry/mesh/IFC meet at the wire with one owner per runtime; internal code uses canonical names and shapes with mapping only at the edge; do not trample a sibling owner while densifying; never introduce a downward dependency or leak a host type into a host-neutral owner.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, method, operator, package ID, path, command, flag, and literal value in backticks. Name the exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no task/process/session/history/proof/review comments, no XML-doc bloat. Densify names and types so comments are rarely needed; cut every low-value comment.'

const authorPrompt = (page) => [LAW, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ground-up REBUILD of ' + page + ' to the ULTRA-ADVANCED bar. Read the page, its sibling pages (for cross-page unification), the docs/stacks/csharp standards plus the relevant domain/ shard(s) for this page concern, the package `.api/*.md` catalogs it composes (MINE + STACK them; for `Rasm/Geometry` pages the catalogs + governing docs are at the `libs/csharp/Rasm/` root), and verify any novel host/NuGet member via `uv run python -m tools.assay api`. Collapse shapes via `[Union]`/`SmartEnum`/`[ValueObject]` + data tables, STACK capability into unified rails, apply AOP, parameterize I/O, latest modern C#. Write high-signal prose with all code backticked. Fix-in-place (read-then-extend, preserve capability). Report what you collapsed (count before->after). Return the fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans], claim} object for any CROSS-FILE item you surface but cannot fix from this one file (NO severity; the reconcile phase fixes all of them).'].join('\n')
const critiquePrompt = (page) => [LAW, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: CONSTRUCTIVE CRITIQUE + FIX IN PLACE of ' + page + '. Is this the densest, most standards-compliant, most world-class form? Push further: deeper `[Union]`/`SmartEnum`/`[ValueObject]` collapse, more capability-stacking (are the package `.api/` catalogs AND the universal Thinktecture/LanguageExt rails maximized, not a surface-level subset?), tighter ROP/typed rails, AOP where features repeat, richer parameterization, source-generated owners where switch arms repeat. VERIFY full compliance with docs/stacks/csharp/ AND the relevant domain/ shard (read it). ENFORCE strata correctness and cross-package consistency. ENFORCE prose + comment hygiene. EDIT the page. Return fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans], claim} object for any CROSS-FILE item you cannot fix from this one file (NO severity; the reconcile phase fixes all of them).'].join('\n')
const redteamPrompt = (page) => [LAW, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE of ' + page + '. You are a HOSTILE principal reviewer whose explicit goal is to REJECT this design — current C# being "good" is not a defense; demand the objectively strongest form. Assume the page is under-powered or non-compliant until it proves otherwise; the burden of proof is on the design. Read the docs/stacks/csharp standards, the relevant domain/ shard, and the host/NuGet surfaces (verify via `assay api`), then ATTACK relentlessly: strata violations (downward dependency, host-type leak into a host-neutral owner); drift from docs/stacks/csharp/ or the relevant domain/ shard; phantom host/NuGet members; surface-level capability use that ignores the package `.api/` catalogs or the universal Thinktecture/LanguageExt rails that should stack into the owner; exception control flow instead of ROP/typed rails; non-exhaustive switch or missing total-match arm; generic IReceipt where a typed receipt is required; weak/erased/nullable-oblivious types; >=3 parallel types / sibling factories / repeated switch arms NOT collapsed into a `[Union]`/`SmartEnum`/`[ValueObject]`/data-table; helper/utility files for single-caller indirection; boundary/wire violations; analyzer-bait; parallel `Get`/`GetMany`/`List`/`Search` names for one concept; prose bloat and un-backticked code; cross-package convention drift. For EVERY weakness, state the concrete failure (what breaks, under which input/output/edge case, and why) and THEN repair it in place — no soft-pedalling, no could/should. Reject "good enough"; force the densest, most powerful, most standards-exact form, and run COUNTERFACTUALS. Hold the highest bar of any stage. EDIT to repair every defect you raise. Return fix-log + residual_high — each item a {files: [every repo-relative path the cross-file fix spans], claim} object for a CROSS-FILE item you cannot fix from one file (NO severity — every finding counts equally and the reconcile phase addresses all; every single-file fix you make yourself).'].join('\n')

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
    if (r === null) break
    logs[st.key] = r
  }
  return { page: w.page, logs, ok: Object.keys(logs).length === STAGES.length }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Discover')
const inv = await agent('List every design page under ' + SWEEP + ' — markdown specs at paths matching */.planning/**/*.md. Return each as a repo-relative path (e.g. ' + ROOT + '/<Package>/.planning/<sub>/<page>.md). Exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md. Use find; do not cd.', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
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
for (const r of done) for (const st of ['rebuild', 'crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, r.page)) }
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
return { root: ROOT, scope: SCOPE || 'ALL', complete: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: total, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped }
