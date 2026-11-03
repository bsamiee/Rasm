export const meta = {
  name: 'extension-research',
  description: 'Research one TypeScript extension domain and its packages at university depth, or build the capability matrix and TYPESCRIPT.md rows',
  whenToUse: 'args.key a domain (color, typography, composition, grids, perso-arabic, print, indesign, data, mcp), one run at a time over shared packages.md; args.key architecture after the domain notes',
  phases: [
    { title: 'Research', detail: 'domain: a principles researcher, then a packages researcher reading the new note, packages.md and the catalog' },
    { title: 'Read', detail: 'architecture: a reader per distilled product group and per note writing capability rows' },
    { title: 'Matrix', detail: 'architecture: one writer of the ARCHITECTURE.md matrix and the TYPESCRIPT.md rows' },
    { title: 'Challenge', detail: 'two adversaries in sequence over the note or the matrix and rows, then for a domain one over packages.md, each against its snapshot', model: 'opus' },
    { title: 'Prose', detail: 'one clean-prose pass over every file the run wrote' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/docs/research/typescript-extension`
const DIST = `${REPO}/docs/research/applications/indesign/distilled`
const PRODUCTS = `${REPO}/docs/research/applications/indesign/products`
const COMPANION = `${REPO}/.claude/workflows/TYPESCRIPT.md`
const study = (...names) => names.map((name) => `${DIR}/studies/${name}.md`)
const distilled = (...names) => names.map((name) => `${DIST}/${name}`)
const DOMAINS = {
  color: ['color.md', 'Colorimetry and color appearance: CIE XYZ, Lab, Oklab and Oklch, CMYK and separations, spot and ink models, ICC profiles and rendering intents, gamut mapping, chromatic adaptation, perceptual difference (deltaE 2000, CAM16), contrast (WCAG, APCA), harmony and palette construction as computation, and how print differs from screen',
    [...study('adobe-indesign-color', 'color-engines'), ...distilled('internals/adobe-indesign-color')]],
  typography: ['typography.md', 'Type as a system: classification, anatomy and metrics (x-height, cap height, ascender, descender, overshoot, sidebearings), OpenType features and variable axes, optical sizes, Unicode and shaping, what a font states about its own capability and how software reads it',
    [...study('inpage-typesetting-tool', 'scripts-typesetting', 'winsoft-scribedoor'), ...distilled('typesetting')]],
  composition: ['composition.md', 'Line and paragraph setting: measure and leading relations, vertical rhythm, justification algorithms (Knuth-Plass and successors), hyphenation (Liang), microtypography (expansion, protrusion), widows and pagination, rivers and color of text, tracking and kerning principles, and optical alignment',
    [...study('adobe-indesign-composers', 'scripts-typesetting', 'scripts-footnotes'), ...distilled('internals/adobe-indesign-composers', 'typesetting/id-extras-v-justify', 'footnotes')]],
  grids: ['grids.md', 'Grids and page construction: canons, modular grids, margin ratios, ratios and scales, format systems, units, derived column, gutter and baseline relations for any format in print or pixels, and alignment and guides as one general relation',
    [...study('designersbookshop-grid-calculator-pe', 'scripts-grids', 'adobe-indesign-guides', 'scripts-pages'), ...distilled('grids', 'internals/adobe-indesign-guides', 'pages')]],
  'perso-arabic': ['perso-arabic.md', 'Perso-Arabic typesetting: Naskh and Nastaliq construction, joining and contextual forms, kashida and justification, mark positioning and stacking, alternates, Persian and Arabic locale forms and digits, W3C alreq, the Unicode bidirectional algorithm and line breaking, and vocalized and Quranic text',
    [...study('scripts-right-to-left', 'adobe-indesign-right-to-left', 'inpage-typesetting-tool'), ...distilled('right-to-left', 'internals/adobe-indesign-right-to-left', 'typesetting')]],
  print: ['print.md', 'Print and document production: PDF/X and PDF/A, page boxes, bleed, trim and imposition mathematics, printing conditions, screening and tone value increase, overprint and trapping, ink limits, image resolution relations, and preflight as computed checks',
    [...study('adobe-indesign-color', 'scripts-pages'), ...distilled('internals/adobe-indesign-color', 'pages')]],
  indesign: ['indesign.md', 'InDesign\'s scripting model: host categories, preference pages, panels, dialogs and controls, DOM members with their enumerations, ranges and units, events, styles, tables and footnotes as host objects, and binding controls to every setting by the host\'s own categories',
    [...study('adobe-indesign-preferences', 'scripts-styles', 'scripts-tables', 'scripts-footnotes'), ...distilled('internals/adobe-indesign-preferences', 'styles', 'tables'), `${REPO}/docs/research/applications/indesign/facts`]],
  data: ['data.md', 'Stores and search: SQL pagination (keyset and cursor), search kinds (exact, prefix, fuzzy, full text, vector), ranking, Arabic-script folding and normalization for search, and store choice by workload', []],
  mcp: ['mcp.md', 'Model Context Protocol at its newest revision: tools, resources, prompts, tasks, elicitation, progress, subscriptions, pagination, completion, caching, middleware, telemetry and transports, and what automation and scripting hosts expose through it', []],
}

// --- [MODELS] --------------------------------------------------------------------------

const PATHS = { type: 'array', items: { type: 'string' } }
const EDITED = { type: 'object', properties: { files: PATHS, report: { type: 'string' } }, required: ['files', 'report'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

const KEY = args?.key
const ARCHITECTURE = KEY === 'architecture'
if (!ARCHITECTURE && !Object.hasOwn(DOMAINS, KEY)) throw new Error(`args.key takes architecture or one of ${Object.keys(DOMAINS).join(', ')}; got ${JSON.stringify(KEY ?? null)}`)
const SCRATCH = `${REPO}/.artifacts/workflows/extension-research/${KEY}`
const BASE = `${SCRATCH}/base`
const NOTES = Object.values(DOMAINS).map(([file]) => `${DIR}/${file}`)
const BASIS = [
  `Load Skill(clean-prose) first, and read ${COMPANION} for the extension's method and levels.`,
  'Depth is what a university course and a working foundry or design studio hold, from primary sources: standards, foundational books, papers and specifications.',
  'Each fact states the principle, its mathematics, its parameters and limits, and what software computes from it; a value is a relation, a parameter or a property a standard, font or host states, and a product\'s fixed value is that product\'s choice.',
  `Each fact sits once: a domain note in ${DIR} owns its scope, packages.md package choice, ARCHITECTURE.md the capability matrix and levels, the [03] rows of ${COMPANION} row scope, and studies/ what one product does; notes omit code paths and provenance.`,
  `Distilled code under ${DIST} is evidence of the logic a product holds, ${PRODUCTS}/<family>/<product>/rebuilt where no distilled folder exists. Fetched files go under ${SCRATCH}/sources.`,
].join(' ')
const diff = (files) => files.map((file) => `\`git diff --no-index ${BASE}/${file.split('/').pop()} ${file}\``).join(' and ')
const adversary = (files, lean, label) => agent(`${BASIS}
Adversary over ${files.join(', ')}. Read ${diff(files)} and the files whole, against the evidence they rest on${lean}. Rewrite in place: what is shallow, naive, wrong against its source, restated or unsupported goes or rises to its source, and what a specialist names as missing joins its owner.
Return (structured) the files you edited, other notes included, and at most 8 lines: what went, what rose, what joined.`, { label, phase: 'Challenge', model: 'opus', schema: EDITED })
const COLLAPSE = ', leaning to collapse: values become the relation or parameter they derive from, lines restating one fact fold into it, and a fact another file owns moves to that file'

// --- [COMPOSITION] ---------------------------------------------------------------------

const domain = async () => {
  const [file, scope, evidence] = DOMAINS[KEY]
  const note = `${DIR}/${file}`
  const packages = `${DIR}/packages.md`
  const domainBasis = `${BASIS}\nDomain: ${KEY}. Scope: ${scope}.${evidence.length > 0 ? ` Evidence: ${evidence.join(', ')}.` : ''}`
  phase('Research')
  const principles = await agent(`${domainBasis}
Researcher, principles. Copy ${note} and ${packages} into ${BASE} before any edit. Read ${note} whole, the evidence, and the notes owning neighboring scopes (${NOTES.filter((path) => path !== note).join(', ')}).
Rewrite ${note} in place: the domain's principles and mathematics, the higher-order capability they imply beyond any single product, and the level of each, raising each line to its source. Return at most 8 lines.`, { label: `research:${KEY}`, phase: 'Research', model: 'opus' })
  if (!principles) throw new Error(`principles researcher returned nothing for ${KEY}`)
  const chosen = await agent(`${domainBasis}
Researcher, packages. Read ${note} as rewritten, ${packages} whole with every domain's chosen and rejected rows, the catalog in ${REPO}/pnpm-workspace.yaml and what ${REPO}/libs/typescript imports.
Find the newest TypeScript and JavaScript packages owning a fact the note states: list each candidate's releases with \`pnpm view <name> dist-tags time --json\` (prereleases count), and judge it by what its source and types compute at its newest release.
Edit ${packages} in place, reloading it before the edit: each chosen package with the facts it owns, the members used and what it lacks that code states by hand, extending a row another domain holds; each rejected candidate with the reason it loses; versions left to catalogs. Return at most 8 lines.`, { label: `packages:${KEY}`, phase: 'Research', model: 'opus' })
  if (!chosen) throw new Error(`packages researcher returned nothing for ${KEY}`)
  const first = await adversary([note], '', `challenge:${KEY}:1`)
  const second = await adversary([note], COLLAPSE, `challenge:${KEY}:2`)
  const third = await adversary([packages], ', reading each chosen package\'s claim against its source and choosing a rejected candidate its newest release makes stronger', `challenge:${KEY}:packages`)
  return [first, second, third]
}

const architecture = async () => {
  const matrix = `${DIR}/ARCHITECTURE.md`
  const rows = `${SCRATCH}/rows`
  const limit = 8000
  phase('Read')
  const mapped = await agent(`Copy ${matrix} and ${COMPANION} into ${BASE}.
List every product folder under ${DIST}/<family>/ with its line count, and every ${PRODUCTS}/<family>/<product>/rebuilt folder whose product has no distilled folder.
Group folders by at most ${limit} lines, products of one family together, a product above the limit split by its top-level folders. Edit nothing else. Return (structured) the groups as absolute paths.`, { label: 'map', phase: 'Read', schema: { type: 'object', properties: { groups: { type: 'array', items: PATHS } }, required: ['groups'] } })
  if (!mapped || mapped.groups.length === 0) throw new Error('map agent grouped no distilled product')
  log(`${mapped.groups.length} product groups and ${NOTES.length + 1} notes`)
  const sources = [...mapped.groups.map((group, index) => [`products-${index + 1}`, group, `each product's code whole, with its study under ${DIR}/studies`]),
    ...[...NOTES, `${REPO}/.artifacts/workflows/typescript-extension/inpage/PLAN.md`].map((path) => [path.split('/').pop().replace(/\.md$/, '').toLowerCase(), [path], 'the note whole'])]
  const read = await parallel(sources.map(([tag, paths, how]) => () => agent(`${BASIS}
Reader over ${paths.join(', ')}: read ${how}, and ${REPO}/libs/typescript where it can hold a capability.
Write ${rows}/${tag}.md: one line per capability, stating the capability, the one relation or parameterized operation behind it, its level (general, Creative Cloud, one app, Atelier), the host mechanism it runs on, the products and paths holding it as evidence, the libs/typescript package owning it now or the row that should, and the host proof or distill it waits on. Edit nothing else. Return the row count.`, { label: `read:${tag}`, phase: 'Read' })))
  const dead = read.filter((result) => !result).length
  if (dead > 0) log(`${dead} readers returned nothing`)
  phase('Matrix')
  const written = await agent(`${BASIS}
Matrix writer. Read every file under ${rows}, ${matrix}, ${COMPANION}, ${DIR}/packages.md and each libs/typescript package.json.
Merge rows naming one capability into one, keeping the strongest relation, at the lowest level that holds it whole, so one relation serves every product's variant. Rewrite ${matrix}: the levels, the matrix (capability, relation, level, owning row, evidence, gate), and the package graph from workspace dependencies. Rewrite the [03] rows of ${COMPANION}: one row per package the matrix names, OWNS stating its capabilities, CONSUMERS its importers, AFTER its workspace dependencies and gates; a row nested in another row's folder stays outside its parent's scope.
Return (structured) the files you edited and at most 8 lines.`, { label: 'matrix', phase: 'Matrix', model: 'opus', schema: EDITED })
  if (!written) throw new Error('matrix writer returned nothing')
  const first = await adversary([matrix, COMPANION], `, and every file under ${rows} and its evidence: a capability missing, narrowed or at the wrong level returns to its row`, 'challenge:matrix:1')
  const second = await adversary([matrix, COMPANION], `${COLLAPSE}, and capabilities one relation subsumes become its uses`, 'challenge:matrix:2')
  return [written, first, second]
}

const results = (ARCHITECTURE ? await architecture() : await domain()).filter(Boolean)
const files = [...new Set(results.flatMap((result) => result.files ?? []))]
phase('Prose')
const prose = files.length === 0 ? null : await agent(`Load Skill(clean-prose). Run \`python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check ${files.join(' ')}\` once and fix each finding in place, keeping every fact. Return at most 5 lines.`, { label: 'prose', phase: 'Prose' })
return [...results.map((result) => result.report), prose ?? 'prose pass returned nothing'].join('\n')
