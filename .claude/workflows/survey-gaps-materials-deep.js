export const meta = {
  name: 'survey-gaps-materials-deep',
  description: 'ADDITIVE, ULTRA-DEEP ecosystem dive for Rasm.Materials centered on the VividOrange structural-section publisher and EVERY adjacent package. Materials already admitted VividOrange.Profiles.Catalogue + VividOrange.Sections.SectionProperties (+ IProfiles/ISerialization floors) and RectangleBinPack; this pass goes ULTRA-DEEP: survey the FULL VividOrange.* publisher catalog (every package they ship) AND the entire adjacent ecosystem around it — steel/timber/concrete/cold-formed section libraries and regional standards, connection/fastener design, structural material design-code models, and arbitrary-section property/analysis solvers — so the folder owns hundreds of profiles and the full structural-section/connection surface through real packages, not hand-rolled tables. EIGHT opus xhigh research facets. One opus xhigh survey+synthesize agent emits the gaps and eight facets, eight opus xhigh research agents each hunt one facet and self-validate the six-condition gate, one opus xhigh execute agent consolidates and ADDS to the existing Materials admissions (read from the manifest/README/csproj, never duplicated) then restores and verifies at newest on osx-arm64, and one sonnet agent writes a one-line placeholder .api catalog per NEWLY admitted package. The full .api authoring and design integration are deferred to a focused rebuild-api run and to plan-cs-folders.',
  whenToUse: 'Re-run Materials as an ultra-deep dive into the full VividOrange package catalog plus every adjacent structural-section/connection/design package, additive over the existing admissions.',
  phases: [
    { title: 'Survey', detail: 'one opus xhigh agent: domain, admitted packages, hand-rolls, gaps, AND eight VividOrange-catalog + adjacent-ecosystem research facets', model: 'opus' },
    { title: 'Research', detail: 'eight opus xhigh agents, one facet each: best-in-class modern candidate discovery with a self-validated six-condition gate', model: 'opus' },
    { title: 'Execute', detail: 'one opus xhigh agent: consolidate the eight findings, ADD to the existing admissions, update csproj + README, restore and verify the install at newest on osx-arm64, self-heal or revert', model: 'opus' },
    { title: 'Stub', detail: 'one sonnet agent: write a one-line placeholder .api catalog file per NEWLY admitted package, nothing else', model: 'sonnet' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 420000
const EXEC_STALL = 900000
const FACET_COUNT = 8
const F = { name: 'Materials', primary: false, doc: 'libs/csharp/Rasm.Materials', api: 'libs/csharp/Rasm.Materials/.api', planning: 'libs/csharp/Rasm.Materials/.planning', csproj: 'libs/csharp/Rasm.Materials/Rasm.Materials.csproj', note: '' }
const MANIFEST = 'Directory.Packages.props (central NuGet pins) + Directory.Build.props (TargetFramework net10.0, RuntimeIdentifiers osx-arm64)'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'gaps', 'facets'], properties: { folder: { type: 'string' }, domain: { type: 'string' }, packages: { type: 'array', items: { type: 'string' } }, handRolls: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, evidence: { type: 'string' } } } }, gaps: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, severity: { type: 'string' }, note: { type: 'string' } } } }, facets: { type: 'array', minItems: FACET_COUNT, maxItems: FACET_COUNT, items: { type: 'object', additionalProperties: false, required: ['id', 'direction', 'gap'], properties: { id: { type: 'string' }, direction: { type: 'string' }, gap: { type: 'string' }, mandate: { type: 'string' } } } } } }
const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['facet', 'candidates'], properties: { facet: { type: 'string' }, candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'fills', 'ok'], properties: { package: { type: 'string' }, fills: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, bestOf: { type: 'boolean' }, macOk: { type: 'boolean' }, newest: { type: 'boolean' }, licenseOk: { type: 'boolean' }, noLegacy: { type: 'boolean' }, notDup: { type: 'boolean' }, dupOf: { type: 'string' }, ok: { type: 'boolean' }, alternativesConsidered: { type: 'string' }, evidence: { type: 'string' } } } } } }
const EXEC_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'applied'], properties: { folder: { type: 'string' }, applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'apiPath'], properties: { package: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, apiPath: { type: 'string' } } } }, skipped: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: { package: { type: 'string' }, why: { type: 'string' } } } }, files: { type: 'array', items: { type: 'string' } }, green: { type: 'boolean' }, summary: { type: 'string' } } }
const STUB_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'stubs'], properties: { folder: { type: 'string' }, stubs: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const ADDITIVE = 'This is an ADDITIVE ultra-deep pass over a folder that already received prior survey admissions (notably ' +
  'VividOrange.Profiles.Catalogue, VividOrange.Sections.SectionProperties, VividOrange.IProfiles, VividOrange.ISerialization, ' +
  'RectangleBinPack.CSharp, plus UnitsNet and Wacton.Unicolour). READ the folder current admissions (its Directory.Packages.props rows, its README ' +
  'roster, its csproj PackageReference list) and treat every already-admitted package as ALREADY-PRESENT for the no-dup gate — KEEP them all ' +
  'untouched, never duplicate or remove them, and admit only NEW packages this deeper pass surfaces.'
const LAW = [
  'Rasm monorepo, PER-FOLDER capability-gap survey-AND-APPLY of the C# AEC stack (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP ' +
    'strata, depend strictly upward). This run is the ULTRA-DEEP, ADDITIVE re-pass of Rasm.Materials centered on the VividOrange ' +
    'structural-section ecosystem. The mission is NOT a per-package keep/replace audit — it is to find AND admit every world-class MODERN package ' +
    'this folder is still MISSING in the VividOrange + adjacent structural-section/connection/design domain, and to STOP hand-rolling ' +
    'profile/section/connection tables a real package owns. Central pins live in ' + MANIFEST + '. ' + ADDITIVE,
  'BAR: Materials must own HUNDREDS of structural profiles/sections and the full connection/section-design surface RIGHT NOW through real, ' +
    'maintained packages — not hand-rolled enumerations. Treat the prior passes as INCOMPLETE in this ecosystem: assume more of the VividOrange ' +
    'catalog and its adjacent packages are still missing and hunt them aggressively. Where the folder hand-rolls a section table, a ' +
    'section-property computation, a connection capacity, or a design-code check that a real package owns, that is a NAMED GAP. A large additive ' +
    'set is the GOAL here IF each entry is best-in-class, production-grade, and non-overlapping.',
  'AGGRESSIVE ADDITION, ZERO HEDGING: admit a package whenever it provides unique, modern, production-grade capability appropriate to the ' +
    'structural-section/profile/connection/material-design domain. DECLINE ONLY for a real reason — (a) old / unmaintained / low-quality / naive, ' +
    '(b) a strictly stronger alternative is already admitted or recommended, or (c) out of the folder scope. NEVER decline for no current consumer.',
  'VALIDATION GATE — every candidate must pass all six, and the RESEARCH agent self-critiques it BEFORE returning (ok=true only when all six hold; ' +
    'default ok=false on any unproven condition): (1) BEST-OF — the strongest, most COMPLETE package for the gap, not a naive wrapper; compare the ' +
    'real alternatives. (2) MAC — works on osx-arm64 (managed AnyCPU, an osx-arm64 native asset, or a Forge-provisioned substrate; reject ' +
    'win/x64-only). (3) NEWEST — current stable release named, actively maintained. (4) LICENSE — OSS (any OSI license) OR free-full commercial ' +
    '(no paid tier/seat cap/usage gate/eval-only); reject fee/tiered AND viral copyleft (GPL/AGPL) for a host-neutral library. (5) NO LEGACY ' +
    'PACKAGING — modern packaging/TFM/abi. (6) NO DUP/OVERLAP — not a duplicate of an already-admitted package (read the current folder ' +
    'admissions) or a sibling candidate; set dupOf and keep the single best. A package not published on NuGet cannot become a central pin — reject ' +
    'it as a pin (note it only as a capability).',
  'MANDATED DEEP AREAS for this ULTRA pass (the survey MUST allocate the eight facets across these, and the research MUST sweep each ' +
    'EXHAUSTIVELY): (A) THE FULL VividOrange.* PUBLISHER CATALOG — enumerate EVERY package the VividOrange publisher ships on NuGet (Profiles, ' +
    'Sections, IProfiles, ISerialization, and any others — connections, loads, design, codes, materials, analysis) and admit every useful one not ' +
    'yet admitted; treat the publisher as a coherent ecosystem to mine fully. CRITICAL — DEPENDENCY CLOSURE / REFERENCED-BUT-NOT-ADMITTED: for ' +
    'EVERY already-admitted VividOrange package, resolve every type/interface it EXPOSES on its public surface and admit the VividOrange package ' +
    'that DEFINES any PHANTOM type — one cited in an .api or required by an admitted surface but resolving via assay to no source/unknown because ' +
    'its owning package is not admitted. KNOWN OPEN INSTANCE: VividOrange.Sections.SectionProperties exposes a Concrete (RC) transformed-section ' +
    'solver requiring IConcreteSection / IRebar / ILongitudinalReinforcement / ILink, which live in the unadmitted ' +
    'VividOrange.Sections.Reinforcement — admit that package plus any sibling reinforcement/interface package it needs so the RC solver and every ' +
    'admitted VividOrange surface is fully COMPOSABLE with zero phantom types. (B) STEEL SECTION LIBRARIES + regional standards (AISC, ' +
    'EN/Eurocode, BS, AS/NZS, CISC, JIS) the catalog does not already cover. (C) TIMBER / GLULAM / CLT + COLD-FORMED / LIGHT-GAUGE section ' +
    'libraries and design data. (D) CONCRETE sections, reinforcement/rebar catalogs, and prestressing data. (E) CONNECTION DESIGN — bolted/welded ' +
    'steel connections, timber connectors/fasteners, and the standards data behind them. (F) STRUCTURAL MATERIAL DESIGN-CODE models — ' +
    'EN1993/1995/1992, AISC360, and the design-check input data a real package owns. (G) ARBITRARY-SECTION PROPERTY + cross-section analysis ' +
    'solvers (warping/shear-centre/plastic-modulus/torsion constants) beyond the admitted section-property solver. (H) any remaining ' +
    'structural-section/profile/connection concern the folder HAND-ROLLS that a package owns. For a server-side or non-NuGet capability, note it ' +
    'but only admit the managed .NET package that drives it.',
  'ASSAY + TFM TRUTH: verify a candidate in the cache via `uv run --frozen python -m tools.assay api`; for a multi-target NuGet package the ' +
    'consumer floor is net10.0 — confirm the lib/<tfm> a net10 consumer actually binds. Confirm versions/license/Mac/packaging against the ' +
    'registry (NuGet flat-container index + nuspec) truthfully; for the VividOrange catalog, query the publisher package list on NuGet directly; ' +
    'web research for the adjacent-ecosystem landscape, newest stable version, and maintenance signals.',
  'WRITE DISCIPLINE — strict phase ownership: SURVEY and RESEARCH are READ-ONLY and write NOTHING. ONLY EXECUTE admits packages, and it writes ' +
    'EXACTLY four things: (a) the central pin in Directory.Packages.props (matching ItemGroup, newest stable version, plus any pure-managed ' +
    'transitive floor pins), (b) the PackageReference in the owning folder csproj, (c) the owning folder README roster + prose, and (d) the ' +
    'restore/verify of the owning project. This is ADDITIVE — KEEP every existing admission, add only NEW packages, and report ONLY the ' +
    'newly-admitted packages in applied. EXECUTE does NOT author .api catalogs and does NOT touch any design page (deferred to a focused ' +
    'rebuild-api run and to plan-cs-folders). The STUB stage writes ONLY a one-line placeholder .api file per NEWLY admitted package.',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (f) => [
  LAW, '',
  'TASK (SURVEY + SYNTHESIZE for folder ' + f.name + ' — the VividOrange-ecosystem ultra-deep focus, go deepest — READ-ONLY, write nothing): build ' +
    'the full capability picture AND emit exactly ' + FACET_COUNT + ' research facets. Read its README + project file (' + f.csproj + '), every ' +
    'catalog under ' + f.api + '/ (especially the VividOrange catalogs already authored), the design pages under ' + f.planning + '/ (the ' +
    'Profiles, Construction, Connection, Properties sub-domains), and the central manifest ' + MANIFEST + ' for this folder rows. The folder ' +
    'already admits the VividOrange profiles/sections packages and others from prior passes — account for them so the facets target what is still ' +
    'MISSING across the VividOrange catalog and its adjacent structural-section/connection/design ecosystem.',
  'Return: (1) a 1-2 sentence DOMAIN summary; (2) the admitted PACKAGES; (3) HAND-ROLLS — section tables, section-property computations, ' +
    'connection capacities, or design-code checks implemented by hand that a real package owns (each with an evidence pointer); (4) GAPS — ' +
    'capabilities a world-class structural-section/connection/material-design folder must have but the folder lacks, INCLUDING every ' +
    'REFERENCED-BUT-NOT-ADMITTED phantom: a type/interface an already-admitted VividOrange package EXPOSES that resolves via assay to no ' +
    'source/unknown because its owning package is unadmitted (the known instance: IConcreteSection/IRebar/ILongitudinalReinforcement/ILink from ' +
    'the unadmitted VividOrange.Sections.Reinforcement, required by the VividOrange.Sections.SectionProperties RC solver) — name each missing ' +
    'owning package so a facet admits it; (5) FACETS — EXACTLY ' + FACET_COUNT + ' non-overlapping research directions covering the MANDATED DEEP ' +
    'AREAS (A-H): allocate at least one facet to the FULL VividOrange.* publisher-catalog enumeration, and spread the rest across ' +
    'steel/timber/concrete/cold-formed section libraries, connection design, structural design-code models, and section-property solvers. Each ' +
    'facet: id, direction, the gap it closes, an optional mandate note. Be skeptical and ambitious — assume the prior passes only scratched the ' +
    'VividOrange ecosystem. Write nothing.',
].join('\n')
const researchPrompt = (f, survey, facet) => [
  LAW, '',
  'TASK (RESEARCH one facet for folder ' + f.name + ' — READ-ONLY, write nothing, then SELF-VALIDATE): facet=' + facet.id + ' · direction=' + facet.direction + ' ' +
    '· gap=' + facet.gap + (facet.mandate ? ' · mandate=' + facet.mandate : '') + '. Find the best-in-class, PRODUCTION-GRADE, FULL-FEATURED ' +
    'MODERN package(s) that fill this gap for a net10/osx-arm64 C# AEC stack. For the VividOrange-catalog facet, ENUMERATE every package the ' +
    'VividOrange publisher ships on NuGet (query the registry directly) and return every useful one not already admitted. ALSO close the ' +
    'DEPENDENCY CLOSURE: for each admitted VividOrange package, find every package whose types it references but which is not admitted (confirm ' +
    'via `uv run --frozen python -m tools.assay api` that a referenced type currently resolves to no source), and return those missing owning ' +
    'packages as ok=true candidates — e.g. VividOrange.Sections.Reinforcement, which defines the ' +
    'IConcreteSection/IRebar/ILongitudinalReinforcement/ILink the SectionProperties RC solver requires; a closure package that makes an ' +
    'already-admitted-but-uncomposable surface composable clears BEST-OF by definition. For an adjacent-ecosystem facet ' +
    '(steel/timber/concrete/cold-formed sections, connections, design codes, section-property solvers), compare the real alternatives and name the ' +
    'strongest, most complete option — NEVER a naive single-feature wrapper. Use web research for the landscape, newest stable version, ' +
    'maintenance, license, and Mac/osx-arm64 fit; confirm versions/license from the registry and members via `uv run --frozen python -m ' +
    'tools.assay api` when resolvable.',
  'Then SELF-VALIDATE each candidate against the six-condition gate and set ok=true ONLY when ALL pass: bestOf (strongest, most complete), macOk ' +
    '(osx-arm64), newest (current stable + active maintenance), licenseOk (OSS or free-full commercial, NOT GPL/AGPL viral for a host-neutral ' +
    'lib), noLegacy (modern packaging/TFM), notDup (not a duplicate of an admitted package — read the current folder admissions, including the ' +
    'VividOrange packages already pinned — or a sibling candidate; set dupOf, keep the single best). A package not on NuGet fails the gate as a ' +
    'pin. Default ok=false on any unproven condition. Exclude anything already admitted unless a strictly stronger replacement (set fills to name ' +
    'what it replaces). Return candidate(s) each with the gate fields, what gap it fills, newest version, license, the alternatives compared, and ' +
    'the evidence. Write nothing.',
].join('\n')
const executePrompt = (f, survey, research) => [
  LAW, '',
  'TASK (EXECUTE for folder ' + f.name + ' — ADDITIVE, WRITE the four owned artifacts only): the ' + FACET_COUNT + ' research+self-validation ' +
    'outputs for THIS folder are INLINED at the END of this prompt — consolidate from THAT payload, never run your own package research and never ' +
    'invent candidates beyond it. FIRST consolidate: keep only candidates with ok=true, resolve every remaining dup/overlap across the facets to ' +
    'the single best, drop anything already admitted (read the current folder admissions) and anything out of the Materials scope — this is the ' +
    'per-folder plan, done here so nothing is dropped. PRIORITIZE every dependency-closure / referenced-but-not-admitted package (e.g. ' +
    'VividOrange.Sections.Reinforcement) — admit it even when it is a pure interface/contract package, because it resolves an UNCOMPOSABLE surface ' +
    'that an already-admitted package exposes. THEN apply each surviving NEW package NOW. The owning folder is ' + f.name + ' (csproj ' + f.csproj + ', ' +
    '.api root ' + f.api + ', central manifest ' + MANIFEST + ').',
  'For EACH surviving NEW package: (a) add the central pin to Directory.Packages.props in the matching ItemGroup at the newest stable version, ' +
    'with a one-line comment, plus any pure-managed transitive floor pins; (b) add the PackageReference to the owning folder csproj; (c) update ' +
    'the owning folder README roster + prose. ADDITIVE — KEEP every existing admission untouched, never remove or duplicate. Do NOT author any ' +
    '.api catalog and do NOT touch any design page (deferred).',
  'Then RESTORE and VERIFY the owning project so every new pin resolves on osx-arm64 at the newest stable version and the project restores clean — ' +
    'run the gate via `uv run --frozen python -m tools.assay static --project ' + f.csproj + '` (or restore) and parse the JSON Envelope. ' +
    'SELF-HEAL in place on a red gate. If a package proves non-resolvable or RID-incompatible on osx-arm64, REVERT its admission entirely and ' +
    'record it under skipped with the reason. For EACH NEWLY applied package report a canonical apiPath = the owning folder .api root + the ' +
    'catalog filename matching the sibling .api naming convention. Report ONLY newly-admitted packages in applied. Return the fix-log: folder, ' +
    'applied [{package, version, license, apiPath}], skipped [{package, why}], files edited, green, and a one-line summary.',
  'THE ' + FACET_COUNT + ' RESEARCH + SELF-VALIDATION OUTPUTS for ' + f.name + ' (consolidate ONLY the ok=true candidates present in this payload; ' +
    'never fabricate a package, version, or member beyond it; if no ok=true candidate is present, admit nothing and report that under summary):\n' + JSON.stringify(research, null, 1),
].join('\n')
const stubPrompt = (f, ex) => [
  LAW, '',
  'TASK (STUB the .api catalogs for folder ' + f.name + ' — WRITE one-line placeholders ONLY): the execute stage NEWLY admitted these packages ' +
    'with their canonical catalog paths: ' + JSON.stringify((ex && ex.applied) || []) + '. For EACH applied package, create the file at its ' +
    'apiPath (under the owning folder .api root ' + f.api + ') containing EXACTLY ONE line: a placeholder marker that names the package and marks ' +
    'the catalog research-pending. NOTHING ELSE — no header block, no member sections, no real members, no prose, no second line. If a directory ' +
    'in the path is missing, create it. Return the list of stub file paths you created.',
].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('=== survey-gaps single-folder (ULTRA-DEEP/ADDITIVE, VividOrange ecosystem): ' + F.name + ' ===')

phase('Survey')
const survey = await agent(surveyPrompt(F), { label: 'survey:' + F.name, phase: 'Survey', schema: SURVEY_SCHEMA, model: 'opus', effort: 'xhigh', stallMs: STALL })
if (!survey || !(survey.facets && survey.facets.length)) return { folder: F.name, applied: 0, note: 'no survey/facets returned' }
log(F.name + ' survey: ' + survey.facets.length + ' research facets, ' + ((survey.gaps && survey.gaps.length) || 0) + ' gaps')

// --- [RESEARCH]
phase('Research')
const research = (await parallel(survey.facets.map((fc) => () =>
  agent(researchPrompt(F, survey, fc), { label: 'research:' + F.name + ':' + fc.id, phase: 'Research', schema: RESEARCH_SCHEMA, model: 'opus', effort: 'xhigh', stallMs: STALL })
))).filter(Boolean)
const okCount = research.reduce((n, r) => n + ((r.candidates || []).filter((c) => c.ok).length), 0)
log(F.name + ' research: ' + research.length + '/' + survey.facets.length + ' facets returned, ' + okCount + ' gated candidates')

// --- [EXECUTE]
phase('Execute')
const ex = await agent(executePrompt(F, survey, research), { label: 'exec:' + F.name, phase: 'Execute', schema: EXEC_SCHEMA, model: 'opus', effort: 'xhigh', stallMs: EXEC_STALL })
const applied = (ex && ex.applied) || []
log(F.name + ' execute: ' + applied.length + ' newly applied, ' + (((ex && ex.skipped) || []).length) + ' skipped, green=' + (ex && ex.green))

let stub = null
if (applied.length) {
  phase('Stub')
  stub = await agent(stubPrompt(F, ex), { label: 'stub:' + F.name, phase: 'Stub', schema: STUB_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
  log(F.name + ' stub: ' + ((stub && stub.stubs && stub.stubs.length) || 0) + ' .api stubs created')
}

const apiStubs = (stub && stub.stubs) || []
log('survey-gaps ' + F.name + ' (ultra-deep/additive) complete: ' + applied.length + ' packages newly admitted, ' + apiStubs.length + ' .api stubs ' +
  'for the focused rebuild-api run')
return { folder: F.name, applied: applied.length, green: !!(ex && ex.green), packages: applied.map((a) => a.package), apiStubs: apiStubs, skipped: (ex && ex.skipped) || [], summary: (ex && ex.summary) || '' }
