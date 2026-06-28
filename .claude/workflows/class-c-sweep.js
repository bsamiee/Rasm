export const meta = {
  name: 'class-c-sweep',
  description: 'Exhaustive Class-C member-coverage sweep over every non-artifacts .api catalog in libs/ (8 C# folders + 5 Python tiers; artifacts EXCLUDED, owned by another agent). For each .api folder, fan out 3-4 agents (one per chunk of that folder catalogs), each verifying that EVERY external member a design-page fence in that folder calls is present in the catalog body, and ADDING any missing member in place. Seeds the 18 already-known Class-C gaps so they are fixed for certain, then finds any others the earlier non-exhaustive scans missed. Members are verified via assay api resolve as the primary local truth; for companion-gated / uninstalled Python packages (scipy cluster, JAX, OCP, usd, vtk) assay returns empty, so members are cross-verified via Context7 + GitHub + Exa + package source and tagged source-verified. Every claim double-verified across at least two sources. Then a union-find cross-catalog reconcile. CAP=8.',
  whenToUse: 'After the .api fill run and the install-fix, to catch any Class-C member-coverage oversight across the whole non-artifacts catalog set before the harden re-run.',
  phases: [
    { title: 'Discover', detail: 'one agent: list every catalog under each of the 13 non-artifacts .api folders' },
    { title: 'Sweep', detail: 'per folder-chunk (3-4 per folder): verify+fix design-page member coverage in place, assay + multi-source double-verified, pooled at CAP=8' },
    { title: 'Reconcile', detail: 'union-find cluster cross-catalog residuals -> fix -> adversarial completeness verify' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 8
const CHUNKS = 4 // up to this many sweep agents per .api folder
const STAGGER_MS = 1500
const FOLDERS = [
  { name: 'cs-AppHost', api: 'libs/csharp/Rasm.AppHost/.api', planning: ['libs/csharp/Rasm.AppHost/.planning'], lang: 'cs' },
  { name: 'cs-AppUi', api: 'libs/csharp/Rasm.AppUi/.api', planning: ['libs/csharp/Rasm.AppUi/.planning'], lang: 'cs' },
  { name: 'cs-Bim', api: 'libs/csharp/Rasm.Bim/.api', planning: ['libs/csharp/Rasm.Bim/.planning'], lang: 'cs' },
  { name: 'cs-Compute', api: 'libs/csharp/Rasm.Compute/.api', planning: ['libs/csharp/Rasm.Compute/.planning'], lang: 'cs' },
  { name: 'cs-Fabrication', api: 'libs/csharp/Rasm.Fabrication/.api', planning: ['libs/csharp/Rasm.Fabrication/.planning'], lang: 'cs' },
  { name: 'cs-Materials', api: 'libs/csharp/Rasm.Materials/.api', planning: ['libs/csharp/Rasm.Materials/.planning'], lang: 'cs' },
  { name: 'cs-Persistence', api: 'libs/csharp/Rasm.Persistence/.api', planning: ['libs/csharp/Rasm.Persistence/.planning'], lang: 'cs' },
  { name: 'cs-Geometry', api: 'libs/csharp/Rasm/.api', planning: ['libs/csharp/Rasm/Geometry/.planning'], lang: 'cs' },
  { name: 'py-shared', api: 'libs/python/.api', planning: ['libs/python/compute/.planning', 'libs/python/data/.planning', 'libs/python/geometry/.planning', 'libs/python/runtime/.planning'], lang: 'py', shared: true },
  { name: 'py-compute', api: 'libs/python/compute/.api', planning: ['libs/python/compute/.planning'], lang: 'py' },
  { name: 'py-data', api: 'libs/python/data/.api', planning: ['libs/python/data/.planning'], lang: 'py' },
  { name: 'py-geometry', api: 'libs/python/geometry/.api', planning: ['libs/python/geometry/.planning'], lang: 'py' },
  { name: 'py-runtime', api: 'libs/python/runtime/.api', planning: ['libs/python/runtime/.planning'], lang: 'py' },
]

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'catalogs'], properties: { name: { type: 'string' }, catalogs: { type: 'array', items: { type: 'string' } } } } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['fixed', 'clean'] }, added: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'members'], properties: { catalog: { type: 'string' }, members: { type: 'array', items: { type: 'string' } }, source: { type: 'string', enum: ['assay', 'source-verified'] } } } }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. .api catalogs are agent-facing declarative records of a package useful surface that DESIGN PAGES compose against. A CLASS-C ' +
    'gap is a member a design-page code fence CALLS that is ABSENT from the catalog body (the package is admitted + already catalogued + visible to ' +
    'the folder; only the specific member is missing). House .api format: header, then member sections grouped by concern, backticked symbols + ' +
    'signatures + a consumer/boundary note. NO provenance/process narration, NO freshness tails, NO gate/wheel/floor rationale. Preserve the ' +
    'existing catalog structure; ADD the missing members into the right concern section, never shrink or reflow real content.',
  'VERIFY EVERY MEMBER — DOUBLE-SOURCED, NEVER FROM MEMORY: the primary local truth is `uv run python -m tools.assay api resolve/query` (reflects ' +
    'the installed C# NuGet / host DLL / Python distribution). READ tools/assay/README.md [API_COMMANDS] for the exact invocation. Cross-check every ' +
    'non-trivial member against a SECOND authoritative source: Context7 (resolve-library-id -> query-docs) for library/API docs; the nuget MCP for ' +
    'C# package facts (version/context/members); GitHub (mcp__github__search_code) for real call-site usage; Exa/Tavily for the package source/docs. ' +
    'A member you cannot confirm in at least two sources is a PHANTOM — do NOT add it.',
  'COMPANION-GATED PYTHON: several Python packages remain gated `python_version<3.15` and are NOT installed in the cp315 venv (scipy and its ' +
    'cluster, the JAX arm, cadquery-ocp/OCP, usd-core, vtk/pyvista, numba/llvmlite, tensorstore, kiss-matcher, python-flint, pdal, pye57, cvxpy, ' +
    'manifold3d, scikit-learn/scikit-image/pywavelets). For these `assay api resolve` returns EMPTY. Verify their members against the AUTHORITATIVE ' +
    'package SOURCE at the released version via Context7 + GitHub + Exa (at least two), and tag the addition `source-verified`. Do NOT skip a real ' +
    'member just because assay is empty; do NOT invent one because you cannot reflect it.',
  'WRITE-FULLY MANDATE: every missing member you confirm you MUST add NOW via Edit directly in the catalog — the structured fix-log REPORTS edits ' +
    'ALREADY MADE, never a to-do list. If a catalog already covers every member its folder design pages call, return verdict=clean for it — never ' +
    'invent edits. A cross-CATALOG dependency you cannot fix from your chunk goes in `residual` as a {files, claim} object.',
].join('\n')
const KNOWN = 'KNOWN Class-C gaps already identified (FIX the ones whose catalog falls in YOUR chunk; they are real — verify the exact member ' +
  'signature then add; also FIND any others). PY: opentelemetry-api += W3CBaggagePropagator/CompositePropagator/TraceContextTextMapPropagator; ' +
  'zarr += ArrayArrayCodec/ArrayBytesCodec/BytesBytesCodec (zarr.abc.codec); xarray += TimeResampler (xarray.groupers); igraph += Graph.add_vertices; ' +
  'topologicpy += Topology.Decompose/Topology.Analyze; cadquery-ocp += OCP.TopLoc.TopLoc_Location; ifcopenshell += geom.iterator ' +
  'initialize/get/next + geom.serializers.gltf writeHeader/finalize + shape.geometry.faces + file.schema_identifier; sympy += srepr. CS: ' +
  'api-mathnet-providers += DenseMatrix.OfArray/Create/CreateDiagonal + DenseVector.OfArray/Create; api-vividorange-sections += concrete ctors ' +
  'LocalPoint2d(Length,Length)/LocalPolyline2d(IEnumerable<ILocalPoint2d>); api-mathnet-symbolics += FSharp.Core OptionModule.IsSome/ListModule.OfSeq/' +
  'FSharpOption.Some/.None; api-cityjson += JsonConvert.DeserializeObject<CityJsonDocument> read row; api-polly-ratelimiting += SlidingWindowRateLimiter/' +
  'TokenBucketRateLimiter (+options)/QueueProcessingOrder.OldestFirst/RateLimiter.AcquireAsync; api-grpc-aspnetcore += HealthServiceImpl.SetStatus + ' +
  'Grpc.Health.V1 ServingStatus; api-npgsql-ef / api-ef-sqlite += EF-core interceptor family (IDbCommandInterceptor/ISaveChangesInterceptor) + ' +
  'IExecutionStrategy.ExecuteAsync + PooledDbContextFactory.CreateDbContextAsync + EntityFrameworkQueryableExtensions + ModelConfigurationBuilder + ' +
  'IMigrator.GenerateScript + savepoints + HasComputedColumnSql/HasCheckConstraint; api-npgsql += ReplicationTuple/TupleDataKind/' +
  'ReplicationSystemIdentification.XLogPos/TimelineHistoryFile; api-arrow += FlightServer/DoGet/GetSchema/FlightServerRecordBatchStreamWriter+Reader; ' +
  'api-nodatime += OffsetPattern.GeneralInvariantWithZ/AnnualDatePattern.Iso/YearMonthPattern.Iso; api-themis-las += MathNet Vector<double> seam note; ' +
  'api-ids-lib += NullLogger.Instance. Also verify geometry [ENERGY] band reference honeybee-energy-standards: is it a real distinct admitted package ' +
  'needing a catalog, or a band-naming slip vs honeybee-standards — report as residual if a genuine missing admission.'

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
const splitN = (arr, n) => { const k = Math.min(n, arr.length); if (k <= 0) return []; const out = Array.from({ length: k }, () => []); arr.forEach((x, i) => out[i % k].push(x)); return out }
const sweepPrompt = (u) => [LAW, '', KNOWN, '',
  'TASK: CLASS-C MEMBER-COVERAGE SWEEP of folder `' + u.folder.name + '` (chunk ' + (u.idx + 1) + '/' + u.nchunks + '). ' +
    (u.folder.lang === 'cs'
      ? 'C# has NO shared .api tier; the catalogs below live in `' + u.folder.api + '` and are the ONLY tier visible to this folder.'
      : 'Python is two-tier: this folder reads the SHARED universal tier `libs/python/.api/*.md` PLUS its own `' + u.folder.api + '`. ' +
        (u.folder.shared ? 'THIS chunk IS the shared tier — verify its catalogs against EVERY listed Python design-page root.' : 'Do NOT duplicate a member already documented in the shared tier.')),
  'DESIGN PAGES (the reference for what members are actually called): all fences under ' + u.folder.planning.map((p) => '`' + p + '/**/*.md`').join(' + ') + ' (EXCLUDE README/IDEAS/TASKLOG/ARCHITECTURE).',
  'YOUR CATALOGS (verify+fix member coverage of EXACTLY these; do not touch catalogs outside this list):\n' + u.catalogs.map((c) => '- ' + c).join('\n'),
  'METHOD: (1) read the design-page fences once and collect every external member they CALL that is attributable to one of YOUR catalogs packages; ' +
    '(2) for each of YOUR catalogs, diff the called members against the catalog body; (3) for each ABSENT member, VERIFY it exists + its exact ' +
    'signature (assay api resolve primary; Context7/nuget/GitHub/Exa cross-check; companion-gated -> source-verified via two sources) and ADD it into ' +
    'the right concern section in place; (4) skip members already covered, stdlib, and (for py folder tiers) members the SHARED tier already owns. ' +
    'Return the fix-log: per catalog you edited, {catalog, members:[added], source}; cross-catalog items you cannot fix from this chunk as residual; ' +
    'verdict=fixed if you added anything, else clean.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Discover')
const inv = await agent('For EACH of these .api folders, list every catalog markdown file directly inside it (maxdepth 1). Run find from the repo ' +
  'root, do NOT cd. Folders (name -> dir):\n' + FOLDERS.map((f) => f.name + ' -> ' + f.api).join('\n') + '\nFor each, run `find <dir> -maxdepth 1 ' +
  '-name "*.md"` and return {name, catalogs:[repo-relative paths]}. Return every folder even if empty.', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: 180000 })
const byName = new Map(((inv && inv.folders) || []).map((f) => [f.name, (f.catalogs || []).filter(Boolean)]))
const units = []
for (const f of FOLDERS) { const cats = byName.get(f.name) || []; splitN(cats, CHUNKS).forEach((c, i, a) => units.push({ folder: f, catalogs: c, idx: i, nchunks: a.length })) }
const totalCats = [...byName.values()].reduce((n, c) => n + c.length, 0)
log('class-c-sweep: ' + totalCats + ' catalogs across ' + FOLDERS.length + ' folders -> ' + units.length + ' sweep units; pooling at CAP=' + CAP)
if (!units.length) return { folders: FOLDERS.length, catalogs: 0, note: 'no catalogs discovered' }

phase('Sweep')
const done = (await pool(units, CAP, (u) => agent(sweepPrompt(u), { label: 'sweep:' + u.folder.name + ':' + (u.idx + 1), phase: 'Sweep', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 360000 }).then((r) => r ? { u, log: r } : null))).filter(Boolean)
const fixedUnits = done.filter((d) => d.log && d.log.verdict === 'fixed').length
const addedTotal = done.reduce((n, d) => n + ((d.log && d.log.added) || []).reduce((m, a) => m + ((a.members || []).length), 0), 0)

const allRes = []
for (const d of done) if (d.log && d.log.residual) for (const x of d.log.residual) allRes.push({ files: x.files && x.files.length ? x.files : d.u.catalogs, claim: x.claim })
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('class-c-sweep: ' + done.length + '/' + units.length + ' units; ' + fixedUnits + ' fixed, ' + addedTotal + ' members added; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters')

let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pipeline(
    clusters,
    (cl) => agent([LAW, '', 'TASK: RECONCILE these cross-CATALOG Class-C residuals the per-chunk pass deferred. Address EVERY residual: read each ' +
      'listed catalog, add the depended-on member (verified via assay api resolve + a second source per the LAW), align across catalogs, never shrink ' +
      'real content. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named catalogs from disk and classify each: ' +
      '"fixed" (real member gap, now resolved + verified), "invalid" (the claim is wrong — the member is already present or is a phantom; cite why), ' +
      'or "open" (real gap still NOT resolved). Default "open" on doubt. Claims:\n' + JSON.stringify(cl, null, 1) + '\nCatalogs touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_residual = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('class-c-sweep reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open, ' + dropped.length + ' dropped as invalid')
return { folders: FOLDERS.length, catalogs: totalCats, units: units.length, fixedUnits, membersAdded: addedTotal, clusters: clusters.length, hard_residual, dropped }
