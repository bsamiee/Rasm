export const meta = {
  name: 'typescript-sweep',
  description: 'Rebuild every TypeScript project in place: readers per project, a reader and lander per group over shared declarations, then per project a lander and an adversarial second lander',
  whenToUse: 'Once no other run edits TypeScript; args.base the commit it starts from, args.exclude the folders a still-running run edits. It edits every TypeScript project, its guidance and the catalog rows they need, so nothing else touching TypeScript runs beside it; Rust runs may.',
  phases: [
    { title: 'Map', detail: 'the TypeScript projects and their groups' },
    { title: 'Read', detail: 'two readers per project with an inventory of its declarations' },
    { title: 'Group', detail: 'per group: a reader over shared declarations, then a lander' },
    { title: 'Land', detail: 'per project: a lander, then an adversarial second lander running its checks' },
    { title: 'Check', detail: 'every TypeScript check, the guidance that steered a defect, and surface.md' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/.artifacts/workflows/typescript-sweep`
const BASIS = `Load Skill(typescript-coding), Skill(typescript-coding-effect), Skill(clean-prose) and, for code running in an Adobe host, Skill(uxp-coding) first; they and ${REPO}/.claude/workflows/TYPESCRIPT.md decide every line. Each project is library-shaped code for software no one has written yet: present code and consumers hold no authority, and each consumer moves to the new form. A consumer states what it wants and gets a finished result: the library derives, defaults and composes internally whatever a caller would otherwise write, and the direct form is the only form. The domain's full capability under advanced use sits in the fewest public and internal names: one concept per module under one export, its type, value and operations sharing one name; a schema, type, enum, error or validation exists only where a caller acts on it differently, and the rest collapses into parameters, data or a value derived from its owner (host, package, declaration). Effect 4 and each dependency work through their full members (Effect source under node_modules/.pnpm/effect@*/node_modules/effect/src), in the newest syntax the compiler accepts, with flat control flow. Project files (package.json, tsconfig, tool configuration) hold only what the shared root configuration lacks, in current standard form, and imports follow one path form throughout. Names, file names and every string an agent or person reads are domain terms in clean prose, messages built from their values. Code moved or extracted without becoming stronger is no change, and a new file, module or export stands only for a concept the base lacked. A change lands only when its result does more with less than its base, base commit ${args.base}. Hosts run in the background through Atelier's MCP tools, watch-apps and tools/harnesses; nothing activates an app or takes the owner's focus.`
const SILENT = 'Report only what changes code, each with file and line; say nothing about what holds.'
const STEERING = 'and each TYPESCRIPT.md or skill line that steered a defect, quoted (empty when none)'
const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } }, steering: { type: 'array', items: { type: 'string' } } }, required: ['findings', 'steering'] }
const flat = (results) => results.filter(Boolean).flatMap((r) => r.findings.map((f) => `- ${f}`)).join('\n')

phase('Map')
const map = await agent(`List every TypeScript project in ${REPO}: each folder holding a package.json beside .ts or .tsx files under libs/typescript, apps, tools and tests (node_modules excluded), plus the root TypeScript files outside every package as one project named "root". Group them by the folder their capability shares (libs/typescript/<group>, an app with the libraries it composes, tools, tests). Return the projects with their group.`, { label: 'map', phase: 'Map', schema: { type: 'object', properties: { projects: { type: 'array', items: { type: 'object', properties: { path: { type: 'string' }, group: { type: 'string' } }, required: ['path', 'group'] } } }, required: ['projects'] } })
const projects = (map?.projects ?? []).filter(({ path }) => !(args.exclude ?? []).some((held) => path.startsWith(held)))

phase('Read')
const LENSES = [
  { lens: 'surface', ask: 'Where its public and internal surface, names, project files and prose fall short of the bar.' },
  { lens: 'logic', ask: 'Where its logic, its use of Effect and each dependency, and its errors fall short of the bar under advanced use.' },
]
const readings = await parallel(projects.flatMap((p) => LENSES.map((l) => () => agent(`Reader for ${p.path}, ${l.lens} lens. Read it whole with its consumers and the dependency sources it calls, as a shallow draft. ${l.ask} ${SILENT} Edit nothing. Also list every schema, type, error and literal set it declares, one line each with file, name and shape. ${BASIS}
Return (structured): findings, the inventory, ${STEERING}.`, { label: `read:${p.path}:${l.lens}`, phase: 'Read', schema: { ...FOUND, properties: { ...FOUND.properties, inventory: { type: 'array', items: { type: 'string' } } }, required: [...FOUND.required, 'inventory'] } }).then((r) => r && { ...r, path: p.path, group: p.group }))))
const of = (path) => readings.filter((r) => r?.path === path)

phase('Group')
const grouped = await pipeline([...new Set(projects.map((p) => p.group))],
  (g) => agent(`Group reader for ${g}. Its projects' inventories:
${readings.filter((r) => r?.group === g).flatMap((r) => r.inventory.map((i) => `- ${r.path}: ${i}`)).join('\n')}
Read the declarations they name. Find each declaration two projects state nearly alike and each one at the wrong level, with the one owner that states it, its form there, and each consumer that then derives from it. ${SILENT} Edit nothing. ${BASIS}
Return (structured): findings, ${STEERING}.`, { label: `group:${g}`, phase: 'Group', schema: FOUND }),
  (found, g) => agent(`Group lander for ${g}. The group reader's findings:
${flat([found])}
Land them without relitigating, and push back on any that moves code without making it stronger, merges concepts that differ, or grows surface, with one line why. State each declaration once at its owner, rebuild each consumer to derive from it, and delete every restatement. Other group landers edit beside you: re-read a file right before each edit. ${BASIS}
Return at most 15 lines: what landed with its owner, what you pushed back on and why, the files you edited.`, { label: `group:${g}:land`, phase: 'Group' }).then((report) => ({ group: g, found, report })),
)

phase('Land')
const landed = await pipeline(projects,
  (p) => agent(`Lander for ${p.path}. The group landers already ran:
${grouped.filter(Boolean).map((g) => `## ${g.group}\n${g.report}`).join('\n')}
The readers' findings for this project:
${flat(of(p.path))}
Read the files as they now stand. Land the findings without relitigating them, in their strongest form, rebuilding a file whole where its findings reshape it and moving every consumer to the new form. Push back on a weak, cosmetic or regressive finding, or one that moves code or grows surface without capability, with one line why. Other landers edit beside you: re-read a file right before each edit. ${BASIS}
Return at most 20 lines: what landed, what you pushed back on and why, the files you edited.`, { label: `land:${p.path}`, phase: 'Land' }),
  (report, p) => agent(`Second lander for ${p.path}, adversarial. Read \`git diff ${args.base} -- ${p.path}\`, the consumers it touched, and the files whole as a draft still short of the bar. The first lander reported:
${report}
Restore anything the base did better, then take the project further: collapse what remains of junk, indirection, wrappers, aliases and surface no operation needs, deepen logic advanced use breaks, and bring its project files and imports to current standard form. Each change is justified by a result that does more with less. Then run \`nx run <project>:check\` for this project, and \`nx run rasm:lint:biome\` and \`nx run rasm:lint:ast-grep\` over the files this run edited here, fixing every finding at its root in code or the rule, never a lowered severity or a suppression. ${BASIS}
Return (structured): findings as what you changed and why, ${STEERING}.`, { label: `deepen:${p.path}`, phase: 'Land', schema: FOUND }).then((r) => r && { ...r, path: p.path }),
)

phase('Check')
const steering = [...readings, ...grouped.map((g) => g?.found), ...landed].filter(Boolean).flatMap((r) => r.steering)
const checked = await agent(`Final check. Fix each TYPESCRIPT.md or skill line named as steering a defect, as a general principle in the file's own form, surgically: ${steering.join(' | ') || 'none named'}.
Add knip as a root check over every TypeScript project with each package's real entry points from its exports and the nx-inferred configs, so an export no other module reads fails the check. Then run in ${REPO}: \`pnpm install --prefer-offline\` when a manifest changed, the knip check, \`nx run-many -t check\` over the TypeScript projects, \`nx run rasm:typecheck:tsc\`, \`nx run rasm:lint:biome\`, \`nx run rasm:lint:ast-grep\`, and each build target. Fix every finding at its root. Measure lines and exports per file at ${args.base} and now, write ${DIR}/surface.md with both and the capability behind each growth, and remove growth that restates, wraps, moves or splits before you return. ${BASIS}
Return at most 20 lines: each command's result, each fix, exports and lines before and after, and \`git diff --shortstat ${args.base}\`.`, { label: 'check', phase: 'Check' })
return { projects: projects.length, grouped: grouped.filter(Boolean).map((g) => `${g.group}: ${g.report}`), landed: landed.filter(Boolean).map((l) => `${l.path}: ${l.findings.length}`), checked }
