export const meta = {
  name: 'scout',
  description: 'Read-only topology/card/ripple MAPPER for a planning scope, with two optional enrichments the implement pass turns on: ripple CLOSURE (walk the bidirectional ripple graph to fixpoint, mapping each out-of-scope folder a ripple reaches and retaining ONLY the ripple-counterpart cards — never that folder\'s other cards) and a serialized live PROBE (one agent per tool-gated [BLOCKED] card, resolving it via assay api / Forge band / Rhino WIP before planning so blocked cards never poison the plan). One sonnet per planning sub-domain maps the file/page/.api surface AND the open IDEAS/TASKLOG cards touching it plus their cross-folder Ripple links. args = a target path string (pure read-only map, no closure/probe) or { scope, probe, closure } ; empty/"ALL" = all of libs.',
  phases: [
    { title: 'Map', detail: '1 sonnet per <pkg>/.planning/<sub-domain>/: files, design pages, in-scope .api catalogs, and the open cards + ripple links touching the sub-domain' },
    { title: 'Closure', detail: 'optional: walk the bidirectional ripple graph to fixpoint, mapping each reached out-of-scope folder and retaining ONLY its ripple-counterpart cards (never its other open cards)' },
    { title: 'Probe', detail: 'optional + SERIAL (host/tools are singletons): one live probe agent per tool-gated [BLOCKED] card resolves it via assay api / Forge band / Rhino WIP, attaching the verified fact' },
  ],
}

// --- [TYPES] -- structured-output schemas -------------------------------------------------
const DIRS_SCHEMA = { type: 'object', additionalProperties: false, required: ['subdomains'], properties: { subdomains: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'lang', 'subdomain', 'path'], properties: { folder: { type: 'string' }, lang: { type: 'string', enum: ['csharp', 'python', 'typescript'] }, subdomain: { type: 'string' }, path: { type: 'string' } } } } } }
const CARD = { type: 'object', additionalProperties: false, required: ['slug', 'kind', 'status'], properties: { slug: { type: 'string' }, kind: { type: 'string', enum: ['idea', 'task'] }, status: { type: 'string', enum: ['ACTIVE', 'QUEUED', 'BLOCKED'] }, atomic: { type: 'boolean' }, thesis: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } }, ripple: { type: 'array', items: { type: 'string' } }, blocker: { type: 'string' }, blockerKind: { type: 'string', enum: ['catalog', 'tooling', 'task'] }, fact: { type: 'string' } } }
const SUBMAP = { type: 'object', additionalProperties: false, required: ['folder', 'lang', 'subdomain', 'pages', 'cards'], properties: { folder: { type: 'string' }, lang: { type: 'string' }, subdomain: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, pages: { type: 'array', items: { type: 'string' } }, apis: { type: 'array', items: { type: 'string' } }, cards: { type: 'array', items: CARD } } }
const PROBE = { type: 'object', additionalProperties: false, required: ['slug', 'resolution'], properties: { slug: { type: 'string' }, resolution: { type: 'string', enum: ['unblocked', 'legitimate'] }, fact: { type: 'string' }, reason: { type: 'string' } } }

// --- [HARNESS] -- bounded worker pool: steady <=cap concurrent, staggered start ------------
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
const PROBE_CAP = 1 // the live Rhino host + assay bridge are per-machine singletons, so the probe MUST run serial

// --- [INPUT] -- args = a target path | string[] | { scope, probe?, closure? } ; empty/"ALL" = all of libs ---
const A = (args && typeof args === 'object' && !Array.isArray(args)) ? args : {}
const rawScope = Array.isArray(args) ? args : (typeof args === 'string') ? args : (A.scope !== undefined ? A.scope : 'libs')
const effectiveScopes = ((Array.isArray(rawScope) ? rawScope : [rawScope]).map((s) => String(s).trim()).filter((s) => s && s !== 'ALL')) || []
if (!effectiveScopes.length) effectiveScopes.push('libs')
const SCOPE = effectiveScopes.join('+') // a single unified pass over ALL target scopes: one shared map+closure+probe so no folder is mapped twice and no target folder is mis-pulled
const PROBE_ON = !!A.probe
const CLOSURE_ON = !!A.closure

// --- [CONSTANTS] -- (lang, pkg) -> realizable folder resolver; Geometry exception folds in --
const LANG_PKGS = { python: ['artifacts', 'compute', 'data', 'geometry', 'runtime'], typescript: ['interchange', 'platform', 'projection', 'services', 'ui'], csharp: ['Rasm.AppHost', 'Rasm.AppUi', 'Rasm.Bim', 'Rasm.Compute', 'Rasm.Fabrication', 'Rasm.Materials', 'Rasm.Persistence'] }
const cardKey = (folder, slug) => folder + '|' + slug
// a ripple token resolves to the folder whose .planning sub-domains a realize pass deepens, or null for a language-level/central/unknown target (nothing to realize)
const resolveRipple = (srcLang, raw) => {
  const t = (raw || '').trim()
  let lang = srcLang, pkg = t, explicit = false
  if (t.includes(':')) { const [l, p] = t.split(':'); lang = l.trim(); pkg = p.trim(); explicit = true }
  if (!pkg || /^libs(\/|$)/.test(pkg)) return null // language-level (`libs/python`) or central (`libs`): no design pages to realize
  if (pkg === 'Geometry') return { folder: 'libs/csharp/Rasm/Geometry', lang: 'csharp' }
  if (explicit && LANG_PKGS[lang] && LANG_PKGS[lang].includes(pkg)) return { folder: 'libs/' + lang + '/' + pkg, lang }
  for (const [L, pkgs] of Object.entries(LANG_PKGS)) if (pkgs.includes(pkg)) return { folder: 'libs/' + L + '/' + pkg, lang: L }
  return null // unknown pkg: report, never guess
}
// a ripple string is `<lang>:<pkg> [SLUG]` / `<pkg> [SLUG]` / `libs/<lang> [SLUG]`; take the first backticked token as the pkg and the [SLUG]
const parseRipple = (s) => { const slug = ((s || '').match(/\[([^\]]+)\]/) || [])[1]; const tok = ((s || '').match(/`([^`]+)`/) || [])[1] || (s || '').trim().split(/\s+/)[0]; return { tok: (tok || '').trim(), slug: (slug || '').trim() || null } }

const LAW = [
  'Rasm monorepo, planning-stage. CLAUDE.md governs; `libs/.planning/architecture.md` and `planning-targets.md` own the topology, and the card grammar below is the convention you map against.',
  'YOUR JOB is exactly one thing: produce a FAITHFUL MANIFEST of the OPEN planning cards in the target scope and the surface each one touches. For EVERY open card — IDEA or TASK — return its target design pages (the `.md` fences a realize pass would deepen) and its Ripple links (the related cards in other folders). You READ and STRUCTURE what is on disk; you never edit, plan, decide, or grade. Accuracy IS the deliverable: a missed card, a missed ripple, or a wrong target page silently breaks the realize pass downstream, so list every open card, capture every Ripple line verbatim, and attach every page each card touches.',
  'CARD GRAMMAR: a card heads with `[ID]-[STATUS]: thesis.`; OPEN status is one of `ACTIVE|QUEUED|BLOCKED`, CLOSED is `COMPLETE|DROPPED` (closed cards are OUT of scope — map open only). Card fields are `Capability/Shape/Unlocks/Anchors/Tension?/Ripple?/Atomic?`. `Ripple: <lang>:<pkg> [SLUG] (relationship)` is BIDIRECTIONAL — the counterpart card in the named pkg carries the mirror slug; capture each ripple line verbatim.',
  'TOPOLOGY: csharp packages `Rasm.AppHost|AppUi|Bim|Compute|Fabrication|Materials|Persistence` each own a `.planning/` and a per-package `.api/`; `libs/csharp/Rasm/Geometry/.planning` holds the geometry design pages, but its core docs + `.api/` live at the `libs/csharp/Rasm/` ROOT (one level UP from `Geometry/`) and the cards live at `libs/csharp/Rasm/{IDEAS,TASKLOG}.md`. python folders `artifacts|compute|data|geometry|runtime` and typescript folders `interchange|platform|projection|services|ui` each own a `.planning/` and a folder `.api/`, plus a LANGUAGE-LEVEL `libs/python/.api` / `libs/typescript/.api`. C# has per-package `.api/` only (no language-level tier). A folder owns its cards at `<folder>/{IDEAS,TASKLOG}.md`.',
].join('\n')

// --- [OPERATIONS] -- probe doctrine + prompt (live tool resolution of tool-gated blockers) --
const PROBE_DOCTRINE = [
  'TOOLING — Parametric_Forge owns the machine (Nix/home-manager: PATH, the Python scientific/native build env, the container runtime, and local service provisioning); Rasm reaches it ONLY through assay and the `forge-scientific-*` env wrappers, never by editing Forge or installing imperatively (a rebuild wipes imperative installs). The live Rhino host and the assay bridge are per-machine SINGLETONS — this phase is SERIALIZED for that reason; never assume a second host, and never fan a heavy tool run.',
  'API EXISTENCE — verify an UNCATALOGUED host/NuGet/Python/TS member with `uv run python -m tools.assay api resolve|query` (read tools/assay/README.md FIRST). A decompile/XML hit is a CLAIM, not a fact: ilspy/XML miss `internal`/`[Obsolete]`/accessibility, member existence is not value-equivalence, and same-family agents co-hallucinate — for a C# plan-anchor the build-probe or `assay bridge verify` (EvidenceCertificate) is ground truth, trust the compiler/generated source over a second read.',
  'PYTHON PACKAGE BANDS — a new Python dependency is exactly one of three, and the band decides the manifest marker AND the install/reflect path: (a) pure-Python wheel -> installs to `.venv`, reflect via `uv run --no-sync python -m tools.assay api query`; (b) scientific/native (anything on the Arrow/GDAL/PROJ stack — pyarrow, datafusion, polars-st, rioxarray, h3ronpy, ...) -> NOT in `.venv`; it needs `forge-scientific-sync` (invoke BARE; it already passes `--locked --group scientific`) and is reflected by running assay UNDER the sci-env python; (c) companion-band -> a package that hard-depends on scipy/numba or carries `; python_version<\'3.15\'` (tensorstore, geoarrow-rust-compute, lazrs, xarray-spatial, flox, pdal, ...) MUST carry the `<3.15` marker and is reflected on the cp312 `forge-companion-env`, else it forces a doomed scipy/numba source-build. Decide the band before you assert admissibility.',
  'LIVE BEHAVIOR — when the blocker is real host/GH2 behavior, launch Rhino WIP (NEVER Rhino 8) and probe via the `rhino-mcp` skill (interactive) or `tools/rhino-bridge` (deterministic, certificate-backed); extract the EXACT behavior as the fact the realizer encodes. Server/native/extension closure facts come from `uv run python -m tools.assay provision check` (sanitized evidence), never a direct `forge-provision` call.',
].join('\n')
const LANG_HINT = {
  csharp: 'CARD LANGUAGE = `csharp`: verify members via `uv run python -m tools.assay api` over host DLLs / NuGet; back a plan-anchor with the build-probe or `assay bridge verify` (EvidenceCertificate).',
  python: 'CARD LANGUAGE = `python`: decide the package BAND (pure-`.venv` / scientific `forge-scientific-sync` / cp312 companion) before asserting admissibility, and reflect members under the matching env.',
  typescript: 'CARD LANGUAGE = `typescript`: verify members against the published types in node_modules via `uv run python -m tools.assay api`.',
}
const probePrompt = (b) => [PROBE_DOCTRINE, '', (LANG_HINT[b.lang] || ''), '', 'TASK: PROBE the `[BLOCKED]` card `' + b.slug + '` (folder `' + b.folder + '`) to RESOLVE its blocker BEFORE planning by running the REAL tools — not by reading catalogs (the map already resolved every catalog-answerable blocker). BLOCKER: ' + (b.blocker || '(unspecified)') + '. Do the live tool work the blocker needs: decompile the uncatalogued member via `assay api`, decide a Python package band + admissibility on the sci/companion env, or launch Rhino WIP and extract the live behavior. Edit NO design page — your output is the resolving evidence. If the blocker is now resolved, return resolution=unblocked with the precise `fact` (the verified member/signature, the package + band + manifest marker, or the extracted behavior) so the plan and package-prep fold it in; if it genuinely needs out-of-scope work first, return resolution=legitimate with the `reason`. Return PROBE.'].join('\n')

const enumeratePrompt = (scope) => [LAW, '', 'TASK: ENUMERATE every `<pkg>/.planning/<sub-domain>/` sub-domain directory under `' + scope + '`. The scope may be `libs` (all three languages), a language root (`libs/python`), a folder/package (`libs/python/compute`, `libs/csharp/Rasm.Bim`), or the Geometry exception (`libs/csharp/Rasm/Geometry`). Use `find` to list the leaf sub-domain directories that sit directly under each `.planning/`; do not cd. For EACH return `{folder (the owning package/folder path that holds the cards + README — e.g. `libs/python/compute`, `libs/csharp/Rasm.Bim`, or `libs/csharp/Rasm/Geometry`), lang (csharp|python|typescript), subdomain (the leaf directory name under `.planning/`), path (the repo-relative sub-domain directory path)}`. Map every sub-domain in scope; omit none.'].join('\n')

const mapPrompt = (sd) => [LAW, '', 'TASK: MAP the sub-domain `' + sd.path + '` (folder `' + sd.folder + '`, lang `' + sd.lang + '`). Read-only; return the SUBMAP.',
  '(1) FILES — list every file in the sub-domain directory.',
  '(2) PAGES — read every design page (`*.md` under the sub-domain) and return its repo-relative path; these are the FENCES the implement pass realizes.',
  '(3) APIS — list the `.api/` catalogs in scope for this sub-domain: the folder catalog set at `' + sd.folder + '/.api/`, PLUS the language-level `libs/' + sd.lang + '/.api/` for python/typescript, PLUS the `libs/csharp/Rasm/.api/` root set for the `Rasm/Geometry` case. Return each catalog path.',
  '(4) CARDS — locate the owning `IDEAS.md` + `TASKLOG.md` at the folder root (`' + sd.folder + '/IDEAS.md`, `' + sd.folder + '/TASKLOG.md`; the `Rasm/Geometry` case reads `libs/csharp/Rasm/{IDEAS,TASKLOG}.md`) and extract EVERY OPEN card (status `ACTIVE|QUEUED|BLOCKED`) whose work touches THIS sub-domain — each `{slug, kind(idea|task), status, atomic, thesis (the card one-line thesis), pages[] (the design pages it realizes/deepens), ripple[] (each `<lang>:<pkg> [SLUG]` link verbatim)}`. For a `BLOCKED` card ALSO capture `blocker` (the blocker text verbatim) and classify `blockerKind`: `catalog` — the blocker is answerable from the `.api` catalogs and design pages you are ALREADY reading (the member is catalogued, the decision is recorded); RESOLVE it inline and return the resolving `fact`, so no live probe is needed. `tooling` — it needs LIVE tool execution you cannot do here (decompile an UNCATALOGED member via `assay api`, decide a Python package band/admissibility, or launch Rhino WIP via `rhino-mcp`/`rhino-bridge`); leave it for the serialized probe. `task` — it is gated on other work that must land first. Resolve `catalog` blockers only; you are read-only and run in parallel, so never attempt `tooling`/`task` live work. Skip CLOSED cards. A card whose realization lands in this sub-domain belongs here even when its origin line lives in another folder.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
const mapScope = async (scope) => {
  const dirs = await agent(enumeratePrompt(scope), { label: 'enumerate:' + scope, phase: 'Map', schema: DIRS_SCHEMA, model: 'sonnet' })
  const subs = ((dirs && dirs.subdomains) || []).filter(Boolean)
  return (await pool(subs, CAP, (sd) => agent(mapPrompt(sd), { label: 'map:' + sd.folder + '/' + sd.subdomain, phase: 'Map', schema: SUBMAP, model: 'sonnet', stallMs: 300000 }).then((m) => m ? { ...m, folder: m.folder || sd.folder, lang: m.lang || sd.lang, subdomain: m.subdomain || sd.subdomain } : null))).filter(Boolean)
}

phase('Map')
// map every target scope into ONE union, deduped by folder|subdomain (overlapping or sibling scopes never double-map a sub-domain)
const targetSubs = []
const seenPath = new Set()
for (const sc of effectiveScopes) for (const sm of await mapScope(sc)) { const k = sm.folder + '|' + sm.subdomain; if (seenPath.has(k)) continue; seenPath.add(k); targetSubs.push({ ...sm, inScope: true }) }
log('Map: ' + targetSubs.length + ' sub-domain map(s) under ' + SCOPE)

// folder cache (full card map per enumerated folder) so closure selects ripple cards without re-mapping a folder
const folderMaps = new Map()
for (const s of targetSubs) { const a = folderMaps.get(s.folder) || []; a.push(s); folderMaps.set(s.folder, a) }
const result = [...targetSubs]
const seen = new Set(); for (const s of targetSubs) for (const c of (s.cards || [])) seen.add(cardKey(s.folder, c.slug))
const rippleOutOfScope = []
const pulledFolders = new Set()
const frontierFrom = (folder, lang, cards) => { const out = []; for (const c of (cards || [])) for (const r of (c.ripple || [])) { const { tok, slug } = parseRipple(r); if (!slug) continue; const res = resolveRipple(c.lang || lang, tok); if (!res) { rippleOutOfScope.push({ from: folder, slug, ripple: r }); continue } const key = cardKey(res.folder, slug); if (seen.has(key)) continue; out.push({ folder: res.folder, lang: res.lang, slug, key }) } return out }

if (CLOSURE_ON) {
  phase('Closure')
  let frontier = result.flatMap((s) => frontierFrom(s.folder, s.lang, s.cards))
  let round = 0
  while (frontier.length) {
    round++
    const fresh = [...new Map(frontier.filter((f) => !seen.has(f.key)).map((f) => [f.key, f])).values()]
    if (!fresh.length) break
    fresh.forEach((f) => seen.add(f.key))
    const wantByFolder = new Map(); for (const f of fresh) { (wantByFolder.get(f.folder) || wantByFolder.set(f.folder, new Set()).get(f.folder)).add(f.slug) }
    const newFolders = [...wantByFolder.keys()].filter((f) => !folderMaps.has(f))
    await pool(newFolders, CAP, async (folder) => { folderMaps.set(folder, await mapScope(folder)) })
    let next = []
    for (const [folder, want] of wantByFolder) {
      for (const sm of (folderMaps.get(folder) || [])) {
        const kept = (sm.cards || []).filter((c) => want.has(c.slug))
        if (!kept.length) continue
        pulledFolders.add(folder) // mark pulled-in only when a card is actually retained (a ripple to a closed/unmapped slug contributes nothing)
        result.push({ ...sm, cards: kept, inScope: false })
        next = next.concat(frontierFrom(folder, sm.lang, kept))
      }
    }
    frontier = next
    log('Closure round ' + round + ': +' + fresh.length + ' ripple card(s) across ' + newFolders.length + ' new folder(s)')
  }
  log('Closure: ' + pulledFolders.size + ' folder(s) pulled in (ripple-only), ' + rippleOutOfScope.length + ' unrealizable ripple(s)')
}

const legitimate = []
if (PROBE_ON) {
  const blockers = [...new Map(result.flatMap((s) => (s.cards || []).filter((c) => (c.status || '').toUpperCase() === 'BLOCKED' && c.blockerKind === 'tooling' && !c.fact).map((c) => [cardKey(s.folder, c.slug), { slug: c.slug, blocker: c.blocker, lang: s.lang, folder: s.folder }]))).values()]
  if (blockers.length) {
    phase('Probe')
    log('Probe: ' + blockers.length + ' tool-gated blocker(s) to resolve (serial)')
    const probed = (await pool(blockers, PROBE_CAP, (b) => agent(probePrompt(b), { label: 'probe:' + b.slug, phase: 'Probe', schema: PROBE, effort: 'high', stallMs: 300000 }).then((r) => r ? { ...r, slug: r.slug || b.slug, folder: b.folder } : null))).filter(Boolean)
    const factBy = new Map(); const reasonBy = new Map()
    for (const p of probed) { if (p.resolution === 'unblocked') factBy.set(cardKey(p.folder, p.slug), p.fact || ''); else reasonBy.set(cardKey(p.folder, p.slug), p.reason || 'probe found it legitimately blocked') }
    for (const s of result) for (const c of (s.cards || [])) { const k = cardKey(s.folder, c.slug); if (factBy.has(k)) c.fact = factBy.get(k); else if (reasonBy.has(k)) legitimate.push({ slug: c.slug, folder: s.folder, reason: reasonBy.get(k) }) }
    log('Probe: ' + factBy.size + ' unblocked, ' + reasonBy.size + ' legitimately blocked of ' + blockers.length)
  }
}

// flat per-card rollup (ideas separated from tasks); a card mapped into >1 sub-domain is unioned, not duplicated
const rollup = new Map()
for (const s of result) for (const c of (s.cards || [])) {
  if (!c.slug) continue
  const prev = rollup.get(cardKey(s.folder, c.slug))
  if (!prev) { rollup.set(cardKey(s.folder, c.slug), { slug: c.slug, kind: c.kind, status: c.status, thesis: c.thesis, folder: s.folder, lang: s.lang, inScope: s.inScope, pages: [...(c.pages || [])], ripple: [...(c.ripple || [])] }); continue }
  prev.pages = [...new Set([...prev.pages, ...(c.pages || [])])]
  prev.ripple = [...new Set([...prev.ripple, ...(c.ripple || [])])]
}
const manifest = [...rollup.values()]
log('Map: ' + manifest.filter((c) => c.kind === 'idea').length + ' open idea(s), ' + manifest.filter((c) => c.kind === 'task').length + ' open task(s) across ' + result.length + ' sub-domain map(s)')
return { scope: SCOPE, subdomains: result, ideas: manifest.filter((c) => c.kind === 'idea'), tasks: manifest.filter((c) => c.kind === 'task'), pulled_in: [...pulledFolders], ripple_out_of_scope: rippleOutOfScope, legitimate }
