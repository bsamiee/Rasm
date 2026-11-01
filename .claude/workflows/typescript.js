export const meta = {
  name: 'typescript',
  description: 'Rebuild one TypeScript package on TYPESCRIPT.md: its code, consumers, checks and catalog rows',
  whenToUse: 'One run per TYPESCRIPT.md [03] row, args.key its [KEY]. TypeScript runs go one at a time, each once the rows its [AFTER] cell names have landed, since every run edits package.json, pnpm-workspace.yaml, pnpm-lock.yaml, tools/ast-grep/rules/typescript and the manage-repo TypeScript reference; one rust and one swift run may go beside it.',
  phases: [
    { title: 'Plan', detail: 'parallel survey into a brief, then the package\'s shape from TYPESCRIPT.md, its catalog packages and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'per file group an adversary, then a ground-up rebuild, then check-and-fix' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${REPO}/.claude/workflows/TYPESCRIPT.md`
const ATELIER = ['apps/creative-cloud/atelier/features', 'apps/creative-cloud/atelier/main.ts', 'apps/creative-cloud/atelier/routes.ts', 'apps/creative-cloud/atelier/status.ts', 'apps/creative-cloud/atelier/indesign-script.ts', 'apps/creative-cloud/atelier/scripts', 'apps/creative-cloud/atelier/package.json']
const PORTFOLIO = ['apps/web/portfolio']
const lib = (key, ...consumers) => [`libs/typescript/${key}`, `tests/typescript/libs/${key}`, ...consumers]
const owned = {
  'data/search': lib('data/search', 'libs/typescript/ai/mcp', 'libs/typescript/creative-cloud', 'libs/typescript/graphics/typography', ...ATELIER, 'plugins/argus'),
  'ai/mcp': lib('ai/mcp', ...ATELIER, 'plugins/argus'),
  'creative-cloud/uxp': lib('creative-cloud/uxp', 'eng/typescript/uxp', 'libs/typescript/creative-cloud/indesign', 'apps/creative-cloud/atelier/scripts', 'vite.shared.ts', '.claude/skills/uxp-coding'),
  'creative-cloud/automation': lib('creative-cloud/automation', ...ATELIER, 'libs/typescript/creative-cloud/indesign'),
  'graphics/typography': lib('graphics/typography', ...ATELIER, 'libs/typescript/creative-cloud/indesign'),
  'creative-cloud/indesign': lib('creative-cloud/indesign', ...ATELIER, '.claude/skills/uxp-coding'),
  'data/database': lib('data/database', ...PORTFOLIO),
  'web/hosting': lib('web/hosting', ...PORTFOLIO, 'infra/hosting.ts'),
  'web/media': lib('web/media', ...PORTFOLIO),
  'web/ui': lib('web/ui', ...PORTFOLIO),
}[args.key]
const paths = [...owned, 'package.json', 'pnpm-workspace.yaml', 'pnpm-lock.yaml', 'tools/ast-grep/rules/typescript', 'tools/ast-grep/utils/typescript', 'biome.json', 'tsconfig.base.json', '.claude/skills/manage-repo/references/typescript.md']
const DIR = `${REPO}/.artifacts/workflows/typescript/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: Skill(typescript-coding), loaded first, with rules and forms that decide every line's shape, and ${RULES} over all target code. The [03] row is the specification: the package owns what it states, built on the catalog packages, Effect first, used fully, with nothing hand-rolled or hand-wrapped that a package provides. Current code is dismantled and rebuilt, never moved or shaped by its present form, and iterated until the package holds the fewest exported and internal names and types the row allows, and nothing the current code or its consumers do is lost. Software engineering practice and TypeScript's own patterns decide layout: a file holds one concept's declarations, package.json exports entries name concept files directly, no folder holds a single file, no folder sprawls flat with many small files, no file grows oversized, and no index.ts, barrel or main file collects or re-exports a package's logic. Present consumers name behavior to keep and never shape the surface: each public declaration serves the kind of operation the hosts' apps do, a declaration with no present caller stays, a weak one is rebuilt, and one parameterized declaration states what two would. A constant, type, record or helper exists only where it states a fact no package member, generic, derived value or argument with a default states, and a consumer reaches a variant by changing an argument, never by declaring or instantiating its own. Logic built poorly is deleted whole and rebuilt in its best form, never kept beside its replacement, wrapped, aliased or bridged. A concept sits where the architecture places it: libraries hold general capability any future app composes, apps hold their own content, and a misplaced concept is rebuilt in its new place, never relocated as the same code. Each package depends only on packages with members it calls, with no cycle and no edge a misplaced concept creates. A change improves code only when its result does more with less than its base: every declaration earns its place through an operation a caller performs, never through a rule, a row cell or the old code alone. A library and its consumers are rebuilt together and neither shapes the other: design starts from the domain and the operations consumers perform, the library states each in its strongest form, and each consumer calls it directly with no adapter, wrapper or reshaping. A run creates no test, and an existing test its rebuild breaks is rewritten onto the new surface, or deleted where the behavior it covers is gone, and never holds the code to its old shape. A rules citation names code at the base tree, and where an earlier run moved that code, the rule applies at its new location. Every consumer the [03] row names, every package.json exports and imports entry and every skill row naming a module is rebuilt to the new form in this run.`
const SOURCES = `Read package behavior from the pinned sources under ${REPO}/node_modules/.pnpm/<name>@<version>/node_modules/<name>/ (effect ships src; .d.ts elsewhere) through the search-code skill, cited by module path and source line, a compiler-forced form by its TS error code; the hosts from @adobe-uxp-types/uxp, indesign-uxp-types, the Cloudflare Workers types and the uxp-coding skill; the apps' kind of work from apps/ and docs/research/applications; the workspace rules from pnpm-workspace.yaml, tsconfig.base.json, biome.json, the manage-repo skill's TypeScript reference and ${REPO}/docs/research/coding/typescript/ (dependency-reimplementation.md, language-features.md, facts/); the checks from tools/ast-grep/rules/typescript, biome.json and the language-service severities, which decide what code may not say; the Swift wire writers under libs/swift and apps/creative-cloud/atelier/macos read only.`
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const BRIEF = `${DIR}/brief`
const GROUND = [
  { key: 'code', ask: `the base surface, measured on \`git archive HEAD\` of the target paths extracted under ${DIR}/base: per file and package, lines \`loc\` counts and declarations and exports \`ast-grep outline --json=stream\` lists under the language's outline rules in tools/ast-grep/outline; then every declaration of the target package with its file, line and purpose, every import site of each exported name in its consumers, and its package.json exports and imports` },
  { key: 'members', ask: 'the package members TYPESCRIPT.md cites and those the target\'s work reaches, read from the pinned sources: purpose and signature of each' },
  { key: 'wire', ask: 'every key, _tag, case and enum spelling the target reads or writes, with the Swift writer file and line on the other side' },
]
await parallel(GROUND.map(g => () => agent(`Survey for the rebuild of ${TARGET} Write ${BRIEF}/${g.key}.md: ${g.ask}. Facts with citations, no design. Edit nothing else. ${SOURCES}`, { label: `ground:${g.key}`, phase: 'Plan' })))
const author = await agent(`Read ${BRIEF}/*.md, the target's [03] row and the code where the design needs more than the brief states. ${TARGET}
${BASIS}
Write ${PLAN}: the target's final shape (modules grouped by concept under libs/typescript/<group>/<package>/, each with what it holds), each concept's one class with its statics and the package member or host decode it depends on, a consumer call, every deletion and move; every exported name with the app operation that composes it and the exports entry that exposes it; the package.json rows (exports, imports, sideEffects, catalog rows) after; the wire spellings the target reads with their Swift writers; then packages, each with its files (consumers included), names, callers rebuilt, deletions and the earlier packages that supply names it reads. Close with the exported names per package before and after, and every behavior of the current code with the name that keeps it. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarial architect. Read ${BRIEF}/*.md and ${PLAN}, and treat the plan as a draft to challenge: every module, file, exported name, dependency edge and package stands only where the [03] row, the brief's facts and the Basis support it, and what they do not support is rewritten in the plan, never annotated or listed for later. Check each claim the brief cites against the code or package source. ${TARGET}
${BASIS}
Rewrite the plan in place so each package section is final. package.json, pnpm-workspace.yaml, tools/ast-grep/rules/typescript, tools/ast-grep/utils/typescript, biome.json, tsconfig.base.json sit in packages that list no source file, source packages never queue on a shared file, and check configuration changes only where a rule misfires on correct code; no package lists a lock file, the Check phase resolves it. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add a name it reads).

Author summary:
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
const near = (a, b) => a.some(f => b.some(g => f.startsWith(g) || g.startsWith(f)))
const groups = results.filter(r => r.files.length > 0).reduce((all, r) => {
  const joined = all.filter(g => near(g.files, r.files))
  return [...all.filter(g => !joined.includes(g)), { ids: [r.id, ...joined.flatMap(g => g.ids)], files: [...new Set([...r.files, ...joined.flatMap(g => g.files)])] }]
}, [])
const reviews = (await pipeline(groups, (g, _item, i) => agent(`Adversarial reviewer of packages ${g.ids.join(', ')}. Edit only these files: ${g.files.join(', ')}. Trust nothing the land agents wrote: read each file against ${PLAN} and its base (\`git show HEAD:<path>\`). ${TARGET} ${BASIS}
Fix in place: code moved as it stands; lines, declarations or exports grown beyond what a capability the [03] row names requires; code a caller's operation does not need; an exported constant, record or struct where a parameter, a derived value or a package type states the fact; a consumer left on the old form; a form a package member, a Schema combinator or a check states; a Schema.Struct beside a reader, an exported schema constant, a literal table, a hand type restating a schema, a Bool or nullable standing for host state, an object literal standing for a class; a package used beside hand code it provides; a Reflect read, member-name string, bare Cause.UnknownError or thunk Effect.try; an error class off item 7's form (a reason that is a union, array or record, no subject, a hand message) or declared for a failure a host error, a package error or Unmatched states; a comment other than a divider; a fixed value where a caller's option with a default belongs; app or vendor content in a library; an exported name no app operation composes, a wrapper or a forwarder; a wire spelling changed; a TYPESCRIPT.md or CLAUDE.md breach; behavior the current code or a consumer has that the new code drops; capability the [03] row names left unbuilt. Where a stronger approach exists (an Effect or package member, a Schema combinator, an algebraic form, a parameter, a derived value), replace the file or block whole; extracting repeated code into a helper is no fix, and nothing poor survives softened or moved. Restore only what the base did objectively better. ${SOURCES}
Return (structured): defects fixed (at most 20 lines); each TYPESCRIPT.md, workflow or skill line that steered a defect, quoted (empty when none); files you edited.`, { label: `adversary:${i + 1}`, phase: 'Check', schema: { type: 'object', properties: { summary: { type: 'string' }, steering: { type: 'array', items: { type: 'string' } }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'steering', 'files'] } }), (first, g, i) => agent(`Ground-up rebuild of packages ${g.ids.join(', ')}, after their first adversary. Edit only these files: ${g.files.join(', ')}. For each file, work out what its callers do with it, then write the strongest form that serves exactly that: values derived from their owner, the fewest lines and members, flat control flow, no indirection, wrapper, forwarder or alias, and every package member used for what it states. Keep a rewrite only where it is objectively better than the file as it stands, with fewer lines or members and the same behavior and wire, and leave a file as it stands when nothing stronger exists. ${TARGET} ${BASIS} ${SOURCES}
Return (structured): what you rebuilt (at most 20 lines); each TYPESCRIPT.md, workflow or skill line that steered a defect, quoted (empty when none); files you edited.`, { label: `rebuild:${i + 1}`, phase: 'Check', schema: { type: 'object', properties: { summary: { type: 'string' }, steering: { type: 'array', items: { type: 'string' } }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'steering', 'files'] } }).then((second) => [first, second]))).flat().filter(Boolean)
const files = [...new Set([...touched, ...reviews.flatMap(r => r.files)])]
const built = await agent(`Check-and-fix. Packages landed, then the adversaries' fixes:
${summaries}
${reviews.map((r, i) => `- adversary ${i + 1}: ${r.summary.slice(0, 1200)}`).join('\n')}
Fix across package seams in place: one concept in two packages, a decode written twice, a name deleted on its consumer count alone, a biome-ignore, a lowered severity or a disabled rule, a wire spelling changed against TYPESCRIPT.md item 8, an exports entry a consumer no longer finds. Fix each TYPESCRIPT.md or .claude/skills/typescript-coding line the adversaries name as steering a defect, as a general principle: ${reviews.flatMap(r => r.steering).join(' | ') || 'none named'}.
Run in ${REPO}: \`pnpm install --prefer-offline\` when a package.json or catalog row changed, \`nx run <project>:check\` for the target and every workspace package that depends on it (\`pnpm ls --filter '...<package name>' --depth -1\`), \`nx run rasm:typecheck:tsc\`, \`nx run rasm:lint:biome -- <files>\` and \`nx run rasm:lint:ast-grep -- <files>\` over ${files.join(', ')}, \`nx run <project>:test:integration\` for @rasm/database and @rasm/hosting when touched, \`nx run <project>:build\` for the portfolio and @rasm/atelier-indesign-script when touched. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a lowered severity, a biome-ignore or a @ts-expect-error); list each finding in other files with file and message. Against the plan's behavior list and the [03] row, restore each behavior the landed code drops and build each capability left unbuilt, in place. Measure the working tree's surface the way the brief measured base and write ${DIR}/surface.md with base and after per file and package; each package ends at or below its base in lines and exports, growth stands only for a capability the base lacked entirely and is stated per item in surface.md, and growth from restating, wrapping or splitting is removed in place before you return. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, each behavior restored or capability built, the exported names per package before and after, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, reviews, built }
