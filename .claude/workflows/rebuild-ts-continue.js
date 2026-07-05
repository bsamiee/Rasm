export const meta = {
  name: 'rebuild-ts-continue',
  description: 'Continuation of the rebuild-ts campaign entering at the improve stage: census + research dossiers already landed on disk under .claude/scratch/rebuild-ts/, so each lane runs improve -> critique -> red-team directly against them, then the terminal fable align and the WRITING verify close. Same mandate, same prompts, zero cache dependence.',
  whenToUse: 'Dispatch only when all census-*.md, stack-*.md, and audit-*.md dossiers exist in .claude/scratch/rebuild-ts/. Ephemeral - delete after the campaign lands.',
  phases: [
    { title: 'Lanes' },
    { title: 'Close' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 8
const STAGGER_MS = 1500
const STALL = 300000
const ROOT = 'libs/typescript'
const SCRATCH = '.claude/scratch/rebuild-ts'
const FOLDERS = ['core', 'security', 'data', 'runtime', 'ui', 'iac']
const TOOLING = 'the TS tooling estate: root tsconfig.json + tsconfig.base.json, biome.json, nx.json, ' +
  'vite.config.ts + vite.factory.ts, vitest.config.ts, playwright.config.ts, stryker-config.json + ' +
  'stryker.config.json (rule the duplicate pair down to one), the TS rows of root package.json, ' + ROOT +
  '/package.json exports + project.json tag triples, and the whole tests/typescript/ estate (_testkit, ' +
  '_architecture gauges, e2e, the .api dev tier) + tests/contracts/'

// --- [INPUTS] ----------------------------------------------------------------------------

const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const wanted = Array.isArray(argsIn) ? argsIn
  : (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.targets)) ? argsIn.targets
  : (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn.trim()] : null
const ACTIVE = wanted ? FOLDERS.filter((f) => wanted.some((w) => String(w).indexOf(f) >= 0)) : FOLDERS
const LANE_KEYS = ACTIVE.concat(
  (!wanted || wanted.some((w) => String(w).indexOf('tooling') >= 0)) ? ['tooling'] : [])

// --- [MODELS] ----------------------------------------------------------------------------

const FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] },
  collapsed: { type: 'string' }, extended: { type: 'string' },
  packageAsks: { type: 'array', items: { type: 'string' } },
  residuals: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const ALIGN_OUT = { type: 'object', additionalProperties: false, required: ['fixes', 'summary'], properties: {
  fixes: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const ACCEPT_OUT = { type: 'object', additionalProperties: false, required: ['fixes', 'unresolved', 'summary'], properties: {
  fixes: { type: 'array', items: { type: 'string' } },
  unresolved: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const CONTEXT = 'Rasm monorepo - libs/typescript planning corpus (markdown design pages whose code fences are the ' +
  'work product; six capability folders core/security/data/runtime/ui/iac in five dependency waves). ' +
  'docs/stacks/typescript composed IN FULL is the bar - author ultra-advanced TypeScript only, discarding naive ' +
  'idioms wholesale; the doctrine is the FLOOR this campaign pushes past.'

const MANDATE = 'CAMPAIGN LAW - THE ULTRA REBUILD. Every fence in libs/typescript is collapsed AND extended in place ' +
  'to a 13/10 world-class bar: fewer, denser, richer owners; reduced surface and LOC through polymorphic collapse ' +
  'with capability GROWN, never lost. BINDING EMPHASES: ' +
  '(1) SHAPE WAR - no mini schemas, no useless structs, no loose type/const spam: deep nested Schema class families ' +
  'with embedded sub-schemas, brand-in-field refinements, class-carried methods and statics; full ADT (tagged ' +
  'unions + exhaustive Match), aspect-oriented composition at the definition seam, total parameterization - zero ' +
  'hardcoded values, zero stringy/fragile plumbing (vocabulary tables, TemplateLiteralParser, keyof-typeof proven ' +
  'keys). Functionality injects INTO owners - overloads, Function.dual twins, class statics, derived projections - ' +
  'never beside them. CLASS-FIRST: a module-level type alias, interface, or bare Struct standing where a ' +
  'Schema.Class/TaggedClass/TaggedError family could carry invariants, statics, derived projections, and behavior ' +
  'is a defect; an embedded concept with its own invariants is its OWN class composed as a field at full depth; ' +
  'Schema.Struct survives ONLY as an anonymous single-consumer field block; Data.taggedEnum owns process-local case ' +
  'families; machines and statecharts are data-carried owners, never switch ladders. Weak structs, option-bag ' +
  'interfaces, and loose module-level type/const exports collapse into their owning families. ' +
  '(2) SINGLE ENTRY POINTS - each folder exposes a small set of deep owners whose internally integrated intelligence ' +
  'is automatic: retries, breakers, telemetry, caching, batching, validation ride the owner so a future consumer ' +
  'composes capability, never plumbing. Imagine the MOST COMPLEX POSSIBLE apps and serve them easily. ' +
  '(3) THOUSANDS OF APPS, NOT A MEGA-APP - every pipeline (telemetry, diagnostics, meters, journals, caches) stays ' +
  'per-app/per-tenant sound: identities scope every stream, nothing tangles when many apps share the library; ' +
  'telemetry and diagnostics grow consumer HOOKS (taps, processors, exporters, redaction points) a project plugs ' +
  'into without forking the plane. ' +
  '(4) MACHINES AT RESEARCH-PAPER DEPTH - statecharts as data (hierarchical/parallel regions, guarded transitions, ' +
  'timers, history), the @effect/experimental Machine serializable actor with snapshot/restore, wired to ' +
  'Subscribable UIs and durable workflows - one deep machine owner, usable everywhere, never a toy transition table. ' +
  '(5) PUBSUB + STREAMING SECOND TO NONE - one pubsub surface owning every topology (in-process PubSub/replay, ' +
  'cross-process JetStream row, browser BroadcastChannel lane) behind one parameterized owner; streaming ' +
  'bidirectional where the transport admits it (Channel duplex, Socket, SSE mirror), chunking/backpressure/resume ' +
  'built in, throughput AND resilience optimized. ' +
  '(6) RESILIENCE AS A CONCEPT - circuit breakers, bulkheads, hedging, load-shed, adaptive retry ride EVERY cache, ' +
  'store, backend, and egress owner internally (Effect primitives + Schedule algebra), never bolted on by consumers. ' +
  '(7) NO NAIVE SQL ANYWHERE - every query rides SqlSchema/SqlResolver typed rails with batching/caching; PG 18.4 ' +
  'maximized: native capabilities (uuidv7, virtual generated columns, RETURNING old/new, temporal constraints, skip ' +
  'scan, async IO), the ruled extension matrix, LISTEN/NOTIFY + SKIP LOCKED composed correctly, richly-defaulted ' +
  'intelligent DDL - the most world-class databases out of the gate. ' +
  '(8) FILESYSTEM + CLOUD ESCALATED - local AND remote file control as one parameterized surface: FileSystem/watch, ' +
  'object storage, transfer (tus/multipart/Range), SSH/remote-exec/VPS lanes via Command + Socket, sync between ' +
  'origins - a complex cloud project needs zero new plumbing. ' +
  '(9) UI READY FOR THE MOST ADVANCED, BEAUTIFUL WEB APPS - the system/view/viewer owners internalize interaction, ' +
  'motion, intl, data-grid, forms, overlays, spatial rendering at full depth with the atom bridge as the one ' +
  'graph seam; the React CANARY channel is admitted - bleeding-edge capability (View Transitions included) ' +
  'integrates on merit, never rejected for channel alone. ' +
  '(10) IAC AS A WORLD-CLASS PRODUCT OF ITS OWN - two-to-three-times the depth of a full app: bootstrap, ' +
  'single-tenant, multi-tenant, K8s and non-K8s, provisioning, secrets, observability stacks, policy, drift - ' +
  'deployment of anything becomes a breeze, intelligent and resilient. ' +
  '(11) TOOLING AS LAW - the doctrine README [04]-[RULE_ENFORCEMENT] four-gate stack made real at current stable ' +
  'tool versions: the full strictness flag set on the dual compiler floor (tsgo + tsc), Biome as the SOLE lint rail ' +
  'with doctrine-breaking anti-patterns promoted as GritQL plugin rules at error, the package exports map as the ' +
  'structural boundary gate, and the tests/typescript gauges owning what resolution cannot express - a doctrine law ' +
  'with no mechanical gate gets a plugin rule or a gauge; the mutation/coverage/e2e estate held to the tests plane ' +
  'own established law. ' +
  'CHANNEL LAW - canary, beta, and pre-release channels are admissible wherever the bleeding edge genuinely adds ' +
  'capability: nothing is rejected for channel alone; a channel candidate is judged on capability delta, ' +
  'maintenance signal, and integration merit, pinned at an exact version with the channel choice and its typing ' +
  'posture recorded in the catalog. ' +
  'NEW FILES are allowed in existing folders when a genuine new owner is earned (a real concern with no home) - ' +
  'never to scatter an existing concern. Buildout over removal: capability is dropped only as an explicit ruled ' +
  'kill; phantoms are the sole silent deletion.'

const READ_FIRST = 'READ FIRST, IN ORDER, AT SOURCE, BEFORE ANY EDIT. (1) DOCTRINE - enumerate docs/stacks/' +
  'typescript/ with a real ls (never memory), read the README and EVERY page it routes IN FULL in atlas order - ' +
  'every card, every snippet, never a skim. Then STATE the complete README [02]-[DOCTRINE] law set by name - all ' +
  'sixteen across FLOW, SHAPE, DERIVATION, MATERIAL, INTEGRATION - each with a one-line reading of how it bears on ' +
  'your scope; a law you cannot state is a law you have not read. ' +
  '(2) .API - ls BOTH catalog tiers in full (libs/typescript/.api/ AND the folder .api/) and read every catalog ' +
  'your scope composes; ULTRA-STACKING IS LAW: layer the Effect substrate end-to-end ON TOP OF the folder domain ' +
  'packages, never the folder set alone; an admitted capability no fence exploits is a defect to close; a member ' +
  'unverifiable against the catalogs or node_modules declarations is a phantom to delete. ' +
  '(3) AUTHORING LAW - libs/.planning/README.md (page grammar, card law, banned hedges) and docs/standards/' +
  'style-guide.md.'

const PAGE_CRAFT = 'PAGE CRAFT - design pages are DESIGN DOCUMENTS under libs/.planning/README.md [03], never ' +
  'doctrine shards; the gold structural exemplar is the libs/csharp/Rasm page corpus - read two of its pages cold ' +
  'before editing any page and match that altitude. A page is one declarative lead, a cluster index, per-cluster ' +
  'cards whose lines are EARNED (owner, packages, growth, boundary - only where each decides something), and then ' +
  'ONE dominant transcription-complete fence per cluster carrying FULL implementation-ready code: real bodies, ' +
  'complete logic, copy-ready - never signatures alone, never logic broken across mini fences, never a fence ' +
  'minted for a wrapper or a low-value type. BANNED ON SIGHT and smashed where found: doctrine-style ' +
  'Law:/Use:/Accept:/Reject: line spam mirroring the docs/stacks prose construction (that grammar belongs to the ' +
  'doctrine corpus, not to design pages), template card fields filled with nothing to decide, prose restating what ' +
  'the fence already states, and fence fragmentation that turns one owner into snippet confetti. The fence mass ' +
  'dominates the page; the prose is short, structural, and load-bearing.'

const STANCE = 'STANCE - every pass is hostile. Hold every fence naive, shallow, or illusory until it survives a ' +
  'real attack; most confident-looking code is naive JavaScript-in-TypeScript dressed in the right vocabulary. ' +
  'NAIVETY is a defect on two axes: COVERAGE (a 2-case family for a 20-case domain, three fields where the concept ' +
  'carries fifteen) and APPROACH (an enumerated roster where one parameterized generator should generate the space - ' +
  'the roster demotes to seed DATA). ILLUSORY code is the primary target: any, unsafe as, non-null assertion, a ' +
  'phantom member, a name promising capability the body omits. Every collapse-signal list is a FLOOR. A no-edit ' +
  'verdict is earned only by an attack that finds nothing.'

const WRITE_FULLY = 'WRITE FULLY - every fix you identify you make NOW; the fix-log reports edits already made. A ' +
  'cross-file ripple your edit causes is YOURS in the same pass: repair the seam counterpart surgically at the ' +
  'symbol anchor, wire-canonical names frozen (the branch ARCHITECTURE.md seam ledger names them), never a ' +
  'foreign-interior rebuild. TWO surfaces are serialized and never yours to write mid-lane: pnpm-workspace.yaml ' +
  '(report needs in packageAsks) and the branch-level .planning docs (the terminal align owns them). Prose per ' +
  'docs/standards/style-guide.md - declarative present-tense fact, every symbol backticked, zero meta framing, ' +
  'never count-based prose. Fences carry zero comments beyond canonical section dividers.'

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

const toolImprovePrompt = () => [CONTEXT, MANDATE, READ_FIRST, WRITE_FULLY,
  'TASK: ULTRA-AGGRESSIVE IMPROVE of ' + TOOLING + ' IN PLACE. Your evidence dossiers are ' + SCRATCH +
  '/stack-tooling.md (execute its per-file improvement map) and ' + SCRATCH + '/audit-tooling.md (close every ' +
  'finding); both are FLOORS. Duties: the [04] four-gate stack made real - complete strictness roster on the dual ' +
  'floor; Biome as the sole rail with the doctrine anti-pattern promotion set authored as GritQL plugin rules at ' +
  'error (each rule shaped semantically - trigger, predicate, exemption - with firing and non-firing spans); ONE ' +
  'stryker config (rule and delete the duplicate); nx.json tags/affected truth for the six-folder + tests estate; ' +
  'vite/vitest/playwright configs modernized to verified current capabilities; the _architecture gauges extended to ' +
  'own every law resolution cannot express against the NEW structure; _testkit and e2e improved under the tests ' +
  'plane own established law; the dev .api tier truthful. Config edits are surgical and verified against the ' +
  'tool version actually pinned; report dev-package needs in packageAsks, never edit pnpm-workspace.yaml. Return ' +
  'the fix-log with collapsed and extended stated concretely.',
].join('\n\n')

const improvePrompt = (folder) => [CONTEXT, MANDATE, READ_FIRST, PAGE_CRAFT, WRITE_FULLY,
  'TASK: ULTRA-AGGRESSIVE IMPROVE of ' + ROOT + '/' + folder + ' IN PLACE - the whole folder, every page. Your two ' +
  'evidence dossiers are ' + SCRATCH + '/stack-' + folder + '.md (execute its per-page integration map) and ' +
  SCRATCH + '/audit-' + folder + '.md (close every finding); both are FLOORS you hunt past, never ceilings. ' +
  'COLLAPSE: fold parallel shapes into deep Schema class families, tagged unions with exhaustive dispatch, ' +
  'vocabulary tables with derived types; kill every mini schema, loose type/const, thin wrapper, and stringy seam; ' +
  'reduce surface and LOC through density, never through capability loss. EXTEND: weave the stacking dossier ' +
  'capability into the owning pages root-up as if always carried; close the mandate emphases for this folder ' +
  '(machines, pubsub, streaming, resilience, SQL/PG depth, filesystem/cloud, telemetry hooks, UI depth, IaC depth - ' +
  'whichever your folder owns); anticipate the most complex apps and the five-times demand. NEW ADMISSIONS ARE ' +
  'DEBT: every just-admitted package in ' + SCRATCH + '/admissions.md that your folder owns currently has ZERO ' +
  'consuming fences - each lands in its owning page(s) at operator depth in THIS pass or your disposition is ' +
  'incomplete; the stacking dossier maps each to its page. A genuine new owner earns a new page in an existing ' +
  'sub-domain. A MID-LANE PACKAGE DISCOVERY is wired FULLY in-lane: verify the surface at source (registry + ' +
  'published types), author the folder .api catalog in the house form, add the folder README domain-package ' +
  'registry row, compose the fences at operator depth - ONLY the central pnpm-workspace.yaml pin defers to the ' +
  'align agent (one-writer law) as a packageAsk naming pkg@verified-version + tier + the catalog and pages already ' +
  'landed; a bare ask with no in-lane wiring is an incomplete disposition. Return the fix-log with collapsed and ' +
  'extended stated concretely.',
].join('\n\n')

const scopeOf = (key) => key === 'tooling' ? TOOLING : ROOT + '/' + key

const critiquePrompt = (key, improve) => [CONTEXT, MANDATE, READ_FIRST, PAGE_CRAFT, STANCE, WRITE_FULLY,
  'TASK: CRITIQUE - your role law is libs/.planning/campaign-method.md [04] CRITIQUE, read at source and held to ' +
  'the letter: the mechanical line-by-line doctrinal-conformance and capability-completeness audit of ' +
  scopeOf(key) + ', every hit a fix made now, checklists as a FLOOR. Dimensions: COLLAPSE_SCAN (README [03] table); ' +
  'OWNER_CHOOSER shape re-derivation with loose type/const spam as the prime target under the one-fifth shape ' +
  'budget; KNOB_TEST + MODAL_ARITY (one entrypoint per modality, Function.dual, overload sets); INJECTION ' +
  '(capability only through Tag/Service/Layer/Reference); RAILS + ASPECTS at their owning pages; STRINGY/FRAGILE ' +
  '(zero any/as/!, vocabulary-derived literals, branded fields); PAGE_CRAFT - the design-doc structure law above is ' +
  'an audit dimension: doctrine-mirroring Law-line spam, signature-only fences, and fence fragmentation are smashed ' +
  'into the Rasm-exemplar shape with full-bodied dominant fences; WIRING-COMPLETENESS - a packageAsk the improver ' +
  'raised without its in-lane catalog, README registry row, and consuming fences is an incomplete wiring you finish ' +
  'now; EXTENSION-IN-PLACE - the audit is not code polish ' +
  'alone: EVERY page in scope is held to the full mandate bar in its own right - the dossiers and the improver ' +
  'fix-log are floors, never scope limits - and each page you touch leaves with capability ADDED at operator depth ' +
  '(machines, pubsub, streaming, resilience, SQL depth, filesystem/cloud, telemetry hooks, UI, IaC - whichever the ' +
  'folder owns), never merely defects removed; CAPABILITY + ILLUSION with the five-times-demand ' +
  'test. IMPROVER RESULT (verify on disk, never trust): ' + JSON.stringify(improve || {}) + ' Return the fix-log.',
].join('\n\n')

const redteamPrompt = (key, crit) => [CONTEXT, MANDATE, READ_FIRST, PAGE_CRAFT, STANCE, WRITE_FULLY,
  'TASK: RED-TEAM - your role law is libs/.planning/campaign-method.md [04] RED-TEAM, read at source and held to ' +
  'the letter: the terminal, most aggressive pass over ' + scopeOf(key) + '; every defect repaired in ' +
  'place; the scope ends objectively DENSER and MORE CAPABLE than the critique left it. Every page in scope is ' +
  're-attacked in isolation and in composition at the full mandate bar - the critique fix-log bounds nothing. ' +
  '(A) COUNTERFACTUAL on every core owner: does a denser tagged family, a Schema class family with derived ' +
  'variants, a vocabulary table, a parameterized generator, or a deeper Effect primitive collapse the whole fence? ' +
  'Build the stronger design. (B) ANTICIPATORY_COLLAPSE: the diff of the next feature - next engine row, provider, ' +
  'topology, tenant model, deployment target - lands as ONE row with consumers untouched or loudly broken. ' +
  '(C) LONG-TAIL: empty/singular/plural/batch/stream/malformed/concurrent/cancelled/partial-failure/version-skew; ' +
  'backpressure, interruption, breaker states. (D) BOUNDARY: wave order, seams both ends, entry surfaces few and ' +
  'deep, per-app isolation under thousands of consuming apps. (E) SPRAWL + PHANTOMS: flat code below operator ' +
  'depth, hand-re-derived package capability, any/as/! anywhere, thin wrappers. (F) FULL COLD RE-REVIEW of every ' +
  'critique dimension by name, then verify on disk that the critique fix-log landed. CRITIQUE RESULT: ' +
  JSON.stringify(crit || {}) + ' Return the fix-log.',
].join('\n\n')

const alignPrompt = (laneResults) => [CONTEXT, MANDATE, READ_FIRST, PAGE_CRAFT, WRITE_FULLY,
  'TASK: TERMINAL ALIGN - ONE pass over ALL of ' + ROOT + ' with whole-repository write authority; you are also ' +
  'the serialized writer for pnpm-workspace.yaml and the branch .planning docs. Read every folder in full. FIX: ' +
  'broken cross-folder chains, partially implemented functionality, naive residue the lanes missed, incorrect or ' +
  'bloated exports blocks, duplicated concerns owned twice, seam mismatches (both ends, wire-canonical names ' +
  'frozen). ALIGN: folders stacked and integrated - internally automatic capability, aligned never coupled, ' +
  'surgical extension where a chain is incomplete; the tooling estate (root configs + tests gauges) verified ' +
  'consistent with the improved corpus - a gauge or config a lane changed that now contradicts a sibling is fixed ' +
  'here. APPLY: every packageAsk below you judge justified lands in pnpm-workspace.yaml once at its named verified ' +
  'version (the lanes already wired catalog + README + fences in-lane - author any piece a lane missed); then run ' +
  'pnpm install and spot-verify each new catalog against the installed node_modules surface; rejected asks get ' +
  'one-line rulings in your summary and their in-lane wiring is unwound. Then true up the branch .planning docs ' +
  '(README router, ARCHITECTURE codemap/seams/edge table, dataflow-system) against the improved corpus. ' +
  'LANE RESULTS: ' + laneResults + ' Return {fixes, summary}.',
].join('\n\n')

const acceptPrompt = () => [CONTEXT, MANDATE, READ_FIRST, PAGE_CRAFT, WRITE_FULLY,
  'TASK: VERIFY CLOSE - your role law is libs/.planning/campaign-method.md [04] VERIFY, read at source and held to ' +
  'the letter: adversarial and WRITING, never a friendly confirmation - every problem you find you FIX in place ' +
  'NOW, and where a single-point patch competes with a root-level dense reconstruction of the same fence, the root ' +
  'form wins. Over the whole improved ' + ROOT + ': ' +
  '(1) cross-page symbol sweep - every cross-page symbol a fence composes resolves on a sibling owner with a ' +
  'matching signature; a mismatch is repaired at the correct end, both ends recorded; ' +
  '(2) catalog truth audit - sample 5 catalogs per tier against node_modules declarations; a lying member is fixed ' +
  'in the catalog and in every fence that composed it; ' +
  '(3) manifest audit - every pnpm-workspace.yaml TS package has exactly one .api catalog and at least one ' +
  'composing page; an orphan either direction is closed (author the catalog, land the composition, or record the ' +
  'explicit kill ruling); ' +
  '(4) doctrine cold-grade - sample 2 pages per folder against the sixteen laws and repair every violation found; ' +
  'a violated pattern found in a sample is then hunted across its siblings, never left as a one-page fix. ' +
  'unresolved carries ONLY what is genuinely unreachable from the files at hand - never a punt on a strengthenable ' +
  'fix. Return {fixes, unresolved, summary}.',
].join('\n\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!ACTIVE.length) {
  log('No matching folders - pass folder names or empty args for the full corpus.')
  return { folders: [], total: 0 }
}
log('Continuation over: ' + LANE_KEYS.join(', ') + ' - census + research dossiers consumed from ' + SCRATCH)

// --- [LANES]

phase('Lanes')
const lanes = (await pool(LANE_KEYS, CAP, async (folder) => {
  const tooling = folder === 'tooling'
  const improve = await agent(tooling ? toolImprovePrompt() : improvePrompt(folder), {
    label: 'improve:' + folder, phase: 'Lanes', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL })
  const crit = await agent(critiquePrompt(folder, improve), {
    label: 'crit:' + folder, phase: 'Lanes', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL })
  const rt = await agent(redteamPrompt(folder, crit), {
    label: 'rt:' + folder, phase: 'Lanes', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL })
  return { folder,
    improved: improve ? improve.verdict : 'dropped',
    critFixes: crit ? crit.files.length : 0, rtFixes: rt ? rt.files.length : 0,
    packageAsks: [].concat((improve && improve.packageAsks) || [], (crit && crit.packageAsks) || [], (rt && rt.packageAsks) || []),
    residuals: [].concat((improve && improve.residuals) || [], (crit && crit.residuals) || [], (rt && rt.residuals) || []) }
})).filter(Boolean)

// --- [CLOSE]

phase('Close')
const align = await agent(alignPrompt(JSON.stringify(lanes)), {
  label: 'align', phase: 'Close', model: 'fable', effort: 'high', schema: ALIGN_OUT, stallMs: STALL })
const accept = await agent(acceptPrompt(), {
  label: 'verify', phase: 'Close', model: 'fable', effort: 'high', schema: ACCEPT_OUT, stallMs: STALL })

return {
  folders: lanes,
  align: align ? align.summary : 'dropped',
  acceptance: accept ? { unresolved: accept.unresolved, summary: accept.summary } : 'dropped',
}
