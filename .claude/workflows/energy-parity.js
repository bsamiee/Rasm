export const meta = {
  name: 'energy-parity',
  whenToUse: 'Ephemeral, one-shot: the energy-parity close over all three branches after the three track chains land. Six opus deep-mappers (an OWNERS lane and a CROSS-language lane per branch) build the full findings/capability/seam map; ONE fable agent fixes everything. Delete after landing.',
  description: 'Ephemeral cross-language energy-parity pass. Map: 6 read-only opus agents — per branch, an OWNERS lane deep-reads every energy surface + the relevant .api catalogs (both tiers) and returns the findings census plus the unexploited-capability inventory; a CROSS lane reads the same surfaces AND the sibling branches\' energy pages to map every wire seam (content-key identity, evidence frames, document flows) as aligned/mismatch/missing. Fix: ONE fable agent holding all six dossiers + all three docs/stacks root doctrines corrects everything in place — gaps, mistakes, misalignments, unexploited capability woven in, parity smoothed WITHOUT coupling. No verify, no iterative tail. args none.',
  phases: [
    { title: 'Map', detail: '6 read-only opus agents (owners + cross per branch): findings census, unexploited .api capability inventory, wire-seam parity map — hostile, disk-anchored', model: 'opus' },
    { title: 'Fix', detail: 'ONE fable agent (all three doctrine roots read) corrects the full map in place: fix, weave, align at the wire — never couple; one retry on death', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const SCRATCH = '.claude/scratch/energy-parity'

// --- [MODELS] ----------------------------------------------------------------------------
const MAP_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'findings', 'capabilities', 'seams'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' },
  findings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['kind', 'files', 'evidence'], properties: {
    kind: { type: 'string', enum: ['gap', 'mistake', 'naive', 'phantom', 'splitbrain', 'flow', 'stale'] },
    files: { type: 'array', items: { type: 'string' } }, anchor: { type: 'string' }, evidence: { type: 'string' }, pointer: { type: 'string' } } } },
  capabilities: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'unexploited'], properties: {
    catalog: { type: 'string' }, unexploited: { type: 'array', items: { type: 'string' } }, owner: { type: 'string' } } } },
  seams: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['concept', 'status', 'detail'], properties: {
    concept: { type: 'string' }, status: { type: 'string', enum: ['aligned', 'mismatch', 'missing'] }, detail: { type: 'string' } } } } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] },
  corrected: { type: 'string' }, woven: { type: 'string' }, parity: { type: 'string' }, summary: { type: 'string' },
  unreachable: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: {
    files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, all three planning corpora (markdown design pages; code fences are the product). The three ENERGY track chains have landed: ' +
    'cs (Rasm.Bim Energy/ exchange+projector+derive, Compute Analysis energy+assessment, Element seams), py (geometry energy/ ' +
    'climate+model+district+simulate, runtime execution/recipe, data impact + tabular egress), ts (wire codec/geo, state evidence/timeline, ' +
    'ui view/compose + viewer geo/bcf). Each branch is INDEPENDENT lib-grade in isolation, aligned to its siblings at the WIRE ONLY ' +
    '(content-keyed document bytes on one XxHash128/ContentIdentity/ContentHash derivation; self-describing column-disciplined frames; ' +
    'producer-opaque evidence references) — NEVER a shared client, NEVER a cross-language import, NEVER a 1-1-1 shape mirror. Parity means ' +
    'the wire contracts agree byte-for-byte and every branch records its side truthfully; coupling is the defect, not the goal.',
  'ADVERSARIAL STANCE (absolute): every fence is naive, shallow, or ILLUSORY until it survives attack; three prior passes per branch are ' +
    'PRIOR AUTHORS whose claims are rejected self-assessments; dense confident work is the prime suspect. NAIVETY on two axes: COVERAGE ' +
    '(thin slice of the concept) and APPROACH (enumerated rosters where a parameterized generator belongs).',
  'REMOVAL DISCIPLINE: capability/packages/concepts are never removed — underutilization is an integration target; the only sanctioned ' +
    'deletion is a proven phantom (a cited member that does not exist and carries no intent evidence).',
].join('\n')
const VERIFY = 'MEMBER TRUTH: verify novel members via `uv run python -m tools.assay api` (cs: restored assemblies; py: `resolve <pkg>` or ' +
  'direct import probes; ts: the published types in node_modules) — fallback: the `.api` catalogs + feed truth; never memory.'

const SURFACES = {
  cs: 'libs/csharp/Rasm.Bim/.planning/Energy/{exchange,projector,derive}.md, Exchange/format.md, Model/faults.md, ' +
    'Rasm.Compute/.planning/Analysis/{energy,assessment}.md, Rasm.Element/{Graph/element,Composition/material}.md, plus the Bim/Compute ' +
    'README.md + ARCHITECTURE.md seam rows',
  py: 'libs/python/geometry/.planning/energy/{climate,model,district,simulate}.md, runtime/.planning/execution/recipe.md (+ the ' +
    'reliability/resilience.md ENGINE row), data/.planning/impact/impact.md + tabular/columnar.md, plus the three folders\' README.md + ' +
    'ARCHITECTURE.md rows',
  ts: 'libs/typescript/wire/.planning/codec/geo.md, state/.planning/evidence/timeline.md, ui/.planning/view/compose.md + ' +
    'viewer/geo/{layers,project}.md + viewer/mark/bcf.md, plus the wire/state/ui README/ARCHITECTURE/BLUEPRINT rows',
}
const APIS = {
  cs: 'Rasm.Bim/.api/ (api-honeybee-schema, api-dragonfly-schema, api-openstudio), Rasm.Compute/.api/, the Persistence ' +
    'api-pollination-sdk.md interim tier, and the shared libs/csharp/.api/ rails the pages compose',
  py: 'the geometry/.api/ energy catalogs (honeybee/dragonfly/ladybug families), runtime/.api/ (queenbee, lbt-recipes, ' +
    'pollination-handlers), data/.api/ (bw2analyzer, openepd, epdx, olca and peers), and the shared libs/python/.api/ rails',
  ts: 'the shared libs/typescript/.api/ Effect-family catalogs and the wire/state/ui folder .api/ tiers',
}

// --- [OPERATIONS] ------------------------------------------------------------------------
const ownersPrompt = (lang) => [LAW, '', VERIFY, '', 'TASK: OWNERS MAP (read-only, do NOT edit), lane = ' + lang + '-owners. Deep-read EVERY ' +
  lang + ' energy surface IN FULL: ' + SURFACES[lang] + ' — plus the folder context each composes (entry owners, seam partners, fault ' +
  'families). Then enumerate and READ the relevant .api capability: ' + APIS[lang] + ' — with a real ls of both tiers, never memory. ' +
  'Your product is the DEEP MAP: (a) findings — every gap, mistake, naivety (both axes), phantom, split-brain, logic-flow break, or stale ' +
  'reference, disk-anchored with evidence (facts; pointer is an initial fix pointer, never a ceiling); (b) capabilities — per catalog, the ' +
  'CONCRETE unexploited members/combinators/surfaces the landed pages admit but do not exploit, each with its best owning page; (c) seams — ' +
  'the wire-facing rows your branch declares (identity derivation, frame columns, document/artifact grammar), each aligned/mismatch/missing ' +
  'judged against what YOUR branch\'s own pages state. WRITE the full dossier to ' + SCRATCH + '/map-' + lang + '-owners.md (dense, ' +
  'evidence-first); return the structured index. NO edits outside ' + SCRATCH + '/.'].join('\n')
const crossPrompt = (lang, others) => [LAW, '', VERIFY, '', 'TASK: CROSS MAP (read-only, do NOT edit), lane = ' + lang + '-cross. Read the ' +
  lang + ' energy surfaces (' + SURFACES[lang] + ') AND the sibling branches\' energy surfaces (' + others.map((o) => SURFACES[o]).join('; ') +
  ') — full reads. Your product is the PARITY MAP from the ' + lang + ' vantage: (a) seams — EVERY cross-language contact (the content-key ' +
  'identity derivation and its seed/format-key transcription, the HBJSON/energy-document byte grammar, the artifact `key:kind` address ' +
  'grammar, evidence/result frame column discipline, document-reference shapes, recipe/simulation handoffs, graduation/handoff records) ' +
  'judged aligned / mismatch (cite BOTH sides\' exact statements) / missing (one side records the seam, the other is silent); (b) findings — ' +
  'defects visible only from the cross-vantage: a concept one branch models richly and a sibling naively where the DOMAIN (not coupling) ' +
  'demands parity, duplicated derivations that should be wire-law, producer-opacity breaches, alignment prose that asserts what the fences ' +
  'do not implement; (c) capabilities — sibling-proven capability patterns this branch\'s admitted packages could reach but do not (cite the ' +
  'sibling precedent + the local catalog member). WRITE the dossier to ' + SCRATCH + '/map-' + lang + '-cross.md; return the structured ' +
  'index. NO edits outside ' + SCRATCH + '/.'].join('\n')
const fixPrompt = (maps) => [LAW, '', VERIFY, '', 'TASK: THE PARITY FIX — you are the ONE writer and the LAST agent; no verify pass, no ' +
  'iterative tail; every fix lands at the objectively-best root-level form NOW. FIRST read ALL THREE doctrine roots IN FULL (real ls each, ' +
  'root pages only): docs/stacks/csharp/ (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, ' +
  'system-apis), docs/stacks/python/ (README atlas order: language, shapes, iteration, surfaces-and-dispatch, rails-and-effects, concurrency, ' +
  'boundaries, algorithms, system-apis, runtime), docs/stacks/typescript/ (README, language, derivation, values, computation, shapes, ' +
  'surfaces-and-dispatch, rails-and-effects, services-and-layers, concurrency, streams, boundaries) — every fence you touch holds its owning ' +
  'language\'s bar. Then read all six dossiers in ' + SCRATCH + '/ IN FULL; the structured indexes below are pointers, the dossiers are the ' +
  'map. Close EVERYTHING: fix every finding at its root (verify anything load-bearing yourself — the mappers point, you decide); WEAVE every ' +
  'justified unexploited capability into its owning page (a case/row/field/operation on the existing owner, cited to the catalog member — ' +
  'justified only, never speculative); resolve every seam mismatch by correcting the WRONG side (or both) to one wire law and every missing ' +
  'seam by recording it on the silent side — alignment lands as seam rows, shared byte grammar, and identical derivations stated in each ' +
  'branch\'s own vocabulary, NEVER as imports, shared clients, mirrored type names, or unusual routing. Keep every README/ARCHITECTURE row ' +
  'truthful in the same motion. WRITE-FULLY now. HARD PROHIBITIONS: never touch libs/csharp/Rasm/** or libs/csharp/Rasm.Fabrication/** (a ' +
  'rebuild leg owns them), libs/python/artifacts/**, RASM-PY-ARTIFACTS-*.md (a live design workflow owns them), RASM-CS-GEOMETRY-*.md, or any ' +
  'root RASM-* doc; a genuinely unreachable fix (locked file, out-of-scope owner) returns in `unreachable` with its exact claim — never ' +
  'silently dropped. MAP INDEXES:\n' + JSON.stringify(maps, null, 1) +
  '\nReturn files (every page edited), verdict, corrected (defects closed), woven (capability integrated), parity (seams aligned), summary, unreachable.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Map')
const LANGS = ['cs', 'py', 'ts']
const mappers = LANGS.flatMap((lang) => [
  () => agent(ownersPrompt(lang), { label: 'map:' + lang + '-owners', phase: 'Map', model: 'opus', effort: 'max', schema: MAP_SCHEMA, stallMs: STALL }),
  () => agent(crossPrompt(lang, LANGS.filter((o) => o !== lang)), { label: 'map:' + lang + '-cross', phase: 'Map', model: 'opus', effort: 'max', schema: MAP_SCHEMA, stallMs: STALL }),
])
const maps = (await parallel(mappers)).filter(Boolean)
const totals = { findings: maps.reduce((n, m) => n + (m.findings || []).length, 0), capabilities: maps.reduce((n, m) => n + (m.capabilities || []).length, 0), mismatches: maps.flatMap((m) => m.seams || []).filter((s) => s.status !== 'aligned').length }
log('Map: ' + maps.length + '/6 lanes; ' + totals.findings + ' finding(s), ' + totals.capabilities + ' capability row(s), ' + totals.mismatches + ' seam mismatch/missing')

if (!maps.length) { log('No maps returned — aborting before Fix; resume re-runs the map fan.'); return { lanes: 0, aborted: 'map' } }

phase('Fix')
// One bounded re-attempt: a transient agent death must never silently lose the terminal pass.
const opts = { label: 'fix (' + totals.findings + 'f/' + totals.capabilities + 'c/' + totals.mismatches + 's)', phase: 'Fix', model: 'fable', effort: 'max', schema: FIX_SCHEMA, stallMs: STALL }
const fix = (await agent(fixPrompt(maps), opts)) || (await agent(fixPrompt(maps), { ...opts, label: opts.label + ':retry' }))
log('Fix: ' + (fix ? fix.verdict + '; ' + (fix.files || []).length + ' file(s); ' + ((fix.unreachable || []).length) + ' unreachable' : 'FIX AGENT DIED TWICE — maps survive in ' + SCRATCH))

return {
  lanes: maps.map((m) => m.lane), totals,
  fix: fix ? { verdict: fix.verdict, files: fix.files, corrected: fix.corrected, woven: fix.woven, parity: fix.parity, summary: fix.summary, unreachable: fix.unreachable || [] } : null,
  scratch: SCRATCH,
}
