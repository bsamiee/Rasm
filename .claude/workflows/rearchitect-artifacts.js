export const meta = {
  name: 'rearchitect-artifacts',
  description: 'Bold-but-disciplined ground-up re-architecture of the libs/python/artifacts design-page folder (structure + external-lib set + design pages + .api + README/ARCHITECTURE/tree) for a bleeding-edge durable-artifact engine. Splits god-files (figures/preview raster+marks+decode+media-detect, figures/chart render+data-transform, figures/color colour-science+ColorAide+ICC/LUT), regroups the 6-page figures god-folder by real sub-domain, authors-or-folds the 4 placeholder domains (provenance/accessibility/media/pipeline), and stands up justified new sub-domains/libs for genuine gaps (layered/editable export, diagrams, richer ICC, template/format pipelines). Phases: Survey (8 read-only mappers over every page + central docs + capability/research) -> Architect (PROPOSE max -> CRITIQUE xhigh -> REDTEAM max, the 3-pass on the new architecture itself, emitting a decision-complete blueprint: taxonomy + workItems + libAdmissions + seamMap) -> Install (serial: admit + ACTUALLY install the new libs on cp315 via uv lock+sync+import, revert non-installable) -> Catalog (one agent per admitted lib authors its .api with assay-verified members) -> Author (pooled per work-item with serialized shared-file edits: create/split/rename/merge/move + author the new/split pages coherently to the .planning schema at a fertile-ground baseline) -> Atlas (update README router + ARCHITECTURE codemap + seams + tree) -> Reconcile (union-find cross-page coherence + deferred residuals -> fix -> adversarial verify) -> Verify (final gate: coherent, every page well-formed, fertile for rebuild-python, uv lock+sync green, ruff/ty clean on contract fences). Establishes the FOUNDATION, realizes no .py (rebuild-python pushes the pages to world-class code). Hardcoded scope, no args. Runs autonomously on a branch for reviewability.',
  phases: [
    { title: 'Survey', detail: '8 read-only mappers: one per .planning subfolder (documents/figures/typography/bundle/receipt/reports) + central docs + capability/web-research; each returns concern-per-page, god-files, gaps, candidate sub-domains and libs' },
    { title: 'Architect', detail: 'the 3-pass on the new architecture itself: PROPOSE (max) -> CRITIQUE (xhigh) -> REDTEAM (max), each adversarial; emits the decision-complete blueprint (taxonomy, workItems, libAdmissions, seamMap)' },
    { title: 'Install', detail: 'serial: admit the new libs to pyproject.toml at newest stable, then uv lock + uv sync + import-verify on cp315; source-build over a marker; revert a genuinely non-installable one' },
    { title: 'Catalog', detail: 'one agent per admitted lib authors its .api catalog with assay-verified members to house format' },
    { title: 'Author', detail: 'pooled over work-item clusters with serialized shared-file edits: create subfolders, split god-files, git mv renames, reallocate/merge content, author the new/split pages coherently per the blueprint owners + seams to the .planning schema' },
    { title: 'Atlas', detail: 'update README [01]-[ROUTER] + [02]-[DOMAIN_PACKAGES] and ARCHITECTURE [01]-[DOMAIN_MAP] codemap + [02]-[SEAMS] + the tree-view to the new structure' },
    { title: 'Reconcile', detail: 'union-find cluster the new/split pages by shared owner/seam plus every deferred residual -> fix in place (unify owners, align seams, compose new libs) -> adversarial verify per claim' },
    { title: 'Verify', detail: 'final adversarial gate: structure coherent, every page well-formed and fertile for rebuild-python, uv lock+sync green, ruff/ty clean on contract fences; returns hard residuals' },
  ],
}

// --- [SCHEMAS] ---------------------------------------------------------------------------
const RES = { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } }
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['area', 'pages'], properties: { area: { type: 'string' }, pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'concern'], properties: { path: { type: 'string' }, concern: { type: 'string' }, godfile: { type: 'boolean' }, mixedConcerns: { type: 'array', items: { type: 'string' } }, thin: { type: 'boolean' } } } }, gaps: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, note: { type: 'string' } } } }, candidateSubdomains: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name'], properties: { name: { type: 'string' }, rationale: { type: 'string' } } } }, candidateLibs: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package'], properties: { package: { type: 'string' }, subdomain: { type: 'string' }, capability: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, installable: { type: 'string' } } } } } }
const PAGE_SPEC = { type: 'object', additionalProperties: false, required: ['path', 'owner', 'charter'], properties: { path: { type: 'string' }, owner: { type: 'string' }, charter: { type: 'string' }, cites: { type: 'array', items: { type: 'string' } }, seams: { type: 'array', items: { type: 'string' } } } }
const FOLDER_SPEC = { type: 'object', additionalProperties: false, required: ['folder', 'pages'], properties: { folder: { type: 'string' }, rationale: { type: 'string' }, pages: { type: 'array', items: PAGE_SPEC } } }
const WORKITEM_SPEC = { type: 'object', additionalProperties: false, required: ['id', 'action'], properties: { id: { type: 'string' }, action: { type: 'string', enum: ['create', 'split', 'rename', 'merge', 'move'] }, sources: { type: 'array', items: { type: 'string' } }, targets: { type: 'array', items: { type: 'string' } }, owner: { type: 'string' }, contentAllocation: { type: 'string' }, seams: { type: 'array', items: { type: 'string' } } } }
const LIBADMIT_SPEC = { type: 'object', additionalProperties: false, required: ['package', 'subdomain', 'gap'], properties: { package: { type: 'string' }, subdomain: { type: 'string' }, gap: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, gated: { type: 'boolean' }, marker: { type: 'string' } } }
const SEAM_SPEC = { type: 'object', additionalProperties: false, required: ['label', 'pages'], properties: { label: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } }, note: { type: 'string' } } }
const BLUEPRINT_SCHEMA = { type: 'object', additionalProperties: false, required: ['taxonomy', 'workItems'], properties: { taxonomy: { type: 'array', items: FOLDER_SPEC }, workItems: { type: 'array', items: WORKITEM_SPEC }, libAdmissions: { type: 'array', items: LIBADMIT_SPEC }, seamMap: { type: 'array', items: SEAM_SPEC }, rationale: { type: 'string' } } }
const INSTALL_SCHEMA = { type: 'object', additionalProperties: false, required: ['admitted', 'lockGreen'], properties: { admitted: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'imported'], properties: { package: { type: 'string' }, version: { type: 'string' }, gated: { type: 'boolean' }, marker: { type: 'string' }, imported: { type: 'boolean' } } } }, reverted: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'reason'], properties: { package: { type: 'string' }, reason: { type: 'string' } } } }, lockGreen: { type: 'boolean' }, summary: { type: 'string' } } }
const CATALOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['package', 'verdict'], properties: { package: { type: 'string' }, apiPath: { type: 'string' }, verdict: { type: 'string', enum: ['authored', 'skipped'] }, members: { type: 'number' }, summary: { type: 'string' } } }
const WORKLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['id', 'verdict'], properties: { id: { type: 'string' }, action: { type: 'string' }, created: { type: 'array', items: { type: 'string' } }, renamed: { type: 'array', items: { type: 'string' } }, deleted: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['done', 'partial', 'skipped'] }, residual_high: { type: 'array', items: RES }, summary: { type: 'string' } } }
const ATLAS_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict'], properties: { verdict: { type: 'string', enum: ['updated', 'clean'] }, residual_high: { type: 'array', items: RES }, summary: { type: 'string' } } }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['coherent', 'lockGreen', 'wellFormed', 'fertile'], properties: { coherent: { type: 'boolean' }, lockGreen: { type: 'boolean' }, staticClean: { type: 'boolean' }, wellFormed: { type: 'boolean' }, fertile: { type: 'boolean' }, hard_residual: { type: 'array', items: RES }, summary: { type: 'string' } } }

// --- [HARNESS] -- steady bounded pool: <=cap in flight AND a serialized launch gate --------
const STAGGER_MS = 1500
const CAP = 10
const STALL = 360000
const EXEC_STALL = 600000
const ROOT = 'libs/python/artifacts'
const PLANNING = ROOT + '/.planning'
const FOLDER_API = ROOT + '/.api'
const SHARED_API = 'libs/python/.api'
const PYPROJECT = 'pyproject.toml'
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
const filesOf = (wi) => [...(wi.sources || []), ...(wi.targets || [])]
const clusterBy = (items, edgesOf) => {
  const parent = new Map()
  const find = (x) => { let p = x; while (parent.get(p) !== p) p = parent.get(p); return p }
  items.forEach((_, i) => parent.set(i, i))
  const owner = new Map()
  items.forEach((it, i) => edgesOf(it).forEach((k) => { if (owner.has(k)) parent.set(find(i), find(owner.get(k))); else owner.set(k, i) }))
  const by = new Map()
  items.forEach((it, i) => { const r = find(i); (by.get(r) || by.set(r, []).get(r)).push(it) })
  return [...by.values()]
}

// --- [MODELS] -- the woven doctrine blocks -----------------------------------------------
const LAW = [
  'TARGET: libs/python/artifacts is a Python DESIGN-PAGE folder — .planning/**/*.md design pages (concrete module blueprints carrying typed ```python signature``` contract fences), a .api/ external-capability catalog tier, and the governing README.md / ARCHITECTURE.md / IDEAS.md / TASKLOG.md. This workflow re-architects the FOLDER STRUCTURE + external-library set + design pages of this host-free durable-artifact engine (docs, spec sheets, presentation-grade graphics, analysis diagrams, charts, machine-readable elements, layered/editable export).',
  'SCOPE = FOUNDATION, NOT CODE: you establish the architecture, libraries, .api catalogs, and design pages (plus README/ARCHITECTURE/tree). You realize NO .py source — the ```python signature``` contract fences ARE the deliverable, and a later rebuild-python run pushes the pages to world-class code. NEVER write a .py file. Both .api tiers are mined: the folder tier ' + FOLDER_API + '/*.md and the shared substrate tier ' + SHARED_API + '/*.md (shared tier is READ-ONLY here).',
  'WRITE BOUNDARY: edit ONLY under ' + ROOT + '/** and the repo-root ' + PYPROJECT + ' / uv.lock. Reading docs/stacks/python, ' + SHARED_API + ', libs/python/.planning/README.md (substrate registry), and sibling pages is allowed; editing anything outside ' + ROOT + '/ (and the manifest pair) is forbidden. Every defect you identify in your scope you FIX NOW via Edit/Write/Bash; the structured log you return is a REPORT of edits ALREADY MADE, never a to-do list or a would/should hedge. Leave nothing behind except genuine cross-FILE items (report those in residual_high as {files:[...], claim}).',
].join('\n')
const CONCRETE_PAGE = [
  'PAGE SCHEMA — match the EXISTING sibling pages exactly (READ 2-3 real pages before authoring): an H1 `# [PY_ARTIFACTS_<TOKEN>]` then a dense charter paragraph naming the ONE polymorphic owner and its boundary; a `## [01]-[INDEX]` block of `- [0N]-[OWNER]: ...` routing rows; one `## [0N]-[OWNER]` section per owner carrying the `Owner:` / `Cases:` / `Entry:` (or `Modality:`) / `Auto:` / `Receipt:` / `Packages:` / `Growth:` / `Boundary:` field bullets; one or more ```python signature``` contract fences holding the typed owner; and a closing `## [0N]-[RESEARCH]` tail of `[TOKEN] [RESOLVED]/[RESEARCH]` design-decision rows.',
  'The pages are CONCRETE project modules, NOT agnostic doctrine snippets: they name the real packages, the real owner types, the real seams, the runtime ContentKey / ContentIdentity content-key, and the one kind-discriminated ArtifactReceipt family. Use the project nouns and the real member names; never genericize a page into a neutral teaching snippet.',
].join('\n')
const CODE_BAR = [
  'CONTRACT-FENCE BAR — bleeding-edge Python 3.15 functional / monadic-ROP, mirrored on the existing strongest pages (documents/model, bundle/bundle, documents/emit): PEP 695 type parameters (`class C[T]`, `def f[T]`, `type Alias = ...`), PEP 604 unions, `expression.tagged_union`/`tag`/`case` closed families, `msgspec.Struct(frozen=True)` wire/value owners, `beartype` contracts, `anyio` structured concurrency and the runtime `to_process.run_sync` gated-band subprocess seam, the runtime `RuntimeRail`/`async_boundary` rail, `ContentIdentity`/`ContentKey`, total `match` + `assert_never` over closed unions, and `frozendict`/`StrEnum`/`Literal` policy tables. NEVER `from __future__ import annotations`, never legacy `typing.List`/`Optional`/`Union`.',
  'ONE polymorphic owner per concept discriminating on input SHAPE (`T | Iterable[T]` normalized once at the head), NEVER a get/get_many/per-mode family and never a name-suffix variant. Cross-cutting concerns (retry, telemetry, validation, receipts, fault rails) are stacked decorators over a thin pure core; configuration enters as ONE behavior-carrying policy value (a vocabulary member, a tagged variant, a frozen policy row), never a `mode`/`batch`/`strict` knob. NO parallel DTOs, no per-operation sibling functions, no erased `params`/`dict[str, Any]` bags, no per-algorithm class family.',
  'EVERY cited package member is REAL: verify novel members via `uv run python -m tools.assay api` over the actually-installed distribution and against both .api tiers; a member you cannot verify is a PHANTOM to delete. Mine each package to its FULL advanced surface and STACK packages into one dense rail, never flat one-shot per-library uses.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY Architect pass (propose, critique, AND red-team) is HOSTILE to the taxonomy: assume the current organization is naively short-sighted AND that the proposed re-taxonomy is over-granular, over-consolidated, or mis-grouped until it survives an aggressive attack; the burden of proof is ON THE DESIGN. A split that is not load-bearing, a new subfolder that mirrors a taxonomy aesthetic rather than a real recurring owner, a page too thin for a sibling not to absorb, and a pipeline with a loose tacked-on arm are all DEFECTS to attack.',
  'CRITIQUE attacks: over-granularity (a trivially-thin page a sibling already owns), over-consolidation (a new god-file conflating semi-independent concerns), unjustified splits, mis-grouping (a page in the wrong sub-domain), loose-arm pipelines (an owner with an arm that does not compose into its ingress->model->transform->egress flow), and whether each page is FERTILE GROUND for a later rebuild-python run. RED-TEAM counterfactuals the WHOLE taxonomy: is there a denser owner decomposition? does any seam dangle? does each new sub-domain genuinely earn its folder? is any admitted lib unjustified or any real gap left unfilled?',
].join('\n')
const BOLD_DISCIPLINED = [
  'RE-TAXONOMY — bold but disciplined: SPLIT god-files (one owner conflating semi-independent concerns into coherent sibling owners), REGROUP a god-folder by real sub-domain, and STAND UP a new sub-domain/page ONLY when it is genuinely load-bearing across pages. NEVER over-granularize (no trivially-thin page a sibling already owns) and NEVER over-consolidate (no new god-file). A new subfolder appears only when a concern recurs across pages as a real owner.',
  'SEEDED PROBLEMS to FIX (verify each against the live pages, do not trust this list blindly): figures/preview.md is a GOD-FILE conflating raster image-processing (pillow/scikit-image) + machine-readable-mark GENERATION (segno/python-barcode/zxing-cpp) + mark DECODING + MIME media-detection (python-magic) — split into coherent owners (raster vs marks vs decode vs media-detect). figures/chart.md MIXES chart-render (altair/lets-plot/matplotlib) + vegafusion data-transform (a data-pipeline concern) + interactive-state HTML — separate chart-render from data-transform. figures/color.md OVER-CONSOLIDATES colour-science + ColorAide + ICC/LUT (4 semi-independent concerns). figures/ is a 6-page GOD-FOLDER (chart/table/scene/preview/compose/color) to regroup into real sub-domains. provenance / accessibility / media / pipeline are ARCHITECTURE-codemap-declared PLACEHOLDER domains with dangling README router rows and NO .planning pages — author them or fold them.',
].join('\n')
const AGGRESSIVE_ADD = [
  'LIBRARY ADMISSION — aggressive addition, ZERO hedging: admit a library WHEN it brings unique modern capability appropriate to the durable-artifact domain; decline ONLY for (a) old/unmaintained, (b) a stronger already-admitted alternative, or (c) genuinely out-of-scope — NEVER for "no current consumer" (zero current consumers never lowers the capability bar, and planned future consumers are real design pressure). No future/planned/deferred framing anywhere in a page. A justified library is admitted to ' + PYPROJECT + ' + ACTUALLY installed on cp315 + given a .api catalog + integrated into the design pages IN PLACE.',
  'CANDIDATE GAPS the workflow researches + VALIDATES (candidates, NOT mandates — your own validated research decides): layered/editable export for Illustrator/InDesign (drawsvg hierarchical/named-layer SVG; pikepdf PDF OCG optional-content-group layers — pikepdf is ALREADY admitted, so this is OCG-layer USAGE not a new admission), diagrams (graphviz/netgraph), richer ICC color management (littlecms/colorspacious), and input->output template/format pipelines. Validate every candidate: osx-arm64, newest stable version, OSS or free-full commercial license, modern packaging, no duplication of an admitted package, and cp315-installable.',
].join('\n')
const GROUND_UP = [
  'GROUND-UP COHERENCE — author the new/split pages as if the WHOLE folder were built ground-up TOGETHER: one unified ingress->model->transform->egress pipeline per sub-domain, collapsed polymorphic owners, aligned typed seams in ARCHITECTURE [02]-[SEAMS], the shared runtime content-key + the ONE kind-discriminated ArtifactReceipt family threaded through every producer, and NO loose arms tacked onto an owner. This is the EXTEND / ANTICIPATORY_COLLAPSE / COMPOSED_IMPLEMENTATION mentality in pure-Python idiom: shape each owner for the family it WILL absorb so the next case/provider/format lands as ONE declaration with every consumer untouched or broken loudly at type-check.',
  'Pages sharing an owner or a seam MUST agree on that owner shape and the seam direction/contract; a new lib admitted to a sub-domain MUST be composed into that sub-domain page (not left as a dangling .api). The result is a coherent, lib-rich foundation a later rebuild-python can push to world-class code — every page a fertile, internally-consistent blueprint, never a tacked-on arm or an orphaned owner.',
].join('\n')
const CP315_TRUTH = [
  'CP315 INSTALL-TRUTH: admissibility is decided by an ACTUAL `uv lock` + `uv sync` + import on CPython 3.15 (the repo floor — ' + PYPROJECT + ' requires-python >=3.15, osx-arm64), NEVER by wheel-presence alone. Admit UN-GATED a pure-Python / cp315-clean wheel (abi3 or py3-none-any) OR a native sdist that SOURCE-BUILDS on cp315 via the Forge scientific toolchain (the shapely/pyarrow/zxing-cpp pattern) — PREFER the source-build over a marker. A companion `python_version<\'3.15\'` marker is honest ONLY for a package with a real, reported cp315 build failure; wheel-absence ALONE is NOT a reason to gate.',
  'Mirror the existing ' + PYPROJECT + ' pinning convention EXACTLY: a quoted PEP 508 requirement in [project].dependencies with an inline `# artifacts: <capability>` comment; the gated band carries the `; python_version<\'3.15\'` marker plus a one-line gate rationale. After any add you MUST run `uv lock` + `uv sync` and import-verify on cp315 with `uv run python -c "import <module>"` (the ruff/ty gate does NOT exercise the install); revert ONLY a package that genuinely cannot resolve/build, removing its manifest row, its .api file, and any page reference.',
].join('\n')
const DOMAIN_ELEVATION = 'DOMAIN ELEVATION — durable-artifact concerns must reach BEYOND table-stakes for a bleeding-edge AEC/Rhino/GH2-adjacent engine: sophisticated typography + format-output, layered/editable export with proper NAMED layers and the real file formats for Illustrator/InDesign, charts AND diagrams, machine-readable elements (barcodes/headers/structured marks), architectural graphics (sheets / spec sheets / analysis diagrams), and input->output template/structure pipelines. A page that only restates a thin single-library wrapper is a DEFECT; each owner composes its packages to their full advanced surface.'
const ZERO_META = 'ZERO META + NO REALIZATION: no provenance, research-origin, freshness, process, session, or review narration in any page or .api file; the `## [..]-[RESEARCH]` rows carry only `[RESOLVED]`/`[RESEARCH]` design-decision facts. Backtick every symbol, type, field, package id, path, and member. NEVER write a .py module — the contract fences are the deliverable; rebuild-python realizes them later.'
const DOCTRINE = [LAW, '', CONCRETE_PAGE, '', CODE_BAR, '', BOLD_DISCIPLINED, '', AGGRESSIVE_ADD, '', GROUND_UP, '', CP315_TRUTH, '', DOMAIN_ELEVATION, '', ZERO_META].join('\n')

// --- [OPERATIONS] -- prompt builders -----------------------------------------------------
const SURVEY_AREAS = [
  { key: 'documents', read: 'every page under ' + PLANNING + '/documents/ (model.md, emit.md, egress.md, lens.md), line-by-line', focus: 'the DocumentNode semantic tree and its emit/egress/lens inverses' },
  { key: 'figures', read: 'every page under ' + PLANNING + '/figures/ (chart.md, table.md, scene.md, preview.md, compose.md, color.md) — the 6-page GOD-FOLDER, line-by-line', focus: 'the god-file preview.md (raster + marks + decode + media-detect), the mixed-concern chart.md (render + vegafusion data-transform + interactive HTML), the over-consolidated color.md (colour-science + ColorAide + ICC/LUT), and how figures regroups into real sub-domains' },
  { key: 'typography', read: PLANNING + '/typography/conformance.md, line-by-line', focus: 'font shaping/subset/instance + PAdES signing + PDF/A conformance audit' },
  { key: 'bundle', read: PLANNING + '/bundle/bundle.md, line-by-line', focus: 'the algorithm-row compression/bundle owner' },
  { key: 'receipt', read: PLANNING + '/receipt/receipt.md, line-by-line', focus: 'the shared kind-discriminated ArtifactReceipt family every producer contributes one case to' },
  { key: 'reports', read: PLANNING + '/reports/report.md, line-by-line', focus: 'report composition binding figures + sections over jinja2 + papermill/nbclient' },
  { key: 'central-docs', read: ROOT + '/README.md, ' + ROOT + '/ARCHITECTURE.md, ' + ROOT + '/IDEAS.md, ' + ROOT + '/TASKLOG.md, and the artifacts-tagged rows of the repo-root ' + PYPROJECT, focus: 'the [01]-[ROUTER] / [01]-[DOMAIN_MAP] codemap / [02]-[SEAMS], the 4 ARCHITECTURE-declared PLACEHOLDER domains (provenance/accessibility/media/pipeline) that have dangling router rows but NO .planning pages, and the full admitted-package set' },
  { key: 'capability-research', read: 'both .api tiers (' + FOLDER_API + '/*.md folder tier AND ' + SHARED_API + '/*.md shared substrate tier) AND do validated web research', focus: 'the capability surface of every admitted lib (where a page under-uses a package it already has) PLUS research candidate libraries for the absent sub-domains — layered/editable export (drawsvg named-layer SVG, pikepdf PDF OCG layers), diagrams (graphviz/netgraph), richer ICC color (littlecms/colorspacious), input->output template/format pipelines — validating each candidate osx-arm64 / newest / OSS-or-free / cp315-installable' },
]
const surveyPrompt = (a) => [DOCTRINE, '',
  'TASK: SURVEY (READ-ONLY — write NOTHING) the ' + a.key + ' area of the artifacts folder. READ: ' + a.read + '. Focus: ' + a.focus + '. Read every file fully; do not skim. For EACH page you read, report its CURRENT one-concern-per-page mapping, whether it is a GOD-FILE (set godfile=true with the conflated concerns in mixedConcerns), and whether it is too THIN for a sibling not to absorb (thin=true). Report capability GAPS (a durable-artifact concern absent or under-served — be aggressive, this is a bleeding-edge engine), candidate NEW SUB-DOMAINS the re-taxonomy should stand up (only genuinely load-bearing ones), and candidate LIBS to admit (each with the sub-domain it serves, the unique capability, newest version, license, and a one-line cp315-installable assessment). Apply the AGGRESSIVE_ADD and BOLD_DISCIPLINED stance: name real gaps and real candidate owners, never hedge with future/deferred framing. Return the structured map for area=' + a.key + '.'].join('\n')

const proposePrompt = (surveys) => [DOCTRINE, '', ADVERSARIAL, '',
  'TASK: PROPOSE the decision-complete RE-ARCHITECTURE BLUEPRINT for ' + ROOT + ' (DECISION only — write NOTHING). You are given the 8-area survey below as data. Design the NEW subfolder/page TAXONOMY as if the whole folder were built ground-up together: split the god-files, regroup the figures god-folder by real sub-domain, author-or-fold the placeholder domains, and stand up justified new sub-domains for genuine gaps. Then emit, decision-complete:',
  '(1) taxonomy: the new folders, each with rationale and its pages {path, owner (the one polymorphic owner the page owns), charter, cites (the .api catalogs it composes — folder tier + shared substrate), seams}. (2) workItems: an ordered list of {id, action (create|split|rename|merge|move), sources (existing paths read/consumed), targets (new/renamed paths written), owner, contentAllocation (exactly which content from which source goes where), seams}. Make each work-item ATOMIC and self-contained (a split of one god-file into N pages is ONE work-item; a merge of M pages is ONE work-item); name EVERY source and target path so shared-file contention is explicit. (3) libAdmissions: the new libs to admit {package, subdomain, gap, newest version, license, gated, marker} — each validated osx-arm64 / newest / OSS-or-free / cp315-installable, decline only for old / stronger-admitted / out-of-scope. (4) seamMap: the unified seam/pipeline map — {label, pages (the page paths this owner or seam spans), note} — so pages sharing an owner or seam are linked. (5) rationale. Build the strongest, densest, most coherent foundation a later rebuild-python can push to world-class.',
  'SURVEY:\n' + JSON.stringify(surveys, null, 1)].join('\n')
const critiquePrompt = (bp) => [DOCTRINE, '', ADVERSARIAL, '',
  'TASK: CRITIQUE + REFINE the proposed blueprint (DECISION only — write NOTHING). ULTRA-HARSH, UNAGREEABLE: assume the taxonomy is over-granular, over-consolidated, or mis-grouped until proven otherwise. Attack EVERY page: is it load-bearing or trivially-thin (merge it)? is any new subfolder a real recurring owner or a taxonomy aesthetic (fold it)? did a split leave a loose arm or a dangling seam? is any god-file un-split or any placeholder domain left dangling? is every content-allocation exact (no content silently dropped or duplicated across the split targets)? is every libAdmission justified by unique capability AND validated (osx-arm64/newest/OSS/cp315), with no duplication of an admitted package? is each page FERTILE GROUND for rebuild-python (a coherent owner with a real growth axis, not a stub)? FIX the blueprint in place and return the REFINED full blueprint (same shape).',
  'BLUEPRINT:\n' + JSON.stringify(bp, null, 1)].join('\n')
const redteamPrompt = (bp) => [DOCTRINE, '', ADVERSARIAL, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + REFINE — the LAST and MOST AGGRESSIVE pass (DECISION only — write NOTHING); red-team is critique AND MORE, burden of proof ON THE DESIGN. COUNTERFACTUAL the whole taxonomy: is there a denser owner decomposition that collapses two proposed pages into one polymorphic owner, or that splits an owner still carrying two concerns? does the ingress->model->transform->egress pipeline close with NO loose arm across every sub-domain? does every seam in seamMap have a real owner on both ends (no dangle)? does each new sub-domain genuinely earn its folder? is any real durable-artifact gap (typography depth, layered/editable export, diagrams, ICC, machine-readable marks, template/format pipelines) still unfilled, or any admitted lib unjustified? are the workItems complete and ordered so the Author phase can execute every create/split/rename/merge/move with exact content-allocation? FIX the blueprint to the strongest defensible form and return the FINAL full blueprint (same shape).',
  'BLUEPRINT:\n' + JSON.stringify(bp, null, 1)].join('\n')

const installPrompt = (libs) => [LAW, '', CP315_TRUTH, '', AGGRESSIVE_ADD, '',
  'TASK: ADMIT + ACTUALLY INSTALL the blueprint-approved new libraries on cp315 (WRITE-FULLY; serial, you own the manifest pair). For EACH library below: (a) add a quoted PEP 508 requirement to the repo-root ' + PYPROJECT + ' [project].dependencies at the newest stable version, mirroring the existing artifacts pinning convention (inline `# artifacts: <capability>` comment; gated band only when a real cp315 build failure is observed, carrying `; python_version<\'3.15\'` + a one-line gate rationale). (b) Run `uv lock` then `uv sync` from the repo root. (c) Import-verify on cp315 with `uv run python -c "import <module>"` (use the real import module name, not the distribution name). PREFER a source-build over a marker; a marker is honest ONLY on a real, reported cp315 build failure. If a library genuinely cannot resolve/build on cp315, REVERT it entirely (remove its manifest row) and record it under reverted with the reason — never leave a broken manifest. Iterate until `uv lock` + `uv sync` resolve GREEN. Do NOT author .api catalogs or touch design pages here (later phases own those). Edit ONLY ' + PYPROJECT + ' and uv.lock. Return admitted (each {package, version, gated, marker, imported}), reverted, lockGreen, summary.',
  'LIBS TO ADMIT:\n' + JSON.stringify(libs, null, 1)].join('\n')

const catalogPrompt = (lib) => [LAW, '', CODE_BAR, '', ZERO_META, '',
  'TASK: AUTHOR the .api capability catalog for the newly-installed library ' + lib.package + ' at ' + FOLDER_API + '/' + lib.package + '.md (WRITE-FULLY). Match the HOUSE .api format of the sibling catalogs in ' + FOLDER_API + ' (read 2-3 first): a header (package / installed version / license / cp315 build-floor or marker), then member sections grouped by concern with backticked REAL symbols + signatures + a one-line consumer/boundary note. Document the package FULL advanced surface AND how it STACKS onto the artifacts owners (the runtime content-key, the ArtifactReceipt family, the host-free posture, the gated-band subprocess seam where relevant). Verify EVERY member against the actually-installed distribution via `uv run python -m tools.assay api` — cite only confirmed members, no phantoms. NO provenance / freshness / process tails. Edit ONLY the one .api file. Return {package, apiPath, verdict, members (count), summary}.',
  'LIB: ' + JSON.stringify(lib)].join('\n')

const authorPrompt = (wi, bp) => [DOCTRINE, '',
  'TASK: EXECUTE work-item ' + (wi.id || '') + ' (action=' + (wi.action || '') + ') of the blessed blueprint, then AUTHOR the resulting page(s) coherently (WRITE-FULLY). Read the source page(s) ' + JSON.stringify(wi.sources || []) + ' fully; apply the action to produce the target(s) ' + JSON.stringify(wi.targets || []) + ' with this exact content-allocation: ' + (wi.contentAllocation || '(see blueprint)') + '. For a SPLIT, distribute the god-file content across the new coherent owners with NO content dropped or duplicated, then DELETE the emptied source. For a RENAME or MOVE, use `git mv` to preserve history, creating the new subfolder if needed. For a MERGE, fold the sources into one owner and delete the consumed pages. Then AUTHOR each target page to the .planning PAGE SCHEMA at a FERTILE-GROUND baseline: the one polymorphic owner ' + (wi.owner || '(per blueprint)') + ', the unified ingress->model->transform->egress pipeline with NO loose arm, the typed ```python signature``` contract fence(s), the seams ' + JSON.stringify(wi.seams || []) + ' threaded to the shared content-key + ArtifactReceipt family, and any blueprint-admitted lib for this owner COMPOSED in place (cite its newly-authored .api members). Apply the GROUND_UP / DOMAIN_ELEVATION / CODE_BAR stance; no future/planned hedging; verify every cited member.',
  'EDIT ONLY design pages under ' + PLANNING + '/ — do NOT touch README.md, ARCHITECTURE.md, the .api catalogs, or ' + PYPROJECT + ' (later phases own those). Return {id, action, created, renamed, deleted, verdict, residual_high ({files:[...], claim} for any cross-FILE seam this work-item could not close alone), summary}.',
  'WORK-ITEM:\n' + JSON.stringify(wi, null, 1) + '\nBLUEPRINT TAXONOMY (for owner shapes + sibling seams):\n' + JSON.stringify(bp.taxonomy || [], null, 1)].join('\n')

const atlasPrompt = (bp, authored, admitted) => [DOCTRINE, '',
  'TASK: UPDATE THE GOVERNING DOCS to the NEW structure (WRITE-FULLY; serial, you own README + ARCHITECTURE). Read the current ' + ROOT + '/README.md and ' + ROOT + '/ARCHITECTURE.md and the now-authored page set. Rewrite to match the realized re-architecture EXACTLY: (1) README [01]-[ROUTER] — the routing rows now point to the real new/split/renamed pages with truthful one-line charters, no dangling row for a folded placeholder, no stale row for a deleted page. (2) README [02]-[DOMAIN_PACKAGES] — add the newly-admitted libs under the right sub-domain group with a one-line capability note, mirroring the existing entry style. (3) ARCHITECTURE [01]-[DOMAIN_MAP] codemap text tree — redraw the tree to the new subfolder/page structure with each node one-line annotated; remove folded placeholders, add new sub-domains. (4) ARCHITECTURE [02]-[SEAMS] — align every typed seam to the new owners (no seam to a deleted page, a real owner on both ends). Keep the [00]-style header prose truthful. Edit ONLY README.md + ARCHITECTURE.md. Return {verdict, residual_high, summary}.',
  'BLUEPRINT:\n' + JSON.stringify(bp, null, 1) + '\nAUTHORED PAGES:\n' + JSON.stringify(authored.map((r) => ({ id: r.id, created: r.created, renamed: r.renamed, deleted: r.deleted })), null, 1) + '\nADMITTED LIBS:\n' + JSON.stringify(admitted, null, 1)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Survey')
const surveys = (await pool(SURVEY_AREAS, CAP, (a) => agent(surveyPrompt(a), { label: 'survey:' + a.key, phase: 'Survey', schema: SURVEY_SCHEMA, effort: a.key === 'capability-research' ? 'xhigh' : 'high', stallMs: STALL }))).filter(Boolean)
log('Survey: ' + surveys.length + '/' + SURVEY_AREAS.length + ' areas mapped; ' + surveys.reduce((n, s) => n + ((s.candidateLibs || []).length), 0) + ' candidate lib(s), ' + surveys.reduce((n, s) => n + ((s.candidateSubdomains || []).length), 0) + ' candidate sub-domain(s) surfaced')

phase('Architect')
const proposed = await agent(proposePrompt(surveys), { label: 'architect:propose', phase: 'Architect', schema: BLUEPRINT_SCHEMA, effort: 'max', stallMs: STALL })
const critiqued = await agent(critiquePrompt(proposed), { label: 'architect:critique', phase: 'Architect', schema: BLUEPRINT_SCHEMA, effort: 'xhigh', stallMs: STALL })
const blueprint = await agent(redteamPrompt(critiqued), { label: 'architect:redteam', phase: 'Architect', schema: BLUEPRINT_SCHEMA, effort: 'max', stallMs: STALL })
const workItems = (blueprint && blueprint.workItems) || []
const libAdmissions = (blueprint && blueprint.libAdmissions) || []
log('Architect: blueprint decided — ' + ((blueprint && blueprint.taxonomy) || []).length + ' folder(s), ' + workItems.length + ' work-item(s), ' + libAdmissions.length + ' lib admission(s)')

phase('Install')
const install = libAdmissions.length
  ? await agent(installPrompt(libAdmissions), { label: 'install:libs', phase: 'Install', schema: INSTALL_SCHEMA, effort: 'max', stallMs: EXEC_STALL })
  : { admitted: [], reverted: [], lockGreen: true, summary: 'no new libs to admit' }
const admitted = ((install && install.admitted) || []).filter((p) => p.imported)
log('Install: ' + admitted.length + ' lib(s) installed on cp315, ' + (((install && install.reverted) || []).length) + ' reverted; lock green=' + (install && install.lockGreen))

phase('Catalog')
const catalogs = admitted.length
  ? (await pool(admitted, CAP, (lib) => agent(catalogPrompt(lib), { label: 'catalog:' + lib.package, phase: 'Catalog', schema: CATALOG_SCHEMA, effort: 'high', stallMs: EXEC_STALL }))).filter(Boolean)
  : []
log('Catalog: ' + catalogs.filter((c) => c.verdict === 'authored').length + '/' + admitted.length + ' .api catalog(s) authored')

phase('Author')
const workClusters = clusterBy(workItems, filesOf)
log('Author: ' + workItems.length + ' work-item(s) -> ' + workClusters.length + ' shared-file cluster(s) (serial within a cluster, pooled across)')
const authored = (await pool(workClusters, CAP, async (cluster) => {
  const logs = []
  for (const wi of cluster) {
    const r = await agent(authorPrompt(wi, blueprint), { label: 'author:' + (wi.id || (wi.action + ':' + (wi.targets || []).join(','))), phase: 'Author', schema: WORKLOG_SCHEMA, effort: 'max', stallMs: STALL })
    if (r) logs.push(r)
  }
  return logs
})).flat().filter(Boolean)
log('Author: ' + authored.filter((r) => r.verdict === 'done').length + '/' + workItems.length + ' work-item(s) done')

phase('Atlas')
const atlas = await agent(atlasPrompt(blueprint, authored, admitted), { label: 'atlas:docs', phase: 'Atlas', schema: ATLAS_SCHEMA, effort: 'xhigh', stallMs: STALL })
log('Atlas: README + ARCHITECTURE ' + ((atlas && atlas.verdict) || 'updated'))

// --- [RECONCILE] -- union-find the new pages by shared owner/seam PLUS deferred residuals --
const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: (x.files && x.files.length) ? x.files : [page], claim: x.claim }
const deferred = []
for (const r of authored) if (r.residual_high) for (const x of r.residual_high) deferred.push(norm(x, (r.created && r.created[0]) || (r.targets && r.targets[0]) || ROOT))
if (atlas && atlas.residual_high) for (const x of atlas.residual_high) deferred.push(norm(x, ROOT + '/ARCHITECTURE.md'))
const coherence = ((blueprint && blueprint.seamMap) || []).filter((s) => (s.pages || []).length >= 2).map((s) => ({ files: s.pages, claim: 'COHERENCE: unify the shared owner/seam "' + (s.label || '') + '" across these pages — consistent owner shape, aligned seam direction/contract, and the blueprint-admitted lib(s) composed (not dangling). ' + (s.note || '') }))
const residuals = [...deferred, ...coherence]
const uniq = [...new Map(residuals.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = clusterBy(uniq, (r) => r.files)
log('Reconcile: ' + deferred.length + ' deferred + ' + coherence.length + ' coherence seed(s) -> ' + uniq.length + ' residual(s) -> ' + clusters.length + ' cluster(s)')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent([DOCTRINE, '', 'TASK: RECONCILE this cluster of new/split artifacts pages — a cross-page COHERENCE + cross-FILE residual pass (WRITE-FULLY). NO severity: treat EVERY claim as must-address. Read EVERY listed page fully. Unify the shared owner shape across the pages, align every seam direction/contract, ensure each blueprint-admitted lib is actually COMPOSED into its owning page (no dangling .api), and resolve each cross-file claim — extend the shared owner to close a gap that spans pages, repair a duplication/altitude/seam issue, preserving all capability. If a claim is FACTUALLY INCORRECT, leave it and say why. Edit ONLY under ' + ROOT + '/. Claims:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix:' + i, phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
    if (!fix) return null
    const verify = await agent([LAW, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named pages from disk and classify each claim: "fixed" (real issue, now resolved — pages agree on the owner, the seam aligns, the lib composes), "invalid" (claim factually wrong — cite why), or "open" (real issue still NOT resolved). Default to "open" on any doubt. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: STALL })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))

phase('Verify')
const verdict = await agent([DOCTRINE, '',
  'TASK: FINAL ADVERSARIAL GATE over the re-architected artifacts folder (read-the-disk; FIX a trivially-closable defect in place, otherwise report it as a hard residual). Verify ALL of: (1) the new subfolder/page STRUCTURE is COHERENT — god-files split, figures regrouped, placeholders authored-or-folded, no orphan owner, every README router row and ARCHITECTURE codemap node and seam truthful against the real files. (2) EVERY page is WELL-FORMED per the .planning PAGE SCHEMA (H1 token, charter, [01]-[INDEX], per-owner sections, typed ```python signature``` fence, [..]-[RESEARCH] tail) with NO future/planned/deferred hedging. (3) every page is FERTILE GROUND for rebuild-python — a coherent polymorphic owner with a real growth axis, the new libs composed, seams aligned. (4) `uv lock` + `uv sync` resolve GREEN (re-run them from the repo root). (5) ruff/ty are CLEAN on the contract fences — extract each ```python``` fence under ' + PLANNING + ' to a temp file and run `uv run ruff check` and `uv run ty check` over the authored/changed pages where the fence is self-contained; report staticClean. Return {coherent, lockGreen, staticClean, wellFormed, fertile, hard_residual ({files:[...], claim} for every UNRESOLVED issue), summary}.',
  'OPEN RECONCILE CLAIMS (already flagged unresolved — include any still open in hard_residual):\n' + JSON.stringify([...openClaims], null, 1)].join('\n'), { label: 'verify:gate', phase: 'Verify', schema: VERIFY_SCHEMA, effort: 'xhigh', stallMs: EXEC_STALL })

const hard_residual = [...(verdict && verdict.hard_residual ? verdict.hard_residual : []), ...uniq.filter((r) => openClaims.has(r.claim))]
log('Verify: coherent=' + (verdict && verdict.coherent) + ' wellFormed=' + (verdict && verdict.wellFormed) + ' fertile=' + (verdict && verdict.fertile) + ' lockGreen=' + (verdict && verdict.lockGreen) + ' staticClean=' + (verdict && verdict.staticClean) + '; ' + hard_residual.length + ' hard residual(s) -> resolve-residuals')
return {
  workflow: 'rearchitect-artifacts',
  root: ROOT,
  blueprint: { folders: ((blueprint && blueprint.taxonomy) || []).length, workItems: workItems.length, libAdmissions: libAdmissions.length },
  installed: admitted.map((p) => p.package),
  reverted: ((install && install.reverted) || []).map((p) => p.package),
  catalogs: catalogs.filter((c) => c.verdict === 'authored').length,
  authored: authored.filter((r) => r.verdict === 'done').length,
  total_work_items: workItems.length,
  reconcile_clusters: clusters.length,
  verdict: verdict,
  hard_residual: hard_residual,
}
