export const meta = {
  name: 'sibling-ripple',
  description: 'WF-3 sibling-ripple — re-bind the named sibling sites (Compute Analysis, Persistence Element, the Fabrication seam rows, the python and typescript wire decoders) to the Component/detail-schema wire WF-1 + WF-2 landed: a thin per-site ripple-discover, a single validate barrier that drops phantom ripples, an in-place decode-not-remint rebind, then a no-defer reconcile fix-verify loop plus a sanity drive-to-zero. Re-opens no Element/Materials/Bim design.',
  whenToUse: 'After WF-1 (element-component) + WF-2 (bim-rebuild) land the Element/Materials/Bim changes; re-bind only the sibling consumer sites to the changed seam.',
  phases: [
    { title: 'Discover' },
    { title: 'Validate' },
    { title: 'Rebind' },
    { title: 'Resolve' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const STALL = 600000
const MAX_ROUNDS = 6
const SANITY_CAP = 6
const IMPLEMENT_BATCH = 2
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const CORE_CS = 'docs/stacks/csharp/{README,language,shapes,surfaces-and-dispatch,rails-and-effects,boundaries,algorithms,system-apis}.md'
const DOMAIN_ROSTER = 'docs/stacks/csharp/domain/<shard>.md ENUMERATED ROSTER (13): runtime, concurrency, diagnostics, ' +
  'validation, resilience, transport, persistence, durability, postgres, data-interchange, compute, visuals, ' +
  'interaction. MAP each page concern -> its required shard(s): IFC/glTF/STEP/wire codec -> transport + ' +
  'data-interchange; Pset/Qto + property/admission validation -> validation; spatial/graph/catalog persistence -> ' +
  'persistence + durability; clash/solve/numeric -> compute; telemetry/receipts -> diagnostics; retry/backoff -> ' +
  'resilience; free-threading/subinterpreter -> runtime + concurrency; SQL/RLS -> postgres; UI/interaction -> ' +
  'interaction; appearance/render -> visuals.'
const API_TIERS = 'BOTH `.api` TIERS: the SHARED substrate tier `libs/csharp/.api/**` (universal cross-cutting catalogs + ' +
  'the Thinktecture / LanguageExt rails) AND the package FOLDER tier `<package>/.api/**` (domain catalogs). For the ' +
  'python/ts sites the substrate is the language-stack `.api` substrate where present plus the package folder catalogs.'
const STACK = {
  cs: 'docs/stacks/csharp/** (README + language + shapes + surfaces-and-dispatch + ' +
    'rails-and-effects + boundaries + algorithms + system-apis + the relevant domain/ shard)',
  py: 'docs/stacks/python/** (the core pages + the relevant domain/ shard)',
  ts: 'docs/stacks/typescript/** (the coding-ts route: core pages + the relevant domain shard)',
}
const SITES = [
  { name: 'Compute/Analysis', root: 'libs/csharp/Rasm.Compute/.planning/Analysis', lang: 'cs',
    bind: 're-bind the analysis consumers to the changed seam; honor scope [1].4 resolution-through-Component / ' +
      'type-resolved accessors and do NOT silently break Compute Op-free graph.SectionOf(member)' },
  { name: 'Persistence/Element', root: 'libs/csharp/Rasm.Persistence/.planning/Element', lang: 'cs',
    bind: 're-bind to the changed seam / Type-node (ObjectKind.Type) shape; surface the Type-node persistence ' +
      'per scope [3] M4 without re-opening the egress-owner decision' },
  { name: 'Fabrication/seam-rows', root: 'libs/csharp/Rasm.Fabrication/.planning', lang: 'cs',
    bind: 're-bind the Fabrication seam rows per scope [5]: the Construction/nesting consumer -> Fabrication/Nesting ' +
      'and the Properties consumer -> Fabrication/Process' },
  { name: 'python/data/decode', root: 'libs/python/data/.planning', lang: 'py',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the data decoders' },
  { name: 'python/geometry/mesh', root: 'libs/python/geometry/.planning/mesh', lang: 'py',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the mesh decoders' },
  { name: 'typescript/interchange/codec', root: 'libs/typescript/interchange/.planning/Codec', lang: 'ts',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the interchange codec' },
]

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const UNDERUTIL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'capability'], properties: { catalog: { type: 'string' }, capability: { type: 'string' } } } }
const RIPPLE = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['detail'], properties: { detail: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, pages: { type: 'array', items: { type: 'string' } }, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['site', 'ripples', 'summary'], properties: { site: { type: 'string' }, ripples: RIPPLE, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const VALIDATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['validated', 'summary'], properties: { validated: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['site', 'ripples'], properties: { site: { type: 'string' }, ripples: RIPPLE, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } } } } }, dropped: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const FOLDER_FIXLOG = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['closed', 'hardened', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SANITY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'items'], properties: { overall: { type: 'boolean' }, items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'open'] }, evidence: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm Materials/Bim/Element rebuild — WF-3 sibling-ripple. READ `' + SCOPE + '` (repo root) in ' +
    'FULL FIRST; a prompt POINTS to a scope section by number and never restates scope.',
  'CROSS-WORKFLOW CONTRACT: WF-1 (element-component) + WF-2 (bim-rebuild) ALREADY landed the Element/Materials/Bim ' +
    'design changes AND the seam-mirror rows; WF-3 ONLY re-binds the named sibling consumer sites to the changed ' +
    'Component/detail-schema wire and records each site`s seam row in its OWN `ARCHITECTURE.md` `[2]-[SEAMS]` where ' +
    'applicable. Do NOT re-open Element/Materials/Bim design here.',
  'These are DESIGN-PAGE specs (`.planning` markdown) — code fences ARE the product (transcription-complete, ' +
    'decompile-verified, implementation-ready).',
].join('\n')
const DUAL = [
  'DUAL-AXIS READ (scope `[6]`): every implementing stage reads BOTH axes and finalizes a page only when a cold ' +
    'read surfaces nothing. CODE doctrine — the per-site language stack pages by name (the injected `STACK` ' +
    'pointer). DOC-CRAFT — `libs/.planning/README.md` `[PLANNING_STANDARD]` (the doc-set, the four index templates, ' +
    'the design-page grammar, the `page#CLUSTER` integration notation) + its `[06]` cold-grade REVIEW gate (the ' +
    'doc-finalization gate); the three `docs/standards/` form standards + `docs/standards/proof.md` claim discipline.',
  'BANNED HEDGES (scope `[6]`, word-boundary, page-wide): should/could/would/might/maybe/perhaps/likely/probably/' +
    'propose/consider/recommended/ideally/TBD/TODO/FIXME/we/our/you + the synonym forms (is expected to, can be, ' +
    'aims to, is designed to, in the future, eventually, as needed, if necessary); future tense is legal only on a ' +
    'card growth line + a RESEARCH item.',
  'ZERO-PROVENANCE (scope `[6]`): no reader address, narration, process, source provenance, source-mining history, ' +
    'freshness disclaimers, checklist tails — and on a design page no links, URLs, versions, dates, or session context.',
].join('\n')
const API = [
  'TWO-TIER API (scope `[7]`): cite ONLY the `.api/` catalogs the concept composes — the SHARED substrate tier AND ' +
    'the folder tier — never noise. Members are verified-local truth via `uv run python -m tools.assay api` (a ' +
    'member you cannot verify is a PHANTOM — drop it).',
  'Live NuGet feed routes through the nuget MCP (ToolSearch-load `mcp__nuget__get_latest_package_version` + ' +
    '`mcp__nuget__get_package_context` by exact name before use); internal/library API shape resolves through ' +
    'Context7 (ToolSearch-load `mcp__plugin_context7_context7__resolve-library-id` + ' +
    '`mcp__plugin_context7_context7__query-docs`). Verified-local wins on conflict.',
].join('\n')
const DECODE = [
  'DECODE-NOT-REMINT (the WF-3 KEY LAW, scope `[1].5` + `[5]`): C# owns the wire vocabulary; the python/ts decoders ' +
    'RE-BIND to the changed Component/detail-schema wire and NEVER re-mint the C# contract. The neutral detail ' +
    'schema is Element-declared over `PropertyBag` + the canonical `PropertyName` vocabulary (scope `[1].5`, ' +
    'NEUTRAL — no `Pset_*` IFC names in the seam).',
  '`ProfileRef`/`ProfileSet`/`ComputedSection` STAY seam-canonical (scope `[5]`, the M7 one-hop resolution) — ' +
    '`Component` composes them unchanged and the semantic-rename STOPS at the Materials folder boundary.',
  'C#-SIBLING RE-BIND (scope `[1].4`): the analysis/persistence consumers resolve through the Component ' +
    '(resolution-through-Component / type-resolved accessors) WITHOUT silently breaking Compute Op-free ' +
    '`graph.SectionOf(member)`; the Type-node (`ObjectKind.Type`) shape and the named Bake type->occurrence ' +
    'inheritance are the seam WF-1 landed, consumed here, not re-authored.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY implementing stage is HOSTILE: assume the page is NAIVE, SHALLOW, JUNIOR, STALE, or ' +
    'ILLUSORY until it survives an aggressive attack, REGARDLESS of how it looks or what any prior pass claimed; the ' +
    'burden of proof is ON THE PAGE, never on you. "Mature", "already strong", "good enough", "done", and a prior ' +
    'clean verdict are REJECTED self-assessments — a no-edit verdict is earned ONLY after a genuinely aggressive ' +
    'attack on the real domain + the verified package surface finds nothing. A confident edit that does not TRULY ' +
    're-bind to the changed wire is INCOMPLETE, never done. Aggression is DEPTH and RIGOR, never churn — every edit ' +
    'cites a source (a package member, a domain attribute, a consumer contract); churn-for-appearance is rejected.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code PRETENDS to be advanced: it uses the ' +
    'doctrine vocabulary, cites packages, reads dense and confident, yet is HOLLOW. Treat dense, confident-looking ' +
    'fences with MORE suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify ' +
    'it against the real domain + the catalogued package surface. HUNT: a name/signature/prose promising capability ' +
    'the body does not implement; a "rich" decoder that decodes a thin slice of the wire; a stale decode left ' +
    'mid-flight; a `.api`/host member cited but never verified (a phantom). Every such illusion is a DEFECT to ' +
    'rebuild — never invent churn to look busy.',
  'WRITE-FULLY: every re-bind you identify you MAKE NOW via Edit/Write; the returned log REPORTS edits already ' +
    'made, never a to-do. A re-bind spanning a FILE you do not own goes to `residual_high` as `{files:[...], ' +
    'claim}` (the resource slot is a LIST so a cross-file seam names EVERY spanned file) for the terminal RESOLVE, ' +
    'which has NO scope cap.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the named docs/stacks laws held as fact: [FLOW] EXPRESSION_SPINE (domain logic is ' +
    'expression-shaped; dependent steps bind monadically, independent ones accumulate applicatively; the carrier ' +
    'selects the algebra) + BOUNDARY_ADMISSION (raw admitted EXACTLY ONCE into an evidence-carrying owner; interior ' +
    'never re-validates). [SHAPE] SHAPE_BUDGET (one concept owns ONE type; variants are cases in one closed family) + ' +
    'DEEP_SURFACES + MODAL_ARITY (one entrypoint owns every modality, discriminating on input shape) + ' +
    'ANTICIPATORY_COLLAPSE. [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + DERIVED_TYPES + SEMANTIC_NAMING. [MATERIAL] ' +
    'LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD (weave new capability into the owner as if ' +
    'always present; no shims/aliases/migration layers) + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION.',
  'ULTRA-ADVANCED COLLAPSE MANDATE: COLLAPSE >=3 parallel types / sibling factory methods / repeated dispatch arms ' +
    '/ single-call private helpers into ONE polymorphic owner IN THE SAME FILE (the language-doctrine ADT/union/' +
    'smart-enum/value-object/source-generated case family / fold algebra / frozen data table) — never extract a new ' +
    'file to reduce LOC, never delete capability.',
  'STACK CAPABILITY: ' + API_TIERS + ' MINE both tiers and LAYER the universal substrate rails onto the domain ' +
    'packages, woven as ONE dense rail at the DEEPEST operator/combinator/generated surface each package reaches ' +
    '(LIBRARY_DEPTH) — NOT flat one-shot per-API uses, NOT a surface-level subset, NOT a thin rename wrapper, NOT a ' +
    'BCL/stdlib-first reflex. A decoder that leaves an admitted `.api` capability the wire ADMITS unexploited is a ' +
    'capability-incompleteness defect. PRESERVE all capability (densify, never delete functionality); regress no ' +
    'correctness or strata law.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse + `.api`-stacking are NECESSARY ' +
    'but NOT SUFFICIENT: a fully-collapsed decoder can still re-bind a NAIVE slice of the changed wire (the obvious 3 ' +
    'fields where the wire carries fifteen; a 2-case match where the seam has twenty). Close the gap by GROWING the ' +
    'existing owner — a case in the closed family, a row/richer column on the smart-enum or frozen table, a field, an ' +
    'operation, a policy value — per ANTICIPATORY_COLLAPSE + ROOT_REBUILD, NEVER a parallel type, a new file, or flat ' +
    'appended code.',
  'GAP SOURCES (every extension cites exactly one): (a) PACKAGE — a member the admitted `.api`/host surface exposes ' +
    'that the wire ADMITS but the page IGNORES (the `apiUnderutilized` suggestions; verify via `assay api`); (b) ' +
    'DOMAIN — an attribute/field/sub-kind/relationship the REAL changed seam demands but the decoder omits; (c) ' +
    'CONSUMER — a contract a downstream owner requires that has no composed spelling here yet. JUSTIFIED, NOT RANDOM: ' +
    'if after a real domain + package sweep the re-bind is genuinely complete, prove it by adding nothing — every ' +
    'added case/field/operation is load-bearing and cites its source; preserve ALL existing capability.',
].join('\n')
const READMANDATE = [
  'DOWNSTREAM READ MANDATE — BEFORE editing, READ: (1) ALL root `' + CORE_CS + '` core pages — the full set, every ' +
    'stage, never a subset (the C# floor governs the wire vocabulary even for the python/ts decoders; the python/ts ' +
    'sites ALSO read their own language stack `' + STACK.py + '` / `' + STACK.ts + '`). (2) the work-item`s ' +
    '`domainShards` — and ONLY those required shards (focused, not all 13). (3) the work-item`s `apiUsed` catalogs at ' +
    'full operator depth AND the `apiUnderutilized` capability to STACK into the owner (closing the underutilization) ' +
    '— BOTH `.api` tiers. (4) `' + SCOPE + '` for the central goal. ' + DOMAIN_ROSTER,
].join('\n')
const DOCTRINE = [LAW, '', DUAL, '', API, '', DECODE, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', READMANDATE].join('\n')

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
const chunk = (xs, n) => { const out = []; for (let i = 0; i < xs.length; i += n) out.push(xs.slice(i, i + n)); return out }
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const SITE_BY = new Map(SITES.map((s) => [s.name, s]))
const discoverPrompt = (s) => [DOCTRINE, '', 'TASK: RIPPLE DISCOVERY for the sibling site `' + s.name + '` (folder `' + s.root + '/**`, CODE stack `' + STACK[s.lang] + '`). ' +
  'READ-ONLY — investigate, do NOT edit. Re-read `' + SCOPE + '` then EVERY page under `' + s.root + '/**` AND the CHANGED seam shape WF-1+WF-2 landed (the Element `Graph`/`Component` ' +
  'seam, the one seam-declared NEUTRAL detail schema over `PropertyBag` + `PropertyName`, the `ObjectKind.Type` ' + 'Type-node + named Bake inheritance). SITE RE-BIND CHARTER: ' + s.bind + '. ' +
  'ENUMERATE both `.api` tiers (' + API_TIERS + ') and the full domain roster (' + DOMAIN_ROSTER + '). For EACH discovered page, MAP its composed catalogs (`apiUsed`, BOTH tiers), the ' +
  'catalogs/members it SHOULD compose but does not (`apiUnderutilized` as {catalog, capability} — the underutilization the re-bind must close), and the REQUIRED domain shard(s) ' +
  '(`domainShards`) its concern demands; members verified-local via `uv run python -m tools.assay api`. ' +
  'Return the PRECISE ripple work-list for this site ONLY — which seam signatures / decoders shifted and what each page MUST ' + 'change to re-bind (decode-not-remint for the python/ts ' +
  'decoders; resolution-through-Component for the C# consumers). Each ' + 'ripple is {detail, files, pages (the design pages the ripple touches), apiUsed, apiUnderutilized, domainShards} ' +
  'so the per-ripple reading map travels with it. ALSO return the site-level rollup apiUsed + apiUnderutilized + domainShards across all ripples. Return site + ripples + apiUsed + ' +
  'apiUnderutilized + domainShards + a summary of the dominant ripple class.'].join('\n')
const validatePrompt = (discovered) => [DOCTRINE, '', 'TASK: VALIDATE the discovered ripple across ALL sites (single barrier pass). READ-ONLY — investigate, do NOT edit. For EACH ' +
  'discovered ripple: CONFIRM it is REAL — the seam shape WF-1+WF-2 landed ACTUALLY changed ' + 'in the direction the ripple claims, and every cited member (including the apiUsed/apiUnderutilized ' +
  'catalogs) is verified via `assay api` / Context7. DROP any PHANTOM ripple (a member that does not exist, a ripple whose seam ' + 'did not actually change, or one that would re-mint the ' +
  'C# contract rather than decode it), and DROP any phantom `apiUnderutilized` suggestion (a catalog/member that does not exist or the page does not actually admit). PRESERVE each surviving ' +
  'ripple`s {detail, files, pages, apiUsed, apiUnderutilized, domainShards} map intact AND the site-level apiUsed/apiUnderutilized/domainShards rollup so the reading map reaches Rebind. Return ' +
  'the VALIDATED re-bind list per site (site + the SURVIVING ripples + apiUsed + apiUnderutilized + domainShards) + dropped (the phantom ripples/suggestions, each with the one-line reason) + ' +
  'summary. DISCOVERED:\n' + JSON.stringify(discovered, null, 1)].join('\n')
const rebindPrompt = (s) => [DOCTRINE, '', 'TASK: HOSTILE RE-BIND of the sibling site `' + s.name + '` (folder `' + s.root + '/**`, CODE stack `' + STACK[s.lang] + '`) to the changed ' +
  'Component/detail-schema wire — IN PLACE, decode-not-remint for the python/ts decoders, ' + 'resolution-through-Component for the C# consumers. DISBELIEVE every existing decoder — assume it is ' +
  'naive/stale/illusory until proven world-class; do NOT polish, REBUILD the re-bind to the strongest CLEAN/MODERN form the site language doctrine + the DECODE-NOT-REMINT law admit, treating ' +
  'dense confident-looking code as a prime suspect for hollow re-binding. READING MAP (read ALL before editing per the DOWNSTREAM READ MANDATE): apiUsed=' + JSON.stringify(s.apiUsed || []) +
  '; apiUnderutilized=' + JSON.stringify(s.apiUnderutilized || []) + '; domainShards=' + JSON.stringify(s.domainShards || []) + '. For EACH validated ripple below, MAKE the re-bind edit NOW, ' +
  'COMPOSE its apiUsed catalogs at full operator depth AND STACK the apiUnderutilized capability into the owner (closing the underutilization) across BOTH `.api` tiers, preserving ' +
  '`ProfileRef`/`ProfileSet`/`ComputedSection` seam-canonical ' + 'and never re-minting the C# contract; record the site`s ' + 'seam row in its `ARCHITECTURE.md` `[2]-[SEAMS]` where applicable. Do ' +
  'NOT re-open Element/Materials/Bim design. Every edit cites a source (a package member / domain attribute / consumer contract) — no churn-for-appearance. A re-bind ' +
  'requiring a FILE outside this site goes to residual_high {files, claim}. This is a FOCUSED 2-ripple BATCH of the site`s validated ripples (the rest of the site is re-bound by sibling batch ' +
  'agents in parallel) — re-bind THESE ripples completely + aggressively. VALIDATED RIPPLES IN THIS BATCH (' + s.ripples.length + ', each carrying its own pages/apiUsed/apiUnderutilized/domainShards):\n' +
  JSON.stringify(s.ripples, null, 1) + '\nReturn folder (the EXACT ' + 'site name `' + s.name + '`) + verdict + integrated (each ripple re-bound + where) + extended + residual_high + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE re-bind residuals the discover/validate/rebind phases surfaced; NO severity, NO ' +
  'leftovers, NO deferral, NO scope cap. Read EVERY listed file across the sibling sites (csharp + python + ts) ' + 'and FIX the real cross-file defect in place to the strongest clean/modern + ' +
  'decode-not-remint form (align the changed wire + every consumer in lockstep, ' + 'preserve `ProfileRef`/`ProfileSet`/`ComputedSection` seam-canonical, never re-mint the C# contract), ' +
  'preserving all capability — a token patch that leaves the wire misaligned ' + 'is NOT a fix; if a residual is FACTUALLY WRONG, leave it and say why. If your fix surfaces a new cross-file ' +
  'need, report it in residual_high. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', DECODE, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and CONFIRM the ' +
  're-bind is ACTUALLY made AND complete + clean/modern/decode-not-remint, ' + 'not a token/naive patch. ATTACK it: shallow, partial, a rename that left a sibling decoder stale, the wire still ' +
  'misaligned, a re-minted C# contract? ' + 'Classify each: "fixed" (real, complete, non-naive), "invalid" (claim PROVABLY WRONG — cite why), or "open" (NOT fixed, OR fixed naively — a ' +
  'confident edit that does not truly resolve the cross-file defect is open, never fixed). Default "open" on ANY doubt. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer ' +
  'touched: ' + JSON.stringify(fixFiles)].join('\n')
const sanityPrompt = (items) => [DOCTRINE, '', 'TASK: SANITY RE-AUDIT over the UNION of ALL ' + items.length + ' surfaced re-bind residuals. For EACH item: re-read the cited file(s) ' +
  'from disk and CONFIRM the re-bind is GENUINELY + CLEANLY done — the changed wire is honored on BOTH endpoints, the decode-not-remint law holds, ' +
  '`ProfileRef`/`ProfileSet`/`ComputedSection` stay seam-canonical, no token patch, ' + 'no sibling decoder left stale, no C# contract re-minted. Be adversarial: a confident-looking edit that ' +
  'does not truly resolve the cross-file defect is "open". Classify each item "fixed" or "open" with one-line evidence; default "open" on ANY doubt. ITEMS:\n' +
  items.map((it, i) => (i + 1) + '. [' + it.files.join(', ') + '] ' + it.claim).join('\n') + '\nReturn overall (true only if ALL fixed) + items (per-item status + evidence) + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('sibling-ripple: ripple-discover (' + SITES.length + ' sibling sites) -> validate -> rebind (decode-not-remint) -> terminal resolve (reconcile + sanity drive-to-zero); CAP=' + CAP)

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(SITES, CAP, (s) => agent(discoverPrompt(s), { label: 'discover:' + s.name, phase: 'Discover', schema: DISCOVER_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
log('Discover: ' + discovered.reduce((n, d) => n + (d.ripples || []).length, 0) + ' ripple(s) across ' + discovered.length + ' sites')

// --- [VALIDATE]
phase('Validate')
const validation = await agent(validatePrompt(discovered), { label: 'validate', phase: 'Validate', schema: VALIDATE_SCHEMA, effort: 'xhigh', stallMs: STALL })
const validated = (validation && validation.validated) || []
const validatedBySite = new Map(validated.map((v) => [v.site, v]))
const discoveredBySite = new Map(discovered.map((d) => [d.site, d]))
log('Validate: ' + validated.reduce((n, v) => n + (v.ripples || []).length, 0) + ' real ripple(s); ' + (((validation && validation.dropped) || []).length) + ' phantom dropped')

// --- [REBIND]
// Thread the per-site reading map (apiUsed/apiUnderutilized/domainShards) from validate into each rebind agent,
// falling back to the discover-stage site rollup so the map survives even if validate omitted the rollup. The
// IMPLEMENT pass batches at IMPLEMENT_BATCH ripples per agent (no implement agent handles more than 2 files at
// once) — each site's validated ripples chunk into 2-ripple sub-batches, 1 agent per batch via the shared pool.
phase('Rebind')
const rebindBatches = SITES.flatMap((s) => {
  const v = validatedBySite.get(s.name) || {}
  const d = discoveredBySite.get(s.name) || {}
  const apiUsed = v.apiUsed || d.apiUsed || []
  const apiUnderutilized = v.apiUnderutilized || d.apiUnderutilized || []
  const domainShards = v.domainShards || d.domainShards || []
  return chunk(v.ripples || [], IMPLEMENT_BATCH).map((ripples, b) => ({ ...s, ripples, apiUsed, apiUnderutilized, domainShards, batch: b }))
})
const rebound = (await pool(rebindBatches, CAP, (s) => agent(rebindPrompt(s), { label: 'rebind:' + s.name + ':b' + s.batch, phase: 'Rebind', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [RESOLVE]
const fbOf = (r) => { const site = SITE_BY.get(r.folder); return site ? site.root : 'libs/csharp' }
const keyOf = (r) => r.files.slice().sort().join(',') + '|' + r.claim
let union = dedup(rebound.flatMap((r) => (r.residual_high || []).map((x) => norm(x, fbOf(r)))))
const seen = new Set(union.map(keyOf))
let pending = union.slice()
let invalid = []
let noFix = []
let round = 0
if (pending.length) {
  phase('Resolve')
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log('Resolve round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (sibling-wide, no-defer)')
    const resolved = (await pool(clusters, CAP, async (cl) => {
      const fix = await agent(reconcileFix(cl), { label: 'resolve-fix:r' + round, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL })
      const touched = (fix && Array.isArray(fix.files) ? fix.files.filter(inLibs) : [])
      // No file-changing progress: the fix found nothing to change -> the cluster is resolved-or-phantom; skip the mandatory verify and drop it (recorded as noFix).
      if (!fix || touched.length === 0 || fix.verdict === 'clean') return { open: [], invalid: [], surfaced: [], dropped: cl, changed: false }
      const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify:r' + round, phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, 'libs/csharp')), dropped: [], changed: true }
    })).filter(Boolean)
    invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
    noFix = dedup([...noFix, ...resolved.flatMap((r) => r.dropped)])
    const invalidKeys = new Set(invalid.map((r) => r.claim))
    const surfaced = resolved.flatMap((r) => r.surfaced)
    // Re-enter ONLY genuinely-new residuals: a key already queued this run cannot re-enter (stops a phantom re-feeding every round).
    const fresh = dedup([...resolved.flatMap((r) => r.open), ...surfaced]).filter((r) => !invalidKeys.has(r.claim) && !seen.has(keyOf(r)))
    fresh.forEach((r) => seen.add(keyOf(r)))
    union = dedup([...union, ...surfaced]).filter((r) => !invalidKeys.has(r.claim))
    pending = fresh
    // NO-PROGRESS BREAK: no cluster changed a file this round -> the remaining residuals are phantom/unfixable; stop instead of grinding to MAX_ROUNDS.
    if (!resolved.some((r) => r.changed)) { log('Resolve round ' + round + ': no file-changing progress — ' + noFix.length + ' residual(s) had nothing to fix (phantom/resolved); breaking'); pending = []; break }
  }
  if (pending.length) log('Resolve: ' + pending.length + ' still open after ' + MAX_ROUNDS + ' reconcile rounds -> handed to the sanity drive-to-zero')
}

// --- [SANITY]
// DRIVE TO ZERO: re-audit the UNION of every surfaced residual; force-close PER union-find cluster (files preserved) + re-audit until nothing is open. The cap is a runaway backstop.
const fileOf = new Map(union.map((r) => [r.claim, r.files]))
let sanity = null
let sanityOpen = []
let saneRound = 0
if (union.length) {
  sanity = await agent(sanityPrompt(union), { label: 'sanity', phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
  sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
  while (sanityOpen.length && saneRound < SANITY_CAP) {
    saneRound++
    const prevOpen = sanityOpen.length
    const openRes = sanityOpen.map((i) => ({ files: fileOf.get(i.claim) || ['libs/csharp'], claim: i.claim }))
    const clusters = cluster(openRes)
    log('Sanity round ' + saneRound + ': ' + sanityOpen.length + ' OPEN -> ' + clusters.length + ' cluster(s) FORCE-CLOSE (files preserved) + re-audit; nothing leaves open')
    const forced = (await pool(clusters, CAP, (cl) => agent(reconcileFix(cl), { label: 'sanity-force-close:r' + saneRound, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
    const fcTouched = forced.flatMap((fc) => (fc && Array.isArray(fc.files) ? fc.files.filter(inLibs) : []))
    // The force-close changed nothing: the remaining items are phantom/unfixable; skip the re-audit and stop.
    if (fcTouched.length === 0) { log('Sanity round ' + saneRound + ': force-close changed no files — ' + sanityOpen.length + ' remaining item(s) phantom/unfixable; breaking'); break }
    sanity = await agent(sanityPrompt(union), { label: 'sanity:r' + saneRound, phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
    sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
    // No net decrease across the force-close -> drive-to-zero has stalled; stop instead of grinding to SANITY_CAP.
    if (sanityOpen.length >= prevOpen) { log('Sanity round ' + saneRound + ': no net progress (' + sanityOpen.length + ' open, was ' + prevOpen + ') — remaining item(s) phantom/unfixable; breaking'); break }
  }
  if (sanityOpen.length) log('Sanity: ' + sanityOpen.length + ' STILL OPEN after the force-close drive — HARD BLOCKER, reported LOUDLY, never silently dropped')
  else log('Sanity: ALL ' + union.length + ' surfaced residual(s) CLOSED + verified across ' + saneRound + ' force-close round(s)')
} else { log('Resolve: no residuals surfaced — clean') }

return {
  workflow: 'sibling-ripple', sites: SITES.map((s) => s.name),
  ripples: discovered.map((d) => ({ site: d.site, ripples: (d.ripples || []).length })),
  dropped: ((validation && validation.dropped) || []).length,
  rebindVerdicts: rebound.map((r) => ({ site: r.folder, verdict: r.verdict })),
  resolveRounds: round, saneRounds: saneRound,
  invalidClaims: invalid.map((x) => x.claim),
  noFix: noFix.map((x) => ({ files: x.files, claim: x.claim })),
  reconcilePending: pending.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: sanityOpen.map((i) => ({ claim: i.claim, evidence: i.evidence })),
}
