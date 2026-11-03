export const meta = {
  name: 'tune',
  description: 'Mine one language\'s committed code into its skills and checks: miners, a writer and adversary per skill and per check batch, one close',
  whenToUse: 'args.language a languages.js key, args.base the commit before the mined work, args.exclude paths other runs edit; skill and check paths committed at launch',
  phases: [
    { title: 'Map', detail: 'changed files in units; rules, utils, outline, typed checks and config files with the baseline rule's cost' },
    { title: 'Read', detail: 'a miner per diff unit, a mapper per check batch' },
    { title: 'Skills', detail: 'per skill with findings a writer, an adversary over its diff and whole, a clean-prose pass' },
    { title: 'Rules', detail: 'per batch a writer and an adversary, the util, typed checker and config writers after every other batch' },
    { title: 'Close', detail: 'renamed ids, owning targets once, rule costs against the baseline, fixes once, targets once more' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const { hosts, languages } = await workflow('languages')
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const BAR = `Read ${REPO}/.claude/workflows/BAR.md first.`
const PROSE = `python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check`
const SPLIT = 3000
const CHUNK = 12

// --- [MODELS] --------------------------------------------------------------------------

const TEXT = { type: 'string' }
const LIST = { type: 'array', items: TEXT }

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ARGS]
const relative = (path) => path.replace(`${REPO}/`, '').replace(/^\.\//, '').replace(/#.*$/, '').replace(/\/+$/, '')
const within = (path, roots) => roots.some((root) => path === root || path.startsWith(`${root}/`))
const row = languages[args?.language]
const valid = row && typeof args.base === 'string' && args.base !== '' && (args.exclude === undefined || (Array.isArray(args.exclude) && args.exclude.every((path) => typeof path === 'string')))
if (!valid) throw new Error(`args take language (${Object.keys(languages).join(', ')}), base (the commit before the work it mines) and exclude (paths runs still edit); got ${JSON.stringify(args ?? null)}`)
const exclude = (args.exclude ?? []).map(relative)
const typed = row.typed
const RANGE = `${args.base}..HEAD`
const RULES = `tools/ast-grep/rules/${row.grep}`
const UTILS = `tools/ast-grep/utils/${row.grep}`
const OUTLINE = `tools/ast-grep/outline/${row.grep}.yml`
const LATE_OWNERS = [...(typed ? [typed.name] : []), 'config']
const OWNED = ['utils', ...LATE_OWNERS]
const RELEASE = row.blender ? `${row.release}, and for files under ${row.blender.roots.join(', ')} ${row.blender.release} with packages under ${row.blender.sources}` : row.release

// --- [PROMPTS]
const COST = `the cost command in use-ast-grep's rule-building reference, \`<globs>\` \`${row.globs}\`, reporting each id's user CPU beside \`${row.baseline}\``
const OWNERS = `Each fact goes to the first owner reporting every instance of it soundly: checker configuration (${row.config.decides}), then an ast-grep rule for a form syntax decides${typed ? `, then a ${typed.name} check for a form that needs ${typed.needs}` : ''}. Owners reporting a subset of another's instances own no fact, and two configured checkers reporting one fact are no overlap. Rows turning a configuration rule off stay while their reason holds: a tool fact (rewrite target, crash, an owner covering more) until the installed release changes it, a design policy always.`
const OTHERS = typed ? ` or ${typed.name} check` : ''
const ASK = {
  rules: 'what it decides; its owner by the ownership rule; overlap or contradiction with another rule; whether syntax alone decides it soundly; its hits in the scopes the `lint:ast-grep:*` targets of `nx show project rasm --json` run it on, split into true and false findings, and the forms it misses; its release metadata against the release its form needs; its cost against its peers\' below, and the walks causing it; its id, file name and message where long, coined or not agent-facing',
  utils: 'what each util and the outline decide, their callers, the walks dominating their callers\' cost, and each util no rule calls',
  typed: `what each check decides; its owner by the ownership rule; its findings on the tree split into true and false, and the forms it misses; what its tests under ${typed ? typed.tests : ''} cover; its name and message where long, coined or not agent-facing`,
  config: `each row (${row.config.rows}): what it decides, whether its reason holds by the ownership rule, rows restating a default, rows an ast-grep rule${OTHERS} restates, and configuration rules no row turns off that can replace an ast-grep rule${OTHERS}`,
}
const listed = (lines) => lines.map((line) => `- ${line}`).join('\n')
const section = (title, lines) => lines.length > 0 ? `${title}:\n${listed(lines)}\n` : ''
const loads = (skills) => skills.map((skill) => `Skill(${skill})`).join(', ')
const changed = (paths) => `each file \`git status --short --untracked-files=all -- ${paths}\` lists`
const later = (batch) => LATE_OWNERS.slice(LATE_OWNERS.indexOf(batch.name) + 1)
const scopes = (batch) => ({
  rules: `${batch.paths.join(', ')} and rules you add under ${RULES}`,
  new: batch.paths.join(', '),
  utils: `${batch.paths.join(', ')} and the rules under ${RULES} calling a util you edit`,
  typed: `${[...(typed ? typed.paths : []), typed ? typed.tests : ''].join(', ')}, rules you add under ${RULES} with the utils under ${UTILS} they call, and the files the map, findings and moves name`,
  config: `the rows of ${batch.paths.join(', ')}, rules you add under ${RULES} with the utils under ${UTILS} they call, and the files the map, findings and moves name`,
})[batch.kind]

// --- [ROUTING]
const chunked = (items, size) => Array.from({ length: Math.ceil(items.length / size) }, (_, index) => items.slice(index * size, (index + 1) * size))
const hosted = (paths) => hosts.filter(([folder, , held]) => held.includes(args.language) && paths.some((path) => path.split('/').includes(folder))).flatMap(([, skills]) => skills)
const skillsOf = (paths) => [...new Set([...row.skills, ...row.families, ...hosted(paths)])]
const folderOf = (path) => within(path, [RULES]) ? (/\.\w+$/.test(path.split('/').pop()) ? path.replace(/\/[^/]+$/, '') : path) : undefined
const owner = (batches, path) => {
  const kind = (name) => batches.find((batch) => batch.kind === name)
  return batches.find((batch) => batch.paths.includes(path))
    ?? (typed && within(path, [...typed.paths, typed.tests]) ? kind('typed') : undefined)
    ?? (row.config.paths.some((config) => path.split('/').pop() === config.split('/').pop()) ? kind('config') : undefined)
    ?? (within(path, [UTILS, OUTLINE]) ? kind('utils') : undefined)
    ?? (folderOf(path) ? batches.find((batch) => batch.kind === 'rules' && batch.paths.some((held) => held.startsWith(`${folderOf(path)}/`))) : undefined)
}
const route = (batches, found) => {
  const checks = found.flatMap((result) => result.checks).map((finding) => ({ ...finding, file: relative(finding.path) }))
  const fresh = [...new Set(checks.filter((finding) => !owner(batches, finding.file) && folderOf(finding.file)).map((finding) => folderOf(finding.file)))]
    .map((folder) => ({ name: `new:${folder.replace(`${RULES}/`, '')}`, kind: 'new', paths: [folder] }))
  const holder = (file) => owner(batches, file) ?? fresh.find((batch) => batch.paths[0] === folderOf(file))
  log(`check findings with no owner: ${checks.filter((finding) => !holder(finding.file)).map((finding) => finding.path).join(', ') || 'none'}`)
  return { fresh, found: (batch) => checks.filter((finding) => holder(finding.file) === batch).map((finding) => `${finding.path}: ${finding.finding}`) }
}

// --- [STAGES]
const mine = (unit, others = skillsOf(unit.paths).filter((skill) => !row.skills.includes(skill))) => agent(`Load ${loads(row.skills)} and read each one's references whole${others.length > 0 ? `, then each of ${others.join(', ')} whose frontmatter \`description\` covers a hunk, with the references a finding touches` : ''}. ${BAR}
Read every hunk of \`git diff ${RANGE} -- ${unit.paths.join(' ')}\` with its files at HEAD and keep what is new or better than the skills, ${row.companion ? `${row.companion}, ` : ''}and checks state, at ${RELEASE} with package sources under ${row.sources}. Regressions become check or anti-pattern findings alone. Before a check finding, read the rows of ${row.config.paths.join(', ')} with their reasons; a form a row turns off with a reason is no finding. Edit nothing. Return (structured), each with the code's file and line, a skill finding with the skill whose frontmatter \`description\` covers it and the file within it:
- skills: forms the code states better than a skill, rules it follows no skill states, and skill lines the package sources show wrong
- checks: forms a check misreports or misses, at a rule file, the rule folder a new rule joins${typed ? `, a ${typed.name} file` : ''}, or \`<config>#<table>.<row>\``, {
  label: `mine:${unit.name}`,
  phase: 'Read',
  model: 'opus',
  schema: {
    type: 'object',
    properties: {
      skills: { type: 'array', items: { type: 'object', properties: { skill: { enum: skillsOf(unit.paths) }, file: TEXT, finding: TEXT }, required: ['skill', 'file', 'finding'] } },
      checks: { type: 'array', items: { type: 'object', properties: { path: TEXT, finding: TEXT }, required: ['path', 'finding'] } },
    },
    required: ['skills', 'checks'],
  },
})
const tune = (skill, skillSet, findings) => agent(`Load Skill(${skill}), Skill(search-code) and Skill(clean-prose), read ${skill}'s references whole and the SKILL.md of each of ${skillSet.filter((other) => other !== skill).join(', ')} whose frontmatter \`description\` covers a finding's topic, with each reference that topic touches. ${BAR}
Writer for .claude/skills/${skill}, from the findings mined between ${args.base} and HEAD:
${listed(findings.map((finding) => `${finding.file}: ${finding.finding}`))}
Replace, add or remove a snippet, section or rule where the mined code justifies it; a new case rewrites the line that owns its rule. Each fact sits once within the skill and once across ${skillSet.join(', ')}, in the skill whose frontmatter \`description\` covers it. Snippets show packages under ${row.sources} at ${RELEASE} with placeholder names, coupled to no repository library. Edit only .claude/skills/${skill}. Return at most 10 lines: each section, snippet and line changed.`, { label: `skill:${skill}`, phase: 'Skills' })
const rebuild = (skill, skillSet) => agent(`Load Skill(${skill}) and Skill(search-code), and read the SKILL.md of each of ${skillSet.filter((other) => other !== skill).join(', ')} whose frontmatter \`description\` covers a changed passage. ${BAR}
Adversary for .claude/skills/${skill}. Read ${changed(`.claude/skills/${skill}`)} against \`git show HEAD:<file>\` hunk by hunk, removals included, then the skill whole. Rebuild in place each passage, snippet and rule weaker than the code in the tree, wrong against the package sources under ${row.sources} at ${RELEASE}, or owned by another skill. Edit only .claude/skills/${skill}. Return at most 6 lines: what you rebuilt, and the skill's bytes at HEAD and now.`, { label: `adversary:${skill}`, phase: 'Skills', model: 'opus' })
const map = (batch) => agent(`Load Skill(use-ast-grep) and Skill(manage-repo). ${BAR}
Map ${batch.paths.join(', ')}: for each file, ${ASK[batch.kind]}. ${OWNERS} Edit nothing. Return (structured) one action per file, and for a config file one per row with path \`<file>#<table>.<row>\`, each with the form it takes and the reason.`, {
  label: `map:${batch.name}`,
  phase: 'Read',
  schema: {
    type: 'object',
    properties: { actions: { type: 'array', items: { type: 'object', properties: { path: TEXT, action: { enum: ['keep', 'remove', 'rebuild', 'widen', 'tighten', 'rename', 'move to config', 'move to ast-grep', ...(typed ? [`move to ${typed.name}`] : [])] }, reason: TEXT }, required: ['path', 'action', 'reason'] } } },
    required: ['actions'],
  },
})
const write = (batch, plan, found, moves) => {
  const actions = (plan?.actions ?? []).filter((action) => action.action !== 'keep').map((action) => `${action.path}: ${action.action}, ${action.reason}`)
  return actions.length + found.length + moves.length === 0 ? Promise.resolve(null) : agent(`Load Skill(use-ast-grep), Skill(manage-repo), Skill(${row.skills[0]}) and Skill(clean-prose). ${BAR}
Writer for ${batch.paths.join(', ')}.
${section('Act on the map', actions)}${section(`Findings mined between ${args.base} and HEAD`, found)}${section(batch.kind === 'utils' ? 'Util changes rule writers returned, each carried to every caller' : 'Moves other writers returned, each adding its row or check and deleting the rule or check it replaces', moves)}${OWNERS} Delete each check about comments or dividers and each ast-grep rule syntax cannot decide soundly. Add a check only for a form the mined code shows and no check decides.${row.metadata.length > 0 ? ` A rule for a form one release introduces or retires sets \`metadata\` ${row.metadata.join(' or ')}.` : ''}${typed ? ` A ${typed.name} check follows the modules beside it and its cases join ${typed.tests}.` : ''} Ids, names and messages are short, agent-facing and clean-prose. Record one before and one after hit state per rule you edit or add, as rule-building states, under a directory from \`mktemp -d\`, and run those scans alone.${batch.kind === 'rules' || batch.kind === 'new' ? ' A util change a rule needs returns as a move to utils naming the util and the change.' : ''}${later(batch).length > 0 ? ` A fact ${later(batch).join(' or ')} takes leaves its rule or check in place and returns as a move naming its path and the row or check replacing it.` : ''}
Edit only ${scopes(batch)}. Return (structured) the files you edited, added or deleted, the hit state files, the moves, and at most 10 lines: each rule, check or row removed, renamed, changed or added, with its old and new id.`, {
    label: `rules:${batch.name}`,
    phase: 'Rules',
    schema: {
      type: 'object',
      properties: { files: LIST, states: LIST, moves: { type: 'array', items: { type: 'object', properties: { path: TEXT, to: { enum: OWNED }, finding: TEXT }, required: ['path', 'to', 'finding'] } }, report: TEXT },
      required: ['files', 'states', 'moves', 'report'],
    },
  })
}
const challenge = (result, batch) => !result ? null : (result.files.length === 0 ? Promise.resolve('no file changed') : agent(`Load Skill(use-ast-grep), Skill(manage-repo) and Skill(clean-prose). ${BAR}
Adversary for ${batch.paths.join(', ')}. Read ${changed(result.files.join(' '))} against \`git show HEAD:<file>\` hunk by hunk, removals included${result.states.length > 0 ? `, with the hit states ${result.states.join(', ')} read as rule-building states` : ''}. Judge each rule, check, row, id and message: rebuild in place each one that reports a correct form, misses a form its findings name, restates another owner, or carries an id or message that is long, coined or not agent-facing, recording an after state for each rule you rebuild. Edit only ${result.files.join(', ')}. Return at most 6 lines: what you rebuilt.`, { label: `challenge:${batch.name}`, phase: 'Rules', model: 'opus' }))
  .then((challenged) => ({ batch: batch.name, report: result.report, moves: result.moves, challenged: challenged ?? 'adversary returned nothing' }))

// --- [COMPOSITION] ---------------------------------------------------------------------

phase('Map')
const listing = agent(`Load Skill(use-ast-grep) and read its references/rule-building.md. ${BAR} List every rule file under ${RULES} with its id; every file under ${UTILS} with ${OUTLINE};${typed ? ` each file under ${typed.paths.join(', ')} and each test file under ${typed.tests} as its own entry;` : ''} and ${row.config.paths.join(', ')} with each member file holding ${row.config.rows}. Then measure \`${row.baseline}\` alone with ${COST}, in the foreground. Edit nothing. Return (structured) the rules with their ids, \`${row.baseline}\`'s user CPU ms as the baseline, the utils with the outline,${typed ? ` the ${typed.name} modules and tests as typed,` : ''} and the config files as configs.`, {
  label: 'map:checks',
  phase: 'Map',
  schema: {
    type: 'object',
    properties: {
      baseline: { type: 'number' },
      rules: { type: 'array', items: { type: 'object', properties: { path: TEXT, id: TEXT }, required: ['path', 'id'] } },
      utils: LIST,
      configs: LIST,
      ...(typed ? { typed: LIST } : {}),
    },
    required: ['baseline', 'rules', 'utils', 'configs', ...(typed ? ['typed'] : [])],
  },
})
const split = await agent(`${BAR} Partition every file \`git diff --name-only ${RANGE} -- ${[...row.pathspec, ...exclude.map((path) => `':(exclude)${path}'`)].join(' ')}\` lists into units: the files under ${row.unit}, a test file under ${row.tests} with the source folder its path mirrors, else the files under their top-level folder; split so each unit's diff stays within ${SPLIT} lines. Every file falls in one unit. Edit nothing. Return (structured) the units.`, { label: 'map:units', phase: 'Map', schema: { type: 'object', properties: { units: { type: 'array', items: { type: 'object', properties: { name: TEXT, paths: LIST }, required: ['name', 'paths'] } } }, required: ['units'] } })
if (!split) throw new Error('map:units returned nothing')
const units = split.units.reduce((kept, unit) => {
  const taken = new Set(kept.flatMap((held) => held.paths))
  const paths = [...new Set(unit.paths.map(relative))].filter((path) => !taken.has(path))
  return paths.length > 0 ? [...kept, { name: unit.name, paths }] : kept
}, [])
const skillSet = skillsOf(units.flatMap((unit) => unit.paths))
log(`${units.length} units, skills ${skillSet.join(', ')}`)

phase('Read')
const mining = parallel(units.map((unit) => () => mine(unit))).then((found) => {
  log(`units with no miner result: ${units.filter((_, index) => !found[index]).map((unit) => unit.name).join(', ') || 'none'}`)
  return found.filter(Boolean)
})
const skilled = mining.then((found) => {
  const findings = found.flatMap((result) => result.skills)
  const held = skillSet.filter((skill) => findings.some((finding) => finding.skill === skill))
  log(`skills with no mined finding: ${skillSet.filter((skill) => !held.includes(skill)).join(', ') || 'none'}`)
  return pipeline(held,
    (skill) => tune(skill, skillSet, findings.filter((finding) => finding.skill === skill)),
    (_, skill) => rebuild(skill, skillSet),
    (_, skill) => agent(`Load Skill(clean-prose) and read every file under its references whole. ${BAR}
Apply it to each line changed against HEAD, with its passage, in ${changed(`.claude/skills/${skill}`)}, then run \`${PROSE} .claude/skills/${skill}\` once and fix what it reports. Return at most 5 lines.`, { label: `prose:${skill}`, phase: 'Skills' }),
  )
})
const inventory = await listing
if (!inventory) throw new Error('map:checks returned nothing')
const rules = inventory.rules.map((rule) => ({ ...rule, path: relative(rule.path) })).sort((a, b) => a.path.localeCompare(b.path))
const batches = [
  ...chunked(rules, CHUNK).map((held, index) => ({ name: `rules-${index + 1}`, kind: 'rules', paths: held.map((rule) => rule.path) })),
  { name: 'utils', kind: 'utils', paths: inventory.utils.map(relative) },
  ...(typed ? [{ name: typed.name, kind: 'typed', paths: inventory.typed.map(relative) }] : []),
  { name: 'config', kind: 'config', paths: inventory.configs.map(relative) },
]
const early = batches.filter((batch) => batch.kind === 'rules')
const late = batches.filter((batch) => batch.kind !== 'rules')
log(`${batches.length} check batches`)
const routed = mining.then((found) => route(batches, found))
const ruled = pipeline(early,
  map,
  (plan, batch) => routed.then((routes) => write(batch, plan, routes.found(batch), [])),
  challenge,
)
const fresh = routed.then((routes) => pipeline(routes.fresh,
  (batch) => write(batch, null, routes.found(batch), []),
  challenge,
))
const plans = pipeline(late, map)
const tuned = Promise.all([ruled, fresh, plans, routed]).then(([first, second, planned, routes]) => late.reduce((prior, batch, index) => prior.then((done) => {
  const moves = done.filter(Boolean).flatMap((result) => result.moves).filter((move) => move.to === batch.name).map((move) => `${move.path}: ${move.finding}`)
  return write(batch, planned[index], routes.found(batch), moves).then((result) => challenge(result, batch)).then((result) => [...done, result])
}), Promise.resolve([...first, ...second])))
const [shaped, results] = await parallel([() => skilled, () => tuned])
const reports = (results ?? []).filter(Boolean)

phase('Close')
const owned = [...new Set([...skillSet.map((skill) => `.claude/skills/${skill}`), RULES, UTILS, OUTLINE, ...row.config.paths, ...inventory.configs.map(relative), ...(typed ? [...typed.paths, typed.tests] : [])])]
const close = await agent(`Load Skill(use-ast-grep), Skill(manage-repo) and Skill(${row.skills[0]}), and read use-ast-grep's references/rule-building.md. ${BAR}
Rule writer and adversary reports:
${reports.map((result) => `## ${result.batch}\n${result.report}\n${result.challenged}`).join('\n\n') || 'no change'}
Update every mention of an id the writers renamed or removed across the tree: skills, config reasons, per-line ignores and sgconfig.yml. Then run once the targets owning ${changed(owned.join(' '))}:
- a rule, util or outline: \`nx run rasm:lint:ast-grep\`, with \`nx run rasm:lint:yamllint\` and \`nx run rasm:lint:yamlfmt\` over its files
- a config row: ${row.config.targets.map((target) => `\`${target}\``).join(', ')}${typed ? `\n- a ${typed.name} file: \`${typed.run}\` and \`${typed.test}\`` : ''}
- a skill: \`${PROSE} .claude/skills/<skill>\`${row.snippet ? `\n- a changed snippet: piped through \`${row.snippet}\`` : ''}
Measure each changed or added rule, and each changed rule as HEAD holds it, once with ${COST} in the foreground. Fix each finding once at its cause (the code it names, or the check when it misfires), and rebuild each rule costing above twice \`${row.baseline}\`${exclude.length > 0 ? `, except findings under ${exclude.join(', ')}` : ''}. Then rerun the commands and costs once. Return at most 10 lines: findings fixed, any left, and each changed rule's cost against its cost before and \`${row.baseline}\`'s ${inventory.baseline} ms.`, { label: 'close', phase: 'Close' })
return { skills: shaped ?? [], rules: reports, close: close ?? 'close agent returned nothing' }
