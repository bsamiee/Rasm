export const meta = {
  name: 'survey-gaps',
  description: 'Wide capability-gap survey-AND-APPLY across the C# AEC stack, persistence-weighted: read every folder (Persistence primary + AppHost/Bim/Fabrication/Geometry/Compute/Materials + the central manifest), consolidate the REAL capability and functionality gaps, research best-in-class modern packages to fill them (all first-party PostgreSQL/SQL and DuckDB extensions plus the top 25 of each fully considered, and IFC/scheduling/cost/materials/GIS-geospatial/compute), validate every candidate (osx-arm64, newest version, OSS or free-full commercial, no legacy packaging, no dup/overlap), then APPLY the validated gap-fill — admit the central pins, author the .api catalogs, update the READMEs, thread each new capability into the design pages (signature-locked), and self-heal the build. Disposable, hardcoded scope, no args.',
  whenToUse: 'Find AND admit the world-class packages the planned C# AEC folders are missing, persistence-first, without per-package narrowness.',
  phases: [
    { title: 'Context', detail: 'one agent per folder: domain, admitted packages, hand-rolls, capability gaps' },
    { title: 'Synthesize-Gaps', detail: 'consolidate all folder gaps into prioritized research areas, persistence-weighted, with the mandated DB/IFC/AEC areas guaranteed' },
    { title: 'Research', detail: 'pool of CAP area-chains, each chain serial: research (best-in-class candidate discovery, exhaustive first-party+top-25 sweep for PG/SQL/DuckDB) then its own validate (osx-arm64 + newest + OSS-or-free-commercial + no-legacy + no-dup + best-of, six-condition gate); validate agents group under the Validate sub-heading' },
    { title: 'Plan', detail: 'consolidate the validated set into the final non-overlapping gap-fill change-set' },
    { title: 'Execute', detail: 'serial per recommendation (shared central manifest): admit the pin + csproj reference + author the .api + update README, then place the capability into the design pages at the right granularity (extend a page, add a page, or add a subfolder) and update ARCHITECTURE/README; ripple-remove a superseded package on a replacement' },
    { title: 'Heal', detail: 'per change: assay static/restore gate then self-heal until the new pin resolves on osx-arm64 and the project builds; revert any non-resolvable admission' },
  ],
}

// --- [HARNESS] -- bounded worker pool: steady <=cap concurrent, no burst ----------------
const STAGGER_MS = 1500
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async (slot) => {
    if (slot) await new Promise((res) => setTimeout(res, slot * STAGGER_MS))
    while (next < items.length) { const i = next++; out[i] = await worker(items[i], i) }
  }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, (_, slot) => run(slot)))
  return out
}
const CAP = 12
const STALL = 360000
const EXEC_STALL = 480000
const MAX_HEAL = 2

// --- [SCOPE] -- hardcoded C# AEC stack, Persistence primary; Geometry has a special layout
const PERSISTENCE = { name: 'Persistence', primary: true, doc: 'libs/csharp/Rasm.Persistence', api: 'libs/csharp/Rasm.Persistence/.api', planning: 'libs/csharp/Rasm.Persistence/.planning', csproj: 'libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj', note: '' }
const FOLDERS = [
  PERSISTENCE,
  { name: 'AppHost', doc: 'libs/csharp/Rasm.AppHost', api: 'libs/csharp/Rasm.AppHost/.api', planning: 'libs/csharp/Rasm.AppHost/.planning', csproj: 'libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj', note: '' },
  { name: 'Bim', doc: 'libs/csharp/Rasm.Bim', api: 'libs/csharp/Rasm.Bim/.api', planning: 'libs/csharp/Rasm.Bim/.planning', csproj: 'libs/csharp/Rasm.Bim/Rasm.Bim.csproj', note: '' },
  { name: 'Fabrication', doc: 'libs/csharp/Rasm.Fabrication', api: 'libs/csharp/Rasm.Fabrication/.api', planning: 'libs/csharp/Rasm.Fabrication/.planning', csproj: 'libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj', note: '' },
  { name: 'Geometry', doc: 'libs/csharp/Rasm', api: 'libs/csharp/Rasm/.api', planning: 'libs/csharp/Rasm/Geometry/.planning', csproj: 'libs/csharp/Rasm/Rasm.csproj', note: 'SPECIAL LAYOUT: README/TASKLOG/IDEAS/.api live at the libs/csharp/Rasm ROOT; the geometry design pages live under libs/csharp/Rasm/Geometry/.planning (the siblings Analysis/Domain/Vectors also live under libs/csharp/Rasm). Read AND write the .api at the Rasm root and the design pages under Rasm/Geometry/.planning.' },
  { name: 'Compute', doc: 'libs/csharp/Rasm.Compute', api: 'libs/csharp/Rasm.Compute/.api', planning: 'libs/csharp/Rasm.Compute/.planning', csproj: 'libs/csharp/Rasm.Compute/Rasm.Compute.csproj', note: '' },
  { name: 'Materials', doc: 'libs/csharp/Rasm.Materials', api: 'libs/csharp/Rasm.Materials/.api', planning: 'libs/csharp/Rasm.Materials/.planning', csproj: 'libs/csharp/Rasm.Materials/Rasm.Materials.csproj', note: '' },
]
const HOMING = JSON.stringify(FOLDERS.map((f) => ({ name: f.name, api: f.api, planning: f.planning, csproj: f.csproj, note: f.note })))
const MANIFEST = 'Directory.Packages.props (central NuGet pins) + Directory.Build.props (TargetFramework net10.0, RuntimeIdentifiers osx-arm64)'

// --- [SCHEMAS] ---------------------------------------------------------------------------
const CONTEXT_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'gaps'], properties: { folder: { type: 'string' }, domain: { type: 'string' }, packages: { type: 'array', items: { type: 'string' } }, handRolls: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, evidence: { type: 'string' } } } }, gaps: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['capability'], properties: { capability: { type: 'string' }, severity: { type: 'string' }, note: { type: 'string' } } } } } }
const AREAS_SCHEMA = { type: 'object', additionalProperties: false, required: ['areas'], properties: { areas: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['id', 'domain', 'gap'], properties: { id: { type: 'string' }, domain: { type: 'string' }, gap: { type: 'string' }, mandate: { type: 'string' }, priority: { type: 'string' } } } } } }
const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['area', 'candidates'], properties: { area: { type: 'string' }, candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'fills'], properties: { package: { type: 'string' }, fills: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, maintenance: { type: 'string' }, macCompat: { type: 'string' }, alternativesConsidered: { type: 'string' }, evidence: { type: 'string' } } } } } }
const VALIDATION_SCHEMA = { type: 'object', additionalProperties: false, required: ['area', 'validated'], properties: { area: { type: 'string' }, validated: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'ok'], properties: { package: { type: 'string' }, ok: { type: 'boolean' }, version: { type: 'string' }, license: { type: 'string' }, licenseOk: { type: 'boolean' }, macOk: { type: 'boolean' }, newest: { type: 'boolean' }, noLegacy: { type: 'boolean' }, notDup: { type: 'boolean' }, dupOf: { type: 'string' }, reason: { type: 'string' } } } } } }
const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['recommendations', 'summary'], properties: { recommendations: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'domain', 'gap'], properties: { package: { type: 'string' }, domain: { type: 'string' }, gap: { type: 'string' }, version: { type: 'string' }, license: { type: 'string' }, macOk: { type: 'boolean' }, replaces: { type: 'string' }, evidence: { type: 'string' } } } }, rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'why'], properties: { package: { type: 'string' }, why: { type: 'string' } } } }, summary: { type: 'string' } } }
const EXEC_SCHEMA = { type: 'object', additionalProperties: false, required: ['package', 'verdict'], properties: { package: { type: 'string' }, verdict: { type: 'string', enum: ['applied', 'skipped'] }, files: { type: 'array', items: { type: 'string' } }, projects: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const BUILD_SCHEMA = { type: 'object', additionalProperties: false, required: ['green'], properties: { green: { type: 'boolean' }, diagnostics: { type: 'string' }, projects: { type: 'array', items: { type: 'string' } } } }
const HEAL_SCHEMA = { type: 'object', additionalProperties: false, required: ['fixed'], properties: { fixed: { type: 'boolean' }, reverted: { type: 'boolean' }, summary: { type: 'string' } } }

// --- [LAW] -- the declarative law every agent obeys -------------------------------------
const LAW = [
  'Rasm monorepo, WIDE capability-gap survey-AND-APPLY of the C# AEC stack (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP strata). The mission is NOT a per-package keep/replace audit — it is to find AND admit the world-class MODERN packages these PLANNED folders are MISSING, both functionality gaps in existing concerns and real capability gaps in the planned scope. Persistence is the PRIMARY weight (backends, store types, store extensions, ingress/egress), then Compute/Bim/Materials/Fabrication/Geometry/AppHost. Central pins live in ' + MANIFEST + '.',
  'BAR: every planned folder must be able to build world-class, bleeding-edge, production-ready, professional-standard apps RIGHT NOW — ultra-dense/complex/rich, minimal-to-no hardcoding, maximal parameterization, easily extended/improved, NO fake or illusory capability. Treat Persistence/Materials/Bim as likely SHORT-SIGHTED in scope: assume valuable modern packages are missing and hunt them. Where a folder hand-rolls a concern a real package owns, that is a gap. Do not hold back: 10+ additions is fine IF each is best-in-class and non-overlapping.',
  'AGGRESSIVE ADDITION, ZERO HEDGING (future-looking justification): admit a package whenever it provides unique, modern capability appropriate to the folder domain/scope. DECLINE ONLY for a real reason — (a) it is old / unmaintained / low-quality, (b) a strictly stronger alternative is already admitted, or (c) it is out of the folder scope/domain. NEVER decline because there is no CURRENT consumer: planned consumers are real design pressure and zero current consumers never lowers the capability bar. Once justified, the Execute stage implements it NOW and FULLY with NO hedging — never document a capability as future / planned / deferred / a separate-future-owner; if it is justified it is admitted, .api-documented, README-listed, and integrated into the relevant EXISTING design pages by growing the existing owner IN PLACE (a case / row / field / operation, the rebuild-csharp expand-in-place mentality), not merely mentioned beside it. An ALREADY-ADMITTED package the survey finds missing its README row, its .api, or its real design integration is an INCOMPLETE admission to COMPLETE, never a thing to re-shelve as future.',
  'VALIDATION GATE (every candidate must pass, enforced in the Validate stage BEFORE any write): (1) BEST-OF — the strongest package for the gap was selected, not the first found; compare the real alternatives. (2) MAC — works on osx-arm64 (managed AnyCPU, or a native lib with an osx-arm64 asset, or a Parametric_Forge-provisioned native substrate; reject win-only / x64-only / dead-on-arm). (3) NEWEST — the current stable release is named, never an old or arbitrary version, and the project is actively maintained. (4) LICENSE — OSS (any OSI license) OR a commercial license that is free with full access (no paid tier, seat cap, usage gate, or eval-only); reject fee/tiered. (5) NO LEGACY PACKAGING — modern packaging only (current TFM/abi, SourceLink-era), reject abandoned/legacy-framework-only artifacts. (6) NO DUP/OVERLAP — it must not duplicate an already-admitted package or another recommended one; when two candidates overlap, keep the single best regardless of which is incumbent.',
  'MANDATED DEEP AREAS (must be researched exhaustively, not sampled): PostgreSQL 18.4+ SQL/extension surface — EVERY first-party/contrib extension truthfully considered AND the top 25 community/third-party extensions fully surveyed so no valuable extension is ignored (PostGIS, pgvector/pgvectorscale, TimescaleDB, pg_search/ParadeDB, pg_duckdb, Citus, pg_partman, pg_cron, pgRouting, h3-pg, Apache AGE, and beyond); DuckDB — EVERY first-party extension AND the top 25 community extensions (spatial, httpfs, iceberg, delta, postgres/mysql/sqlite scanners, vss, fts, h3, ducklake, and beyond); plus dedicated areas for IFC (model graph, geometry/tessellation, validation, BCF, IDS), scheduling (CPM/resource-leveling/4D), cost (estimating/QTO/cost models), materials (appearance/spectral/measured-BSDF/datasets), GIS/geospatial (CRS, vector/raster formats, tiles, spatial indices), compute (solvers/optimization/ML/numerics), store/backend types (columnar, time-series, graph, vector, search, object, embedded, lakehouse), and ingress/egress codecs. Rhino/GH2/AEC-adjacent capability is in scope where it is host-neutral.',
  'ASSAY + TFM TRUTH: verify a candidate that is in the cache via `uv run --frozen python -m tools.assay api`; for a multi-target NuGet package the consumer floor is net10.0 — confirm the lib/<tfm> a net10 consumer actually binds rather than trusting assay default resolution (a non-bound TFM can expose a different surface; set DOTNET_ROOT=$(dirname "$(readlink -f "$(command -v dotnet)")") and ilspycmd the bound TFM for member-exact claims). Confirm versions/license/Mac/packaging against the registry (NuGet flat-container index + nuspec) truthfully; web research for the domain landscape and maintenance signals.',
  'WRITE DISCIPLINE: the Context/Research/Validate/Plan stages are READ-ONLY analysis and write NOTHING. ONLY the Execute stage writes, and it WRITES-FULLY the validated gap-fill: (a) admit the central pin in Directory.Packages.props (the matching ItemGroup, newest version, plus any pure-managed transitive floor pins the new package needs); (b) add the PackageReference to the OWNING folder csproj; (c) author the .api catalog under that folder .api path to house format with consumer-TFM-correct REAL members; (d) update that folder README central-manifest section + prose; (e) PLACE the new capability into the design pages under that folder planning path at the RIGHT granularity — extend an EXISTING design page when it belongs to an existing concern, author a NEW page within an existing subfolder when it warrants its own owner inside an existing concern group, or create a NEW subfolder+page when it is a genuinely new concern that justifies one — always SIGNATURE-LOCKED (the capability is available and composed by the design; NEVER realize .cs source, these are design-page folders). Whenever a page or subfolder is created or a seam shifts, UPDATE the folder ARCHITECTURE.md seam/structure map and README so the structure stays truthful. On a REPLACEMENT, also ripple-REMOVE the superseded package repo-wide (manifest row, .api, README, every reference) and rewire the design to the new owner. A gap-fill addition usually has NO existing call site — that is expected; admit + document + integrate is the write, not a code refactor.',
].join('\n')

// --- [PROMPTS] ---------------------------------------------------------------------------
const contextPrompt = (f) => [
  LAW, '',
  'TASK (READ-ONLY CONTEXT for folder ' + f.name + (f.primary ? ' — THE PRIMARY FOCUS, go deepest' : '') + '): build the full picture of this folder so the survey can find its gaps. Read its README + project file' + (f.csproj ? ' (' + f.csproj + ')' : '') + ', every catalog under ' + f.api + '/, the design pages under ' + f.planning + '/, and the central manifest ' + MANIFEST + ' for this folder rows.' + (f.note ? ' ' + f.note : ''),
  'Return: (1) a 1-2 sentence DOMAIN summary; (2) the admitted PACKAGES this folder uses; (3) HAND-ROLLS — capabilities the folder implements by hand that a real ecosystem package likely owns (each with an evidence pointer); (4) GAPS — the capabilities a WORLD-CLASS, production-ready version of this domain must have but the folder lacks or under-serves, judged against the bleeding-edge state of the art (for Persistence: backends, store types, store extensions, PG/DuckDB extension surface, ingress/egress, CDC, search, vector, time-series, graph, lakehouse; for Bim: IFC depth, scheduling, cost, validation, GIS; for Materials: appearance/spectral/measured datasets; etc.). Be skeptical and ambitious — assume the folder is short-sighted. Write nothing.',
].join('\n')

const synthPrompt = (ctx) => [
  LAW, '',
  'TASK (SYNTHESIZE GAPS into research areas — the barrier): you hold the context for all ' + FOLDERS.length + ' folders. Consolidate every hand-roll and gap into a deduped, prioritized list of RESEARCH AREAS, persistence-weighted. Each area is one focused capability gap to research (id, domain, the gap, an optional mandate note for the deep ones, priority). You MUST include — in addition to the folder-derived gaps — the MANDATED DEEP AREAS as explicit areas: PostgreSQL first-party+top-25 extensions, DuckDB first-party+top-25 extensions, IFC (model/geometry/validation/BCF/IDS), scheduling, cost, materials appearance/spectral/datasets, GIS/geospatial, compute solvers/optimization/ML, store/backend types, ingress/egress codecs, and any AEC/Rhino/GH2-adjacent host-neutral capability the context surfaced. Emit a bounded, non-redundant area list (merge duplicates across folders). Write nothing.',
  'CONTEXT:\n' + JSON.stringify(ctx, null, 1),
].join('\n')

const researchPrompt = (a) => [
  LAW, '',
  'TASK (RESEARCH one gap area — READ-ONLY): area=' + a.id + ' · domain=' + a.domain + ' · gap=' + a.gap + (a.mandate ? ' · mandate=' + a.mandate : '') + '. Find the best-in-class MODERN package(s) that fill this gap for a net10/osx-arm64 C# AEC stack. Compare the real alternatives (do not stop at the first hit) and name the strongest. For a PostgreSQL or DuckDB extension area, survey the FULL first-party/contrib extension set AND the top 25 community extensions and return every valuable one (not a sample). Use web research for the landscape, newest stable version, maintenance, license, and Mac/osx-arm64 fit; confirm versions/license from the registry where possible. Return candidate(s) each with: package, what gap it fills, newest version, license, maintenance signal, mac compatibility, the alternatives you compared, and the evidence. Exclude anything already admitted in the central manifest unless it is a strictly stronger replacement. Write nothing.',
].join('\n')

const validatePrompt = (a, res) => [
  LAW, '',
  'TASK (VALIDATE the candidates for area ' + a.id + ' — the mandated gate, READ-ONLY): for EACH candidate prove the six gate conditions and set ok=true ONLY if all pass: (1) best-of (the strongest for the gap), (2) macOk (osx-arm64 — managed AnyCPU, an osx-arm64 native asset, or a Forge-provisioned substrate; reject win/x64-only), (3) newest (name the current stable release; reject old/arbitrary; confirm active maintenance), (4) licenseOk (OSS or free-full commercial; reject fee/tiered/eval-only), (5) noLegacy (modern packaging/TFM/abi; reject abandoned/legacy-only), (6) notDup (does not duplicate an admitted package or a sibling candidate — set dupOf when it does, and keep only the single best of an overlapping pair). Verify versions/license/Mac/packaging truthfully against the registry (NuGet flat-container + nuspec) and members via `uv run --frozen python -m tools.assay api` consumer-TFM-correct when the package is resolvable. Default ok=false on any unproven condition. Candidates:\n' + JSON.stringify(res, null, 1),
].join('\n')

const planPrompt = (validated) => [
  LAW, '',
  'TASK (PLAN — consolidate the final gap-fill change-set, the barrier): you hold every area research + validation. Emit the final non-overlapping set of packages to ADD: keep only candidates that PASSED the validation gate (ok=true), resolve every remaining dup/overlap to the single best, and organize by domain (persistence first). Each recommendation carries: package, domain (the owning folder concern), the gap it fills, newest version, license, macOk, what it replaces (a hand-roll or a weaker admitted package, or empty for a pure new capability), and the decisive evidence. List the rejected candidates with the gate condition each failed. Be ambitious but honest — every entry must be best-in-class, Mac-fit, newest, license-clean, modern-packaged, and non-duplicative. The summary states the count by domain and the highest-value additions. Write nothing — the Execute stage applies this set.',
  'VALIDATED AREAS:\n' + JSON.stringify(validated, null, 1),
].join('\n')

const executePrompt = (r) => [
  LAW, '',
  'TASK (EXECUTE one validated gap-fill — WRITE-FULLY): apply this recommendation NOW across the repo: ' + JSON.stringify(r) + '. Determine the OWNING folder from its domain using this homing map (Geometry has the special layout — admit its .api at the Rasm root and design pages under Rasm/Geometry/.planning): ' + HOMING + '. Steps: (a) add the central pin to Directory.Packages.props in the matching ItemGroup at the newest stable version (' + (r.version || 'confirm newest') + '), with a one-line comment, plus any pure-managed transitive floor pins it needs; (b) add the PackageReference to the owning folder csproj; (c) author the .api catalog under the owning folder .api path to house format (header package/version/license/build-floor, member sections with backticked REAL members verified consumer-TFM-correct via assay/ilspycmd at the net10-bound TFM, a consumer/boundary note; NO provenance/freshness tails); (d) update the owning folder README central-manifest section + prose; (e) PLACE the new capability into the design pages under the owning folder planning path at the right granularity — extend an EXISTING design page if it belongs to an existing concern, author a NEW page within an existing subfolder if it warrants its own owner, or create a NEW subfolder+page if it is a genuinely new concern that justifies one — always SIGNATURE-LOCKED (composed by the design, NEVER realize .cs source); and whenever you create a page/subfolder or shift a seam, UPDATE that folder ARCHITECTURE.md and README so the structure stays truthful. If r.replaces names a hand-roll or a weaker admitted package, ALSO ripple-remove the superseded package repo-wide (manifest row, .api file, README mention, every code/design reference) and rewire the design to the new owner. Re-confirm the full six-condition gate one last time — BEST-OF + LICENSE + MAC(osx-arm64) + NEWEST + NO-LEGACY + NO-DUP; if it now fails, return verdict=skipped with the reason rather than a partial write. Restore the owning project so the pin resolves and lands in the lockfile. Return the fix-log (verdict, the files you edited, the projects affected, a one-line summary).',
].join('\n')

const verifyPrompt = (r, ex, attempt) => [
  LAW, '',
  'TASK (BUILD/STATIC GATE — verify one applied gap-fill): the addition of ' + r.package + ' touched these projects: ' + JSON.stringify((ex && ex.projects) || []) + '. Run the assay static/restore gate over them via `uv run --frozen python -m tools.assay static --project <csproj>` (parse the one JSON Envelope on stdout), green=true ONLY if the new pin restores cleanly on osx-arm64 AND the owning project builds with no FAILED diagnostic. On red, return green=false with the concrete diagnostics (NU* restore error, RID mismatch, build error). Attempt ' + attempt + '.',
].join('\n')

const healPrompt = (r, ex, v) => [
  LAW, '',
  'TASK (SELF-HEAL — the last gap-fill left the build red, WRITE-FULLY): ' + r.package + ' broke the gate. Diagnostics: ' + ((v && v.diagnostics) || 'see prior gate') + '. Files touched: ' + JSON.stringify((ex && ex.files) || []) + '. Fix the real defect in place — a wrong ItemGroup, a missing pure-managed transitive floor pin, a version that does not exist, a stale .api/README reference. If the package proves genuinely non-resolvable or RID-incompatible on osx-arm64 (no osx-arm64 asset, win/x64-only native), REVERT its admission entirely (the manifest row + csproj reference + .api file + README mention + design note) and report fixed=true AND reverted=true with the revert reason — a non-Mac package must not be admitted. Set reverted=true ONLY when you removed the admission; for an in-place repair leave reverted false. Return fixed=true with a one-line summary.',
].join('\n')

const finalVerifyPrompt = (applied) => [
  LAW, '',
  'TASK (FINAL WHOLE-STACK GATE): every gap-fill is applied. Applied summary: ' + JSON.stringify(applied) + '. Run the assay static/restore gate over the union of affected projects via `uv run --frozen python -m tools.assay static ...`, parse the JSON Envelope, green=true ONLY if the whole affected set restores on osx-arm64 and builds clean. On red, return the residual diagnostics.',
].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Context')
const ctx = (await pool(FOLDERS, CAP, (f) => agent(contextPrompt(f), { label: 'ctx:' + f.name, phase: 'Context', schema: CONTEXT_SCHEMA, effort: 'high', stallMs: STALL }))).filter(Boolean)
log('context: mapped ' + ctx.length + '/' + FOLDERS.length + ' folders')

phase('Synthesize-Gaps')
const areasOut = await agent(synthPrompt(ctx), { label: 'synth-gaps', phase: 'Synthesize-Gaps', schema: AREAS_SCHEMA, effort: 'max', stallMs: STALL })
const areas = ((areasOut && areasOut.areas) || []).filter((a) => a && a.id)
log('synthesize: ' + areas.length + ' research areas (persistence-weighted, mandated areas guaranteed)')
if (!areas.length) return { scope: 'csharp-aec-stack', folders: ctx.length, areas: 0, note: 'no gap areas synthesized' }

// research -> validate per area, CAP=12 concurrent area-chains (each chain serial: research then its validation)
phase('Research')
const reviewed = (await pool(areas, CAP, async (a) => {
  const res = await agent(researchPrompt(a), { label: 'research:' + a.id, phase: 'Research', schema: RESEARCH_SCHEMA, effort: 'high', stallMs: STALL })
  if (!res) return null
  const v = await agent(validatePrompt(a, res), { label: 'validate:' + a.id, phase: 'Validate', schema: VALIDATION_SCHEMA, effort: 'xhigh', stallMs: STALL })
  return { area: a, research: res, validation: v }
})).filter(Boolean)
log('research+validate: ' + reviewed.length + '/' + areas.length + ' areas surveyed and gated')

phase('Plan')
const plan = await agent(planPrompt(reviewed), { label: 'plan', phase: 'Plan', schema: PLAN_SCHEMA, effort: 'max', stallMs: STALL })
const recs = ((plan && plan.recommendations) || []).filter((r) => r && r.package)
const rejected = (plan && plan.rejected) || []
log('plan: ' + recs.length + ' validated gap-fill additions, ' + rejected.length + ' rejected at the gate')
if (!recs.length) return { scope: 'csharp-aec-stack', folders: ctx.length, areas: areas.length, applied: 0, rejected: rejected.length, note: 'no addition survived the validation gate' }

// --- [EXECUTE] -- SERIAL: shared Directory.Packages.props + cross-file design integration must not race
phase('Execute')
const applied = []
for (let i = 0; i < recs.length; i++) {
  const r = recs[i]
  const ex = await agent(executePrompt(r), { label: 'exec:' + r.package, phase: 'Execute', schema: EXEC_SCHEMA, effort: 'max', stallMs: EXEC_STALL })
  if (!ex || ex.verdict !== 'applied') { applied.push({ package: r.package, applied: false, reason: (ex && ex.summary) || 'skipped' }); continue }
  let green = false
  let reverted = false
  for (let h = 0; h <= MAX_HEAL; h++) {
    const v = await agent(verifyPrompt(r, ex, h), { label: 'verify:' + r.package + ':' + h, phase: 'Heal', schema: BUILD_SCHEMA, effort: 'low', stallMs: EXEC_STALL })
    if (v && v.green) { green = true; break }
    if (h === MAX_HEAL) break
    const hv = await agent(healPrompt(r, ex, v), { label: 'heal:' + r.package + ':' + h, phase: 'Heal', schema: HEAL_SCHEMA, effort: 'max', stallMs: EXEC_STALL })
    if (hv && hv.reverted) { reverted = true; break }
  }
  if (reverted) applied.push({ package: r.package, applied: false, reverted: true, reason: 'reverted: non-resolvable or RID-incompatible on osx-arm64', projects: ex.projects || [] })
  else applied.push({ package: r.package, applied: true, green, files: ex.files || [], projects: ex.projects || [] })
}

phase('Heal')
const finalV = await agent(finalVerifyPrompt(applied), { label: 'verify:final', phase: 'Heal', schema: BUILD_SCHEMA, effort: 'low', stallMs: EXEC_STALL })
const appliedCount = applied.filter((a) => a.applied).length
const revertedCount = applied.filter((a) => a.reverted).length
log('execute: ' + appliedCount + '/' + recs.length + ' gap-fills applied, ' + revertedCount + ' reverted (non-Mac/non-resolvable); final gate green=' + (finalV && finalV.green))

return { scope: 'csharp-aec-stack', folders: ctx.length, areas: areas.length, surveyed: reviewed.length, candidates: recs.length, applied: appliedCount, reverted: revertedCount, finalGreen: finalV && finalV.green, rejected: rejected.length, additions: applied, summary: (plan && plan.summary) || '', residualDiagnostics: (finalV && !finalV.green) ? finalV.diagnostics : '' }
