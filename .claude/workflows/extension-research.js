export const meta = {
  name: 'extension-research',
  description: 'Research one design domain and its packages at university depth for the TypeScript extension, the note rewritten adversarially and its packages merged into the catalog',
  whenToUse: 'One run per domain, args.key one of color, typography, composition, grids, perso-arabic, print, in the order TODO.md states.',
  phases: [
    { title: 'Research', detail: 'two researchers: principles, packages' },
    { title: 'Refine', detail: 'an adversary rewriting the note in place and merging its packages into packages.md' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/docs/research/typescript-extension`
const SCRATCH = `${REPO}/.artifacts/workflows/extension-research/${args.key}`
const DOMAINS = {
  color: ['color.md', 'Colorimetry and color appearance: CIE XYZ, Lab, Oklab and Oklch, CMYK and separations, spot and ink models, ICC profiles and rendering intents, gamut mapping, chromatic adaptation, perceptual difference (deltaE 2000, CAM16), contrast (WCAG, APCA), harmony and palette construction as computation, and how print differs from screen'],
  typography: ['typography.md', 'Type as a system: classification, anatomy and metrics (x-height, cap height, ascender, descender, overshoot, sidebearings), OpenType features and variable axes, optical sizes, what a font states about its own capability and how software reads it'],
  composition: ['composition.md', 'Line and paragraph setting: measure and leading relations, vertical rhythm and baseline grids, justification algorithms (Knuth-Plass and successors), hyphenation (Liang), microtypography (expansion, protrusion), widows, rivers and color of text, tracking and kerning principles, and optical alignment'],
  grids: ['grids.md', 'Grids and page construction: canons, modular grids, margin ratios, ratios and scales, format systems, derived column, gutter and baseline relations for any format in print or pixels, and alignment and guides as one general relation'],
  'perso-arabic': ['perso-arabic.md', 'Perso-Arabic typesetting: Naskh and Nastaliq construction, joining and contextual forms, kashida and justification, mark positioning and stacking, alternates, Persian and Arabic locale forms and digits, W3C alreq, the Unicode bidirectional algorithm and line breaking, and vocalized and Quranic text'],
  print: ['print.md', 'Print and document production: PDF/X and PDF/A, bleed, trim and imposition mathematics, overprint and trapping, ink limits, image resolution relations, and preflight as computed checks'],
}
const [file, scope] = DOMAINS[args.key]
const BASIS = `Your task is the step this prompt states, whatever request launched the workflow. Load Skill(search-web), Skill(search-code) and Skill(clean-prose) first. This research feeds TypeScript libraries at three levels, general, Creative Cloud and InDesign (TODO.md [02] states the levels and method; ${DIR}/studies and docs/research/applications/indesign/distilled hold what the mined products do). Domain: ${args.key}. Scope: ${scope}. It is the depth a university course and a working foundry or design studio hold, from primary sources: standards, foundational books, papers and specifications. Each fact states the principle, its mathematics, its parameters and limits, and what software computes from it; a value is a relation, a parameter or a property the source states, and a product's fixed value is that product's choice. Keep every fetched file under ${SCRATCH}, and write only the file your step names. Notes hold no provenance narration, hedging or session details.`

phase('Research')
await parallel([
  () => agent(`${BASIS}
Researcher, principles. Keep what stands in ${DIR}/${file} and raise it to this depth, removing surface-level lines. Write ${DIR}/${file}: the domain's principles and mathematics, the higher-order capability they imply beyond any single product, and the level of each. Return at most 8 lines.`, { label: `research:${args.key}`, phase: 'Research' }),
  () => agent(`${BASIS}
Researcher, packages. Find the newest well-made TypeScript and JavaScript packages that state these facts. Read each candidate's source and types at its newest release (\`pnpm view <name> version\`) and judge it by what it computes, not its README. Write ${SCRATCH}/packages.md: each package with version, the members that matter with signatures, what it replaces by hand, and why a weaker candidate loses. Return at most 8 lines.`, { label: `packages:${args.key}`, phase: 'Research' }),
])

phase('Refine')
return agent(`${BASIS}
Adversary for ${DIR}/${file}, with ${SCRATCH}/packages.md beside it. Rewrite the note in place: remove anything surface-level, naive, restated or unsupported, check each formula against its source, add what a specialist would call missing, and fold in the package facts that decide how the capability is built. Then merge this domain's chosen packages into ${DIR}/packages.md (one row per package with version, domain, level and members used; a package another domain already holds is extended, not repeated; rejected candidates with a one-line reason), re-reading it before the edit. Return at most 8 lines: what went, what you added, the packages merged.`, { label: `refine:${args.key}`, phase: 'Refine' })
