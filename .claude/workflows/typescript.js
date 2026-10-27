export const meta = {
  name: 'typescript',
  description: 'Rebuild one TypeScript package on TYPESCRIPT.md: its code, consumers, checks and catalog rows',
  whenToUse: 'One run per TYPESCRIPT.md [03] row, args.key its [KEY]; a run starts once the rows its [AFTER] cell names have landed and runs beside every other run, future-rust and future-swift included.',
  phases: [
    { title: 'Plan', detail: 'the package\'s shape from TYPESCRIPT.md, its catalog packages and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'adversarial read, then check-and-fix' },
  ],
}

const W = '/Users/bardiasamiee/.claude/projects/-Users-bardiasamiee-Developer-Rasm/host-ui-rework'
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${W}/future-langs/TYPESCRIPT.md`
const CHECK_TREES = ['tools/ast-grep/rules/typescript', 'tools/ast-grep/utils/typescript', 'biome.json', 'tsconfig.base.json']
const ROOT = ['package.json', 'pnpm-workspace.yaml', 'pnpm-lock.yaml', '.claude/skills/manage-repo/references/typescript.md']
const ATELIER = ['apps/creative-cloud/atelier/features', 'apps/creative-cloud/atelier/main.ts', 'apps/creative-cloud/atelier/routes.ts', 'apps/creative-cloud/atelier/status.ts', 'apps/creative-cloud/atelier/indesign-script.ts', 'apps/creative-cloud/atelier/scripts', 'apps/creative-cloud/atelier/package.json']
const PORTFOLIO = ['apps/web/portfolio', 'tests/typescript/apps/web/portfolio']
const lib = (key, ...consumers) => [`libs/typescript/${key}`, `tests/typescript/libs/${key}`, ...consumers]
const TARGETS = {
  'data/search': { title: '@rasm/search, with the check packages for the classes no check reports', paths: lib('data/search', 'libs/typescript/ai/mcp', 'libs/typescript/creative-cloud', 'libs/typescript/graphics/typography', ...ATELIER, 'plugins/argus', ...CHECK_TREES) },
  'ai/mcp': { title: '@rasm/mcp with its Node composition under ./node', paths: lib('ai/mcp', ...ATELIER, 'plugins/argus') },
  'creative-cloud/uxp': { title: '@rasm/uxp', paths: lib('creative-cloud/uxp', 'eng/typescript/uxp', 'libs/typescript/creative-cloud/indesign', 'apps/creative-cloud/atelier/scripts', 'vite.shared.ts', '.claude/skills/uxp-coding') },
  'creative-cloud/automation': { title: '@rasm/automation with one State class', paths: lib('creative-cloud/automation', ...ATELIER, 'libs/typescript/creative-cloud/indesign') },
  'graphics/typography': { title: '@rasm/typography', paths: lib('graphics/typography', ...ATELIER, 'libs/typescript/creative-cloud/indesign') },
  'creative-cloud/indesign': { title: '@rasm/indesign with the do-script layer taken from Atelier', paths: lib('creative-cloud/indesign', ...ATELIER, '.claude/skills/uxp-coding') },
  'data/database': { title: '@rasm/database with one engine per dialect', paths: lib('data/database', ...PORTFOLIO) },
  'web/hosting': { title: '@rasm/hosting', paths: lib('web/hosting', ...PORTFOLIO, 'tests/typescript/e2e-portfolio', 'infra/hosting.ts') },
  'web/media': { title: '@rasm/media', paths: lib('web/media', ...PORTFOLIO) },
  'web/ui': { title: '@rasm/ui with the components the portfolio writes around it', paths: lib('web/ui', ...PORTFOLIO, 'tests/typescript/e2e-portfolio') },
}
const { title, paths: owned } = TARGETS[args.key]
const paths = [...owned, ...ROOT]
const DIR = `${W}/future-langs/typescript/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const MEMORY = `${W.replace('/host-ui-rework', '')}/memory`
const TARGET = `Target: ${title}, row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: ${RULES} over all the target's code, with ${REPO}/CLAUDE.md and the memory file ${MEMORY}/unused-public-api-stays.md. The [03] row is the specification: the package owns what the row states, built on the catalog packages used fully, and holds nothing the row and the apps do not state. Code that exists is remade, never moved: current code and its callers have no authority, its idioms are the quality floor and not the architecture, every consumer the [03] row names, every package.json exports and imports entry and every skill row naming a module is rebuilt to the new form in this run, and counted code lines, files and exported names fall. Logic derives from a package member, a host value decoded through a Schema, or the structure a caller supplies, and a hand form beside a member that states it is a defect.`
const SOURCES = `Read package behavior from the pinned sources under ${REPO}/node_modules/.pnpm/<name>@<version>/node_modules/<name>/ (effect ships src; .d.ts elsewhere) through the search-code skill, cited by module path and source line, a compiler-forced form by its TS error code; the hosts from @adobe-uxp-types/uxp, indesign-uxp-types, the Cloudflare Workers types and the uxp-coding skill; the workspace rules from pnpm-workspace.yaml, tsconfig.base.json, biome.json and the manage-repo skill's TypeScript reference; the checks from tools/ast-grep/rules/typescript, biome.json and the language-service severities, which decide what code may not say; the Swift wire writers libs/swift/rpc, libs/swift/creative-cloud, libs/swift/macos/Applications, libs/swift/macos/AppleEvents and libs/swift/macos/Fonts read only.`
const CHECKS = `code moved as it stands, other than the moves the target title names; a consumer left on the old form; a form a package member, a Schema combinator or a check states; a Schema.Struct beside a reader, an exported schema constant, a literal table, a hand type restating a schema, a Bool or nullable standing for host state, an object literal standing for a class; a package used beside hand code it provides; a Reflect read, member-name string, bare Cause.UnknownError or thunk Effect.try; a comment other than a divider; a fixed value where a caller's option with a default belongs; app or vendor content in a library; an exported name no app operation composes, a wrapper or a forwarder; a wire spelling changed; a TYPESCRIPT.md or CLAUDE.md breach; counted lines, files or exported names that grow`
const PKG = { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, needs: { type: 'array', items: { type: 'string' } } }, required: ['id', 'title', 'section', 'files', 'needs'] }
const GRAPH = { type: 'object', properties: { packages: { type: 'array', items: PKG } }, required: ['packages'] }
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const author = await agent(`Read the target's [03] row, the package whole with every consumer of its exported names, its package.json exports and imports, and the package members TYPESCRIPT.md cites. ${TARGET}
${BASIS}
Write ${PLAN}: the target's shape after this work (one module per concept under libs/typescript/<group>/<package>/, each with what it holds), each concept's one class with its statics and the package member or host decode it depends on, a consumer call, every deletion and move; every exported name with the app operation that composes it and the exports entry that exposes it; the package.json rows (exports, imports, sideEffects, catalog rows) after; the wire spellings the target reads with their Swift writers; where the target's paths hold tools/ast-grep/rules/typescript, one check package for each class TYPESCRIPT.md [03] names and each recurring mistake the plan finds that no check reports, in the one tool that decides it (a tools/ast-grep/rules/typescript rule with its utils when syntax decides, a biome.json row when Biome owns the fact, a tsconfig.base.json severity row when the language service reports it, none when no tool decides); then packages, each with its files (consumers included), names, callers rebuilt, deletions and the earlier packages that supply names it reads. Close with the counted code lines (loc), files and exported names per package before and after. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarially refine ${PLAN} (author summary below) against the code, ${RULES} and the package sources. Find in every package: ${CHECKS}. ${TARGET} ${SOURCES}
Rewrite the plan in place so each package section is final. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add a name it reads).

${author}`, { label: 'challenge', phase: 'Plan', schema: GRAPH })

phase('Land')
const overlaps = (a, b) => a.files.some(f => b.files.some(g => f.startsWith(g) || g.startsWith(f)))
const land = (p, landed) => agent(`Land package ${p.id} (${p.title}) exactly as ${PLAN} section "${p.section}" states. Edit only these files: ${p.files.join(', ')}. ${BASIS} ${SOURCES}${landed.length ? `\nPackages it reads, landed:\n${landed.join('\n')}` : ''}
Return (structured): summary of what landed per plan item (at most 25 lines); files you edited.`, { label: `land:${p.id}`, phase: 'Land', schema: LANDED })
const results = await new Promise(resolve => {
  const ids = new Set(graph.packages.map(p => p.id))
  const queue = [...graph.packages], running = new Map(), done = new Map()
  const finish = () => resolve(graph.packages.map(q => ({ id: q.id, ...(done.get(q.id) || { summary: 'not started: its needs never landed', files: [] }) })))
  const pump = () => {
    for (const p of [...queue]) {
      if (!p.needs.every(n => done.has(n) || !ids.has(n)) || [...running.values()].some(r => overlaps(r, p))) continue
      queue.splice(queue.indexOf(p), 1)
      running.set(p.id, p)
      const landed = p.needs.filter(n => done.has(n)).map(n => `- ${n}: ${done.get(n).summary.slice(0, 600)}`)
      land(p, landed).then(r => r, () => null).then(r => {
        running.delete(p.id)
        done.set(p.id, r || { summary: 'agent died', files: [] })
        log(`${p.id} ${r ? 'landed' : 'died'}`)
        pump()
      })
    }
    if (running.size === 0) {
      if (queue.length) log(`${queue.map(p => p.id).join(', ')} never started: needs form a cycle`)
      finish()
    }
  }
  pump()
})

phase('Check')
const summaries = results.map(r => `- ${r.id}: ${r.summary.slice(0, 600)}`).join('\n')
const touched = [...new Set(results.flatMap(r => r.files))]
const adversary = await agent(`Adversarial agent. Packages landed:
${summaries}
Read these files against ${PLAN}: ${touched.join(', ')}. ${TARGET} ${BASIS} Find and fix in place: ${CHECKS}. Hunt across package seams: one concept in two packages, a decode written twice, a name deleted on its consumer count alone, a biome-ignore, a lowered severity or a disabled rule, a wire spelling changed against TYPESCRIPT.md item 15, an exports entry a consumer no longer finds, a test proving nothing. ${SOURCES}
Return (structured): defects fixed (at most 25 lines); files you edited.`, { label: 'adversary', phase: 'Check', schema: LANDED })
const files = [...new Set([...touched, ...(adversary ? adversary.files : [])])]
const built = await agent(`Check-and-fix. Packages landed, then the adversary's fixes:
${summaries}
- adversary: ${adversary ? adversary.summary.slice(0, 1500) : 'agent died'}
Run in ${REPO}: \`pnpm install --prefer-offline\` when a package.json or catalog row changed, \`nx run <project>:check\` for the target and every workspace package that depends on it (\`pnpm ls --filter '...<package name>' --depth -1\`), \`nx run rasm:typecheck:tsc\`, \`nx run rasm:lint:biome\`, \`nx run rasm:lint:ast-grep\`, \`ast-grep scan\` over ${files.join(', ')}, \`nx run <project>:test:integration\` for @rasm/database and @rasm/hosting when touched, \`nx run <project>:build\` for the portfolio and @rasm/atelier-indesign-script when touched, and \`nx run @rasm/e2e-portfolio:e2e\` when the portfolio changed. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a lowered severity, a biome-ignore or a @ts-expect-error); list each finding in other files with file and message. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, the counted code lines, files and exported names per package against the plan's numbers, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, adversary, built }
