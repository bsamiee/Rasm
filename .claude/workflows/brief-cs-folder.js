export const meta = {
  name: 'brief-cs-folder',
  description: 'Author ONE world-class campaign brief for a libs/csharp package or kernel sub-folder: 4-6 parallel surveyors (corpus halves + both .api tiers/manifests + cross-package census; deep mode adds 2 OSS-ecosystem research lanes) -> 1 author agent writing the brief on the artifacts gold standard -> 4 sequential adversarial writing passes (strata/folder-architecture, capability/output grade, package ultra-stacking with verified roster overhaul, cold re-read + final verdict). 9-11 agents, peak concurrency 6. args = {folder, out, upstream, deep, mandate} — folder required; upstream = finalized earlier brief paths honored as consumer pressure with surgical waterfall-ripple authority back into them; deep adds the ecosystem research lanes; mandate = scope-expansion law appended to every prompt. Run in dependency order: Rasm/Geometry, Rasm.AppHost, Rasm.AppUi, Rasm.Fabrication.',
  whenToUse: 'Per csharp target, in dependency order, prior briefs passed as upstream — produces the campaign brief a later specialized rebuild workflow executes. Ephemeral: delete after the cs brief set lands.',
  phases: [
    { title: 'Survey', detail: '4 read-only agents (6 in deep mode): corpus halves + .api tiers/manifests + cross-package census (+ 2 OSS ecosystem research lanes)' },
    { title: 'Author', detail: '1 agent writes the brief from the dossiers on the artifacts gold standard: verdict/telos/laws/verdicts/evidence/escalation/package-pressure/legs' },
    { title: 'Refine', detail: '4 sequential adversarial writing passes: strata -> capability/output -> package ultra-stacking -> cold re-read; each may waterfall-ripple upstream briefs' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const GOLD = 'RASM-PY-ARTIFACTS-BRIEF.md'

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const FOLDER = (typeof argsIn === 'string' && argsIn.trim()) ? argsIn.trim().replace(/\/+$/, '')
  : (argsIn && typeof argsIn === 'object' && typeof argsIn.folder === 'string' && argsIn.folder.trim()) ? argsIn.folder.trim().replace(/\/+$/, '')
  : ''
const NAME = (FOLDER.split('/').pop() || '').replace(/^Rasm\./, '')
const PKG_ROOT = FOLDER.includes('/Rasm/') ? FOLDER.slice(0, FOLDER.indexOf('/Rasm/') + 5) : FOLDER
const OUT = (argsIn && typeof argsIn === 'object' && typeof argsIn.out === 'string' && argsIn.out.trim()) ? argsIn.out.trim()
  : 'RASM-CS-' + NAME.toUpperCase() + '-BRIEF.md'
const UPSTREAM = (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.upstream)) ? argsIn.upstream.filter((u) => typeof u === 'string' && u.trim()) : []
const DEEP = !!(argsIn && typeof argsIn === 'object' && argsIn.deep)
const MANDATE = (argsIn && typeof argsIn === 'object' && typeof argsIn.mandate === 'string' && argsIn.mandate.trim()) ? argsIn.mandate.trim() : ''
const SCRATCH = '.claude/scratch/brief-cs-' + NAME.toLowerCase()

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'key_facts'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' }, pages_read: { type: 'number' },
  key_facts: { type: 'array', items: { type: 'string' } },
  verdict_candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'evidence'], properties: { what: { type: 'string' }, evidence: { type: 'string' } } } } } }
const AUTHOR_SCHEMA = { type: 'object', additionalProperties: false, required: ['brief', 'verdict_count', 'evidence_rows', 'thesis'], properties: {
  brief: { type: 'string' }, verdict_count: { type: 'number' }, evidence_rows: { type: 'number' }, thesis: { type: 'string' } } }
const PASS_SCHEMA = { type: 'object', additionalProperties: false, required: ['edits', 'findings'], properties: {
  edits: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['section', 'what'], properties: { section: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } } } },
  findings: { type: 'array', items: { type: 'string' } },
  roster_changes: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['action', 'package', 'concern'], properties: { action: { type: 'string', enum: ['ADD', 'REMOVE', 'REPLACE'] }, package: { type: 'string' }, concern: { type: 'string' }, verification: { type: 'string' } } } },
  upstream_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['brief', 'what'], properties: { brief: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } } } },
  final_verdict: { type: 'string' }, top_risks: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const PRE = 'Rasm monorepo. Target: ' + FOLDER + '/.planning/ (markdown design pages of intended C# code; code fences are the product). ' +
  'docs/stacks/csharp/ (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) plus the ' +
  'relevant docs/stacks/csharp/domain/ shards govern every fence shape. The WORKSPACE_LAW strata govern placement: KERNEL -> AEC-DOMAIN -> ' +
  'APP-PLATFORM -> HOST-BOUNDARY -> APP, depending strictly upward. Member verification: uv run python -m tools.assay api over restored ' +
  'assemblies is truth for what exists; the nuget MCP owns live feed intelligence; a member you cannot verify is a phantom. Your stance is ' +
  'HOSTILE: assume the corpus is naive with illusory depth until disk proves otherwise; NAIVETY is a defect on two axes — COVERAGE (thin slice ' +
  'of the owned domain) and APPROACH (enumerated instances where a parameterized generator belongs; rosters are seed DATA). Hunt ' +
  'architectural/flow/logic fundamental-approach problems, underutilized admitted capability, concern mixing, duplicate mechanisms, dead typed ' +
  'carriers, hardcoding, prose-vs-fence splits, unwired declared seams, parallel rails where one owner belongs, AND folder-architecture rot: ' +
  'loose one-file-one-folder combos, flat page sprawl, and structures not conducive to growth — a logical, well-made folder architecture is a ' +
  'graded axis of this campaign.' + (MANDATE ? '\nSCOPE MANDATE (binding): ' + MANDATE : '')
const FIT = 'FIT, NEVER COUPLED: the target must fit the whole libs architecture — libs/.planning/architecture.md (package roster + strata ' +
  'law), the sibling-package seams its pages record (read the seam-counterpart pages at whatever depth fit requires — Persistence, Element, ' +
  'Bim, Materials, Compute, the kernel), and .archive/RASM-COMPONENT-PARADIGM-DECISION.md where its seams touch that campaign — as boundary contracts ' +
  'composed at the edge, never coupling to a sibling interior.' + (UPSTREAM.length ? ' UPSTREAM BRIEFS (finalized law for this target\'s ' +
  'foundations — read each FULLY): ' + JSON.stringify(UPSTREAM) + '. The upstream campaigns will land new capabilities and structure; this ' +
  'target is their CONSUMER — every upstream capability it could compose instead of hand-rolling is a named opportunity, and every assumption ' +
  'it makes that an upstream brief changes is a named migration pressure. WATERFALL RIPPLE: when THIS target demands a capability an upstream ' +
  'brief lacks, EDIT that upstream brief surgically in place — an owner extension (a verdict clause, an escalation delta, a package row, a ' +
  'capability row) framed as consumer pressure with this target named as the demanding consumer — never a rewrite, never re-planning the ' +
  'upstream; record every such edit in your return.' : '')
const ROSTER_LAW = 'PACKAGE ROSTER LAW: central ownership is Directory.Packages.props (hand-edited, label-grouped; never dotnet add) + the ' +
  'target .csproj; per-package .api catalogs live under ' + PKG_ROOT + '/.api/ with the shared substrate under libs/csharp/.api/. The roster is ' +
  'OPEN: modern/bleeding-edge OSS additions, removals, and categorical-best replacements are all in scope — one categorical-best owner per ' +
  'concern, superseded packages flagged for removal. License gate: OSS licenses usable by an OSS project and free-for-OSS commercial licenses ' +
  'are admissible; pay-tiered, seat-licensed, and proprietary-gated are REJECTED. Every candidate verified real, maintained, and .NET-current ' +
  '(nuget MCP + two corroborating web sources); native/C++ engines with clean C# bindings or stable P/Invoke surfaces are admissible when the ' +
  'managed ecosystem lacks the concern. INTEGRATION-FIRST: an admitted package with zero page consumers is an integration MANDATE before it ' +
  'is ever a removal candidate — realize it as a row/arm on the owning polymorphic axis (named page, named row); a REMOVE row requires proven ' +
  'redundancy (the concern already lands on a stronger admitted owner, named), feed-verified upstream abandonment, or a charter/license ' +
  'conflict — absence of consumers alone is NEVER a REMOVE reason (zero current consumers never lowers the bar). SEAL-CHALLENGE: a closed ' +
  'sweep, NEVER row, or rejection list in the target corpus is challengeable design pressure, not inherited law — a verdict may re-open it as ' +
  'a parameterized axis (provider rows on one closed dispatch surface); deference to an on-disk seal is argued from architecture, never ' +
  'assumed. PACKAGE WATERFALL: where a package reaches full power only with a counterpart elsewhere (an upstream seam, a decoder, a companion ' +
  'admission), record the enablement BOTH directions — upstream briefs gain the owner extension through the waterfall-ripple authority, ' +
  'downstream folders gain a named forward-obligation row the later brief consumes — so an addition lands clean across the chain, never a ' +
  'folder-local orphan.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (lane, scope) => [PRE, FIT, ROSTER_LAW, 'TASK: READ-ONLY SURVEY, lane = ' + lane + '. Scope: ' + scope + '. Deep-read ' +
  'fully — never skim. WRITE a dense dossier to ' + SCRATCH + '/dossier-' + lane + '.md: per-page {verdict 1-10, defects with file:line, ' +
  'split/merge/move pressure, the owner charter as it SHOULD be}, cross-cutting {duplication, concern mixing, hardcoding-vs-generator, dead ' +
  'carriers, unwired seams, unmined capability with catalog anchors, folder-architecture verdicts}, and the 5-10 strongest VERDICT CANDIDATES ' +
  '(campaign-defining structural rulings with evidence). Dense, evidence-first, zero narration. Return the pointer + key facts + verdict ' +
  'candidates. NO edits outside ' + SCRATCH + '/.'].join('\n')
const authorPrompt = (dossiers) => [PRE, FIT, ROSTER_LAW, 'TASK: AUTHOR the campaign brief ' + OUT + ' (repo root) for the ground-up ' +
  'restructure+rebuild of ' + FOLDER + '/.planning/. GOLD STANDARD: read ' + GOLD + ' (repo root) COMPLETELY first — match its density, ' +
  'structure, and law-grade voice: [00]-[SHARED_LAW] (VERDICT with independently-fatal proofs anchored file:line; TELOS naming what world-class ' +
  'means for THIS target and the 5x-consumer bar; STRUCTURAL_AUTHORITY incl. split/merge/move/new-folder freedom and the folder-architecture ' +
  'law — no one-file folders, structures conducive to growth; the strata/placement law; GENERATOR_LAW; the target-appropriate seam/entry/rail ' +
  'law; the roster reconciliation law), [01] the campaign phase guidance with NUMBERED BINDING VERDICTS (V1..Vn — each a structural ruling with ' +
  'a recommended-shape floor and a ruled default where decidable NOW; hedges carry deciding criteria), [02] the EVIDENCE REGISTER (E-rows, ' +
  'file:line anchors, sound-surfaces line), [03] CAPABILITY ESCALATION (per-plane now->target grades with concrete deltas), [04] ' +
  'PACKAGE_PRESSURE (both .api tiers stack; mine-to-depth rows with stub anchors; roster ADD/REMOVE/REPLACE rows under the roster law), [05] ' +
  'BUILD_LEGS (dependency-ordered waves inside the target; per-leg closeout obligations incl. .api stub + manifest + index-doc alignment), ' +
  '[06] OUT_OF_SCOPE (sibling-owned concerns, upstream-owned concerns, blocked cards). Sources: the survey dossiers ' + JSON.stringify(dossiers) +
  ' (evidence — re-verify on disk anything load-bearing)' + (UPSTREAM.length ? ' + the upstream briefs (law)' : '') + '. Every claim anchored; ' +
  'agent-facing declarative; no provenance, no hedging, no restated doctrine. Return the path + counts + a one-line thesis.'].join('\n')
const passPrompts = [
  (brief) => [PRE, FIT, 'TASK: ADVERSARIAL PASS 1 of 4 — STRATA/FOLDER-ARCHITECTURE COHERENCE. You WRITE: fix and improve ' + brief + ' in ' +
    'place. Interrogate the brief AS AN ARCHITECTURE: do its verdicts compose into one coherent domain map (draw the post-campaign dependency ' +
    'graph — cycles, owners with two masters, fuzzy boundaries get boundary law); is the proposed FOLDER architecture logical and ' +
    'growth-conducive (no one-file folders, no flat sprawl, subfolder boundaries that name real domains — a weak folder plan is a defect you ' +
    'fix with a stronger one); is the internal wave/leg order a true topological order (name and dispose every inversion on disk); are the ' +
    'strata placements right (KERNEL->APP upward-only); is [01] executable without guessing. Verify every claim you add on disk. Never dilute; ' +
    '~1.2x length cap.'].join('\n'),
  (brief) => [PRE, FIT, 'TASK: ADVERSARIAL PASS 2 of 4 — CAPABILITY/OUTPUT GRADE. You WRITE: fix and improve ' + brief + ' in place. Walk the ' +
    'target\'s FLAGSHIP outputs backward (the 2-3 hardest real deliverables a world-class version must produce' + (UPSTREAM.length ? ', ' +
    'including what the upstream briefs now make possible' : '') + ') and find every chain break or vague link — each becomes a verdict ' +
    'extension, escalation delta, or named obligation, with honest ingress boundaries (what arrives from siblings/upstream vs computed here). ' +
    'Where a verdict or [03] row settles for parity or repair, RAISE it to what the world-class version owns, backed by real package surfaces ' +
    '(verify in the .api stubs / assay api; no vapor). Apply the WATERFALL RIPPLE law where a demand belongs upstream. Never dilute; ~1.25x cap.'].join('\n'),
  (brief) => [PRE, FIT, ROSTER_LAW, 'TASK: ADVERSARIAL PASS 3 of 4 — PACKAGE ULTRA-STACKING + ROSTER OVERHAUL. You WRITE: fix and improve ' +
    brief + ' in place. Inventory BOTH tiers (ls libs/csharp/.api/ and ' + PKG_ROOT + '/.api/) + Directory.Packages.props + the target csproj; ' +
    'for every stub the brief does not cite, judge mined-vs-unmined against the owning pages and add verified mine-to-depth rows (stub anchors ' +
    'mandatory). Audit existing [04] rows against the stubs; fix contradicted spellings. Then the ROSTER OVERHAUL under the roster law: ' +
    'supersession sweep (categorical-best; REMOVE/REPLACE rows), and DOMAIN-GAP research — where the target\'s scope demands capability NO ' +
    'admitted package owns, run a REAL ecosystem deep-dive (web research; nuget MCP for the managed side; two corroborating sources per ' +
    'candidate; license gate enforced) and add ADD rows naming the concern, the categorical-best candidate, the license, and the binding ' +
    'surface. Never silently narrow the domain to the current roster. Never dilute; ~1.25x cap.'].join('\n'),
  (brief) => [PRE, FIT, 'TASK: ADVERSARIAL PASS 4 of 4 — COLD RE-READ, the last hands on this brief. You WRITE: fix in place. Read the ENTIRE ' +
    'brief twice as a first reader and as the most hostile one: cross-reference closure (every verdict <-> evidence row <-> escalation delta ' +
    '<-> package row <-> leg assignment; fix every dangling end); naivety hunt on the brief itself (kill hedges — where a "the campaign ' +
    'decides" is decidable NOW, decide it with a ruled default; where genuinely open, verify deciding criteria exist); executability dry-run ' +
    '(could a design/build workflow run from [01]/[05] alone); prose law (declarative, no meta, no provenance, binding forms); spot-check 4-5 ' +
    'evidence anchors on disk for drift. Apply the WATERFALL RIPPLE law for anything that belongs upstream. Return the final verdict ' +
    '(world-class if executed faithfully? with nuance) + top 3 residual risks. Never dilute; ~1.15x cap.'].join('\n'),
]

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!FOLDER) { log('brief-cs-folder: no folder passed (args = {folder: "libs/csharp/<target>", upstream: [...], deep, mandate}). No-op.'); return { folder: '', total: 0 } }

// --- [SURVEY]
phase('Survey')
const LANES = [
  { lane: 'corpus-a', scope: FOLDER + '/.planning/ — ls the subfolder set, sort alphabetically, deep-read every page in the FIRST HALF of the ' +
    'subfolders (plus root-level pages), and the owning package ARCHITECTURE.md + README.md + TASKLOG.md + IDEAS.md in full' },
  { lane: 'corpus-b', scope: FOLDER + '/.planning/ — ls the subfolder set, sort alphabetically, deep-read every page in the SECOND HALF of ' +
    'the subfolders; skim the first half only where a seam crosses into it' },
  { lane: 'api-tiers', scope: 'BOTH catalog tiers COMPLETE: ls + read libs/csharp/.api/*.md and ' + PKG_ROOT + '/.api/*.md, plus ' +
    'Directory.Packages.props and the target .csproj; per stub judge mined vs unmined against the owning pages; inventory capability the ' +
    'target never demands; note DOMAIN GAPS where no admitted package owns a concern the scope needs (roster candidates, named not researched)' },
  { lane: 'census', scope: 'the whole ' + FOLDER + '/.planning/ tree: sweep every cross-package seam/reference in every fence, diff the real ' +
    'graph against the owning ARCHITECTURE.md seam ledger both directions (declared-unwired + wired-undeclared), list page-level cycles, and ' +
    'read the seam-counterpart sibling pages (Persistence/Element/Bim/Materials/Compute/kernel as the pages name them) at fit depth' +
    (UPSTREAM.length ? '; read the upstream briefs FULLY and list every upstream capability this target could consume instead of hand-rolling ' +
    '+ every assumption an upstream campaign invalidates' : '') },
]
const DEEP_LANES = [
  { lane: 'ecosystem-core', scope: 'REAL OSS ecosystem deep-dive for the target\'s CORE domain concerns (derive them from the scope mandate + ' +
    'the corpus): sweep the modern landscape via web research + the nuget MCP — managed packages first, then native engines with C# bindings ' +
    'or stable P/Invoke surfaces; per candidate record concern, license (gate enforced), maturity/maintenance signal, binding surface, and two ' +
    'corroborating sources; produce a candidate table ranked categorical-best per concern' },
  { lane: 'ecosystem-adjacent', scope: 'REAL OSS ecosystem deep-dive for the target\'s ADJACENT/expansion concerns (the scope mandate\'s ' +
    'growth axes + what the corpus is silent on): same method, same evidence bar, same license gate; hunt the concerns the target SHOULD own ' +
    'that nobody has named yet — the candidate table is seed data for the brief\'s escalation and roster rows' },
]
const lanes = DEEP ? LANES.concat(DEEP_LANES) : LANES
const dossiers = (await parallel(lanes.map((l) => () =>
  agent(surveyPrompt(l.lane, l.scope), { label: 'survey:' + l.lane, phase: 'Survey', model: 'opus', effort: 'max', schema: SURVEY_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Survey[' + NAME + (DEEP ? '|deep' : '') + ']: ' + dossiers.length + '/' + lanes.length + ' dossiers; ' +
  dossiers.reduce((n, d) => n + (d.verdict_candidates || []).length, 0) + ' verdict candidate(s)')

// --- [AUTHOR]
phase('Author')
const authored = await agent(authorPrompt(dossiers.map((d) => d.dossier)), { label: 'author:' + NAME, phase: 'Author', effort: 'max', schema: AUTHOR_SCHEMA, stallMs: STALL })
if (!authored) { log('Author produced nothing — brief not written; resume from this run id re-runs it.'); return { folder: FOLDER, dossiers: dossiers.map((d) => d.dossier), aborted: 'author' } }
log('Author: ' + authored.brief + ' — ' + authored.verdict_count + ' verdicts, ' + authored.evidence_rows + ' evidence rows. Thesis: ' + authored.thesis)

// --- [REFINE]
phase('Refine')
const PASS_LABELS = ['strata', 'capability', 'packages', 'cold-read']
const passes = []
for (let i = 0; i < passPrompts.length; i++) {
  const p = await agent(passPrompts[i](authored.brief), { label: 'pass:' + PASS_LABELS[i], phase: 'Refine', effort: i === 3 ? 'max' : 'xhigh', schema: PASS_SCHEMA, stallMs: STALL })
  passes.push(p)
  log('Pass ' + (i + 1) + '/4 (' + PASS_LABELS[i] + '): ' + (p ? (p.edits || []).length + ' edit(s), ' + (p.findings || []).length + ' finding(s)' +
    ((p.roster_changes || []).length ? ', roster ' + p.roster_changes.length : '') +
    ((p.upstream_ripples || []).length ? ', upstream ripples ' + p.upstream_ripples.length : '') : 'NO RESULT — rerun via resume'))
}
const final = passes[3]

return {
  folder: FOLDER, brief: authored.brief, thesis: authored.thesis,
  verdicts: authored.verdict_count, evidence_rows: authored.evidence_rows,
  pass_edits: passes.map((p, i) => ({ pass: PASS_LABELS[i], edits: p ? (p.edits || []).length : -1 })),
  roster_changes: passes.filter(Boolean).flatMap((p) => p.roster_changes || []),
  upstream_ripples: passes.filter(Boolean).flatMap((p) => p.upstream_ripples || []),
  final_verdict: (final && final.final_verdict) || 'PASS 4 MISSING — rerun',
  top_risks: (final && final.top_risks) || [],
  dossiers: dossiers.map((d) => d.dossier), scratch: SCRATCH,
}
