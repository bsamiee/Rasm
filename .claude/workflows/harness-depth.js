export const meta = {
  name: 'harness-depth',
  description: 'Harden one host harness: mine failures and the host model, run a case battery from its needs once, fix findings at their cause, deepen, one check',
  whenToUse: 'args.key a tools/harnesses folder, args.name the host, args.apps its installed releases, args.skills the skills driving it, args.products product folders with proofs that used it, args.base the base commit; args.finished a workflow script path returning {label: result} for agents a fixed script must not run again',
  phases: [
    { title: 'Mine', detail: 'readers over past proofs, failures and the host model, the model reader returning the battery needs' },
    { title: 'Battery', detail: 'one case author per need, one runner through the harness' },
    { title: 'Edit', detail: 'findings sliced by shared cause, one editor per slice at the cause, slices over disjoint files at once and slices sharing a file in order' },
    { title: 'Deepen', detail: 'two adversaries in sequence over the diff against base, editing in place' },
    { title: 'Check', detail: 'owning targets over touched files and the battery once, one fix pass at the cause, one rerun' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const { languages } = await workflow('languages')
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const KIT = `${REPO}/tools/harnesses`
const BAR = `${REPO}/.claude/workflows/BAR.md`
const ARGS = ['key', 'name', 'apps', 'skills', 'products', 'base']
const PLURALS = new Intl.PluralRules('en')
const NOUNS = {
  finding: { one: 'finding', other: 'findings' },
  slice: { one: 'slice', other: 'slices' },
}

// --- [MODELS] --------------------------------------------------------------------------

const LINES = { type: 'array', items: { type: 'string' } }
const FOUND = { type: 'object', properties: { findings: LINES }, required: ['findings'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

const missing = ARGS.filter((name) => !args?.[name])
if (missing.length > 0) throw new Error(`args take ${ARGS.join(', ')}, missing ${missing.join(', ')}`)
const HARNESS = `${KIT}/${args.key}`
const BATTERY = `/Users/bardiasamiee/.claude/rasm/workflows/harness-depth/${args.key}/battery`
const RUN = `harness ${args.key}`
const FINISHED = typeof args.finished === 'string' ? await workflow({ scriptPath: args.finished }) : {}
const SKILLS = [...new Set(['watch-apps', args.skills].flat())].map((skill) => `Skill(${skill})`).join(', ')
const BASIS = [
  `Harness: ${HARNESS} in ${KIT}, run through \`${RUN} <folder of *.cases.json>\`, its answers read through \`${RUN} log\`, base commit ${args.base}.`,
  'The harness isolates every case in one host call per batch on a fresh background host from `watch-apps launch`, builds real documents from case preconditions and reads whole answers in one call.',
  'It presses only the dialog button a case names and records any other dialog, hang or crash with its pid, leaving the instance running for the agent.',
  `It serves every installed release of ${args.name}: ${[args.apps].flat().join(', ')}.`,
  'Recorded findings include the harness facts index. Each agent acts on the documents and host processes it starts.',
  'Nothing takes focus from the owner, who is using this machine: no app.activate in a script body, no computer-use, whose access prompt also holds an agent until the owner answers.',
  'A host runs under `watch-apps watch` in a Monitor with its output unfiltered, so every dialog and alert line reaches the agent, which answers it, and a proof that needs focus is reported, not taken.',
].join(' ')
const READ = `Load ${SKILLS} first.\n${BASIS}`
const WRITE = `Load ${SKILLS} and each edited language's coding skill, and read ${BAR} first.\n${BASIS}`
const DIFF = `Read \`git diff ${args.base} -- ${KIT}\` hunk by hunk, files from \`git ls-files --others --exclude-standard -- ${KIT}\` whole, and each changed file whole.`
const rows = Object.entries(languages).flatMap(([name, row]) => [[name, row], ...(row.blender ? [[`${name} in Blender`, { ...row, ...row.blender }]] : [])])
const COMMANDS = [...[
  ...rows.flatMap(([name, row]) => row.format.map((command) => [`${command} <files>`, 'format', name])),
  ['nx affected -t check --exclude=rasm --files=<files joined by commas>', 'projects'],
  ...rows.flatMap(([name, row]) => [...row.lint.map(([tool, command]) => [`${command} <files>`, tool, name]), ...row.whole.map((target) => [`nx run rasm:${target}`, target, name])]),
  ['nx run rasm:lint:ast-grep:tree -- <files>', 'ast-grep'],
  ['python .claude/skills/clean-prose/scripts/prose.py check <files>', 'prose'],
].reduce((held, [command, tool, name]) => held.set(command, { tool, names: [...(held.get(command)?.names ?? []), ...(name ? [name] : [])] }), new Map())]
  .map(([command, { tool, names }]) => `- ${tool}${names.length > 0 ? ` for ${names.join(', ')}` : ''}: \`${command}\``)
const INSTALLS = [...new Set(rows.map(([, row]) => row.install))].map((command) => `\`${command}\``).join(', ')
const listed = (lines) => lines.map((line) => `- ${line}`).join('\n')
const counted = (count, noun) => `${count} ${noun[PLURALS.select(count)]}`
const relative = (path) => path.replace(`${REPO}/`, '').replace(/^\.\//, '').replace(/\/+$/, '')
const within = (path, roots) => roots.some((root) => path === root || path.startsWith(`${root}/`))
const overlaps = (left, right) => left.some((path) => within(path, right)) || right.some((path) => within(path, left))
const KIT_PATH = relative(KIT)
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
const mine = (lens, ask) => once(`${READ}
Miner, ${lens}. Read ${ask}, one line each with its cause. Edit nothing. Return (structured) the findings.`, { label: `mine:${lens}`, phase: 'Mine', schema: FOUND })
const adversary = (index, lean) => once(`${WRITE}
Adversary over ${KIT}. ${DIFF} Finish or revert each shuffle, restore each regression and apply in place every change the bar asks of the kit.${lean} Return at most 6 lines: what you changed.`, { label: `deepen:${index}`, phase: 'Deepen' })

// --- [COMPOSITION] ---------------------------------------------------------------------

log(`harness-depth ${args.key} for ${args.name} (${[args.apps].flat().join(', ')})`)
phase('Mine')
const proofs = mine('proofs', `every proof run for ${[args.products].flat().join(', ')}: each workaround it needed and each case left unproven where the harness could not express it`)
const failures = mine('failures', `crash reports in ~/Library/Logs/DiagnosticReports for ${args.name}, the harness run records under .artifacts/logs/harnesses/ and past run transcripts that drove it: each crash, hang, dialog and repeated call`)
const mined = (await Promise.all([proofs, failures])).filter(Boolean)
const model = await once(`${READ}
Miner, host model. Read ${args.name}'s scripting model (dictionary, object model, introspection) against ${HARNESS}, with what its proofs and runs left:
${listed(mined.flatMap((result) => result.findings))}
Needs: each area of the scripting model a real proof touches, named in the host's own terms with the classes or members it covers and the failures it shows.
Findings: each kind of host state a proof needs that the harness cannot set up, read or compare, with its cause. Edit nothing. Return (structured) the needs and the findings.`, { label: 'mine:model', phase: 'Mine', schema: { type: 'object', properties: { needs: LINES, findings: LINES }, required: ['needs', 'findings'] } })
if (!model) throw new Error(`model miner returned nothing for ${args.name}`)
if (model.needs.length === 0) throw new Error(`model miner derived no need from ${args.name}'s scripting model`)
log(`${model.needs.length} needs from ${args.name}'s scripting model`)

phase('Battery')
const authored = (await pipeline(model.needs,
  (need, _, index) => once(`${READ}
Case author for ${args.name}, need: ${need}. Write the cases a real proof in this area needs as the harness's case files under ${BATTERY}/${index + 1}/. Each case the harness cannot express is a finding. Return (structured) the findings.`, { label: `battery:author:${index + 1}`, phase: 'Battery', schema: FOUND }),
)).filter(Boolean)
const ran = await once(`${READ}
Battery runner. Run every case file under ${BATTERY}/ through the harness once, against each installed release of ${args.name}. Record each crash, hang, dialog, wrong result and missing capability with its cause. Return (structured) the findings.`, { label: 'battery:run', phase: 'Battery', schema: FOUND })
const findings = [...mined, model, ...(ran ? [ran] : []), ...authored].flatMap((result) => result.findings)

phase('Edit')
const sliced = findings.length === 0 ? { slices: [], refused: [] } : await once(`${READ}
Slice the findings below over ${KIT}: findings sharing a cause join one slice with one general fix, and a slice holds no more findings than one editor lands at full depth, so a file more fixes touch than one editor lands sits in several slices, listed in the order their fixes land. Each slice names every file its fixes edit and the folder each added file joins. Refuse a finding specific to one session or one that hides a failure, one line each. Edit nothing.
${listed(findings)}
Return (structured) the slices as repository-relative paths with their findings verbatim, and the refusals.`, { label: 'edit:slice', phase: 'Edit', schema: { type: 'object', properties: { slices: { type: 'array', items: { type: 'object', properties: { files: LINES, findings: LINES }, required: ['files', 'findings'] } }, refused: LINES }, required: ['slices', 'refused'] } })
if (!sliced) throw new Error(`slicer returned nothing for ${counted(findings.length, NOUNS.finding)}`)
const slices = sliced.slices.map((slice) => ({ ...slice, files: slice.files.map(relative).filter((path) => within(path, [KIT_PATH])) }))
  .filter((slice) => slice.files.length > 0 || note('edit:slice', `no file under ${KIT_PATH} for ${slice.findings.join('; ')}`))
  .map((slice, index) => ({ ...slice, id: index + 1 }))
log(`${counted(slices.length, NOUNS.slice)} from ${counted(findings.length, NOUNS.finding)}`)
const fixes = new Map()
const edited = (await parallel(slices.map((slice) => () => {
  const fix = Promise.all(slices.slice(0, slice.id - 1).filter((other) => overlaps(other.files, slice.files)).map((other) => fixes.get(other.id)))
    .then(() => once(`${WRITE}
Editor for ${slice.files.join(', ')}. Read them whole as they stand, then fix every finding below at its cause, one general fix per shared cause, rebuilding a file whole where its fixes reshape it.
${listed(slice.findings)}
Edit only ${slice.files.join(', ')} and files you add beside them. Return (structured) at most 12 lines of what you fixed, and each change reaching another file.`, { label: `edit:${slice.id}`, phase: 'Edit', schema: { type: 'object', properties: { report: { type: 'string' }, spanning: LINES }, required: ['report', 'spanning'] } }))
  fixes.set(slice.id, fix)
  return fix
}))).filter(Boolean)
const spanning = edited.flatMap((result) => result.spanning)

phase('Deepen')
const first = await adversary(1, spanning.length > 0 ? ` Land each change the editors found reaching past their files:\n${listed(spanning)}\n` : '')
const second = await adversary(2, ' Lean to collapsing existing types, constants, strings and functions into stronger logic.')

phase('Check')
const checked = await once(`${WRITE}
Check over ${KIT}. <files> is the files \`git diff --name-only ${args.base} -- ${KIT}\` and \`git ls-files --others --exclude-standard -- ${KIT}\` list, less paths a tool does not read; drop each command whose tool reads none.
Run first the install of each language with a changed manifest among them (${INSTALLS}), then each command once:
${COMMANDS.join('\n')}
Run the battery under ${BATTERY}/ through the harness once. Fix each finding at its cause in one pass, then rerun the commands and the battery once.
Return at most 10 lines: findings fixed and any left with why.`, { label: 'check', phase: 'Check' })
return { edit: [...edited.map((result) => result.report), ...sliced.refused.map((refusal) => `refused: ${refusal}`)], deepen: [first, second].map((report) => report ?? 'adversary returned nothing'), check: checked ?? 'check agent returned nothing', lost }
