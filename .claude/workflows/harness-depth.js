export const meta = {
  name: 'harness-depth',
  description: 'Harden one host harness: mine failures and the host model, run a case battery from its needs once, fix findings at their cause, deepen, one check',
  whenToUse: 'args.key a tools/harnesses folder, args.name the host, args.apps its installed releases, args.skills the skills driving it, args.products product folders with proofs that used it, args.base the base commit',
  phases: [
    { title: 'Mine', detail: 'readers over past proofs, failures and the host model, the model reader returning the battery needs' },
    { title: 'Battery', detail: 'one case author per need, one runner through the harness' },
    { title: 'Edit', detail: 'one editor fixes every finding at its cause' },
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

// --- [MODELS] --------------------------------------------------------------------------

const LINES = { type: 'array', items: { type: 'string' } }
const FOUND = { type: 'object', properties: { findings: LINES }, required: ['findings'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

const missing = ARGS.filter((name) => !args?.[name])
if (missing.length > 0) throw new Error(`args take ${ARGS.join(', ')}, missing ${missing.join(', ')}`)
const HARNESS = `${KIT}/${args.key}`
const BATTERY = `${REPO}/.artifacts/workflows/harness-depth/${args.key}/battery`
const RUN = `nx run @rasm/harnesses:${args.key} --`
const SKILLS = [args.skills].flat().map((skill) => `Skill(${skill})`).join(', ')
const BASIS = [
  `Harness: ${HARNESS} in ${KIT}, run through \`${RUN}\`, base commit ${args.base}.`,
  'The harness isolates every case in one host call per batch on a fresh hidden host from `watch-apps launch`, builds real documents from case preconditions and reads whole answers in one call.',
  'It presses only the dialog button a case names and records any other dialog, hang or crash with its pid, leaving the instance running for the agent.',
  `It serves every installed release of ${args.name}: ${[args.apps].flat().join(', ')}.`,
  'Recorded findings include the harness facts index. Each agent acts on the documents and host processes it starts.',
].join(' ')
const READ = `Load ${SKILLS} first.\n${BASIS}`
const WRITE = `Load ${SKILLS} and each edited language's coding skill, and read ${BAR} first.\n${BASIS}`
const DIFF = `Read \`git diff ${args.base} -- ${KIT}\` hunk by hunk, files from \`git ls-files --others --exclude-standard -- ${KIT}\` whole, and each changed file whole.`
const rows = Object.entries(languages).flatMap(([name, row]) => [[name, row], ...(row.blender ? [[`${name} in Blender`, { ...row, ...row.blender }]] : [])])
const COMMANDS = [...[
  ...rows.map(([name, row]) => [`${row.format} <files>`, 'format', name]),
  ['nx affected -t check --exclude=rasm --files=<files joined by commas>', 'projects'],
  ...rows.flatMap(([name, row]) => [...row.lint.map(([tool, command]) => [`${command} <files>`, tool, name]), ...row.whole.map((target) => [`nx run rasm:${target}`, target, name])]),
  ['nx run rasm:lint:ast-grep:tree -- <files>', 'ast-grep'],
  ['python .claude/skills/clean-prose/scripts/prose.py check <files>', 'prose'],
].reduce((held, [command, tool, name]) => held.set(command, { tool, names: [...(held.get(command)?.names ?? []), ...(name ? [name] : [])] }), new Map())]
  .map(([command, { tool, names }]) => `- ${tool}${names.length > 0 ? ` for ${names.join(', ')}` : ''}: \`${command}\``)
const INSTALLS = [...new Set(rows.map(([, row]) => row.install))].map((command) => `\`${command}\``).join(', ')
const listed = (lines) => lines.map((line) => `- ${line}`).join('\n')
const live = (results, role) => {
  const dead = results.filter((result) => !result).length
  if (dead > 0) log(`${dead} ${role} returned nothing`)
  return results.filter(Boolean)
}
const mine = (lens, ask) => agent(`${READ}
Miner, ${lens}. Read ${ask}, one line each with its cause. Edit nothing. Return (structured) the findings.`, { label: `mine:${lens}`, phase: 'Mine', schema: FOUND })
const adversary = (index, lean) => agent(`${WRITE}
Adversary over ${KIT}. ${DIFF} Finish or revert each shuffle, restore each regression and apply in place every change the bar asks of the kit.${lean} Return at most 6 lines: what you changed.`, { label: `deepen:${index}`, phase: 'Deepen' })

// --- [COMPOSITION] ---------------------------------------------------------------------

phase('Mine')
const proofs = mine('proofs', `every proof run for ${[args.products].flat().join(', ')}: each workaround it needed and each case left unproven where the harness could not express it`)
const failures = await mine('failures', `crash reports in ~/Library/Logs/DiagnosticReports for ${args.name}, the harness run records under .artifacts/logs/harnesses/ and past run transcripts that drove it: each crash, hang, dialog and repeated call`)
if (!failures) log('failures miner returned nothing')
const model = await agent(`${READ}
Miner, host model. Read ${args.name}'s scripting model (dictionary, object model, introspection) against ${HARNESS}, with the failures mined from its runs:
${listed(failures?.findings ?? [])}
Needs: each area of the scripting model a real proof touches, named in the host's own terms with the classes or members it covers and the failures it shows.
Findings: each kind of host state a proof needs that the harness cannot set up, read or compare, with its cause. Edit nothing. Return (structured) the needs and the findings.`, { label: 'mine:model', phase: 'Mine', schema: { type: 'object', properties: { needs: LINES, findings: LINES }, required: ['needs', 'findings'] } })
if (!model) throw new Error(`model miner returned nothing for ${args.name}`)
if (model.needs.length === 0) throw new Error(`model miner derived no need from ${args.name}'s scripting model`)
log(`${model.needs.length} needs from ${args.name}'s scripting model`)

phase('Battery')
const authored = live(await pipeline(model.needs,
  (need, _, index) => agent(`${READ}
Case author for ${args.name}, need: ${need}. Write the cases a real proof in this area needs as the harness's case files under ${BATTERY}/${index + 1}/. Each case the harness cannot express is a finding. Return (structured) the findings.`, { label: `battery:author:${index + 1}`, phase: 'Battery', schema: FOUND }),
), 'case authors')
const ran = await agent(`${READ}
Battery runner. Run every case file under ${BATTERY}/ through the harness once, against each installed release of ${args.name}. Record each crash, hang, dialog, wrong result and missing capability with its cause. Return (structured) the findings.`, { label: 'battery:run', phase: 'Battery', schema: FOUND })
const findings = [...live([await proofs, failures, model, ran], 'miners and battery runner'), ...authored].flatMap((result) => result.findings)

phase('Edit')
const edited = findings.length > 0 ? await agent(`${WRITE}
Editor. Fix every finding below at its cause across ${KIT}, one general fix per shared cause, rebuilding a file whole where its fixes reshape it. Refuse a finding specific to one session or one that hides a failure, one line each.
${listed(findings)}
Return at most 12 lines: what you fixed and refused.`, { label: 'edit', phase: 'Edit' }) : 'no finding'
if (!edited) log('editor returned nothing')

phase('Deepen')
const first = await adversary(1, '')
const second = await adversary(2, ' Lean to collapsing existing types, constants, strings and functions into stronger logic.')
live([first, second], 'adversaries')

phase('Check')
const checked = await agent(`${WRITE}
Check over ${KIT}. <files> is the files \`git diff --name-only ${args.base} -- ${KIT}\` and \`git ls-files --others --exclude-standard -- ${KIT}\` list, less paths a tool does not read; drop each command whose tool reads none.
Run first the install of each language with a changed manifest among them (${INSTALLS}), then each command once:
${COMMANDS.join('\n')}
Run the battery under ${BATTERY}/ through the harness once. Fix each finding at its cause in one pass, then rerun the commands and the battery once.
Return at most 10 lines: findings fixed and any left with why.`, { label: 'check', phase: 'Check' })
return { edit: edited ?? 'editor returned nothing', deepen: [first, second].map((report) => report ?? 'adversary returned nothing'), check: checked ?? 'check agent returned nothing' }
