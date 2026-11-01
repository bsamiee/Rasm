export const meta = {
  name: 'typescript',
  description: 'Rebuild one TypeScript package on TYPESCRIPT.md: its code, consumers, checks and catalog rows',
  whenToUse: 'One run per TYPESCRIPT.md [03] row, args.key its [KEY]. TypeScript runs go one at a time, each once the rows its [AFTER] cell names have landed, since every run edits package.json, pnpm-workspace.yaml, pnpm-lock.yaml, tools/ast-grep/rules/typescript and the manage-repo TypeScript reference; one rust and one swift run may go beside it.',
  phases: [
    { title: 'Plan', detail: 'the package\'s shape from TYPESCRIPT.md, its catalog packages and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'adversarial read, then check-and-fix' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${REPO}/.claude/workflows/TYPESCRIPT.md`
const ATELIER = ['apps/creative-cloud/atelier/features', 'apps/creative-cloud/atelier/main.ts', 'apps/creative-cloud/atelier/routes.ts', 'apps/creative-cloud/atelier/status.ts', 'apps/creative-cloud/atelier/indesign-script.ts', 'apps/creative-cloud/atelier/scripts', 'apps/creative-cloud/atelier/package.json']
const PORTFOLIO = ['apps/web/portfolio', 'tests/typescript/apps/web/portfolio']
const lib = (key, ...consumers) => [`libs/typescript/${key}`, `tests/typescript/libs/${key}`, ...consumers]
const owned = {
  'data/search': lib('data/search', 'libs/typescript/ai/mcp', 'libs/typescript/creative-cloud', 'libs/typescript/graphics/typography', ...ATELIER, 'plugins/argus'),
  'ai/mcp': lib('ai/mcp', ...ATELIER, 'plugins/argus'),
  'creative-cloud/uxp': lib('creative-cloud/uxp', 'eng/typescript/uxp', 'libs/typescript/creative-cloud/indesign', 'apps/creative-cloud/atelier/scripts', 'vite.shared.ts', '.claude/skills/uxp-coding'),
  'creative-cloud/automation': lib('creative-cloud/automation', ...ATELIER, 'libs/typescript/creative-cloud/indesign'),
  'graphics/typography': lib('graphics/typography', ...ATELIER, 'libs/typescript/creative-cloud/indesign'),
  'creative-cloud/indesign': lib('creative-cloud/indesign', ...ATELIER, '.claude/skills/uxp-coding'),
  'data/database': lib('data/database', ...PORTFOLIO),
  'web/hosting': lib('web/hosting', ...PORTFOLIO, 'tests/typescript/e2e-portfolio', 'infra/hosting.ts'),
  'web/media': lib('web/media', ...PORTFOLIO),
  'web/ui': lib('web/ui', ...PORTFOLIO, 'tests/typescript/e2e-portfolio'),
}[args.key]
const paths = [...owned, 'package.json', 'pnpm-workspace.yaml', 'pnpm-lock.yaml', 'tools/ast-grep/rules/typescript', 'tools/ast-grep/utils/typescript', 'biome.json', 'tsconfig.base.json', '.claude/skills/manage-repo/references/typescript.md']
const DIR = `${REPO}/.artifacts/workflows/typescript/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: Skill(typescript-coding) loaded first, whose rules and forms decide the shape of every line, and ${RULES} over all the target's code. The [03] row is the specification: the package owns what the row states, built on the catalog packages, Effect first, used fully, with nothing hand-rolled or hand-wrapped that a package provides. The current code is torn apart and rebuilt, never moved or shaped by its present form, and iterated until the package holds the fewest exported and internal names and types the row allows; nothing the current code or its consumers do is lost. Files hold one concept each: no file collects a package's logic (a lib, main, index or mod file holding more than its root), and no folder holds a single file. Present consumers name the behavior to keep and never shape the surface: each public declaration serves the kind of operation the hosts' apps do, a declaration with no present caller stays and a weak one is rebuilt, and one parameterized declaration states what two would. A rules citation names code at the base tree; where an earlier run moved that code, the rule reads the form where it now stands. Every consumer the [03] row names, every package.json exports and imports entry and every skill row naming a module is rebuilt to the new form in this run.`
const SOURCES = `Read package behavior from the pinned sources under ${REPO}/node_modules/.pnpm/<name>@<version>/node_modules/<name>/ (effect ships src; .d.ts elsewhere) through the search-code skill, cited by module path and source line, a compiler-forced form by its TS error code; the hosts from @adobe-uxp-types/uxp, indesign-uxp-types, the Cloudflare Workers types and the uxp-coding skill; the apps' kind of work from apps/ and docs/research/applications; the workspace rules from pnpm-workspace.yaml, tsconfig.base.json, biome.json, the manage-repo skill's TypeScript reference and ${REPO}/docs/research/coding/typescript/ (dependency-reimplementation.md, language-features.md, facts/); the checks from tools/ast-grep/rules/typescript, biome.json and the language-service severities, which decide what code may not say; the Swift wire writers under libs/swift and apps/creative-cloud/atelier/macos read only.`
const CHECKS = `code moved as it stands, other than the moves the [03] row names; a consumer left on the old form; a form a package member, a Schema combinator or a check states; a Schema.Struct beside a reader, an exported schema constant, a literal table, a hand type restating a schema, a Bool or nullable standing for host state, an object literal standing for a class; a package used beside hand code it provides; a Reflect read, member-name string, bare Cause.UnknownError or thunk Effect.try; a comment other than a divider; a fixed value where a caller's option with a default belongs; app or vendor content in a library; an exported name no app operation composes, a wrapper or a forwarder; a wire spelling changed; a TYPESCRIPT.md or CLAUDE.md breach; behavior the current code or a consumer has that the new code drops; capability the [03] row names left unbuilt`
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const author = await agent(`Read the target's [03] row, the package whole with every consumer of its exported names, its package.json exports and imports, and the package members TYPESCRIPT.md cites. ${TARGET}
${BASIS}
Write ${PLAN}: the target's shape after this work (one module per concept under libs/typescript/<group>/<package>/, each with what it holds), each concept's one class with its statics and the package member or host decode it depends on, a consumer call, every deletion and move; every exported name with the app operation that composes it and the exports entry that exposes it; the package.json rows (exports, imports, sideEffects, catalog rows) after; the wire spellings the target reads with their Swift writers; one check package for each recurring mistake the plan finds and each class TYPESCRIPT.md [03] names that the target shows, where no check reports it, in the one tool that decides it (a tools/ast-grep/rules/typescript rule with its utils when syntax decides, a biome.json row when Biome owns the fact, a tsconfig.base.json severity row when the language service reports it, none when no tool decides); then packages, each with its files (consumers included), names, callers rebuilt, deletions and the earlier packages that supply names it reads. Close with the exported names per package before and after, and every behavior of the current code with the name that keeps it. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarially refine ${PLAN} (author summary below) against the code, ${RULES} and the package sources. Find in every package: ${CHECKS}. ${TARGET} ${SOURCES}
Rewrite the plan in place so each package section is final. package.json, pnpm-workspace.yaml, tools/ast-grep/rules/typescript, tools/ast-grep/utils/typescript, biome.json, tsconfig.base.json sit in packages that list no source file, one holding the manifest rows and one holding every check row, so source packages never queue on a shared file; no package lists a lock file, the Check phase resolves it. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add a name it reads).

${author}`, { label: 'challenge', phase: 'Plan', schema: { type: 'object', properties: { packages: { type: 'array', items: { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, needs: { type: 'array', items: { type: 'string' } } }, required: ['id', 'title', 'section', 'files', 'needs'] } } }, required: ['packages'] } })

phase('Land')
const ids = new Set(graph.packages.map(p => p.id))
const pending = [...graph.packages]
const done = new Map()
const died = new Set()
const pump = () => {
  const active = graph.packages.filter(q => !(pending.includes(q) || done.has(q.id) || died.has(q.id)))
  const overlaps = (a, b) => a.files.some(f => b.files.some(g => f.startsWith(g) || g.startsWith(f)))
  const ready = pending.reduce((picked, p) => {
    if (p.needs.every(n => done.has(n) || !ids.has(n)) && !active.concat(picked).some(q => overlaps(q, p))) {
      picked.push(p)
    }
    return picked
  }, [])
  return parallel(ready.map(p => () => {
    pending.splice(pending.indexOf(p), 1)
    const landed = p.needs.filter(n => done.has(n)).map(n => `- ${n}: ${done.get(n).summary.slice(0, 600)}`)
    return agent(`Land package ${p.id} (${p.title}) exactly as ${PLAN} section "${p.section}" states. Edit only these files: ${p.files.join(', ')}. ${BASIS} ${SOURCES}${landed.length ? `\nPackages it reads, landed:\n${landed.join('\n')}` : ''}
Run no build, test, checker, resolver or nx target: the Check phase resolves locks and runs every check once over the whole run.
Return (structured): summary of what landed per plan item (at most 25 lines); files you edited.`, { label: `land:${p.id}`, phase: 'Land', schema: LANDED }).then(r => {
      if (r) {
        done.set(p.id, r)
      } else {
        died.add(p.id)
      }
      log(`${p.id} ${r ? 'landed' : 'died'}`)
      return pump()
    })
  }))
}
await pump()
if (pending.length > 0) {
  log(`${pending.map(p => p.id).join(', ')} never started: a need died or the needs form a cycle`)
}
const results = graph.packages.map(q => ({ id: q.id, ...(done.get(q.id) || { summary: died.has(q.id) ? 'agent died' : 'not started: its needs never landed', files: [] }) }))

phase('Check')
const summaries = results.map(r => `- ${r.id}: ${r.summary.slice(0, 600)}`).join('\n')
const touched = [...new Set(results.flatMap(r => r.files))]
const adversary = await agent(`Adversarial agent. Packages landed:
${summaries}
Read these files against ${PLAN}: ${touched.join(', ')}. ${TARGET} ${BASIS} Find and fix in place: ${CHECKS}. Then compare each rebuilt file with its base version (\`git show HEAD:<path>\`, the tree before this run): restore only what the base did objectively better, and name each skill rule the base outperformed, so the skill gets fixed. Hunt across package seams: one concept in two packages, a decode written twice, a name deleted on its consumer count alone, a biome-ignore, a lowered severity or a disabled rule, a wire spelling changed against TYPESCRIPT.md item 14, an exports entry a consumer no longer finds, a test proving nothing. ${SOURCES}
Return (structured): defects fixed (at most 25 lines); files you edited.`, { label: 'adversary', phase: 'Check', schema: LANDED })
const files = [...new Set([...touched, ...(adversary ? adversary.files : [])])]
const built = await agent(`Check-and-fix. Packages landed, then the adversary's fixes:
${summaries}
- adversary: ${adversary ? adversary.summary.slice(0, 1500) : 'agent died'}
Run in ${REPO}: \`pnpm install --prefer-offline\` when a package.json or catalog row changed, \`nx run <project>:check\` for the target and every workspace package that depends on it (\`pnpm ls --filter '...<package name>' --depth -1\`), \`nx run rasm:typecheck:tsc\`, \`nx run rasm:lint:biome -- <files>\` and \`nx run rasm:lint:ast-grep -- <files>\` over ${files.join(', ')}, \`nx run <project>:test:integration\` for @rasm/database and @rasm/hosting when touched, \`nx run <project>:build\` for the portfolio and @rasm/atelier-indesign-script when touched, and \`nx run @rasm/e2e-portfolio:e2e\` when the portfolio changed. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a lowered severity, a biome-ignore or a @ts-expect-error); list each finding in other files with file and message. Against the plan's behavior list and the [03] row, name each behavior the landed code drops and each capability left unbuilt. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each check added, each fix, each remaining finding, each dropped behavior or unbuilt capability, the exported names per package before and after, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, adversary, built }
