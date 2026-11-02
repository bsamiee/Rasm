export const meta = {
  name: 'harness-depth',
  description: 'Harden one host harness: mine past failures, run a battery of needs through it once, land root-cause fixes, deepen in place, one check',
  whenToUse: 'args.key the folder under tools/harnesses, args.name the host, args.apps the installed releases, args.skills the skills that drive it, args.products the product folders whose proofs used it.',
  phases: [
    { title: 'Mine', detail: 'readers over past proofs, failures and the host model' },
    { title: 'Battery', detail: 'case authors per need in parallel, one runner through the harness' },
    { title: 'Fix', detail: 'one lander fixes every finding at its cause' },
    { title: 'Deepen', detail: 'adversaries in parallel, each fixing its lens in place' },
    { title: 'Check', detail: 'checks and the battery once, fixes once, checks once' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const KIT = `${REPO}/tools/harnesses`
const HARNESS = `${KIT}/${args.key}`
const DIR = `${REPO}/.artifacts/workflows/harness-depth/${args.key}`
const BASIS = `Load Skill(manage-repo), Skill(watch-apps), ${args.skills} and the coding skill of each language you write first. The harness runs one host call per batch with every case isolated; it launches a fresh hidden host through \`watch-apps launch\`, builds real documents from each case's preconditions, reads whole answers in one call, presses only the dialog button a case names, and on any other dialog, a hang or a crash records it with the pid and leaves the instance running for the agent to act on. It serves every installed release of ${args.name} (${args.apps}). Findings already recorded (.artifacts/workflows/, docs/research, the facts index) are facts to act on, never to rediscover. Code is derived from the host's own facts: no stringly type, written-in version or path, wrapper, retry, guard or timeout that hides a failure, and no entry point a parameter serves. A fix lands at the cause as general capability. Touch only your own documents and processes.`
const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } } }, required: ['findings'] }
const flat = (results) => results.filter(Boolean).flatMap((r) => r.findings.map((f) => `- ${f}`)).join('\n')

phase('Mine')
const mined = flat(await parallel([
  ['proofs', `every proof run for ${args.products}: each workaround it needed and each case left unproven because the harness could not express it`],
  ['failures', `crash reports in ~/Library/Logs/DiagnosticReports for ${args.name} and past run transcripts that drove it: each crash, hang, dialog and repeated call with its cause`],
  ['model', `${args.name}'s scripting model (dictionary, object model, introspection) against ${HARNESS}: each kind of host state a proof needs that the harness cannot set up, read or compare`],
].map(([lens, ask]) => () => agent(`${BASIS}
Miner, ${lens} lens. Read ${ask}. One line each with its cause. Edit nothing. Return (structured) the findings.`, { label: `mine:${lens}`, phase: 'Mine', schema: FOUND }))))

phase('Battery')
const authored = await parallel(['documents and their setup', 'selection, active window and context', 'text, composition and right-to-left', 'units, numbers and precision', 'styles, tables and structured content', 'export and file round trips', 'errors, dialogs and recovery', 'plugins and script engines', 'host releases side by side'].map((need, i) => () => agent(`${BASIS}
Case author for ${args.name}, need: ${need}. Write the cases a real proof in this area needs as the harness's case files under ${DIR}/battery/${i + 1}/, without running them. Each case the harness cannot express is a finding. Return (structured) the findings.`, { label: `battery:author:${i + 1}`, phase: 'Battery', schema: FOUND })))
const ran = await agent(`${BASIS}
Battery runner. Run every case file under ${DIR}/battery/ through the harness once, against each installed release of ${args.name}. Record each crash, hang, dialog, wrong result and missing capability with its cause. Return (structured) the findings.`, { label: 'battery:run', phase: 'Battery', schema: FOUND })

phase('Fix')
const fixed = await agent(`${BASIS}
Lander. Fix every finding below at its cause across ${KIT} and ${HARNESS}, grouping findings that share a cause into one general fix and rebuilding a file whole where its fixes reshape it. Push back only on a session-specific finding or one that hides a failure, one line each.
${mined}
${flat(authored)}
${flat([ran])}
Return at most 12 lines: what landed.`, { label: 'fix', phase: 'Fix' })

phase('Deepen')
await parallel([
  ['information', `ways ${args.name} answers a proof's needs richer or in fewer calls than the harness asks now`],
  ['capability', 'capability a proof needs that the harness lacks or holds shallowly, folded into existing entry points'],
  ['reduction', 'types, variants, validations, constants, exports and hand logic no caller acts on differently, and restated host facts'],
].map(([lens, ask]) => () => agent(`${BASIS}
Adversary, ${lens} lens. The lander reported:
${fixed}
Read ${HARNESS} and ${KIT} as they now stand and fix in place every remaining ${ask}. Other adversaries edit beside you; re-read a file before each edit. Return at most 6 lines: what you changed.`, { label: `deepen:${lens}`, phase: 'Deepen' })))

phase('Check')
return agent(`${BASIS}
Check ${KIT}: run its nx project checks and the battery under ${DIR}/battery/ through the harness once, fix every finding at its cause once, then run them once more. Record what the battery settled in the facts index. Return at most 10 lines: findings fixed and any that remain.`, { label: 'check', phase: 'Check' })
