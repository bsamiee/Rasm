export const meta = {
  name: 'survey-packages',
  description: 'Autonomous package-modernization survey-and-execute over one target .planning folder: inventory the package surface, research modern / redundant / replaceable packages (license-gated, consumer-TFM-verified), adversarially gate every change, then apply manifest + .api + README + code refactors across the whole-repo blast radius and self-heal the build. args = target folder path (e.g. libs/csharp/Rasm.Persistence); empty = no-op.',
  whenToUse: 'Modernize, replace, or prune the package set a planning folder depends on, end to end.',
  phases: [
    { title: 'Inventory', detail: 'map the folder package surface, domain, and hand-roll suspicions' },
    { title: 'Survey', detail: 'per package: research modern / redundant / replace verdict, license-gated, consumer-TFM-verified, pooled' },
    { title: 'Decide', detail: 'reconcile all verdicts, resolve the redundancy graph, anti-bloat + license gate, emit the change-set' },
    { title: 'Verify-Proposals', detail: 'critique then redteam each change adversarially before any write' },
    { title: 'Execute', detail: 'apply manifest + .api + README + code refactor across the whole-repo blast radius, serial' },
    { title: 'Heal', detail: 'run the assay build/static gate and self-heal until green' },
  ],
}

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
const CAP = 10
const MAX_HEAL = 2

// --- [INPUT] -- args = target .planning folder; empty = destructive no-op ----------------
const TARGET = typeof args === 'string' ? args.trim() : (args && typeof args === 'object' && args.target ? String(args.target).trim() : '')
if (!TARGET) {
  log('survey-packages: no target folder given. This workflow makes destructive cross-repo changes, so it refuses to run with no scope. Pass args = a planning folder path, e.g. libs/csharp/Rasm.Persistence.')
  return { skipped: true, reason: 'no target folder' }
}
const norm = TARGET.replace(/\/+$/, '').replace(/\/\.planning$/, '')
const lang = norm.startsWith('libs/csharp') ? 'csharp' : norm.startsWith('libs/python') ? 'python' : norm.startsWith('libs/typescript') ? 'typescript' : 'unknown'
const isGeometry = norm === 'libs/csharp/Rasm/Geometry' || norm.startsWith('libs/csharp/Rasm/Geometry/')
const docHome = isGeometry ? 'libs/csharp/Rasm' : norm
const planningHome = isGeometry ? 'libs/csharp/Rasm/Geometry/.planning' : norm + '/.planning'
const manifests = lang === 'csharp' ? 'Directory.Packages.props (central pins) + Directory.Build.props (TargetFramework / build floor)' : lang === 'python' ? 'pyproject.toml (dependencies / dependency-groups)' : lang === 'typescript' ? 'pnpm-workspace.yaml (catalog)' : '(unknown manifest)'

// --- [SCHEMAS] ---------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['language', 'packages'], properties: { language: { type: 'string' }, domain: { type: 'string' }, packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name'], properties: { name: { type: 'string' }, version: { type: 'string' }, role: { type: 'string' }, apiFile: { type: 'string' } } } }, suspicions: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, evidence: { type: 'string' } } } } } }
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['name', 'verdict'], properties: { name: { type: 'string' }, verdict: { type: 'string', enum: ['keep', 'replace', 'remove', 'add'] }, target: { type: 'string' }, reason: { type: 'string' }, license: { type: 'string' }, licenseOk: { type: 'boolean' }, maintenance: { type: 'string' }, feasible: { type: 'boolean' }, evidence: { type: 'string' } } }
const DECISION_SCHEMA = { type: 'object', additionalProperties: false, required: ['changes'], properties: { changes: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['id', 'kind', 'package'], properties: { id: { type: 'string' }, kind: { type: 'string', enum: ['replace', 'remove', 'add'] }, package: { type: 'string' }, target: { type: 'string' }, rationale: { type: 'string' } } } }, rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: { package: { type: 'string' }, why: { type: 'string' } } } } } }
const VERDICT_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'approved'], properties: { id: { type: 'string' }, approved: { type: 'boolean' }, severity: { type: 'string' }, findings: { type: 'string' } } }
const EXEC_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'verdict'], properties: { id: { type: 'string' }, verdict: { type: 'string', enum: ['applied', 'skipped'] }, files: { type: 'array', items: { type: 'string' } }, projects: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const BUILD_SCHEMA = { type: 'object', additionalProperties: false, required: ['green'], properties: { green: { type: 'boolean' }, diagnostics: { type: 'string' }, projects: { type: 'array', items: { type: 'string' } } } }
const HEAL_SCHEMA = { type: 'object', additionalProperties: false, required: ['fixed'], properties: { fixed: { type: 'boolean' }, summary: { type: 'string' } } }

// --- [LAW] -- the declarative law every agent in this run obeys --------------------------
const LAW = [
  'Rasm monorepo, autonomous package-modernization run. The target planning folder is ' + norm + ' (language=' + lang + '). Its README and .api catalogs home to ' + docHome + (isGeometry ? ' (GEOMETRY SPECIAL-CASE: README / TASKLOG / IDEAS / .api live at the libs/csharp/Rasm ROOT, while the design pages live under ' + planningHome + ' — never trample the mature siblings Analysis / Domain / Vectors)' : '') + '; design pages live under ' + planningHome + '. Central package pins live in ' + manifests + ' — never a per-package manifest.',
  'ASSAY OWNER-ROUTING + CONSUMER-TFM HARDENING (a verified lesson, non-negotiable): external members are verified via `uv run --frozen python -m tools.assay api` (decompile/reflection over NuGet DLLs / installed Python distributions / node_modules). assay api default resolution selects ONE primary_assembly per key and CAN pick a target framework the build does NOT bind, whose public surface differs from the consumed asset (observed: RectpackSharp resolved netstandard2.0 Pack(PackingRectangle[]) while the net10 consumer binds net5.0 Pack(Span<PackingRectangle>) — a different signature). For C#, the consumer floor is net10.0 (Directory.Build.props TargetFramework): decompile the lib/<tfm> the net10 consumer actually binds (highest compatible lib folder), explicitly, and diff against assay default before trusting any member claim — set DOTNET_ROOT (DOTNET_ROOT=$(dirname "$(readlink -f "$(command -v dotnet)")")) and run ilspycmd <pkg>/lib/<consumer-tfm>/<asm>.dll -t <FQN>. For Python/TS, verify against the actually-installed distribution / declared .d.ts. Never document or compare a phantom from a non-bound TFM.',
  'LICENSE GATE: a package is admissible only if it is OSS (any OSI-approved license) OR carries a commercial license that is NOT fee-based and NOT tiered/seat/usage-gated (a free, unrestricted commercial grant). Any fee, subscription, seat cap, eval-only, or usage-tiered license is REJECTED. State the exact license (SPDX or the grant) and the OK/REJECT verdict for every add/replace target.',
  'REDUNDANCY + ANTI-BLOAT DISCIPLINE: only REMOVE a package when another ALREADY-ADMITTED package provably covers EVERY call site it serves — cite the subsuming package and the call sites. Only ADD a package when it closes a real hand-rolled capability or a genuine gap AND no admitted package already covers it; never add for its own sake, and never add a thin wrapper over an admitted surface. A replacement must be strictly more modern / more capable / better maintained than the incumbent, not a lateral move. When two packages each could cover the other, keep ONE (resolve the redundancy graph), never remove both.',
  'CROSS-REPO RIPPLE MANDATE: a package is shared. Every add / remove / replace must touch EVERY point repo-wide: the central manifest row, EVERY planning folder README that lists it, EVERY .api/*.md catalog for it, and EVERY code call site across ALL folders (not only the target folder). Survey the full blast radius first; fix every touch point in the same change. A removed package leaves no README mention, no .api file, no manifest pin, and no code reference; an added package is present in all of those plus a new .api catalog authored to house format.',
  'HOUSE .api FORMAT: header (package / version / license / build-floor or target / marker), then member sections grouped by concern with backticked symbols + signatures + a consumer/boundary note. NO provenance, NO process narration, NO freshness tails. Cite REAL members only, consumer-TFM-correct. Code obeys the route-owned standard for the file (docs/stacks/csharp for .cs, coding-python for .py, coding-ts for .ts).',
  'WRITE-FULLY MANDATE (execute stage only): every edit is made NOW via Edit/Write across all touch points; the fix-log REPORTS edits already made, never a to-do list. The survey / decide / verify stages are READ-ONLY analysis and WRITE NOTHING.',
].join('\n')

// --- [PROMPTS] ---------------------------------------------------------------------------
const invPrompt = [
  LAW, '',
  'TASK (READ-ONLY INVENTORY): map the complete package surface of the target folder ' + norm + '. Read its README at ' + docHome + ', its project file (the .csproj under ' + docHome + ' if C#), every catalog under ' + docHome + '/.api/, and the central manifest ' + manifests + '. Enumerate EVERY package the folder uses (name + current pinned version + its role in this folder + the matching .api filename if one exists). Then read the design pages under ' + planningHome + ' and the folder code to surface HAND-ROLL SUSPICIONS: capabilities the folder implements by hand that an ecosystem package likely owns (each with a short evidence pointer). Summarize the folder DOMAIN in one or two sentences. Return the structured inventory. Write nothing.',
].join('\n')

const surveyPrompt = (w) => [
  LAW, '',
  w.kind === 'gap'
    ? 'TASK (READ-ONLY SURVEY of a HAND-ROLL SUSPICION): the target folder may be hand-rolling: ' + w.capability + ' (evidence: ' + (w.evidence || 'n/a') + '). Research whether a modern, well-maintained, license-admissible ecosystem package owns this concern and is not already covered by an admitted package. Use web research (latest stable version, maintenance signal, license) and, when an existing admitted package is the real owner, prove it via assay (consumer-TFM-correct). Return verdict=add with the target package ONLY if it closes a real gap and passes the license gate and is not redundant; else verdict=keep with the reason it stays hand-rolled or is already covered.'
    : 'TASK (READ-ONLY SURVEY of one package): assess ' + w.name + ' (role: ' + (w.role || 'n/a') + ', current: ' + (w.version || 'n/a') + ') for the target folder. Determine: is there a newer / more capable / better-maintained replacement (license-admissible)? Is it stale or poorly maintained? Is it REDUNDANT because another admitted package already covers every call site (cite them)? Use web research (latest stable, release recency, maintenance, license) AND assay decompile (CONSUMER-TFM-CORRECT — for C# decompile the net10-bound lib/<tfm>, not assay default; diff a multi-target package explicitly) to ground every member claim. Return one verdict: keep | replace (with target + why strictly better) | remove (with the subsuming admitted package + the call sites it covers) | add is not used here. Apply the license gate to any replacement target. Default to keep unless the evidence is decisive.',
  'Item id: ' + w.name + '. Return the single structured verdict (set name to the id). Write nothing.',
].join('\n')

const decidePrompt = (verdicts) => [
  LAW, '',
  'TASK (READ-ONLY DECISION — the anti-bloat barrier): you hold EVERY survey verdict for the target folder. Reconcile them into a final, globally-consistent change-set. (1) Drop every verdict that fails the LICENSE gate. (2) Resolve the REDUNDANCY GRAPH: if two packages each could subsume the other, keep exactly one; never remove both; never remove a package whose call sites the proposed subsumer cannot actually cover. (3) Apply the ANTI-BLOAT gate: reject any add that does not close a real gap or that duplicates an admitted surface, and reject any replace that is a lateral move rather than a strict upgrade. (4) Dedup conflicting proposals (two verdicts touching the same package). Emit the ordered change-set (each a replace | remove | add with a stable id, the package, the target for replace/add, and the decisive rationale) plus the rejected list with the reason each was dropped. Be conservative: a change ships only if its evidence is decisive. Write nothing.',
  'VERDICTS:\n' + JSON.stringify(verdicts, null, 1),
].join('\n')

const critiquePrompt = (c) => [
  LAW, '',
  'TASK (CONSTRUCTIVE CRITIQUE of one proposed change — READ-ONLY GATE, no writes): the change is ' + JSON.stringify(c) + '. Independently verify it is correct and complete BEFORE any destructive write. Confirm: the replacement/new target is real, license-admissible, and strictly better (or, for remove, the subsuming admitted package genuinely covers every call site); the member surface is consumer-TFM-correct (re-verify via assay / ilspycmd on the net10-bound TFM for C#); and the whole-repo blast radius is fully mapped (every manifest row, README, .api, and code call site across ALL folders). Set approved=true only if the change is sound and the touch-point map is complete; otherwise approved=false with the gap. Default to approved=false on any doubt.',
].join('\n')

const redteamPrompt = (c, crit) => [
  LAW, '',
  'TASK (HOSTILE RED-TEAM of one proposed change — READ-ONLY GATE, no writes): assume the change is WRONG until proven. Change: ' + JSON.stringify(c) + '. Prior critique: ' + JSON.stringify(crit) + '. Attack it: is the replacement actually a downgrade or a lateral move? Is the license secretly fee-based or tiered? Does the "redundant" package actually lose a capability at some call site (find the call site that breaks)? Is any cited member a phantom from a non-bound TFM (re-decompile the consumer TFM explicitly and diff)? Is the blast-radius map missing a touch point that would leave a dangling reference? Set approved=true ONLY if the change survives every attack; default approved=false on any unresolved doubt. This is the last gate before destructive execution.',
].join('\n')

const executePrompt = (c) => [
  LAW, '',
  'TASK (EXECUTE one approved change — WRITE-FULLY, across the whole-repo blast radius): apply ' + JSON.stringify(c) + ' in full, NOW, via Edit/Write. Touch EVERY point: (a) the central manifest ' + manifests + ' — add / remove / replace the pin (and any build-floor change); (b) EVERY planning folder README that mentions the package; (c) EVERY .api/*.md catalog — DELETE the removed package catalog, AUTHOR a new catalog to house format for an added/replacement package (consumer-TFM-correct members verified via assay), UPDATE existing ones; (d) refactor EVERY code call site across ALL folders to the new surface (or to the subsuming package for a removal), obeying the route-owned coding standard. Do not leave a dangling reference anywhere. Return the fix-log: verdict (applied | skipped), the files you edited, the projects/areas affected (so the build gate can target them), and a one-line summary. If on inspection the change is unsafe, return verdict=skipped with the reason rather than a partial edit.',
].join('\n')

const verifyPrompt = (c, ex, attempt) => [
  LAW, '',
  'TASK (BUILD/STATIC GATE — verify one applied change): change ' + c.id + ' touched these projects/areas: ' + JSON.stringify((ex && ex.projects) || []) + '. Run the assay build/static gate over them via `uv run --frozen python -m tools.assay static` (for C# pass --project <csproj> or --folder; for Python/TS pass --folder <path>), parse the one JSON Envelope on stdout, and report green=true only if the gate is clean (no FAILED diagnostics). On red, return green=false with the concrete diagnostics (file + rule + message) so the heal step can act. Attempt index: ' + attempt + '.',
].join('\n')

const healPrompt = (c, ex, v) => [
  LAW, '',
  'TASK (SELF-HEAL — fix the build the last applied change broke, WRITE-FULLY): change ' + c.id + ' left the build red. Diagnostics: ' + ((v && v.diagnostics) || 'see prior gate') + '. Files touched: ' + JSON.stringify((ex && ex.files) || []) + '. Fix the real defects in place (a missed call-site refactor, a wrong member from a non-bound TFM, a stale README/.api reference, a manifest floor) so the gate goes green, obeying the route-owned coding standard and never re-introducing the removed package. Return fixed=true with a one-line summary of what you corrected.',
].join('\n')

const finalVerifyPrompt = (applied) => [
  LAW, '',
  'TASK (FINAL WHOLE-TARGET BUILD GATE): every approved change for ' + norm + ' is applied. Applied summary: ' + JSON.stringify(applied) + '. Run the assay build/static gate over the union of affected projects/areas (for C# the affected .csproj set; for Python/TS the affected folders) via `uv run --frozen python -m tools.assay static ...`, parse the JSON Envelope, and report green=true only if the whole affected set is clean. On red, return the residual diagnostics.',
].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Inventory')
const inv = await agent(invPrompt, { label: 'inventory:' + norm.split('/').pop(), phase: 'Inventory', schema: INVENTORY_SCHEMA, effort: 'max', stallMs: 300000 })
const packages = ((inv && inv.packages) || []).filter((p) => p && p.name)
const suspicions = ((inv && inv.suspicions) || []).filter(Boolean)
const worklist = [
  ...packages.map((p) => ({ kind: 'pkg', name: p.name, role: p.role || '', version: p.version || '' })),
  ...suspicions.map((s, i) => ({ kind: 'gap', name: 'gap-' + i + '-' + String(s.capability || '').slice(0, 40), capability: s.capability, evidence: s.evidence || '' })),
]
log('inventory under ' + norm + ': ' + packages.length + ' packages, ' + suspicions.length + ' hand-roll suspicions; surveying ' + worklist.length + ' items at CAP=' + CAP)
if (!worklist.length) return { target: norm, language: lang, skipped: true, reason: 'inventory found no packages or suspicions' }

phase('Survey')
const verdicts = (await pool(worklist, CAP, (w) => agent(surveyPrompt(w), { label: 'survey:' + w.name, phase: 'Survey', schema: SURVEY_SCHEMA, effort: 'high', stallMs: 300000 }))).filter(Boolean)

phase('Decide')
const decision = await agent(decidePrompt(verdicts), { label: 'decide', phase: 'Decide', schema: DECISION_SCHEMA, effort: 'max', stallMs: 300000 })
const changes = ((decision && decision.changes) || []).filter((c) => c && c.id)
const rejected = (decision && decision.rejected) || []
log('decide: ' + verdicts.length + ' verdicts -> ' + changes.length + ' candidate changes, ' + rejected.length + ' rejected')
if (!changes.length) return { target: norm, language: lang, surveyed: worklist.length, candidates: 0, applied: 0, rejected: rejected.length, note: 'no modernization change warranted' }

phase('Verify-Proposals')
const reviewed = (await pipeline(
  changes,
  (c) => agent(critiquePrompt(c), { label: 'critique:' + c.id, phase: 'Verify-Proposals', schema: VERDICT_SCHEMA, effort: 'high', stallMs: 300000 }),
  (crit, c) => agent(redteamPrompt(c, crit), { label: 'redteam:' + c.id, phase: 'Verify-Proposals', schema: VERDICT_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((rt) => ({ change: c, critique: crit, redteam: rt })),
)).filter(Boolean)
const approved = reviewed.filter((r) => r.critique && r.critique.approved && r.redteam && r.redteam.approved).map((r) => r.change)
log('verify-proposals: ' + approved.length + '/' + changes.length + ' changes survived critique+redteam')
if (!approved.length) return { target: norm, language: lang, surveyed: worklist.length, candidates: changes.length, approved: 0, applied: 0, note: 'no change survived adversarial verification' }

// --- [EXECUTE] -- SERIAL: shared central manifest + cross-file refactors must not race ----
phase('Execute')
const applied = []
for (let i = 0; i < approved.length; i++) {
  const c = approved[i]
  const ex = await agent(executePrompt(c), { label: 'exec:' + c.id, phase: 'Execute', schema: EXEC_SCHEMA, effort: 'max', stallMs: 420000 })
  if (!ex || ex.verdict !== 'applied') { applied.push({ id: c.id, applied: false, reason: (ex && ex.summary) || 'skipped' }); continue }
  let green = false
  for (let h = 0; h <= MAX_HEAL; h++) {
    const v = await agent(verifyPrompt(c, ex, h), { label: 'verify:' + c.id + ':' + h, phase: 'Heal', schema: BUILD_SCHEMA, effort: 'low', stallMs: 420000 })
    if (v && v.green) { green = true; break }
    if (h === MAX_HEAL) break
    await agent(healPrompt(c, ex, v), { label: 'heal:' + c.id + ':' + h, phase: 'Heal', schema: HEAL_SCHEMA, effort: 'max', stallMs: 420000 })
  }
  applied.push({ id: c.id, applied: true, green, files: ex.files || [], projects: ex.projects || [] })
}

phase('Heal')
const finalV = await agent(finalVerifyPrompt(applied), { label: 'verify:final', phase: 'Heal', schema: BUILD_SCHEMA, effort: 'low', stallMs: 420000 })
const appliedCount = applied.filter((a) => a.applied).length
log('execute: ' + appliedCount + '/' + approved.length + ' changes applied; final build green=' + (finalV && finalV.green))

return { target: norm, language: lang, docHome, planningHome, surveyed: worklist.length, candidates: changes.length, approved: approved.length, applied: appliedCount, finalGreen: finalV && finalV.green, rejected: rejected.length, changes: applied, residualDiagnostics: (finalV && !finalV.green) ? finalV.diagnostics : '' }
