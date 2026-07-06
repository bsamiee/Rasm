export const meta = {
  name: 'brief',
  description: 'Durable polyglot campaign-brief author over libs/{python,csharp,typescript} planning corpora. args = {targets, upstream, deep, mandate, review, gold} — targets a folder path or an ORDERED array (a waterfall: each later brief consumes every earlier one as finalized law with surgical ripple authority back); upstream = pre-existing finalized brief paths (any language) joining the corpus; deep = true or a target-path subset gaining 2 OSS-ecosystem research lanes; mandate = a scope-expansion law string for all targets or a {targetPath: text} map; review = extra brief paths for the terminal cross-corpus review, or false to skip it; gold = the exemplar brief (default RASM-PY-ARTIFACTS-BRIEF.md). Per target: 5 surveyors (corpus halves + api/manifest tiers + seam/consumer census + cross-folder strata census; +2 deep lanes) all on gpt-5.5 via codex dispatch wrappers (sonnet shells; surveyors write dossiers workspace-write, deep lanes add live web search; CODEX flag false restores native lanes) -> 1 author (a single-phase decision-complete brief that never requires a second document, carrying the bidirectional CROSS_FOLDER enablement section, the section-utility anti-chaff law, and the header campaign law) -> 4 sequential adversarial passes (architecture, capability incl. the cross-folder audit, roster under the integration-first/seal-challenge/package-waterfall laws, cold-read + hedge-kill + chaff-sweep + RIPPLE AUDIT re-verifying every claimed upstream edit on disk). Terminal: when 1+ briefs were produced, 3 sequential review passes (initial/critique/redteam) cross-align the WHOLE corpus in place. Output naming RASM-<PY|CS|TS>-<NAME>-BRIEF.md. 10-12 agents per target + 3 review.',
  whenToUse: 'The standing brief engine: author one brief, or a dependency-ordered waterfall of them, in any language mix, with the cross-corpus review built in. Empty args = no-op.',
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const CODEX = true // survey/strata + deep research lanes run on gpt-5.5 via the codex wrapper; false restores native lanes
const CODEX_DIR = '.claude/scratch/codex' // wrapper task/schema/report files, one triple per lane
const LANG = {
  python: { tag: 'PY', doctrine: 'docs/stacks/python/', tiers: 'libs/python/.api/ (branch substrate) + the folder .api/ (domain)',
    manifest: 'the root pyproject.toml — lean unpinned names, bounds only on resolver evidence, one owning manifest',
    verify: 'PyPI JSON + two corroborating web sources per candidate (license, wheels incl. the <3.15 band where relevant, maintenance); paid/gated/proprietary REJECTED',
    law: 'the shared-tier weave is corpus law: expression tagged_union/Result/Option/Block/Map as the one ADT and dispatch spine, anyio at ' +
      'concurrent seams, runtime guarded/railed retry over the POLICY rows, msgspec one-shot ingress, beartype public entries' },
  csharp: { tag: 'CS', doctrine: 'docs/stacks/csharp/ plus the docs/stacks/csharp/domain/ shards', tiers: 'libs/csharp/.api/ (shared substrate) + the package .api/ (domain)',
    manifest: 'Directory.Packages.props (hand-edited, label-grouped; never dotnet add) + the target .csproj',
    verify: 'uv run python -m tools.assay api over restored assemblies (member truth, verified-local wins) + the nuget MCP (feed truth) + two ' +
      'corroborating web sources; license gate enforced (OSS or free-for-OSS commercial; pay-tiered/seat-licensed/proprietary-gated REJECTED)',
    law: 'the WORKSPACE_LAW strata govern placement: KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP, depending strictly upward; ' +
      'AEC peers never reference each other' },
  typescript: { tag: 'TS', doctrine: 'docs/stacks/typescript/', tiers: 'libs/typescript/.api/ (branch) + the folder .api/ (domain)',
    manifest: 'pnpm-workspace.yaml + the workspace catalog — central version ownership, no per-package drift',
    verify: 'the npm registry + two corroborating web sources per candidate (license, types, maintenance); paid/gated/proprietary REJECTED',
    law: 'the Effect-native platform doctrine governs: services/layers/runtime wiring, one rail, schema-first boundaries per docs/stacks/typescript' },
}

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const TARGETS = (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn.trim().replace(/\/+$/, '')]
  : (Array.isArray(argsIn)) ? argsIn.filter((t) => typeof t === 'string' && t.trim()).map((t) => t.trim().replace(/\/+$/, ''))
  : (argsIn && typeof argsIn === 'object' && argsIn.targets)
    ? (Array.isArray(argsIn.targets) ? argsIn.targets : [argsIn.targets]).filter((t) => typeof t === 'string' && t.trim()).map((t) => t.trim().replace(/\/+$/, ''))
    : []
const UPSTREAM = (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.upstream)) ? argsIn.upstream.filter((u) => typeof u === 'string' && u.trim()) : []
const DEEP = (argsIn && typeof argsIn === 'object') ? argsIn.deep : false
const MANDATE = (argsIn && typeof argsIn === 'object') ? argsIn.mandate : ''
const REVIEW_EXTRA = (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.review)) ? argsIn.review.filter((r) => typeof r === 'string' && r.trim()) : []
const REVIEW_OFF = !!(argsIn && typeof argsIn === 'object' && argsIn.review === false)
const GOLD = (argsIn && typeof argsIn === 'object' && typeof argsIn.gold === 'string' && argsIn.gold.trim()) ? argsIn.gold.trim() : 'RASM-PY-ARTIFACTS-BRIEF.md'
const langOf = (t) => { const m = t.match(/libs\/(python|csharp|typescript)(\/|$)/); return m ? LANG[m[1]] : null }
const nameOf = (t) => ((t.split('/').pop() || '').replace(/^Rasm\.?/, '') || (t.split('/').pop() || '')).toUpperCase().replace(/[^A-Z0-9]/g, '')
const outOf = (t) => 'RASM-' + (langOf(t) || { tag: 'X' }).tag + '-' + nameOf(t) + '-BRIEF.md'
const deepFor = (t) => DEEP === true || (Array.isArray(DEEP) && DEEP.includes(t))
const mandateFor = (t) => (typeof MANDATE === 'string') ? MANDATE.trim() : (MANDATE && typeof MANDATE === 'object' && typeof MANDATE[t] === 'string') ? MANDATE[t].trim() : ''

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
  roster_changes: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['action', 'package', 'concern'], properties: { action: { type: 'string', enum: ['ADD', 'REMOVE', 'REPLACE', 'INTEGRATE'] }, package: { type: 'string' }, concern: { type: 'string' }, verification: { type: 'string' } } } },
  upstream_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['brief', 'what'], properties: { brief: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } } } },
  ripple_audit: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claimed', 'verdict'], properties: { claimed: { type: 'string' }, verdict: { type: 'string', enum: ['LANDED', 'APPLIED-BY-ME', 'CORRECTED'] } } } },
  final_verdict: { type: 'string' }, top_risks: { type: 'array', items: { type: 'string' } } } }
const REVIEW_SCHEMA = { type: 'object', additionalProperties: false, required: ['edits', 'opportunities', 'alignments'], properties: {
  edits: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['brief', 'what'], properties: { brief: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } } } },
  opportunities: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['enabler', 'consumer', 'what'], properties: { enabler: { type: 'string' }, consumer: { type: 'string' }, what: { type: 'string' } } } },
  alignments: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['seam', 'fix'], properties: { seam: { type: 'string' }, fix: { type: 'string' } } } },
  final_verdict: { type: 'string' }, residual_risks: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const ROSTER_LAW = 'PACKAGE ROSTER LAW: central version ownership per the language manifest; per-package catalogs live in the .api tiers and every ' +
  'admission/removal carries its catalog motion. The roster is OPEN — modern/bleeding-edge additions, verified removals, and categorical-best ' +
  'replacements are all in scope, one categorical-best owner per concern. INTEGRATION-FIRST: an admitted package with zero page consumers is an ' +
  'integration MANDATE before it is ever a removal candidate — realize it as a row/arm on the owning polymorphic axis (named page, named row); a ' +
  'REMOVE requires proven redundancy (the concern already lands on a stronger admitted owner, named), feed-verified upstream abandonment, or a ' +
  'charter/license conflict — absence of consumers alone is NEVER a REMOVE reason. SEAL-CHALLENGE: a closed sweep, NEVER row, or rejection list ' +
  'in the target corpus is challengeable design pressure, not inherited law — a verdict may re-open it as a parameterized axis (provider rows on ' +
  'one closed dispatch surface); deference to an on-disk seal is argued from architecture, never assumed. PACKAGE WATERFALL: where a package ' +
  'reaches full power only with a counterpart elsewhere (an upstream seam, a decoder, a companion admission), record the enablement BOTH ' +
  'directions — upstream briefs gain the owner extension through the ripple authority, downstream folders gain a named forward-obligation row — ' +
  'so an addition lands clean across the chain, never a folder-local orphan. DOMAIN-GAP research is mandatory where the target scope demands ' +
  'capability no admitted package owns: name the concern + the categorical-best candidate + the binding surface, verified per the language rail. ' +
  'Never silently narrow the domain to the current roster.'
const CROSS_FOLDER_LAW = 'CROSS-FOLDER LAW: high-value capability is never planned in isolation. The brief names enablement rows in RELATED ' +
  'FOLDERS — the realized corpora on disk, independent of any upstream brief — in BOTH directions: a base/lower-stratum extension that unlocks a ' +
  'stronger form in the target (the base folder gains a fully-specified IDEAS-row obligation, the target verdict composes it), and a target ' +
  'capability that opens doors in consumers or siblings (the consumer opportunity named with its seam). Every row binds at declared wire/seam ' +
  'boundaries — content keys, frozen wire names, entry/receipt ports — never a coupling to a sibling interior; cross-language rows bind at the ' +
  'wire only. A cross-folder row without a named seam is a defect.'
const CHAFF_LAW = 'SECTION UTILITY LAW: the brief is LAW the rebuild engine executes directly — it never requires a second document, a follow-up ' +
  'design pass, or a DECISION file. Every section, verdict clause, evidence row, and table row must change the executing agent\'s behavior; ' +
  'boilerplate framing, restated doctrine, generic methodology, empty filler sections, and prose that describes rather than rules are deleted on ' +
  'sight. A genuine evidence-gated hinge is DECIDED in the brief — a ruled default plus the deciding criteria that would flip it — never deferred.'
const RIPPLE_LAW = 'CORPUS + WATERFALL LAW: the finalized briefs listed as CORPUS are law for this target — read each FULLY; this target is their ' +
  'CONSUMER (every upstream capability it could compose instead of hand-rolling is a named opportunity; every assumption an upstream brief ' +
  'changes is a named migration pressure), and cross-LANGUAGE corpus rows bind at wire/content-key seams only, decoded at the boundary, never a ' +
  'coupling. RIPPLE AUTHORITY: when THIS target demands a capability an upstream brief lacks, EDIT that brief surgically in place — an owner ' +
  'extension (a verdict clause, an evidence row, an escalation delta, a package row) framed as consumer pressure with this target named as the ' +
  'demanding consumer, EXTENDING its numbering (a new Vn/En), never a rewrite, never re-planning the upstream, and every such edit recorded in ' +
  'your return as upstream_ripples so the terminal audit can re-verify it on disk.'
const preOf = (t, corpus) => { const L = langOf(t), m = mandateFor(t); return 'Rasm monorepo. Target: ' + t + '/.planning/ (markdown design pages ' +
  'of intended code; the fences are the product). ' + L.doctrine + ' governs every fence; ' + L.law + '; libs/.planning/architecture.md governs ' +
  'the cross-branch map. Both .api tiers are member truth: ' + L.tiers + '. Manifest: ' + L.manifest + '. Verification rail: ' + L.verify + '. ' +
  'Your stance is HOSTILE: assume the corpus is naive with illusory depth until disk proves otherwise; NAIVETY is a defect on two axes — ' +
  'COVERAGE (a thin slice of the owned domain) and APPROACH (enumerated instances where a parameterized generator belongs; rosters are seed ' +
  'DATA). Hunt architectural/flow/logic fundamental-approach problems, underutilized admitted capability, concern mixing, duplicate mechanisms, ' +
  'dead typed carriers, hardcoding, prose-vs-fence splits (a capability claimed in prose but absent from the fence is ILLUSORY), unwired ' +
  'declared seams, parallel rails where one owner belongs, and folder-architecture rot (one-file folders, flat sprawl, structures not conducive ' +
  'to growth). ' + CROSS_FOLDER_LAW + ' ' + CHAFF_LAW +
  (corpus.length ? ' CORPUS (finalized briefs, dependency order): ' + JSON.stringify(corpus) + '. ' + RIPPLE_LAW : '') +
  (m ? '\nSCOPE MANDATE (binding): ' + m : '') }

// --- [OPERATIONS] ------------------------------------------------------------------------
// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// CODEX_DIR, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns that JSON verbatim. It never does, edits, or judges
// the work. `web` inserts live web search.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes, web) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and relay ' +
    'its typed answer VERBATIM. Never perform, edit, judge, soften, or summarize the task yourself.',
  '(1) mkdir -p ' + CODEX_DIR + '; write the TASK block below verbatim to ' + base + '-task.md; write this JSON ' +
    'Schema exactly to ' + base + '-schema.json: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call that returns immediately: ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral ' +
    (web ? '-c web_search="live" ' : '') + '--output-schema ' + base + '-schema.json -o ' + base + '-report.json ' +
    '"Do the task in ' + base + '-task.md from the repository root. Final message: JSON per the output schema." ' +
    '</dev/null >/dev/null 2>&1 &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rpt + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rpt + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). Cap at 7 poll calls.',
  '(4) READY: return the report-file JSON through your structured output VERBATIM, unchanged. GONE with no report: ' +
    'relaunch the (2) command once (detached, never foreground) and resume polling; a second GONE returns the ' +
    'schema shape with every array empty and each required string field set to CODEX-FAILED plus the one-line reason.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}
// Every survey/research lane routes here: gpt-5.5 wrapper when CODEX, the lane's native model otherwise.
const recon = (task, o) => CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes, !!o.web),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: o.schema, stallMs: STALL })
  : agent(task, o.model
    ? { label: o.label, phase: o.phase, model: o.model, effort: 'high', schema: o.schema, stallMs: STALL }
    : { label: o.label, phase: o.phase, effort: 'high', schema: o.schema, stallMs: STALL })
const surveyPrompt = (pre, scratch, lane, scope) => [pre, 'TASK: READ-ONLY SURVEY, lane = ' + lane + '. Scope: ' + scope + '. Deep-read fully — ' +
  'never skim. WRITE a dense dossier to ' + scratch + '/dossier-' + lane + '.md: per-page {verdict 1-10, defects with file:line, ' +
  'split/merge/move pressure, the owner charter as it SHOULD be}, cross-cutting {duplication, concern mixing, hardcoding-vs-generator, dead ' +
  'carriers, unwired seams, unmined capability with catalog anchors}, and the 5-10 strongest VERDICT CANDIDATES (campaign-defining structural ' +
  'rulings with evidence). Dense, evidence-first, zero narration. NO edits outside ' + scratch + '/.'].join('\n')
const deepPrompt = (pre, scratch, lane, focus) => [pre, ROSTER_LAW, 'TASK: ECOSYSTEM RESEARCH, lane = ' + lane + '. ' + focus + ' Web research ' +
  'with two corroborating sources per candidate and the license gate enforced; verify real, maintained, current. WRITE the dossier to ' +
  scratch + '/dossier-' + lane + '.md: per-candidate {package, concern owned, binding surface, license, maturity signals, verdict ' +
  'ADMIT-CANDIDATE/REFERENCE-ONLY/REJECT with the reason}. NO edits outside ' + scratch + '/.'].join('\n')
const authorPrompt = (pre, t, out, dossiers) => [pre, ROSTER_LAW, 'TASK: AUTHOR the campaign brief ' + out + ' (repo root) for the ground-up ' +
  'restructure+rebuild of ' + t + '/.planning/. GOLD STANDARD: read ' + GOLD + ' (repo root) COMPLETELY first — match its density, structure, ' +
  'and law-grade voice. The brief carries: [00]-SHARED_LAW (a VERDICT with independently-fatal proofs anchored file:line; TELOS with the ' +
  '5x-consumer bar; STRUCTURAL_AUTHORITY incl. split/merge/move/new-folder freedom; the placement/strata law; GENERATOR_LAW; the seam/entry/' +
  'rail law; roster reconciliation), [01] NUMBERED BINDING VERDICTS (V1..Vn — each a structural ruling with a recommended-shape floor and a ' +
  'ruled default where decidable NOW; a hedge carries its deciding criteria), [02] the EVIDENCE REGISTER (E-rows with file:line anchors + the ' +
  'sound-surfaces line), [03] CAPABILITY ESCALATION (per-plane now->target grades with concrete deltas), [04] PACKAGE_PRESSURE (mine-to-depth ' +
  'rows with stub anchors; roster rows under the roster law), [05] BUILD_LEGS (dependency-ordered legs + per-leg closeout + acceptance proofs), ' +
  '[06] CROSS_FOLDER (the bidirectional enablement rows per the cross-folder law — each row {folder, direction, capability, seam}, fully ' +
  'specified from the strata dossier and re-verified on disk; base-extension rows land as fully-specified IDEAS-row obligations the campaign ' +
  'applies), [07] OUT_OF_SCOPE. The brief is SINGLE-PHASE and decision-complete per the section utility law: the rebuild engine consumes it ' +
  'directly, no second document ever follows it, and every structural hinge is DECIDED — a ruled default plus the deciding criteria that would ' +
  'flip it. HEADER LAW: line 3 of the brief is the campaign line, 1-3 lines: its track order relative to the corpus, the Workflow invocation ' +
  '(rebuild.js args), and its one sequencing constraint. Sources: the survey dossiers ' + JSON.stringify(dossiers) + ' (evidence — re-verify on ' +
  'disk anything load-bearing) plus the corpus as law. Every claim anchored; agent-facing declarative; no provenance, no hedging, no restated ' +
  'doctrine. Return the path + counts + a one-line thesis.'].join('\n')
const passPrompts = (pre, brief) => [
  [pre, 'TASK: ADVERSARIAL PASS 1 of 4 — ARCHITECTURE. You WRITE: fix and improve ' + brief + ' in place. Interrogate the brief AS AN ' +
    'ARCHITECTURE: do its verdicts compose into one coherent domain map (draw the post-campaign dependency graph — cycles, owners with two ' +
    'masters, fuzzy boundaries get boundary law); is the folder plan growth-conducive (no one-file folders, no flat sprawl; a weak plan is a ' +
    'defect you fix with a stronger one); is the leg order a true topological order (name and dispose every inversion on disk); are the strata/' +
    'placement calls right; is [01] executable without guessing. Verify every claim you add on disk. Never dilute; ~1.2x length cap.'].join('\n'),
  [pre, 'TASK: ADVERSARIAL PASS 2 of 4 — CAPABILITY/OUTPUT GRADE. You WRITE: fix and improve ' + brief + ' in place. Walk the target\'s ' +
    'FLAGSHIP outputs backward (the 2-3 hardest real deliverables a world-class version must produce, including what the corpus briefs now ' +
    'make possible) and find every chain break or vague link — each becomes a verdict extension, escalation delta, or named obligation, with ' +
    'honest ingress boundaries (what arrives from siblings/upstream vs computed here). Where a verdict or [03] row settles for parity or ' +
    'repair, RAISE it to what the world-class version owns, backed by real package surfaces (verify per the language rail; no vapor). Audit ' +
    '[06] CROSS_FOLDER against the flagship walk: every chain link that crosses a folder boundary either composes a named enablement row or ' +
    'gains one, both directions, seam-bound. Apply the ripple authority where a demand belongs upstream. Never dilute; ~1.25x cap.'].join('\n'),
  [pre, ROSTER_LAW, 'TASK: ADVERSARIAL PASS 3 of 4 — ROSTER + API ULTRA-STACKING. You WRITE: fix and improve ' + brief + ' in place. Inventory ' +
    'BOTH .api tiers + the manifest; for every stub the brief does not cite, judge mined-vs-unmined against the owning pages and add verified ' +
    'mine-to-depth rows (stub anchors mandatory); audit existing [04] rows against the stubs and fix contradicted spellings. Then the ROSTER ' +
    'OVERHAUL under the roster law: integration mandates for zero-consumer admissions, supersession sweep (categorical-best), DOMAIN-GAP ' +
    'research with ADD rows, every motion verified per the language rail. Never dilute; ~1.25x cap.'].join('\n'),
  [pre, 'TASK: ADVERSARIAL PASS 4 of 4 — COLD RE-READ + RIPPLE AUDIT, the last hands on this brief. You WRITE: fix in place. Read the ENTIRE ' +
    'brief twice, as a first reader and as the most hostile one: cross-reference closure (every verdict <-> evidence row <-> escalation delta ' +
    '<-> package row <-> leg assignment <-> cross-folder row; fix every dangling end); hedge-kill (where "the campaign decides" is decidable ' +
    'NOW, decide it with a ruled default; where genuinely open, verify deciding criteria exist — a two-phase deferral or DECISION-file pointer ' +
    'is a defect rewritten to a decided verdict); CHAFF SWEEP (delete every section, row, or clause that fails the section utility law); ' +
    'executability dry-run ([01]+[05] alone must run without guessing); ' +
    'HEADER check (line 3 carries track order + the invocation + the one constraint). RIPPLE AUDIT: re-verify ON DISK every upstream ' +
    'ripple the author and passes 1-3 claimed — read the upstream brief at the claimed site; a claimed-but-absent edit is APPLIED by you now ' +
    '(owner-extension form), a wrong one CORRECTED; record each in ripple_audit. Return the final verdict + top risks.'].join('\n'),
]
const reviewPre = (scope) => 'Rasm monorepo. THE CORPUS, in dependency order (earlier = upstream foundation, later = consumer; languages may ' +
  'mix — cross-language rows bind at wire/content-key seams only): ' + JSON.stringify(scope) + ' — each a finalized campaign brief a rebuild ' +
  'workflow executes against its folder. Read EVERY brief COMPLETELY and in order before any edit. You WRITE: improvements land in the briefs ' +
  'in place as owner extensions (a verdict clause, an evidence row, an escalation delta, a package row, a leg obligation — never a parallel ' +
  'section, never a rewrite of settled law). LAWS: (1) FIT, NEVER COUPLED — a later brief composes an earlier folder at declared seams and ' +
  'never re-plans it; an earlier brief never reaches forward except as recorded consumer pressure; (2) ALIGNMENT is bidirectional — a seam one ' +
  'brief names and the counterpart is silent on is a defect fixed on BOTH sides; conflicting spellings/shapes for one seam resolve to one; ' +
  '(3) OPPORTUNITY law — where an earlier brief lands a capability that makes a stronger feature possible in a later folder, the later brief ' +
  'gains the row (with the enabling brief named as the seam), and where a later brief needs something no earlier brief provides, the earlier ' +
  'brief gains the consumer-pressure extension; (4) every claim you add is disk-verified or brief-anchored — no vapor; (5) each brief must ' +
  'remain internally closed after your edits (dispositions complete, partitions acyclic, hedges criteria-bearing); (6) every brief is single-phase and ' +
  'decision-complete — every edit preserves the brief\'s own header contract and numbered verdict/evidence grammar, EXTENDING the numbering ' +
  '(a new Vn/En row) rather than restructuring it, and a deferral to a second document is a defect decided in place; ' +
  '(7) INTEGRATION-FIRST roster audit — re-judge every REMOVE/strike/' +
  'prune row across the corpus: zero consumers is an integration mandate, never a removal reason — a removal survives only on proven ' +
  'redundancy (stronger admitted owner named), feed-verified abandonment, or charter/license conflict, and one failing that bar rewrites to a ' +
  'realization row on its owning axis; record cross-folder package enablement both directions. The goal: the rebuild workflows inherit ' +
  'guidance with zero gaps, zero contradictions, and every cross-folder possibility named.'
const reviewPrompts = (scope) => [
  [reviewPre(scope), 'TASK: PASS 1 of 3 — INITIAL HOLISTIC. First full read of the corpus as one program. Per brief: fix mistakes, close gaps, ' +
    'kill silences on concerns a sibling brief treats as load-bearing. Collectively: walk the dependency chain both directions — enumerate ' +
    'every predecessor-enabled opportunity no brief names yet and every consumer-demand hole (what a later brief assumes upstream that ' +
    'upstream never promises). Apply every finding in place.'].join('\n'),
  [reviewPre(scope), 'TASK: PASS 2 of 3 — CRITIQUE. Verify every pass-1 edit against disk and the briefs (an unanchored or wrong edit is ' +
    'repaired, never tolerated). Then the mechanical floor per brief: cross-reference closure, disposition completeness, seam-ledger ' +
    'consistency ACROSS briefs (one seam, one spelling, both sides), header campaign lines current, corpus references valid after pass-1 ' +
    'edits. Fix everything you find in place.'].join('\n'),
  [reviewPre(scope), 'TASK: PASS 3 of 3 — RED-TEAM COLD READ, the last hands on this corpus. Read the whole set twice as its future ' +
    'implementing agents will: hostile, fresh, lens-by-lens — counterfactual (would faithful execution actually produce world-class folders, ' +
    'or is a brief load-bearing on an unstated assumption?), long-tail (rare-but-real cases no brief covers), boundary (every cross-folder and ' +
    'cross-language seam honest?), sprawl (do two briefs quietly plan the same owner twice?), completeness (is any [03] target unreachable ' +
    'from the verdicts as written?). Fix in place; return the corpus verdict + residual risks.'].join('\n'),
]

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length) { log('brief: pass {targets, upstream?, deep?, mandate?, review?} — targets a path or ordered array. No-op.'); return { targets: 0, produced: [] } }
const bad = TARGETS.filter((t) => !langOf(t))
if (bad.length) { log('brief: unrecognized language root for ' + JSON.stringify(bad) + ' — targets must live under libs/{python,csharp,typescript}.'); return { targets: TARGETS.length, produced: [], rejected: bad } }

const corpus = [...UPSTREAM]
const produced = []
for (let ti = 0; ti < TARGETS.length; ti++) {
  const t = TARGETS[ti]
  const L = langOf(t)
  const name = nameOf(t)
  const out = outOf(t)
  const scratch = '.claude/scratch/brief-' + L.tag.toLowerCase() + '-' + name.toLowerCase()
  const pre = preOf(t, corpus)
  const P = L.tag + ':' + name.toLowerCase()

  // --- [SURVEY]
  phase(P + ' survey')
  const lanes = [
    { lane: 'corpus-a', scope: 'the FIRST half of the target .planning pages (alphabetical by path) FULLY, plus the folder README/ARCHITECTURE/TASKLOG/IDEAS where present' },
    { lane: 'corpus-b', scope: 'the SECOND half of the target .planning pages FULLY, plus every page the first half seams to at the depth fit requires' },
    { lane: 'api-tiers', scope: 'BOTH .api tiers COMPLETE (' + L.tiers + ') + the manifest (' + L.manifest + '); judge every stub ' +
      'mined-vs-unmined against the owning pages; verify roster claims per the language rail where versions are cited' },
    { lane: 'census', scope: 'the seam/consumer census: every cross-page and cross-package/cross-language edge the target carries, the ' +
      'governance surfaces (ledger/router/cards) vs realized truth, and the corpus briefs\' clauses that name this target' },
    { lane: 'strata', scope: 'the CROSS-FOLDER enablement census over realized corpora on disk (independent of any brief): the folders this ' +
      'target composes or feeds — its language kernel/base strata below, its consumers above, its cross-language wire counterparts — each read ' +
      'at the depth fit requires; per related folder return BOTH directions with the binding seam named: the base extension that would unlock a ' +
      'stronger form in the target, and the target capability that would open a door in that folder' },
  ]
  const deepLanes = deepFor(t) ? [
    { lane: 'ecosystem-a', focus: 'Sweep the OSS ecosystem for the target\'s CORE domain concerns: the categorical-best owners for capability ' +
      'the mandate/telos demands, judged against what the roster already admits.' },
    { lane: 'ecosystem-b', focus: 'Sweep the ADJACENT/emerging lanes: bleeding-edge or cross-domain packages that could raise the capability ' +
      'ceiling, plus supersession candidates for weak admitted owners.' },
  ] : []
  const surveyed = (await parallel([
    ...lanes.map((l) => () => recon(surveyPrompt(pre, scratch, l.lane, l.scope),
      { label: 'survey:' + l.lane, phase: P + ' survey', model: 'opus', schema: SURVEY_SCHEMA, writes: true })),
    ...deepLanes.map((l) => () => recon(deepPrompt(pre, scratch, l.lane, l.focus),
      { label: 'survey:' + l.lane, phase: P + ' survey', schema: SURVEY_SCHEMA, writes: true, web: true })),
  ])).filter(Boolean)
  const dossiers = surveyed.map((s) => s.dossier)
  log(P + ' survey: ' + surveyed.length + '/' + (lanes.length + deepLanes.length) + ' dossiers')

  // --- [AUTHOR]
  phase(P + ' author')
  const authored = await agent(authorPrompt(pre, t, out, dossiers), { label: 'author:' + name.toLowerCase(), phase: P + ' author', effort: 'high', schema: AUTHOR_SCHEMA, stallMs: STALL })
  if (!authored) { log(P + ': author produced nothing — aborting this target; resume re-runs it.'); continue }
  log(P + ' author: ' + authored.brief + ' — ' + authored.verdict_count + ' verdicts, ' + authored.evidence_rows + ' E-rows')

  // --- [REFINE]
  phase(P + ' refine')
  const PASS_LABELS = ['architecture', 'capability', 'roster', 'cold-read']
  let lastPass = null
  for (let i = 0; i < 4; i++) {
    const p = await agent(passPrompts(pre, authored.brief)[i], { label: 'pass:' + PASS_LABELS[i], phase: P + ' refine', effort: 'high', schema: PASS_SCHEMA, stallMs: STALL })
    if (p) lastPass = p
    log(P + ' pass ' + (i + 1) + '/4 (' + PASS_LABELS[i] + '): ' + (p ? (p.edits || []).length + ' edit(s), ' + (p.upstream_ripples || []).length + ' ripple(s)' : 'NO RESULT — rerun via resume'))
  }
  produced.push({ target: t, brief: authored.brief, thesis: authored.thesis,
    final_verdict: (lastPass && lastPass.final_verdict) || '', top_risks: (lastPass && lastPass.top_risks) || [] })
  corpus.push(authored.brief)
}

// --- [REVIEW]
let review = null
if (produced.length && !REVIEW_OFF) {
  phase('review')
  const scope = [...corpus, ...REVIEW_EXTRA.filter((r) => !corpus.includes(r))]
  const REVIEW_LABELS = ['initial', 'critique', 'redteam']
  const passes = []
  for (let i = 0; i < 3; i++) {
    const p = await agent(reviewPrompts(scope)[i], { label: 'review:' + REVIEW_LABELS[i], phase: 'review', effort: 'high', schema: REVIEW_SCHEMA, stallMs: STALL })
    passes.push(p)
    log('review ' + (i + 1) + '/3 (' + REVIEW_LABELS[i] + '): ' + (p ? (p.edits || []).length + ' edit(s), ' + (p.opportunities || []).length + ' opportunit(ies), ' + (p.alignments || []).length + ' alignment(s)' : 'NO RESULT — rerun via resume'))
  }
  review = {
    scope, passes: passes.map((p, i) => ({ pass: REVIEW_LABELS[i], edits: p ? (p.edits || []).length : -1 })),
    opportunities: passes.filter(Boolean).flatMap((p) => p.opportunities || []),
    alignments: passes.filter(Boolean).flatMap((p) => p.alignments || []),
    final_verdict: (passes[2] && passes[2].final_verdict) || 'REVIEW PASS 3 MISSING — rerun',
    residual_risks: (passes[2] && passes[2].residual_risks) || [],
  }
}

return { targets: TARGETS, produced, review }
