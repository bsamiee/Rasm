export const meta = {
  name: 'design-artifacts-restructure',
  description: 'Ephemeral Phase-1 design pass for the py artifacts campaign: survey (5 read-only plane agents re-verifying the E1-E13 register on disk, both .api tiers, the dead-marker census, and the four upstream briefs as landed law) -> 4 complete structural blueprints under the brief lenses -> 3 judge lenses with FIVE disqualifying gates (disposition completeness, foundations-first acyclicity incl. the four live inversions, entry-contract completeness, owner-boundaries tier naming, integration-first roster) -> synthesize winner+grafts (fable) -> red-team decide gate (fable) that WRITES RASM-PY-ARTIFACTS-DECISION.md in rebuild-engine-native action vocabulary with every hinge ruled at decision time -> salvage (4 draft miners + overturn auditor + 1 fable writing integrator) -> refine (3 sequential fable cold passes — initial/critique/redteam — over the WRITTEN decision, extending it where justified and surgically pre-adjusting artifacts pages a ruling makes appropriate). About 24 agents, peak concurrency 5. args = {brief, out, scratch} with defaults; a string arg overrides the brief path.',
  whenToUse: 'One-shot: turn RASM-PY-ARTIFACTS-BRIEF.md into RASM-PY-ARTIFACTS-DECISION.md — the binding page-set/seam-ledger/entry-contract/roster/leg-partition blueprint the durable rebuild engine executes. Launch when the artifacts campaign opens (py track 5/5, upstream campaigns treated as landed). Delete after the campaign lands.',
  phases: [
    { title: 'Survey', detail: '5 read-only plane surveyors: register rows re-verified on disk (HOLD/DRIFT/REFUTED), both .api tiers + pyproject census, upstream briefs as landed law', model: 'opus' },
    { title: 'Draft', detail: '4 complete blueprints, one per brief lens (foundations-maximalist, entry-seam-first, package-mining-first, output-first)', model: 'opus' },
    { title: 'Judge', detail: '3 lenses; disqualifying gates: dispositions, foundations-first acyclicity, entry contract, tier naming, integration-first roster', model: 'opus' },
    { title: 'Synthesize', detail: '1 fable agent merges the winning blueprint with every accepted graft', model: 'fable' },
    { title: 'Decide', detail: '1 fable red-team gate: attack, rule every hinge on evidence, then WRITE the DECISION (a-f contract)', model: 'fable' },
    { title: 'Salvage', detail: '4 per-draft miners + 1 overturn auditor against the WRITTEN decision, then 1 fable writing integrator', model: 'opus' },
    { title: 'Refine', detail: '3 sequential fable cold passes over the WRITTEN decision — initial extend/improve, mechanical critique, most-aggressive redteam — each fixing in place; surgical artifacts-page pre-adjustments only where a ruling justifies them', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const BRIEF = (typeof argsIn === 'string' && argsIn.trim()) ? argsIn.trim()
  : (argsIn && typeof argsIn === 'object' && typeof argsIn.brief === 'string' && argsIn.brief.trim()) ? argsIn.brief.trim()
  : 'RASM-PY-ARTIFACTS-BRIEF.md'
const OUT = (argsIn && typeof argsIn === 'object' && typeof argsIn.out === 'string' && argsIn.out.trim()) ? argsIn.out.trim() : 'RASM-PY-ARTIFACTS-DECISION.md'
const SCRATCH = (argsIn && typeof argsIn === 'object' && typeof argsIn.scratch === 'string' && argsIn.scratch.trim()) ? argsIn.scratch.trim() : '.claude/scratch/design-py-artifacts'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'register_verdicts', 'key_facts'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' }, pages_read: { type: 'number' },
  register_verdicts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['row', 'status'], properties: { row: { type: 'string' }, status: { type: 'string', enum: ['HOLD', 'DRIFT', 'REFUTED'] }, note: { type: 'string' } } } },
  key_facts: { type: 'array', items: { type: 'string' } } } }
const DRAFT_SCHEMA = { type: 'object', additionalProperties: false, required: ['blueprint', 'lens', 'page_rows', 'verdicts_disposed', 'evidence_disposed', 'packages_disposed', 'targets_disposed'], properties: {
  blueprint: { type: 'string' }, lens: { type: 'string' }, page_rows: { type: 'number' }, verdicts_disposed: { type: 'number' },
  evidence_disposed: { type: 'number' }, packages_disposed: { type: 'number' }, targets_disposed: { type: 'number' }, thesis: { type: 'string' } } }
const JUDGE_SCHEMA = { type: 'object', additionalProperties: false, required: ['lens', 'scores', 'winner'], properties: {
  lens: { type: 'string' }, winner: { type: 'string' }, rationale: { type: 'string' },
  scores: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['draft', 'gate_dispositions', 'gate_acyclic', 'gate_entry', 'gate_tier', 'gate_roster', 'capability', 'collapse'], properties: {
    draft: { type: 'string' }, gate_dispositions: { type: 'string', enum: ['pass', 'fail'] }, gate_acyclic: { type: 'string', enum: ['pass', 'fail'] },
    gate_entry: { type: 'string', enum: ['pass', 'fail'] }, gate_tier: { type: 'string', enum: ['pass', 'fail'] }, gate_roster: { type: 'string', enum: ['pass', 'fail'] },
    capability: { type: 'number' }, collapse: { type: 'number' }, elegance: { type: 'number' }, grafts: { type: 'array', items: { type: 'string' } } } } } } }
const SYNTH_SCHEMA = { type: 'object', additionalProperties: false, required: ['synthesis', 'base', 'grafts_applied'], properties: {
  synthesis: { type: 'string' }, base: { type: 'string' }, grafts_applied: { type: 'array', items: { type: 'string' } }, open_items: { type: 'array', items: { type: 'string' } } } }
const DECIDE_SCHEMA = { type: 'object', additionalProperties: false, required: ['decision', 'gate_record', 'overturns', 'page_rows', 'legs', 'rulings'], properties: {
  decision: { type: 'string' }, page_rows: { type: 'number' }, legs: { type: 'array', items: { type: 'string' } },
  rulings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['hinge', 'ruling'], properties: { hinge: { type: 'string' }, ruling: { type: 'string' } } } },
  gate_record: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['attack', 'outcome'], properties: { attack: { type: 'string' }, outcome: { type: 'string' }, disposition: { type: 'string' } } } },
  overturns: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'why'], properties: { what: { type: 'string' }, why: { type: 'string' } } } } } }
const MINE_SCHEMA = { type: 'object', additionalProperties: false, required: ['source', 'findings'], properties: {
  source: { type: 'string' }, findings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'evidence'], properties: { what: { type: 'string' }, evidence: { type: 'string' }, belongs: { type: 'string' } } } } } }
const AUDIT_SCHEMA = { type: 'object', additionalProperties: false, required: ['overturn_verdicts', 'silent_drops'], properties: {
  overturn_verdicts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['overturn', 'verdict'], properties: { overturn: { type: 'string' }, verdict: { type: 'string', enum: ['JUSTIFIED', 'OVERZEALOUS', 'PARTIAL'] }, note: { type: 'string' } } } },
  silent_drops: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'evidence'], properties: { what: { type: 'string' }, evidence: { type: 'string' } } } } } }
const INTEGRATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['applied', 'rejected', 'files_touched'], properties: {
  applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['finding', 'where'], properties: { finding: { type: 'string' }, where: { type: 'string' } } } },
  rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['finding', 'why'], properties: { finding: { type: 'string' }, why: { type: 'string' } } } },
  files_touched: { type: 'array', items: { type: 'string' } } } }
const REFINE_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['extended', 'clean'] },
  extended: { type: 'string' }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const PRE = 'Rasm monorepo, python planning corpus. THE BRIEF ' + BRIEF + ' (repo root) is binding law — read it COMPLETELY first: [EXECUTION], ' +
  '[00]-[SHARED_LAW] (the four-proof verdict, telos, structural authority, foundations-first wave law, generator law, owner-boundaries tier law, ' +
  'seam-and-entry law, pyproject reconciliation + the dead-marker census), V1-V16 the binding verdicts with ruled defaults, [02] the evidence ' +
  'register E1-E13, [03] capability escalation targets, [04] package pressure + the roster-closure table + the pvlib admission, [05] the ' +
  'three-wave build-leg sequencing law + the four live inversions, [06] out-of-scope + the gated obligations. Target: ' +
  'libs/python/artifacts/.planning/ (the full page tree) plus the folder README.md/ARCHITECTURE.md, BOTH .api tiers (shared libs/python/.api/ ' +
  'AND the artifacts folder .api/), and root pyproject.toml.\n' +
  'UPSTREAM IS LANDED LAW: the four upstream py campaigns (RASM-PY-RUNTIME-BRIEF.md, RASM-PY-DATA-BRIEF.md, RASM-PY-GEOMETRY-BRIEF.md, ' +
  'RASM-PY-COMPUTE-BRIEF.md at repo root) are treated as DONE state — runtime identity/lane/retry/metric-recorder exports, data tabular ' +
  'column discipline + the expression `Map` rail, geometry mesh/energy planes, compute evidence/handoff hub. artifacts is py track 5/5, the ' +
  'CONSUMER plane: the DECISION composes those contracts as existing law and never re-plans, forks, or locally re-mints them; the [V16] track ' +
  'rebind laws (identity rename, frozendict->Map, [RESEARCH] purge) bind every blueprint.\n' +
  'INTEGRATION-FIRST IS ABSOLUTE: every admitted package was admitted for a purpose — closure is full lib-grade integration at the owning ' +
  'surface, NEVER removal; a zero-consumer admission is a missing-owner gap the page-set closes; functionality and external libs are never ' +
  'dropped — a weak/incorrect/spam surface is rebuilt or restructured to strong form, never deleted. The ONLY sanctioned removal defaults are ' +
  'the brief\'s own [PYPROJECT_RECONCILIATION] rows, each ruled by its brief-stated survival condition (a live consumer landing in the ' +
  'page-set, a process-boundary re-site, a parity landing, a marker resolution) — rule the CONDITION on evidence, never zero-consumer counting; ' +
  'copyleft sits behind a process boundary per the stated posture. A PRUNE table, a removal justified by underutilization, or a capability ' +
  'dropped for lack of a current consumer is a DISQUALIFYING defect in any blueprint and in the DECISION.\n' +
  'LEG PARTITION LAW: legs are DEPENDENCY BARRIERS, never bookkeeping — the [05] three-wave law (FOUNDATIONS -> MID -> AEC/EGRESS) is the ' +
  'shape; the only sanctioned extra split is the brief-named leg-1 1a/1b blast-radius ruling, made explicitly either way; more legs than the ' +
  'dependency structure demands is a partition defect. Every leg is SELF-CONTAINED on the current rebuild engine: its in-run reconcile closes ' +
  'every confirmed residual with whole-repository write authority before the leg returns — the DECISION contains NO post-leg residual steps, ' +
  'NO hard_residual channel, NO between-legs cleanup rows. Per-leg engine invocation rows carry repo-relative ' +
  'libs/python/artifacts/.planning/... targets.\n' +
  'CARD GOVERNANCE IS DEAD repo-wide: IDEAS.md/TASKLOG.md are empty boilerplate — no card disposition tables, no TASKLOG anchors, no ' +
  'BLOCKED-card gates anywhere in dossiers, blueprints, or the DECISION; a finding lands as corpus law (a page row, a seam row, a gated ' +
  'obligation), never a card.\n' +
  'docs/stacks/python/ is the operative doctrine bar for every skeleton, entry signature, and receipt case (py3.15 PEP 585/604/695, expression ' +
  '`Result`/`Option`/`Map`, closed `@tagged_union` families, `msgspec`/`TypedDict` payload law, concurrency only via the runtime lane owner). ' +
  'Member verification: `uv run python -m tools.assay api resolve <pkg>` where it answers; the `.api` catalogs + PyPI feed truth + upstream ' +
  'source via web research otherwise — never training data, never memory. Your stance is HOSTILE: the register is an initial pointer, never a ' +
  'ceiling; prose claiming capability is verified against fences; dense confident pages are the prime suspect. HARDENING LAW: capability is ' +
  'improved or extended, NEVER dropped; zero consumers never lowers the bar. The DECISION is an agent-facing declarative blueprint: no ' +
  '[RESEARCH] appendices, no provenance/freshness tails, no process narration; every open item RULED.\n' +
  'ADMISSIONS ARE ADDITIVE: the roster grows as well as binds — a real capability gap no admitted package serves is ADMISSION PRESSURE, and ' +
  'the proper close is a feed-verified new admission with its `.api` obligation in the same motion (the brief\'s pvlib row is the exemplar), ' +
  'never a hand-rolled kernel where the ecosystem owns the domain, and never admission for admission\'s sake; underutilization of an admitted ' +
  'package is integration pressure on its owning surface, both directions weighed in every roster ruling.\n' +
  'THE ENERGY DATA FLOW IS REAL CONSUMER PRESSURE: the upstream energy band (the geometry energy/climate planes, compute evidence frames, the ' +
  'honeybee/ladybug result surfaces) flows INTO artifacts as self-describing `data/tabular` frames — sun-path and climate furniture (V15), ' +
  'energy charts and result-frame tables (V10), and award-winning architectural diagram suites (V13/V15) at the grade top offices publish, ' +
  'never low-grade technical plots; the DECISION treats that ingress as a DEMANDING NAMED CONSUMER of the visualization/diagram/table planes, ' +
  'and the same standard binds every sibling cross-plane flow (geospatial frames, compute study evidence, QTO rows).'
const PY_DOCTRINE = 'READ docs/stacks/python/ IN FULL before writing: enumerate the root with a real ls, then read EVERY root page top-to-bottom ' +
  'in the README [01]-[ATLAS] routing order (language, shapes, iteration, surfaces-and-dispatch, rails-and-effects, concurrency, boundaries, ' +
  'algorithms, system-apis, runtime) — never a partial, skim, or section-sample; the skeletons and signatures you author must be exactly what ' +
  'that doctrine admits.'
const ENGINE_VOCAB = 'ENGINE-NATIVE ACTIONS: the page-set table carries TWO action columns — the semantic action (KEEP/REBUILD/SPLIT/MERGE/MOVE/' +
  'DELETE/NEW) AND its rebuild-engine lowering (kind new|rebuild|improve, deletePages rows, absorb pairs {into, from}) so the durable rebuild ' +
  'engine executes the DECISION unmodified: a SPLIT lowers to N new pages + one delete-with-absorb; a MERGE lowers to one absorber + ' +
  'delete-with-absorb rows; a MOVE lowers to a new page at the destination + delete-with-absorb at the source; KEEP lowers to improve.'
const ROW_SCHEMA = 'PAGE-ROW SCHEMA (every row total): path | semantic action + engine lowering | owner charter with its [OWNER_BOUNDARIES] ' +
  'tier (generate/bind/select where the page touches the triads, N/A elsewhere) and per-page skeleton (owner roster, key shapes) | entry ' +
  'signature (`emit() -> ArtifactWork | Iterable[ArtifactWork]` for producer pages with the receipt case named; NONE for substrate/vocabulary ' +
  'pages) | in/out seam edges (one anchor per seam, both ledger directions) | wave + leg. Plus: the corrected seam ledger (every cross-domain ' +
  'import a ledger edge, every edge composed in a fence, the whole graph acyclic with the four live inversions disposed); the `ArtifactWork` ' +
  'entry contract as law with the `core/plan` constructing owner named; the package roster delta table with per-admission `.api` obligations ' +
  '(integration-first; every [PYPROJECT_RECONCILIATION] census row ruled by its condition; the pvlib admission ruled); verdict (V1-V16), ' +
  'evidence (E1-E13), [03] capability-target, and [04] package-row disposition tables; the [06] gated-obligation rulings (realize or keep ' +
  'gated with the blocker named — never silent). A blueprint leaving ANY disposition open is INCOMPLETE and loses at judge.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (lane, scope, rows) => [PRE, 'TASK: READ-ONLY SURVEY, lane = ' + lane + '. Scope: ' + scope + '. Deep-read fully — never ' +
  'a skim. Re-verify ON DISK every register row assigned: ' + rows + ' — HOLD (anchor exact), DRIFT (real defect, anchor moved — corrected ' +
  'anchor), or REFUTED (disk falsifies — cite the line). Beyond the register: both naivety axes (COVERAGE thin slices, APPROACH enumerated ' +
  'rosters where generators belong), unwired seams, duplicate mechanisms, dead carriers, phantom members (verify every load-bearing member ' +
  'claim in your lane via assay/the .api catalogs/feed truth), prose-vs-fence drift. WRITE the dossier to ' + SCRATCH + '/dossier-' +
  '{lane}.md (per-page verdicts + defects with anchors + charter-as-it-should-be; cross-cutting findings; integration homes for every ' +
  'orphan-looking admission in your lane — an integration TARGET, never a removal candidate). Dense, evidence-first. NO edits outside ' +
  SCRATCH + '/.'].join('\n')
const draftPrompt = (lens, thesis, dossiers) => [PRE, PY_DOCTRINE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: author ONE COMPLETE structural blueprint ' +
  'under the ' + lens + ' lens: ' + thesis + ' Read the brief in full and every survey dossier: ' + JSON.stringify(dossiers) + '. Honor every ' +
  'ruled default unless you carry disk proof a departure is stronger; every deciding criterion the brief names resolves to a ruling, never a ' +
  'hedge; every producer page walks to its `emit()` signature and receipt case; every [05] inversion is disposed (re-home or re-wave, stated). ' +
  'WRITE the blueprint to ' + SCRATCH + '/draft-' + lens + '.md. Self-report exact counts. NO edits outside ' + SCRATCH + '/.'].join('\n')
const judgePrompt = (lens, focus, drafts) => [PRE, 'TASK: ADVERSARIAL JUDGE, lens = ' + lens + '. ' + focus + ' Read the brief and all four ' +
  'blueprints: ' + JSON.stringify(drafts) + '. Score each: gate_dispositions (EVERY V1-V16 / E1-E13 / [03] target / [04] package row / [06] ' +
  'gate disposed — count yourself), gate_acyclic (walk the seam ledger + waves: every edge within-wave or earlier per [FOUNDATIONS_FIRST], ' +
  'zero page-level import cycles, the four live inversions explicitly disposed), gate_entry (every producer row carries the `ArtifactWork` ' +
  '`emit()` + receipt case; the `core/plan` constructing owner named; the six legacy carrier conventions collapsed), gate_tier (every triad-' +
  'touching page names its generate/bind/select tier), gate_roster (integration-first holds: zero prune framing, every census row ruled by ' +
  'its brief condition, every remaining admission bound to an owning surface). A gate fail DISQUALIFIES. Capability 0-10, collapse 0-10, ' +
  'elegance 0-10. Name the GRAFTS precisely. Winner = highest capability among gate-passers; collapse then elegance tiebreak. Read-only ' +
  'outside ' + SCRATCH + '/.'].join('\n')
const synthPrompt = (drafts, judges) => [PRE, PY_DOCTRINE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: SYNTHESIZE the blueprint of record. Judges: ' +
  JSON.stringify(judges) + '. Blueprints: ' + JSON.stringify(drafts) + '. Base = consensus winner; apply every graft two judges named and every ' +
  'single-judge graft surviving your own disk check; re-verify the merge stays disposition-complete, acyclic, entry-complete, tier-named, and ' +
  'integration-first. WRITE ' + SCRATCH + '/synthesis.md in the full row schema. Return pointer, base, grafts, open items. NO edits outside ' +
  SCRATCH + '/.'].join('\n')
const decidePrompt = (synthesis) => [PRE, PY_DOCTRINE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: RED-TEAM DECIDE GATE + EMIT. Attack the synthesis at ' +
  synthesis + ': walk the ledger and prove acyclicity or break it; audit every V1-V16 disposition; spot-verify 10+ factual claims on disk ' +
  'before trusting them; attack the leg partition for blast radius and for SPAM (a leg the dependency structure does not demand dies; the ' +
  'leg-1 1a/1b ruling is made explicitly either way); attack every departure from a ruled default; attack every removal ruling against its ' +
  'brief condition — an unconditioned removal reverts to integration. THE HINGE RULINGS ARE YOURS, evidence-bound, recorded in rulings and ' +
  'baked into the affected rows: V2 pattern siting (in-folder vs standalone), V5 mark siting, V9 scene build-vs-defer (with the drawing-' +
  'content ingress boundary stated either way), V13 style-owner siting, V15 solar (pvlib admission vs owned closed-form kernel), the ' +
  'grandalf-on-parity / pdf-arm / iptcinfo3 / python-xmp-toolkit / pyexiv2 / PyICU census rulings, the [06] measured-signals and ' +
  'figure-hand-off gated obligations (realize or keep gated, blocker named), and the leg-1 split. Then WRITE ' + OUT + ' (repo root) carrying ' +
  'exactly the brief [01] emit contract: (a) the final page-set table (full row schema, both action vocabularies, per-page skeletons dense ' +
  'enough that a build agent starts without re-deriving structure); (b) the corrected seam ledger; (c) the `ArtifactWork` entry contract as ' +
  'law with its constructing owner; (d) package roster deltas with `.api` obligations; (e) the leg partition proven acyclic with per-leg ' +
  'rebuild-engine invocation rows (repo-relative targets; self-contained legs, no residual steps); (f) the gated-obligation rulings. ' +
  'Agent-facing declarative; every open item RULED; no [RESEARCH] tails. The DECISION stands alone without ' + SCRATCH + '/.'].join('\n')
const minePrompt = (draft, decision) => [PRE, 'TASK: SALVAGE MINER. Compare ' + draft + ' against the WRITTEN decision ' + decision + ' (both ' +
  'fully). Hunt content the draft carried that the DECISION lacks or thins: mechanisms, charters, seam rows, entry signatures, package ' +
  'integrations, tier namings, gated-obligation dispositions. A finding names WHAT, the draft evidence, and WHERE it belongs as an owner ' +
  'extension — and must be STRONGER than what the DECISION holds. Read-only.'].join('\n')
const auditPrompt = (decision, gateRecord) => [PRE, 'TASK: OVERTURN AUDITOR. Gate record: ' + gateRecord + '. Against ' + decision + ' and the ' +
  'synthesis + drafts in ' + SCRATCH + '/, classify every overturn JUSTIFIED / OVERZEALOUS (cite the evidence) / PARTIAL. Sweep for SILENT ' +
  'drops — a draft capability, package integration, or gated obligation the decision neither carries nor explicitly rejects. Spot-verify ' +
  'factual premises on disk. Read-only.'].join('\n')
const integratePrompt = (decision, findings) => [PRE, PY_DOCTRINE, 'TASK: WRITING INTEGRATOR — the salvage close. Findings: ' + findings + '. ' +
  'Judge each on evidence; ACCEPT what is genuinely stronger (convergent findings strongest; OVERZEALOUS/PARTIAL verdicts restore corrected ' +
  'content), REJECT with the falsifying line named. Apply accepted findings IN PLACE in ' + OUT + ' as owner extensions — never a parallel ' +
  'section. Keep the DECISION disposition-complete, acyclic, entry-complete, tier-named, and integration-first after every edit.'].join('\n')
const REFINE_LAW = 'REFINE SCOPE: the WRITTEN DECISION at ' + OUT + ' (repo root) is the PRIMARY surface — improve, correct, clarify, and ' +
  'EXTEND it IN PLACE (never a parallel section, never a regression of a ruling without disk proof). SECONDARY surface: ' +
  'libs/python/artifacts/.planning/ pages (and their README/ARCHITECTURE) may be adjusted PRE-EMPTIVELY where a DECISION row makes a ' +
  'leverage/integration adjustment genuinely appropriate NOW — surgical, justified by the named row, never forced, never noise; when in doubt, ' +
  'the DECISION row alone carries it. Ground every judgment: read ' + OUT + ' + the brief ' + BRIEF + ' in FULL, the four upstream briefs as ' +
  'landed law, and every artifacts page a judgment touches. EXTENSION TARGETS (justified only, never speculative): the energy/geospatial/' +
  'compute-evidence data flows into the visualization/diagram/table planes at the award-winning output bar; package ADMISSIONS where a real ' +
  'capability gap demands one (feed-verified, `.api` obligation in the same motion); skeleton density where a build agent would otherwise ' +
  're-derive structure; guidance correctness everywhere.'
const refineInitialPrompt = () => [PRE, PY_DOCTRINE, REFINE_LAW, 'TASK: REFINE-INITIAL — the first cold pass over the WRITTEN DECISION. ' +
  'DISBELIEVE it: hunt naivety (both axes — COVERAGE thin rulings, APPROACH enumerated rosters where a generator/derivation belongs), ' +
  'incorrect or ambiguous guidance a build agent could misread, thin per-page skeletons, missed extensions, missed admissions, and any ' +
  'under-served cross-plane data flow (energy above all — result frames into charts/tables/diagram suites at the published-work grade). FIX ' +
  'and EXTEND in place; WRITE-FULLY now. Return files (every file edited), verdict, extended (what grew), summary.'].join('\n')
const refineCritiquePrompt = () => [PRE, PY_DOCTRINE, REFINE_LAW, 'TASK: REFINE-CRITIQUE — the mechanical audit pass, ultra-harsh and ' +
  'unagreeable. Line-by-line over ' + OUT + ': every ruling evidence-bound (spot-verify anchors on disk); every count internally consistent ' +
  '(page rows vs leg targets vs disposition tables); the seam ledger acyclic with the four inversions disposed; every leg invocation row ' +
  'executable verbatim (repo-relative targets, kinds matching the action lowering, self-contained — zero residual steps); the entry contract ' +
  'total over every producer row; integration-first held on every roster row (an unconditioned removal is a defect to overturn); the energy/' +
  'geospatial ingress named on its consumer rows; zero [RESEARCH]/provenance/process noise; prose hygiene (declarative, backticked symbols, ' +
  'no hedges). REPAIR every hit in place. Return files, verdict, extended, summary.'].join('\n')
const refineRedteamPrompt = () => [PRE, PY_DOCTRINE, REFINE_LAW, 'TASK: REFINE-REDTEAM — the LAST and MOST AGGRESSIVE pass; assume the ' +
  'earlier passes missed things. (A) COUNTERFACTUAL on the page-set and partition: is a categorically stronger structure or leg shape ' +
  'available on the evidence? If so, rebuild the affected rows — never defend the incumbent. (B) DIFF-OF-NEXT-FEATURE: the next diagram kind, ' +
  'theme row, energy result surface, pattern material, or export target must land as ONE row/case with consumers untouched — reshape any row ' +
  'where it would not. (C) PHANTOM HUNT: spot-verify 10+ member/anchor claims against disk and the .api catalogs; a claim that fails is ' +
  'corrected or deleted. (D) COLD RE-REVIEW of every section with fresh hostile eyes — rulings, skeletons, ledger, roster, legs, gated ' +
  'obligations. FIX everything in place; the DECISION must end objectively stronger, denser, and more correct than the critique left it — or ' +
  'prove the strongest form is present by finding nothing. Return files, verdict, extended, summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [SURVEY]
phase('Survey')
const LANES = [
  { lane: 'foundations', scope: 'libs/python/artifacts/.planning/ graphic/ (vector, pattern-adjacent, color/, raster/, marks/) + typography/ + ' +
    'core/ pages FULLY, plus the folder README.md/ARCHITECTURE.md', rows: 'E1 (the core/plan half), E2, E3 (the typography half), E4 (the ' +
    'no-SVG-fill-owner claim), E9 (the receipt stringly-kind half), E10, and every register anchor in your pages' },
  { lane: 'mid-plane', scope: 'libs/python/artifacts/.planning/ visualization/ + scene/ + composition/ + document/ pages FULLY', rows: 'E1 ' +
    '(the emit/report/format rails), E5 (the table render form), E6 (the press fold + Pdf.from_html + egress algebra), E7, E11, E12 (the ' +
    'sheet/emit gaps), and every register anchor in your pages' },
  { lane: 'aec-egress', scope: 'libs/python/artifacts/.planning/ drawing/ + specification/ + delivery/ + export/ + exchange/ + media/ + ' +
    'package/ pages FULLY', rows: 'E3 (the drawing half), E4, E5 (the register compose + media/analysis imports), E6 (the terminator/symbol/' +
    'register/AdaptMethod duplications), E9, E12 (the draw/glyphset/layout gaps), and every register anchor in your pages' },
  { lane: 'api-census', scope: 'BOTH .api tiers COMPLETE (the shared catalogs under libs/python/.api/ AND the artifacts folder .api/ catalogs), ' +
    'the root pyproject.toml artifacts rows, and the [PYPROJECT_RECONCILIATION] dead-marker census — verify each census row and each [04] ' +
    'mine-to-depth member claim against the catalogs/assay/feed truth, the pvlib admission row included', rows: 'the eight census rows, every ' +
    '[04] load-bearing member claim, E13 (the frozendict-is-not-a-builtin premise), and every roster-closure row' },
  { lane: 'upstream-law', scope: 'the four upstream briefs at root (RASM-PY-RUNTIME-BRIEF.md, RASM-PY-DATA-BRIEF.md, RASM-PY-GEOMETRY-BRIEF.md, ' +
    'RASM-PY-COMPUTE-BRIEF.md) read as LANDED LAW, the artifacts ARCHITECTURE.md seam ledger, and the [V16]/[06] contact surface — re-count the ' +
    'track-law debt on disk (content_identity spellings, frozendict import sites, [RESEARCH] tails) and verify each [06] gated obligation ' +
    'against the upstream verdict text it gates on', rows: 'E13 (the counts), E7/E10 (the ARCH-side rows), and every upstream-anchored claim in the brief' },
]
const surveyed = (await parallel(LANES.map((l) => () =>
  agent(surveyPrompt(l.lane, l.scope, l.rows), { label: 'survey:' + l.lane, phase: 'Survey', model: 'opus', effort: 'max', schema: SURVEY_SCHEMA, stallMs: STALL })))).filter(Boolean)
const refuted = surveyed.flatMap((d) => (d.register_verdicts || []).filter((v) => v.status !== 'HOLD').map((v) => d.lane + ':' + v.row + '=' + v.status))
log('Survey: ' + surveyed.length + '/5 dossiers; register drift: ' + (refuted.length ? refuted.join('; ') : 'none'))

// --- [DRAFT]
phase('Draft')
const DOSSIER_PATHS = surveyed.map((d) => d.dossier)
const LENSES = [
  { lens: 'foundations-maximalist', thesis: 'push maximum capability into the foundational plane (vector folder + pattern + typography ' +
    'completion + color re-homing + LayerPlan + style owner) so every consumer composes, never re-derives — then fit the mid and AEC planes ' +
    'onto those foundations.' },
  { lens: 'entry-seam-first', thesis: 'work BACKWARD from the ArtifactWork entry contract and the corrected seam ledger — one producer ' +
    'contract corpus-wide, core/plan constructed and satisfiable, every cross-domain import a composed ledger edge, the six legacy carriers ' +
    'collapsed — to the page structure that makes the contract real.' },
  { lens: 'package-mining-first', thesis: 'work OUTWARD from the [04] package-pressure obligations and the roster-closure table — every ' +
    'admitted package bound to an owning surface at operator depth, every census row ruled, the shared-tier weave (expression/runtime lane/' +
    'identity/receipt) composed in every fold — to the page-set that houses them.' },
  { lens: 'output-first', thesis: 'work BACKWARD from the flagship artifacts — a full issued sheet set, a portfolio AEC diagram suite (all six ' +
    'DiagramKinds at the published-work grade), an editorial document package — through V13 themes, V15 machinery, and V7/V12 completion to ' +
    'the structure that produces them.' },
]
const drafts = (await parallel(LENSES.map((l) => () =>
  agent(draftPrompt(l.lens, l.thesis, DOSSIER_PATHS), { label: 'draft:' + l.lens, phase: 'Draft', model: 'opus', effort: 'max', schema: DRAFT_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Draft: ' + drafts.length + '/4 blueprints; rows [' + drafts.map((d) => d.page_rows).join(', ') + ']; dispositions V[' +
  drafts.map((d) => d.verdicts_disposed).join(',') + '] E[' + drafts.map((d) => d.evidence_disposed).join(',') + '] pkgs[' +
  drafts.map((d) => d.packages_disposed).join(',') + '] targets[' + drafts.map((d) => d.targets_disposed).join(',') + ']')

// --- [JUDGE]
phase('Judge')
const DRAFT_PATHS = drafts.map((d) => d.blueprint)
const JUDGES = [
  { lens: 'law-gates', focus: 'Own the disqualifying gates hardest: dispositions counted row-by-row, foundations-first acyclicity + the four ' +
    'live inversions walked edge-by-edge, tier namings checked per triad page, integration-first proven per roster row, ruled-default ' +
    'departures without disk proof.' },
  { lens: 'contract-reach', focus: 'Own the contract gates hardest: walk EVERY producer page to its emit() signature and receipt case; walk ' +
    'the ArtifactWork constructing owner end to end; walk the upstream contact surface (runtime lane/retry/identity/metric, data tabular ' +
    'discipline, compute handoff, geometry mesh ingress) and verify each composes an upstream export, never a local re-mint; verify the [06] ' +
    'gated obligations are ruled, never silently carried.' },
  { lens: 'partition-elegance', focus: 'Own partition quality: the three-wave law honored with zero spam legs, the leg-1 1a/1b blast-radius ' +
    'ruling made explicitly, leg balance, over- vs under-splitting of pages and folders, MOVE/MERGE churn honesty, flagship-acceptance ' +
    'reachability after leg 3.' },
]
const judges = (await parallel(JUDGES.map((j) => () =>
  agent(judgePrompt(j.lens, j.focus, DRAFT_PATHS), { label: 'judge:' + j.lens, phase: 'Judge', model: 'opus', effort: 'xhigh', schema: JUDGE_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Judge: ' + judges.length + '/3; winners [' + judges.map((j) => j.lens + '->' + j.winner).join(', ') + ']')

// --- [SYNTHESIZE]
phase('Synthesize')
const synth = await agent(synthPrompt(DRAFT_PATHS, JSON.stringify(judges)), { label: 'synthesize', phase: 'Synthesize', model: 'fable', effort: 'max', schema: SYNTH_SCHEMA, stallMs: STALL })
if (!synth) { log('Synthesize produced nothing — aborting before Decide; resume re-runs it.'); return { dossiers: DOSSIER_PATHS, drafts: DRAFT_PATHS, aborted: 'synthesize' } }
log('Synthesize: base=' + synth.base + ', ' + (synth.grafts_applied || []).length + ' grafts, ' + (synth.open_items || []).length + ' open items')

// --- [DECIDE]
phase('Decide')
const decided = await agent(decidePrompt(synth.synthesis), { label: 'decide', phase: 'Decide', model: 'fable', effort: 'max', schema: DECIDE_SCHEMA, stallMs: STALL })
if (!decided) { log('Decide produced nothing — DECISION not written; resume re-runs it.'); return { dossiers: DOSSIER_PATHS, synthesis: synth.synthesis, aborted: 'decide' } }
log('Decide: ' + decided.decision + ' — ' + decided.page_rows + ' rows, legs [' + (decided.legs || []).join(' | ') + '], rulings [' +
  (decided.rulings || []).map((r) => r.hinge + ': ' + r.ruling).join(' | ') + '], ' + (decided.overturns || []).length + ' overturn(s)')

// --- [SALVAGE]
phase('Salvage')
const mined = (await parallel(DRAFT_PATHS.map((p) => () =>
  agent(minePrompt(p, decided.decision), { label: 'mine:' + p.split('draft-').pop().replace('.md', ''), phase: 'Salvage', model: 'opus', effort: 'high', schema: MINE_SCHEMA, stallMs: STALL })))).filter(Boolean)
const audit = await agent(auditPrompt(decided.decision, JSON.stringify({ gate_record: decided.gate_record, overturns: decided.overturns })),
  { label: 'overturn-audit', phase: 'Salvage', model: 'opus', effort: 'max', schema: AUDIT_SCHEMA, stallMs: STALL })
const integrated = await agent(integratePrompt(decided.decision, JSON.stringify({ miners: mined, audit: audit || { overturn_verdicts: [], silent_drops: [] } })),
  { label: 'integrate', phase: 'Salvage', model: 'fable', effort: 'max', schema: INTEGRATE_SCHEMA, stallMs: STALL })
log('Salvage: ' + mined.reduce((n, m) => n + (m.findings || []).length, 0) + ' mined, ' + ((audit && audit.silent_drops) || []).length +
  ' silent drop(s), applied ' + ((integrated && integrated.applied) || []).length + ' / rejected ' + ((integrated && integrated.rejected) || []).length)

// --- [REFINE]
phase('Refine')
const refineInitial = await agent(refineInitialPrompt(), { label: 'refine:initial', phase: 'Refine', model: 'fable', effort: 'max', schema: REFINE_SCHEMA, stallMs: STALL })
const refineCritique = await agent(refineCritiquePrompt(), { label: 'refine:critique', phase: 'Refine', model: 'fable', effort: 'xhigh', schema: REFINE_SCHEMA, stallMs: STALL })
const refineRedteam = await agent(refineRedteamPrompt(), { label: 'refine:redteam', phase: 'Refine', model: 'fable', effort: 'max', schema: REFINE_SCHEMA, stallMs: STALL })
const refine = [refineInitial, refineCritique, refineRedteam].filter(Boolean)
log('Refine: ' + refine.length + '/3 passes [' + refine.map((r) => r.verdict).join(', ') + ']; files touched ' +
  [...new Set(refine.flatMap((r) => r.files || []))].length)

return {
  decision: decided.decision, page_rows: decided.page_rows, legs: decided.legs,
  rulings: decided.rulings, register_drift: refuted,
  judges: judges.map((j) => ({ lens: j.lens, winner: j.winner })), synthesis: synth.synthesis,
  salvageApplied: (integrated && integrated.applied) || [], salvageRejected: (integrated && integrated.rejected) || [],
  refine: refine.map((r) => ({ verdict: r.verdict, summary: r.summary })),
  scratch: SCRATCH,
}
