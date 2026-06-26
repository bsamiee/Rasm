export const meta = {
  name: 'survey-gaps-fabrication-deep',
  description: 'ADDITIVE, broadened second-pass capability-gap survey-AND-APPLY for Rasm.Fabrication, weighted hard toward CAM/toolpath generation, post-processing/G-code, nesting/packing optimization, sheet-metal unfolding, robotics kinematics and motion planning, additive-manufacturing slicing, and machining data so the folder stops hand-rolling concerns a real ecosystem package owns. The first pass found a mature core (NFP/IFP/GA nesting, geometry3Sharp, Clipper2) and admitted only Robots; this deeper EIGHT-facet pass digs the long tail. One opus xhigh survey+synthesize agent emits the gaps and eight facets, eight opus xhigh research agents each hunt one facet and self-validate the six-condition gate, one opus xhigh execute agent consolidates and ADDS to the existing Fabrication admissions (read from the manifest/README/csproj, never duplicated) then restores and verifies at newest on osx-arm64, and one sonnet agent writes a one-line placeholder .api catalog per NEWLY admitted package. The full .api authoring and design integration are deferred to a focused rebuild-api run and to plan-cs-folders.',
  whenToUse: 'Re-run Fabrication with deeper coverage of CAM/toolpath, post-processing, nesting, sheet-metal, robotics, AM slicing, and machining data, additive over the existing admissions.',
  phases: [
    { title: 'Survey', detail: 'one opus xhigh agent: domain, admitted packages, hand-rolls, gaps, AND eight CAM/toolpath/nesting/robotics/AM research facets', model: 'opus' },
    { title: 'Research', detail: 'eight opus xhigh agents, one facet each: best-in-class modern candidate discovery with a self-validated six-condition gate', model: 'opus' },
    { title: 'Execute', detail: 'one opus xhigh agent: consolidate the eight findings, ADD to the existing admissions, update csproj + README, restore and verify the install at newest on osx-arm64, self-heal or revert', model: 'opus' },
    { title: 'Stub', detail: 'one sonnet agent: write a one-line placeholder .api catalog file per NEWLY admitted package, nothing else', model: 'sonnet' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 420000
const EXEC_STALL = 900000
const FACET_COUNT = 8
const F = { name: 'Fabrication', primary: false, doc: 'libs/csharp/Rasm.Fabrication', api: 'libs/csharp/Rasm.Fabrication/.api', planning: 'libs/csharp/Rasm.Fabrication/.planning', csproj: 'libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj', note: '' }
const MANIFEST = 'Directory.Packages.props (central NuGet pins) + Directory.Build.props (TargetFramework net10.0, RuntimeIdentifiers osx-arm64)'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'gaps', 'facets'], properties: { folder: { type: 'string' }, domain: { type: 'string' }, packages: { type: 'array', items: { type: 'string' } }, handRolls: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, evidence: { type: 'string' } } } }, gaps: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, severity: { type: 'string' }, note: { type: 'string' } } } }, facets: { type: 'array', minItems: FACET_COUNT, maxItems: FACET_COUNT, items: { type: 'object', additionalProperties: false, required: ['id', 'direction', 'gap'], properties: { id: { type: 'string' }, direction: { type: 'string' }, gap: { type: 'string' }, mandate: { type: 'string' } } } } } }
const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['facet', 'candidates'], properties: { facet: { type: 'string' }, candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'fills', 'ok'], properties: { package: { type: 'string' }, fills: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, bestOf: { type: 'boolean' }, macOk: { type: 'boolean' }, newest: { type: 'boolean' }, licenseOk: { type: 'boolean' }, noLegacy: { type: 'boolean' }, notDup: { type: 'boolean' }, dupOf: { type: 'string' }, ok: { type: 'boolean' }, alternativesConsidered: { type: 'string' }, evidence: { type: 'string' } } } } } }
const EXEC_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'applied'], properties: { folder: { type: 'string' }, applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'apiPath'], properties: { package: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, apiPath: { type: 'string' } } } }, skipped: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: { package: { type: 'string' }, why: { type: 'string' } } } }, files: { type: 'array', items: { type: 'string' } }, green: { type: 'boolean' }, summary: { type: 'string' } } }
const STUB_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'stubs'], properties: { folder: { type: 'string' }, stubs: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const ADDITIVE = 'This is an ADDITIVE second pass over a folder that already received a prior survey admission (notably Robots, plus the mature ' +
  'incumbents geometry3Sharp, Clipper2, RectpackSharp, and a hand-authored NFP/IFP/GA nesting + author kernel). READ the folder current admissions ' +
  '(its Directory.Packages.props rows, its README roster, its csproj PackageReference list) and treat every already-admitted package as ' +
  'ALREADY-PRESENT for the no-dup gate — KEEP them all untouched, never duplicate or remove them, and admit only NEW packages this deeper pass ' +
  'surfaces.'
const LAW = [
  'Rasm monorepo, PER-FOLDER capability-gap survey-AND-APPLY of the C# AEC stack (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP ' +
    'strata, depend strictly upward). This run is the DEEP, ADDITIVE re-pass of Rasm.Fabrication — the portable fabrication folder ' +
    '(HLR/CAM/nesting/toolpath/robotics). The mission is NOT a per-package keep/replace audit — it is to find AND admit the world-class MODERN ' +
    'packages this folder is still MISSING in the CAM/toolpath/nesting/robotics/manufacturing domain, and to STOP hand-rolling concerns a real ' +
    'package owns. Central pins live in ' + MANIFEST + '. ' + ADDITIVE,
  'BAR: Fabrication must back a world-class, production-ready CAM/fabrication platform RIGHT NOW. Treat the first pass as having covered only the ' +
    'core: assume valuable modern CAM/toolpath/nesting/robotics/AM packages are still missing and hunt them aggressively. Where the folder ' +
    'HAND-ROLLS a concern a real ecosystem package owns — a toolpath strategy, a G-code post, a nesting heuristic, a forward/inverse kinematics ' +
    'solve, a sheet-metal unfold, a slicer — that is a NAMED GAP. A large additive set is the GOAL here IF each entry is best-in-class, ' +
    'production-grade, and non-overlapping.',
  'AGGRESSIVE ADDITION, ZERO HEDGING: admit a package whenever it provides unique, modern, production-grade capability appropriate to the ' +
    'fabrication/CAM/nesting/robotics domain. DECLINE ONLY for a real reason — (a) old / unmaintained / low-quality / naive, (b) a strictly ' +
    'stronger alternative is already admitted or recommended (the mature NFP/IFP/GA nesting engine, geometry3Sharp, Clipper2, Robots), or (c) out ' +
    'of the folder scope. NEVER decline for no current consumer.',
  'VALIDATION GATE — every candidate must pass all six, and the RESEARCH agent self-critiques it BEFORE returning (ok=true only when all six hold; ' +
    'default ok=false on any unproven condition): (1) BEST-OF — the strongest, most COMPLETE package for the gap, not a naive wrapper, and ' +
    'genuinely stronger than the mature incumbent it would sit beside; compare the real alternatives. (2) MAC — works on osx-arm64 (managed ' +
    'AnyCPU, an osx-arm64 native asset, or a Forge-provisioned substrate; reject win/x64-only). (3) NEWEST — current stable release named, ' +
    'actively maintained. (4) LICENSE — OSS (any OSI license) OR free-full commercial (no paid tier/seat cap/usage gate/eval-only); reject ' +
    'fee/tiered AND viral copyleft (GPL/AGPL) for a host-neutral library. (5) NO LEGACY PACKAGING — modern packaging/TFM/abi. (6) NO DUP/OVERLAP — ' +
    'not a duplicate of an already-admitted package (read the current folder admissions) or a sibling candidate; set dupOf and keep the single ' +
    'best. A package not published on NuGet cannot become a central pin — reject it as a pin (note it only as a capability).',
  'MANDATED DEEP AREAS for Fabrication (the survey MUST allocate the eight facets across these, and the research MUST sweep each EXHAUSTIVELY): ' +
    '(A) CAM / TOOLPATH GENERATION — pocketing, contouring, drilling, adaptive/trochoidal clearing, rest-machining, 3-axis surface strategies. (B) ' +
    'POST-PROCESSING / G-CODE — G-code/CNC dialect emit + parse, post-processor frameworks, controller dialects (Fanuc/Haas/GRBL/LinuxCNC/Marlin). ' +
    '(C) NESTING / PACKING optimization beyond the incumbent NFP/IFP/GA engine (true-shape irregular nesting, multi-sheet/bin, remnant). (D) ' +
    'SHEET-METAL — unfolding/flat-pattern, bend allowance, K-factor, press-brake sequencing. (E) ROBOTICS — forward/inverse kinematics, motion ' +
    'planning, trajectory generation beyond the admitted Robots package (jerk-limited profiles, collision-aware planning). (F) ADDITIVE ' +
    'MANUFACTURING — slicing, infill, support generation, gcode for FFF/SLA. (G) MACHINING DATA — feeds/speeds, tool libraries, material ' +
    'cutting-data (where a free-full package exists). (H) any remaining fabrication concern the folder HAND-ROLLS that a package owns. For a ' +
    'server-side or non-NuGet capability, note it but only admit the managed .NET package that drives it.',
  'ASSAY + TFM TRUTH: verify a candidate in the cache via `uv run --frozen python -m tools.assay api`; for a multi-target NuGet package the ' +
    'consumer floor is net10.0 — confirm the lib/<tfm> a net10 consumer actually binds. Confirm versions/license/Mac/packaging against the ' +
    'registry (NuGet flat-container index + nuspec) truthfully; web research for the domain landscape, newest stable version, and maintenance ' +
    'signals.',
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
  'TASK (SURVEY + SYNTHESIZE for folder ' + f.name + ' — the CAM/toolpath/nesting/robotics deep focus, go deepest — READ-ONLY, write nothing): ' +
    'build the full capability picture AND emit exactly ' + FACET_COUNT + ' research facets. Read its README + project file (' + f.csproj + '), ' +
    'every catalog under ' + f.api + '/, the design pages under ' + f.planning + '/ (Toolpath/kinematics, nesting, post-processing), and the ' +
    'central manifest ' + MANIFEST + ' for this folder rows. The folder already admits Robots and carries a mature nesting/geometry core — account ' +
    'for them so the facets target what is still MISSING or still HAND-ROLLED.',
  'Return: (1) a 1-2 sentence DOMAIN summary; (2) the admitted PACKAGES; (3) HAND-ROLLS — toolpath strategies, G-code posts, nesting heuristics, ' +
    'kinematics solves, sheet-metal unfolds, or slicers implemented by hand that a real package owns (each with an evidence pointer) — the PRIMARY ' +
    'signal for this pass; (4) GAPS — capabilities a world-class CAM/fabrication platform must have but the folder lacks; (5) FACETS — EXACTLY ' + FACET_COUNT + ' ' +
    'non-overlapping research directions covering the MANDATED DEEP AREAS (A-H): allocate facets so CAM/toolpath, post-processing/G-code, advanced ' +
    'nesting, sheet-metal, robotics/motion-planning, AM slicing, and machining data each get coverage. Each facet: id, direction, the gap it ' +
    'closes, an optional mandate note. Be skeptical and ambitious, but respect that the incumbent nesting/geometry core is strong — only surface ' +
    'genuinely stronger or genuinely new capability. Write nothing.',
].join('\n')
const researchPrompt = (f, survey, facet) => [
  LAW, '',
  'TASK (RESEARCH one facet for folder ' + f.name + ' — READ-ONLY, write nothing, then SELF-VALIDATE): facet=' + facet.id + ' · direction=' + facet.direction + ' ' +
    '· gap=' + facet.gap + (facet.mandate ? ' · mandate=' + facet.mandate : '') + '. Find the best-in-class, PRODUCTION-GRADE, FULL-FEATURED ' +
    'MODERN package(s) that fill this gap for a net10/osx-arm64 C# AEC stack and let Fabrication STOP hand-rolling the concern. Compare the real ' +
    'alternatives (do not stop at the first hit) and name the strongest, most complete option — NEVER a naive single-feature wrapper, and only ' +
    'when genuinely stronger than the mature incumbent it would sit beside. Use web research for the landscape, newest stable version, ' +
    'maintenance, license, and Mac/osx-arm64 fit; confirm versions/license from the registry and members via `uv run --frozen python -m ' +
    'tools.assay api` when resolvable.',
  'Then SELF-VALIDATE each candidate against the six-condition gate and set ok=true ONLY when ALL pass: bestOf (strongest, most complete, beats ' +
    'the incumbent), macOk (osx-arm64), newest (current stable + active maintenance), licenseOk (OSS or free-full commercial, NOT GPL/AGPL viral ' +
    'for a host-neutral lib), noLegacy (modern packaging/TFM), notDup (not a duplicate of an admitted package — read the current folder admissions ' +
    '— or a sibling candidate; set dupOf, keep the single best). A package not on NuGet fails the gate as a pin. Default ok=false on any unproven ' +
    'condition. Exclude anything already admitted unless a strictly stronger replacement (set fills to name what it replaces). Return candidate(s) ' +
    'each with the gate fields, what gap it fills, newest version, license, the alternatives compared, and the evidence. Write nothing.',
].join('\n')
const executePrompt = (f, survey, research) => [
  LAW, '',
  'TASK (EXECUTE for folder ' + f.name + ' — ADDITIVE, WRITE the four owned artifacts only): the ' + FACET_COUNT + ' research+self-validation ' +
    'outputs for THIS folder are INLINED at the END of this prompt — consolidate from THAT payload, never run your own package research and never ' +
    'invent candidates beyond it. FIRST consolidate: keep only candidates with ok=true, resolve every remaining dup/overlap across the facets to ' +
    'the single best, drop anything already admitted (read the current folder admissions) and anything out of the Fabrication scope — this is the ' +
    'per-folder plan, done here so nothing is dropped. THEN apply each surviving NEW package NOW. The owning folder is ' + f.name + ' (csproj ' + f.csproj + ', ' +
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

log('=== survey-gaps single-folder (DEEP/ADDITIVE): ' + F.name + ' ===')

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
log('survey-gaps ' + F.name + ' (deep/additive) complete: ' + applied.length + ' packages newly admitted, ' + apiStubs.length + ' .api stubs for ' +
  'the focused rebuild-api run')
return { folder: F.name, applied: applied.length, green: !!(ex && ex.green), packages: applied.map((a) => a.package), apiStubs: apiStubs, skipped: (ex && ex.skipped) || [], summary: (ex && ex.summary) || '' }
