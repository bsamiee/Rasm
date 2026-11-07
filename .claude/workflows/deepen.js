export const meta = {
  name: 'deepen',
  description: 'Build, deepen or reduce code in any language: per file group a reader and an editor, or per unit of plan rows an editor per slice, disjoint slices at once and overlapping ones in order, then two adversaries and a verdict, cross-group sets, and a lead',
  whenToUse: 'args.language a languages.js key; args.mode build, deepen (default) or reduce; one of args.path, args.paths or args.key (build: a companion [03] row [KEY]); args.base the base commit; args.exclude folders another run edits; args.inputs evidence paths every agent reads; args.finished a workflow script path returning {label: result} for agents a fixed script must not run again; args.plan a plan whose rows for the paths, grouped into units by the package and files they own and sliced into what one editor lands, are the work in place of file-group readers (build: the architect reads it in place of brief readers and planner, and the writers land it)',
  phases: [
    { title: 'Plan', detail: 'build: two brief readers and a planner without args.plan, an adversarial architect returning the package graph', model: 'opus' },
    { title: 'Write', detail: 'build: one writer per package over disjoint files once the packages it reads are written' },
    { title: 'Map', detail: 'per root, files grouped with their importers; with args.plan outside build, one agent grouping the plan rows into units by the package and files they own, the rows of each unit into slices one editor lands at full depth' },
    { title: 'Read', detail: 'without args.plan, per file group one reader over every facet of the mode; per root a reader of relations across file groups, with args.plan only what its rows do not state, none in build', model: 'opus' },
    { title: 'Edit', detail: 'one editor per file group over its local findings, or per slice of a unit over its plan rows, slices over disjoint files at once and slices sharing a file in plan order' },
    { title: 'Challenge', detail: 'per file group or unit two adversaries in sequence, editing in place', model: 'opus' },
    { title: 'Verdict', detail: 'per file group or unit, diff against base judged hunk by hunk in place' },
    { title: 'Cross-group', detail: 'one pass over the deduplicated spanning changes: per set over the same files an editor, moving the consumers its changes reach, then two adversaries and a verdict where it changed a file, disjoint sets at once and overlapping ones in order' },
    { title: 'Lead', detail: 'diff against base, unneeded growth reverted, report' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const { hosts, languages } = await workflow('languages')
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const WORKFLOWS = '/Users/bardiasamiee/.claude/rasm/workflows'
const OUTLINE = 'ast-grep outline'
const RETURN = 'Return (structured) files you edited, added or deleted, changes reaching other files, facts you verified with their source (installed release, source path, host behavior), and errors you met (a checker conflict, a build error, a logic defect a live run caught).'
const MODES = {
  deepen: {
    facets: [
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
    lean: ', leaning to deepen: capability the plan rows, inputs and packages hold that the files still lack, narrow or hand-write lands as general logic at full depth, extending its categories where the domain justifies, in the existing file or a new one held to the same bar',
    verdict: 'Growth against base names a capability absent at base, in its smallest form, and growth naming none is reverted.',
    report: 'strongest changes',
  },
  reduce: {
    facets: [
      ['derivation', 'values, kinds, keys and labels an owner states, strings standing in for them, and values callers rebuild'],
      ['shape', 'declarations and parameters one shape with a parameter or case replaces, fields one state alone reads, and relations restated'],
      ['indirection', 'wrappers, forwarders, aliases, adapters, error and validation layers no caller branches on, and nesting a reshape flattens'],
      ['packages', 'hand code a package, host or current language feature states, and prose past its facts'],
      ['relations', 'one relation, codec or parameterized function several functions restate, and the existing functions that become its uses'],
    ],
    lines: [
      'Edits add no capability, and added lines stand only where they replace a larger form or give a hidden failure its case.',
    ],
    finding: 'what it removes, and one removing nothing is dropped',
    lean: ', leaning to reduce: facts an owner, package member or parameterized form states that the files still restate, and layers no caller branches on, fold away at full depth across the files and their consumers',
    verdict: 'Growth against base names the hidden failure or invalid state that gained its case, and growth naming none is reverted.',
    report: 'strongest reductions',
  },
}

const PLURALS = new Intl.PluralRules('en')
const NOUNS = {
  group: { one: 'file group', other: 'file groups' },
  unit: { one: 'unit', other: 'units' },
  slice: { one: 'slice', other: 'slices' },
  reader: { one: 'reader', other: 'readers' },
  root: { one: 'root', other: 'roots' },
  set: { one: 'cross-group set', other: 'cross-group sets' },
  change: { one: 'change', other: 'changes' },
}

// --- [MODELS] --------------------------------------------------------------------------

const PATHS = { type: 'array', items: { type: 'string' } }
const FINDINGS = { type: 'array', items: { type: 'object', properties: { files: PATHS, change: { type: 'string' } }, required: ['files', 'change'] } }
const FOUND = { type: 'object', properties: { findings: FINDINGS }, required: ['findings'] }
const EDITED = { type: 'object', properties: { files: PATHS, spanning: FINDINGS, facts: PATHS, errors: PATHS }, required: ['files', 'spanning', 'facts', 'errors'] }
const REPORTED = { type: 'object', properties: { files: PATHS, report: { type: 'string' } }, required: ['files', 'report'] }
const MAPPED = { type: 'object', properties: { groups: { type: 'array', items: PATHS } }, required: ['groups'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ARGS]

const within = (path, roots) => roots.some((root) => path === root || path.startsWith(`${root}/`))
const trimmed = (path) => path.replace(/^\.\//, '').replace(/\/+$/, '')
const strings = (value) => Array.isArray(value) && value.every((item) => typeof item === 'string')
const text = (value) => typeof value === 'string'
const row = Object.hasOwn(languages, args?.language) ? languages[args.language] : null
const MODE = args?.mode ?? 'deepen'
const exclude = strings(args?.exclude) ? args.exclude.map(trimmed) : []
const PLAN = text(args?.plan) ? trimmed(args.plan) : null
const inputs = [...new Set([...(PLAN === null ? [] : [PLAN]), ...(strings(args?.inputs) ? args.inputs.map(trimmed) : [])])]
const given = [args?.path, args?.paths, args?.key].filter((value) => value !== undefined)
const named = !row ? [] : strings(args.paths) ? args.paths : text(args.path) ? [args.path] : text(args.key) ? [`${row.libs}/${args.key}`] : []
const scoped = [...new Set(named.map(trimmed))].filter((root) => !within(root, exclude))
const roots = scoped.filter((root) => !scoped.some((other) => other !== root && within(root, [other])))
const valid = row && ['build', ...Object.keys(MODES)].includes(MODE) && text(args.base) && given.length === 1 && roots.length > 0
  && [args.exclude, args.inputs].every((value) => value === undefined || strings(value)) && [args.plan, args.finished].every((value) => value === undefined || text(value))
  && (MODE !== 'build' || (text(args.key) && row.companion))
if (!valid) throw new Error(`args take language (${Object.keys(languages).join(', ')}), mode (build, ${Object.keys(MODES).join(', ')}), base, `
  + `one of path, paths or key (key with build, for ${Object.keys(languages).filter((name) => languages[name].companion).join(', ')}), `
  + `exclude and inputs as path lists, plan and finished paths, with a root outside exclude. Got ${JSON.stringify(args ?? null)}`)
const mode = MODES[MODE === 'build' ? 'deepen' : MODE]
const rowed = PLAN !== null && MODE !== 'build'
const FINISHED = text(args.finished) ? await workflow({ scriptPath: args.finished }) : {}

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
  const evidence = [...(row.companion ? [`companion ${row.companion}`] : []), ...(inputs.length > 0 ? [`evidence ${inputs.join(', ')}`] : []), `catalog ${row.catalog}`, ...sources(paths), `recorded findings in ${WORKFLOWS} and docs/research`]
  return [
    `Target: ${target} in ${REPO}, base commit ${args.base}.`,
    `Load ${skills.join(', ')}${families}, Skill(clean-prose) and Skill(search-code).`,
    `Inputs: ${evidence.join('; ')}.`,
    ...(hosted.length > 0 ? ['Nothing takes focus from the owner, who is using this machine: no app.activate in a script body, no computer-use, whose access prompt also holds an agent until the owner answers. An app runs through Skill(watch-apps), under `watch-apps watch` in a Monitor with its output unfiltered, so every dialog and alert line reaches you and you answer it, and a proof that needs focus is reported, not taken.'] : []),
  ].join('\n')
}
const brief = (paths, target) => `${basis(paths, target)}\n${mode.lines.join(' ')}`
const own = (files) => `Edit only ${files.join(', ')} and files you add beside them, and return each change reaching another file as a finding.`
const diff = (files) => `Read \`git diff ${args.base} -- ${files.join(' ')}\` with the untracked files \`git status --short -- ${files.join(' ')}\` lists, and the files whole.`
const apart = exclude.length > 0 ? `, except files under ${exclude.join(', ')}` : ''
const counted = (count, noun) => `${count} ${noun[PLURALS.select(count)]}`

// --- [LOSSES]

const lost = []
const spent = { agents: 0 }
const note = (label, why) => {
  lost.push(`${label}: ${why}`)
  log(`LOST ${label}: ${why}`)
  return null
}
const broke = (label) => (error) => note(label, String(error?.message ?? error))
const once = (prompt, opts) => {
  if (Object.hasOwn(FINISHED, opts.label)) return Promise.resolve(FINISHED[opts.label])
  spent.agents += 1
  return agent(prompt, opts).then((result) => result ?? note(opts.label, 'agent ended with no result'), broke(opts.label))
}

// --- [STAGES]

const advance = (state, result) => {
  if (!result) return state
  const files = result.files.map(relative)
  return { ...state, files: union(state.files, files), edited: union(state.edited, files), spanning: [...state.spanning, ...result.spanning.map(located)], facts: [...state.facts, ...result.facts], errors: [...state.errors, ...result.errors] }
}
const fresh = (files) => ({ files, edited: [], spanning: [], facts: [], errors: [] })
const carried = (state) => [...(state.facts.length > 0 ? [`Verified facts, challengeable with evidence alone:\n${listed(state.facts)}\n`] : []), ...(state.errors.length > 0 ? [`Errors the pass before met:\n${listed(state.errors)}\n`] : [])].join('')
const editor = (state, changes, label, phase, origin = '', bound = own(state.files)) => once(`${brief(state.files)}
Editor for ${state.files.join(', ')}. ${origin}Apply deletions first, rewrite a file whole where its changes reshape it, and refuse each change that reads as a shuffle or a regression.
${listed(changes)}
${bound} ${RETURN}`, { label, phase, schema: EDITED }).then((result) => advance(state, result))
const adversary = (phase, lean, tier) => (state, item) => once(`${brief(state.files)}
Adversary over ${state.files.join(', ')}. ${diff(state.files)} Finish or revert each shuffle, restore each regression, and rebuild them in place${lean}.
${carried(state)}${own(state.files)} ${RETURN}`, { label: `challenge:${item.tag}:${tier}`, phase, model: 'opus', schema: EDITED }).then((result) => advance(state, result))
const verdict = (phase) => (state, item) => once(`${brief(state.files)}
Verdict over ${state.files.join(', ')}. ${diff(state.files)} Judge each hunk: finish or revert each shuffle and restore each regression, in place.
${carried(state)}${own(state.files)} ${RETURN}`, { label: `verdict:${item.tag}`, phase, schema: EDITED }).then((result) => advance(state, result))
const finish = (challenge, judge) => [adversary(challenge, mode.lean, 1), adversary(challenge, ', leaning to collapse existing declarations into stronger logic', 2), verdict(judge)]
const owned = (group) => (finding) => finding.files.every((path) => group.files.includes(path))
const reader = (group) => once(`${brief(group.files)}
Reader over ${group.files.join(', ')}. Read each file whole, with \`${OUTLINE} --items structure ${group.root}\` for the rest of the folder, and name each change in each facet:
${listed(mode.facets.map(([facet, ask]) => `${facet}: ${ask}`))}
Each finding names the existing files it edits, its current and stronger form, and ${mode.finding}. Edit nothing. Return (structured) the findings.`,
{ label: `read:${group.tag}`, phase: 'Read', model: 'opus', schema: FOUND }).then((result) => result?.findings.map(located) ?? [])
const relations = (root, index) => {
  const others = roots.slice(index + 1)
  return once(`${brief([root])}
Reader, relations across file groups, over ${root}. Read \`${OUTLINE} --items structure ${root}\` and \`${OUTLINE} --items exports --view signatures ${root}\`${others.length > 0 ? `, \`${OUTLINE} --items exports ${others.join(' ')}\`` : ''}, then the source of each declaration the outlines show alike.
Name each relation, codec or parameterized function several functions in different files restate, with the existing functions that become its uses, and each declaration two files or two projects state nearly alike, with the owner that states it once${rowed ? `, where the rows of ${PLAN} do not state it` : ''}.
Each finding names the existing files it edits, its current and stronger form, and ${mode.finding}. Edit nothing. Return (structured) the findings.`,
  { label: `read:relations:${root}`, phase: 'Read', model: 'opus', schema: FOUND })
}
const overlaps = (left, right) => left.some((path) => within(path, right)) || right.some((path) => within(path, left))
const ordered = (items) => items.map((item, index) => ({ ...item, needs: items.slice(0, index).filter((other) => overlaps(other.files, item.files)).map((other) => other.id) }))
const distinct = (findings) => [...new Map(findings.map((finding) => [`${union(finding.files).toSorted().join(' ')} ${finding.change}`, finding])).values()]
const merged = (findings, tag) => ordered([...Map.groupBy(distinct(findings), (finding) => union(finding.files).toSorted().join(' ')).values()].map((found, index) => ({ files: union(found[0].files), findings: found, id: `${tag}:${index + 1}`, tag: `${tag}:${index + 1}`, label: `edit:${tag}:${index + 1}` })))
const moving = (files) => `Edit ${files.join(', ')}, files you add beside them and the consumers of each API your changes move${apart}, and return each other change reaching another file as a finding.`
const cross = (set) => editor(fresh(set.files), set.findings.map((finding) => finding.change), set.label, 'Cross-group',
  'Changes spanning file groups, found before the groups changed: apply each that holds against the current files. ', moving(set.files))
  .then((state) => state.edited.length === 0 ? state : finish('Cross-group', 'Cross-group').reduce((prior, stage) => prior.then((held) => stage(held, set)), Promise.resolve(state)))
const crossing = (sets) => pumped(sets, cross).then((done) => [...done.values()])
const claimed = (parts) => parts.reduce((kept, unit) => {
  const taken = kept.flatMap((group) => group.files)
  const files = union(unit.files).filter((path) => !within(path, exclude) && !overlaps([path], taken))
  return [...kept, { ...unit, files, tag: `${kept.length + 1}` }]
}, [])
const pumped = (items, run) => {
  const gates = new Map(items.map((item) => [item.id, Promise.withResolvers()]))
  const done = new Map()
  return parallel(items.map((item) => async () => {
    const needs = item.needs.filter((need) => gates.has(need))
    const met = await Promise.all(needs.map((need) => gates.get(need).promise))
    const unmet = needs.filter((_, index) => !met[index])
    const result = unmet.length > 0 ? note(item.label, `not run, ${unmet.join(', ')} unfinished`) : await run(item, done).catch(broke(item.label))
    if (result) done.set(item.id, result)
    gates.get(item.id).resolve(Boolean(result))
  })).then(() => new Map(items.filter((item) => done.has(item.id)).map((item) => [item.id, done.get(item.id)])))
}
const refused = (packages) => {
  const ids = packages.map((pkg) => pkg.id)
  const needs = new Map(packages.map((pkg) => [pkg.id, pkg.needs.filter((need) => ids.includes(need))]))
  const cyclic = (id, path = []) => (path.includes(id) ? [[...path, id].join(' -> ')] : (needs.get(id) ?? []).flatMap((next) => cyclic(next, [...path, id])))
  return [...new Set([
    ...[...Map.groupBy(ids, String)].filter(([, held]) => held.length > 1).map(([id]) => `duplicate package ${id}`),
    ...packages.flatMap((pkg) => pkg.needs.filter((need) => !ids.includes(need)).map((need) => `${pkg.id} reads ${need}, which is no package`)),
    ...ids.flatMap((id) => cyclic(id)).map((path) => `cycle ${path}`),
    ...packages.flatMap((pkg, index) => packages.slice(index + 1).filter((other) => overlaps(union(pkg.files), union(other.files))).map((other) => `${pkg.id} and ${other.id} share files`)),
  ])]
}
const written = (packages, plan) => pumped(packages.map((pkg) => ({ ...pkg, label: `write:${pkg.id}` })), (pkg, done) => {
  const reads = pkg.needs.filter((need) => done.has(need)).map((need) => `${need}: ${done.get(need).summary}`)
  return once(`${brief(pkg.files)}
Writer for package ${pkg.id} (${pkg.title}): write what ${plan} section "${pkg.section}" states, editing only ${pkg.files.join(', ')}.${reads.length > 0 ? `\nPackages it reads, written:\n${listed(reads)}` : ''}
Return (structured) the files you edited and a summary in at most 15 lines.`, { label: pkg.label, phase: 'Write', schema: { type: 'object', properties: { summary: { type: 'string' }, files: PATHS }, required: ['summary', 'files'] } })
}).then((done) => [...done.values()].flatMap((result) => result.files.map(relative)))
const landed = (group) => pumped(group.slices, (slice) => editor(fresh(slice.files), slice.rows, slice.label, 'Edit',
  `Read ${['these files', ...group.reads].join(', ')} whole as they stand, then land each row of ${PLAN} below. `))
  .then((done) => [...done.values()].reduce((state, result) => ({ files: union(state.files, result.files), edited: union(state.edited, result.edited), spanning: [...state.spanning, ...result.spanning], facts: [...state.facts, ...result.facts], errors: [...state.errors, ...result.errors] }),
    fresh(group.files)))
const local = (group) => (found) => {
  const state = { ...fresh(group.files), spanning: found.filter((finding) => !owned(group)(finding)) }
  const changes = found.filter(owned(group)).map((finding) => finding.change)
  return changes.length > 0 ? editor(state, changes, `edit:${group.tag}`, 'Edit') : state
}

// --- [COMPOSITION] ---------------------------------------------------------------------

log(`${MODE} ${args.language} over ${roots.join(', ')}`)
const writes = MODE !== 'build' ? [] : await (async () => {
  const root = roots[0]
  const target = `row \`${args.key}\` of ${row.companion} [03], folder ${root}`
  const dir = `${WORKFLOWS}/build/${args.language}/${args.key}`
  const plan = PLAN ?? `${dir}/PLAN.md`
  phase('Plan')
  const GRAPH = { type: 'object', properties: { packages: { type: 'array', items: { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: PATHS, needs: PATHS }, required: ['id', 'title', 'section', 'files', 'needs'] } } }, required: ['packages'] }
  const planned = PLAN !== null || await parallel([
    ['code', `every declaration of ${root} with file, line and purpose, every import site of each public name in the consumers the row's [CONSUMERS] cell names and every other importer, and the manifest and workspace rows naming it`],
    ['members', `members of every catalog package the row and ${row.companion} name, at the release ${row.catalog} pins; use the search-code tarball form for packages no project declares`],
  ].map(([key, ask]) => () => once(`${brief([root], target)}
Write ${dir}/brief/${key}.md: ${ask}, as cited facts the planner designs from.`, { label: `brief:${key}`, phase: 'Plan', model: 'opus' })))
    .then((briefs) => briefs.every(Boolean) && once(`${brief([root], target)}
Planner. Read ${dir}/brief/code.md, ${dir}/brief/members.md and the row. Write only ${plan}: every file of ${root} with the declarations it holds and the package members it calls, every public name with the operation that composes it, every deletion, the manifest rows, every behavior of the current code with the name that keeps it, each consumer the row's [CONSUMERS] cell names with the form it moves to, then packages over disjoint files with the earlier packages each reads.
Return the package titles, one per line.`, { label: 'plan', phase: 'Plan' }))
  if (!planned) throw new Error(`no plan for ${target}: a brief reader or the planner was lost, and a plan needs both briefs`)
  const ARCHITECT = `${brief([root], target)}
Adversarial architect. Read ${plan}${PLAN === null ? `, ${dir}/brief` : ''} and the row whole, and rewrite ${plan} in place: every file, declaration, dependency edge and package stands where the row and the cited sources support it, and the rest leaves the plan. Manifests and check configuration sit in one package every source package needs, lock files in none.
Return (structured) the packages as a dependency graph: id, title, section (its exact heading in the plan), files (every path it edits, consumers included, each file in one package), needs (ids of packages it reads).`
  const drafted = await once(ARCHITECT, { label: 'architect', phase: 'Plan', model: 'opus', schema: GRAPH })
  const defects = drafted ? refused(drafted.packages) : []
  const graph = defects.length === 0 ? drafted : await once(`${ARCHITECT}\n\nThis graph has defects. Correct them and return the whole graph.\nGraph: ${JSON.stringify(drafted)}\nDefects:\n${listed(defects)}`, { label: 'architect:corrected', phase: 'Plan', model: 'opus', schema: GRAPH })
  if (!graph || graph.packages.length === 0) throw new Error(`architect returned no package graph for ${target}`)
  const left = refused(graph.packages)
  if (left.length > 0) throw new Error(`package graph for ${target} refused: ${left.join('; ')}`)
  const held = graph.packages.flatMap((pkg) => union(pkg.files).filter((path) => within(path, exclude)))
  if (held.length > 0) log(`${held.join(', ')} left to the run editing ${exclude.join(', ')}`)
  phase('Write')
  return written(graph.packages.map((pkg) => ({ ...pkg, files: union(pkg.files).filter((path) => !within(path, exclude)) })).filter((pkg) => pkg.files.length > 0), plan)
})()

phase('Map')
const maps = (await parallel((rowed ? [] : roots).map((root) => () => once(`Target: ${root} in ${REPO}, base commit ${args.base}.
List every file under ${root} its language's tools read with its line count, and every outside file importing from it${apart}.
Group files into what one agent reads whole with its consumers and edits at full depth, counting outside files, a family folder with the file of its name, an outside file with the group it imports most that has room, else with other outside files.
Edit nothing. Return (structured) groups as repository-relative paths.`,
  { label: `map:${root}`, phase: 'Map', schema: MAPPED }).then((result) => result && { ...result, root })))).filter(Boolean)
const units = rowed ? await once(`Target: ${roots.join(', ')} in ${REPO}, base commit ${args.base}.
Inputs: ${inputs.join('; ')}.
Read ${PLAN} whole and take each row whose change lands under ${roots.join(', ')}, a row with its nested lines. Group the rows into units by the package and files they own: a unit's files are every existing file its rows edit, consumers included, or for rows adding files alone the folder they join${apart}, no file in two units, and each unit is what two adversaries read whole with its reads and judge well. A unit's reads are the consumers it leaves unedited and the distilled files its rows link.
Slice each unit's rows: rows touching the same files join one slice, and a slice holds no more rows than one editor lands at full depth, so a file more rows touch than one editor lands sits in several slices, listed in the order their rows land. A slice's files are the unit's files its rows edit, or the folder its added files join.
Edit nothing. Return (structured) the units and their slices as repository-relative paths with each row as its first line number and its text verbatim.`,
  { label: 'map:plan', phase: 'Map', schema: { type: 'object', properties: { units: { type: 'array', items: { type: 'object', properties: { files: PATHS, reads: PATHS, slices: { type: 'array', items: { type: 'object', properties: { files: PATHS, rows: { type: 'array', items: { type: 'object', properties: { line: { type: 'integer' }, text: { type: 'string' } }, required: ['line', 'text'] } } }, required: ['files', 'rows'] } } }, required: ['files', 'reads', 'slices'] } } }, required: ['units'] } }) : null
if (rowed && !units) throw new Error(`map agent grouped no row of ${PLAN} under ${roots.join(', ')}`)
const claims = claimed(rowed
  ? units.units.map((unit) => ({ files: unit.files, reads: union(unit.reads), slices: unit.slices.map((slice) => ({ files: union(slice.files), rows: slice.rows.map((found) => `${PLAN} line ${found.line}: ${found.text}`) })) }))
  : maps.flatMap((map) => map.groups.map((files) => ({ root: map.root, files: union(files).filter((path) => within(path, [map.root]) || !within(path, roots)), reads: [], slices: [] }))))
  .map((group) => ({ ...group, slices: group.slices.map((slice) => ({ ...slice, wanted: slice.files, files: slice.files.filter((path) => within(path, group.files)) })) }))
const groups = claims.filter((group) => group.files.length > 0)
  .map((group) => ({ ...group, slices: ordered(group.slices.filter((slice) => slice.files.length > 0).map((slice, index) => ({ ...slice, id: `${group.tag}.${index + 1}`, label: `edit:${group.tag}.${index + 1}` }))) }))
const stranded = claims.flatMap((group) => group.slices).filter((slice) => slice.files.length === 0).flatMap((slice) => slice.rows.map((change) => ({ files: slice.wanted, change })))
if (groups.length === 0 && writes.length === 0) throw new Error(`map agents grouped no file under ${roots.join(', ')}`)
const relating = MODE !== 'build' || PLAN === null
const work = PLAN === null ? `${counted(groups.length, NOUNS.group)}, ${counted(groups.length + roots.length, NOUNS.reader)}`
  : rowed ? `${counted(groups.length, NOUNS.unit)} in ${counted(groups.flatMap((group) => group.slices).length, NOUNS.slice)} of ${PLAN} rows, ${counted(roots.length, NOUNS.reader)} of relations` : `${counted(groups.length, NOUNS.group)}, ${PLAN} landed by the writers`
const estimate = groups.reduce((sum, group) => sum + 3 + (rowed ? group.slices.length : PLAN === null ? 2 : 0), spent.agents + (relating ? roots.length + 2 : 2))
log(`${work} over ${counted(roots.length, NOUNS.root)}, about ${estimate} agents before cross-group sets`)

if (relating) phase('Read')
const reading = relating ? parallel(roots.map((root, index) => () => relations(root, index))) : Promise.resolve([])
const rebuilt = (await pipeline(groups,
  (group) => rowed ? landed(group) : PLAN === null ? reader(group).then(local(group)) : local(group)([]),
  ...finish('Challenge', 'Verdict'),
)).filter(Boolean)
const across = (await reading).filter(Boolean).flatMap((result) => result.findings).map(located)
const excluded = (finding) => finding.files.length === 0 || finding.files.some((path) => within(path, exclude))
const gathered = [...rebuilt.flatMap((state) => state.spanning), ...across, ...stranded]
const spanning = gathered.filter((finding) => !excluded(finding))

phase('Cross-group')
const sets = merged(spanning, 'cross')
const crossed = await crossing(sets)
const left = [...gathered.filter(excluded), ...crossed.flatMap((state) => state.spanning)]
log(`${counted(sets.length, NOUNS.set)} from ${counted(spanning.length, NOUNS.change)}, ${left.length} left`)

phase('Lead')
const states = [...rebuilt, ...crossed]
const edited = union(writes, ...states.map((state) => state.edited))
const lead = await once(`${brief(roots)}
Lead. Read \`git diff --stat ${args.base} -- ${union(roots, edited).join(' ')}\` and \`git ls-files -z --others --exclude-standard -- ${union(roots, edited).join(' ')} | xargs -0 wc -l\`.
${mode.verdict} Read the diff of each file that grew, one file at a time.
Return (structured) files you edited and at most 15 lines: ${mode.report} by file and line, reverts.`, { label: 'lead', phase: 'Lead', schema: REPORTED })
log(`${spent.agents} agents against about ${estimate} before cross-group sets, ${budget.spent()} output tokens this turn`)
return {
  lead: lead?.report ?? 'lead returned nothing',
  left: left.map((finding) => `${finding.files.join(', ')}: ${finding.change}`),
  agents: spent.agents,
  lost,
}
