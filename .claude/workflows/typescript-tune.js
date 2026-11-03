export const meta = {
  name: 'typescript-tune',
  description: 'Mine committed TypeScript into the TypeScript skills and checks: readers, per skill a writer, adversary and clean-prose pass, per rule batch a writer, one check',
  whenToUse: 'args.base the commit before the TypeScript work it mines; args.exclude the paths runs still edit; no TypeScript run lands beside it.',
  phases: [
    { title: 'Map', detail: 'every changed file in units, TypeScript rules and utils, scan time per rule' },
    { title: 'Read', detail: 'a miner per unit of the diff, a mapper per rule batch, utils and config' },
    { title: 'Skills', detail: 'per skill a writer, an adversary rebuilding the changed content, a clean-prose pass' },
    { title: 'Rules', detail: 'per batch a writer acting on its map and the mined findings' },
    { title: 'Close', detail: 'checks once, fixes once, checks once, scan time against the map' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const SKILLS = ['typescript-coding', 'typescript-coding-effect', 'uxp-coding']
const RANGE = `${args.base}..HEAD`
const SCAN = `ast-grep scan --custom-languages allow --no-ignore hidden --globs '*.{ts,tsx,js}' --globs '!tools/*/examples/**'`
const TIME = (filter) => `\`hyperfine -N -i -w 1 -r 3 --output=null "${SCAN}${filter}"\``
const BY_ID = " --filter '^<id>$'"
const CHUNK = 12
const SPLIT = 3000
const ACTIONS = ['keep', 'remove', 'rebuild', 'widen', 'tighten', 'rename', 'move to Biome', 'move to the compiler']
const ASK = {
  rules: 'what it decides; whether Biome or the compiler (tsconfig.base.json with its @effect/language-service diagnostics) already decides it; overlap or contradiction with another rule; whether syntax alone decides it soundly; its hits on the tree split into true and false findings, and the forms it misses; the walks that make it slower than its peers in the times below; whether its id, file name and message are short, agent-facing and free of coined terms',
  utils: 'what each util and the outline decide, their callers, the walks that make them slow, and each util no rule calls',
  config: 'each TypeScript row: what it decides, rows restating a default or an ast-grep rule, and Biome rules or compiler options the tree passes that would replace an ast-grep rule',
}
const LIST = { type: 'array', items: { type: 'string' } }
const MAP = { type: 'object', properties: { units: { type: 'array', items: { type: 'object', properties: { name: { type: 'string' }, paths: LIST }, required: ['name', 'paths'] } }, scan: { type: 'number' }, rules: { type: 'array', items: { type: 'object', properties: { path: { type: 'string' }, id: { type: 'string' }, ms: { type: 'number' } }, required: ['path', 'id', 'ms'] } }, utils: LIST }, required: ['units', 'scan', 'rules', 'utils'] }
const MINED = {
  type: 'object',
  properties: {
    skills: { type: 'array', items: { type: 'object', properties: { skill: { enum: SKILLS }, file: { type: 'string' }, finding: { type: 'string' } }, required: ['skill', 'file', 'finding'] } },
    checks: { type: 'array', items: { type: 'object', properties: { path: { type: 'string' }, finding: { type: 'string' } }, required: ['path', 'finding'] } },
  },
  required: ['skills', 'checks'],
}
const MAPPED = { type: 'object', properties: { actions: { type: 'array', items: { type: 'object', properties: { path: { type: 'string' }, action: { enum: ACTIONS }, reason: { type: 'string' } }, required: ['path', 'action', 'reason'] } } }, required: ['actions'] }
const listed = (lines) => lines.map((line) => `- ${line}`).join('\n')

phase('Map')
const map = await agent(`Partition every file \`git diff --name-only ${RANGE}\` lists into units, each the files under the folder holding the project file nearest them, else under their top-level folder, split so no unit's diff exceeds ${SPLIT} lines; every file falls in exactly one unit. List every rule file under tools/ast-grep/rules/typescript with its id, and every file under tools/ast-grep/utils/typescript with tools/ast-grep/outline/typescript.yml. Then time the whole scan with ${TIME('')} and each rule with ${TIME(BY_ID)}, one after another. Edit nothing. Return (structured) the units, the rules with their mean ms, the utils and the whole scan's mean ms.`, { label: 'map', phase: 'Map', schema: MAP })
const rules = [...(map?.rules ?? [])].sort((a, b) => a.path.localeCompare(b.path))
const batches = [
  ...Array.from({ length: Math.ceil(rules.length / CHUNK) }, (_, i) => rules.slice(i * CHUNK, (i + 1) * CHUNK)).map((held, i) => ({ name: `rules-${i + 1}`, paths: held.map((r) => r.path), ask: ASK.rules, timing: `\nTimes before, whole scan ${map.scan} ms:\n${listed(held.map((r) => `${r.id} ${r.ms} ms`))}` })),
  { name: 'utils', paths: map?.utils ?? [], ask: ASK.utils, timing: '' },
  { name: 'config', paths: ['biome.json', 'tsconfig.base.json', 'sgconfig.yml'], ask: ASK.config, timing: '' },
]
const owner = (path) => batches.find((b) => b.paths.includes(path)) ?? batches.find((b) => b.paths.some((held) => held.startsWith(`${path.replace(/\/$/, '')}/`))) ?? batches[0]

const mining = parallel((map?.units ?? []).map((unit) => () => agent(`Load Skill(typescript-coding), Skill(typescript-coding-effect) and Skill(uxp-coding) and read each one's references whole. Read every hunk of \`git diff ${RANGE} -- ${unit.paths.join(' ')}\` with its files as of HEAD and keep only what is new or better than the skills and checks state, better meaning stronger derived or algebraic logic, less code and surface or a removal, nesting solved flat, an Effect 4 or package member in place of hand logic, or richer objects in place of type and string clutter, and a hunk showing a worse form becomes a check or anti-pattern finding, never a snippet. Edit nothing. Return (structured), each with the code's file and line:
- skills: each form the code states better than a skill line or snippet, each rule the code follows that no skill states, and each skill line the code shows wrong, with the skill and file that own the fact
- checks: each form a TypeScript check misreports or misses, with path the rule file, the rule folder a new rule belongs in, biome.json or tsconfig.base.json`, { label: `mine:${unit.name}`, phase: 'Read', schema: MINED }))).then((found) => found.filter(Boolean))

const skills = mining.then((found) => pipeline(SKILLS,
  (skill) => agent(`Load Skill(${skill}) and Skill(clean-prose), read ${skill}'s references whole, and read ${SKILLS.filter((other) => other !== skill).map((other) => `.claude/skills/${other}`).join(' and ')} whole. Writer for .claude/skills/${skill}. From these findings, replace content where the code shows a better form, rebuild each weak snippet in the skill to the strongest form its packages state, add a snippet only where no snippet shows the rule, and extend, merge or remove a section where the findings justify it. Each fact sits once, in the file that owns it: none repeats between SKILL.md and a reference or across the three skills, and a fact another of them owns leaves this one. Snippets couple to no library of this repository. Edit only .claude/skills/${skill}.
${listed(found.flatMap((r) => r.skills).filter((f) => f.skill === skill).map((f) => `${f.file}: ${f.finding}`))}
Return at most 10 lines: each section, snippet and line changed.`, { label: `skill:${skill}`, phase: 'Skills' }),
  (report, skill) => agent(`Load Skill(${skill}) and read its references whole. Adversary for .claude/skills/${skill}. The writer reported:
${report ?? ''}
Rebuild in place each passage and snippet \`git diff HEAD -- .claude/skills/${skill}\` changes that is weaker than the code under libs/typescript and apps, wrong against the package sources, stated in another file of the three TypeScript skills, or outside the file that owns it. Edit only .claude/skills/${skill}. Return at most 6 lines: what you rebuilt.`, { label: `adversary:${skill}`, phase: 'Skills' }),
  (_, skill) => agent(`Load Skill(clean-prose) and read every file under its references whole. Apply it to each line \`git diff HEAD -- .claude/skills/${skill}\` changes and the passage around it, then run \`python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check .claude/skills/${skill}\` once and fix what it reports. Return at most 5 lines.`, { label: `prose:${skill}`, phase: 'Skills' }),
))

const tuned = pipeline(batches,
  (batch) => agent(`Load Skill(use-ast-grep) and Skill(manage-repo). Map ${batch.paths.join(', ')}: for each file, ${batch.ask}. Time a rule with ${TIME(BY_ID)}. Edit nothing. Return (structured) one action per file with the form it takes and the reason.${batch.timing}`, { label: `map:${batch.name}`, phase: 'Read', schema: MAPPED }),
  async (mapped, batch) => {
    const actions = (mapped?.actions ?? []).filter((a) => a.action !== 'keep').map((a) => `${a.path}: ${a.action}, ${a.reason}`)
    const found = (await mining).flatMap((r) => r.checks).filter((f) => owner(f.path) === batch).map((f) => `${f.path}: ${f.finding}`)
    return actions.length + found.length === 0 ? null : agent(`Load Skill(use-ast-grep), Skill(manage-repo) and Skill(clean-prose). Writer for ${batch.paths.join(', ')}. Act on the map:
${listed(actions)}
and the findings mined from the committed code:
${listed(found)}
Each fact goes to one owner: a Biome row or compiler option when either decides it, an ast-grep rule only where neither can, with no overlap or contradiction between owners. Delete each rule about comments or dividers and each rule syntax cannot decide soundly. Add a rule only for a form the mined code shows and no rule under tools/ast-grep/rules/typescript decides. Ids, file names and messages are short, agent-facing and free of coined terms. Each changed rule scans no slower than before by ${TIME(BY_ID)}.${batch.timing}
Other writers edit tools/ast-grep beside you; re-read a file before each edit. Edit nothing outside tools/ast-grep, biome.json, tsconfig.base.json and sgconfig.yml. Return at most 10 lines: each rule or row removed, changed or added, with its time before and after.`, { label: `rules:${batch.name}`, phase: 'Rules' })
  },
)

const [shaped, ruled] = await Promise.all([skills, tuned])

phase('Close')
const checked = await agent(`Load Skill(use-ast-grep). The rule writers reported:
${ruled.filter(Boolean).join('\n\n')}
Run \`nx run rasm:lint:ast-grep\`, \`nx run rasm:lint:biome\` and \`nx run-many -t typecheck:tsc\` once, fix every finding at its cause once (the code a finding names, or the rule when it misfires; never a suppression or a lowered severity), then run them once more. Leave findings under ${(args.exclude ?? []).join(', ') || 'no path'}: runs still editing them fix their own. Then time the whole scan with ${TIME('')}. Return at most 10 lines: findings fixed, any that remain, and the scan time against ${map?.scan} ms before.`, { label: 'close', phase: 'Close' })
return { skills: shaped, rules: ruled, checked }
