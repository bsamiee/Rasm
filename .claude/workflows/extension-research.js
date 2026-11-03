export const meta = {
  name: 'extension-research',
  description: 'Research one TypeScript extension domain note and its packages at university depth, or write one domain\'s focused build plan from its distilled products, rebuilt code and research',
  whenToUse: 'args.key a note (color, typography, composition, grids, perso-arabic, print, indesign, data, mcp); args.mode plan with args.key a plan domain (typography, grids, color, indesign, data, mcp); one run at a time, notes sharing packages.md and plans ARCHITECTURE.md and TYPESCRIPT.md',
  phases: [
    { title: 'Research', detail: 'note: principles researcher, then packages researcher reading the new note, packages.md and the catalog' },
    { title: 'Read', detail: 'plan: evidence grouped by product into what one reader holds, then one reader per group writing the capability it holds or implies' },
    { title: 'Plan', detail: 'plan: one writer of the domain plan, its ARCHITECTURE.md rows and its TYPESCRIPT.md rows' },
    { title: 'Challenge', detail: 'two adversaries in sequence over the note or plan, for a note one over packages.md beside them, each against its snapshot', model: 'opus' },
    { title: 'Prose', detail: 'one clean-prose pass over every file the run wrote' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/docs/research/typescript-extension`
const HOST = `${REPO}/docs/research/applications/indesign`
const COMPANION = `${REPO}/.claude/workflows/TYPESCRIPT.md`
const BAR = `${REPO}/.claude/workflows/BAR.md`
const study = (...names) => names.map((name) => `${DIR}/studies/${name}.md`)
const distilled = (...names) => names.map((name) => `${HOST}/distilled/${name}`)
const COLOR = [...study('adobe-indesign-color', 'color-engines'), ...distilled('internals/adobe-indesign-color'), `${HOST}/internals/adobe-indesign-color/README.md`]
const TYPESETTING = [...study('inpage-typesetting-tool', 'scripts-typesetting', 'winsoft-scribedoor'), ...distilled('typesetting')]
const RIGHT_TO_LEFT = [...study('scripts-right-to-left', 'adobe-indesign-right-to-left'), ...distilled('right-to-left', 'internals/adobe-indesign-right-to-left')]
const COMPOSERS = [...study('adobe-indesign-composers'), ...distilled('internals/adobe-indesign-composers')]
const GRIDS = [...study('designersbookshop-grid-calculator-pe', 'scripts-grids', 'adobe-indesign-guides', 'scripts-pages'), ...distilled('grids', 'internals/adobe-indesign-guides', 'pages')]
const DOCUMENT = [...study('adobe-indesign-preferences', 'adobe-indesign-guides', 'adobe-indesign-composers', 'adobe-indesign-right-to-left', 'adobe-indesign-color', 'scripts-styles', 'scripts-tables', 'scripts-footnotes', 'scripts-pages'), ...distilled('internals', 'typesetting/inpage-typesetting-tool/panels', 'styles', 'tables', 'footnotes', 'pages'), `${HOST}/facts`]
const NOTES = {
  color: ['Colorimetry and color appearance: CIE XYZ, Lab, Oklab and Oklch, CMYK and separations, spot and ink models, ICC profiles and rendering intents, gamut mapping, chromatic adaptation, perceptual difference (deltaE 2000, CAM16), contrast (WCAG, APCA), harmony and palette construction as computation, and how print differs from screen', COLOR],
  typography: ['Type as a system: classification, anatomy and metrics (x-height, cap height, ascender, descender, overshoot, sidebearings), OpenType features and variable axes, optical sizes, Unicode and shaping, what a font states about its own capability and how software reads it', TYPESETTING],
  composition: ['Line and paragraph setting: measure and leading relations, vertical rhythm, justification algorithms (Knuth-Plass and successors), hyphenation (Liang), microtypography (expansion, protrusion), widows and pagination, rivers and color of text, tracking and kerning principles, and optical alignment', [...COMPOSERS, ...distilled('typesetting/id-extras-v-justify', 'footnotes')]],
  grids: ['Grids and page construction: canons, modular grids, margin ratios, ratios and scales, format systems, units, derived column, gutter and baseline relations for any format in print or pixels, and alignment and guides as one general relation', GRIDS],
  'perso-arabic': ['Perso-Arabic typesetting: Naskh and Nastaliq construction, joining and contextual forms, kashida and justification, mark positioning and stacking, alternates, Persian and Arabic locale forms and digits, W3C alreq, the Unicode bidirectional algorithm and line breaking, and vocalized and Quranic text', [...RIGHT_TO_LEFT, ...TYPESETTING]],
  print: ['Print and document production: PDF/X and PDF/A, page boxes, bleed, trim and imposition mathematics, printing conditions, screening and tone value increase, overprint and trapping, ink limits, image resolution relations, and preflight as computed checks', [...COLOR, ...distilled('pages')]],
  indesign: ['InDesign\'s interface and scripting model: panels, panes, dialogs, preference pages, lists, buttons, swatches, color wheels and hue controls native in look and behavior beside InDesign\'s own interface; every application preference set with no document open and every document setting with one open; host categories, DOM members with their enumerations, ranges and units, events, styles, tables and footnotes as host objects, and controls bound as data to every setting by the host\'s own categories', DOCUMENT],
  data: ['Stores and search: SQL pagination (keyset and cursor), search kinds (exact, prefix, fuzzy, full text, vector), ranking, Arabic-script folding and normalization for search, and store choice by workload', []],
  mcp: ['Model Context Protocol at its newest revision: tools, resources, prompts, tasks, elicitation, progress, subscriptions, pagination, completion, caching, middleware, telemetry and transports, and what automation and scripting hosts expose through it', []],
}
const PLANS = {
  typography: [['typography', 'composition', 'perso-arabic'], [...TYPESETTING, ...RIGHT_TO_LEFT, ...COMPOSERS, `${REPO}/.artifacts/workflows/typescript-extension/inpage/PLAN.md`], ['graphics/typography']],
  grids: [['grids'], GRIDS, ['graphics/grid']],
  color: [['color', 'print'], COLOR, ['graphics/color']],
  indesign: [['indesign'], DOCUMENT, ['creative-cloud/indesign', 'creative-cloud/indesign/events']],
  data: [['data'], [], ['data/search', 'data/database']],
  mcp: [['mcp'], [], ['ai/mcp', 'ai/mcp/tasks']],
}

// --- [MODELS] --------------------------------------------------------------------------

const PATHS = { type: 'array', items: { type: 'string' } }
const EDITED = { type: 'object', properties: { files: PATHS, report: { type: 'string' } }, required: ['files', 'report'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

const KEY = args?.key
const PLANNING = args?.mode === 'plan'
const table = PLANNING ? PLANS : NOTES
if (![undefined, 'plan'].includes(args?.mode) || !Object.hasOwn(table, KEY)) throw new Error(`args take key (${Object.keys(NOTES).join(', ')}) or mode plan with key (${Object.keys(PLANS).join(', ')}), got ${JSON.stringify(args ?? null)}`)
const SCRATCH = `${REPO}/.artifacts/workflows/extension-research/${PLANNING ? 'plan-' : ''}${KEY}`
const BASE = `${SCRATCH}/base`
const NOTE_PATHS = Object.keys(NOTES).map((name) => `${DIR}/${name}.md`)
const BASIS = `Load Skill(clean-prose) first and read ${COMPANION} for extension method, levels and the file owning each fact.`
const DEPTH = [
  'Depth is what a university course and a working foundry or design studio hold, from primary sources: standards, foundational books, papers and specifications.',
  'Each fact states its principle, mathematics, parameters and limits, and what software computes from it.',
  'Values are relations, parameters or properties a standard, font or host states, a product\'s fixed value being that product\'s choice.',
  'Each fact sits once, in the note owning its scope, and notes omit code paths and provenance.',
].join(' ')
const diff = (files) => files.map((file) => `\`git diff --no-index ${BASE}/${file.split('/').pop()} ${file}\``).join(' and ')
const adversary = (basis, files, read, lean, label) => agent(`${basis}
Adversary over ${files.join(', ')}. Read ${read} and the files whole, against the evidence they rest on${lean}. Rewrite in place: what is shallow, naive, wrong against its source, restated or unsupported goes or rises to its source, and what a specialist names as missing joins its owner.
Return (structured) the files you edited, other files included, and at most 8 lines: what went, what rose, what joined.`, { label, phase: 'Challenge', model: 'opus', schema: EDITED })
const COLLAPSE = ', leaning to collapse: values become the relation or parameter they derive from, lines restating one fact fold into it, and a fact another file owns moves to that file'

// --- [COMPOSITION] ---------------------------------------------------------------------

const research = async () => {
  const [scope, evidence] = NOTES[KEY]
  const note = `${DIR}/${KEY}.md`
  const packages = `${DIR}/packages.md`
  const basis = `${BASIS} ${DEPTH}\nNote: ${note}. Scope: ${scope}.${evidence.length > 0 ? ` Evidence: ${evidence.join(', ')}.` : ''} Fetched files go under ${SCRATCH}/sources.`
  phase('Research')
  const principles = await agent(`${basis}
Researcher, principles. Copy ${note} and ${packages} into ${BASE} before any edit. Read ${note} whole, the evidence, and the notes owning neighboring scopes (${NOTE_PATHS.filter((path) => path !== note).join(', ')}).
Rewrite ${note} in place: the domain's principles and mathematics, the higher-order capability they imply beyond any single product, and the level of each, raising each line to its source. Return at most 8 lines.`, { label: `research:${KEY}`, phase: 'Research', model: 'opus' })
  if (!principles) throw new Error(`principles researcher returned nothing for ${KEY}`)
  const chosen = await agent(`${basis}
Researcher, packages. Read ${note} as rewritten, ${packages} whole with every domain's chosen and rejected rows, the catalog in ${REPO}/pnpm-workspace.yaml and what ${REPO}/libs/typescript imports.
Find the newest TypeScript and JavaScript packages owning a fact the note states: list each candidate's releases with \`pnpm view <name> dist-tags time --json\` (prereleases count), and judge it by what its source and types compute at its newest release.
Edit ${packages} in place, rereading it before editing: each chosen package with the facts it owns, the members used and what it lacks that code states by hand, extending a row another domain holds; each rejected candidate with the reason it loses; versions left to catalogs. Return at most 8 lines.`, { label: `packages:${KEY}`, phase: 'Research', model: 'opus' })
  if (!chosen) throw new Error(`packages researcher returned nothing for ${KEY}`)
  const [challenged, third] = await parallel([
    () => adversary(basis, [note], diff([note]), '', `challenge:${KEY}:1`).then((first) => adversary(basis, [note], diff([note]), COLLAPSE, `challenge:${KEY}:2`).then((second) => [first, second])),
    () => adversary(basis, [packages], diff([packages]), ', reading each chosen package\'s claim against its source and choosing a rejected candidate its newest release makes stronger', `challenge:${KEY}:packages`),
  ])
  return [{ files: [note, packages], report: `${principles}\n${chosen}` }, ...(challenged ?? []), third]
}

const plan = async () => {
  const [notes, evidence, rows] = PLANS[KEY]
  const matrix = `${DIR}/ARCHITECTURE.md`
  const written = `${REPO}/.artifacts/workflows/typescript-extension/${KEY}/PLAN.md`
  const found = `${SCRATCH}/capability`
  const sources = [...notes.map((name) => `${DIR}/${name}.md`), `${DIR}/packages.md`]
  const folders = rows.map((row) => `${REPO}/libs/typescript/${row}`)
  const basis = `${BASIS} Read ${BAR}, the code bar a build run on the plan meets.\nPlan domain: ${KEY}, rows ${rows.join(', ')}.`
  phase('Read')
  const mapped = await agent(`${evidence.length === 0 ? '' : `List every file under ${evidence.join(', ')} with its size in bytes, and every ${HOST}/products/<family>/<product>/rebuilt folder of the same families with no distilled folder for its product.
Group the files by the product or concern they hold, each group what one reader reads whole at full depth: one product's files together, a product more than one reader holds split by its top-level folders.
`}Edit nothing. Return (structured) the groups as absolute paths, and as code the folders of ${folders.join(', ')} that exist.`, { label: 'map', phase: 'Read', schema: { type: 'object', properties: { groups: { type: 'array', items: PATHS }, code: PATHS }, required: ['groups', 'code'] } })
  if (!mapped || (evidence.length > 0 && mapped.groups.length === 0)) throw new Error(`map agent grouped no evidence for ${KEY}`)
  const code = mapped.code
  const groups = [...mapped.groups, [...sources, ...code]]
  log(`reader groups for ${KEY}: ${groups.length}`)
  const read = await parallel(groups.map((paths, index) => () => agent(`${basis}
Reader over ${paths.join(', ')}: read them whole${code.length > 0 ? `, and ${code.join(', ')} where the capability may already exist` : ''}.
Write ${found}/${index + 1}.md: one line per capability the files hold or their principles imply beyond any one product, stating the capability, the one relation or parameterized operation behind it and the functions that become its uses, its level, the host mechanism it runs on, the product values a caller passes, its evidence paths, the member of a ${DIR}/packages.md package that states it, read from the package source at its catalog release, or the hand code it needs, what the rows' code holds of it, and the host proof it waits on. Edit nothing else. Return the line count.`, { label: `read:${KEY}:${index + 1}`, phase: 'Read', model: 'opus' })))
  const capability = groups.map((_, index) => `${found}/${index + 1}.md`).filter((_, index) => read[index])
  if (capability.length < groups.length) log(`${groups.length - capability.length} readers returned nothing`)
  if (capability.length === 0) throw new Error(`no reader wrote capability for ${KEY}`)
  phase('Plan')
  const planned = await agent(`${basis}
Plan writer. Copy ${matrix} and ${COMPANION} into ${BASE} before any edit. Read ${[...capability, ...sources, matrix, COMPANION, ...code].join(', ')} as they stand.
Write ${written}, the input a \`deepen\` build run per row reads, opening with one pointer each to ${COMPANION} (method) and ${BAR} (code bar): per row, each capability as one relation or parameterized operation at the lowest level holding it whole, the capabilities readers name apart merged into it, with the functions that become its uses, the host mechanism, each packages.md package at its full capability with the hand code its members delete and the hand code they leave, the consumers that move, and the gates, omitting what the code holds whole and what no build agent acts on.
Then write the domain's capability rows in ${matrix} and its rows (${rows.join(', ')}) in ${COMPANION} [03] (OWNS its capabilities, CONSUMERS its importers, AFTER its workspace dependencies and gates), a nested row outside its parent's scope.
Return (structured) the files you edited and at most 8 lines.`, { label: `plan:${KEY}`, phase: 'Plan', model: 'opus', schema: EDITED })
  if (!planned) throw new Error(`plan writer returned nothing for ${KEY}`)
  const before = diff([matrix, COMPANION])
  const first = await adversary(basis, [written, matrix, COMPANION], before, `, and ${capability.join(', ')} with their evidence: a capability missing, narrowed or at the wrong level returns`, `challenge:${KEY}:1`)
  const second = await adversary(basis, [written, matrix, COMPANION], before, `${COLLAPSE}, capabilities one relation subsumes become its uses, and a line no build agent acts on goes`, `challenge:${KEY}:2`)
  return [{ ...planned, files: [written, matrix, COMPANION] }, first, second]
}

log(PLANNING ? `plan typescript-extension domain ${KEY} over ${PLANS[KEY][2].join(', ')}` : `research typescript-extension note ${KEY}`)
const results = (PLANNING ? await plan() : await research()).filter(Boolean)
const files = [...new Set(results.flatMap((result) => result.files ?? []))]
phase('Prose')
const prose = files.length === 0 ? null : await agent(`Load Skill(clean-prose). Run \`python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check ${files.join(' ')}\` once and fix each finding in place, keeping every fact. Return at most 5 lines.`, { label: 'prose', phase: 'Prose' })
return [...results.map((result) => result.report), prose ?? 'prose pass returned nothing'].join('\n')
