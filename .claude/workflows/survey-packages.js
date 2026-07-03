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

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const MAX_HEAL = 2
const STAGGER_MS = 1500

// --- [INPUTS] ----------------------------------------------------------------------------
const TARGET = typeof args === 'string' ? args.trim() : (args && typeof args === 'object' && args.target ? String(args.target).trim() : '')
const norm = TARGET.replace(/\/+$/, '').replace(/\/\.planning$/, '')
const lang = norm.startsWith('libs/csharp') ? 'csharp' : norm.startsWith('libs/python') ? 'python' : norm.startsWith('libs/typescript') ? 'typescript' : 'unknown'
const docHome = norm
const planningHome = norm + '/.planning'
const langRoot = lang === 'unknown' ? norm : 'libs/' + lang
const apiTiers = docHome + '/.api (folder tier) + ' + langRoot + '/.api (language substrate tier)'
const manifests = lang === 'csharp' ? 'Directory.Packages.props (central pins) + Directory.Build.props (TargetFramework / build floor)' : lang === 'python' ? 'pyproject.toml ' +
  '(dependencies / dependency-groups)' : lang === 'typescript' ? 'pnpm-workspace.yaml (catalog)' : '(unknown manifest)'

// --- [MODELS] ----------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['language', 'packages'], properties: { language: { type: 'string' }, domain: { type: 'string' }, packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name'], properties: { name: { type: 'string' }, version: { type: 'string' }, role: { type: 'string' }, apiFile: { type: 'string' } } } }, suspicions: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, evidence: { type: 'string' } } } } } }
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['name', 'verdict'], properties: { name: { type: 'string' }, verdict: { type: 'string', enum: ['keep', 'replace', 'remove', 'add'] }, target: { type: 'string' }, reason: { type: 'string' }, license: { type: 'string' }, licenseOk: { type: 'boolean' }, maintenance: { type: 'string' }, feasible: { type: 'boolean' }, evidence: { type: 'string' } } }
const DECISION_SCHEMA = { type: 'object', additionalProperties: false, required: ['changes'], properties: { changes: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['id', 'kind', 'package'], properties: { id: { type: 'string' }, kind: { type: 'string', enum: ['replace', 'remove', 'add'] }, package: { type: 'string' }, target: { type: 'string' }, rationale: { type: 'string' } } } }, rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: { package: { type: 'string' }, why: { type: 'string' } } } } } }
const VERDICT_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'approved'], properties: { id: { type: 'string' }, approved: { type: 'boolean' }, severity: { type: 'string' }, findings: { type: 'string' } } }
const EXEC_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'verdict'], properties: { id: { type: 'string' }, verdict: { type: 'string', enum: ['applied', 'skipped'] }, files: { type: 'array', items: { type: 'string' } }, projects: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const BUILD_SCHEMA = { type: 'object', additionalProperties: false, required: ['green'], properties: { green: { type: 'boolean' }, diagnostics: { type: 'string' }, projects: { type: 'array', items: { type: 'string' } }, repairedFiles: { type: 'array', items: { type: 'string' } } } }
const HEAL_SCHEMA = { type: 'object', additionalProperties: false, required: ['fixed'], properties: { fixed: { type: 'boolean' }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, autonomous package-modernization run. The target planning folder is ' + norm + ' (language=' + lang + '). Its README and .api ' +
    'catalogs home to ' + docHome + '; design ' +
    'pages live under ' + planningHome + '. Central package pins live in ' + manifests + ' — never a per-package manifest.',
  'ASSAY OWNER-ROUTING + CONSUMER-TFM HARDENING (a verified lesson, non-negotiable): external members are verified via `uv run --frozen python -m ' +
    'tools.assay api` (decompile/reflection over NuGet DLLs / installed Python distributions / node_modules). assay api default resolution selects ' +
    'ONE primary_assembly per key and CAN pick a target framework the build does NOT bind, whose public surface differs from the consumed asset ' +
    '(observed: RectpackSharp resolved netstandard2.0 Pack(PackingRectangle[]) while the net10 consumer binds net5.0 Pack(Span<PackingRectangle>) ' +
    '— a different signature). For C#, the consumer floor is net10.0 (Directory.Build.props TargetFramework): decompile the lib/<tfm> the net10 ' +
    'consumer actually binds (highest compatible lib folder), explicitly, and diff against assay default before trusting any member claim — set ' +
    'DOTNET_ROOT (DOTNET_ROOT=$(dirname "$(readlink -f "$(command -v dotnet)")")) and run ilspycmd <pkg>/lib/<consumer-tfm>/<asm>.dll -t <FQN>. ' +
    'For Python/TS, verify against the actually-installed distribution / declared .d.ts. Never document or compare a phantom from a non-bound TFM.',
  'ULTRA-STACKING BOTH .api TIERS: ' + apiTiers + ' are enumerated with a real ls/fd listing and read at operator depth — a member claim comes ' +
    'from a catalog line or a fresh consumer-TFM-correct decompile, never memory. An admitted capability the folder concern admits but no owner ' +
    'exploits is a named defect feeding the keep/replace/remove/integration analysis, never a shrug; a cited member that cannot be re-verified on ' +
    'the consumer-bound surface is a PHANTOM — reject any change that leans on it, and a writing stage deletes it from every catalog it touches.',
  'LICENSE GATE: a package is admissible only if it is OSS (any OSI-approved license) OR carries a commercial license that is NOT fee-based and ' +
    'NOT tiered/seat/usage-gated (a free, unrestricted commercial grant). Any fee, subscription, seat cap, eval-only, or usage-tiered license is ' +
    'REJECTED. State the exact license (SPDX or the grant) and the OK/REJECT verdict for every add/replace target.',
  'PYTHON RUNTIME GATE (binding when language=python): the workspace floor is CPython 3.15 (pyproject.toml requires-python >=3.15) on osx-arm64, ' +
    'and admissibility is decided by an ACTUAL `uv lock` + `uv sync` + import on cp315, NEVER by wheel-presence alone. A package is admitted ' +
    'UN-GATED when it is (a) pure-Python / a cp315-clean wheel (abi3 cp3x<=15 or py3-none-any), OR (b) a native sdist that SOURCE-BUILDS on cp315 ' +
    'via the Forge scientific toolchain (the shapely/pyarrow/pyogrio/geopandas pattern) — PREFER the source-build over a marker, and verify it by ' +
    'running `uv lock`+`uv sync` and importing the module. A `python_version<3.15` companion marker is honest ONLY for a package with a real, ' +
    'reported cp315 build failure — wheel-absence ALONE is NOT a reason to gate (that is the reflexive hedge to eliminate). Heavy native ' +
    'substrates (GDAL/GEOS/PROJ/Arrow/HDF5/tesseract/libvips) are Forge-provisioned. After any Python add you MUST `uv lock`+`uv sync` and ' +
    'import-verify it on cp315 (the ruff/ty gate does NOT exercise the install); revert only a package that genuinely cannot resolve/build. Reject ' +
    'legacy/abandoned distributions.',
  'AGGRESSIVE ADDITION, ZERO HEDGING (future-looking justification): admit a package whenever it provides unique, modern capability appropriate to ' +
    'the folder domain/scope. DECLINE ONLY for a real reason — (a) it is old / unmaintained / low-quality, (b) a strictly stronger alternative is ' +
    'already admitted, or (c) it is out of the folder scope/domain. NEVER decline because there is no CURRENT consumer: planned consumers are real ' +
    'design pressure and zero current consumers never lowers the capability bar. Once justified, implement NOW and FULLY with NO hedging — never ' +
    'pin or document a capability as future / planned / deferred / a separate-future-owner. A justified package is admitted, INSTALLED/RESOLVABLE ' +
    'on the floor, .api-documented, README-listed, AND integrated into the relevant EXISTING design pages by growing the existing owner IN PLACE ' +
    '(a case / row / field / operation, the rebuild-csharp expand-in-place mentality), not merely mentioned beside it. An ALREADY-ADMITTED package ' +
    'missing its README row, its .api, its actual install, or its real integration is an INCOMPLETE admission to COMPLETE now (manifest + install ' +
    '+ .api + README + real design integration), never a thing to re-shelve as future.',
  'REDUNDANCY + ANTI-BLOAT DISCIPLINE: only REMOVE a package when another ALREADY-ADMITTED package provably covers EVERY call site it serves — ' +
    'cite the subsuming package and the call sites. Only ADD a package when it closes a real hand-rolled capability or a genuine gap AND no ' +
    'admitted package already covers it; never add for its own sake, and never add a thin wrapper over an admitted surface. A replacement must be ' +
    'strictly more modern / more capable / better maintained than the incumbent, not a lateral move. When two packages each could cover the other, ' +
    'keep ONE (resolve the redundancy graph), never remove both.',
  'CROSS-REPO RIPPLE MANDATE: a package is shared. Every add / remove / replace must touch EVERY point repo-wide: the central manifest row, EVERY ' +
    'planning folder README that lists it, EVERY .api/*.md catalog for it, and EVERY code call site across ALL folders (not only the target ' +
    'folder). Survey the full blast radius first; fix every touch point in the same change. A removed package leaves no README mention, no .api ' +
    'file, no manifest pin, and no code reference; an added package is present in all of those plus a new .api catalog authored to house format.',
  'HOUSE .api FORMAT: header (package / version / license / build-floor or target / marker), then member sections grouped by concern with ' +
    'backticked symbols + signatures + a consumer/boundary note. NO provenance, NO process narration, NO freshness tails. Cite REAL members only, ' +
    'consumer-TFM-correct. Code obeys the route-owned standard for the file (docs/stacks/csharp for .cs, docs/stacks/python for .py, docs/stacks/typescript for .ts).',
  'ADVERSARIAL REVIEW LAW: every map, proposal, and applied change is naive, shallow, or illusory until it survives an aggressive attack; dense ' +
    'confident-looking work is the prime suspect, and a clean verdict is EARNED by an attack that finds nothing, never conceded on first read. ' +
    'NAIVETY is a defect on two orthogonal axes: COVERAGE — the work models a thin slice of its concern (the obvious three call sites where the ' +
    'repo carries fifteen, one README row where several folders list the package); APPROACH — an enumerated hardcoded roster (of touch points, ' +
    'call sites, variants, members) stands where a parameterized repo-wide search or generator should derive the space: a roster is seed DATA ' +
    'feeding one derivation, never the mechanism. Every enumerated check or attack list in a task prompt is a FLOOR, never the complete set — ' +
    'hunt defects beyond it.',
  'WRITE-FULLY MANDATE: the execute, heal, and post-execution verify stages make every edit NOW via Edit/Write across all touch points; a fix-log ' +
    'REPORTS edits already made — never a to-do list, a ledger, or a would/should hedge. The inventory / survey / decide / verify-proposal stages ' +
    'run BEFORE any change is applied and are read-only — read-only is their ONLY concession: analysis runs at full adversarial depth against ' +
    'real disk state, and a pre-write verdict is earned by an attack that finds nothing, never conceded.',
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
const invPrompt = [
  LAW, '',
  'TASK (DISCOVERY INVENTORY — read-only is the ONLY concession this stage gets, and it never licenses a skim): map the complete package surface ' +
    'of ' + norm + ' against real disk state, never memory. (1) ENUMERATE with a real ls/fd listing: both .api tiers (' + apiTiers + '), the ' +
    'central manifest ' + manifests + ', and the folder tree. (2) FULL-FILE reads: the README at ' + docHome + ', the project file (the .csproj ' +
    'under ' + docHome + ' if C#), every catalog in both .api tiers the folder touches, and every design page under ' + planningHome + '. (3) Emit ' +
    'the package MAP: EVERY package the folder uses — name + current pinned version + a role line carrying its composed capability here, its ' +
    'concrete underutilized members (verified spellings only, never a phantom), and a hostile weak/strong call — plus the matching .api filename ' +
    'when one exists. (4) HAND-ROLL SUSPICIONS from the design pages and folder code: each capability the folder implements by hand that an ' +
    'ecosystem package likely owns, with a concrete evidence pointer (page + section). (5) The folder DOMAIN in one or two sentences. The map ' +
    'grounds every downstream stage as an initial pointer, never a ceiling — downstream agents re-read and exceed it. Return the structured ' +
    'inventory. Write nothing.',
].join('\n')
const surveyPrompt = (w) => [
  LAW, '',
  w.kind === 'gap'
    ? 'TASK (READ-ONLY SURVEY of a HAND-ROLL SUSPICION): the target folder may be hand-rolling: ' + w.capability + ' (evidence: ' + (w.evidence || 'n/a') + '). ' +
      'First enumerate both .api tiers (' + apiTiers + ') with a real listing and check whether an ALREADY-ADMITTED package owns this concern — ' +
      'when one does, prove the exact members via assay (consumer-TFM-correct) and return verdict=keep naming that owner and members: unexploited ' +
      'admitted capability is the strongest finding this survey can make. Otherwise research the ecosystem (latest stable version, maintenance ' +
      'signal, license) for a modern owner. Return verdict=add with the target package ONLY if it closes a real gap, passes the license gate, and ' +
      'is not redundant; else verdict=keep with the reason it stays hand-rolled or is already covered.'
    : 'TASK (READ-ONLY SURVEY of one package): assess ' + w.name + ' (role: ' + (w.role || 'n/a') + ', current: ' + (w.version || 'n/a') + ') for ' +
      'the target folder. Read its catalogs in both .api tiers (' + apiTiers + ') first and mine them at operator depth: underutilized admitted ' +
      'capability strengthens keep and sharpens the redundancy graph, and any catalog member you cannot re-verify is a phantom to name in ' +
      'evidence. Determine: is there a newer / more capable / better-maintained replacement (license-admissible)? Is it stale or poorly ' +
      'maintained? Is it REDUNDANT because another admitted package already covers every call site (cite them)? Use web research (latest stable, ' +
      'release recency, maintenance, license) AND assay decompile (CONSUMER-TFM-CORRECT — for C# decompile the net10-bound lib/<tfm>, not assay ' +
      'default; diff a multi-target package explicitly) to ground every member claim. Return one verdict: keep | replace (with target + why ' +
      'strictly better) | remove (with the subsuming admitted package + the call sites it covers) | add is not used here. Apply the license gate ' +
      'to any replacement target. Default to keep unless the evidence is decisive.',
  'Item id: ' + w.name + '. Return the single structured verdict (set name to the id). Write nothing.',
].join('\n')
const decidePrompt = (verdicts) => [
  LAW, '',
  'TASK (READ-ONLY DECISION — the anti-bloat barrier): you hold EVERY survey verdict for the target folder. Reconcile them into a final, ' +
    'globally-consistent change-set. (1) Drop every verdict that fails the LICENSE gate. (2) Resolve the REDUNDANCY GRAPH: if two packages each ' +
    'could subsume the other, keep exactly one; never remove both; never remove a package whose call sites the proposed subsumer cannot actually ' +
    'cover. (3) Apply the ANTI-BLOAT gate: reject any add that does not close a real gap or that duplicates an admitted surface, and reject any ' +
    'replace that is a lateral move rather than a strict upgrade. (4) Dedup conflicting proposals (two verdicts touching the same package). Emit ' +
    'the ordered change-set (each a replace | remove | add with a stable id, the package, the target for replace/add, and the decisive rationale) ' +
    'plus the rejected list with the reason each was dropped. Be conservative: a change ships only if its evidence is decisive. Write nothing.',
  'VERDICTS:\n' + JSON.stringify(verdicts, null, 1),
].join('\n')
const critiquePrompt = (c) => [
  LAW, '',
  'TASK (CRITIQUE GATE of one proposed change — a pre-write gate over a not-yet-applied proposal, so it reads and attacks): the change is ' +
    JSON.stringify(c) + '. Hold it naive and illusory until it survives a mechanical line-by-line audit. (1) NECESSITY re-derived: establish from ' +
    'the repo itself, never the rationale string, that the change closes a real gap, is a strict upgrade, or is a true subsumption. (2) TARGET ' +
    'truth: the replacement/new target is real, license-admissible, and strictly better (for remove: the subsuming admitted package covers EVERY ' +
    'call site — re-find them yourself). (3) MEMBER truth: every cited member re-verified consumer-TFM-correct (assay / ilspycmd on the ' +
    'net10-bound TFM for C#); one phantom sinks the change. (4) BLAST RADIUS re-derived, never trusted: run your own repo-wide rg/fd search across ' +
    'the central manifest, every README, both .api tiers, and all code; the proposal touch-point roster is seed data — your search is the ' +
    'generator that derives the true set, and a single missed touch point is a COVERAGE defect. These checks are a FLOOR — hunt past them. Set ' +
    'approved=true only if the change survives everything you can construct AND the re-derived touch-point map is complete; else approved=false ' +
    'with findings naming each concrete defect (file + claim) — the red-team re-derives them, and a change failing either gate is dropped, so the ' +
    'findings are its only record. Default approved=false on unresolved doubt. Return id = the change id.',
].join('\n')
const redteamPrompt = (c, crit) => [
  LAW, '',
  'TASK (HOSTILE RED-TEAM of one proposed change — the terminal, most aggressive pre-write gate; assume the change is WRONG until it survives): ' +
    'Change: ' + JSON.stringify(c) + '. Prior critique: ' + JSON.stringify(crit) + ' — re-derive it COLD; a prior approved=true is a rejected ' +
    'self-assessment, never evidence. Attack: (a) COUNTERFACTUAL — is a strictly stronger resolution available (a better target, a deeper removal, ' +
    'the reverse verdict)? A token move shipped where a root-level modernization is available is a defect. (b) DOWNGRADE — is the replacement ' +
    'weaker on any axis (capability, maintenance, license, build-floor or runtime compatibility)? (c) LICENSE — secretly fee-based, tiered, ' +
    'seat-capped, or eval-only? (d) LONG-TAIL — find the call site only the "redundant" package can serve, the transitive dependency that snaps, ' +
    'the cp315/net10 gate the target fails. (e) PHANTOMS — re-decompile the consumer-bound TFM explicitly and diff; a cited member absent there ' +
    'sinks the change. (f) BLAST RADIUS — re-run the repo-wide search yourself; a roster the critique blessed is still seed data, and one dangling ' +
    'reference is a fail. (g) NEXT-DIFF — when the next package or consumer lands on this axis, does the change hold as one row, or does it bake ' +
    'in an enumerated shape a parameterized owner should carry? This attack list is a FLOOR — invent attacks beyond it. Set approved=true ONLY if ' +
    'the change survives every attack you can construct; default approved=false with findings naming the killing defect. Return id = the change ' +
    'id. This is the last gate before destructive execution.',
].join('\n')
const executePrompt = (c) => [
  LAW, '',
  'TASK (EXECUTE one approved change — WRITE-FULLY, across the whole-repo blast radius): apply ' + JSON.stringify(c) + ' in full, NOW, via ' +
    'Edit/Write. Touch EVERY point: (a) the central manifest ' + manifests + ' — add / remove / replace the pin (and any build-floor change); (b) ' +
    'EVERY planning folder README that mentions the package; (c) EVERY .api/*.md catalog — DELETE the removed package catalog, AUTHOR a new ' +
    'catalog to house format for an added/replacement package (consumer-TFM-correct members verified via assay), UPDATE existing ones; (d) ' +
    'refactor EVERY code call site across ALL folders to the new surface (or to the subsuming package for a removal), obeying the route-owned ' +
    'coding standard. Do not leave a dangling reference anywhere. Return the fix-log: verdict (applied | skipped), the files you edited, the ' +
    'projects/areas affected (so the build gate can target them), and a one-line summary. If on inspection the change is unsafe, return ' +
    'verdict=skipped with the reason rather than a partial edit.',
].join('\n')
const verifyPrompt = (c, ex, attempt) => [
  LAW, '',
  'TASK (WRITING VERIFY of one applied change — adversarial, never a friendly confirmation): change ' + c.id + ' claims applied. Files touched: ' +
    JSON.stringify((ex && ex.files) || []) + '; projects/areas: ' + JSON.stringify((ex && ex.projects) || []) + '. (1) NECESSITY: re-derive from ' +
    'the repo that the applied resolution is complete and right — an under-applied change (a half-migrated call site, a skipped repo area) fails ' +
    'here. (2) PROVE ON DISK: re-run the repo-wide search for the old and new package yourself; every touch point (manifest row, every README, ' +
    'both .api tiers, every code call site) must hold the end-state. A dangling reference, stale row, phantom member in a catalog, or a ' +
    'loose/weak/token fix is a defect YOU repair NOW via Edit/Write to the root-level form of the same files, obeying the route-owned standard — ' +
    'a single-point patch where the dense root form is available is itself a defect to repair. (3) GATE: run `uv run --frozen python -m ' +
    'tools.assay static` over the affected projects/areas (for C# pass --project <csproj> or --folder; for Python/TS pass --folder <path>), parse ' +
    'the one JSON Envelope on stdout. Report green=true only if the gate is clean AFTER your repairs (no FAILED diagnostics); on red return ' +
    'green=false with the concrete residual diagnostics (file + rule + message) so the heal step can act. List every file you edited in ' +
    'repairedFiles. Attempt index: ' + attempt + '.',
].join('\n')
const healPrompt = (c, ex, v) => [
  LAW, '',
  'TASK (SELF-HEAL — fix the build the last applied change broke, WRITE-FULLY): change ' + c.id + ' left the build red. Diagnostics: ' + ((v && v.diagnostics) || 'see ' +
    'prior gate') + '. Files touched: ' + JSON.stringify((ex && ex.files) || []) + '. Fix the real defects in place (a missed call-site refactor, ' +
    'a wrong member from a non-bound TFM, a stale README/.api reference, a manifest floor) so the gate goes green, obeying the route-owned coding ' +
    'standard and never re-introducing the removed package. Return fixed=true with a one-line summary of what you corrected; on a defect ' +
    'genuinely unfixable from the files at hand return fixed=false naming the blocker.',
].join('\n')
const finalVerifyPrompt = (applied) => [
  LAW, '',
  'TASK (FINAL WHOLE-TARGET WRITING VERIFY): the approved change-set for ' + norm + ' is executed; outcomes: ' + JSON.stringify(applied) + '. ' +
    'A change execute skipped as unsafe stays skipped — out of scope, never force-applied here. This is an adversarial verify, never a ' +
    'confirmation: re-derive the union blast radius of the APPLIED changes yourself (repo-wide rg/fd for every changed package), hunt dangling ' +
    'references, stale README rows, orphaned or missing .api catalogs in both tiers, phantom members, and loose/token fixes left by ' +
    'earlier steps — REPAIR every defect in place NOW via Edit/Write to the root-level form, never a note, and list the files in repairedFiles. ' +
    'Then run the assay build/static gate over the union of affected projects/areas (for C# the affected .csproj set; for Python/TS the affected ' +
    'folders) via `uv run --frozen python -m tools.assay static ...`, parse the JSON Envelope, and report green=true only if the whole affected ' +
    'set is clean after your repairs. On red, return the residual diagnostics.',
].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGET) {
  log('survey-packages: no target folder given. This workflow makes destructive cross-repo changes, so it refuses to run with no scope. Pass args ' +
    '= a planning folder path, e.g. libs/csharp/Rasm.Persistence.')
  return { skipped: true, reason: 'no target folder' }
}

phase('Inventory')
const inv = await agent(invPrompt, { label: 'inventory:' + norm.split('/').pop(), phase: 'Inventory', schema: INVENTORY_SCHEMA, effort: 'max', stallMs: 300000 })
const packages = ((inv && inv.packages) || []).filter((p) => p && p.name)
const suspicions = ((inv && inv.suspicions) || []).filter(Boolean)
const worklist = [
  ...packages.map((p) => ({ kind: 'pkg', name: p.name, role: p.role || '', version: p.version || '' })),
  ...suspicions.map((s, i) => ({ kind: 'gap', name: 'gap-' + i + '-' + String(s.capability || '').slice(0, 40), capability: s.capability, evidence: s.evidence || '' })),
]
log('inventory under ' + norm + ': ' + packages.length + ' packages, ' + suspicions.length + ' hand-roll suspicions; surveying ' + worklist.length + ' ' +
  'items at CAP=' + CAP)
if (!worklist.length) return { target: norm, language: lang, skipped: true, reason: 'inventory found no packages or suspicions' }

// --- [SURVEY]
phase('Survey')
const verdicts = (await pool(worklist, CAP, (w) => agent(surveyPrompt(w), { label: 'survey:' + w.name, phase: 'Survey', schema: SURVEY_SCHEMA, effort: 'high', stallMs: 300000 }))).filter(Boolean)

// --- [DECIDE]
phase('Decide')
const decision = await agent(decidePrompt(verdicts), { label: 'decide', phase: 'Decide', schema: DECISION_SCHEMA, effort: 'max', stallMs: 300000 })
const changes = ((decision && decision.changes) || []).filter((c) => c && c.id)
const rejected = (decision && decision.rejected) || []
log('decide: ' + verdicts.length + ' verdicts -> ' + changes.length + ' candidate changes, ' + rejected.length + ' rejected')
if (!changes.length) return { target: norm, language: lang, surveyed: worklist.length, candidates: 0, applied: 0, rejected: rejected.length, note: 'no ' +
  'modernization change warranted' }

// --- [VERIFY_PROPOSALS]
phase('Verify-Proposals')
const reviewed = (await pipeline(
  changes,
  (c) => agent(critiquePrompt(c), { label: 'critique:' + c.id, phase: 'Verify-Proposals', schema: VERDICT_SCHEMA, effort: 'high', stallMs: 300000 }),
  (crit, c) => agent(redteamPrompt(c, crit), { label: 'redteam:' + c.id, phase: 'Verify-Proposals', schema: VERDICT_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((rt) => ({ change: c, critique: crit, redteam: rt })),
)).filter(Boolean)
const approved = reviewed.filter((r) => r.critique && r.critique.approved && r.redteam && r.redteam.approved).map((r) => r.change)
log('verify-proposals: ' + approved.length + '/' + changes.length + ' changes survived critique+redteam')
if (!approved.length) return { target: norm, language: lang, surveyed: worklist.length, candidates: changes.length, approved: 0, applied: 0, note: 'no ' +
  'change survived adversarial verification' }

// --- [EXECUTE]
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

// --- [HEAL]
phase('Heal')
const finalV = await agent(finalVerifyPrompt(applied), { label: 'verify:final', phase: 'Heal', schema: BUILD_SCHEMA, effort: 'low', stallMs: 420000 })
const appliedCount = applied.filter((a) => a.applied).length
log('execute: ' + appliedCount + '/' + approved.length + ' changes applied; final build green=' + (finalV && finalV.green))

return { target: norm, language: lang, docHome, planningHome, surveyed: worklist.length, candidates: changes.length, approved: approved.length, applied: appliedCount, finalGreen: finalV && finalV.green, rejected: rejected.length, changes: applied, residualDiagnostics: (finalV && !finalV.green) ? finalV.diagnostics : '' }
