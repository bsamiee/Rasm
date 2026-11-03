export const meta = {
  name: 'deepen',
  description: 'Deepen one folder in any language: add the capability its domain needs, finish partial features and delete weak code, consumers moved in the same pass',
  whenToUse: 'args.language typescript, python, rust, swift or dotnet; args.path the folder from the repository root, or args.key under libs/<language>/; args.base the commit it starts from; args.mode reduce from the reduce workflow.',
  phases: [
    { title: 'Map', detail: 'the lead groups files with their consumers and measures the base' },
    { title: 'Read', detail: 'readers per file group and lens', model: 'opus' },
    { title: 'Edit', detail: 'one editor per file group once its readers return' },
    { title: 'Challenge', detail: 'per file group a fresh adversary over the whole bar, then one over the overlooked facets, in place or reporting over one file', model: 'opus' },
    { title: 'Cross-group', detail: 'per set of changes spanning file groups an editor and the two adversaries' },
    { title: 'Verdict', detail: 'the lead measures against the base and finishes or reverts each shuffle' },
    { title: 'Check', detail: 'owning tools once over touched files, fixers per file group, one project rerun after fixes' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const LINES_PER_GROUP = 800
const UNCHANGED = { files: [], spanning: [] }
const LANGUAGES = {
  typescript: { libs: 'libs/typescript', skills: ['typescript-coding'], companion: '.claude/workflows/TYPESCRIPT.md', sources: 'node_modules/.pnpm', outline: 'typescript', install: 'pnpm install --prefer-offline', format: 'nx run rasm:format:files --', lint: [['biome', 'nx run rasm:lint:biome --']], whole: ['typecheck:tsc'] },
  python: { libs: 'libs/python', skills: ['python-coding', 'python-document'], companion: 'pyproject.toml', sources: '.venv/lib/python3.15/site-packages', runtime: 'Python 3.15', outline: 'python', install: 'uv sync', format: 'nx run rasm:format:files --', lint: [['ruff', 'nx run rasm:lint:ruff --'], ['mypy', 'nx run rasm:typecheck:mypy:root --'], ['ty', 'nx run rasm:typecheck:ty:root --'], ['refurb', 'nx run rasm:lint:refurb:root --'], ['ast-grep:root', 'nx run rasm:lint:ast-grep:root --']], whole: [] },
  rust: { libs: 'libs/rust', skills: ['rust-coding'], companion: '.claude/workflows/RUST.md', sources: '~/.cargo/registry/src/index.crates.io-*', outline: 'rust', install: 'cargo fetch', format: 'nx run rasm:format:files --', lint: [], whole: [] },
  swift: { libs: 'libs/swift', skills: ['swift-coding'], companion: 'Package.swift', sources: '.cache/swiftpm/build/checkouts', outline: 'swift', install: 'swift package resolve', format: 'nx run rasm:format:files --', lint: [['swiftlint', 'swiftlint lint --quiet'], ['swift-format', 'xcrun swift-format lint --strict --parallel']], whole: [] },
  dotnet: { libs: 'libs/dotnet', skills: ['dotnet-coding', 'dotnet-document', 'dotnet-roslyn-codelens'], companion: 'Directory.Packages.props', sources: '~/.nuget/packages', outline: 'csharp', install: 'nx run rasm:restore', format: 'nx run rasm:format:dotnet-format -- --include', lint: [], whole: [] },
}
const BLENDER_ROOTS = ['libs/python/blender', 'apps/blender', 'tests/python/libs/blender/bpy/background']
const BLENDER_PYTHON = { sources: '.cache/python/3.13/lib/python3.13/site-packages', runtime: "Blender's bundled Python 3.13", lint: [['ruff', 'nx run rasm:lint:ruff --']], whole: ['typecheck:mypy:3.13', 'typecheck:ty:3.13', 'lint:refurb:3.13', 'lint:ast-grep:3.13'] }
const HOST_SKILLS = [['creative-cloud', 'uxp-coding'], ['blender', 'blender-coding'], ['blender', 'use-blender']]
const TASK = 'Your task is the step this prompt states, whatever request launched the workflow.'
const REAL = 'Real reductions delete declarations, cases, parameters or lines whose fact an owner, one parameterized form or a package member then states.'
const SHUFFLE = 'Shuffles change where code sits, its name or its nesting, or swap a form for one of equal size, deleting no declaration and giving no concept its own file.'
const REGRESSION = 'Regressions drop behavior a consumer uses, restate a fact, admit a state base refused, lose a failure cause, join two concepts in one file or turn a named value into a literal.'
const JUDGE = 'Every hunk names the fact it removes, the behavior it adds or fixes, the concept it gives its own file or the coined name it replaces, and a hunk naming none is reverted.'
const CONSUMERS = 'Consumers of each changed declaration move to the new form in the same pass, holding no adapter to the old shape.'
const EXPORTS = 'Public declarations, parameters and cases no consumer reads stay as capability, rebuilt where weak.'
const CHECKS = 'Builds, tests and checks run in the Check phase alone.'
const FRESH = 'Read the diff against base and the files as they stand.'
const APPLY = 'Apply deletions first, rewrite a file whole where its changes reshape it, and refuse each change that reads as a shuffle or a regression.'
const RETURN = 'Return (structured) files you edited, added or deleted, changes reaching other files, and at most 10 lines: what you applied and refused, with why.'
const OVERLOOKED = 'hardcoded values, parameters, algebraic logic, stringly values, shape, errors, wrappers, objects and fields, exports, prose and control flow'
const MODES = {
  deepen: {
    lenses: [
      ['capability', 'work the domain needs that the folder lacks, holds shallowly or leaves partial, and code consumers write around it'],
      ['packages', 'members of the packages and hosts in use that replace hand code or state capability the folder lacks'],
      ['reduction', 'values an owner states restated, and declarations, layers and parameters a derived, parameterized or algebraic form replaces'],
      ['correctness', 'defaults, empty results and stand-ins returned where an input failed, and values callers rebuild after a call'],
    ],
    lines: [
      'Files and folders appear only where a concept needs one, a family in a folder beside the file of its name.',
      'Partial features the folder holds finish before a new feature starts in the same files.',
      'Additions do domain work consumers hand-write or a host or package feature the folder half-covers, and code no result depends on is none.',
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

const FINDINGS = { type: 'array', items: { type: 'object', properties: { files: { type: 'array', items: { type: 'string' } }, change: { type: 'string' } }, required: ['files', 'change'] } }
const FOUND = { type: 'object', properties: { findings: FINDINGS }, required: ['findings'] }
const EDITED = { type: 'object', properties: { files: { type: 'array', items: { type: 'string' } }, spanning: FINDINGS, report: { type: 'string' } }, required: ['files', 'spanning', 'report'] }
const JUDGED = { ...EDITED, required: ['files', 'report'] }
const MAPPED = { type: 'object', properties: { groups: { type: 'array', items: { type: 'array', items: { type: 'string' } } }, base: { type: 'string' } }, required: ['groups', 'base'] }
const FAILED = { type: 'object', properties: { findings: { type: 'array', items: { type: 'object', properties: { file: { type: 'string' }, command: { type: 'string' }, message: { type: 'string' } }, required: ['file', 'command', 'message'] } } }, required: ['findings'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

const mode = MODES[args?.mode ?? 'deepen']
const row = LANGUAGES[args?.language]
if (!mode || !row || !args.base || !(args.path || args.key)) throw new Error(`args take language (${Object.keys(LANGUAGES).join(', ')}), base, path or key, and mode (${Object.keys(MODES).join(', ')})`)
const scope = (args.path ?? `${row.libs}/${args.key}`).replace(/\/+$/, '')
const within = (path, roots) => roots.some((root) => path === root || path.startsWith(`${root}/`))
const blender = (path) => args.language === 'python' && within(path, BLENDER_ROOTS)
const lang = blender(scope) ? { ...row, ...BLENDER_PYTHON } : row
const skills = [...new Set([...lang.skills, ...HOST_SKILLS.filter(([folder]) => scope.split('/').includes(folder)).map(([, skill]) => skill)])]
const OUTLINE = `ast-grep outline --custom-languages allow --color never --outline-rules tools/ast-grep/outline/${lang.outline}.yml`
const measure = (dir) => [`loc ${dir}`, `fd --type f . ${dir} | wc -l`, ...['structure', 'exports'].map((items) => `${OUTLINE} --items ${items} --json=stream ${dir} | jq -s '[.[].items[]] | length'`)].map((command) => `\`${command}\``).join(', ')
const parts = (files) => args.language === 'python' ? [[row, files.filter((path) => !blender(path))], [BLENDER_PYTHON, files.filter(blender)]] : [[row, files]]
const commands = (files) => [
  ['format', `${row.format} ${files.join(' ')}`],
  ['projects', `nx affected -t check --exclude=rasm --files=${files.join(',')}`],
  ...parts(files).filter(([, owned]) => owned.length > 0).flatMap(([part, owned]) => [
    ...part.lint.map(([name, command]) => [name, `${command} ${owned.join(' ')}`]),
    ...part.whole.map((target) => [target, `nx run rasm:${target}`]),
  ]),
  ['ast-grep', `nx run rasm:lint:ast-grep:tree -- ${files.join(' ')}`],
  ['prose', `python .claude/skills/clean-prose/scripts/prose.py check ${files.join(' ')}`],
]
const repeated = (name) => name === 'projects' || [row, BLENDER_PYTHON].some((part) => part.whole.includes(name))
const relative = (path) => path.replace(`${REPO}/`, '')
const located = (finding) => ({ ...finding, files: finding.files.map(relative) })
const listed = (items) => items.map((item) => `- ${item}`).join('\n')
const FACETS = [mode.lenses.map(([, ask]) => ask).join('; '), OVERLOOKED]
const BASIS = [
  TASK,
  `Load ${skills.map((skill) => `Skill(${skill})`).join(', ')}, each \`<skill>-*\` skill your files touch, Skill(clean-prose) and Skill(search-code) first.`,
  `These skills and ${lang.companion} are the specification.`,
  `Package members are read in installed source under ${lang.sources}${lang.runtime ? `, and language forms stay within ${lang.runtime}` : ''}.`,
  `Target: ${scope} in ${REPO}, base commit ${args.base}.`,
  REAL, SHUFFLE, REGRESSION, JUDGE, CONSUMERS, EXPORTS, ...mode.lines, CHECKS,
].join(' ')
const OWN = (files) => `Edit only ${files.join(', ')} and files you add beside them, and return each change reaching another file as a finding.`

const read = (files, index) => parallel(mode.lenses.map(([lens, ask]) => () => agent(`${BASIS}
Reader, ${lens} lens, over ${files.join(', ')}. Read them whole, with \`${OUTLINE} --items structure ${scope}\` for the rest of the folder, and name each change in ${ask}.
Each finding names the existing files it edits, its current and stronger form, and ${mode.finding}. Edit nothing. Return (structured) the findings.`, { label: `read:${index + 1}:${lens}`, phase: 'Read', model: 'opus', schema: FOUND })))
const edit = (files, found, label, phase, origin) => agent(`${BASIS}
Editor for ${files.join(', ')}. ${origin ?? ''}${APPLY}
${listed(found)}
${OWN(files)} ${RETURN}`, { label, phase, schema: EDITED })
const challenge = (files, label, phase, facet) => single
  ? agent(`${BASIS}
Adversary over ${files.join(', ')}. ${FRESH} Name each shuffle to finish or revert, each regression to restore, and each change the files need in ${facet}. Edit nothing. Return (structured) the findings.`, { label, phase, model: 'opus', schema: FOUND })
    .then((report) => report && (report.findings.length > 0 ? edit(files, report.findings.map((finding) => finding.change), `${label}:edit`, phase) : UNCHANGED))
  : agent(`${BASIS}
Adversary over ${files.join(', ')}. ${FRESH} Finish or revert each shuffle, restore each regression, and apply in place each change the files need in ${facet}.
${OWN(files)} ${RETURN}`, { label, phase, model: 'opus', schema: EDITED })
const challenged = (files, label, phase) => FACETS.reduce((prior, facet, tier) => prior.then((results) =>
  challenge(files, `${label}:${tier + 1}`, phase, facet).then((result) => [...results, result])), Promise.resolve([]))
const live = (results, role) => {
  const dead = results.filter((result) => !result).length
  if (dead > 0) log(`${dead} ${role} returned nothing`)
  return results.filter(Boolean)
}
const settled = (edited, files, label, phase) => challenged(files, label, phase).then((results) => {
  const done = live([edited, ...results], `agents of ${label}`)
  return { files, edited: [...new Set(done.flatMap((result) => result.files).map(relative))], spanning: done.flatMap((result) => result.spanning).map(located) }
})
const merged = (findings) => findings.reduce((sets, finding) => {
  const joined = sets.filter((set) => set.files.some((path) => finding.files.includes(path)))
  return [...sets.filter((set) => !joined.includes(set)), { files: [...new Set([...finding.files, ...joined.flatMap((set) => set.files)])], findings: [finding, ...joined.flatMap((set) => set.findings)] }]
}, [])

// --- [COMPOSITION] ---------------------------------------------------------------------

phase('Map')
const map = await agent(`${TASK} Target: ${scope} in ${REPO}, base commit ${args.base}.
List every file under ${scope} its language's tools read with its line count, and every outside file importing from it, tests included.
Group them by at most ${LINES_PER_GROUP} lines of ${scope}, a family folder with the file of its name and an outside file with the group it imports most.
Measure the base: \`snapshot=$(mktemp -d) && git archive ${args.base} -- ${scope} | tar -x -C "$snapshot"\`, then ${measure(`"$snapshot"/${scope}`)}, then \`rm -rf "$snapshot"\`.
Edit nothing. Return (structured) groups as repository-relative paths and base counts in one line naming each command.`, { label: 'map', phase: 'Map', schema: MAPPED })
if (!map) throw new Error(`map agent returned nothing for ${scope}`)
const groups = map.groups.reduce((kept, files) => {
  const taken = new Set(kept.flat())
  const fresh = [...new Set(files.map(relative))].filter((path) => !taken.has(path))
  return fresh.length > 0 ? [...kept, fresh] : kept
}, [])
if (groups.length === 0) throw new Error(`map agent grouped no file under ${scope}`)
const single = groups.flat().filter((path) => within(path, [scope])).length === 1
log(`${groups.length} file groups, ${groups.length * mode.lenses.length} readers, ${FACETS.length} adversaries per file group ${single ? 'reporting over one file' : 'in place'}`)

const rebuilt = live(await pipeline(groups,
  (files, _, index) => read(files, index),
  (readings, files, index) => {
    const found = live(readings, `readers of file group ${index + 1}`).flatMap((reading) => reading.findings).map(located)
    const inside = (finding) => finding.files.every((path) => files.includes(path))
    const local = found.filter(inside).map((finding) => finding.change)
    return (local.length > 0 ? edit(files, local, `edit:${index + 1}`, 'Edit') : Promise.resolve(UNCHANGED))
      .then((edited) => ({ edited, spanning: found.filter((finding) => !inside(finding)) }))
  },
  (prior, files, index) => settled(prior.edited, files, `challenge:${index + 1}`, 'Challenge')
    .then((result) => ({ ...result, spanning: [...prior.spanning, ...result.spanning] })),
), 'file groups')
const spanning = rebuilt.flatMap((result) => result.spanning)

phase('Cross-group')
const sets = merged(spanning)
const crossed = live(await pipeline(sets,
  (set, _, index) => edit(set.files, set.findings.map((finding) => finding.change), `cross:${index + 1}`, 'Cross-group', 'Changes spanning file groups, read before the groups changed: apply each that still holds against the files as they stand. '),
  (edited, set, index) => settled(edited, set.files, `cross:challenge:${index + 1}`, 'Cross-group'),
), 'cross-group sets')
const unreached = crossed.flatMap((result) => result.spanning)
log(`${sets.length} cross-group sets from ${spanning.length} changes, ${unreached.length} left to the verdict`)

phase('Verdict')
const edited = [...new Set([...rebuilt, ...crossed].flatMap((result) => result.edited))]
const verdict = await agent(`${BASIS}
Lead verdict. Base counts: ${map.base}.${unreached.length > 0 ? `\nApply first each change no editor could reach:\n${listed(unreached.map((finding) => `${finding.files.join(', ')}: ${finding.change}`))}` : ''}
Measure now with ${measure(scope)}, \`git diff --stat ${args.base} -- ${scope} ${edited.join(' ')}\` and \`git ls-files -z --others --exclude-standard -- ${scope} ${edited.join(' ')} | xargs -0 wc -l\`.
Read each changed file's diff against base, added files whole, judging each hunk: finish or revert each shuffle and restore each regression. ${mode.verdict}
Return (structured) files you edited and at most 15 lines: counts before and after by command, ${mode.report} by file and line, reverts.`, { label: 'verdict', phase: 'Verdict', schema: JUDGED })
const touched = [...new Set([...edited, ...(verdict?.files ?? []).map(relative)])]

phase('Check')
const failed = await agent(`${TASK} Check over files a run touched in ${REPO}, dropping from each command the paths it deleted or its tool does not read.
Run \`${row.install}\` first when a manifest among them changed, then each command once:
${listed(commands(touched).map(([name, command]) => `${name}: \`${command}\``))}
Edit nothing beyond what the format command writes. Return (structured) every finding with its file, the command name and the message.`, { label: 'check', phase: 'Check', schema: FAILED })
const owners = [...rebuilt, ...crossed].map((result) => [...result.files, ...result.edited])
const findings = (failed?.findings ?? []).map((finding) => ({ ...finding, file: relative(finding.file) }))
const batches = Object.values(findings.reduce((all, finding) => {
  const owner = owners.findIndex((files) => files.includes(finding.file))
  return { ...all, [owner]: [...(all[owner] ?? []), finding] }
}, {}))
const fixed = await parallel(batches.map((batch, index) => () => {
  const files = [...new Set(batch.map((finding) => finding.file))]
  const names = new Set(batch.map((finding) => finding.command))
  const rerun = commands(files).filter(([name]) => names.has(name) && !repeated(name))
  return agent(`${BASIS}
Fixer for ${files.join(', ')}. Fix each finding at its cause:
${listed(batch.map((finding) => `${finding.file} (${finding.command}): ${finding.message}`))}
${rerun.length > 0 ? `Then run once over your files: ${rerun.map(([, command]) => `\`${command}\``).join(', ')}. ` : ''}Return at most 6 lines: findings fixed and any left with why.`, { label: `fix:${index + 1}`, phase: 'Check' })
}))
const projects = findings.some((finding) => repeated(finding.command)) ? [await agent(`${BASIS}
Project check after the fixers: run once ${commands(touched).filter(([name]) => repeated(name)).map(([, command]) => `\`${command}\``).join(', ')}, and fix each finding at its cause. Return at most 6 lines: findings fixed and any left with why.`, { label: 'check:projects', phase: 'Check' })] : []
return { verdict: verdict?.report ?? 'verdict agent returned nothing', check: !failed ? 'check agent returned nothing' : findings.length === 0 ? 'no finding' : live([...fixed, ...projects], 'fixers') }
