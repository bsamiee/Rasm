export const meta = {
  name: 'convert-host',
  description: 'Convert Rasm.Rhino and Rasm.Grasshopper from durable host-bound source into full planning folders: hostile source census, current-only host-API research (assay decompile over RhinoCommon/Eto/Grasshopper2 - never Rhino 7 or GH1 material), one architect per folder ruling the sub-domain map and executing the scaffold, sequential implement-critique-redteam unit pipelines rebuilding every capability as dense design fences, folder index docs, one serialized law tail owning central manifests and every stale out-of-scope claim, read-only acceptance close.',
  whenToUse: 'Launch once to open the host-boundary planning campaign. Ephemeral - delete after the campaign lands.',
  phases: [
    { title: 'Survey', model: 'opus' },
    { title: 'Architect' },
    { title: 'Build' },
    { title: 'Close' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 8
const STAGGER_MS = 1500
const STALL = 300000
const MAX_UNITS = 6
const SCRATCH = '.claude/scratch/convert-host'

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
      { key: 'rhinocommon', charge: 'RhinoCommon on Rhino 9 WIP: RhinoDoc lifecycle and events, object tables ' +
        '(objects/layers/materials/groups/views/named views/instance definitions), geometry-to-display pipeline, ' +
        'DisplayConduit and custom draw, ViewCapture, RhinoApp/Command infrastructure, transaction/undo records, ' +
        'render pipeline hooks, units and tolerance regimes, file IO surfaces, and every Rhino-9-new capability the ' +
        'current assemblies carry.' },
      { key: 'eto', charge: 'Eto.Forms + Eto.Drawing as shipped with Rhino 9: the FULL control roster, layout ' +
        'containers (dynamic/table/pixel/stack), data binding (IndirectBinding/BindableBinding, MVVM patterns), ' +
        'dialogs and semi-modal, custom drawing (Drawable, Graphics), styles and platform handlers, Rhino UI ' +
        'integration surfaces (Panels.RegisterPanel, RhinoEtoApp, EtoExtensions), toolbars, and the capability set a ' +
        'generator-shaped UI layer should own so any native Rhino UI element is a row, never hand assembly.' },
    ],
  },
  {
    key: 'gh', root: 'libs/csharp/Rasm.Grasshopper', name: 'Rasm.Grasshopper',
    host: 'Grasshopper2 SDK (GH2 on Rhino 9 WIP) + Eto',
    census: [
      { key: 'a', paths: 'Components/' },
      { key: 'b', paths: 'the FIRST half of UI/ by alphabetical file order (enumerate UI/ fully first; own files 1..ceil(n/2))' },
      { key: 'c', paths: 'the SECOND half of UI/ by alphabetical file order (enumerate UI/ fully first; own the remainder)' },
    ],
    research: [
      { key: 'gh2', charge: 'The Grasshopper2 SDK as shipped with Rhino 9 WIP: the component model (Component base, ' +
        'construction-time inputs/outputs, Access levels, the pin/parameter families), the document/solver model ' +
        '(GH_Document successor, solution lifecycle, expiry), data model (trees/paths/typed goo successors), ' +
        'attribute/canvas/rendering model, plugin registration, and every GH2-vs-GH1 paradigm break - GH1 idioms ' +
        '(GH_Component, IGH_*, RegisterInputParams, SolveInstance signatures) are LEGACY POISON: naming one as ' +
        'current is a defect. GH2 documentation is sparse; the decompile IS the truth.' },
      { key: 'gh2ui', charge: 'GH2 UI + Eto inside Grasshopper: canvas widgets, component attributes and custom UI, ' +
        'Eto-hosted panels/dialogs inside the GH2 process, preview/display integration, and the capability set a ' +
        'higher-order layer should own so custom component UI, canvas overlays, and GH2-native panels are rows.' },
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
const UNIT = { type: 'object', additionalProperties: false, required: ['key', 'pages', 'charter'], properties: {
  key: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } }, charter: { type: 'string' },
  catalogs: { type: 'array', items: { type: 'string' } } } }
const ARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['preSwap', 'units', 'packageDeltas', 'summary'],
  properties: { preSwap: { type: 'string' }, units: { type: 'array', items: UNIT },
    packageDeltas: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] },
  phantoms: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const DOCS_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const ACCEPT_SCHEMA = { type: 'object', additionalProperties: false, required: ['unresolved', 'summary'], properties: {
  unresolved: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const CONTEXT = 'Rasm monorepo - libs/csharp planning corpus (markdown specs of intended C# package designs). ' +
  'CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; ' +
  'depend strictly upward). docs/stacks/csharp is the FLOOR, never the ceiling - every fence meets it and pushes past ' +
  'it to the strongest form the doctrine admits.'

const MANDATE = 'CAMPAIGN LAW - THE HOST-BOUNDARY CONVERSION. Rasm.Rhino and Rasm.Grasshopper become FULL planning ' +
  'folders: each entirely captures its host API surface and builds higher-order abstractions, value, and capability ' +
  'so countless future Rhino/GH2 apps and plugins compose rows instead of re-deriving host plumbing. BINDING RULES: ' +
  '(1) Each folder references ONLY the Rasm kernel - leverage Rasm surfaces where they fit, NEVER any other sibling ' +
  'package, never a coupling to Element/Materials/Bim/Compute/Persistence, and NEVER Rasm.AppUi: Eto is THE native ' +
  'UI framework and each folder owns a full Eto sub-domain able to build any native UI element with native host ' +
  'integration. (2) CURRENT HOST ONLY: Rhino 9 WIP RhinoCommon, Grasshopper2 SDK, current Eto - a Rhino 6/7-era ' +
  'pattern or a GH1 idiom (GH_Component, IGH_*, SolveInstance, RegisterInputParams) presented as current is a ' +
  'phantom-class defect; every host member is verified via `uv run python -m tools.assay api` decompile over the ' +
  'host assemblies (assay blocked: the .api catalogs, Context7, and official McNeel material own the fallback - and ' +
  'the claim is marked catalog-verified, never guessed). (3) Planning-folder form per libs/.planning/README.md: ' +
  'design pages at <pkg>/.planning/<SubDomain>/<page>.md - PascalCase sub-folders, lowercase page names - one page ' +
  'per eventual source file, one dense polymorphic owner per page at 400-700+ LOC fence mass, transcription-complete ' +
  'fences, zero fence comments beyond canonical section dividers. (4) The existing source is CONSIDERABLE BUT NOT ' +
  'GOOD: capture every capability from the census, rebuild it ground-up denser/richer/parameterized, kill every ' +
  'naive pattern (both axes: thin COVERAGE slices, enumerated APPROACH rosters that one generator should generate), ' +
  'and extend to the full domain the host admits. (5) Buildout over removal: a capability is dropped only as an ' +
  'explicit ruled kill; a phantom member is the sole silent deletion. (6) You fix what you notice anywhere in the ' +
  'repo EXCEPT two serialized surfaces: the owning-folder index docs (the docs agent writes them once) and the ' +
  'central manifests + cross-repo law docs (the law tail writes them once) - report needs instead of editing those.'

const READ_FIRST = 'READ FIRST, IN ORDER, BEFORE ANY EDIT. (1) DOCTRINE - enumerate docs/stacks/csharp/ with a real ' +
  'ls (never memory), read the README and EVERY root page it routes IN FULL in atlas order; then enumerate ' +
  'docs/stacks/csharp/domain/ through its router README and read every shard your pages touch (the Rhino/geometry/' +
  'host shards are mandatory here). This prompt does not restate doctrine; read it at source and conform every fence ' +
  'to it. (2) .API - ls BOTH catalog tiers in full: the shared substrate libs/csharp/.api/ AND the folder .api/, plus ' +
  'the kernel folder catalogs libs/csharp/Rasm/.api/ (the RhinoCommon-adjacent truths live there); layer the ' +
  'Thinktecture/LanguageExt rails onto the host surfaces, never the host set alone. (3) AUTHORING LAW - ' +
  'libs/.planning/README.md in full (doc-set, page grammar, card law, banned hedges) and docs/standards/' +
  'style-guide.md. (4) KERNEL - the Rasm package README.md + ARCHITECTURE.md so kernel leverage composes real ' +
  'surfaces, never guesses.'

const STANCE = 'STANCE - every pass is hostile. Hold every fence naive, shallow, or illusory until it survives a ' +
  'real attack; the burden of proof is on the code. Dense, confident, package-fluent code is the PRIME suspect. ' +
  'NAIVETY is a defect on two axes: COVERAGE (a 2-case family for a 20-case domain) and APPROACH (an enumerated ' +
  'roster where one parameterized generator should generate the space - the roster demotes to seed DATA). ILLUSORY ' +
  'code is the primary target: doctrine vocabulary ([Union]/[SmartEnum<TKey>]/[ValueObject]/rails), cited hosts, ' +
  'confident prose, hollow body. Every collapse-signal list is a FLOOR. A no-edit verdict is earned by an attack ' +
  'that finds nothing.'

const WRITE_FULLY = 'WRITE FULLY - every fix you identify you make NOW; the fix-log reports edits already made, ' +
  'never a to-do or a hedge. A cross-file ripple your edit causes is YOURS in the same pass. Latest modern C# 14 on ' +
  'net10; apply the docs/stacks/csharp file-organization + section-order law; total generated Switch, no silent _ ' +
  'arm; prose per docs/standards/style-guide.md - declarative present-tense fact, every symbol backticked, zero ' +
  'meta framing, zero provenance, and never fragile count-based prose.'

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

const censusPrompt = (folder, lane) => [CONTEXT, MANDATE,
  'TASK: HOSTILE READ-ONLY SOURCE CENSUS over ' + folder.root + ' - your assigned slice: ' + lane.paths + '. ' +
  'Read every assigned .cs file IN FULL. For EACH file emit one register row: file | owned capability in concept ' +
  'terms | the exact host members it composes (' + folder.host + ') | quality verdict with the naivety axes named | ' +
  'collapse signal (which sibling capabilities belong inside one polymorphic owner) | rebuild intent (what survives ' +
  'as concept, what dies as pattern). Also inventory ' + folder.root + '/' + folder.name + '.csproj: references, ' +
  'packages, host assembly bindings. The register must be COMPLETE - every assigned file appears; downstream ' +
  'disposition audits key off your rows. Write the dossier to ' + SCRATCH + '/' + folder.key + '-census-' + lane.key +
  '.md and return {path, summary} - summary max 10 lines.',
].join('\n\n')

const researchPrompt = (folder, lane) => [CONTEXT, MANDATE,
  'TASK: CURRENT-ONLY HOST-API RESEARCH for ' + folder.name + ' - lane ' + lane.key + '. ' + lane.charge + ' ' +
  'PRIMARY ROUTE: `uv run python -m tools.assay api` decompile over the installed host assemblies - enumerate ' +
  'namespaces, then drill the surfaces your charge names; quote verified member signatures with their assembly ' +
  'anchors. SECONDARY: existing .api catalogs under libs/csharp/Rasm/.api/ and libs/csharp/.api/, Context7, and ' +
  'current McNeel developer material - marked catalog-verified when assay cannot reach a surface. FORBIDDEN: any ' +
  'Rhino 6/7-era or GH1 pattern presented as current; training-data recall without verification. PRODUCT: the ' +
  'capability inventory a higher-order layer should own - for each capability cluster: the verified host members, ' +
  'the integration shape (generator/table/rail the doctrine admits), and what the census-era source missed. Write ' +
  'the dossier to ' + SCRATCH + '/' + folder.key + '-research-' + lane.key + '.md with verified anchors ONLY - a ' +
  'fake anchor is your defect. Return {path, summary} - summary max 10 lines.',
].join('\n\n')

const architectPrompt = (folder, dossiers) => [CONTEXT, MANDATE, READ_FIRST, STANCE,
  'TASK: RULE + SCAFFOLD the ' + folder.name + ' planning folder. EVIDENCE - read every dossier in full: ' +
  dossiers.join(', ') + '. RULE the architecture: the sub-domain map (PascalCase, each sub-domain earning 3+ pages ' +
  'or folding), the complete page roster (lowercase page names, one dense owner per page, an Eto sub-domain is ' +
  'MANDATORY and owns full native UI construction as generator rows), the folder .api catalog set (which host/' +
  'package catalogs the folder tier needs - rhinocommon, eto, grasshopper2 families as fits - each assigned to the ' +
  'unit that authors it), the disposition of every census register row (absorbed into a page or explicitly killed ' +
  'with a ruling), and any packageDeltas (central Directory.Packages.props additions from the existing central ' +
  'roster or newly justified pins - REPORT them, never edit the central manifest yourself; folder .csproj edits are ' +
  'yours). Partition the roster into at most ' + MAX_UNITS + ' build units of at most 5 pages, dependency-ordered ' +
  '(foundations first), each with a charter carrying its collapse rulings, census-row dispositions, host-capability ' +
  'targets from the research dossiers, and assigned catalogs. THEN EXECUTE the scaffold: record the current HEAD ' +
  'hash as preSwap; create ' + folder.root + '/.planning/<SubDomain>/ dirs and ' + folder.root + '/.api/; git rm ' +
  'every existing .cs source file and source sub-folder (the census captured them; git history recovers them) - ' +
  'keep the .csproj and packages.lock.json; apply ruled folder .csproj edits. Do NOT commit. Return {preSwap, ' +
  'units, packageDeltas, summary}.',
].join('\n\n')

const implementPrompt = (folder, unit, preSwap, dossiers) => [CONTEXT, MANDATE, READ_FIRST, WRITE_FULLY,
  'TASK: GROUND-UP AUTHOR unit ' + unit.key + ' of ' + folder.name + ' - build freely and ambitiously to the full ' +
  'bar; the trailing critique and red-team passes carry the attack. EXACTLY these pages: ' +
  unit.pages.join(', ') + '. CHARTER: ' + unit.charter + ' ' +
  (unit.catalogs && unit.catalogs.length ? 'CATALOGS you author first, verified-member-only, integration-shaped: ' +
    unit.catalogs.join(', ') + '. ' : '') +
  'Evidence dossiers: ' + dossiers.join(', ') + '. Old source recovers via git show ' + preSwap + ':<path> when a ' +
  'census row needs depth beyond its register. Earlier units of this folder are settled law on disk - compose their ' +
  'vocabulary, never re-mint. Construct in LIFECYCLE order: admit raw once, canonical owner by OWNER_CHOOSER, ' +
  'stacked rail/aspect over a thin pure core, projection, egress, BOTH ingress and egress parameterized; collapse ' +
  'parallel shapes into ONE [Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/source-generated case ' +
  'family IN THE SAME FILE; one polymorphic entrypoint per modality. Every host member verified per the mandate ' +
  'route before it is written. Return the fix-log.',
].join('\n\n')

const critiquePrompt = (folder, unit) => [CONTEXT, MANDATE, READ_FIRST, STANCE, WRITE_FULLY,
  'TASK: CRITIQUE - your role law is libs/.planning/campaign-method.md [04] CRITIQUE, read at source and held to the ' +
  'letter: the mechanical line-by-line doctrinal-conformance and capability-completeness audit, every hit a fix made ' +
  'now, never a note; the named checklists are a FLOOR you hunt past. Fix EACH page of unit ' + unit.key +
  ' in place: ' + unit.pages.join(', ') + '. ' +
  '- COLLAPSE_SCAN: run the docs/stacks/csharp README [03] table on every fence. ' +
  '- OWNER_CHOOSER (shapes.md [01]): re-derive every shape from its discriminants; kill every parallel DTO, ' +
  'one-field wrapper, and null/default ghost. ' +
  '- KNOB_TEST: delete each parameter - where the value reconstructs it, collapse to a policy value or input-shape ' +
  'discriminant. ' +
  '- ASPECTS + RAILS: audit against surfaces-and-dispatch.md and rails-and-effects.md at their owning pages. ' +
  '- BLOAT + OPTIMIZATION: density is anti-redundancy and polymorphic collapse - fold parallel shapes into one ' +
  'owner, unify rails, reduce LOC through collapse with capability conserved, never through capability loss; ' +
  'optimization is correctness and sophistication in the algorithmic form, not only line count; prose and comment ' +
  'hygiene per the file-organization law. ' +
  '- HOST TRUTH: every RhinoCommon/Eto/GH2 member re-verified; a legacy idiom (GH1, Rhino 7-era) is a phantom - ' +
  'delete or rebuild on the current surface. ' +
  '- BOUNDARY: the folder references ONLY the Rasm kernel; a coupling to any other sibling or to AppUi is a defect ' +
  'fixed now. ' +
  '- CAPABILITY-COMPLETENESS + ILLUSION: the body implements what names and prose promise; close every admitted ' +
  'host capability the owner omits per the charter and the research dossiers; attack both naivety axes. ' +
  'Return the fix-log.',
].join('\n\n')

const redteamPrompt = (folder, unit, crit) => [CONTEXT, MANDATE, READ_FIRST, STANCE, WRITE_FULLY,
  'TASK: RED-TEAM - your role law is libs/.planning/campaign-method.md [04] RED-TEAM, read at source and held to the ' +
  'letter: the terminal and most aggressive review; every defect repaired in place, and the work ends objectively ' +
  'DENSER and MORE CAPABLE than critique left it. Fix EACH page of unit ' + unit.key + ' in place: ' +
  unit.pages.join(', ') +
  '. Assume the author and critique missed things. (A) COUNTERFACTUAL on the core owner/algebra/dispatch - does a ' +
  'denser generated family, a derived table, a parameterized generator over the enumerated space, or a deeper ' +
  'LanguageExt/Thinktecture/host primitive collapse the whole fence? A fundamentally stronger design is built, never ' +
  'defended against. (B) ANTICIPATORY_COLLAPSE - the diff of the next feature: the next control kind, command, ' +
  'conduit, component family, or canvas widget lands as one row with every consumer untouched or loudly broken. ' +
  '(C) LONG-TAIL - empty/singular/plural/stream/malformed/concurrent/cancelled/partial-failure/host-teardown; undo ' +
  'records, document events, and solution expiry handled where the host demands them. (D) BOUNDARY/STRATA - ' +
  'kernel-only references; host types never leak past the folder contract. (E) SPRAWL + PHANTOMS - hand-re-derived ' +
  'host capability, flat code below the operator depth the packages reach, a phantom or legacy member (delete), a ' +
  'thin wrapper. (F) a FULL COLD RE-REVIEW of every conformance dimension by name. CRITIQUE RESULT: ' +
  JSON.stringify(crit || {}) + '. Return the fix-log.',
].join('\n\n')

const docsPrompt = (folder, unitReports) => [CONTEXT, MANDATE, WRITE_FULLY,
  'TASK: the FOUR index docs for ' + folder.root + ' per libs/.planning/README.md [02] - you are their ONE writer. ' +
  'README.md: the page router over the landed .planning tree + the domain-package registry (host assemblies + any ' +
  'folder additions) + the substrate section pointing at the branch registry. ARCHITECTURE.md: the standardized ' +
  'intro, the codemap tree naming every sub-domain with one-line charters (the eventual source tree, never the ' +
  '.planning scaffold), [02]-[SEAMS] carrying only genuine cross-folder rows (kernel consumption is a codemap fact, ' +
  'not a seam; the folder has no cross-language seam). IDEAS.md and TASKLOG.md: the two-section shells ' +
  '([1]-[OPEN] holding a single "(none)" line, [2]-[CLOSED] empty). Verify every router row against disk. Unit ' +
  'results: ' + JSON.stringify(unitReports) + '. Return {files, summary}.',
].join('\n\n')

const lawPrompt = (results) => [CONTEXT, MANDATE, WRITE_FULLY,
  'TASK: SERIALIZED LAW TAIL - you are the ONE writer for central manifests and cross-repo law docs. ' +
  '(1) CENTRAL MANIFEST: apply the collected packageDeltas to Directory.Packages.props at the correct label groups, ' +
  'newest stable versions verified via the nuget MCP; then reflect any new package in the owning folder README ' +
  'registry if the docs agent missed it: ' + JSON.stringify(results.map((r) => ({ folder: r.folder, packageDeltas: r.packageDeltas }))) + '. ' +
  '(2) LAW DOCS - both folders are now planning folders; every stale claim dies: libs/.planning/planning-targets.md ' +
  '(add both to the CSHARP Planning Folders row), libs/.planning/architecture.md (the [05] lifecycle exception ' +
  'naming them out-of-scope, and any host-boundary prose asserting they carry no .planning), ' +
  'libs/csharp/.planning/README.md (the router gains both package rows in strata order; the out-of-scope-durable ' +
  'line dies), libs/csharp/.planning/ARCHITECTURE.md (the roster note and dependency-direction prose reflect ' +
  'planning-scoped host-boundary folders). (3) SWEEP: rg for Rasm.Rhino and Rasm.Grasshopper across libs/**/*.md, ' +
  'README.md, and docs/** (excluding docs/stacks) - fix every remaining stale out-of-scope/no-planning claim. ' +
  'PROSE LAW: docs/standards/style-guide.md - declarative present-tense fact, zero meta framing, structure named by ' +
  'members and law, never by count. Return {files, summary}.',
].join('\n\n')

const acceptPrompt = (results) => [CONTEXT,
  'TASK: READ-ONLY ACCEPTANCE - investigate, never edit, never block. Over both converted folders (' +
  results.map((r) => r.folder).join(', ') + '): (1) cross-page symbol sweep - every cross-page symbol a landed ' +
  'fence composes resolves on a sibling owner with a matching signature; (2) census-disposition spot-audit - sample ' +
  '10 register rows per folder from the census dossiers under ' + SCRATCH + '/ and confirm each landed in a page or ' +
  'carries an explicit kill; (3) boundary audit - no reference beyond the Rasm kernel, no AppUi, no GH1/legacy ' +
  'member cited as current; (4) law-doc audit - no surviving out-of-scope claim. Report each miss as unresolved ' +
  'with file evidence. Return {unresolved, summary}.',
].join('\n\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!ACTIVE.length) {
  log('No matching targets - pass Rasm.Rhino / Rasm.Grasshopper paths or run with empty args for both.')
  return { targets: [], total: 0 }
}
log('Converting: ' + ACTIVE.map((f) => f.name).join(', '))

const results = (await pool(ACTIVE, CAP, async (folder) => {
  const lanes = folder.census.map((lane) => ({ kind: 'census', lane })).concat(
    folder.research.map((lane) => ({ kind: 'research', lane })))
  const dossiers = (await pool(lanes, CAP, (entry) =>
    agent(entry.kind === 'census' ? censusPrompt(folder, entry.lane) : researchPrompt(folder, entry.lane), {
      label: entry.kind + ':' + folder.key + ':' + entry.lane.key, phase: 'Survey', model: 'opus', effort: 'max',
      schema: DOSSIER, stallMs: STALL,
    }))).filter(Boolean).map((d) => d.path)
  if (!dossiers.length) throw new Error(folder.name + ': no survey dossier landed')

  const arch = await agent(architectPrompt(folder, dossiers), {
    label: 'architect:' + folder.key, phase: 'Architect', model: 'fable', effort: 'max', schema: ARCH_SCHEMA, stallMs: STALL,
  })
  if (!arch || !arch.units || !arch.units.length) throw new Error(folder.name + ': architect did not land')
  const units = arch.units.slice(0, MAX_UNITS)
  log(folder.name + ': ' + units.length + ' unit(s), ' + units.reduce((n, u) => n + u.pages.length, 0) + ' pages ruled')

  const unitReports = []
  for (const unit of units) {
    const fix = await agent(implementPrompt(folder, unit, arch.preSwap, dossiers), {
      label: 'impl:' + folder.key + ':' + unit.key, phase: 'Build', model: 'fable', effort: 'max', schema: FIXLOG, stallMs: STALL,
    })
    if (!fix) { unitReports.push({ unit: unit.key, verdict: 'failed' }); continue }
    const crit = await agent(critiquePrompt(folder, unit), {
      label: 'crit:' + folder.key + ':' + unit.key, phase: 'Build', model: 'fable', effort: 'xhigh', schema: FIXLOG, stallMs: STALL,
    })
    const rt = await agent(redteamPrompt(folder, unit, crit), {
      label: 'rt:' + folder.key + ':' + unit.key, phase: 'Build', model: 'fable', effort: 'xhigh', schema: FIXLOG, stallMs: STALL,
    })
    unitReports.push({ unit: unit.key, pages: unit.pages.length, verdict: (rt && rt.verdict) || (crit && crit.verdict) || fix.verdict })
  }

  const docs = await agent(docsPrompt(folder, unitReports), {
    label: 'docs:' + folder.key, phase: 'Build', model: 'fable', effort: 'xhigh', schema: DOCS_SCHEMA, stallMs: STALL,
  })
  return { folder: folder.name, preSwap: arch.preSwap, units: unitReports,
    packageDeltas: arch.packageDeltas || [], docs: docs ? docs.summary : 'dropped' }
})).filter(Boolean)

phase('Close')
const law = await agent(lawPrompt(results), {
  label: 'law', phase: 'Close', model: 'fable', effort: 'xhigh', schema: DOCS_SCHEMA, stallMs: STALL,
})
const accept = await agent(acceptPrompt(results), {
  label: 'acceptance', phase: 'Close', model: 'opus', effort: 'max', schema: ACCEPT_SCHEMA, stallMs: STALL,
})

return {
  folders: results,
  law: law ? law.summary : 'dropped',
  acceptance: accept ? { unresolved: accept.unresolved, summary: accept.summary } : 'dropped',
}
