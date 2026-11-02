export const meta = {
  name: 'harness-depth',
  description: 'Harden one host harness: mine past proofs, run a battery of needs through it, land root-cause fixes, then two unanchored depth passes',
  whenToUse: 'One run per harness folder under tools/harnesses: args.key the folder, args.name the host, args.apps the installed releases, args.skills the skills that drive it, args.products the docs/research product folders whose proofs used it. One run per host at a time.',
  phases: [
    { title: 'Mine', detail: 'readers over past proofs, workarounds, crashes and dialogs' },
    { title: 'Battery', detail: 'case authors per need in parallel, one serial runner through the harness' },
    { title: 'Fix', detail: 'one architect groups every finding by cause, one lander lands the root fixes' },
    { title: 'Depth', detail: 'two identical unanchored passes, each lens readers then one adversarial lander' },
    { title: 'Check', detail: 'checks and a live batch in the background' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const KIT = `${REPO}/tools/harnesses`
const HARNESS = `${KIT}/${args.key}`
const DIR = `${REPO}/.artifacts/workflows/harness-depth/${args.key}`
const BASIS = `Load Skill(manage-repo), Skill(clean-prose), Skill(watch-apps), ${args.skills} and the coding skill of each language you write first. ${KIT}/README.md states the kit's shape: one host call per batch, every case isolated so a failure is recorded and never reaches the host as a dialog or crash, preconditions as data, results compared bit-exact, a facts index checked before any question. Each case runs in the host's real state with the host hidden from its launch (`watch-apps launch --hides`; an instance hidden after launch shows again at its next modal dialog, so a case that opens a dialog needs the launch-hidden instance): a real document, scene or canvas built from the case's preconditions, so selection, the active window and context work. The harness is version-independent (every installed release of ${args.name}: ${args.apps}) and asks the host for whole answers in one call through its object model, dictionary, introspection or MCP reads. Code holds the repository bar: logic derived and parameterized from the host's own facts, algebraic data, fewest entry points, exports and items, no stringly type, arbitrary constant, wrapper, forwarder, version or path written in, or hand-rolled fact a package or host API states. A fix lands at the cause as general capability, never as a case-specific branch, a new entry point a parameter serves, or a retry, guard or timeout that hides a failure. Hosts run in the background with \`watch-apps watch\` on them throughout; nothing activates an app or takes the owner's focus. Other runs drive these hosts: touch only your own documents and processes.`
const SILENT = 'Report only what changes the harness, each with its cause and source; say nothing about what holds.'
const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } } }, required: ['findings'] }
const flat = (results) => results.filter(Boolean).flatMap((r) => r.findings.map((f) => `- ${f}`)).join('\n')

const MINERS = [
  { lens: 'proofs', ask: `Read every proof the harness or its predecessors ran for ${args.products}: each product's proof/ notes, reports, case and result files, and host scripts. Name each workaround a proof needed (a fake selection, an added window, a skipped case, a stand-in environment), each case recorded as thrown or unproven because the harness could not express it, and each place a proof hand-wrote what the harness should hold.` },
  { lens: 'failures', ask: `Read ~/Library/Logs/DiagnosticReports for ${args.name}, and the agent transcripts under ~/.claude/projects/-Users-bardiasamiee-Developer-Rasm/*/subagents for runs that drove it (rg for its name beside osascript, do script, execute, eval, probe, harness). Name each crash, hang, dialog and repeated probe, its cause, and the general harness capability that removes it.` },
  { lens: 'model', ask: `Read ${args.name}'s scripting model whole (dictionary through \`sdef\`, object model types, introspection, MCP surfaces, command line) against ${HARNESS}. Name each kind of host fact or state a proof will need that the harness cannot set up, read or compare.` },
]

phase('Mine')
const mined = flat(await parallel(MINERS.map((m) => () => agent(`Miner for ${HARNESS}, ${m.lens} lens. ${m.ask} ${SILENT} Edit nothing. ${BASIS}
Return (structured): findings.`, { label: `mine:${m.lens}`, phase: 'Mine', schema: FOUND }))))

phase('Battery')
const authored = await parallel(['documents and their setup (intent, size, color, units, language)', 'selection, active window, view and context', 'text, composition and right-to-left', 'units, numbers, precision and rounding', 'styles, tables and structured content', 'export, rendering and file round trips', 'errors, dialogs, alerts and recovery', 'plugins, extensions and script engines the host loads', 'host releases side by side and version reporting'].map((need, i) => () => agent(`Case author for ${args.name}, need: ${need}. Write the cases a real proof in this area needs, as the harness's case files under ${DIR}/battery/${i + 1}/, covering edge, malformed and large inputs and every release installed, without running them. Where the harness cannot express a case, record that as a finding rather than working around it. ${SILENT} ${BASIS}
Return (structured): findings, each naming its case file.`, { label: `battery:author:${i + 1}`, phase: 'Battery', schema: FOUND })))
const ran = await agent(`Battery runner for ${HARNESS}. Run every case file under ${DIR}/battery/ through the harness's own entry points, one batch at a time, serially, against each installed release of ${args.name}, with \`watch-apps watch\` on it. Never probe outside the harness. Record each crash, hang, dialog, wrong result, thrown case, slow batch and missing capability with its cause. Authors' findings: ${flat(authored)}
${SILENT} ${BASIS}
Return (structured): findings.`, { label: 'battery:run', phase: 'Battery', schema: FOUND })

phase('Fix')
await agent(`Architect. Group every finding below by its cause and write ${DIR}/FIXES.md: for each cause, the general capability or rule in the harness that removes it, where it lives in the kit (host-neutral modules or ${HARNESS}), and the findings it closes. Reject a session-specific finding, one a parameter of an existing operation serves, and one that hides a failure, with one line why. Findings:
${mined}
${flat(authored)}
${flat([ran])}
${BASIS}
Return at most 12 lines: the causes and their fixes.`, { label: 'fix:plan', phase: 'Fix' })
const fixed = await agent(`Lander for ${DIR}/FIXES.md. Land each fix in its strongest form across ${KIT} and ${HARNESS}, rebuilding a file whole where its fixes reshape it, then rerun the battery cases each fix closes through the harness and confirm they pass. Refuse a plan step that would add surface without capability, with one line why. ${BASIS}
Return at most 15 lines: what landed, what you refused and why, battery results before and after.`, { label: 'fix:land', phase: 'Fix' })

const LENSES = [
  { lens: 'information', ask: `Every way ${args.name} can answer what a proof needs, richer and faster than the harness asks now. Do not trust the current approach; find where it guesses, asks piecemeal, runs a stand-in environment, or misses a kind of fact.` },
  { lens: 'capability', ask: 'Each capability a proof needs that the harness lacks or holds shallowly, folded into existing entry points as deeper logic, never a new command a parameter serves.' },
  { lens: 'derivation', ask: 'Each type, enum, error variant or validation no caller acts on differently, each piece of hand-rolled logic, restated host fact, version or path written in, stringly form, constant or export, and each branch an algebraic form or derived value states.' },
  { lens: 'abstraction', ask: `Whether each fix the lander made sits at the right level (host-neutral or ${args.key}), names the general capability rather than the case that exposed it, and leaves no special case behind.` },
]
const depth = (n) => async () => {
  const found = flat(await parallel(LENSES.map((l) => () => agent(`Depth reader for ${HARNESS} and ${KIT}, ${l.lens} lens. Read the harness whole as it stands, as a shallow draft. ${l.ask} ${SILENT} Edit nothing. ${BASIS}
Return (structured): findings.`, { label: `depth:${n}:${l.lens}`, phase: 'Depth', schema: FOUND }))))
  return agent(`Depth lander for ${HARNESS} and ${KIT}. Findings:
${found}
Judge each as an adversary: reject a weak or cosmetic one, one that adds an entry point a parameter serves, and one that trades a real host answer for a guess, each with one line why. Land the rest in their strongest form, rebuilding a file whole where its findings reshape it, and verify each change live through the battery cases it touches. ${BASIS}
Return at most 12 lines: what landed, what you rejected and why, exports before and after.`, { label: `depth:${n}:land`, phase: 'Depth' })
}

phase('Depth')
const first = await depth(1)()
const second = await depth(2)()

phase('Check')
const checked = await agent(`Check ${KIT}. Run its project checks and the linters for each language it holds through nx, then the whole battery under ${DIR}/battery/ through the harness against every installed release of ${args.name} with watch-apps watching. Fix every finding at its root. Record in the facts index what the battery settled. ${BASIS}
Return at most 10 lines: each command and the battery result, each fix, and \`git diff --shortstat\` over ${KIT}.`, { label: 'check', phase: 'Check' })
return { fixed, first, second, checked }
