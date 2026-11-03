export const meta = {
  name: 'deepen',
  description: 'Build, deepen or reduce code in any language: readers, an editor, two adversaries and a verdict per file group, cross-group sets, a lead, one check',
  whenToUse: 'args.language a languages.js key; args.mode build, deepen (default) or reduce; one of args.path, args.paths or args.key (build: a companion [03] row [KEY]); args.base the base commit; args.exclude folders another run edits; args.inputs evidence paths every agent reads; args.oracle a behavior command the check runs; args.plan (build) a plan the architect reads in place of brief readers and planner',
  phases: [
    { title: 'Plan', detail: 'build: two brief readers and a planner without args.plan, an adversarial architect returning the package graph', model: 'opus' },
    { title: 'Write', detail: 'build: one writer per package over disjoint files once the packages it reads are written' },
    { title: 'Map', detail: 'per root, files grouped with their importers and base counts measured' },
    { title: 'Read', detail: 'per file group a reader per lens, per root a reader of relations across file groups', model: 'opus' },
    { title: 'Edit', detail: 'one editor per file group over its local findings' },
    { title: 'Challenge', detail: 'per file group two adversaries in sequence, editing in place', model: 'opus' },
    { title: 'Verdict', detail: 'per file group, diff against base judged hunk by hunk in place' },
    { title: 'Cross-group', detail: 'per merged set of spanning changes an editor, two adversaries and a verdict, then the same over changes no editor reached' },
    { title: 'Lead', detail: 'counts against base, unneeded growth reverted, report' },
    { title: 'Check', detail: 'owning targets and args.oracle once over touched files, one fix pass at the cause, one rerun' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const { hosts, languages } = await workflow('languages')
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const BAR = `${REPO}/.claude/workflows/BAR.md`
const LINES_PER_GROUP = 800
const RETURN = 'Return (structured) files you edited, added or deleted, and changes reaching other files.'
const MODES = {
  deepen: {
    lenses: [
      ['capability', 'work the domain needs that the folder lacks, holds shallowly or leaves partial, and code consumers write around it'],
      ['packages', 'members of packages and hosts in use that replace hand code or add capability the folder lacks'],
      ['reduction', 'values an owner states restated, and declarations, layers and parameters a derived, parameterized or algebraic form replaces'],
      ['correctness', 'stand-ins for a failed input, values callers rebuild, thread safety, blocking work, needless allocation and slow paths'],
      ['relations', 'one relation, codec or parameterized function several functions restate, and the existing functions that become its uses'],
    ],
    lines: [
      'Files and folders appear only where a concept needs one, a family in a folder beside the file of its name.',
      'Partial features the folder holds finish before a new feature starts in the same files.',
      'Additions do domain work consumers hand-write or finish a host or package feature the folder half-covers, and every addition feeds a result.',
    ],
    finding: 'what it removes or adds, and one doing neither is dropped',
    verdict: 'Every count above base names the capability that needs it, and growth naming none is reverted.',
    report: 'strongest changes',
  },
  reduce: {
    lenses: [
      ['derivation', 'values, kinds, keys and labels an owner states, strings or probes standing in for them, and values callers rebuild'],
      ['shape', 'declarations and parameters one shape with a parameter or case replaces, fields one state alone reads, and relations restated'],
      ['indirection', 'wrappers, forwarders, aliases, adapters, error and validation layers no caller branches on, and nesting a reshape flattens'],
      ['packages', 'hand code a package, host or current language feature states, and prose past its facts'],
      ['relations', 'one relation, codec or parameterized function several functions restate, and the existing functions that become its uses'],
    ],
    lines: [
      'Edits add no capability, and added lines stand only where they replace a larger form or give a hidden failure its case.',
    ],
    finding: 'what it removes, and one removing nothing is dropped',
    verdict: 'Every count above base names the hidden failure or invalid state that gained its case, and growth naming none is reverted.',
    report: 'strongest reductions',
  },
}

// --- [MODELS] --------------------------------------------------------------------------

const PATHS = { type: 'array', items: { type: 'string' } }
const FINDINGS = { type: 'array', items: { type: 'object', properties: { files: PATHS, change: { type: 'string' } }, required: ['files', 'change'] } }
const FOUND = { type: 'object', properties: { findings: FINDINGS }, required: ['findings'] }
const EDITED = { type: 'object', properties: { files: PATHS, spanning: FINDINGS }, required: ['files', 'spanning'] }
const REPORTED = { type: 'object', properties: { files: PATHS, report: { type: 'string' } }, required: ['files', 'report'] }
const MAPPED = { type: 'object', properties: { groups: { type: 'array', items: PATHS }, base: { type: 'string' } }, required: ['groups', 'base'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ARGS]
const within = (path, roots) => roots.some((root) => path === root || path.startsWith(`${root}/`))
const trimmed = (path) => path.replace(/^\.\//, '').replace(/\/+$/, '')
const strings = (value) => Array.isArray(value) && value.every((item) => typeof item === 'string')
const optional = (value, test) => value === undefined || test(value)
const text = (value) => typeof value === 'string'
const row = Object.hasOwn(languages, args?.language) ? languages[args.language] : null
const MODE = args?.mode ?? 'deepen'
const exclude = strings(args?.exclude) ? args.exclude.map(trimmed) : []
const inputs = strings(args?.inputs) ? args.inputs.map(trimmed) : []
const given = [args?.path, args?.paths, args?.key].filter((value) => value !== undefined)
const named = !row ? [] : strings(args.paths) ? args.paths : text(args.path) ? [args.path] : text(args.key) ? [`${row.libs}/${args.key}`] : []
const scoped = [...new Set(named.map(trimmed))].filter((root) => !within(root, exclude))
const roots = scoped.filter((root) => !scoped.some((other) => other !== root && within(root, [other])))
const valid = row && ['build', ...Object.keys(MODES)].includes(MODE) && text(args.base) && given.length === 1 && roots.length > 0
  && optional(args.exclude, strings) && optional(args.inputs, strings) && optional(args.oracle, text)
  && optional(args.plan, (plan) => text(plan) && MODE === 'build') && (MODE !== 'build' || (text(args.key) && row.companion))
if (!valid) throw new Error(`args take language (${Object.keys(languages).join(', ')}), mode (build, ${Object.keys(MODES).join(', ')}), base, `
  + `one of path, paths or key (key with build, for ${Object.keys(languages).filter((name) => languages[name].companion).join(', ')}), `
  + `exclude and inputs as path lists, oracle a command and plan a path with build, with a root outside exclude; got ${JSON.stringify(args ?? null)}`)
const mode = MODES[MODE === 'build' ? 'deepen' : MODE]
const OUTLINE = `ast-grep outline --custom-languages allow --color never --outline-rules tools/ast-grep/outline/${row.grep}.yml`

// --- [PROMPTS]
const relative = (path) => trimmed(path.replace(`${REPO}/`, ''))
const union = (...lists) => [...new Set(lists.flat().map(relative))]
const located = (finding) => ({ ...finding, files: finding.files.map(relative) })
const listed = (items) => items.map((item) => `- ${item}`).join('\n')
const blender = (path) => Boolean(row.blender) && within(path, row.blender.roots)
const holds = (path) => Boolean(row.blender) && row.blender.roots.some((root) => within(root, [path]))
const sources = (paths) => [
  ...(paths.some((path) => !blender(path)) ? [`package sources under ${row.sources}; release: ${row.release}`] : []),
  ...(paths.some((path) => blender(path) || holds(path)) ? [`under ${row.blender.roots.join(', ')}, package sources under ${row.blender.sources}; release: ${row.blender.release}`] : []),
]
const basis = (paths, target = paths.join(', ')) => {
  const hosted = hosts.filter(([folder, , held]) => held.includes(args.language) && paths.some((path) => path.split('/').includes(folder))).flatMap(([, skills]) => skills)
  const skills = [...new Set([...row.skills, ...hosted])].map((skill) => `Skill(${skill})`)
  const families = row.families.length > 0 ? `, each of ${row.families.map((skill) => `Skill(${skill})`).join(', ')} whose description covers your files` : ''
  const evidence = [...(row.companion ? [`companion ${row.companion}`] : []), ...(inputs.length > 0 ? [`evidence ${inputs.join(', ')}`] : []), `catalog ${row.catalog}`, ...sources(paths), 'recorded findings in .artifacts/workflows and docs/research']
  return [
    `Target: ${target} in ${REPO}, base commit ${args.base}.`,
    `Load ${skills.join(', ')}${families}, Skill(clean-prose) and Skill(search-code), and read ${BAR} first.`,
    `Inputs: ${evidence.join('; ')}.`,
  ].join('\n')
}
const brief = (paths, target) => `${basis(paths, target)}\n${mode.lines.join(' ')}`
const own = (files) => `Edit only ${files.join(', ')} and files you add beside them, and return each change reaching another file as a finding.`
const diff = (files) => `Read \`git diff ${args.base} -- ${files.join(' ')}\` with the untracked files \`git status --short -- ${files.join(' ')}\` lists, and the files whole.`
const measure = (dir) => [`loc ${dir}`, `fd --type f . ${dir} | wc -l`, ...['structure', 'exports'].map((items) => `${OUTLINE} --items ${items} --json=stream ${dir} | jq -s '[.[].items[]] | length'`)]
  .map((command) => `\`${command}\``).join(', ')

// --- [STAGES]
const live = (results, role) => {
  const dead = results.filter((result) => !result).length
  if (dead > 0) log(`${dead} ${role} returned nothing`)
  return results.filter(Boolean)
}
const advance = (state, result, label) => {
  if (!result) {
    log(`${label} returned nothing`)
    return state
  }
  const files = result.files.map(relative)
  return { ...state, files: union(state.files, files), edited: union(state.edited, files), spanning: [...state.spanning, ...result.spanning.map(located)] }
}
const editor = (state, changes, label, phase, origin = '') => agent(`${brief(state.files)}
Editor for ${state.files.join(', ')}. ${origin}Apply deletions first, rewrite a file whole where its changes reshape it, and refuse each change that reads as a shuffle or a regression.
${listed(changes)}
${own(state.files)} ${RETURN}`, { label, phase, schema: EDITED }).then((result) => advance(state, result, label))
const adversary = (phase, lean, tier) => (state, item) => agent(`${brief(state.files)}
Adversary over ${state.files.join(', ')}. ${diff(state.files)} Finish or revert each shuffle, restore each regression, and apply in place each change the bar asks of them${lean}.
${own(state.files)} ${RETURN}`, { label: `challenge:${item.tag}:${tier}`, phase, model: 'opus', schema: EDITED }).then((result) => advance(state, result, `challenge:${item.tag}:${tier}`))
const verdict = (phase) => (state, item) => agent(`${brief(state.files)}
Verdict over ${state.files.join(', ')}. ${diff(state.files)} Judge each hunk by the bar: finish or revert each shuffle and restore each regression, in place.
${own(state.files)} ${RETURN}`, { label: `verdict:${item.tag}`, phase, schema: EDITED }).then((result) => advance(state, result, `verdict:${item.tag}`))
const finish = (challenge, judge) => [adversary(challenge, '', 1), adversary(challenge, ', leaning to collapse existing declarations into stronger logic', 2), verdict(judge)]
const relations = (root) => {
  const others = roots.filter((other) => other !== root)
  return agent(`${brief([root])}
Reader, relations across file groups, over ${root}. Read \`${OUTLINE} --items structure ${root}\` and \`${OUTLINE} --items exports ${root}\`${others.length > 0 ? `, \`${OUTLINE} --items exports ${others.join(' ')}\`` : ''}, then the source of each declaration the outlines show alike.
Name each relation, codec or parameterized function several functions in different files restate, with the existing functions that become its uses, and each declaration two files or two projects state nearly alike, with the owner that states it once.
Each finding names the existing files it edits, its current and stronger form, and ${mode.finding}. Edit nothing. Return (structured) the findings.`,
  { label: `read:relations:${root}`, phase: 'Read', model: 'opus', schema: FOUND })
}
const merged = (findings) => findings.reduce((sets, finding) => {
  const [joined, apart] = sets.reduce(([hit, miss], set) => overlaps(set.files, finding.files) ? [[...hit, set], miss] : [hit, [...miss, set]], [[], []])
  return [...apart, { files: union(finding.files, ...joined.map((set) => set.files)), findings: [finding, ...joined.flatMap((set) => set.findings)] }]
}, [])
const chunked = (set) => {
  const limit = 16
  return set.findings.reduce((chunks, finding) => {
    const current = chunks.at(-1)
    const files = current ? union(current.files, finding.files) : []
    return current && files.length <= limit
      ? [...chunks.slice(0, -1), { files, findings: [...current.findings, finding] }]
      : [...chunks, { files: union(finding.files), findings: [finding] }]
  }, [])
}
const cross = (chunk) => finish('Cross-group', 'Cross-group').reduce((prior, stage) => prior.then((state) => stage(state, chunk)),
  editor({ files: chunk.files, edited: [], spanning: [] }, chunk.findings.map((finding) => finding.change), `edit:${chunk.tag}`, 'Cross-group',
    'Changes spanning file groups, found before the groups changed: apply each that holds against the current files. '))
const crossing = (sets, tag) => parallel(sets.map((set, index) => () => chunked(set).reduce(
  (prior, chunk, part) => prior.then((states) => cross({ ...chunk, tag: `${tag}:${index + 1}.${part + 1}` }).then((state) => [...states, state])),
  Promise.resolve([]))))
  .then((results) => live(results, `${tag} sets`).flat())
const overlaps = (left, right) => left.some((path) => within(path, right)) || right.some((path) => within(path, left))
const written = (packages, plan) => {
  const ids = new Set(packages.map((pkg) => pkg.id))
  const pending = [...packages]
  const running = []
  const done = new Map()
  const dead = new Set()
  const prune = () => {
    const blocked = pending.filter((pkg) => pkg.needs.some((need) => dead.has(need)))
    blocked.forEach((pkg) => {
      pending.splice(pending.indexOf(pkg), 1)
      dead.add(pkg.id)
      log(`${pkg.id} not written, ${pkg.needs.filter((need) => dead.has(need)).join(', ')} unwritten`)
    })
    return blocked.length > 0 && prune()
  }
  const pump = () => {
    prune()
    const ready = pending.reduce((picked, pkg) => pkg.needs.every((need) => done.has(need) || !ids.has(need))
      && ![...running, ...picked].some((other) => overlaps(other.files, pkg.files)) ? [...picked, pkg] : picked, [])
    ready.forEach((pkg) => {
      pending.splice(pending.indexOf(pkg), 1)
      running.push(pkg)
    })
    return parallel(ready.map((pkg) => () => {
      const reads = pkg.needs.filter((need) => done.has(need)).map((need) => `${need}: ${done.get(need).summary}`)
      return agent(`${brief(pkg.files)}
Writer for package ${pkg.id} (${pkg.title}): write what ${plan} section "${pkg.section}" states, editing only ${pkg.files.join(', ')}.${reads.length > 0 ? `\nPackages it reads, written:\n${listed(reads)}` : ''}
Return (structured) the files you edited and a summary in at most 15 lines.`, { label: `write:${pkg.id}`, phase: 'Write', schema: { type: 'object', properties: { summary: { type: 'string' }, files: PATHS }, required: ['summary', 'files'] } }).then((result) => {
        running.splice(running.indexOf(pkg), 1)
        if (result) done.set(pkg.id, result)
        else {
          dead.add(pkg.id)
          log(`write:${pkg.id} returned nothing`)
        }
        return pump()
      })
    }))
  }
  return pump().then(() => {
    if (pending.length > 0) log(`${pending.map((pkg) => pkg.id).join(', ')} not written, needs form a cycle`)
    return [...done.values()].flatMap((result) => result.files.map(relative))
  })
}

// --- [CHECKS]
const parts = (files) => row.blender ? [[row, files.filter((path) => !blender(path))], [row.blender, files.filter(blender)]] : [[row, files]]
const commands = (files) => [...new Map([
  ...row.format.map((command) => ['format', `${command} ${files.join(' ')}`]),
  ['projects', `nx affected -t check --exclude=rasm --files=${files.join(',')}`],
  ...parts(files).filter(([, owned]) => owned.length > 0).flatMap(([part, owned]) => [
    ...part.lint.map(([name, command]) => [name, `${command} ${owned.join(' ')}`]),
    ...part.whole.map((target) => [target, `nx run rasm:${target}`]),
  ]),
  ['ast-grep', `nx run rasm:lint:ast-grep:tree -- ${files.join(' ')}`],
  ['prose', `python .claude/skills/clean-prose/scripts/prose.py check ${files.join(' ')}`],
  ...(text(args.oracle) ? [['oracle', args.oracle]] : []),
].map(([name, command]) => [command, [name, command]])).values()]

// --- [COMPOSITION] ---------------------------------------------------------------------

const writes = MODE !== 'build' ? [] : await (async () => {
  const root = roots[0]
  const target = `row \`${args.key}\` of ${row.companion} [03], folder ${root}`
  const dir = `${REPO}/.artifacts/workflows/build/${args.language}/${args.key}`
  const plan = args.plan ?? `${dir}/PLAN.md`
  phase('Plan')
  const planned = args.plan !== undefined || await parallel([
    ['code', `every declaration of ${root} with file, line and purpose, every import site of each public name in the consumers the row's [CONSUMERS] cell names and every other importer, and the manifest and workspace rows naming it`],
    ['members', `members of every catalog package the row and ${row.companion} name, at the release ${row.catalog} pins: purpose, signature and feature of each`],
  ].map(([key, ask]) => () => agent(`${brief([root], target)}
Write ${dir}/brief/${key}.md: ${ask}, as cited facts the planner designs from.`, { label: `brief:${key}`, phase: 'Plan', model: 'opus' })))
    .then((briefs) => live(briefs, 'brief readers'))
    .then(() => agent(`${brief([root], target)}
Planner. Read ${dir}/brief/code.md, ${dir}/brief/members.md and the row. Write only ${plan}: every file of ${root} with the declarations it holds and the package members it calls, every public name with the operation that composes it, every deletion, the manifest rows, every behavior of the current code with the name that keeps it, each consumer the row's [CONSUMERS] cell names with the form it moves to, then packages over disjoint files with the earlier packages each reads.
Return the package titles, one per line.`, { label: 'plan', phase: 'Plan' }))
  if (!planned) throw new Error(`planner returned nothing for ${target}`)
  const graph = await agent(`${brief([root], target)}
Adversarial architect. Read ${plan}${args.plan === undefined ? `, ${dir}/brief` : ''} and the row whole, and rewrite ${plan} in place: every file, declaration, dependency edge and package stands where the row and the cited sources support it, and the rest leaves the plan. Manifests and check configuration sit in one package every source package needs, lock files in none.
Return (structured) the packages as a dependency graph: id, title, section (its exact heading in the plan), files (every path it edits, consumers included), needs (ids of packages it reads).`, { label: 'architect', phase: 'Plan', model: 'opus', schema: { type: 'object', properties: { packages: { type: 'array', items: { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: PATHS, needs: PATHS }, required: ['id', 'title', 'section', 'files', 'needs'] } } }, required: ['packages'] } })
  if (!graph || graph.packages.length === 0) throw new Error(`architect returned no package graph for ${target}`)
  const held = graph.packages.flatMap((pkg) => union(pkg.files).filter((path) => within(path, exclude)))
  if (held.length > 0) log(`${held.join(', ')} left to the run editing ${exclude.join(', ')}`)
  phase('Write')
  return written(graph.packages.map((pkg) => ({ ...pkg, files: union(pkg.files).filter((path) => !within(path, exclude)) })), plan)
})()

phase('Map')
const maps = live(await parallel(roots.map((root) => () => agent(`Target: ${root} in ${REPO}, base commit ${args.base}.
List every file under ${root} its language's tools read with its line count, and every outside file importing from it, tests included${exclude.length > 0 ? `, except files under ${exclude.join(', ')}` : ''}.
Group files by at most ${LINES_PER_GROUP} lines counting outside files, a family folder with the file of its name, an outside file with the group it imports most that has room, else with other outside files.
Measure the base: \`snapshot=$(mktemp -d) && git archive ${args.base} -- ${root} | tar -x -C "$snapshot"\`, then ${measure(`"$snapshot"/${root}`)}, then \`rm -rf "$snapshot"\`; a root absent at base counts zero.
Edit nothing. Return (structured) groups as repository-relative paths and base counts in one line naming each command.`,
  { label: `map:${root}`, phase: 'Map', schema: MAPPED }).then((result) => result && { ...result, root }))), 'map agents')
const groups = maps.reduce((kept, map) => map.groups.reduce((claimed, files) => {
  const taken = new Set(claimed.flatMap((group) => group.files))
  const fresh = union(files).filter((path) => !taken.has(path) && !within(path, exclude) && (within(path, [map.root]) || !within(path, roots)))
  return fresh.length > 0 ? [...claimed, { root: map.root, files: fresh, tag: `${claimed.length + 1}` }] : claimed
}, kept), [])
if (groups.length === 0) throw new Error(`map agents grouped no file under ${roots.join(', ')}`)
log(`${groups.length} file groups over ${roots.length} roots, ${groups.length * mode.lenses.length + roots.length} readers`)

phase('Read')
const reading = parallel(roots.map((root) => () => relations(root)))
const rebuilt = live(await pipeline(groups,
  (group) => parallel(mode.lenses.map(([lens, ask]) => () => agent(`${brief(group.files)}
Reader, ${lens} lens, over ${group.files.join(', ')}. Read each file whole, with \`${OUTLINE} --items structure ${group.root}\` for the rest of the folder, and name each change in ${ask}.
Each finding names the existing files it edits, its current and stronger form, and ${mode.finding}. Edit nothing. Return (structured) the findings.`,
    { label: `read:${group.tag}:${lens}`, phase: 'Read', model: 'opus', schema: FOUND }))),
  (readings, group) => {
    const found = live(readings, `readers of file group ${group.tag}`).flatMap((result) => result.findings).map(located)
    const inside = (finding) => finding.files.every((path) => group.files.includes(path))
    const state = { files: group.files, edited: [], spanning: found.filter((finding) => !inside(finding)) }
    const local = found.filter(inside).map((finding) => finding.change)
    return local.length > 0 ? editor(state, local, `edit:${group.tag}`, 'Edit') : state
  },
  ...finish('Challenge', 'Verdict'),
), 'file groups')
const across = live(await reading, 'relation readers').flatMap((result) => result.findings).map(located)
const excluded = (finding) => finding.files.some((path) => within(path, exclude))
const gathered = [...rebuilt.flatMap((state) => state.spanning), ...across]
const spanning = gathered.filter((finding) => !excluded(finding))

phase('Cross-group')
const sets = merged(spanning)
const crossed = await crossing(sets, 'cross')
const unreached = crossed.flatMap((state) => state.spanning).filter((finding) => !excluded(finding))
const last = unreached.length > 0 ? await crossing(merged(unreached), 'last') : []
const left = [...gathered, ...crossed.flatMap((state) => state.spanning)].filter(excluded).concat(last.flatMap((state) => state.spanning))
log(`${sets.length} cross-group sets from ${spanning.length} changes, ${unreached.length} to the last sets, ${left.length} left`)

phase('Lead')
const states = [...rebuilt, ...crossed, ...last]
const edited = union(writes, ...states.map((state) => state.edited))
const bases = maps.map((map) => `${map.root}: ${map.base}`)
const lead = await agent(`${brief(roots)}
Lead. Base counts:
${listed(bases)}
Measure the tree with${roots.map(measure).join('; ')}, \`git diff --stat ${args.base} -- ${union(roots, edited).join(' ')}\` and \`git ls-files -z --others --exclude-standard -- ${union(roots, edited).join(' ')} | xargs -0 wc -l\`.
${mode.verdict} Read the diff of each file the counts show growing, one file at a time.
Return (structured) files you edited and at most 15 lines: counts before and after by command, ${mode.report} by file and line, reverts.`, { label: 'lead', phase: 'Lead', schema: REPORTED })
const touched = union(edited, lead?.files ?? [])

phase('Check')
const check = touched.length === 0 ? null : await agent(`${basis(touched)}
Check over files the run touched, each command without deleted paths or paths its tool does not read. Run \`${row.install}\` first when a manifest among them changed, then each command once:
${listed(commands(touched).map(([name, command]) => `${name}: \`${command}\``))}
Fix each finding at its cause in one pass, then rerun the commands once.
Return (structured) files you edited and at most 10 lines: findings fixed and any left with why.`, { label: 'check', phase: 'Check', schema: REPORTED })
return {
  lead: lead?.report ?? 'lead returned nothing',
  left: left.map((finding) => `${finding.files.join(', ')}: ${finding.change}`),
  check: touched.length === 0 ? 'no file touched' : check?.report ?? 'check agent returned nothing',
}
