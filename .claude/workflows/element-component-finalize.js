export const meta = {
  name: 'element-component-finalize',
  description: 'Finalizes the element-component rebuild (WF-1 run wf_35e9d759-619 completed DISCOVER through REBUILD then was killed before its tail). This workflow IS that tail: a lightweight pickup DISCOVER enumerates the rebuilt-but-unfinalized corpus (the 11 Component pages, the Projection collapse, the refined Element seam pages, Construction, the touched Appearance anchors, the four Materials index docs) and seeds the 66 surfaced residuals; then a HOSTILE dual-axis CRITIQUE then REDTEAM fixes each batch in place against the executed RASM-REBUILD-DECISION architecture and the frozen anchors, carrying an explicit phantom-verification checklist; then a SWEEP lands the cross-cutting work the rebuild punted (the seam-mirror rows at both endpoints, the Materials domain-map collapse plus README router, the 72-row rename map, the Appearance anchor-only fixups, the folder-casing reconcile, the intra-Component boundary-row deletes, and recording the WF-2 Bim hand-off rows); then a terminal no-defer RESOLVE drives every residual to zero through an adversarial reconcile fix-then-verify loop and a sanity re-audit. Design-doc work only; it does not re-author from scratch and does not touch Bim/Compute/Persistence/python/ts bodies.',
  whenToUse: 'After element-component was killed mid-tail — finalizes the rebuilt corpus (critique, redteam, sweep, resolve).',
  phases: [
    { title: 'Discover' },
    { title: 'Critique' },
    { title: 'Redteam' },
    { title: 'Sweep' },
    { title: 'Resolve' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const STALL = 600000
const MAX_ROUNDS = 6
const SANITY_CAP = 6
const BATCH = 4
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const DECISION = 'RASM-REBUILD-DECISION.md'
const HANDOFF = 'rebuild-handoff'
const BASE = 'c29e0b79'
const ELEMENT = 'libs/csharp/Rasm.Element'
const MATERIALS = 'libs/csharp/Rasm.Materials'

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages', 'seedResiduals', 'summary'], properties: { pages: { type: 'array', items: { type: 'string' } }, seedResiduals: RESIDUAL, summary: { type: 'string' } } }
const FOLDER_FIXLOG = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['closed', 'hardened', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SANITY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'items'], properties: { overall: { type: 'boolean' }, items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'open'] }, evidence: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const SCOPELAW = [
  'CAMPAIGN: the unified Material/Component/Element rebuild (WF-1, the atomic Element+Materials concern). The rebuild leg already RAN ' +
    '(DISCOVER->RESEARCH->VALIDATE->DECIDE->REBUILD, 22 page fixlogs, corpus complete on disk) and was KILLED before CRITIQUE/REDTEAM/SWEEP/RESOLVE — this ' +
    'workflow IS those four phases plus a pickup DISCOVER. READ ' + SCOPE + ' (repo root) in FULL FIRST; its named section governs the stage. READ ' + DECISION +
    ' (repo root) in FULL — it is the EXECUTED architecture (12 component pages, 7 Element-seam refinements, the 72-row rename map, the 18 retirements) the ' +
    'rebuild realized; every critique/redteam/sweep gate is measured against it. All work is .planning design-doc work — code fences ARE the product ' +
    '(transcription-complete, decompile-verified, implementation-ready). The Materials index docs + .api live at the package root ' + MATERIALS +
    '/, NOT inside .planning.',
  'CROSS-WORKFLOW CONTRACT: runs AFTER element-component was killed; FINALIZES the rebuilt corpus (critique/redteam/sweep/resolve). Does NOT re-author from ' +
    'scratch. WF-1 LANDS the seam-mirror ROWS at BOTH endpoints (Element ARCHITECTURE.md [2]-[SEAMS] and Materials ARCHITECTURE.md [02]-[SEAMS]). The sibling ' +
    'Bim/Compute/Persistence/python/ts BODY re-binds are WF-2/WF-3 — do NOT edit those bodies here; only RECORD the WF-2 Bim hand-off seam rows. Resolve a ' +
    'captured WATCH residual per ' + SCOPE + ' [3] ONLY when this rebuild touches its site.',
].join('\n')
const AXES = [
  'DUAL-AXIS READ (every implementing stage reads BOTH ' + SCOPE + ' + ' + DECISION + ' in FULL first; finalizes only when a cold read surfaces nothing per ' +
    SCOPE + ' [6]). CODE doctrine: docs/stacks/csharp/ every core page by name (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, ' +
    'algorithms, system-apis) PLUS the relevant docs/stacks/csharp/domain/ shard — for this campaign especially data-interchange, diagnostics, interaction, ' +
    'transport, validation.',
  'DOC-CRAFT axis: libs/.planning/README.md [PLANNING_STANDARD] incl. its [06] cold-grade gate, the design-page grammar, and the page#CLUSTER ' +
    'integration-point notation; the three docs/standards/ form standards (information-structure, formatting, style-guide); docs/standards/proof.md claim ' +
    'discipline. The README cold-grade is the doc-finalization gate.',
].join('\n')
const APILAW = 'TWO-TIER .api (per ' + SCOPE + ' [7]): cite BOTH the shared substrate libs/csharp/.api/ AND the folder tier ' + MATERIALS + '/.api/ — and ONLY ' +
  'the catalogs the concept composes, never noise. ' + ELEMENT + ' has no .api tier of its own; it composes the shared substrate. Members are verified-local ' +
  'truth via `uv run python -m tools.assay api`; a member you cannot verify is a PHANTOM — never introduce it. Live NuGet feed intelligence routes through the ' +
  'nuget MCP; assay api wins on conflict for member existence. NO new central pin is needed (scope [4] + the RESEARCH/VALIDATE legs confirmed ZERO pinsNeeded).'
const HEDGELAW = 'BANNED HEDGES + ZERO-PROVENANCE (word-boundary, page-wide, per ' + SCOPE + ' [6]): no should, could, would, might, maybe, perhaps, likely, ' +
  'probably, propose, consider, recommended, ideally, TBD, TODO, FIXME, we, our, you, or the synonym forms (is expected to, can be, aims to, is designed to, ' +
  'in the future, eventually, as needed, if necessary). On a design page: no reader address, narration, process, provenance, source-mining history, freshness ' +
  'disclaimers, checklist tails, links, URLs, versions, dates, or session context. Future tense is legal only on a card growth line and a RESEARCH item.'
const STANCE = 'ADVERSARIAL + WRITE-FULLY: assume the page is incomplete, naive, or illusory until it survives an aggressive attack; the burden of proof is on ' +
  'the page. Close every gap NOW via Edit/Write — the returned log REPORTS edits already made, never a to-do. Only a genuine cross-FILE item goes to ' +
  'residual_high as {files, claim} for the terminal Resolve (which has no scope cap).'
const DOCTRINE = [SCOPELAW, '', AXES, '', APILAW, '', HEDGELAW, '', STANCE].join('\n')

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
const inLibs = (p) => typeof p === 'string' && (p.startsWith('libs/') || p.indexOf('/libs/') !== -1)
const norm = (x, fallback) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : [fallback], claim: x.claim } }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const chunk = (xs, n) => { const out = []; for (let i = 0; i < xs.length; i += n) out.push(xs.slice(i, i + n)); return out }
const discoverPrompt = () => [DOCTRINE, '', 'TASK: READ-ONLY PICKUP DISCOVERY — enumerate the rebuilt-but-unfinalized corpus the killed element-component run ' +
  'left on disk. (1) Run `git diff --name-only ' + BASE + ' -- ' + MATERIALS + ' ' + ELEMENT + '` for changed/new tracked pages PLUS `git ls-files --others ' +
  '--exclude-standard ' + MATERIALS + ' ' + ELEMENT + '` for any untracked Component pages (the rebuild leg can be uncommitted). (2) Cross-reference ' + DECISION +
  ' [01] componentPages + [02] the 7 Element-seam refinements for the AUTHORITATIVE page list. (3) Read ' + HANDOFF + '/residuals.json (the 66 cross-file ' +
  'residuals the rebuild surfaced) and return it VERBATIM as seedResiduals (each {files, claim}). Return the full rebuilt `.md` page list as `pages` — the 11 ' +
  'Component/ pages, Projection/component.md, the rebuilt Element seam pages + ' + ELEMENT + '/ARCHITECTURE.md, the construction/ pages, the touched ' +
  'appearance/ pages, and the 4 Materials index docs at ' + MATERIALS + '/ (README.md, ARCHITECTURE.md, IDEAS.md, TASKLOG.md). DO NOT edit anything; run git ' +
  'read commands only, never write. Return pages + seedResiduals + summary.'].join('\n')
const critiquePrompt = (batch, i) => [DOCTRINE, '', 'TASK: DUAL-AXIS HOSTILE CRITIQUE + FIX IN PLACE across this batch of rebuilt pages (batch ' + i + '). ' +
  'Measure every fence against ' + DECISION + ' (the decided architecture the rebuild executed) and the FROZEN anchors in ' + HANDOFF +
  '/investigation-discover.md [ANCHORS_TO_PRESERVE_CONSOLIDATED]. Assume a violation in EVERY fence until proven otherwise. Run the CODE-axis mechanical checks ' +
  '(COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST, two-weave ASPECTS, RAILS, STRATA/MEMBERS, both .api tiers MAXIMIZED, no phantom member) AND the DOC-CRAFT axis (the ' +
  '[06] cold-grade, page grammar, banned hedges, zero-provenance) and REPAIR every hit in place.\n' +
  'PHANTOM-VERIFICATION CHECKLIST (from ' + HANDOFF + '/investigation-research.md — confirm EACH was CORRECTED in the rebuilt family pages, verifying via assay ' +
  'api / Context7 / the cited standard): glazing SpecificHeat 840->720; the GWP double-count; ceramic-fire lambda 1.40->~1.14; the MuntinGrid false "EN/ASTM" ' +
  'citation; the catalogue 2857-vs-2867 row; fastener Coupler->USERDEFINED is FALSE (COUPLER is real IFC4X2; the enum omits RAILJOINT/RAILFASTENING/CHAIN/ROPE); ' +
  'weld/joint WeldType 14-way duplicates GrooveType 9, PJP -1/8" wrongly includes SAW, GrooveRadiusMm "U larger" contradicts the table; steel European.HEM300 ' +
  'is phantom (real HE300M), SteelGrade.S420/S460 railing to ProfileFault.Family; the brick MortarJointProfile.prism drop. ALSO settle the ' +
  'PositiveMagnitude/Dimension namespace contradiction (Profiles cited Rasm.Domain, Connection cited Rasm.Vectors — one is phantom; settle via assay api); ' +
  'confirm quantity.md (Element/Properties) sanctions the non-UnitsNet engineering discriminators SectionModulus/SecondMomentOfArea/TorsionConstant/' +
  'WarpingConstant the SeamSection chain mints (else phantom); verify the Component/ComponentSection [Union]/ComponentFault band-2300/ComponentCatalogue ' +
  'contract in Component/component.md is consistent across all 9 family pages; verify the Element Graph/element.md seam-body (NodeId.RootedType/ToTypeSeedBytes, ' +
  'the named Bake Type->Occurrence inheritance, BakedMaterial/TypeBinding/Element.TypeId) is fully realized. FIX in place; a cross-FILE item goes to ' +
  'residual_high {files, claim}. PAGES:\n' + JSON.stringify(batch, null, 1) +
  '\nReturn folder + verdict + integrated + extended + residual_high + summary.'].join('\n')
const redteamPrompt = (batch, crit, i) => [DOCTRINE, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE across this batch (batch ' + i + ') — the LAST, most ' +
  'aggressive pass; trust nothing the critique claimed. Re-measure against ' + DECISION + ' (the executed architecture) and the frozen anchors. COUNTERFACTUAL ' +
  'on each owner (is it the densest the doctrine admits?), ANTICIPATORY_COLLAPSE of the next case, LONG-TAIL ingress AND egress, STRATA + BOUNDARY-INTEGRITY, ' +
  'SURFACE-SPRAWL + PHANTOM kill, plus a FULL COLD re-review of every critique dimension AND the phantom-verification checklist on BOTH axes. The batch MUST end ' +
  'denser, more capable, more correct. Edit in place; cross-FILE -> residual_high {files, claim}. PAGES:\n' + JSON.stringify(batch, null, 1) + '\nPRIOR CRITIQUE:\n' +
  JSON.stringify(crit, null, 1) + '\nReturn folder + verdict + integrated + extended + residual_high + summary.'].join('\n')
const sweepPrompt = (batch, i) => [DOCTRINE, '', 'TASK: SEAM-MIRROR + RENAME + LANDING SWEEP over this batch of rebuilt pages (batch ' + i + '). This phase owns ' +
  'the cross-cutting landings the rebuild PUNTED — the bulk of the 66 residuals concentrate on the Materials index docs. Apply the VERBATIM 72-row rename map ' +
  'from ' + DECISION + ' [03] across every stale anchor these pages own (sibling refs to the retired Profiles/ + Connection/ pages, hanger->connector, ' +
  'ProfileId->ComponentId, MaterialBinding->BakedMaterial, etc.) and verify ZERO dangling references to the 18 retired items. Apply the Appearance ANCHOR-ONLY ' +
  'fixups (the 5 cross-page anchors into the collapsed pages; never touch an Appearance signature; AppearanceKey + AppearanceSummary.Of tolerance-0.0 FROZEN). ' +
  (i === 0 ? 'THIS BATCH (index 0) ALSO owns the cross-cutting landings: LAND the [2]-[SEAMS] unified-paradigm MIRROR ROWS at BOTH endpoints (' + ELEMENT +
  '/ARCHITECTURE.md [2]-[SEAMS] <-> ' + MATERIALS + '/ARCHITECTURE.md [02]-[SEAMS], currently empty of unified-paradigm rows, mirrored both sides); land the ' +
  'Materials [1]-[DOMAIN_MAP] codemap collapse + README router; DELETE the intra-Component [BOUNDARY] seam rows (an in-package relation is codemap, never a ' +
  'seam); reconcile the FOLDER-CASING question (the new Component/ + existing Projection/ are capitalized while siblings appearance/ + construction/ are ' +
  'lowercase — settle to ONE convention per ' + DECISION + ' / the repo norm); and RECORD (do not implement) the WF-2 Bim hand-off seam rows. ' : '') +
  'WF-1 lands the rows at BOTH endpoints; the Bim/Compute/Persistence/python/ts BODY re-binds are WF-2/WF-3 — do NOT edit those bodies. A counterpart edit you ' +
  'cannot complete -> residual_high {files, claim}. PAGES:\n' + JSON.stringify(batch, null, 1) + '\nReturn verdict + aligned + residual_high + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the discover-seed/critique/redteam/sweep ' +
  'phases surfaced; NO severity, NO leftovers, NO deferral, NO scope cap. Read EVERY listed file across libs (csharp + py + ts) and FIX the real cross-file ' +
  'defect in place to the strongest clean/modern form (align the seam + every consumer in lockstep, repair strata/boundary, finish a spanning admission, make a ' +
  'doc truthful), preserving all capability — a token patch that leaves the seam misaligned is NOT a fix; if a residual is FACTUALLY WRONG, leave it and say ' +
  'why. A new cross-file need -> residual_high. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [SCOPELAW, '', STANCE, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and ' +
  'CONFIRM the fix is ACTUALLY made AND complete + high-quality + clean/modern, not a token/naive patch. ATTACK it: shallow, partial, a rename that left a ' +
  'sibling stale, a seam still misaligned? Classify each "fixed" (real, complete, non-naive), "invalid" (claim factually wrong — cite why), or "open" (NOT ' +
  'fixed or fixed naively — redo). Default "open" on ANY doubt; a confident edit that does not truly resolve the cross-file defect is "open", never "fixed". ' +
  'Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')
const sanityPrompt = (items) => [DOCTRINE, '', 'TASK: SINGLE SANITY PASS over ALL ' + items.length + ' surfaced residuals (the snapshotted union). For EACH: ' +
  're-read the cited file(s) from disk and CONFIRM the defect is GENUINELY + CLEANLY fixed — the seam holds on BOTH endpoints, the fix is clean/modern per ' +
  'docs/stacks/csharp, no token patch, no sibling left stale, no new drift. Be adversarial: a confident-looking edit that does not truly resolve the defect is ' +
  '"open". Classify each "fixed" or "open" with one-line evidence; default "open" on ANY doubt. The full libs corpus is in scope to read. ITEMS:\n' +
  items.map((it, n) => (n + 1) + '. ' + it.claim).join('\n') + '\nReturn overall + items + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('element-component-finalize: discover(pickup) -> critique/redteam -> sweep -> resolve; CAP=' + CAP)

// --- [DISCOVER]
phase('Discover')
const discovered = await agent(discoverPrompt(), { label: 'discover', phase: 'Discover', schema: DISCOVER_SCHEMA, model: 'opus', effort: 'max', stallMs: STALL })
const pages = [...new Set(((discovered && discovered.pages) || []).filter((p) => typeof p === 'string' && p))]
const seedResiduals = ((discovered && discovered.seedResiduals) || []).map((x) => norm(x, MATERIALS))
const batches = chunk(pages, BATCH)
log('Discover: ' + pages.length + ' rebuilt page(s) -> ' + batches.length + ' review batch(es); ' + seedResiduals.length + ' seed residual(s) from ' + HANDOFF + '/residuals.json')

// --- [CRITIQUE]
phase('Critique')

// --- [REDTEAM]
phase('Redteam')
const reviewed = (await pipeline(batches,
  (batch, _batch, i) => agent(critiquePrompt(batch, i), { label: 'critique:b' + i, phase: 'Critique', schema: FOLDER_FIXLOG, effort: 'xhigh', stallMs: STALL }),
  (crit, batch, i) => agent(redteamPrompt(batch, crit, i), { label: 'redteam:b' + i, phase: 'Redteam', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }).then((rt) => ({ critique: crit, redteam: rt })),
)).filter(Boolean)

// --- [SWEEP]
phase('Sweep')
const swept = (await pool(batches, CAP, (batch, i) => agent(sweepPrompt(batch, i), { label: 'sweep:b' + i, phase: 'Sweep', schema: SWEEP_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [RESOLVE]
phase('Resolve')
const surfaced = dedup([
  ...seedResiduals,
  ...reviewed.flatMap((r) => ((r.critique && r.critique.residual_high) || []).map((x) => norm(x, MATERIALS))),
  ...reviewed.flatMap((r) => ((r.redteam && r.redteam.residual_high) || []).map((x) => norm(x, MATERIALS))),
  ...swept.flatMap((r) => (r.residual_high || []).map((x) => norm(x, MATERIALS))),
])
let pending = surfaced
let invalid = []
let round = 0
while (pending.length && round < MAX_ROUNDS) {
  round++
  const clusters = cluster(pending)
  log('Resolve reconcile round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (lib-wide, no-defer)')
  const resolved = (await pool(clusters, CAP, async (cl) => {
    const fix = await agent(reconcileFix(cl), { label: 'resolve-fix:r' + round, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL })
    if (!fix) return { open: cl, invalid: [], surfaced: [] }
    const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify:r' + round, phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
    const claims = (verify && verify.claims) || []
    const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
    const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
    return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, MATERIALS)) }
  })).filter(Boolean)
  invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
  const invalidKeys = new Set(invalid.map((r) => r.claim))
  pending = dedup([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim))
}
if (round && pending.length) log('Resolve reconcile: ' + pending.length + ' open after ' + MAX_ROUNDS + ' rounds — re-audited in the sanity drive-to-zero')

// DRIVE TO ZERO: re-audit the whole snapshotted residual universe after EVERY force-close until nothing is open. The cap is a runaway backstop, not a give-up.
const universe = surfaced
let sanity = universe.length ? await agent(sanityPrompt(universe), { label: 'sanity', phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL }) : null
let openClaims = new Set(((sanity && sanity.items) || []).filter((i) => i.status === 'open').map((i) => i.claim))
let sanityOpen = universe.filter((r) => openClaims.has(r.claim))
let saneRound = 0
while (sanityOpen.length && saneRound < SANITY_CAP) {
  saneRound++
  const clusters = cluster(sanityOpen)
  log('Resolve sanity round ' + saneRound + ': ' + sanityOpen.length + ' OPEN -> force-close per cluster (' + clusters.length + ') + re-audit; nothing leaves open')
  await pool(clusters, CAP, (cl) => agent(reconcileFix(cl), { label: 'sanity-force-close:r' + saneRound, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL }))
  sanity = await agent(sanityPrompt(universe), { label: 'sanity:r' + saneRound, phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
  openClaims = new Set(((sanity && sanity.items) || []).filter((i) => i.status === 'open').map((i) => i.claim))
  sanityOpen = universe.filter((r) => openClaims.has(r.claim))
}
if (sanityOpen.length) log('Resolve SANITY: ' + sanityOpen.length + ' STILL OPEN after ' + SANITY_CAP + ' force-close rounds — HARD BLOCKER, reported LOUDLY, never silently dropped')
else log('Resolve SANITY: all ' + universe.length + ' surfaced residual(s) CLOSED + verified across ' + saneRound + ' force-close round(s)')

return {
  workflow: 'element-component-finalize',
  pages: pages.length,
  batches: batches.length,
  seedResiduals: seedResiduals.length,
  surfaced: surfaced.length,
  resolveRounds: round,
  sanityRounds: saneRound,
  invalidClaims: invalid.map((x) => x.claim),
  openResidual: sanityOpen.map((x) => ({ files: x.files, claim: x.claim })),
}
