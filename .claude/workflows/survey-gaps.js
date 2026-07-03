export const meta = {
  name: 'survey-gaps',
  description: 'Per-folder capability-gap survey-AND-APPLY across the C# AEC stack, one folder at a time so the apply only ever consolidates a single folder findings and nothing is dropped. Each folder runs its own pipeline: ONE opus survey+synthesize agent (reads the folder + central manifest, emits the gaps AND six research facets), SIX opus research agents (one facet each, find best-in-class modern packages and self-validate the six-condition gate so no nonsense is admitted), ONE opus execute agent (consolidates the six findings, admits the central pins, updates the csproj + README, restores and verifies the install on osx-arm64 at newest), and ONE sonnet stub agent (writes a one-line placeholder .api catalog per admitted package). Folders run sequentially because every execute agent writes the shared central manifest. The full .api authoring and the design-page integration are deliberately deferred — a focused rebuild-api run fills the stubs, and plan-cs threads the capability into the design pages. Disposable, hardcoded scope, no args.',
  whenToUse: 'Admit the world-class modern packages each planned C# AEC folder is missing, persistence-first, one folder at a time with no dropped findings.',
  phases: [
    { title: 'Survey', detail: 'one opus xhigh agent per folder: domain, admitted packages, hand-rolls, gaps, AND the six research facets', model: 'opus' },
    { title: 'Research', detail: 'six opus xhigh agents per folder, one facet each: best-in-class modern candidate discovery with a self-validated six-condition gate', model: 'opus' },
    { title: 'Execute', detail: 'one opus xhigh agent per folder: consolidate the six findings, admit the central pin + csproj reference + README, restore and verify the install at newest on osx-arm64, self-heal or revert', model: 'opus' },
    { title: 'Stub', detail: 'one sonnet agent per folder: write a one-line placeholder .api catalog file per admitted package, nothing else', model: 'sonnet' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 360000
const EXEC_STALL = 600000
const PERSISTENCE = { name: 'Persistence', primary: true, doc: 'libs/csharp/Rasm.Persistence', api: 'libs/csharp/Rasm.Persistence/.api', planning: 'libs/csharp/Rasm.Persistence/.planning', csproj: 'libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj', note: '' }
const FOLDERS = [
  PERSISTENCE,
  { name: 'AppHost', doc: 'libs/csharp/Rasm.AppHost', api: 'libs/csharp/Rasm.AppHost/.api', planning: 'libs/csharp/Rasm.AppHost/.planning', csproj: 'libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj', note: '' },
  { name: 'Bim', doc: 'libs/csharp/Rasm.Bim', api: 'libs/csharp/Rasm.Bim/.api', planning: 'libs/csharp/Rasm.Bim/.planning', csproj: 'libs/csharp/Rasm.Bim/Rasm.Bim.csproj', note: '' },
  { name: 'Fabrication', doc: 'libs/csharp/Rasm.Fabrication', api: 'libs/csharp/Rasm.Fabrication/.api', planning: 'libs/csharp/Rasm.Fabrication/.planning', csproj: 'libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj', note: '' },
  { name: 'Rasm', doc: 'libs/csharp/Rasm', api: 'libs/csharp/Rasm/.api', planning: 'libs/csharp/Rasm/.planning', csproj: 'libs/csharp/Rasm/Rasm.csproj', note: '' },
  { name: 'Compute', doc: 'libs/csharp/Rasm.Compute', api: 'libs/csharp/Rasm.Compute/.api', planning: 'libs/csharp/Rasm.Compute/.planning', csproj: 'libs/csharp/Rasm.Compute/Rasm.Compute.csproj', note: '' },
  { name: 'Materials', doc: 'libs/csharp/Rasm.Materials', api: 'libs/csharp/Rasm.Materials/.api', planning: 'libs/csharp/Rasm.Materials/.planning', csproj: 'libs/csharp/Rasm.Materials/Rasm.Materials.csproj', note: '' },
]
const HOMING = JSON.stringify(FOLDERS.map((f) => ({ name: f.name, api: f.api, csproj: f.csproj, note: f.note })))
const MANIFEST = 'Directory.Packages.props (central NuGet pins) + Directory.Build.props (TargetFramework net10.0, RuntimeIdentifiers osx-arm64)'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'gaps', 'facets'], properties: { folder: { type: 'string' }, domain: { type: 'string' }, packages: { type: 'array', items: { type: 'string' } }, handRolls: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, evidence: { type: 'string' } } } }, gaps: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, severity: { type: 'string' }, note: { type: 'string' } } } }, facets: { type: 'array', minItems: 6, maxItems: 6, items: { type: 'object', additionalProperties: false, required: ['id', 'direction', 'gap'], properties: { id: { type: 'string' }, direction: { type: 'string' }, gap: { type: 'string' }, mandate: { type: 'string' } } } } } }
const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['facet', 'candidates'], properties: { facet: { type: 'string' }, candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'fills', 'ok'], properties: { package: { type: 'string' }, fills: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, bestOf: { type: 'boolean' }, macOk: { type: 'boolean' }, newest: { type: 'boolean' }, licenseOk: { type: 'boolean' }, noLegacy: { type: 'boolean' }, notDup: { type: 'boolean' }, dupOf: { type: 'string' }, ok: { type: 'boolean' }, alternativesConsidered: { type: 'string' }, evidence: { type: 'string' } } } } } }
const EXEC_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'applied'], properties: { folder: { type: 'string' }, applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'apiPath'], properties: { package: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, apiPath: { type: 'string' } } } }, skipped: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: { package: { type: 'string' }, why: { type: 'string' } } } }, files: { type: 'array', items: { type: 'string' } }, green: { type: 'boolean' }, summary: { type: 'string' } } }
const STUB_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'stubs'], properties: { folder: { type: 'string' }, stubs: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, PER-FOLDER capability-gap survey-AND-APPLY of the C# AEC stack (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP ' +
    'strata, depend strictly upward). This run works ONE folder at a time end-to-end so the apply only ever consolidates a single folder findings ' +
    'and nothing is dropped. The mission is NOT a per-package keep/replace audit — it is to find AND admit the world-class MODERN packages this ' +
    'PLANNED folder is MISSING: both functionality gaps in existing concerns and real capability gaps in the planned scope. Persistence is the ' +
    'PRIMARY weight (backends, store types, store extensions, ingress/egress), then Compute/Bim/Materials/Fabrication/Rasm/AppHost. Central ' +
    'pins live in ' + MANIFEST + '.',
  'BAR: the folder must be able to build world-class, bleeding-edge, production-ready, professional-standard apps RIGHT NOW — ' +
    'ultra-dense/complex/rich, minimal-to-no hardcoding, maximal parameterization, easily extended, NO fake or illusory capability. Treat ' +
    'Persistence/Materials/Bim as likely SHORT-SIGHTED in scope: assume valuable modern packages are missing and hunt them. Where a folder ' +
    'hand-rolls a concern a real package owns, that is a gap. Do not hold back: many additions are fine IF each is best-in-class and non-overlapping. ' +
    'Every stage holds the folder picture and every upstream payload naive, thin, or illusory until it survives attack — dense, confident-looking ' +
    'coverage is the prime suspect for hollowness.',
  'AGGRESSIVE ADDITION, ZERO HEDGING (future-looking justification): admit a package whenever it provides unique, modern capability appropriate to ' +
    'the folder domain/scope. DECLINE ONLY for a real reason — (a) it is old / unmaintained / low-quality, (b) a strictly stronger alternative is ' +
    'already admitted or recommended, or (c) it is out of the folder scope/domain. NEVER decline because there is no CURRENT consumer: planned ' +
    'consumers are real design pressure and zero current consumers never lowers the capability bar.',
  'VALIDATION GATE — every candidate must pass all six, and the RESEARCH agent self-critiques it BEFORE returning (set ok=true only when all six ' +
    'hold; default ok=false on any unproven condition, so no nonsense reaches the apply): (1) BEST-OF — the strongest package for the gap was ' +
    'selected, not the first found; compare the real alternatives. (2) MAC — works on osx-arm64 (managed AnyCPU, or a native lib with an osx-arm64 ' +
    'asset, or a Parametric_Forge-provisioned native substrate; reject win-only / x64-only / dead-on-arm). (3) NEWEST — the current stable release ' +
    'is named, never an old or arbitrary version, and the project is actively maintained. (4) LICENSE — OSS (any OSI license) OR a commercial ' +
    'license that is free with full access (no paid tier, seat cap, usage gate, or eval-only); reject fee/tiered. (5) NO LEGACY PACKAGING — modern ' +
    'packaging only (current TFM/abi, SourceLink-era); reject abandoned/legacy-framework-only artifacts. (6) NO DUP/OVERLAP — it must not ' +
    'duplicate an already-admitted package or another recommended one; when two candidates overlap, keep the single best and set dupOf on the loser. ' +
    'The six conditions are a FLOOR, never the complete attack: hunt real disqualifiers beyond them — dead or vulnerable transitive dependencies, ' +
    'native supply-chain rot, eval-gated features behind an OSS shell, an upstream rewrite orphaning the package — and any found disqualifier is ' +
    'evidence for ok=false.',
  'MANDATED DEEP AREAS (when the active folder owns the concern, the survey MUST raise them as facets and the research MUST sweep them ' +
    'exhaustively, not sample): for Persistence — PostgreSQL 18.4+ SQL/extension surface (EVERY first-party/contrib extension truthfully ' +
    'considered AND the top 25 community/third-party extensions fully surveyed: PostGIS, pgvector/pgvectorscale, TimescaleDB, pg_search/ParadeDB, ' +
    'pg_duckdb, Citus, pg_partman, pg_cron, pgRouting, h3-pg, Apache AGE, and beyond) and DuckDB (EVERY first-party extension AND the top 25 ' +
    'community extensions: spatial, httpfs, iceberg, delta, postgres/mysql/sqlite scanners, vss, fts, h3, ducklake, and beyond), plus ' +
    'store/backend types (columnar, time-series, graph, vector, search, object, embedded, lakehouse) and ingress/egress codecs; for Bim — IFC ' +
    '(model graph, geometry/tessellation, validation, BCF, IDS), scheduling (CPM/resource-leveling/4D), cost (estimating/QTO/cost models), ' +
    'GIS/geospatial; for Materials — appearance/spectral/measured-BSDF/datasets; for Compute — solvers/optimization/ML/numerics; for Rasm — ' +
    'host-neutral geometry/mesh/spatial capability. Rhino/GH2/AEC-adjacent capability is in scope where it is host-neutral. Every named exemplar ' +
    'list here is SEED DATA marking the floor of a sweep, never its extent — the sweep covers the full space the parameters name.',
  'ASSAY + TFM TRUTH: verify a candidate that is in the cache via `uv run --frozen python -m tools.assay api`; for a multi-target NuGet package ' +
    'the consumer floor is net10.0 — confirm the lib/<tfm> a net10 consumer actually binds rather than trusting assay default resolution. Confirm ' +
    'versions/license/Mac/packaging against the registry (NuGet flat-container index + nuspec) truthfully; web research for the domain landscape, ' +
    'newest stable version, and maintenance signals. A package, version, extension, or member that cannot be verified against registry, repo, or ' +
    'assay evidence is a PHANTOM: never survey it, never return it, never admit it.',
  'WRITE DISCIPLINE — strict phase ownership: SURVEY and RESEARCH are the DISCOVERY reconnaissance — read-only is that role ONLY concession, ' +
    'never its depth — and they write NOTHING. ONLY the EXECUTE stage ' +
    'admits packages, and it writes EXACTLY four things and NO more: (a) the central pin in Directory.Packages.props (the matching ItemGroup, ' +
    'newest stable version, plus any pure-managed transitive floor pins the new package needs), (b) the PackageReference in the OWNING folder ' +
    'csproj, (c) the OWNING folder README central-manifest/domain-packages section + prose, and (d) the restore/verify of the owning project so ' +
    'the pin resolves and lands in the lockfile. The EXECUTE stage does NOT author .api catalogs and does NOT touch any design page — full .api ' +
    'authoring is deferred to a focused rebuild-api run over the stubs, and design-page integration is deferred to plan-cs. The STUB stage writes ' +
    'ONLY a one-line placeholder .api catalog file per admitted package (the proper catalog filename, a single placeholder line, nothing else) so ' +
    'the focused rebuild-api run has an exact target list. On a REPLACEMENT the execute stage ripple-removes the superseded package central row + ' +
    'csproj reference + README mention (its .api removal rides the focused rebuild-api stub list).',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (f) => [
  LAW, '',
  'TASK (SURVEY + SYNTHESIZE for folder ' + f.name + (f.primary ? ' — THE PRIMARY FOCUS, go deepest' : '') + ' — DISCOVERY, write nothing): ' +
    'build the full capability MAP of this folder AND emit exactly SIX research facets. Enumerate the ' +
    'real disk state first, never memory: `ls`/`fd` the folder tree and BOTH .api tiers — the folder tier ' + f.api + '/ AND the substrate tier ' +
    'libs/csharp/.api/. Then FULL-read, never skim: the README, the project file' + (f.csproj ? ' (' + f.csproj + ')' : '') + ', every catalog ' +
    'under ' + f.api + '/, every design page under ' + f.planning + '/, the substrate-tier catalogs for packages this folder references, and this ' +
    'folder rows in ' + MANIFEST + '.' + (f.note ? ' ' + f.note : ''),
  'Return the MAP, never a bare verdict: (1) a 1-2 sentence DOMAIN summary; (2) the admitted PACKAGES this folder uses, each carrying a hostile ' +
    'weak/strong call on how fully the design pages exploit it — an admitted capability no page exploits is a GAP row naming the concrete ' +
    'unexploited members, never a footnote; (3) HAND-ROLLS — capabilities the folder implements by hand that a real ecosystem package owns, each ' +
    'with a VERIFIED evidence pointer (design-page section or assay-verified member; an unverifiable pointer is a phantom, never listed); (4) ' +
    'GAPS — the capabilities a WORLD-CLASS, production-ready version of this domain must have but the folder lacks or under-serves, judged against ' +
    'the bleeding-edge state of the art on BOTH naivety axes: COVERAGE — the folder models a thin slice of its concept, the obvious three concerns ' +
    'where the domain carries fifteen; APPROACH — enumerated hand-rolled instances where an admitted package or one parameterized owner should ' +
    'generate the space, a fixed roster of variants being seed data for a generator, never the mechanism; (5) FACETS — EXACTLY SIX non-overlapping ' +
    'research directions that together cover the folder highest-value gaps. Each facet is one focused capability gap a single research agent will ' +
    'hunt the best modern package for (id, direction, the gap it closes, a mandate note). The facet is ALL that researcher receives — pack the map ' +
    'into it: the mandate names the admitted-adjacent packages and substrate-tier capability the candidate must NOT duplicate plus the seams it ' +
    'must integrate with, and the facet is that researcher initial pointer, never a ceiling. For a folder that owns a MANDATED DEEP AREA, those ' +
    'deep areas MUST be among the six facets (e.g. Persistence MUST spend facets on the PostgreSQL first-party+top-25 extension sweep and the ' +
    'DuckDB first-party+top-25 extension sweep). If the folder has fewer than six obvious gaps, split a broad area into sub-facets or cover ' +
    'adjacent in-domain capability so exactly six load-bearing facets are returned. Write nothing.',
].join('\n')
const researchPrompt = (f, survey, facet) => [
  LAW, '',
  'TASK (RESEARCH one facet for folder ' + f.name + ' — DISCOVERY reconnaissance, write nothing, then SELF-VALIDATE): ' +
    'facet=' + facet.id + ' · direction=' + facet.direction + ' ' +
    '· gap=' + facet.gap + (facet.mandate ? ' · mandate=' + facet.mandate : '') + '. Find the best-in-class MODERN package(s) that fill this gap ' +
    'for a net10/osx-arm64 C# AEC stack. The facet is your initial pointer, never a ceiling: re-derive the landscape yourself and sweep the full ' +
    'space its parameters name — the first-found candidate is the presumed-naive pick until the real alternatives are attacked and the strongest ' +
    'named. Judge candidates on BOTH naivety axes: reject a thin-slice package covering one corner of the gap when a full-space owner exists ' +
    '(COVERAGE), and prefer ONE parameterized, generator-grade owner over a roster of narrow point solutions (APPROACH). For a PostgreSQL or ' +
    'DuckDB extension facet, survey the FULL first-party/contrib extension set AND the top 25 community extensions and return every valuable one ' +
    '(not a sample). Use web research for the landscape, newest stable version, maintenance, license, and Mac/osx-arm64 fit; confirm ' +
    'versions/license from the registry where possible and members via `uv run --frozen python -m tools.assay api` when resolvable.',
  'Then SELF-VALIDATE each candidate against the six-condition gate and set ok=true ONLY when ALL pass: bestOf, macOk (osx-arm64), newest (name ' +
    'the current stable release + active maintenance), licenseOk (OSS or free-full commercial), noLegacy (modern packaging/TFM), notDup (not a ' +
    'duplicate of an admitted package or a sibling candidate — set dupOf when it is, and keep only the single best of an overlapping pair). The ' +
    'gate is a FLOOR: hunt disqualifiers beyond the six and fail a candidate on any found. Default ok=false on any unproven condition — an ' +
    'unverified version, license, or member is a phantom that never reaches the payload. Exclude anything already admitted in the central manifest ' +
    'unless it is a strictly stronger replacement (then set fills to name what it replaces). Return candidate(s) each with the gate fields, what ' +
    'gap it fills, newest version, license, the alternatives you compared, and the evidence. Write nothing.',
].join('\n')
const executePrompt = (f, survey, research) => [
  LAW, '',
  'TASK (EXECUTE for folder ' + f.name + ' — WRITE the four owned artifacts only): the six research+self-validation outputs for THIS folder are ' +
    'INLINED at the END of this prompt — consolidate from THAT payload, never run your own package research and never invent candidates beyond it. ' +
    'FIRST consolidate ADVERSARIALLY — an inlined ok=true is a claim to re-derive, never a fact: keep only ok=true candidates whose admission ' +
    'survives re-checking against the central manifest truth (a dup with an admitted row, a phantom version, or scope drift kills the claim), ' +
    'resolve every remaining dup/overlap across the six facets to the single best, and drop anything out of the folder scope — this is the ' +
    'per-folder plan, done here so nothing is dropped. THEN apply each surviving package NOW. ' +
    'Homing map: ' + HOMING + '. The owning ' +
    'folder is ' + f.name + ' (csproj ' + f.csproj + ', .api root ' + f.api + ', central manifest ' + MANIFEST + ').' + (f.note ? ' ' + f.note : ''),
  'For EACH surviving package: (a) add the central pin to Directory.Packages.props in the matching ItemGroup at the newest stable version, with a ' +
    'one-line comment, plus any pure-managed transitive floor pins it needs; (b) add the PackageReference to the owning folder csproj; (c) update ' +
    'the owning folder README central-manifest/domain-packages section + prose so the admission is registered. Do NOT author any .api catalog and ' +
    'do NOT touch any design page — those are deferred (the STUB stage writes a one-line .api placeholder; rebuild-api fills it; plan-cs ' +
    'integrates the design). On a replacement named in fills, ripple-remove the superseded package central row + csproj reference + README mention.',
  'Then RESTORE and VERIFY the owning project so every new pin resolves on osx-arm64 at the newest stable version and the project restores clean — ' +
    'run the gate via `uv run --frozen python -m tools.assay static --project ' + f.csproj + '` (or restore) and parse the JSON Envelope. ' +
    'SELF-HEAL in place on a red gate (wrong ItemGroup, missing pure-managed transitive floor pin, a version that does not exist, a stale README ' +
    'reference). If a package proves genuinely non-resolvable or RID-incompatible on osx-arm64, REVERT its admission entirely (manifest row + ' +
    'csproj reference + README mention) and record it under skipped with the reason — a non-Mac package must not be admitted. For EACH applied ' +
    'package report a canonical apiPath = the owning folder .api root + the catalog filename that matches the sibling .api naming convention in ' +
    'that folder (the STUB stage creates exactly these files). Return the fix-log: folder, applied [{package, version, license, apiPath}], skipped ' +
    '[{package, why}], files edited, green (true only if the final restore is clean on osx-arm64), and a one-line summary.',
  'THE SIX RESEARCH + SELF-VALIDATION OUTPUTS for ' + f.name + ' (consolidate ONLY the ok=true candidates present in this payload; never fabricate ' +
    'a package, version, or member beyond it; if no ok=true candidate is present, admit nothing and report that under summary):\n' + JSON.stringify(research, null, 1),
].join('\n')
const stubPrompt = (f, ex) => [
  LAW, '',
  'TASK (STUB the .api catalogs for folder ' + f.name + ' — WRITE one-line placeholders ONLY): the execute stage admitted these packages with ' +
    'their canonical catalog paths: ' + JSON.stringify((ex && ex.applied) || []) + '. For EACH applied package, create the file at its apiPath ' +
    '(under the owning folder .api root ' + f.api + (f.note ? '; ' + f.note : '') + ') containing EXACTLY ONE line: a placeholder marker that ' +
    'names the package and marks the catalog research-pending. NOTHING ELSE — no header block, no member sections, no real members, no prose, no ' +
    'second line. The file exists only so a focused rebuild-api run has an exact target to fill. If a directory in the path is missing, create it. ' +
    'Return the list of stub file paths you created.',
].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

const results = []
for (const f of FOLDERS) {
  log('=== FOLDER: ' + f.name + (f.primary ? ' (PRIMARY)' : '') + ' ===')

  phase('Survey')
  const survey = await agent(surveyPrompt(f), { label: 'survey:' + f.name, phase: 'Survey', schema: SURVEY_SCHEMA, model: 'opus', effort: 'xhigh', stallMs: STALL })
  if (!survey || !(survey.facets && survey.facets.length)) { results.push({ folder: f.name, applied: 0, note: 'no survey/facets returned' }); continue }
  log(f.name + ' survey: ' + survey.facets.length + ' research facets, ' + ((survey.gaps && survey.gaps.length) || 0) + ' gaps')

  phase('Research')
  const research = (await parallel(survey.facets.map((fc) => () =>
    agent(researchPrompt(f, survey, fc), { label: 'research:' + f.name + ':' + fc.id, phase: 'Research', schema: RESEARCH_SCHEMA, model: 'opus', effort: 'xhigh', stallMs: STALL })
  ))).filter(Boolean)
  const okCount = research.reduce((n, r) => n + ((r.candidates || []).filter((c) => c.ok).length), 0)
  log(f.name + ' research: ' + research.length + '/' + survey.facets.length + ' facets returned, ' + okCount + ' gated candidates')

  phase('Execute')
  const ex = await agent(executePrompt(f, survey, research), { label: 'exec:' + f.name, phase: 'Execute', schema: EXEC_SCHEMA, model: 'opus', effort: 'xhigh', stallMs: EXEC_STALL })
  const applied = (ex && ex.applied) || []
  log(f.name + ' execute: ' + applied.length + ' applied, ' + (((ex && ex.skipped) || []).length) + ' skipped, green=' + (ex && ex.green))

  let stub = null
  if (applied.length) {
    phase('Stub')
    stub = await agent(stubPrompt(f, ex), { label: 'stub:' + f.name, phase: 'Stub', schema: STUB_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
    log(f.name + ' stub: ' + ((stub && stub.stubs && stub.stubs.length) || 0) + ' .api stubs created')
  }

  results.push({ folder: f.name, applied: applied.length, green: !!(ex && ex.green), packages: applied.map((a) => a.package), apiStubs: (stub && stub.stubs) || [], skipped: (ex && ex.skipped) || [], summary: (ex && ex.summary) || '' })
}

const totalApplied = results.reduce((n, r) => n + (r.applied || 0), 0)
const allStubs = results.flatMap((r) => r.apiStubs || [])
log('survey-gaps complete: ' + results.length + ' folders, ' + totalApplied + ' packages admitted, ' + allStubs.length + ' .api stubs for the ' +
  'focused rebuild-api run')
return { scope: 'csharp-aec-stack', mode: 'per-folder-sequential', folders: results.length, totalApplied: totalApplied, apiStubs: allStubs, results: results }
