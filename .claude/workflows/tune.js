export const meta = {
  name: 'tune',
  description: 'Tune one language\'s checks and skills from its committed diff: readers name each improvement and regression with its owner, a writer per checker and per ast-grep rule folder, per skill a writer, an adversary and clean-prose, one close',
  whenToUse: 'args.language a languages.js key, args.base the commit before the work, args.exclude paths other runs edit, whose findings return unwritten; args.finished a workflow script path returning {label: result} for agents a fixed script must not run again; skill and check paths committed at launch',
  phases: [
    { title: 'Map', detail: 'the diff packed into the units one reader reads whole and judges well, per unit a reader naming each improvement and regression its hunks show with its owner and the owner files it changes', model: 'opus' },
    { title: 'Checks', detail: 'per checker, and per rule folder for ast-grep, one writer holding every finding for it, disjoint writers at once and overlapping ones in order, rerouted findings in one later round' },
    { title: 'Skills', detail: 'per skill with findings a writer, an adversary over its diff, a clean-prose pass' },
    { title: 'Close', detail: 'renamed ids, owning targets once, skill snippets, one fix pass at the cause, one rerun' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const { hosts, languages } = await workflow('languages')
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const PROSE = `python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check`
const PLURALS = new Intl.PluralRules('en')
const NOUNS = {
  unit: { one: 'unit', other: 'units' },
  finding: { one: 'finding', other: 'findings' },
  group: { one: 'check group', other: 'check groups' },
  skill: { one: 'skill', other: 'skills' },
}

// --- [MODELS] --------------------------------------------------------------------------

const TEXT = { type: 'string' }
const LIST = { type: 'array', items: TEXT }
const FINDINGS = (owners) => ({
  type: 'array',
  items: { type: 'object', properties: { site: TEXT, kind: { enum: ['improvement', 'regression'] }, fact: TEXT, owner: { enum: owners }, paths: LIST, joins: TEXT }, required: ['site', 'kind', 'fact', 'owner', 'paths', 'joins'] },
})

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ARGS]
const relative = (path) => path.replace(`${REPO}/`, '').replace(/^\.\//, '').replace(/#.*$/, '').replace(/\/+$/, '')
const within = (path, roots) => roots.some((root) => path === root || path.startsWith(`${root}/`))
const row = languages[args?.language]
const valid = row && typeof args.base === 'string' && args.base !== '' && (args.exclude === undefined || (Array.isArray(args.exclude) && args.exclude.every((path) => typeof path === 'string'))) && (args.finished === undefined || typeof args.finished === 'string')
if (!valid) throw new Error(`args take language (${Object.keys(languages).join(', ')}), base (the commit before the work it reads), exclude (paths runs still edit) and finished (a script path); got ${JSON.stringify(args ?? null)}`)
const FINISHED = args.finished ? await workflow({ scriptPath: args.finished }) : {}
const exclude = (args.exclude ?? []).map(relative)
const RANGE = `${args.base}..HEAD`
const OWNERS = row.owners
const NAMES = OWNERS.map((owner) => owner.name)
const SYNTAX = OWNERS.at(-1)
const RELEASE = row.blender ? `${row.release}, and for files under ${row.blender.roots.join(', ')} ${row.blender.release} with packages under ${row.blender.sources}` : row.release

// --- [PROMPTS]
const listed = (lines) => lines.map((line) => `- ${line}`).join('\n')
const loads = (skills) => [...new Set(skills)].map((skill) => `Skill(${skill})`).join(', ')
const counted = (count, noun) => `${count} ${noun[PLURALS.select(count)]}`
const hosted = (paths) => hosts.filter(([folder, , held]) => held.includes(args.language) && paths.some((path) => path.split('/').includes(folder))).flatMap(([, skills]) => skills)
const skillsOf = (paths) => [...new Set([...row.skills, ...row.families, ...hosted(paths)])]
const ranked = (skills) => [...OWNERS.map((owner) => `${owner.name} (${owner.paths.join(', ')}): ${owner.decides}`), `a skill of ${skills.join(', ')}: a fact no check decides soundly, in the skill whose frontmatter \`description\` covers it`]
  .map((line, index) => `${index + 1}. ${line}`).join('\n')
const shown = (finding) => `${finding.site} (${finding.kind}): ${finding.fact}${finding.paths.length > 0 ? ` [${finding.paths.join(', ')}]` : ''}`
const untracked = (paths) => `the untracked files \`git status --short --untracked-files=all -- ${paths}\` lists`

// --- [SCHEDULE]
const normal = (finding) => ({ ...finding, paths: [...new Set(finding.paths.map(relative))].toSorted(), joins: relative(finding.joins) })
const folder = (path) => {
  const [rules] = SYNTAX.paths
  return within(path, [rules]) && path !== rules ? `${rules}/${path.slice(rules.length + 1).split('/')[0]}` : SYNTAX.paths.find((held) => within(path, [held])) ?? rules
}
const scoped = (finding, owner) => {
  const sources = [...finding.paths, ...(finding.joins === '' ? [] : [finding.joins])]
  return owner === SYNTAX ? [...new Set((sources.length > 0 ? sources : SYNTAX.paths.slice(0, 1)).map(folder))].toSorted() : owner.paths
}
const overlaps = (left, right) => left.some((path) => within(path, right)) || right.some((path) => within(path, left))
const ordered = (items) => items.map((item, index) => ({ ...item, needs: items.slice(0, index).filter((other) => overlaps(other.files, item.files)).map((other) => other.id) }))
const grouped = (findings, round) => ordered([...Map.groupBy(findings.map((finding) => ({ ...finding, scope: scoped(finding, OWNERS.find((owner) => owner.name === finding.owner)) })),
  (finding) => `${finding.owner} ${finding.scope.join(' ')}`).values()].map((found, index) => {
  const [{ owner, scope }] = found
  const files = [...scope, ...new Set(found.flatMap((finding) => finding.paths).filter((path) => !within(path, scope)))]
  return { owner: OWNERS.find((held) => held.name === owner), files, findings: found, id: `${round}:${index + 1}`, label: owner === SYNTAX.name ? `${round}:${owner}:${scope.map((path) => path.replace(`${SYNTAX.paths[0]}/`, '').replace('tools/ast-grep/', '')).join('+')}` : `${round}:${owner}` }
}))
const pumped = (items, run) => {
  const gates = new Map(items.map((item) => [item.id, Promise.withResolvers()]))
  const done = new Map()
  return parallel(items.map((item) => async () => {
    const met = await Promise.all(item.needs.map((need) => gates.get(need).promise))
    const unmet = item.needs.filter((_, index) => !met[index])
    const result = unmet.length > 0 ? note(item.label, `not run, ${unmet.join(', ')} unfinished`) : await run(item).catch(broke(item.label))
    if (result) done.set(item.id, result)
    gates.get(item.id).resolve(Boolean(result))
  })).then(() => items.filter((item) => done.has(item.id)).map((item) => done.get(item.id)))
}

// --- [LOSSES]
const lost = []
const note = (label, why) => {
  lost.push(`${label}: ${why}`)
  log(`LOST ${label}: ${why}`)
  return null
}
const broke = (label) => (error) => note(label, String(error?.message ?? error))
const once = (prompt, opts) => (Object.hasOwn(FINISHED, opts.label)
  ? Promise.resolve(FINISHED[opts.label])
  : agent(prompt, opts).then((result) => result ?? note(opts.label, 'agent ended with no result'), broke(opts.label)))

// --- [STAGES]
const read = (unit, skills = skillsOf(unit.paths)) => once(`Load ${loads(row.skills)}.
Reader over \`git diff ${RANGE} -- ${unit.paths.join(' ')}\`: read every hunk with its file at HEAD, at ${RELEASE} with package sources under ${row.sources}, and name each improvement and each regression the hunks show, small and large alike, that its owner does not yet state or check soundly. A regression names the check or skill line that refuses it.
Each finding goes to the first owner below stating it soundly for every instance; an owner reporting a subset of another's instances owns nothing, and a row turning a check off stays while its reason holds:
${ranked(skills)}
Search the owner's paths for the check, row or skill lines deciding the form and name the files holding them; a fact no present check or reference holds names the folder its new check or reference joins. Edit nothing. Return (structured) the findings, each with the code's file and line as its site, the fact as its current form, stronger form and reason, its owner, the existing owner files it edits, and the folder a new check or reference joins, else empty.`, {
  label: `read:${unit.name}`,
  phase: 'Map',
  model: 'opus',
  schema: { type: 'object', properties: { findings: FINDINGS([...NAMES, ...skills]) }, required: ['findings'] },
})
const write = (group, reroutable, scope = `the files at or under ${group.files.join(', ')}`) => once(`Load ${loads([...group.owner.skills, row.skills[0], 'clean-prose'])}.
Writer for ${group.owner.name}, owning ${group.owner.decides}, over ${scope}, from the improvements and regressions committed between ${args.base} and HEAD:
${listed(group.findings.map(shown))}
Read ${group.files.join(', ')} whole with the code at each site. At this owner, for every instance each fact covers: fix each contradiction, tighten or widen each weak, naive or narrow check, remove each check its owner cannot decide soundly or that judges comments or dividers, and add each missing check.${row.metadata.length > 0 ? ` A rule for a form one release introduces or retires sets \`metadata\` ${row.metadata.join(' or ')}.` : ''} A new check follows the checks beside it${group.owner === SYNTAX ? '' : ', its cases beside theirs'}. Ids, names and messages are short, agent-facing and clean-prose. ${group.owner === SYNTAX ? 'Prove each rule you edit or add once, as use-ast-grep\'s rule-building states: one scan of a scratch file of violating forms and near misses, then the project at zero findings, in the foreground.' : `Run ${group.owner.name} over the finding sites alone in the foreground, before and after your edits.`}${reroutable ? ' A change a file outside yours needs (a util other rules call, a row or check of another owner, an earlier owner stating the fact) returns as a reroute with its owner and the files it edits, and that file stays as it is.' : ''}
Edit only ${scope}. Return (structured) the files you edited, added or deleted; ids renamed and removed; ids of checks you changed or added, as they now stand;${reroutable ? ' the reroutes;' : ''} and at most 10 lines: each check removed, renamed, changed or added, with its hits${group.owner === SYNTAX ? '' : ' before and after'}.`, {
  label: `write:${group.label}`,
  phase: 'Checks',
  schema: {
    type: 'object',
    properties: {
      files: LIST,
      renamed: { type: 'array', items: { type: 'object', properties: { from: TEXT, to: TEXT }, required: ['from', 'to'] } },
      removed: LIST,
      changed: LIST,
      ...(reroutable ? { reroutes: FINDINGS(NAMES) } : {}),
      report: TEXT,
    },
    required: ['files', 'renamed', 'removed', 'changed', ...(reroutable ? ['reroutes'] : []), 'report'],
  },
}).then((result) => result && { group, result })
const close = (written, skills) => {
  const touched = written.filter((done) => done.result.files.length > 0)
  const owners = OWNERS.filter((owner) => touched.some((done) => done.group.owner === owner))
  const renamed = [...new Set(touched.flatMap((done) => done.result.renamed).map((pair) => `\`${pair.from}\` to \`${pair.to}\``))]
  const removed = [...new Set(touched.flatMap((done) => done.result.removed).map((id) => `\`${id}\``))]
  const commands = [...new Set(owners.flatMap((owner) => owner.targets))].map((command) => `\`${command}\``)
  const snippets = row.snippet && skills.length > 0 ? [`each snippet \`git diff HEAD -- ${skills.map((skill) => `.claude/skills/${skill}`).join(' ')}\` changes, piped through \`${row.snippet}\``] : []
  return touched.length + snippets.length === 0 ? Promise.resolve('no check or snippet changed') : once(`Load ${loads([...owners.flatMap((owner) => owner.skills), row.skills[0]])}.
Close over ${[...new Set(touched.flatMap((done) => done.result.files.map(relative)))].join(', ') || 'the skill snippets'}. Writer reports:
${touched.map((done) => `## ${done.group.label}\n${done.result.report}`).join('\n\n') || 'no check changed'}
${renamed.length + removed.length > 0 ? `Update every mention across the tree (skills, config reasons, per-line ignores, sgconfig.yml) of each id renamed (${renamed.join(', ') || 'none'}) or removed (${removed.join(', ') || 'none'}). ` : ''}Run each command once in the foreground, a command taking paths over the changed files it reads:
${listed([...commands, ...snippets])}
Fix each finding once at its cause (the code it names, or the check when it misfires)${exclude.length > 0 ? `, except findings under ${exclude.join(', ')}` : ''}, then rerun once each command that failed. Return at most 10 lines: findings fixed, any left.`, { label: 'close', phase: 'Close' })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

log(`tune ${args.language} over ${RANGE}${exclude.length > 0 ? `, except ${exclude.join(', ')}` : ''}`)
phase('Map')
const listing = await once(`Partition every file \`git diff --name-only ${RANGE} -- ${[...row.pathspec, ...exclude.map((path) => `':(exclude)${path}'`)].join(' ')}\` lists into units by \`git diff --numstat\` volume: related files join (the files under the ${row.unit} with the test files under ${row.tests} mirroring them, consumers beside what they consume), and units pack as much diff as one reader reads whole with its files and judges well, so small folders share a unit and a folder splits only where its diff alone exceeds one reader. Every file falls in one unit. Edit nothing. Return (structured) the units.`, {
  label: 'map:units',
  phase: 'Map',
  schema: { type: 'object', properties: { units: { type: 'array', items: { type: 'object', properties: { name: TEXT, paths: LIST }, required: ['name', 'paths'] } } }, required: ['units'] },
})
if (!listing) throw new Error('map:units returned nothing')
const units = listing.units.reduce((kept, unit) => {
  const taken = new Set(kept.flatMap((held) => held.paths))
  const paths = [...new Set(unit.paths.map(relative))].filter((path) => !taken.has(path))
  return paths.length > 0 ? [...kept, { ...unit, paths }] : kept
}, [])
const skillSet = skillsOf(units.flatMap((unit) => unit.paths))
log(`${counted(units.length, NOUNS.unit)}, skills ${skillSet.join(', ')}`)
const reads = await parallel(units.map((unit) => () => read(unit)))
const barred = (finding) => overlaps(exclude, [...finding.paths, ...(NAMES.includes(finding.owner) ? scoped(finding, OWNERS.find((owner) => owner.name === finding.owner)) : [`.claude/skills/${finding.owner}`])])
const found = reads.filter(Boolean).flatMap((result) => result.findings).map(normal)
const findings = found.filter((finding) => !barred(finding))
const checked = findings.filter((finding) => NAMES.includes(finding.owner))
const groups = grouped(checked, 'check')
const held = skillSet.filter((skill) => findings.some((finding) => finding.owner === skill))
log(`${counted(findings.length, NOUNS.finding)}: ${checked.length} in ${counted(groups.length, NOUNS.group)}, ${findings.length - checked.length} across ${counted(held.length, NOUNS.skill)}${held.length > 0 ? ` (${held.join(', ')})` : ''}`)

const skilling = pipeline(held,
  (skill) => once(`Load Skill(${skill}) and Skill(clean-prose).
Writer for .claude/skills/${skill}, from the improvements and regressions committed between ${args.base} and HEAD:
${listed(findings.filter((finding) => finding.owner === skill).map(shown))}
Read the files these findings name and the lines they touch. Replace, add or remove the snippet, section or rule the committed code justifies; a new case rewrites the line that owns its rule. Each fact sits once within the skill and once across ${skillSet.join(', ')}, \`rg\` over their folders showing where another states it. Snippets show packages under ${row.sources} at ${RELEASE} with placeholder names, coupled to no repository library. Edit only .claude/skills/${skill}. Return at most 10 lines: each section, snippet and line changed.`, { label: `skill:${skill}`, phase: 'Skills' }),
  (_, skill) => once(`Load Skill(${skill}).
Adversary for .claude/skills/${skill}. Read \`git diff HEAD -- .claude/skills/${skill}\` and ${untracked(`.claude/skills/${skill}`)} hunk by hunk, removals included, with the passage around each. Rebuild in place each passage, snippet and rule weaker than the code in the tree, wrong against the package sources under ${row.sources} at ${RELEASE}, or stating a fact another of ${skillSet.join(', ')} states. Edit only .claude/skills/${skill}. Return at most 6 lines: what you rebuilt.`, { label: `adversary:${skill}`, phase: 'Skills', model: 'opus' }),
  (_, skill) => once(`Load Skill(clean-prose) and read every file under its references whole.
Apply it to each line \`git diff HEAD -- .claude/skills/${skill}\` and ${untracked(`.claude/skills/${skill}`)} change, with its passage, then run \`${PROSE} .claude/skills/${skill}\` once in the foreground and fix what it reports. Return at most 5 lines.`, { label: `prose:${skill}`, phase: 'Skills' }),
)
const checking = pumped(groups, (group) => write(group, true)).then((first) => {
  const rerouting = first.flatMap((done) => done.result.reroutes).map(normal)
  rerouting.filter(barred).forEach((finding) => found.push(finding))
  const rerouted = grouped(rerouting.filter((finding) => !barred(finding)), 'reroute')
  if (rerouted.length > 0) log(`${counted(rerouted.length, NOUNS.group)} from rerouted findings`)
  return pumped(rerouted, (group) => write(group, false)).then((second) => [...first, ...second])
})
const [shaped, written] = await parallel([() => skilling.catch(broke('skills')), () => checking.catch(broke('checks'))])

phase('Close')
const closed = await close(written ?? [], held)
return {
  checks: (written ?? []).map((done) => `${done.group.label}: ${done.result.report}`),
  skills: (shaped ?? []).filter(Boolean),
  close: closed ?? 'close agent returned nothing',
  left: found.filter(barred).map(shown),
  lost,
}
