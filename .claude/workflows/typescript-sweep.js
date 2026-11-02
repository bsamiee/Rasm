export const meta = {
  name: 'typescript-sweep',
  description: 'Rebuild every TypeScript project in place: readers per project, group readers over shared schemas and types, one cross-project lander, then per-project landers, reviewers and checks',
  whenToUse: 'Once no other run edits TypeScript; args.base the commit it starts from. It edits every TypeScript project, its guidance and the catalog rows they need, so nothing else touching TypeScript runs beside it; Rust runs may.',
  phases: [
    { title: 'Map', detail: 'the TypeScript projects and their groups' },
    { title: 'Read', detail: 'two readers per project, one inventory of its schemas, types and errors' },
    { title: 'Cross', detail: 'one reader per group over the inventories, then one lander across projects' },
    { title: 'Land', detail: 'per project: one adversarial lander, one diff reviewer, one fixer' },
    { title: 'Check', detail: 'every TypeScript check, the guidance that steered a defect, and surface.md' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/.artifacts/workflows/typescript-sweep`
const BASIS = `Load Skill(typescript-coding), Skill(typescript-coding-effect), Skill(clean-prose) and, for code running in an Adobe host, Skill(uxp-coding) first; they and ${REPO}/.claude/workflows/TYPESCRIPT.md decide every line. Each project is library-shaped code for software no one has written yet: present code and consumers hold no authority, and each consumer moves to the new form. A consumer states what it wants and gets a finished result: the library derives, defaults and composes internally whatever a caller would otherwise write, and the direct form is the only form. The domain's full capability under advanced use sits in the fewest public and internal names: one concept per module under one export, its type, value and operations sharing one name; a schema, type, enum, error or validation exists only where a caller acts on it differently, and the rest collapses into parameters, data or a value derived from its owner (host, package, declaration). Effect 4 and each dependency work through their full members (Effect source under node_modules/.pnpm/effect@*/node_modules/effect/src), in the newest syntax the compiler accepts, with flat control flow. Names, file names and every string an agent or person reads are domain terms in clean prose, messages built from their values. Code moved or extracted without becoming stronger is no change, and a new file, module or export stands only for a concept the base lacked. A change lands only when its result does more with less than its base, base commit ${args.base}. Hosts run in the background through Atelier's MCP tools, watch-apps and tools/harnesses; nothing activates an app or takes the owner's focus.`
const SILENT = 'Report only what changes code, each with file and line; say nothing about what holds.'
const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } }, steering: { type: 'array', items: { type: 'string' } } }, required: ['findings', 'steering'] }
const flat = (results) => results.filter(Boolean).flatMap((r) => r.findings.map((f) => `- ${f}`)).join('\n')

phase('Map')
const map = await agent(`List every TypeScript project in ${REPO}: each folder holding a package.json beside .ts or .tsx files under libs/typescript, apps, tools and tests (node_modules excluded), plus the root TypeScript files outside every package as one project named "root". Group them by the folder their capability shares (libs/typescript/<group>, an app with the libraries it composes, tools, tests). Return the projects with their group.`, { label: 'map', phase: 'Map', schema: { type: 'object', properties: { projects: { type: 'array', items: { type: 'object', properties: { path: { type: 'string' }, group: { type: 'string' } }, required: ['path', 'group'] } } }, required: ['projects'] } })
const projects = map?.projects ?? []

phase('Read')
const LENSES = [
  { lens: 'surface', ask: 'Each export no other module reads, each pair of exports one shared name or a static states, each schema, type, enum, error, validation, constant or helper no caller acts on differently, each step a consumer writes that the library could derive or compose, each stringly type or literal union a host or package type states, and each name, file name or agent-facing string that is coined or not clean prose.' },
  { lens: 'logic', ask: 'Each place Effect 4 or a dependency states what the code writes by hand, each shallow operation that advanced use breaks (malformed, partial, large or concurrent input, cancellation, every variant the domain holds), each deep nesting or flat run of near-identical statements, each error path losing its cause, and each consumer adapting or reshaping a library instead of calling it directly.' },
]
const readings = await parallel(projects.flatMap((p) => LENSES.map((l) => () => agent(`Reader for ${p.path}, ${l.lens} lens. Read it whole with its consumers and the dependency sources it calls. ${l.ask} Treat it as a shallow draft. ${SILENT} Edit nothing. Also return every schema, type, error and literal vocabulary it declares, one line each with file, name and shape, for the group reader. ${BASIS}
Return (structured): findings, the inventory, and each TYPESCRIPT.md or skill line that steered a defect, quoted (empty when none).`, { label: `read:${p.path}:${l.lens}`, phase: 'Read', schema: { ...FOUND, properties: { ...FOUND.properties, inventory: { type: 'array', items: { type: 'string' } } }, required: [...FOUND.required, 'inventory'] } }).then((r) => r && { ...r, path: p.path, group: p.group }))))
const of = (path) => readings.filter((r) => r?.path === path)

phase('Cross')
const groups = [...new Set(projects.map((p) => p.group))]
const crossed = await parallel(groups.map((g) => () => agent(`Group reader for ${g}. The inventories of its projects:
${readings.filter((r) => r?.group === g).flatMap((r) => r.inventory.map((i) => `- ${r.path}: ${i}`)).join('\n')}
Read the declarations they name. Name each schema, type, error or vocabulary two projects state nearly alike, and each one sitting at the wrong level (a fact every Adobe host shares stated per app, a host fact stated in a general package), with the one owner that should state it, the form it takes there, and every consumer that then derives from it. ${SILENT} Edit nothing. ${BASIS}
Return (structured): findings, and each TYPESCRIPT.md or skill line that steered a defect, quoted (empty when none).`, { label: `cross:${g}`, phase: 'Cross', schema: FOUND })))
const united = await agent(`Cross-project lander. Findings:
${flat(crossed)}
Judge each as an adversary: reject one that moves code without making it stronger, merges concepts that differ, or grows surface, each with one line why. Land the rest in their strongest form at the owner, rebuilding each consumer to derive from it and deleting every restatement. ${BASIS}
Return at most 20 lines: what landed with its owner, what you rejected and why, the files you edited.`, { label: 'cross:land', phase: 'Cross' })

phase('Land')
const landed = await pipeline(projects,
  (p) => agent(`Lander for ${p.path}. The cross-project landing already ran: ${united}
The readers' findings for this project:
${flat(of(p.path))}
Read the files as they now stand. Judge each finding as an adversary: reject a weak, cosmetic or regressive one, one that moves or extracts code without making it stronger, or one that grows surface without capability, each with one line why. Land the rest without relitigating them, in their strongest form, rebuilding a file whole where its findings reshape it, and move every consumer to the new form. Edit only ${p.path} and its consumers' call sites. ${BASIS}
Return at most 20 lines: what landed, what you rejected and why, the files you edited.`, { label: `land:${p.path}`, phase: 'Land' }),
  (report, p) => agent(`Reviewer for ${p.path}. Read \`git diff ${args.base} -- ${p.path}\` and the consumers it touched, then the files whole. Name each capability or behavior the base held that is now lost, each item added without an operation a consumer could not perform before, each moved or extracted piece that is no stronger, each remaining export, name, string or nesting the bar rejects, and each check suppression. The lander reported:
${report}
${SILENT} Edit nothing. ${BASIS}
Return (structured): findings, and each TYPESCRIPT.md or skill line that steered a defect, quoted (empty when none).`, { label: `review:${p.path}`, phase: 'Land', schema: FOUND }).then((r) => r && { ...r, path: p.path }),
  (review, p) => agent(`Fixer for ${p.path}. The reviewer's findings:
${flat([review])}
Judge each as the lander did and fix those that hold at their root, then run \`nx run <project>:check\` for this project and \`nx run rasm:lint:biome\` and \`nx run rasm:lint:ast-grep\` over the files this run edited here, fixing every finding at its root in code or the rule, never a lowered severity or a suppression. ${BASIS}
Return at most 10 lines: each command's result and each fix.`, { label: `fix:${p.path}`, phase: 'Land' }).then((built) => ({ path: p.path, review, built })),
)

phase('Check')
const steering = [...readings, ...crossed, ...landed.map((l) => l?.review)].filter(Boolean).flatMap((r) => r.steering)
const checked = await agent(`Final check. Fix each TYPESCRIPT.md or skill line named as steering a defect, as a general principle in the file's own form, surgically: ${steering.join(' | ') || 'none named'}.
Then run in ${REPO}: \`pnpm install --prefer-offline\` when a manifest changed, \`nx run-many -t check\` over the TypeScript projects, \`nx run rasm:typecheck:tsc\`, \`nx run rasm:lint:biome\`, \`nx run rasm:lint:ast-grep\`, and each build target. Fix every finding at its root. Measure lines and exports per file at ${args.base} and now, write ${DIR}/surface.md with both and the capability behind each growth, and remove growth that restates, wraps, moves or splits before you return. ${BASIS}
Return at most 20 lines: each command's result, each fix, exports and lines before and after, and \`git diff --shortstat ${args.base}\`.`, { label: 'check', phase: 'Check' })
return { projects: projects.length, united, landed: landed.filter(Boolean).map((l) => `${l.path}: ${l.built}`), checked }
