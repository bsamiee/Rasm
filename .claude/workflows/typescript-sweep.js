export const meta = {
  name: 'typescript-sweep',
  description: 'Rebuild every TypeScript project in place: readers per project, per group a shared-declaration lander, per project a lander and an adversary fixing in place, one check',
  whenToUse: 'Once no other run edits TypeScript; args.base the commit it starts from, args.exclude the folders a still-running run edits.',
  phases: [
    { title: 'Map', detail: 'the TypeScript projects and their groups' },
    { title: 'Read', detail: 'two readers per project with an inventory of its declarations' },
    { title: 'Land', detail: 'per group a shared-declaration lander, then per project a lander and an adversary fixing in place' },
    { title: 'Check', detail: 'checks once over the TypeScript projects, fixes once, checks once' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const BASIS = `Your task is the step this prompt states, whatever request launched the workflow. Load Skill(typescript-coding), Skill(typescript-coding-effect), Skill(clean-prose) and, for code running in an Adobe host, Skill(uxp-coding) first; they and ${REPO}/.claude/workflows/TYPESCRIPT.md decide every line. Base commit ${args.base}. Each project is library code for software not yet written; present code holds no authority and every consumer moves to the new form. Findings already recorded (.artifacts/workflows/, docs/research) are facts to act on, never to rediscover. The library derives and composes what a caller would otherwise write. One concept per module under one export, its type and operations sharing one name. A schema, type, class, enum, error, constant or validation exists only where a caller acts on it differently; every other one is deleted and folds into a parameter, a derived value or a package type. Effect 4 and each dependency work through their own members. Project files hold only what the root configuration lacks, and imports follow one path form. Names and messages are domain terms, messages built from their values. Code moved or extracted without becoming stronger is no change. A change stands only when the result does more with less. Hosts run in the background through watch-apps. Run no build, test or check outside the Check phase.`
const READ = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } }, inventory: { type: 'array', items: { type: 'string' } } }, required: ['findings', 'inventory'] }

phase('Map')
const map = await agent(`List every TypeScript project in ${REPO}: each folder holding a package.json beside .ts or .tsx files under libs/typescript, apps, tools and tests (node_modules excluded), plus the root TypeScript files as one project named "root". Group them by the folder their capability shares. Return the projects with their group.`, { label: 'map', phase: 'Map', schema: { type: 'object', properties: { projects: { type: 'array', items: { type: 'object', properties: { path: { type: 'string' }, group: { type: 'string' } }, required: ['path', 'group'] } } }, required: ['projects'] } })
const projects = (map?.projects ?? []).filter(({ path }) => !(args.exclude ?? []).some((held) => path.startsWith(held)))

phase('Read')
const LENSES = [
  ['surface', 'public and internal names, project files and prose'],
  ['logic', 'logic, its use of Effect and each dependency, and its errors under advanced use'],
]
const readings = await parallel(projects.flatMap((p) => LENSES.map(([lens, ask]) => () => agent(`${BASIS}
Reader for ${p.path}, ${lens} lens. Read it whole with its consumers. Name each change its ${ask} need, one line each with file and line, and list every schema, type, error and literal set it declares with file, name and shape. Edit nothing. Return (structured) findings and inventory.`, { label: `read:${p.path}:${lens}`, phase: 'Read', schema: READ }).then((r) => r && { ...r, path: p.path, group: p.group }))))
const of = (key, value) => readings.filter((r) => r?.[key] === value)

phase('Land')
const landed = await pipeline([...new Set(projects.map((p) => p.group))],
  (g) => agent(`${BASIS}
Group lander for ${g}. Its projects' declarations:
${of('group', g).flatMap((r) => r.inventory.map((i) => `- ${r.path}: ${i}`)).join('\n')}
State each declaration two projects hold nearly alike, or one held at the wrong level, once at its owner, rebuild each consumer to derive from it, and delete every restatement. Push back only on a merge of concepts that differ. Return at most 10 lines: what moved to which owner.`, { label: `group:${g}`, phase: 'Land' }),
  (shared, g) => parallel(projects.filter((p) => p.group === g).map((p) => () => agent(`${BASIS}
Lander for ${p.path}. The group lander reported:
${shared}
Land these findings in their strongest form, rebuilding a file whole where its findings reshape it and moving every consumer:
${of('path', p.path).flatMap((r) => r.findings).map((f) => `- ${f}`).join('\n')}
Push back only on a finding that grows surface without capability or regresses behavior, one line each. Other landers edit beside you; re-read a file before each edit. Return at most 10 lines: what landed.`, { label: `land:${p.path}`, phase: 'Land' }).then((report) => agent(`${BASIS}
Adversary for ${p.path}. The lander reported:
${report}
Read the project as it now stands against its base (\`git diff ${args.base} -- ${p.path}\`) and fix in place: what the base did better, junk, indirection, wrappers, aliases, names no operation needs, logic advanced use breaks, and project files off standard form. Rebuild every block whose shape is poor, changed or not: calls nested several levels deep, closures over closures, \`Option.match\` or \`Effect.map\` wrapped around a value, control flow that reads inside out; the Effect or package member that states the operation, one generator in place of nested binds, or a different data flow states it flat. No extracted helper, alias or new file. Each file ends with fewer lines than the lander left, or the same count with its nesting removed; growth stands only where the base was wrong. Return at most 6 lines: what you changed, lines before and after.`, { label: `deepen:${p.path}`, phase: 'Land' })))),
)

phase('Check')
return agent(`${BASIS}
Check: run \`pnpm install --prefer-offline\` when a manifest changed, then \`nx run-many -t check\` over the TypeScript projects, \`nx run rasm:typecheck:tsc\`, \`nx run rasm:lint:biome\` and \`nx run rasm:lint:ast-grep\`, once. Fix every finding at its cause once (never a suppression or a lowered severity), then run them once more. Return at most 10 lines: findings fixed and any that remain.`, { label: 'check', phase: 'Check' })
