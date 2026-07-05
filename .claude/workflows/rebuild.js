export const meta = {
  name: 'rebuild',
  whenToUse: 'The standing rebuild engine for any libs/ planning corpus: pass targets (file / sub-folder / package root, any number) and optionally a campaign brief with a leg selector; it plans, discovers, hostile-implements, critiques, and red-teams in dependency-ordered waves at the owning-language doctrine bar — one serialized tail per wave owns the shared central files, one fix-empowered acceptance agent closes the run.',
  description: 'Durable language-agnostic rebuild engine over libs/{csharp,python,typescript} planning corpora. args = a target path, an array, or {targets, brief, leg, waves, riders, acceptance}; empty = no-op; language derives from the target root (or the RASM-<CS|PY|TS>- brief name when a leg supplies the targets) and selects the doctrine, both .api tiers, casing, and the member-verification rail. Plan (1 sonnet) expands targets to pages IN DEPENDENCY ORDER; with a brief it admits ONLY brief-named pages classified against real disk state (kind new/rebuild/improve + deletions + absorb pairs + typed rider rows + acceptance traces + wave/leg partition; an unnamed page is out of scope; an out-of-order leg halts via upstreamMissing); without one the owning-package charter owns scope (full expansion, charter-demanded new, settled-page skip). Then per WAVE and per PACKAGE LANE: Discover (opus, read-only, 1 per 4 pages) deep-reads pages + folder + BOTH .api tiers + the doctrine, emits per-page reading maps, and writes a grounding dossier of verified primary extracts; Build runs a bounded per-batch pipeline in dependency order — implement then critique then redteam (all fable), each batch chained behind its predecessor so consumers author against landed foundations, page-routed riders receipt-forced in the implement pass; a delete executor handles brief-declared deletions verify-then-delete; ONE serialized per-wave TAIL (fable) is the sole writer for package index docs and central manifests (applies reported indexRows + serial riders, audits rider receipts); a failed page halts before dependent waves, landed waves intact. Close: one fail-open acceptance agent (fable) runs the brief dry-run traces plus a generic cross-page symbol-resolution sweep, fixing every surgical miss in place. No reconcile stage: every agent repairs the cross-file ripples its own work exposes in its own pass; shared central files have exactly one writer.',
  phases: [
    { title: 'Plan', detail: 'one thin agent expands the targets into the dependency-ordered page list; with a brief it admits only brief-named pages (kinds from disk, riders + acceptance traces transcribed, wave/leg partition honored); without one the owning-package charter owns scope', model: 'sonnet' },
    { title: 'Discover', detail: 'per wave, per package lane: hostile read-only discovery, 1 agent per 4 pages — deep-read each page + the folder + BOTH .api tiers + the doctrine at source, emit per-page reading maps, write the grounding dossier of verified primary extracts', model: 'opus' },
    { title: 'Build', detail: 'per wave, per package lane: bounded per-batch pipeline in dependency order — implement then critique then redteam, each batch chained behind its predecessor; page riders receipt-forced; brief deletions verify-then-delete; one serialized tail applies index rows + serial riders and audits receipts' },
    { title: 'Close', detail: 'one fail-open acceptance agent runs the brief dry-run traces plus the generic cross-page symbol-resolution sweep and FIXES every surgical miss in place — pass/fixed/miss verdicts, nothing merely reported that an edit can close', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const STALL = 300000
const DISCOVER_BATCH = 4 // policy value; discover rides the build batching so dossier paths align
const IMPL_BATCH = 4 // policy value; the dossier landing licenses 2-3 later — never below it
const SCRATCH = '.claude/scratch/rebuild-grounding' // deterministic dossier root

// --- [INPUTS] ----------------------------------------------------------------------------

const normTarget = (t) => String(t).trim().replace(/\/+$/, '').replace(/^\/+/, '')
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const isObj = !!argsIn && typeof argsIn === 'object' && !Array.isArray(argsIn)
const rawTargets = Array.isArray(argsIn) ? argsIn
  : (isObj && Array.isArray(argsIn.targets)) ? argsIn.targets
  : (isObj && typeof argsIn.targets === 'string') ? [argsIn.targets]
  : (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn]
  : []
const BRIEF = (isObj && typeof argsIn.brief === 'string' && argsIn.brief.trim()) ? argsIn.brief.trim() : ''
const rawLeg = (isObj && argsIn.leg != null) ? argsIn.leg : null
const LEG = BRIEF ? rawLeg : null // leg without brief is a no-op
const ARG_WAVES = (isObj && Array.isArray(argsIn.waves)) ? argsIn.waves : null // no-brief wave pinning
const ARG_RIDERS = (isObj && Array.isArray(argsIn.riders)) ? argsIn.riders : []
const ARG_ACCEPT = (isObj && Array.isArray(argsIn.acceptance)) ? argsIn.acceptance : []
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))]
const langOf = (t) => t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null
const LANG_KEYS = [...new Set(TARGETS.map(langOf))]
// Brief-derived language covers the targetless {brief, leg} form: RASM-<CS|PY|TS>- names the root.
const briefLang = BRIEF ? (((/RASM-(CS|PY|TS)-/i.exec(BRIEF) || [])[1]) || '').toLowerCase() : ''
const LANG_KEY = (LANG_KEYS.length === 1 && LANG_KEYS[0]) ? LANG_KEYS[0] : (!TARGETS.length && briefLang) ? briefLang : null

// --- [MODELS] ----------------------------------------------------------------------------

const UNDERUTIL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'capability'],
  properties: { catalog: { type: 'string' }, capability: { type: 'string' } } } }
const RIDER = { type: 'object', additionalProperties: false, required: ['motion', 'target', 'anchor', 'wave'], properties: {
  motion: { type: 'string', enum: ['manifest-drop', 'manifest-add', 'catalog-delete', 'counterpart-edit', 'verify'] },
  target: { type: 'string' }, // repo-relative file the motion edits or asserts
  anchor: { type: 'string' }, // SYMBOL anchor — never a line number
  detail: { type: 'string' },
  page: { type: 'string' }, // owning in-run page (counterpart-edit / verify); absent -> tail-routed
  guardPage: { type: 'string' }, // catalog-delete: the vendored owner that must exist on disk first
  wave: { type: 'integer' } } }
const TRACE = { type: 'object', additionalProperties: false, required: ['name', 'needs'], properties: {
  name: { type: 'string' }, needs: { type: 'array', items: { type: 'string' } } } } // '<page>#<entry>' | '<seam-anchor>'
const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['packages', 'pages'], properties: {
  packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'root', 'planning', 'api'],
    properties: { name: { type: 'string' }, root: { type: 'string' }, planning: { type: 'string' }, api: { type: 'string' }, note: { type: 'string' } } } },
  pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'kind', 'wave'], properties: {
    page: { type: 'string' }, kind: { type: 'string', enum: ['new', 'rebuild', 'improve'] }, absorb: { type: 'string' },
    wave: { type: 'integer' } } } }, // ARRAY ORDER IS DEPENDENCY ORDER — the engine never re-sorts
  deletePages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'capturedIn', 'wave'],
    properties: { page: { type: 'string' }, capturedIn: { type: 'string' }, wave: { type: 'integer' } } } },
  riders: { type: 'array', items: RIDER },
  acceptance: { type: 'array', items: TRACE },
  upstreamMissing: { type: 'array', items: { type: 'string' } } } }
const WORK = { type: 'array', items: { type: 'object', additionalProperties: false,
  required: ['page', 'kind', 'apiUsed', 'apiUnderutilized', 'contextNote', 'stackingGuidance', 'weak'], properties: {
  page: { type: 'string' }, kind: { type: 'string', enum: ['new', 'rebuild', 'improve'] }, absorb: { type: 'string' },
  apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL,
  contextNote: { type: 'string' }, stackingGuidance: { type: 'string' }, weak: { type: 'boolean' } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['worklist', 'dossier'], properties: {
  worklist: WORK, dossier: { type: 'string' }, summary: { type: 'string' } } } // dossier = the path it WROTE
const SEAMS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['file', 'counterpart', 'bothEnds'],
  properties: { file: { type: 'string' }, counterpart: { type: 'string' }, bothEnds: { type: 'boolean' } } } }
const BEYOND = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'member'],
  properties: { catalog: { type: 'string' }, member: { type: 'string' } } } }
const INDEXROWS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['doc', 'row'],
  properties: { doc: { type: 'string' }, row: { type: 'string' } } } } // doc = the package index file; row = the exact row text
const RECEIPTS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['anchor', 'disposition', 'evidence'],
  properties: { anchor: { type: 'string' }, disposition: { type: 'string', enum: ['done', 'drift', 'blocked'] },
  evidence: { type: 'string' } } } }
// Required-but-empty arrays are attestations: forced seamsTouched/beyondMap/indexRows/dossierPhantoms
// make "read fully / exceed the map / repair both ends" structurally checkable, never wishful prose.
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false,
  required: ['files', 'verdict', 'summary', 'seamsTouched', 'beyondMap', 'indexRows', 'dossierPhantoms'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] },
  collapsed: { type: 'string' }, extended: { type: 'string' }, summary: { type: 'string' },
  seamsTouched: SEAMS, beyondMap: BEYOND, indexRows: INDEXROWS, dossierPhantoms: { type: 'array', items: { type: 'string' } },
  riders: RECEIPTS } }
const REVIEW_SCHEMA = { type: 'object', additionalProperties: false,
  required: ['files', 'verdict', 'summary', 'seamsTouched', 'beyondMap', 'indexRows'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] },
  extended: { type: 'string' }, summary: { type: 'string' }, seamsTouched: SEAMS, beyondMap: BEYOND, indexRows: INDEXROWS } }
const DELETE_SCHEMA = { type: 'object', additionalProperties: false, required: ['deleted', 'summary'], properties: {
  deleted: { type: 'array', items: { type: 'string' } }, indexRows: INDEXROWS,
  kept: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'reason'],
    properties: { page: { type: 'string' }, reason: { type: 'string' } } } }, summary: { type: 'string' } } }
const TAIL_SCHEMA = { type: 'object', additionalProperties: false, required: ['applied', 'summary'], properties: {
  applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action', 'evidence'],
    properties: { target: { type: 'string' }, action: { type: 'string' }, evidence: { type: 'string' } } } },
  drift: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const ACCEPT_SCHEMA = { type: 'object', additionalProperties: false, required: ['traces', 'files', 'summary'], properties: {
  traces: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'verdict'], properties: {
    name: { type: 'string' }, verdict: { type: 'string', enum: ['pass', 'fixed', 'miss'] },
    missing: { type: 'array', items: { type: 'string' } } } } },
  files: { type: 'array', items: { type: 'string' } },
  unresolved: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
// LANG carries routing data and engine-parameter rows ONLY — doctrine content is reached
// through READ_FIRST at the source, never paraphrased here.

const LANG = {
  cs: {
    key: 'cs', name: 'C#', root: 'libs/csharp', stack: 'docs/stacks/csharp', casing: 'PascalCase',
    corpus: 'libs/csharp planning corpus (markdown specs of intended C# package designs)',
    strata: 'CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; ' +
      'depend strictly upward; a host-neutral owner only where a non-Rhino runtime consumes the contract).',
    stackFloor: 'docs/stacks/csharp is the FLOOR, never the ceiling — every fence meets it and pushes past it to the strongest ' +
      'form the doctrine admits; the tools/cs-analyzer compiled-doctrine gate enforces it (a true positive is architecture ' +
      'pressure, a false positive is rule pressure, never a suppression).',
    apiTiers: 'the SHARED substrate catalogs `libs/csharp/.api/*.md` (Thinktecture generated owners, LanguageExt ' +
      'rails/effects/schedules/immutable collections, QuikGraph, Mapperly and siblings) AND the folder catalogs ' +
      '`<package>/.api/*.md`, always layering the universal Thinktecture/LanguageExt rails onto the domain packages, never the ' +
      'folder set alone.',
    verify: '`uv run python -m tools.assay api` (assay blocked or unavailable: the `.api` catalogs, the nuget MCP for feed ' +
      'truth, and Context7/exa/tavily for the official surface own the fallback)',
    vocab: '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject]`/`Fold`/the rails)',
    slur: 'naive, surface-level code dressed in the right vocabulary',
    illusion: 'a `.api`/host member cited but never verified',
    docBloat: 'XML-doc',
    collapseInto: 'ONE `[Union]` / `[SmartEnum<TKey>]` / `[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case ' +
      'family IN THE SAME FILE',
    gapPkg: 'LIBRARY_DEPTH: e.g. an IFC schema gives a zone its quantities, space boundaries, and properties the page never ' +
      'reads — stacking that full surface IS new functionality woven into the owner, not a denser spelling of the same call',
    gapDomain: 'a BIM zone owns its boundary/area/volume, per-kind attributes — a fire compartment a rating, a thermal zone a ' +
      'setpoint, a load group its combinations, an MEP system its medium/flow/pressure — adjacency/nesting topology, and ' +
      'coverage/aggregation/spatial-query operations, not a flat member-id set alone; a profile owns section properties, grade, ' +
      'fabrication + code-check inputs, not width/height; a durable store owns its constraints, indexes, partitions, RLS, ' +
      'migration, and lifecycle, not naive columns',
    ownerGrammar: 'a CASE in the existing closed family, a ROW or richer data on the existing smart-enum, a FIELD or a composed ' +
      '`[ValueObject]`/`[ComplexValueObject]` on the existing record, an OPERATION on the existing surface, or a POLICY_VALUE ' +
      'on the existing vocabulary',
    deepPkgs: 'LanguageExt/Thinktecture/MathNet/CSparse',
    body: 'nested `Bind`/`Map` lambda towers where LINQ query syntax or one composed `Eff`/`Fin` pipeline reads flat; ' +
      '`Match(_ => unit)` and swallowed `IfFail` where a typed failure case belongs; manual loop/accumulator plumbing ' +
      'where `Fold`/`Traverse`/`Sequence`/`Partition` compose the join; helper statics and one-off records orbiting an ' +
      'owner instead of living on it',
    exhaust: 'total generated `Switch`, no silent `_` arm',
    modern: 'Latest modern C# 14 on net10',
    mechanics: '',
    fileOrg: 'apply the `docs/stacks/csharp` file-organization + section-order law',
  },
  py: {
    key: 'py', name: 'Python', root: 'libs/python', stack: 'docs/stacks/python', casing: 'snake_case',
    corpus: 'libs/python planning corpus (markdown specs of intended Python module designs)',
    strata: 'CLAUDE.md manifest law governs.',
    stackFloor: 'docs/stacks/python is the bar and docs/stacks/csharp is the density/ambition FLOOR — match its richness, ' +
      'never import C#-shaped idioms.',
    apiTiers: 'the SHARED/universal branch catalogs `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, ' +
      'pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*) AND the folder catalogs ' +
      '`<package>/.api/*.md`, always layering the shared/universal rails ON TOP OF the folder-specific domain packages, never ' +
      'the folder set alone.',
    verify: '`uv run --frozen python -m tools.assay api resolve <pkg>` (a gated/uninstalled package, or a blocked/unavailable ' +
      'assay, falls back to its catalog/official surface)',
    vocab: '(`@tagged_union`/`frozendict`/`Result`/`Option`/the rails)',
    slur: 'naive, surface-level, old-style Python dressed in the right vocabulary',
    illusion: 'a `.api` member cited but never verified',
    docBloat: 'docstring',
    collapseInto: 'one closed `@tagged_union`/`Literal`/`StrEnum` family, a derived `frozendict` table, or a fold IN THE SAME FILE',
    gapPkg: 'BOTH tiers; stacking that full surface IS new functionality woven into the owner, not a denser spelling of the same call',
    gapDomain: 'a dimension owner owns the full ISO 129-1 linear/aligned/angular/radial/diameter/ordinate/chain/baseline + ' +
      'tolerance family, not a single linear case; a layer codec owns the full ISO 13567 + NCS discipline/major/minor/status ' +
      'structure, not a flat string',
    ownerGrammar: 'a CASE in the existing closed `@tagged_union`/`Literal`/`StrEnum` family, a ROW or richer data on the ' +
      'existing `frozendict` table, a FIELD on the existing `msgspec.Struct`/Pydantic model/frozen dataclass/`TypedDict`, an ' +
      'OPERATION on the existing surface, or a POLICY_VALUE on the existing vocabulary',
    deepPkgs: 'the admitted both-tier catalogs (expression/msgspec/pydantic/anyio + the folder domain packages)',
    body: 'nested try/except and if-ladders where the `expression` Result/Option pipeline or one `match` expression ' +
      'reads flat; bare `except` and silently discarded `Result` where a typed failure case belongs; manual ' +
      'loop/accumulator plumbing where fold/traverse/partition combinators compose the join; module-level helper ' +
      'functions and one-off aliases orbiting an owner instead of living on it',
    exhaust: 'total `match` + `assert_never` over the FULL case set',
    modern: 'py3.15-modern only',
    mechanics: 'MECHANICAL EXECUTABILITY — a fence is a signature-and-implementation contract: mentally compile and type-check ' +
      'each one against the real cross-page owners it imports, then hunt these defect classes at their owning doctrine sites ' +
      'and fix each in place by growing the existing owner: FENCE-PARSES (`language.md` CLOSED_MATCH_SITE) · MODEL-COHERENCE ' +
      '(README CORPUS_LAW) · TOTAL-DISPATCH (`shapes.md` families) · SINGLE-FACT-EVIDENCE (`rails-and-effects.md` ' +
      'STATE_RECEIPTS + `boundaries.md` BYTE_IDENTITY) · LOOP-OFFLOAD (`concurrency.md` OFFLOAD_LANE) · HANDLE-LIFETIME + ' +
      'BINARY-KERNEL (`boundaries.md` CAPSULE_OWNER) · IDENTITY-REGIME (`boundaries.md` MEMO_KEY) · TEMPLATE-SAFETY ' +
      '(`language.md` TEMPLATE_STRUCTURE_SITE) · STREAM-OVER-MATERIALIZE (`iteration.md` LAZY_COMBINATORS) · ' +
      'NO-EXCEPTION-HOTLOOP (`rails-and-effects.md` EXPRESSION_SPINE) · DERIVED-NOT-PARALLEL + PER-MODE PAYLOADS (README ' +
      'DERIVED_LOGIC). The defect definitions live at the sites; read them there.',
    fileOrg: 'apply the `docs/stacks/python` file-organization + section-order law',
  },
  ts: {
    key: 'ts', name: 'TypeScript', root: 'libs/typescript', stack: 'docs/stacks/typescript', casing: 'camelCase',
    corpus: 'libs/typescript planning corpus (markdown specs of intended TypeScript module designs)',
    strata: 'CLAUDE.md manifest law governs.',
    stackFloor: 'docs/stacks/typescript composed in full is the bar — author ultra-advanced TS only, discarding naive idioms wholesale.',
    apiTiers: 'the SHARED/universal `libs/typescript/.api/*.md` Effect substrate rails AND the folder catalogs ' +
      '`<folder>/.api/*.md`, cross-checked against the published types in node_modules, always layering the shared Effect ' +
      'ecosystem end-to-end ON TOP OF the area-specific packages, never the folder set alone.',
    verify: 'the published types in node_modules (`uv run python -m tools.assay api` over node_modules declarations where a member is novel)',
    vocab: '(`Schema.Class`/`TaggedClass` families, tagged unions, `Effect`/`Layer`, value-derived vocabulary tables)',
    slur: 'naive JavaScript-in-TypeScript dressed in the right vocabulary',
    illusion: '`any`/unsafe `as`/non-null `!` smuggled under a confident surface; a member cited but unverifiable against node_modules',
    docBloat: 'TSDoc',
    collapseInto: 'ONE deep `Schema.Class`/`TaggedClass`/`TaggedError` family — embedded sub-schemas, brand-in-field ' +
      'refinements, class-carried methods and statics — or ONE tagged discriminated union + exhaustive match, IN THE SAME ' +
      'FILE; CLASS-FIRST: a module-level type alias, interface, or bare `Struct` standing where a class family could carry ' +
      'invariants, statics, and derived projections is a defect, and `Schema.Struct` survives only as an anonymous ' +
      'single-consumer field block',
    gapPkg: 'BOTH tiers: the shared `libs/typescript/.api/` Effect substrate rails AND the folder domain packages, ' +
      'cross-checked against the published node_modules types; stacking that full surface IS new functionality woven into the ' +
      'owner, not naive Promise/try-catch glue',
    gapDomain: 'a chart owns scale/axis/series/interaction/annotation families and zoom/brush/tooltip/legend operations, not ' +
      'two naive renders; a service owns retry/breaker/telemetry/validation/cache layers internally, not a bare fetch; a ' +
      'machine owns hierarchical/parallel regions, guarded transitions, timers, and history as data, not a switch ladder; a ' +
      'projection owns the full transform/diff/patch family the domain needs',
    ownerGrammar: 'a CASE in the existing tagged discriminated union, a FIELD or embedded sub-schema on the existing ' +
      '`Schema.Class` family, an OVERLOAD or `Function.dual` twin on the existing entrypoint, a STATIC or derived ' +
      'projection on the existing class, a member on the existing `Effect.Service`, a ROW in the existing ' +
      'const-union/table, or a POLICY value on the existing vocabulary',
    deepPkgs: 'the Effect ecosystem (`Effect`/`Layer`/`Context`/`Schema`/`Stream` + platform/experimental/cluster/' +
      'workflow/sql/rpc/ai) + the area packages',
    body: 'nested `Effect.flatMap(Effect.flatMap(...))` and pipe-inside-pipe pyramids where `Effect.gen`/`Do`/one flat ' +
      'pipe owns the sequence; `catchAll(() => Effect.void)` blanket swallows where typed `catchTag`/`catchTags` or an ' +
      'explicitly ruled ignore belongs; `flatMap` where `map` serves, manual fold/partition plumbing where ' +
      '`zipWith`/`all`/`validate`/`partition` compose the join, run-and-discard where `tap`/`tapError`/`tapBoth` ' +
      'belongs, sequential steps where `zip`/`all` with concurrency expresses the parallel join; loose module-level ' +
      'consts, aliases, and option-bags orbiting an owner instead of integrating as statics, fields, or derived ' +
      'projections',
    exhaust: 'exhaustive `Match.exhaustive` dispatch (or a checked `never` sink)',
    modern: 'ultra-advanced modern TS only',
    mechanics: '',
    fileOrg: 'apply the `docs/stacks/typescript` file-organization + section-order law',
  },
}
const L = LANG[LANG_KEY] || LANG.cs // inert fallback: composition no-ops before any agent call when LANG_KEY is null

// --- [MODALITY] --------------------------------------------------------------------------
// The modality discriminant, resolved once. A third modality later is one MODE row.

const MODE = {
  brief: {
    scopeLaw: 'CAMPAIGN BRIEF: READ ' + BRIEF + ' — the shared-law head plus every section covering the targeted ' +
      'folder(s). Classify each page FROM THE BRIEF against REAL DISK STATE: a brief-new page confirmed absent on ' +
      'disk -> kind `new` (it may open a new sub-folder; carry `absorb` when the brief names an existing page it ' +
      'absorbs); a brief-new page already on disk -> `rebuild`; a page the brief names for rebuild/paradigm work -> ' +
      '`rebuild`; a page with a bounded KEEP-with-extension row -> `improve`; every other existing page under the ' +
      'targets is OUT OF SCOPE — the brief is the scope law, no cold-pass set. Kind counts derive from disk, never ' +
      'from a brief census figure. deletePages: only brief-declared outright deletions confirmed present on disk ' +
      'whose content maps to a destination outside the page set ({page, capturedIn, wave}); an absorbed page whose ' +
      'absorber is in the page set travels via `absorb` on the absorber. ',
    legRead: LEG != null ? 'LEG SELECTOR: the brief leg/wave-partition table OWNS the page set — the selected leg(s) ' +
      JSON.stringify(LEG) + ' page rows ARE the pages. When TARGETS is empty the leg rows are the SOLE source: resolve ' +
      'each row\'s page path repo-relative under the brief\'s package root and derive `packages` from those paths; when ' +
      'TARGETS is present the rows bound the admission. Admit them in their listed order (listed order IS dependency ' +
      'order), consuming their declared kind/absorb/deletePages/rider cells verbatim — verified against disk, never ' +
      're-invented. An empty `pages` return with a leg selected is a DEFECT — the leg row names its targets. Transcribe ' +
      'each rider cell into a typed rider row (SYMBOL anchors, never line numbers). Emit the brief acceptance ' +
      'dry-runs covering the selected leg(s) as `acceptance` traces. Where a selected leg names an upstream-leg page ' +
      'absent on disk, list it in `upstreamMissing` — never silently proceed. ' : '',
    conceptSource: 'in the brief',
    intentRead: 'Read ' + BRIEF + ' — the shared-law head plus every section covering these folders — in FULL. ',
    decisionClause: 'Every paradigm, rename, absorb, and delete decision follows the brief. ',
    frozenGuard: 'BRIEF-FROZEN: a signature, name, or invariant the brief freezes is byte-identical law — never ' +
      'rename, re-shape, or re-collapse it, even to a stronger form; seam-canonical wire names stay frozen unless ' +
      'the brief amends them AND assigns the counterpart work; no page outside the brief scope is rebuilt. ',
    deleteAuthority: true,
  },
  folder: {
    scopeLaw: 'FOLDER CHARTER SCOPE: full-target expansion — every existing design page under the targets enters ' +
      'as kind `rebuild`; a page the owning-package charter (ARCHITECTURE.md + README.md + IDEAS.md) demands but ' +
      'disk lacks enters as kind `new`; a page the charter marks landed/settled is SKIPPED (excluded from the page ' +
      'list). deletePages is empty; no absorb pairs; `improve` does not apply. ',
    legRead: '',
    conceptSource: 'from the owning-package charter (ARCHITECTURE.md + README.md + IDEAS.md)',
    intentRead: 'Read the owning-package charter — ARCHITECTURE.md + README.md + IDEAS.md — as the INTENT authority ' +
      'for what each page owns and which pages are settled. ',
    decisionClause: '',
    frozenGuard: 'CHARTER-SETTLED: a page the charter marks landed or settled is not re-litigated; every other page ' +
      'is rebuilt to the strongest form the doctrine admits. ',
    deleteAuthority: false,
  },
}
const M = BRIEF ? MODE.brief : MODE.folder

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
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }
const pkgOf = (p) => p.split('/.planning/')[0] // package = the write-partition key (index docs live at its root)
const dossierPath = (w, pkg, i) => SCRATCH + '/w' + w + '-' + pkg.split('/').pop() + '-b' + i + '.md'
const serialRider = (r) => r.motion === 'manifest-drop' || r.motion === 'manifest-add' || r.motion === 'catalog-delete' || !r.page
const pageRiders = (batch, w, riders) => riders.filter((r) => r.wave === w && !serialRider(r) && batch.some((p) => p.page === r.page))
// Preserves plan emission order (dependency order); dedupe key = page|wave, first wins; args-pinned waves override.
const normalizePages = (pl, pinned) => {
  const seen = new Set()
  const out = []
  for (const p of ((pl && pl.pages) || [])) {
    if (!p || !p.page) continue
    let wave = Number.isInteger(p.wave) ? p.wave : 0
    if (pinned) { const i = pinned.findIndex((ws) => Array.isArray(ws) && ws.indexOf(p.page) >= 0); if (i >= 0) wave = i }
    const key = p.page + '|' + wave
    if (seen.has(key)) continue
    seen.add(key)
    out.push({ page: p.page, kind: p.kind || 'rebuild', absorb: p.absorb, wave })
  }
  return out
}

// MAP_BY_PAGE — the per-page reading map Discover surfaced, wave-local, indexed for every downstream stage.
let MAP_BY_PAGE = new Map()
const mapFor = (p) => MAP_BY_PAGE.get(p.page || p) || { page: p.page || p, kind: p.kind || 'rebuild', absorb: p.absorb,
  apiUsed: [], apiUnderutilized: [], contextNote: '', stackingGuidance: '', weak: true }
const mapsFor = (pgs) => pgs.map((p) => mapFor(p))

// --- [SHARED_BLOCKS] — every rigor law appears exactly once, here; stages compose subsets.
const CONTEXT = 'Rasm monorepo — ' + L.corpus + '. ' + L.strata + ' ' + L.stackFloor
const readFirst = (pkg, dossier) => [
  'READ FIRST, IN ORDER, BEFORE ANY EDIT — no fence is judged before this read lands.',
  '(1) DOCTRINE — enumerate `' + L.stack + '/` with a real `ls` (never memory), then read the README and EVERY root page it ' +
    'routes IN FULL in the README [01]-[ATLAS] order — top-to-bottom, never a partial, skim, grep-jump, or section-sample; a ' +
    'root page on disk but absent from the atlas is still mandatory law. The README [02]-[DOCTRINE] laws, the ' +
    '[03]-[COLLAPSE_SCAN] table, OWNER_CHOOSER (`shapes.md` [01]), RAIL_CHOOSER (`rails-and-effects.md` [01]), and the aspect ' +
    'two-weave (`surfaces-and-dispatch.md` AND `rails-and-effects.md` — both owners) are binding law AT THE SOURCE. This ' +
    'prompt does not restate the doctrine; read it there, hold it as fact, and conform every fence to it — a summary is never ' +
    'a substitute for the read.',
  L.key === 'cs' ? '(1b) Enumerate `docs/stacks/csharp/domain/` with a real `ls` through its router README, then read every ' +
    'shard the page concerns touch — chosen from the enumerated set, truthfully, never from memory or skipped; shard ' +
    'conformance is a hard gate.' : '',
  '(1c) ANALYZER LAW — read the repo `.editorconfig` rules for your language: every rule at `error` severity is a COMPILE ' +
    'GATE the fences must satisfy (`dotnet_style_namespace_match_folder = true:error` means namespace ALWAYS equals folder ' +
    'path — a namespace matrix, mapping table, or doc claim that contradicts an error-level analyzer rule is a FICTION to ' +
    'correct, never law to compose).',
  '(2) .API — `ls` BOTH catalog tiers in full — the shared substrate `' + L.root + '/.api/` AND the folder `' + pkg +
    '/.api/` — then read every catalog relevant to these pages, layering the shared rails (' + L.deepPkgs + ') ON TOP OF the ' +
    'folder domain packages, never the folder set alone.',
  dossier ? 'The grounding dossier at `' + dossier + '` carries verified two-tier extracts for this batch: read it, ' +
    'SPOT-VERIFY its anchors (a fake anchor goes in `dossierPhantoms`), and hunt PAST it — members you compose beyond it are ' +
    'enumerated in `beyondMap`. Absent or stale, run the full two-tier `ls`+read yourself.' : '',
  '(3) SCOPE — ' + M.intentRead,
].filter(Boolean).join('\n')
const STANCE = 'STANCE — every pass is hostile: author, critique, and red-team alike. Hold the fence naive, shallow, or ' +
  'illusory until it survives a real attack; the burden of proof is on the code, never on you. "Mature", "already strong", ' +
  '"good enough", and a prior clean verdict are rejected self-assessments — most of this corpus is ' + L.slur + '. Dense, ' +
  'confident, package-fluent code is the PRIME suspect for hollowness: disbelieve every claim a fence makes about itself and ' +
  'verify it against the real domain and the catalogued package surface. NAIVETY is a defect on two orthogonal axes. ' +
  'COVERAGE — the owner models a thin slice of its concept: a 2-case family for a 20-case domain, three fields where the ' +
  'concept carries fifteen. APPROACH — an enumerated roster of styles, variants, or arms where one parameterized generator ' +
  'should GENERATE the space; the roster demotes to seed DATA over named parameters. ILLUSORY code is the primary target: ' +
  'doctrine vocabulary ' + L.vocab + ', cited packages, confident prose, hollow body — a phantom (' + L.illusion + '), a name ' +
  'promising capability the body omits, decorative density carrying nothing, a stub dressed as a finished design. Every ' +
  'collapse-signal list in these prompts is a FLOOR, never the complete set. Reach a no-edit verdict only after an attack ' +
  'finds nothing; prove completeness by adding nothing — never invent churn.'
const BUILD_LAW = 'BUILD LAW — buildout over removal, always. A package admission, a capability, or a domain concept is ' +
  'removed only where the campaign brief rules that removal; the sole exception is a PHANTOM — a cited member that does not ' +
  'exist. An underutilized catalog, an orphan-looking admission, or a weak fence is an INTEGRATION target: the capability ' +
  'lands as ' + L.ownerGrammar + ' — inside the existing owner, reshaped as if always carried — or is wired into its owning ' +
  'sibling page in the same pass. Never a parallel type, a new file, a sibling shape, or flat appended code; never extract a ' +
  'file to cut LOC; never regress existing capability. Structural collapse and CAPABILITY completeness are orthogonal — a ' +
  'fully collapsed owner can still model a naive slice; close both. Every extension cites exactly one gap source. PACKAGE — ' +
  'a member the admitted surface exposes that the concept admits but the page ignores (' + L.gapPkg + '). DOMAIN — an ' +
  'attribute, metric, sub-kind, relationship, state, or operation the real concept demands (' + L.gapDomain + '). CONSUMER — ' +
  'a contract a sibling or downstream owner will require. Byte-count is a weak proxy: assess every owner against its full ' +
  'domain and both-tier package surface independently of size. Anticipate the FIVE-TIMES demand: model each owner for five ' +
  'times today\'s cases, fields, and consumers — a thin slice built "for now" is the COVERAGE defect by definition. ' +
  'CHANNEL LAW — a canary/beta/pre-release channel is admissible wherever the bleeding edge genuinely adds capability: ' +
  'judged on capability delta, maintenance signal, and integration merit, never rejected for channel alone, pinned exact ' +
  'with the typing posture recorded in the catalog.'
const BODY = 'FENCE-BODY LAW — the interior of every fence is judged at the same bar as its shapes; a correct owner ' +
  'carrying a naive body is a defect. Rebuilt on sight: ' + L.body + '. The optimal body is dense, flat, ' +
  'expression-shaped, and reads as one algebra — the admitted combinator surface is the material, never hand-rolled ' +
  'control flow, nesting, or extraction to loose helpers.'
const VERIFY = 'VERIFY — cite only members confirmed via ' + L.verify + '; a member you cannot verify is a phantom to delete. ' +
  'Mine BOTH .api tiers to operator depth: ' + L.apiTiers + ' An admitted capability the concept admits that no owner ' +
  'exploits is a defect to close.'
const WRITE_FULLY = 'WRITE FULLY — every fix you identify you make NOW via Edit/Write; the fix-log is a report of edits ' +
  'already made, never a to-do, a ledger, or a would/should hedge. A cross-file ripple your edit causes is YOURS in the same ' +
  'pass: repair the seam counterpart and the consumer site wherever the fix lives — surgical anchored edits, wire-canonical ' +
  'names frozen (the branch ARCHITECTURE.md [02]-[SEAMS] ledger names the wire rows), never a foreign-interior rebuild — and ' +
  'record every touched seam in `seamsTouched`, both ends. TWO surfaces are serialized and never yours to write: the ' +
  'owning-package index docs (ARCHITECTURE.md + README.md at the path before `/.planning/`) and the central manifests — ' +
  'report the exact rows you need in `indexRows`; the wave tail applies them once.'
const PROSE_COMMENTS = 'PROSE + COMMENTS — apply docs/standards/style-guide.md, information-structure.md, and formatting.md. ' +
  'The page is a design spec: lead each section with the controlling contract, one idea per paragraph, close on the ' +
  'consequence; no provenance, process narration, freshness disclaimers, or hedges. Backtick every symbol, type, field, ' +
  'function, operator, package ID, path, command, flag, and literal; name the exact member instead of paraphrasing behavior; ' +
  'trimming never reduces technical density. Code fences comment for the next agent only: keep the canonical section-divider ' +
  'headers; beyond them default to zero comments, 1-2 lines only for a truly subtle invariant, contract, or boundary; no ' +
  'restating the code, no narration, no ' + L.docBloat + ' bloat.'
const readingMap = (maps) => 'READING MAP — the per-page grounding Discover surfaced (an initial pointer, never the ' +
  'ceiling):\n' + JSON.stringify(maps, null, 1) + '\nThe map POINTS; you VERIFY and EXCEED it: compose every `apiUsed` ' +
  'catalog at full operator depth, stack every `apiUnderutilized` {catalog, capability} INTO the owning page as a case, row, ' +
  'field, or operation, and independently confirm no other relevant admitted catalog (either tier) is missing. Members you ' +
  'compose beyond the map are enumerated in `beyondMap` — an empty `beyondMap` is an attestation that the map was genuinely ' +
  'complete, not a license to treat it as a ceiling.'
const preamble = (batch, dossier) => [CONTEXT, readFirst(pkgOf(batch[0].page), dossier), STANCE, BUILD_LAW, BODY, VERIFY,
  WRITE_FULLY, PROSE_COMMENTS].concat(L.mechanics ? [L.mechanics] : []).concat([readingMap(mapsFor(batch))])

// --- [PROMPTS] — each task states only its own action; shared checks are referenced by name.
const planPrompt = () => [CONTEXT,
  'TASK: thin enumerate + classify (read-only, do NOT edit). TARGETS (repo-relative): ' + JSON.stringify(TARGETS) +
  (TARGETS.length ? '. The ' : ' — EMPTY: the brief leg selector below supplies the page set; the selected leg rows ARE ' +
  'the expansion, so skip target expansion and go straight to the brief. The ') +
  'OWNING PACKAGE of a page is the path before `/.planning/`. Use find/ls; validate the expansion against ' +
  '`libs/.planning/planning-targets.md` (a mis-scoped or renamed target is reported in `upstreamMissing`, a deliberately ' +
  'page-less target is skipped silently). Return `packages` (one entry per distinct owning package: {name, root, planning, ' +
  'api}). PAGES: expand each target — a ROOT to every design page under its planning tree, a SUB-FOLDER to every page under ' +
  'it, a FILE to itself; union + dedup; exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md and any campaign-brief page.',
  M.scopeLaw + M.legRead,
  (!BRIEF && ARG_WAVES) ? 'ARGS-PINNED WAVES (page arrays by wave index): ' + JSON.stringify(ARG_WAVES) : '',
  'EMIT `pages` IN DEPENDENCY ORDER — the brief\'s listed order where a brief rules it, otherwise grouped by sub-folder then ' +
  'alphabetical; the engine chunks in your emitted order and never re-sorts. Assign each page its `wave` (brief wave/leg ' +
  'partition; args-pinned waves; otherwise 0). A page carrying both an earlier-wave `improve` pre-motion and a later-wave ' +
  'main motion emits TWO rows, one per (wave, kind). Home each absorb pair to the absorber\'s wave. `riders`: transcribe ' +
  'every brief rider cell covering the admitted pages into typed rows — symbol anchors only.',
].filter(Boolean).join('\n\n')
const discoverPrompt = (batch, dossier) => [CONTEXT, readFirst(pkgOf(batch[0].page), ''), STANCE, VERIFY,
  'TASK: HOSTILE READ-ONLY DISCOVERY over these ' + batch.length + ' pages — read-only is the only concession; the hunt is ' +
  'as adversarial as every writing pass (investigate, do NOT edit): ' +
  batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') + '. For an improve/rebuild page read the page IN FULL; for a ' +
  '`new` page read its concept ' + M.conceptSource + ' plus its nearest sibling pages. Read the folder at large — the ' +
  'sibling pages each composes and the owning-package index docs — as full-file reads. DISBELIEVE the page: prose claiming ' +
  'a package is composed is verified against the fence body; attack every admitted catalog (both tiers) for the members, ' +
  'combinators, generated surfaces, and native pipelines the concept ADMITS but no fence exploits, and DIFF the complete ' +
  'admitted inventory against the whole folder — a capability no page exploits is a named integration gap ROUTED to ' +
  'EVERY page whose concept admits it, never one "best" owner alone. SINGLE-CONSUMER EXPANSION: a package with a catalog ' +
  'at ANY tier (central manifest, shared substrate, folder) consumed by only ONE page — even deliberately narrowly — is ' +
  'expansion pressure on its siblings: name the package, its unexploited members in exact spellings, and each candidate ' +
  'page where deeper integration would extend the capability. Discovery has ZERO removal authority: an underutilized ' +
  'catalog, orphan-looking admission, or weak fence is always framed as a buildout target (which owner grows which ' +
  'case/row/field/operation), never as removal evidence. ANTI-ANCHOR LAW: your maps and dossier carry FINDINGS, never ' +
  'designs — quality defects graded against the doctrine read at source (name the law and the ' + L.stack + ' pattern ' +
  'whose application would most deeply transform the page — the collapse, the owner form, the rail — never the ' +
  'resulting code) and capability inventory in catalog-anchored spellings; a fence sketch, a prescribed shape, or a ' +
  'pre-ruled design in the dossier ANCHORS and WEAKENS the rebuild and is your defect — the implement agent rules ' +
  'every design.\n' +
  'For EACH page produce its reading map: `apiUsed` (catalogs the page composes — for a new page, will compose), ' +
  '`apiUnderutilized` ({catalog, capability}: the concrete unexploited member in exact catalog-anchored spelling plus its ' +
  'integration shape), `contextNote` (sibling owners/seams composed, folder position, any folder-wide gap routed here), ' +
  '`stackingGuidance` (the initial integration pointer the implement confirms and deepens — capability names and the ' +
  'doctrine patterns/laws whose application would most improve the page, never a prescribed design), `weak` (a hostility ' +
  'verdict earned by the attack, always paired with buildout guidance). The map is a pointer downstream stages verify and ' +
  'EXCEED, never a ceiling or an anchor. Verify every cited member via ' + L.verify + '; never list a phantom.\n' +
  'GROUNDING DOSSIER: write `' + dossier + '` carrying VERIFIED PRIMARY EXTRACTS ONLY for this batch — the real `ls` ' +
  'inventories of both .api tiers and the doctrine root, quoted .api member blocks with `file:line` anchors for every ' +
  'member the maps cite, the brief seam/rider anchors covering these pages, and folder-context anchors. FORBIDDEN in the ' +
  'dossier: any doctrine-law digest, summary, or paraphrase; any removal framing; any claim without a `file:line` anchor; ' +
  'any prescriptive design, fence sketch, or code-shape ruling — findings name the defect, the law, and the capability, ' +
  'never the resulting code. ' +
  'Downstream agents spot-verify your anchors — a fake anchor is your defect, surfaced loudly. Return worklist + dossier ' +
  '(the path).',
].join('\n\n')
const implementPrompt = (batch, dossier, riders) => preamble(batch, dossier).concat([
  'TASK: HOSTILE IMPLEMENT of these ' + batch.length + ' pages IN PLACE, each per its kind: ' +
  batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') + '.\n' +
  'kind=`new`: GROUND-UP AUTHOR the page (it does not exist; it may open a new sub-folder) to the full doctrine + ' +
  'domain-complete capability bar, in the code-fence-first design-page form of its mature siblings, wired into the folder ' +
  'entry/receipt seam owners where the folder has them; when its map names an absorb target, MOVE the real content over ' +
  'and DELETE the absorbed page in this pass. kind=`rebuild`: HOSTILE GROUND-UP REBUILD in place. kind=`improve`: bounded ' +
  'cold improve per the ruled disposition. Construct in LIFECYCLE order — admit raw once, canonical owner by ' +
  'OWNER_CHOOSER, stacked rail/aspect over a thin pure core, projection, egress, BOTH ingress and egress parameterized; ' +
  'collapse parallel shapes into ' + L.collapseInto + '; one polymorphic entrypoint per modality. COMPOSE the map\'s ' +
  '`apiUsed` at full operator depth, STACK every `apiUnderutilized` into the owner, and CONFIRM no other admitted catalog ' +
  'is missing. CLOSE the concept capability gaps per BUILD LAW.\n' + M.decisionClause + M.frozenGuard + '\n' +
  'RIDERS (yours, receipt-forced): ' + JSON.stringify(riders) + '. Execute each now — a counterpart-edit is a surgical ' +
  'wire edit at the symbol anchor; a verify is a read-and-assert returning `drift` with disk evidence when the assertion ' +
  'fails. Report every rider in `riders` receipts. ' + L.modern + '; ' + L.fileOrg + '; high-signal all-backticked prose. ' +
  'Return the fix-log.',
]).join('\n\n')
const deletePrompt = (dels) => [CONTEXT, WRITE_FULLY,
  'TASK: BRIEF-DECLARED PAGE DELETIONS (verify-then-delete, zero content loss). For EACH page: read it in full, read the ' +
  'brief-named destination (capturedIn), VERIFY the design intent is genuinely captured — every load-bearing owner, seam ' +
  'row, and real standards value accounted for. Captured: delete the file and report the index rows the deletion requires ' +
  'in `indexRows` (the tail applies them). Not captured: MOVE the missing content into the destination yourself, verify, ' +
  'then delete. Keep a page only when the destination itself is wrong (kept {page, reason}). Pages: ' + JSON.stringify(dels),
].join('\n\n')
const critiquePrompt = (batch, dossier) => preamble(batch, dossier).concat([
  'TASK: HOSTILE DOCTRINAL-CONFORMANCE + CAPABILITY AUDIT; fix EACH page in place: ' +
  batch.map((p) => p.page).join(', ') + '. Audit every fence against the doctrine you read at source, never a summary; ' +
  'repair every hit now — a fix, never a ledger note; a cross-file hit is yours per WRITE FULLY. ' + M.frozenGuard + '\n' +
  '- COLLAPSE_SCAN: run the README [03] table on every fence — any signal triggers the move, 3+ instances make it ' +
  'mandatory, the table is a FLOOR you hunt past.\n' +
  '- OWNER_CHOOSER (`shapes.md` [01]): re-derive every shape from the 5 discriminants — admission, identity regime, ' +
  'variant arity, payload timing, openness — and replace any non-discriminant-correct owner; kill every parallel DTO, ' +
  'one-field wrapper, field-rename shape, and null/default ghost.\n' +
  '- KNOB_TEST: delete each parameter — where the value reconstructs it, collapse the knob to a policy value or ' +
  'input-shape discriminant; move every timeout/retry/deadline off the signature onto the carrier or a composition-time ' +
  'aspect.\n' +
  '- ASPECTS (`surfaces-and-dispatch.md` AND `rails-and-effects.md` — both owners), RAILS + closed-fault + ' +
  'accumulate-vs-abort (`rails-and-effects.md` [01]), STRATA/MEMBERS (' + L.modern + '; both .api tiers maximized per ' +
  'VERIFY; ' + L.fileOrg + '): audit each at its owning page.\n' +
  '- CAPABILITY-COMPLETENESS + ILLUSION: verify the body implements what names and prose promise; close any admitted ' +
  'capability the owner omits by growing it per BUILD LAW; attack both naivety axes.\n' +
  'Return the batched fix-log.',
]).join('\n\n')
const redteamPrompt = (batch, dossier, crit) => preamble(batch, dossier).concat([
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM; fix EACH page in place: ' + batch.map((p) => p.page).join(', ') + '. Assume the ' +
  'author and critique missed things. ' + M.frozenGuard + '\n' +
  '(A) COUNTERFACTUAL on the core owner/algebra/dispatch — does a denser owner (' + L.collapseInto + '), a derived table, ' +
  'a parameterized generator over the enumerated space, or a deeper admitted-package primitive (' + L.deepPkgs + ') ' +
  'collapse the whole fence? A fundamentally stronger design is built, never defended against. (B) ANTICIPATORY_COLLAPSE ' +
  '— compute the diff of the next feature: the next case/dimension/modality lands as one row with every consumer ' +
  'untouched or loudly broken (' + L.exhaust + '). (C) LONG-TAIL — empty/singular/plural/stream/malformed/concurrent/' +
  'cancelled/partial-failure/version-skew; accumulate-vs-abort correct for the real boundary; ingress AND egress ' +
  'parameterized. (D) BOUNDARY/STRATA — grade every concern against `libs/.planning/architecture.md` and the branch ' +
  'ARCHITECTURE.md [02]-[SEAMS] ledger (read the ledger, never a summary of it): a concern owned twice, a downward ' +
  'dependency, a host-type leak, or coupling to a sibling interior is a defect fixed both ends. (E) SPRAWL + PHANTOMS — ' +
  'hand-re-derived package capability, flat code below the operator depth the packages reach, a phantom member (delete), ' +
  'a thin wrapper. (F) CAPABILITY-COMPLETENESS + ILLUSION per STANCE and BUILD LAW. Then a FULL COLD RE-REVIEW of every ' +
  'conformance dimension by name — COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST, ASPECTS, RAILS, ' + L.modern + ', ' +
  L.fileOrg + ', both-tier .api maximization, prose + comment hygiene. VERIFY the critique\'s `seamsTouched` landed on ' +
  'BOTH ends of every seam; make any missed repair yourself. CRITIQUE RESULT: ' + JSON.stringify(crit || {}) + '. Return ' +
  'the batched fix-log.',
]).join('\n\n')
const tailPrompt = (w, rows, serial, receipts) => [CONTEXT, PROSE_COMMENTS,
  'TASK: SERIALIZED WAVE CLOSE — you are the wave\'s ONLY writer for shared central files. (1) INDEX ROWS: apply every ' +
  'reported row below to its owning-package ARCHITECTURE.md / README.md exactly once — dedupe semantically identical ' +
  'rows, keep each doc\'s section grammar, verify every landed page, absorb, and delete this wave is truthfully ' +
  'reflected: ' + JSON.stringify(rows) + '. (2) SERIAL RIDERS: execute each row — `manifest-drop`/`manifest-add` ' +
  'hand-edit the grouped central manifest at the SYMBOL anchor (never a line number), preserving label-group order; ' +
  '`catalog-delete` first confirms `guardPage` exists on disk with the vendored owner landed, then deletes the catalog ' +
  'and its index references: ' + JSON.stringify(serial) + '. (3) RECEIPT AUDIT: the page-routed rider receipts below ' +
  'claim done/drift — spot-verify each `done` on disk and report any lie in `drift`: ' + JSON.stringify(receipts) + '. ' +
  'Return applied (target, action, evidence) + drift.',
].join('\n\n')
const acceptancePrompt = (traces, pages) => [CONTEXT, readFirst(pkgOf(pages[0].page), ''), VERIFY, WRITE_FULLY,
  'TASK: ACCEPTANCE TRACE + FIX — you run AFTER every writer including the tail; you are the run\'s last agent, so a ' +
  'finding you can close surgically you close NOW via Edit, never merely report. (1) For each trace, confirm every ' +
  '`needs` entry resolves on landed disk — the page exists, the named entry/owner carries the expected signature, the ' +
  'seam anchor is present: ' + JSON.stringify(traces) + '. (2) GENERIC SWEEP: over the pages landed this run — ' +
  JSON.stringify(pages.map((p) => p.page)) + ' spanning packages ' +
  JSON.stringify([...new Set(pages.map((p) => pkgOf(p.page)))]) + ' (`ls` EACH package\'s `.api/` folder tier before ' +
  'judging resolution, never only the first) — every cross-page symbol a landed fence composes resolves on a sibling ' +
  'owner with a matching signature. (3) FIX every miss at its root: a missing entry/receipt/seam row grows on its ' +
  'OWNING page at that page\'s bar; a mismatched signature corrects at the CONSUMER when the owner is right, at the ' +
  'OWNER when the consumers agree and the owner drifted — disk evidence decides, never preference; a naming drift ' +
  'against a correctly-declared owner corrects at every citing site. Verdicts: `pass` (resolved as found), `fixed` ' +
  '(you closed it — the edit is made), `miss` (genuinely beyond surgical reach — exact anchors in `missing`, ' +
  'surfaced loudly, never silently dropped). `unresolved` carries only the sweep failures you could not close, with ' +
  '`file -> missing owner` evidence; `files` lists every file you edited. Never block, never rebuild a page interior ' +
  'beyond the surgical fix, never invent a member.',
].join('\n\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length && !(BRIEF && LEG != null)) {
  log('No targets — pass a target path, an array, or {targets, brief, leg}. Empty args is a no-op.')
  return { targets: [], total: 0 }
}
if (rawLeg != null && !BRIEF) log('leg without brief is a no-op — the leg selector is ignored.')
if (!LANG_KEY) {
  log('Targets must live under ONE language root (libs/csharp | libs/python | libs/typescript), or a targetless run needs a ' +
    'RASM-<CS|PY|TS>- brief name. Got: ' + JSON.stringify({ targets: TARGETS, brief: BRIEF }))
  return { targets: TARGETS, total: 0 }
}

phase('Plan')
const plan = await agent(planPrompt(), { label: 'plan', phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN_SCHEMA, stallMs: STALL })
const PAGES = normalizePages(plan, (!BRIEF && ARG_WAVES) ? ARG_WAVES : null)
const DELS = ((plan && plan.deletePages) || []).filter((d) => d && d.page)
  .map((d) => ({ page: d.page, capturedIn: d.capturedIn, wave: Number.isInteger(d.wave) ? d.wave : 0 }))
const RIDERS = ((plan && plan.riders) || []).concat(ARG_RIDERS).filter((r) => r && r.motion && r.target)
  .map((r) => ({ motion: r.motion, target: r.target, anchor: r.anchor || '', detail: r.detail, page: r.page,
    guardPage: r.guardPage, wave: Number.isInteger(r.wave) ? r.wave : 0 }))
const ACCEPT = ((plan && plan.acceptance) || []).concat(ARG_ACCEPT).filter((t) => t && t.name)
  .map((t) => ({ name: t.name, needs: Array.isArray(t.needs) ? t.needs : [] }))
const MISSING = (plan && plan.upstreamMissing) || []
if (MISSING.length) {
  log('HALT at plan — upstream not landed: ' + MISSING.join(', '))
  return { targets: TARGETS, halted: 'plan', upstreamMissing: MISSING }
}
const kindCount = (k) => PAGES.filter((p) => p.kind === k).length
log('Plan[' + LANG_KEY + (BRIEF ? '|brief' : '') + (LEG != null ? '|leg ' + JSON.stringify(LEG) : '') + ']: ' + PAGES.length +
  ' pages (' + kindCount('new') + ' new, ' + kindCount('rebuild') + ' rebuild, ' + kindCount('improve') + ' improve) + ' +
  DELS.length + ' deletion(s), ' + RIDERS.length + ' rider(s), ' + ACCEPT.length + ' trace(s); CAP=' + CAP)
if (!PAGES.length && !DELS.length) {
  log('No pages resolved under the targets')
  return { targets: TARGETS, language: LANG_KEY, brief: BRIEF, total: 0 }
}
const WAVES = [...new Set(PAGES.map((p) => p.wave).concat(DELS.map((d) => d.wave)))].sort((a, b) => a - b)

const landed = []
let halt = null
for (const w of WAVES) { // ── the ONE inter-wave barrier — real: cross-wave content moves ──
  const wp = PAGES.filter((p) => p.wave === w)
  const wd = DELS.filter((d) => d.wave === w)
  if (!wp.length && !wd.length) continue
  const lanes = [...new Set(wp.map((p) => pkgOf(p.page)))].map((pkg) => ({ pkg, pages: wp.filter((p) => pkgOf(p.page) === pkg) }))
  phase('W' + w + ' Discover'); phase('W' + w + ' Build')
  MAP_BY_PAGE = new Map() // wave-local, rebuilt from landed disk

  const laneOut = (await pool(lanes, CAP, async (lane) => {
    const batches = chunk(lane.pages, IMPL_BATCH) // plan order preserved = dependency order
    const dos = await pool(batches, CAP, (b, i) =>
      agent(discoverPrompt(b, dossierPath(w, lane.pkg, i)), { label: 'discover:w' + w + ':' + lane.pkg + ':b' + i,
        phase: 'W' + w + ' Discover', model: 'opus', effort: 'high', schema: DISCOVER_SCHEMA, stallMs: STALL }))
    dos.filter(Boolean).forEach((d) => (d.worklist || []).forEach((m) => { if (m && m.page) MAP_BY_PAGE.set(m.page, m) }))
    // Bounded per-batch pipeline: impl chain + review chain, both in dependency order —
    // B[k].impl awaits B[k-1].impl; B[k]'s reviews await B[k].impl + B[k-1]'s reviews.
    let prevImpl = Promise.resolve(true)
    let prevReview = Promise.resolve(true)
    const chains = batches.map((b, i) => {
      const implP = prevImpl.then((ok) => ok === null ? null :
        agent(implementPrompt(b, dossierPath(w, lane.pkg, i), pageRiders(b, w, RIDERS)),
          { label: 'impl:w' + w + ':' + lane.pkg + ':b' + i, phase: 'W' + w + ' Build', model: 'fable', effort: 'high', schema: FIXLOG_SCHEMA, stallMs: STALL }))
      prevImpl = implP
      const revP = Promise.all([implP, prevReview]).then(async (pair) => {
        const fix = pair[0]
        if (!fix) return { batch: b, fix: null, crit: null, rt: null } // failure isolation: a dead impl skips its reviews
        const crit = await agent(critiquePrompt(b, dossierPath(w, lane.pkg, i)),
          { label: 'crit:w' + w + ':' + lane.pkg + ':b' + i, phase: 'W' + w + ' Build', model: 'fable', effort: 'xhigh', schema: REVIEW_SCHEMA, stallMs: STALL })
        const rt = await agent(redteamPrompt(b, dossierPath(w, lane.pkg, i), crit),
          { label: 'rt:w' + w + ':' + lane.pkg + ':b' + i, phase: 'W' + w + ' Build', model: 'fable', effort: 'xhigh', schema: REVIEW_SCHEMA, stallMs: STALL })
        return { batch: b, fix, crit, rt }
      })
      prevReview = revP
      return revP
    })
    const done = await Promise.all(chains)
    return { pkg: lane.pkg, done, failed: done.filter((d) => !d.fix).flatMap((d) => d.batch.map((p) => p.page)) }
  })).filter(Boolean)

  const delLog = (wd.length && M.deleteAuthority)
    ? await agent(deletePrompt(wd), { label: 'delete:w' + w, phase: 'W' + w + ' Build', model: 'fable', effort: 'high', schema: DELETE_SCHEMA, stallMs: STALL })
    : null
  const rows = laneOut.flatMap((l) => l.done.flatMap((d) =>
    ((d.fix && d.fix.indexRows) || []).concat((d.crit && d.crit.indexRows) || [], (d.rt && d.rt.indexRows) || [])))
    .concat((delLog && delLog.indexRows) || [])
  const receipts = laneOut.flatMap((l) => l.done.flatMap((d) => (d.fix && d.fix.riders) || []))
  const serial = RIDERS.filter((r) => r.wave === w && serialRider(r))
  const tail = (rows.length || serial.length || receipts.length)
    ? await agent(tailPrompt(w, rows, serial, receipts), { label: 'tail:w' + w, phase: 'W' + w + ' Build', model: 'fable', effort: 'high', schema: TAIL_SCHEMA, stallMs: STALL })
    : null
  const failed = laneOut.flatMap((l) => l.failed)
  landed.push({ wave: w, lanes: laneOut.length, pages: wp.length - failed.length, failed,
    deleted: (delLog && delLog.deleted) || [], tail: (tail && tail.applied && tail.applied.length) || 0 })
  log('Wave ' + w + ': ' + (wp.length - failed.length) + '/' + wp.length + ' pages landed across ' + laneOut.length +
    ' lane(s)' + (failed.length ? ' — FAILED: ' + failed.join(', ') : ''))
  if (failed.length) {
    halt = { wave: w, failed }
    log('HALT before dependent waves — landed waves intact; resume re-runs only the failed agents')
    break
  }
}

phase('Close')
const accept = !halt
  ? await agent(acceptancePrompt(ACCEPT, PAGES), { label: 'acceptance', phase: 'Close', model: 'fable', effort: 'high', schema: ACCEPT_SCHEMA, stallMs: STALL })
  : null
return { targets: TARGETS, language: LANG_KEY, brief: BRIEF, leg: LEG, waves: landed, halted: halt,
  acceptance: accept && { traces: accept.traces, unresolved: accept.unresolved } }
