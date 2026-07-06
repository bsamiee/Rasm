export const meta = {
  name: 'convert-host',
  description: 'Convert Rasm.Rhino and Rasm.Grasshopper from durable host-bound source into full planning folders: hostile source census + current-only host-API research (assay decompile over RhinoCommon/Eto/Grasshopper2 - never Rhino 7 or GH1 material) deepening the existing folder .api stubs in place - the census/research/kernel/acceptance lanes run on gpt-5.5 dispatched through codex wrapper agents (CODEX flag; false restores native opus) - one shared kernel recon lane over the rebuilt libs/csharp/Rasm corpus (unified motion/easing/color math and every universal owner the host layers must compose instead of re-deriving), one architect per folder ruling the re-derived sub-domain map + unit dependency edges and executing the scaffold, then ALL build units across both folders CONCURRENT under one agent-level slot cap (a unit waits only on the implement stage of its declared dependency units, never on sibling chains) - each unit an implement-critique-redteam chain with navigation-fact handoffs, seam-ledger coordination, and bidirectional kernel ripple authority under the evidence/expand-form/depth bounds; per-folder index docs authored last from the landed tree, one serialized law tail owning central manifests and stale-claim verification, read-only acceptance close.',
  whenToUse: 'Launch once to open the host-boundary planning campaign. Ephemeral - delete after the campaign lands.',
  phases: [
    { title: 'Survey', detail: 'census + catalog-deepening research + kernel recon lanes on gpt-5.5 via the codex wrapper (sonnet dispatch shells); CODEX=false restores native opus', model: 'sonnet' },
    { title: 'Architect' },
    { title: 'Build' },
    { title: 'Close' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 14 // runtime concurrency clamp is min(16, cores-2) = 14 on this machine
const STAGGER_MS = 1500
const STALL = 300000
const MAX_UNITS = 6
const UNIT_PAGES = 8 // pages per unit ceiling; editing fidelity degrades past ~8 dense pages per writer
const SCRATCH = '.claude/scratch/convert-host'
const CODEX = true // survey/kernel/acceptance lanes run on gpt-5.5 via the codex wrapper; false restores native opus lanes
const CODEX_DIR = '.claude/scratch/codex' // wrapper task/schema/report files, one triple per lane

const FOLDERS = [
  {
    key: 'rhino', root: 'libs/csharp/Rasm.Rhino', name: 'Rasm.Rhino',
    host: 'RhinoCommon (Rhino 9 WIP) + Eto.Forms/Eto.Drawing',
    census: [
      { key: 'a', paths: 'Blocks/ and Camera/' },
      { key: 'b', paths: 'Commands/, Capture.cs, and Events.cs' },
      { key: 'c', paths: 'Exchange/' },
      { key: 'd', paths: 'UI/' },
    ],
    research: [
      { key: 'rhinocommon', catalogs: 'api-rhinocommon-document.md, api-rhinocommon-commands.md, api-rhinocommon-display.md, ' +
        'api-rhinocommon-blocks.md, api-rhinocommon-fileio.md, api-rhinocommon-geometry.md, api-rhino-ui.md',
        charge: 'RhinoCommon on Rhino 9 WIP: RhinoDoc lifecycle and events, object tables (objects/layers/materials/groups/' +
        'views/named views/instance definitions), geometry-to-display pipeline, DisplayConduit and custom draw, ViewCapture, ' +
        'RhinoApp/Command infrastructure, the full Get* interactive families, transaction/undo records, render pipeline ' +
        'hooks, units and tolerance regimes, file IO surfaces incl. the format-engine roster and FilePdf, Rhino.UI panels/' +
        'dialogs/pages/gumball/toolbars, and every Rhino-9-new capability the current assemblies carry.' },
      { key: 'eto', catalogs: 'api-eto-forms.md, api-eto-runtime.md, api-eto-drawing.md, api-macos-native.md',
        charge: 'Eto.Forms + Eto.Drawing as shipped with Rhino 9: the FULL control roster, layout containers (dynamic/table/' +
        'pixel/stack), data binding (IndirectBinding/BindableBinding, MVVM patterns), dialogs and semi-modal, custom drawing ' +
        '(Drawable, Graphics, paths/brushes/pens/text), styles and platform handlers, Rhino UI integration surfaces ' +
        '(Panels.RegisterPanel, RhinoEtoApp, EtoExtensions), toolbars, clipboard/drag-drop/notifications, plus the ' +
        'platform-gated AppKit/CoreAnimation vsync-and-cosmetics seam - the capability set a generator-shaped UI layer ' +
        'should own so any native Rhino UI element is a row, never hand assembly.' },
    ],
  },
  {
    key: 'gh', root: 'libs/csharp/Rasm.Grasshopper', name: 'Rasm.Grasshopper',
    host: 'Grasshopper2 SDK (GH2 on Rhino 9 WIP) + Eto',
    census: [
      { key: 'a', paths: 'Components/' },
      { key: 'b', paths: 'UI/Canvas.cs, UI/Ui.cs, UI/Document.cs, and UI/Editor.cs' },
      { key: 'c', paths: 'UI/Events.cs, UI/Input.cs, UI/Interaction.cs, and UI/Layout.cs' },
      { key: 'd', paths: 'UI/Motion.cs, UI/Paint.cs, and UI/Wire.cs' },
    ],
    research: [
      { key: 'gh2', catalogs: 'api-gh2-components.md, api-gh2-document.md',
        charge: 'The Grasshopper2 SDK as shipped with Rhino 9 WIP: the component model (Component base, construction-time ' +
        'inputs/outputs, Access levels, the pin/parameter families), the document/solver model (Document, DocumentMethods, ' +
        'ObjectList, Connectivity/Connections, SolutionServer lifecycle, expiry), data model (Garden trees/paths/pears), ' +
        'special objects, undo actions, plugin registration, and every GH2-vs-GH1 paradigm break - GH1 idioms ' +
        '(GH_Component, IGH_*, RegisterInputParams, SolveInstance signatures) are LEGACY POISON: naming one as current is a ' +
        'defect. GH2 documentation is sparse; the decompile IS the truth.' },
      { key: 'gh2ui', catalogs: 'api-gh2-canvas.md, api-gh2-editor.md, api-gh2-flex.md, api-gh2-interaction.md, ' +
        'api-eto-forms.md, api-eto-runtime.md, api-eto-drawing.md, api-macos-native.md',
        charge: 'GH2 UI + Eto inside Grasshopper: canvas paint phases and picking, skinning, wire shapes, snapping/' +
        'alignment, the flex/animation substrate (Motion.Pacer/Spring, RepaintRequest, Subscription, MotionEquations), ' +
        'editor/toolbar/input-panel/tooltip chrome, floating buttons, Eto-hosted panels/dialogs inside the GH2 process, and ' +
        'the platform-gated AppKit/CoreAnimation gesture/vsync/cosmetics seam - the capability set a higher-order layer ' +
        'should own so custom component UI, canvas overlays, and GH2-native panels are rows.' },
    ],
  },
]

// --- [INPUTS] ----------------------------------------------------------------------------

const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const wanted = Array.isArray(argsIn) ? argsIn
  : (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.targets)) ? argsIn.targets
  : (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn.trim()] : null
const ACTIVE = wanted
  ? FOLDERS.filter((f) => wanted.some((w) => String(w).indexOf(f.name) >= 0 || String(w).indexOf(f.key) >= 0))
  : FOLDERS

// --- [MODELS] ----------------------------------------------------------------------------

const DOSSIER = { type: 'object', additionalProperties: false, required: ['path', 'summary'], properties: {
  path: { type: 'string' }, summary: { type: 'string' } } }
const UNIT = { type: 'object', additionalProperties: false, required: ['key', 'pages', 'owns', 'charter', 'after'], properties: {
  key: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } },
  owns: { type: 'string' }, // the ownership boundary: which vocabulary/owners THIS unit mints vs composes
  charter: { type: 'string' }, after: { type: 'array', items: { type: 'string' } } } } // unit keys whose IMPLEMENT must land first (vocabulary producers)
const ARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['preSwap', 'units', 'packageDeltas', 'summary'],
  properties: { preSwap: { type: 'string' }, units: { type: 'array', items: UNIT },
    packageDeltas: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const DELTAS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['symbol', 'change'],
  properties: { symbol: { type: 'string' }, change: { type: 'string' } } } } // navigation facts: what moved, as data, zero adjectives
const DEFERRED = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'],
  properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const INDEXROWS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['doc', 'row'],
  properties: { doc: { type: 'string' }, row: { type: 'string' } } } }
const FIXLOG = { type: 'object', additionalProperties: false,
  required: ['files', 'verdict', 'summary', 'deltas', 'deferred', 'indexRows'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] },
  phantoms: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' },
  deltas: DELTAS, deferred: DEFERRED, indexRows: INDEXROWS } }
const DOCS_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const ACCEPT_SCHEMA = { type: 'object', additionalProperties: false, required: ['unresolved', 'summary'], properties: {
  unresolved: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const CONTEXT = 'Rasm monorepo - libs/csharp planning corpus (markdown specs of intended C# package designs). ' +
  'CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; ' +
  'depend strictly upward). docs/stacks/csharp is the FLOOR, never the ceiling - every fence meets it and pushes past ' +
  'it to the strongest form the doctrine admits.'

const MANDATE = 'CAMPAIGN LAW - THE HOST-BOUNDARY CONVERSION. Rasm.Rhino and Rasm.Grasshopper are planning folders (the ' +
  'index-doc shells, .planning/, and folder .api stub catalogs exist on disk); this campaign realizes them: each folder ' +
  'entirely captures its host API surface and builds higher-order abstractions, value, and capability so countless future ' +
  'Rhino/GH2 apps and plugins compose rows instead of re-deriving host plumbing. BINDING RULES: ' +
  '(1) Each folder references ONLY the Rasm kernel - leverage Rasm surfaces where they fit, NEVER any other sibling ' +
  'package, never a coupling to Element/Materials/Bim/Compute/Persistence, and NEVER Rasm.AppUi: Eto is THE native UI ' +
  'framework and each folder owns a full Eto sub-domain able to build any native UI element with native host integration. ' +
  '(2) KERNEL UNIFICATION: host-agnostic logic - easing/spring/interpolation math, perceptual color blending, pure ' +
  'geometry/numeric algorithms - COMPOSES the Rasm kernel, never a second in-folder derivation; where the kernel lacks a ' +
  'universal owner the concept demands, the kernel owner is EXTENDED in place in expand-form (a case, row, field, or ' +
  'operation on the existing Rasm planning page - the kernel recon map names the exact sites) with the host folder ' +
  'composing it; renaming or collapsing a kernel surface is recorded in `deferred` for the law tail, never raced. ' +
  '(3) CURRENT HOST ONLY: Rhino 9 WIP RhinoCommon, Grasshopper2 SDK, current Eto - a Rhino 6/7-era pattern or a GH1 idiom ' +
  '(GH_Component, IGH_*, SolveInstance, RegisterInputParams) presented as current is a phantom-class defect; every host ' +
  'member is verified via `uv run python -m tools.assay api` decompile over the host assemblies (assay blocked: the .api ' +
  'catalogs, Context7, and official McNeel material own the fallback - and the claim is marked catalog-verified, never ' +
  'guessed). (4) Planning-folder form per libs/.planning/README.md: design pages at <pkg>/.planning/<SubDomain>/<page>.md ' +
  '- PascalCase sub-folders, lowercase page names - one page per eventual source file, one dense polymorphic owner per ' +
  'page at 400-700+ LOC fence mass, transcription-complete fences, zero fence comments beyond canonical section dividers. ' +
  '(5) The existing source is CONSIDERABLE BUT NOT GOOD: capture every capability from the census, rebuild it ground-up ' +
  'denser/richer/parameterized, kill every naive pattern (both axes: thin COVERAGE slices, enumerated APPROACH rosters ' +
  'that one generator should generate), and extend to the full domain the host admits. (6) Buildout over removal: a ' +
  'capability is dropped only as an explicit ruled kill; a phantom member is the sole silent deletion. (7) You fix what ' +
  'you notice anywhere in the repo EXCEPT the serialized surfaces: the owning-folder index docs (the docs agent writes ' +
  'them once), and the central manifests + cross-repo law docs (the law tail writes them once) - report exact rows in ' +
  '`indexRows` instead of editing those.'

const READ_FIRST = 'READ FIRST, IN ORDER, BEFORE ANY EDIT. (1) DOCTRINE - enumerate docs/stacks/csharp/ with a real ' +
  'ls (never memory), read the README and EVERY root page it routes IN FULL in atlas order; then enumerate ' +
  'docs/stacks/csharp/domain/ through its router README and read every shard your pages touch (the Rhino/geometry/' +
  'host shards are mandatory here). This prompt does not restate doctrine; read it at source and conform every fence ' +
  'to it. (2) .API - ls BOTH catalog tiers in full: the shared substrate libs/csharp/.api/ AND the folder .api/, plus ' +
  'the kernel folder catalogs libs/csharp/Rasm/.api/ (the RhinoCommon-adjacent truths live there); layer the ' +
  'Thinktecture/LanguageExt rails onto the host surfaces, never the host set alone. (3) AUTHORING LAW - ' +
  'libs/.planning/README.md in full (doc-set, page grammar, card law, banned hedges) and docs/standards/' +
  'style-guide.md. (4) KERNEL - the Rasm package README.md + ARCHITECTURE.md so kernel leverage composes real ' +
  'surfaces, never guesses; the kernel recon dossier carries the exact unified-owner sites.'

const STANCE = 'STANCE - every pass is hostile; the pages under review were authored by ANOTHER engineer and are under ' +
  'adversarial review. Hold every fence naive, shallow, or illusory until it survives a real attack; the burden of proof ' +
  'is on the code. Dense, confident, package-fluent code is the PRIME suspect. NAIVETY is a defect on two axes: COVERAGE ' +
  '(a 2-case family for a 20-case domain) and APPROACH (an enumerated roster where one parameterized generator should ' +
  'generate the space - the roster demotes to seed DATA). ILLUSORY code is the primary target: doctrine vocabulary ' +
  '([Union]/[SmartEnum<TKey>]/[ValueObject]/rails), cited hosts, confident prose, hollow body. Every collapse-signal ' +
  'list is a FLOOR. NO CHURN: an edit requires a named violated law or invariant and the concrete case that breaks it; a ' +
  'clean verdict earned by an attack that finds nothing is a first-class result, proven by adding nothing.'

const WRITE_FULLY = 'WRITE FULLY - every fix you identify you make NOW; the fix-log reports edits already made, ' +
  'never a to-do or a hedge. A first-order cross-file ripple your edit causes is YOURS in the same pass; a second-order ' +
  'ripple (exposed by a ripple repair) or a surface a concurrent unit owns is recorded in `deferred` as {files, claim} - ' +
  'the law tail drains the backlog; nothing is silently dropped. Latest modern C# 14 on net10; apply the ' +
  'docs/stacks/csharp file-organization + section-order law; total generated Switch, no silent _ arm; prose per ' +
  'docs/standards/style-guide.md - declarative present-tense fact, every symbol backticked, zero meta framing, zero ' +
  'provenance, never fragile count-based prose.'

const CURRENT_STATE = 'CURRENT STATE - sibling units land work concurrently with yours. Before any edit, re-read the ' +
  'CURRENT on-disk state of your pages AND every sibling page your pages compose; landed sibling work is picked up as ' +
  'found. A vocabulary owner your charter marks as composed (not minted) is read from disk as it NOW stands; a conflict ' +
  'between your design and a landed sibling resolves to the stronger form, never a revert. SEAM LEDGER: append one row ' +
  'per cross-unit event to your unit ledger `' + SCRATCH + '/<folder>-<unit>-seams.md` (`SEAM_CHANGED | <files> | ' +
  '<symbol fact, old -> new>` when a shared name/signature you mint moves; `RIPPLE_REPAIRED | <files> | <fact>` when you ' +
  'repair a counterpart). Before any edit outside your unit pages, ls `' + SCRATCH + '/` and read every sibling ' +
  '`*-seams.md` row whose files intersect yours - a RIPPLE_REPAIRED row is work you do NOT redo.'

const GIT_GROUND = 'DELTA GROUNDING - run `git status` and `git diff --stat -- <your unit pages>` to see what this run ' +
  'changed before judging it; the diff is orientation, CURRENT disk is truth.'

// --- [OPERATIONS] ------------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// Agent-level slot scheduler: CAP agents in flight across ALL folders and units, staggered launch,
// work-conserving backfill. The single governor for every agent call.
const makeSlots = (cap) => {
  let active = 0
  let gate = Promise.resolve()
  const waiters = []
  const stagger = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  return async (fn) => {
    if (active >= cap) await new Promise((res) => waiters.push(res))
    active++
    await stagger()
    try { return await fn() } finally { active--; const next = waiters.shift(); if (next) next() }
  }
}
const slot = makeSlots(CAP)
// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// CODEX_DIR, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns that JSON verbatim. It never does, edits, or judges the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and relay ' +
    'its typed answer VERBATIM. Never perform, edit, judge, soften, or summarize the task yourself.',
  '(1) mkdir -p ' + CODEX_DIR + '; write the TASK block below verbatim to ' + base + '-task.md; write this JSON ' +
    'Schema exactly to ' + base + '-schema.json: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call that returns immediately: ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral ' +
    '--output-schema ' + base + '-schema.json -o ' + base + '-report.json "Do the task in ' + base + '-task.md ' +
    'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>&1 &',
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
// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, native opus otherwise.
const recon = (task, o) => CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: o.schema, stallMs: STALL })
  : agent(task, { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: o.schema, stallMs: STALL })
// Navigation handoff: FACTS ONLY - files, symbol deltas, backlog. Never verdicts, summaries, or adjectives.
const navOf = (logs) => { const rows = logs.filter(Boolean); return { files: [...new Set(rows.flatMap((r) => r.files || []))],
  deltas: rows.flatMap((r) => r.deltas || []), deferred: rows.flatMap((r) => r.deferred || []) } }

const censusPrompt = (folder, lane) => [CONTEXT, MANDATE,
  'TASK: HOSTILE READ-ONLY SOURCE CENSUS over ' + folder.root + ' - your assigned slice: ' + lane.paths + '. ' +
  'Read every assigned .cs file IN FULL. For EACH file emit one register row: file | owned capability in concept ' +
  'terms | the exact host members it composes (' + folder.host + ') | quality verdict with the naivety axes named | ' +
  'collapse signal (which sibling capabilities belong inside one polymorphic owner) | HOST-AGNOSTIC vs HOST-BOUND split ' +
  '(pure math/algebra liftable to the Rasm kernel vs genuinely host-coupled mechanism) | rebuild intent (what survives ' +
  'as concept, what dies as pattern). Also inventory ' + folder.root + '/' + folder.name + '.csproj: references, ' +
  'packages, host assembly bindings. The register must be COMPLETE - every assigned file appears; downstream ' +
  'disposition audits key off your rows. Write the dossier to ' + SCRATCH + '/' + folder.key + '-census-' + lane.key +
  '.md and return {path, summary} - summary max 10 lines.',
].join('\n\n')

const researchPrompt = (folder, lane) => [CONTEXT, MANDATE,
  'TASK: CURRENT-ONLY HOST-API RESEARCH for ' + folder.name + ' - lane ' + lane.key + '. ' + lane.charge + ' ' +
  'PRIMARY ROUTE: `uv run python -m tools.assay api` decompile over the installed host assemblies - enumerate ' +
  'namespaces, then drill the surfaces your charge names; quote verified member signatures with their assembly ' +
  'anchors. SECONDARY: existing catalogs under libs/csharp/Rasm/.api/ and libs/csharp/.api/, Context7, and current ' +
  'McNeel developer material - marked catalog-verified when assay cannot reach a surface. FORBIDDEN: any Rhino 6/7-era ' +
  'or GH1 pattern presented as current; training-data recall without verification. PRODUCT: DEEPEN these existing ' +
  'folder .api stub catalogs IN PLACE at ' + folder.root + '/.api/ - each stub declares its owned namespace scope; you ' +
  'grow it to a verified-member catalog (exact signatures, integration shape per capability cluster, what the ' +
  'census-era source missed), preserving each stub file scope and name: ' + lane.catalogs + '. These files are YOURS ' +
  'alone this run - no other lane writes them' + (folder.key === 'gh' ? ' (the Rhino folder owns its own copies of the ' +
  'eto catalogs; never edit outside ' + folder.root + ')' : '') + '. Also write a dossier of cross-catalog findings to ' +
  SCRATCH + '/' + folder.key + '-research-' + lane.key + '.md with verified anchors ONLY - a fake anchor is your ' +
  'defect. Return {path, summary} - summary max 10 lines.',
].join('\n\n')

const kernelPrompt = () => [CONTEXT, MANDATE,
  'TASK: READ-ONLY KERNEL RECON over libs/csharp/Rasm - the rebuilt kernel planning corpus the host folders compose. ' +
  'Read the package README.md + ARCHITECTURE.md, then EVERY design page under libs/csharp/Rasm/.planning/ IN FULL ' +
  '(all sub-domains - Analysis, Domain, Drawing, Meshing, Numerics, Parametric, Processing, Solving, Spatial), and ls ' +
  'libs/csharp/Rasm/.api/. PRODUCT - information, never prescriptions: (1) UNIFIED OWNERS: every kernel owner the ' +
  'host-boundary layers must COMPOSE instead of re-deriving - easing/spring/interpolation/timeline math, perceptual ' +
  'color algebra, pure geometry/numeric algorithms, parametric/motion vocabulary - each with the exact page, owner ' +
  'name, and member spellings quoted with file:line anchors. (2) KERNEL GAPS: host-agnostic capability the census-era ' +
  'host source hand-rolls (46-curve easing families, damped-spring integrators, OKLab/OKLCH blending, repeat/yoyo ' +
  'cycle arithmetic) that NO kernel owner carries yet - name the closest existing kernel owner and the expand-form ' +
  'extension site (the page + section where a case/row/field/operation would land). (3) SEAMS: every kernel surface ' +
  'whose shape the host folders depend on, quoted. MANDATORY SELF-VERIFY: re-open every cited anchor before returning; ' +
  'a guess or vague entry is deleted. Write the dossier to ' + SCRATCH + '/kernel-recon.md and return {path, summary}.',
].join('\n\n')

const architectPrompt = (folder, dossiers, kernelDossier) => [CONTEXT, MANDATE, READ_FIRST, STANCE,
  'TASK: RULE + SCAFFOLD the ' + folder.name + ' planning folder. EVIDENCE - read every dossier in full: ' +
  dossiers.join(', ') + ', and the kernel recon at ' + kernelDossier + '. RULE the architecture: RE-DERIVE the ' +
  'sub-domain map from the census evidence, never from the current folder layout - a sub-domain earns 3+ pages or ' +
  'folds; concern-mixing in the source is split pressure (a single UI sub-domain carrying canvas painting, wire ' +
  'routing, motion, interaction, chrome, and events at ~9k LOC fails the map - rule the split the evidence demands); ' +
  'an Eto sub-domain is MANDATORY and owns full native UI construction as generator rows. Emit the complete page ' +
  'roster (PascalCase sub-folders, lowercase page names, one dense owner per page), the disposition of every census ' +
  'register row (absorbed into a named page or explicitly killed with a ruling), the host-agnostic rows routed to the ' +
  'kernel per MANDATE rule (2) (each with its kernel extension site from the recon), and any packageDeltas (central ' +
  'Directory.Packages.props motions - REPORT them, never edit the central manifest; folder .csproj edits are yours). ' +
  'PARTITION the roster into at most ' + MAX_UNITS + ' build units of at most ' + UNIT_PAGES + ' pages each: units ' +
  'run CONCURRENTLY, so each unit carries `owns` (the vocabulary/owners it MINTS - no two units mint the same owner), ' +
  'a charter (collapse rulings, census-row dispositions, host-capability targets from the research catalogs, kernel ' +
  'compose/extend rows), and `after` (the unit keys whose IMPLEMENT must land before this unit starts - ONLY true ' +
  'vocabulary dependence, typically the foundation unit alone; an empty after is the default, and after may only name ' +
  'units earlier in your emitted order). THEN EXECUTE the scaffold: record the current HEAD hash as preSwap; create ' +
  folder.root + '/.planning/<SubDomain>/ dirs; git rm every existing .cs source file and source sub-folder (the ' +
  'census captured them; git history recovers them) - keep the .csproj, packages.lock.json, the index-doc shells ' +
  '(IDEAS.md, TASKLOG.md), and the .api/ catalogs; apply ruled folder .csproj edits. Do NOT commit. Return {preSwap, ' +
  'units, packageDeltas, summary}.',
].join('\n\n')

const implementPrompt = (folder, unit, preSwap, dossiers, kernelDossier, scopes) => [CONTEXT, MANDATE, READ_FIRST,
  STANCE, WRITE_FULLY, CURRENT_STATE,
  'TASK: GROUND-UP AUTHOR unit ' + unit.key + ' of ' + folder.name + ' - build freely and ambitiously to the full ' +
  'bar; the trailing critique and red-team passes carry the attack. EXACTLY these pages: ' + unit.pages.join(', ') +
  '. OWNS: ' + unit.owns + '. CHARTER: ' + unit.charter + ' Evidence dossiers: ' + dossiers.join(', ') +
  '; kernel recon: ' + kernelDossier + '; the folder .api catalogs are deepened and verified - compose them. ' +
  'CONCURRENT UNIT SCOPES (a page another unit owns is composed from disk, never edited; a needed change there is a ' +
  '`deferred` row): ' + scopes + '. Old source recovers via git show ' + preSwap + ':<path> when a census row needs ' +
  'depth beyond its register. Before authoring EACH page, restate in one line the owner it holds, the vocabulary it ' +
  'mints vs composes, and the doctrine laws that bind it - then build against that restatement. Construct in ' +
  'LIFECYCLE order: admit raw once, canonical owner by OWNER_CHOOSER, stacked rail/aspect over a thin pure core, ' +
  'projection, egress, BOTH ingress and egress parameterized; collapse parallel shapes into ONE [Union]/' +
  '[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/source-generated case family IN THE SAME FILE; one ' +
  'polymorphic entrypoint per modality. Host-agnostic math COMPOSES or EXTENDS the kernel per MANDATE rule (2). ' +
  'Every host member verified per the mandate route before it is written. Return the fix-log - `deltas` carries every ' +
  'minted symbol/wire as data, `deferred` the backlog rows, both exact.',
].join('\n\n')

const critiquePrompt = (folder, unit, scopes, nav) => [CONTEXT, MANDATE, READ_FIRST, STANCE, WRITE_FULLY, CURRENT_STATE,
  'NAVIGATION (facts from the pass that landed these pages - locations only, no assessments; it changes where you look ' +
  'FIRST, never what you conclude): ' + JSON.stringify(nav),
  GIT_GROUND,
  'TASK: CRITIQUE - your role law is libs/.planning/campaign-method.md [04] CRITIQUE, read at source and held to the ' +
  'letter: the mechanical line-by-line doctrinal-conformance and capability-completeness audit, every hit a fix made ' +
  'now, never a note; the named checklists are a FLOOR you hunt past. Your mandate is PREDICATE-POSITIVE: verify each ' +
  'required law holds and cite the clause. FORM YOUR OWN DEFECT LIST FIRST - read each page cold from CURRENT disk ' +
  'before consulting NAVIGATION. Fix EACH page of unit ' + unit.key + ' in place: ' + unit.pages.join(', ') + '. ' +
  'CONCURRENT UNIT SCOPES (foreign pages composed, never edited; changes there go to `deferred`): ' + scopes + '. ' +
  '- COLLAPSE_SCAN: run the docs/stacks/csharp README [03] table on every fence. ' +
  '- OWNER_CHOOSER (shapes.md [01]): re-derive every shape from its discriminants; kill every parallel DTO, ' +
  'one-field wrapper, and null/default ghost. ' +
  '- KNOB_TEST: delete each parameter - where the value reconstructs it, collapse to a policy value or input-shape ' +
  'discriminant. ' +
  '- ASPECTS + RAILS: audit against surfaces-and-dispatch.md and rails-and-effects.md at their owning pages. ' +
  '- HOST TRUTH: every RhinoCommon/Eto/GH2 member re-verified against the folder .api catalogs; a legacy idiom (GH1, ' +
  'Rhino 7-era) is a phantom - delete or rebuild on the current surface. ' +
  '- KERNEL UNIFICATION: host-agnostic math re-derived in the fence instead of composing/extending the kernel is a ' +
  'defect fixed per MANDATE rule (2). ' +
  '- BOUNDARY: the folder references ONLY the Rasm kernel; a coupling to any other sibling or to AppUi is a defect ' +
  'fixed now. ' +
  '- CAPABILITY-COMPLETENESS + ILLUSION: the body implements what names and prose promise; close every admitted host ' +
  'capability the owner omits per the charter and the .api catalogs; attack both naivety axes. ' +
  'Return the fix-log - `deltas` and `deferred` exact.',
].join('\n\n')

const redteamPrompt = (folder, unit, scopes, nav, crit) => [CONTEXT, MANDATE, READ_FIRST, STANCE, WRITE_FULLY, CURRENT_STATE,
  'NAVIGATION (locations only, no assessments): ' + JSON.stringify(nav),
  'PRIOR CLAIMS (UNVERIFIED - a previous pass asserts these edits and verdicts; refutation targets you judge against ' +
  'CURRENT disk, never a settled record): ' + JSON.stringify(crit ? { files: crit.files, verdict: crit.verdict,
    summary: crit.summary } : {}),
  GIT_GROUND,
  'TASK: RED-TEAM - your role law is libs/.planning/campaign-method.md [04] RED-TEAM, read at source and held to the ' +
  'letter: the terminal and most aggressive review; every defect repaired in place, and the work ends objectively ' +
  'DENSER and MORE CAPABLE than critique left it. Your mandate is PREDICATE-NEGATIVE - a pre-mortem, not a second ' +
  'conformance audit. FORM YOUR OWN ATTACK FIRST - cold-read each page from CURRENT disk before consulting the claims. ' +
  'Fix EACH page of unit ' + unit.key + ' in place: ' + unit.pages.join(', ') + '. CONCURRENT UNIT SCOPES (foreign ' +
  'pages composed, never edited; changes there go to `deferred`): ' + scopes + '. ' +
  '(A) COUNTERFACTUAL on the core owner/algebra/dispatch - does a denser generated family, a derived table, a ' +
  'parameterized generator over the enumerated space, or a deeper LanguageExt/Thinktecture/host/kernel primitive ' +
  'collapse the whole fence? A fundamentally stronger design is built, never defended against. (B) ' +
  'ANTICIPATORY_COLLAPSE - the diff of the next feature: the next control kind, command, conduit, component family, or ' +
  'canvas widget lands as one row with every consumer untouched or loudly broken. (C) LONG-TAIL - empty/singular/' +
  'plural/stream/malformed/concurrent/cancelled/partial-failure/host-teardown; undo records, document events, and ' +
  'solution expiry handled where the host demands them. (D) BOUNDARY/STRATA - kernel-only references; host types never ' +
  'leak past the folder contract; host-agnostic math composes the kernel per MANDATE rule (2). (E) SPRAWL + PHANTOMS - ' +
  'hand-re-derived host capability, flat code below the operator depth the packages reach, a phantom or legacy member ' +
  '(delete), a thin wrapper; and the inverse: an edit that ADDED surface where the doctrine demands collapse is ' +
  'regression rebuilt denser. (F) a FULL COLD RE-REVIEW of every conformance dimension by name. Return the fix-log.',
].join('\n\n')

const docsPrompt = (folder, unitReports) => [CONTEXT, MANDATE, WRITE_FULLY,
  'TASK: the index docs for ' + folder.root + ' per libs/.planning/README.md [02] - you are their ONE writer. Read ' +
  'libs/csharp/Rasm/README.md and libs/csharp/Rasm/ARCHITECTURE.md IN FULL as the sibling form exemplars, plus ' +
  'docs/standards/style-guide.md and docs/standards/formatting.md - the new docs match that structure, styling, and ' +
  'grammar exactly. README.md: the page router over the landed .planning tree + the domain-package registry (host ' +
  'assemblies + any folder additions) + the substrate section pointing at the branch registry. ARCHITECTURE.md: the ' +
  'standardized intro, the codemap tree naming every sub-domain with one-line charters (the eventual source tree, ' +
  'never the .planning scaffold), [02]-[SEAMS] carrying only genuine cross-folder rows (kernel consumption is a ' +
  'codemap fact, not a seam; the folder has no cross-language seam). IDEAS.md and TASKLOG.md exist as shells - ' +
  'PRESERVE them; add a card only where a unit reported a genuinely deferred concept. Verify every router row against ' +
  'disk. Unit results: ' + JSON.stringify(unitReports) + '. Return {files, summary}.',
].join('\n\n')

const lawPrompt = (results, backlog) => [CONTEXT, MANDATE, WRITE_FULLY,
  'TASK: SERIALIZED LAW TAIL - you are the ONE writer for central manifests and cross-repo law docs, and the drain for ' +
  'the deferred backlog. ' +
  '(1) CENTRAL MANIFEST: apply the collected packageDeltas to Directory.Packages.props at the correct label groups, ' +
  'newest stable versions verified via the nuget MCP; reflect any new package in the owning folder README registry if ' +
  'the docs agent missed it: ' + JSON.stringify(results.map((r) => ({ folder: r.folder, packageDeltas: r.packageDeltas }))) + '. ' +
  '(2) DEFERRED BACKLOG (second-order and cross-unit ripples the writers recorded - re-verify each {files, claim} on ' +
  'current disk, fix what holds, reject what disk already resolved; kernel-surface collapse/rename rows are yours to ' +
  'apply now that no concurrent writer runs): ' + JSON.stringify(backlog) + '. ' +
  '(3) INDEX ROWS reported by writers (apply each to its owning doc exactly once, deduped): ' +
  JSON.stringify(results.flatMap((r) => r.indexRows || [])) + '. ' +
  '(4) LAW DOCS - both folders are planning folders and the cross-repo docs already state it; VERIFY and fix residuals: ' +
  'libs/.planning/planning-targets.md (both in the CSHARP Planning Folders row), libs/.planning/architecture.md, ' +
  'libs/csharp/.planning/README.md (upgrade the two HOST-BOUNDARY router rows to README.md links now the files exist), ' +
  'libs/csharp/.planning/ARCHITECTURE.md. (5) SWEEP: rg for Rasm.Rhino and Rasm.Grasshopper across libs/**/*.md, ' +
  'README.md, and docs/** (excluding docs/stacks) - fix every remaining stale out-of-scope/no-planning/durable-source ' +
  'claim. PROSE LAW: docs/standards/style-guide.md - declarative present-tense fact, zero meta framing, structure named ' +
  'by members and law, never by count. Return {files, summary}.',
].join('\n\n')

const acceptPrompt = (results) => [CONTEXT,
  'TASK: READ-ONLY ACCEPTANCE - investigate, never edit, never block. Over both converted folders (' +
  results.map((r) => r.folder).join(', ') + '): (1) cross-page symbol sweep - every cross-page symbol a landed ' +
  'fence composes resolves on a sibling owner with a matching signature; (2) census-disposition spot-audit - sample ' +
  '10 register rows per folder from the census dossiers under ' + SCRATCH + '/ and confirm each landed in a page or ' +
  'carries an explicit kill; (3) boundary audit - no reference beyond the Rasm kernel, no AppUi, no GH1/legacy ' +
  'member cited as current; (4) kernel-unification audit - no host-agnostic math re-derived in a fence where the ' +
  'kernel owns or gained the owner; (5) law-doc audit - no surviving out-of-scope claim. Report each miss as ' +
  'unresolved with file evidence. Return {unresolved, summary}.',
].join('\n\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!ACTIVE.length) {
  log('No matching targets - pass Rasm.Rhino / Rasm.Grasshopper paths or run with empty args for both.')
  return { targets: [], total: 0 }
}
log('Converting: ' + ACTIVE.map((f) => f.name).join(', ') + '; CAP=' + CAP)

phase('Survey')
const kernelRecon = slot(() => recon(kernelPrompt(),
  { label: 'recon:kernel', phase: 'Survey', schema: DOSSIER, writes: true })) // shared across folders; awaited before each architect

const results = (await Promise.all(ACTIVE.map(async (folder) => {
  const lanes = folder.census.map((lane) => ({ kind: 'census', lane })).concat(
    folder.research.map((lane) => ({ kind: 'research', lane })))
  const dossiers = (await Promise.all(lanes.map((entry) => slot(() =>
    recon(entry.kind === 'census' ? censusPrompt(folder, entry.lane) : researchPrompt(folder, entry.lane), {
      label: entry.kind + ':' + folder.key + ':' + entry.lane.key, phase: 'Survey', schema: DOSSIER, writes: true,
    })).catch(() => null)))).filter(Boolean).map((d) => d.path)
  if (!dossiers.length) throw new Error(folder.name + ': no survey dossier landed')
  const kmap = await kernelRecon
  const kernelDossier = (kmap && kmap.path) || SCRATCH + '/kernel-recon.md'

  const arch = await slot(() => agent(architectPrompt(folder, dossiers, kernelDossier), {
    label: 'architect:' + folder.key, phase: 'Architect', model: 'fable', effort: 'high', schema: ARCH_SCHEMA, stallMs: STALL,
  }))
  if (!arch || !arch.units || !arch.units.length) throw new Error(folder.name + ': architect did not land')
  const units = arch.units.slice(0, MAX_UNITS)
  const scopes = JSON.stringify(units.map((u) => ({ unit: u.key, owns: u.owns, pages: u.pages })))
  log(folder.name + ': ' + units.length + ' unit(s), ' + units.reduce((n, u) => n + u.pages.length, 0) + ' pages ruled')

  // Concurrent unit chains: a unit waits ONLY on the IMPLEMENT stage of its declared `after` units
  // (vocabulary producers, earlier-indexed only - cycles impossible), never on sibling chains.
  const implDone = new Map()
  units.forEach((u) => { let release; implDone.set(u.key, { p: new Promise((res) => { release = res }), release }) })
  const keyIndex = new Map(units.map((u, i) => [u.key, i]))
  const unitReports = await Promise.all(units.map(async (unit, i) => {
    const deps = (unit.after || []).filter((k) => implDone.has(k) && keyIndex.get(k) < i)
    await Promise.all(deps.map((k) => implDone.get(k).p))
    const fix = await slot(() => agent(implementPrompt(folder, unit, arch.preSwap, dossiers, kernelDossier, scopes), {
      label: 'impl:' + folder.key + ':' + unit.key, phase: 'Build', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL,
    })).catch(() => null)
    implDone.get(unit.key).release() // dependents launch even on failure; they compose current disk
    if (!fix) return { unit: unit.key, pages: unit.pages.length, verdict: 'failed', deferred: [], indexRows: [] }
    const crit = await slot(() => agent(critiquePrompt(folder, unit, scopes, navOf([fix])), {
      label: 'crit:' + folder.key + ':' + unit.key, phase: 'Build', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL,
    })).catch(() => null)
    const rt = await slot(() => agent(redteamPrompt(folder, unit, scopes, navOf([fix, crit]), crit), {
      label: 'rt:' + folder.key + ':' + unit.key, phase: 'Build', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL,
    })).catch(() => null)
    return { unit: unit.key, pages: unit.pages.length, verdict: (rt && rt.verdict) || (crit && crit.verdict) || fix.verdict,
      deferred: [fix, crit, rt].filter(Boolean).flatMap((r) => r.deferred || []),
      indexRows: [fix, crit, rt].filter(Boolean).flatMap((r) => r.indexRows || []) }
  }))

  const docs = await slot(() => agent(docsPrompt(folder, unitReports.map((u) => ({ unit: u.unit, pages: u.pages, verdict: u.verdict }))), {
    label: 'docs:' + folder.key, phase: 'Build', model: 'fable', effort: 'high', schema: DOCS_SCHEMA, stallMs: STALL,
  }))
  return { folder: folder.name, preSwap: arch.preSwap, units: unitReports,
    packageDeltas: arch.packageDeltas || [], deferred: unitReports.flatMap((u) => u.deferred),
    indexRows: unitReports.flatMap((u) => u.indexRows), docs: docs ? docs.summary : 'dropped' }
}).map((p) => p.catch((e) => { log('folder failed: ' + e.message); return null })))).filter(Boolean)

if (!results.length) {
  log('No folder landed - nothing to close')
  return { folders: [], law: 'skipped', acceptance: 'skipped' }
}

phase('Close')
const BACKLOG = results.flatMap((r) => r.deferred)
log('Close: ' + BACKLOG.length + ' deferred backlog row(s), ' +
  results.flatMap((r) => r.packageDeltas).length + ' package delta(s)')
const law = await slot(() => agent(lawPrompt(results, BACKLOG), {
  label: 'law', phase: 'Close', model: 'fable', effort: 'high', schema: DOCS_SCHEMA, stallMs: STALL,
}))
const accept = await slot(() => recon(acceptPrompt(results),
  { label: 'acceptance', phase: 'Close', schema: ACCEPT_SCHEMA }))

return {
  folders: results.map((r) => ({ folder: r.folder, preSwap: r.preSwap, units: r.units.map((u) => ({ unit: u.unit, verdict: u.verdict })),
    packageDeltas: r.packageDeltas, docs: r.docs })),
  backlog: BACKLOG.length,
  law: law ? law.summary : 'dropped',
  acceptance: accept ? { unresolved: accept.unresolved, summary: accept.summary } : 'dropped',
}
