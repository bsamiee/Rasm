export const meta = {
  name: 'python-tune',
  description: 'Mine committed Python into the Python skills and checks: readers, per skill a writer, adversary and clean-prose pass, per rule batch a writer, one check',
  whenToUse: 'args.base the commit before the Python work it mines, on a tree whose Python, skill and check paths match HEAD; no Python run commits beside it.',
  phases: [
    { title: 'Map', detail: 'every changed Python file in units; Python rules, utils, outline, refurb checks, checker tables; rule costs and scan times' },
    { title: 'Read', detail: 'a miner per unit of the diff, a mapper per check batch' },
    { title: 'Skills', detail: 'per skill a writer, an adversary rebuilding the changed content, a clean-prose pass' },
    { title: 'Rules', detail: 'per batch a writer acting on its map and the mined findings' },
    { title: 'Close', detail: 'checks once, fixes once, checks once, costs and scan times against the map' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const SKILLS = ['python-coding', 'python-coding-cyclopts', 'python-coding-expression', 'python-coding-msgspec', 'python-coding-pydantic', 'python-coding-telemetry', 'python-document']
const RANGE = `${args.base}..HEAD`
const PATHSPEC = "'*.py' '*.pyi' '*pyproject.toml' 'tools/ast-grep/*/python/*' 'tools/ast-grep/outline/python.yml'"
const SCAN = '`hyperfine -N -w 1 -r 3 --output=null "ast-grep scan --custom-languages allow --no-ignore hidden --warning --globs \'*.{py,pyi}\' --globs \'!tools/*/examples/**\'"`'
const REFURB = '`hyperfine -N -w 1 -r 3 --output=null "nx run rasm:lint:refurb --skip-nx-cache"`'
const COST = "the cost command in use-ast-grep's rule-building reference, `<globs>` `*.{py,pyi}`, reporting each id's user CPU beside `no-parse-error-python`"
const CHANGED = (paths) => `each file \`git status --short -- ${paths}\` lists`
const CHUNK = 12
const SPLIT = 3000
const RELEASE = 'the release the file is checked at: the `requires-python` of the member pyproject.toml holding it, a Ruff `per-file-target-version` row, else the first mise Python row'
const OWNERS = 'Each fact goes to the first owner that reports every instance of it soundly: checker configuration (a Ruff rule or option, a ty rule, a mypy error code, a FURB check), then an ast-grep rule for a form syntax decides, then a check under tools/refurb for a form that needs the types mypy resolves. An owner that reports a subset of another\'s instances does not own the fact. ty and mypy both run over the tree, and a fact both report is no overlap. A row that turns a Ruff rule, ty rule, mypy code or FURB check off stays while its reason holds: a reason naming a tool fact (a rewrite target, a crash, an owner that covers more) holds until the installed release changes it, a reason stating design policy holds always.'
const ACTIONS = ['keep', 'remove', 'rebuild', 'widen', 'tighten', 'rename', 'move to Ruff', 'move to ty or mypy', 'move to refurb', 'move to ast-grep']
const ASK = {
  rules: 'what it decides; which owner the ownership rule gives it; overlap or contradiction with another rule; whether syntax alone decides it soundly; its hits within the scopes its `python-from` or `python-below` metadata runs on (the `lint:ast-grep:*` commands `nx show project rasm --json` prints), split into true and false findings, and the forms it misses; whether its metadata matches the release the form needs; whether its cost stands above its peers\' in the costs below, and the walks that cause it; whether its id, file name and message are short, agent-facing and free of coined terms',
  utils: 'what each util and the outline decide, their callers, which walks dominate their callers\' cost, and each util no rule calls',
  refurb: 'what each check decides; which owner the ownership rule gives it; its findings on the tree split into true and false, and the forms it misses; what its tests cover; whether its name and message are short, agent-facing and free of coined terms',
  config: 'each row of the [tool.ruff*], [tool.ty*], [tool.mypy] and [tool.refurb] tables: what it decides, whether its reason holds by the ownership rule, rows restating a default, rows an ast-grep rule or refurb check restates, and Ruff rules, ty rules or mypy codes outside every off row that would replace an ast-grep rule or refurb check',
}
const LIST = { type: 'array', items: { type: 'string' } }
const UNITS = { type: 'object', properties: { units: { type: 'array', items: { type: 'object', properties: { name: { type: 'string' }, paths: LIST }, required: ['name', 'paths'] } } }, required: ['units'] }
const INVENTORY = { type: 'object', properties: { scan: { type: 'number' }, refurb: { type: 'number' }, baseline: { type: 'number' }, rules: { type: 'array', items: { type: 'object', properties: { path: { type: 'string' }, id: { type: 'string' }, user: { type: 'number' } }, required: ['path', 'id', 'user'] } }, utils: LIST, checks: LIST, configs: LIST }, required: ['scan', 'refurb', 'baseline', 'rules', 'utils', 'checks', 'configs'] }
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
const SCOPE = `Each fact belongs to the skill among ${SKILLS.join(', ')} whose frontmatter \`description\` covers it: a package member's own behavior to the skill naming that package, a design rule every package follows to python-coding, a fact two descriptions cover where it sits now. A bpy, RNA, operator or extension fact belongs to the blender-coding skills, which this run leaves as they are.`

phase('Map')
const [split, inventory] = await Promise.all([
  agent(`Partition every file \`git diff --name-only ${RANGE} -- ${PATHSPEC}\` lists into units: the files under the folder holding the nearest \`py.typed\`, \`blender_manifest.toml\` or member pyproject.toml (a \`[tool.uv.workspace] members\` row), a file under tests/python with the source folder its path mirrors, else the files under their top-level folder; split so no unit's diff exceeds ${SPLIT} lines. Every file falls in exactly one unit. Edit nothing. Return (structured) the units.`, { label: 'map:units', phase: 'Map', schema: UNITS }),
  agent(`Load Skill(use-ast-grep) and read its references/rule-building.md. List every rule file under tools/ast-grep/rules/python with its id; every file under tools/ast-grep/utils/python with tools/ast-grep/outline/python.yml; each module under tools/refurb and each test file under tests/python/tools/refurb as its own entry; and the root pyproject.toml with each member pyproject.toml \`[tool.uv.workspace] members\` names. Then, one after another, measure every rule with ${COST}, the whole scan with ${SCAN} and refurb with ${REFURB}. Edit nothing. Return (structured) the rules with their user CPU ms, \`no-parse-error-python\`'s user CPU ms as the baseline, the utils with the outline, the refurb modules and tests as checks, the pyproject.toml files as configs, and the whole scan's and refurb's mean ms.`, { label: 'map:checks', phase: 'Map', schema: INVENTORY }),
])
const rules = [...inventory.rules].sort((a, b) => a.path.localeCompare(b.path))
const costs = (held) => `\nUser CPU before, \`no-parse-error-python\` ${inventory.baseline} ms:\n${listed(held.map((r) => `${r.id} ${r.user} ms`))}`
const batches = [
  ...Array.from({ length: Math.ceil(rules.length / CHUNK) }, (_, i) => rules.slice(i * CHUNK, (i + 1) * CHUNK)).map((held, i) => ({ name: `rules-${i + 1}`, paths: held.map((r) => r.path), ask: ASK.rules, timing: costs(held) })),
  { name: 'utils', paths: inventory.utils, ask: ASK.utils, timing: '' },
  { name: 'refurb', paths: inventory.checks, ask: ASK.refurb, timing: '' },
  { name: 'config', paths: inventory.configs, ask: ASK.config, timing: '' },
]
const named = (name) => batches.find((b) => b.name === name)
const owner = (finding) => {
  const path = finding.replace(/#.*$/, '').replace(/\/$/, '')
  const folder = path.replace(/\/[^/]+\.\w+$/, '')
  return batches.find((b) => b.paths.includes(path))
    ?? (/^(tools\/refurb|tests\/python\/tools\/refurb)(\/|$)/.test(path) ? named('refurb') : undefined)
    ?? (/(^|\/)pyproject\.toml$/.test(path) ? named('config') : undefined)
    ?? (/^tools\/ast-grep\/(utils\/python|outline\/python\.yml)/.test(path) ? named('utils') : undefined)
    ?? batches.find((b) => b.name.startsWith('rules-') && b.paths.some((held) => held.startsWith(`${folder}/`)))
}

const mining = parallel(split.units.map((unit) => () => agent(`Load Skill(python-coding) and the skills it points to (${SKILLS.slice(1).join(', ')}) and read each one's references whole; for a unit under a Blender folder load Skill(blender-coding) as well. Read every hunk of \`git diff ${RANGE} -- ${unit.paths.join(' ')}\` with its files as of HEAD and keep only what is new or better than the skills and checks state. Better means: derived or algebraic logic where the hunk spelled out cases; less code with the same behavior and public capability; nesting reshaped into a \`match\`, a table or a comprehension, never a one-caller helper; a package or standard library member \`banned-api\` accepts in place of hand logic; a record, enum, union or typed tuple where a string, untyped \`dict\` or loose parameters stood, each case holding its own fields; a newer form (\`lazy\` imports, comprehension unpacking, \`frozendict\`, type parameter syntax, \`match\`, walrus) where it states the same fact in less code and ${RELEASE} has it, unless a per-file ignore keeps it out of that folder. A hunk showing a worse form, code that re-validates its own code, configuration or structure included, becomes a check or anti-pattern finding, never a snippet. Comment wording and divider labels are no finding. Before reporting a check finding, read the [tool.ruff*], [tool.ty*], [tool.mypy] and [tool.refurb] tables of pyproject.toml with their reasons and list the rule folder the form belongs in; a form a row turns off with a reason is no finding. ${SCOPE} Edit nothing. Return (structured), each with the code's file and line:
- skills: each form the code states better than a skill line or snippet, each rule the code follows that no skill states, and each skill line the package source at its locked release shows wrong (Skill(search-code), the source as evidence), with the owning skill and the file within it
- checks: each form a Python check misreports or misses, with path the rule file, the rule folder a new rule belongs in, the tools/refurb module, or the pyproject.toml holding the row`, { label: `mine:${unit.name}`, phase: 'Read', schema: MINED }))).then((found) => {
  log(`units with no miner result: ${split.units.filter((_, i) => !found[i]).map((unit) => unit.name).join(', ') || 'none'}`)
  return found.filter(Boolean)
})

const skills = mining.then((found) => {
  const held = SKILLS.filter((skill) => found.some((r) => r.skills.some((f) => f.skill === skill)))
  log(`skills with no mined finding, left as they are: ${SKILLS.filter((skill) => !held.includes(skill)).join(', ') || 'none'}`)
  return pipeline(held,
    (skill) => agent(`Load Skill(${skill}), Skill(search-code) and Skill(clean-prose), read ${skill}'s references whole, and read the SKILL.md of ${SKILLS.filter((other) => other !== skill).join(', ')} under .claude/skills with any reference of theirs a finding's topic touches. Writer for .claude/skills/${skill}. From these findings, replace content where the code shows a better form, rebuild each weak snippet in the skill to the strongest form its packages state at their locked release, add a snippet only for a finding's rule no snippet shows, and extend, merge or remove a section where the findings justify it. Each fact sits once within the skill, never in both SKILL.md and a reference. ${SCOPE} Delete a fact here only when another skill owns it and states it at HEAD (\`git show HEAD:<file>\`). Snippets couple to no library of this repository. Edit only .claude/skills/${skill}.
${listed(found.flatMap((r) => r.skills).filter((f) => f.skill === skill).map((f) => `${f.file}: ${f.finding}`))}
Return at most 10 lines: each section, snippet and line changed.`, { label: `skill:${skill}`, phase: 'Skills' }),
    (report, skill) => agent(`Load Skill(${skill}) and Skill(search-code), read its references whole and the SKILL.md of ${SKILLS.filter((other) => other !== skill).join(', ')}. Adversary for .claude/skills/${skill}. The writer reported:
${report ?? ''}
Read ${CHANGED(`.claude/skills/${skill}`)} against \`git show HEAD:<file>\`. Pipe each changed Python block once through \`ruff check --stdin-filename libs/python/snippet.py -\`. Rebuild in place each changed passage and snippet that is weaker than the code under libs/python, apps and tools, wrong against the package sources at their locked release, a form Ruff (\`banned-api\` included) reports, or a fact another skill owns. ${SCOPE} Edit only .claude/skills/${skill}. Return at most 6 lines: what you rebuilt.`, { label: `adversary:${skill}`, phase: 'Skills' }),
    (_, skill) => agent(`Load Skill(clean-prose) and read every file under its references whole. Apply it to each line ${CHANGED(`.claude/skills/${skill}`)} changes against HEAD and the passage around it, then run \`python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check .claude/skills/${skill}\` once and fix what it reports. Return at most 5 lines.`, { label: `prose:${skill}`, phase: 'Skills' }),
  )
})

const tuned = pipeline(batches,
  (batch) => agent(`Load Skill(use-ast-grep) and Skill(manage-repo). Map ${batch.paths.join(', ')}: for each file, ${batch.ask}. ${OWNERS} Edit nothing. Return (structured) one action per file, and for a pyproject.toml one per row with path \`<file>#<table>.<row>\`, each with the form it takes and the reason.${batch.timing}`, { label: `map:${batch.name}`, phase: 'Read', schema: MAPPED }),
  async (mapped, batch) => {
    const actions = (mapped?.actions ?? []).filter((a) => a.action !== 'keep').map((a) => `${a.path}: ${a.action}, ${a.reason}`)
    const mined = (await mining).flatMap((r) => r.checks)
    if (batch === batches[0]) log(`check findings with no batch: ${mined.filter((f) => !owner(f.path)).map((f) => f.path).join(', ') || 'none'}`)
    const found = mined.filter((f) => owner(f.path) === batch).map((f) => `${f.path}: ${f.finding}`)
    return actions.length + found.length === 0 ? null : agent(`Load Skill(use-ast-grep), Skill(manage-repo), Skill(python-coding) and Skill(clean-prose). Writer for ${batch.paths.join(', ')}. Act on the map:
${listed(actions)}
and the findings mined from the committed code:
${listed(found)}
${OWNERS} Delete each check about comments or dividers and each ast-grep rule syntax cannot decide soundly. A check reporting code that re-validates its own code, configuration or structure stays. Add a check only for a form the mined code shows and no Python check decides. A rule for a form one release introduces or retires sets \`metadata\` \`python-from\` or \`python-below\`. A refurb check follows the tools/refurb modules beside it and its cases join tests/python/tools/refurb. Ids, names and messages are short, agent-facing and free of coined terms. Record one before and one after hit state per rule you edit or add, as rule-building states; run no cost measurement and no other checker, the close runs them.${batch.timing}
Other writers edit tools/ast-grep, tools/refurb and the pyproject.toml files beside you; re-read a file before each edit. Edit nothing outside tools/ast-grep/{rules,utils}/python, tools/ast-grep/outline/python.yml, tools/refurb, tests/python/tools/refurb and the [tool.ruff*], [tool.ty*], [tool.mypy] and [tool.refurb] tables of ${inventory.configs.join(', ')}. Return at most 10 lines: each rule, check or row removed, renamed, changed or added, with its old and new id.`, { label: `rules:${batch.name}`, phase: 'Rules' })
  },
)

const [shaped, ruled] = await Promise.all([skills, tuned])

phase('Close')
const checked = await agent(`Load Skill(use-ast-grep), Skill(manage-repo) and Skill(python-coding), and read use-ast-grep's references/rule-building.md. The rule writers reported:
${ruled.filter(Boolean).join('\n\n')}
Update or delete every mention of an id or RASM code the writers renamed or removed, under .claude/skills and in pyproject.toml reasons. Run \`nx run rasm:lint:ruff\`, \`nx run rasm:typecheck:ty\`, \`nx run rasm:typecheck:mypy\`, \`nx run rasm:lint:refurb\`, \`nx run rasm:lint:ast-grep\`, \`nx run rasm:lint:yamllint\`, \`nx run rasm:lint:yamlfmt\` and \`nx run rasm:lint:tombi\` once, with \`pytest tests/python/tools/refurb\` when ${CHANGED('tools/refurb tests/python/tools/refurb')} holds any; then measure with ${COST} each rule ${CHANGED('tools/ast-grep/rules/python')} names and each rule calling a util ${CHANGED('tools/ast-grep/utils/python')} names, a removed id having no cost after; then time the whole scan with ${SCAN} and refurb with ${REFURB}. Fix every finding at its cause once (the code a finding names, or the check when it misfires; never a suppression, an ignore row or a lowered severity, except the rule-specific per-line ignore manage-repo's Python reference gives a typing provider's wrong or missing declaration), and rebuild once each changed rule whose user CPU exceeds twice \`no-parse-error-python\`'s. Then run the same checks, costs and timings once more. Return at most 10 lines: findings fixed, any that remain, and the times against ${inventory.scan} ms for the scan and ${inventory.refurb} ms for refurb before, with each changed rule's cost against its own.
${costs(rules)}`, { label: 'close', phase: 'Close' })
return { skills: shaped, rules: ruled, checked }
