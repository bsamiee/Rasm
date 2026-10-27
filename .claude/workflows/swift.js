export const meta = {
  name: 'swift',
  description: 'Rebuild one Swift module cluster on SWIFT.md: its code, consumers, checks and package rows',
  whenToUse: 'One run per SWIFT.md [03] row, args.key its [KEY]; a run starts once the rows its [AFTER] cell names have landed and runs beside every other run, future-rust and future-typescript included.',
  phases: [
    { title: 'Plan', detail: 'the cluster\'s shape from SWIFT.md, its frameworks and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'adversarial read, then check-and-fix' },
  ],
}

const W = '/Users/bardiasamiee/.claude/projects/-Users-bardiasamiee-Developer-Rasm/host-ui-rework'
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${W}/future-langs/SWIFT.md`
const CHECK_TREES = ['tools/ast-grep/rules/swift', 'tools/ast-grep/utils/swift', '.swiftlint.yml', '.swift-format', '.config/periphery.yml', '.config/periphery.json']
const ROOT = ['Package.swift', 'Package.resolved', 'Xcode.xcconfig']
const RELAY = ['apps/macos/relay']
const ATELIER = ['apps/creative-cloud/atelier/macos']
const TARGETS = {
  core: { title: 'Results, Encoding and JSONRPC, with EffectRPC merged into JSONRPC', paths: ['libs/swift', 'tests/swift/libs/core', 'tools/watch-apps', ...RELAY, ...ATELIER] },
  'macos/process': { title: 'CurrentProcess, Files and Processes, with Lockfiles merged into Files and Keychain into Relay', paths: ['libs/swift/macos/CurrentProcess', 'libs/swift/macos/Files', 'libs/swift/macos/Lockfiles', 'libs/swift/macos/Keychain', 'libs/swift/macos/Processes', 'libs/swift/macos/Applications', 'tests/swift/libs/macos', ...RELAY, ...ATELIER] },
  'macos/applications': { title: 'Applications and AppleEvents, with Fonts merged into Applications and the creative-cloud modules into AppleEvents and the Atelier helper', paths: ['libs/swift/macos/Applications', 'libs/swift/macos/AppleEvents', 'libs/swift/macos/Fonts', 'libs/swift/creative-cloud', 'tests/swift/libs/macos', 'tools/watch-apps', ...ATELIER] },
}
const { title, paths: owned } = TARGETS[args.key]
const paths = [...owned, ...ROOT, ...CHECK_TREES]
const DIR = `${W}/future-langs/swift/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: ${title}, row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: ${RULES} over all the target's code, with ${REPO}/CLAUDE.md and the memory file ${W.replace('/host-ui-rework', '')}/memory/unused-public-api-stays.md. The [03] row is the specification: the modules own what the row states, built on the frameworks and pinned packages used fully, and hold nothing the row and the apps do not state. Code that exists is remade, never moved: current code and its callers have no authority, its idioms are the quality floor and not the architecture, every consumer the [03] row names and every Package.swift and .xcodeproj package row is rebuilt to the new form in this run, and counted code lines, files and public declarations fall. Logic derives from a framework member, a pinned package member or the structure a caller supplies, and a hand form beside a member that states it is a defect.`
const SOURCES = `Read framework behavior from the macOS 27 SDK (\`xcrun --show-sdk-path\`, the .swiftinterface files and \`xcrun swift-synthesize-interface -include-submodules\`, the headers) and Apple documentation through the xcode MCP, cited by declared name and availability; package behavior from the pinned checkouts (swift-subprocess 1.0.2, swift-async-algorithms 1.1.7, swift-case-paths 1.10.0, swift-collections 1.7.3, swift-argument-parser 1.8.2 under $SWIFTPM_BUILD_DIR/checkouts/) through the search-code skill, cited by file and line; the toolchain's forms from Package.swift settings and Xcode.xcconfig; the workspace rules from the manage-repo skill's Swift reference and ${REPO}/docs/research/coding/swift/ where present; the checks from tools/ast-grep/rules/swift, .swiftlint.yml, .swift-format and Periphery, which decide what code may not say; the TypeScript wire readers libs/typescript/creative-cloud/automation/*.ts and apps/creative-cloud/atelier/status.ts read only.`
const CHECKS = `code moved as it stands, other than the merges the target title names; a consumer left on the old form; a form a framework member, a package member or a check states; a struct, enum, optional pair, Bool or string reason standing in for a shape a framework type, a Result or a typed failure states; a framework or package used beside hand code it provides; a product imported for one member; a caller's configuration overwritten or a fixed value where a parameter with a default belongs; app content in a library module; a public declaration that is weak, redundant or wrongly shaped, a wrapper or a forwarder; a wire spelling changed; a SWIFT.md or CLAUDE.md breach; counted lines, files or public declarations that grow`
const PKG = { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, needs: { type: 'array', items: { type: 'string' } } }, required: ['id', 'title', 'section', 'files', 'needs'] }
const GRAPH = { type: 'object', properties: { packages: { type: 'array', items: PKG } }, required: ['packages'] }
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const author = await agent(`Read the target's [03] row, the modules whole with every consumer of their public declarations, the Package.swift and .xcodeproj rows that name them, and the framework and package members SWIFT.md cites. ${TARGET}
${BASIS}
Write ${PLAN}: the target's shape after this work (module folders under libs/swift/<category>/<Module>/, one file per concept, each with what it holds), each concept's one location and form, the framework or package member it depends on, a consumer call, every deletion and merge; every public declaration with the app operation that composes it; the Package.swift rows (targets, dependencies per module, products) and the .xcodeproj package-product rows after; the wire spellings the target writes with their readers; one check package for each recurring mistake the plan finds that no check reports, in the one tool that decides it (a tools/ast-grep/rules/swift rule with its utils when syntax decides, a .swiftlint.yml rule row when its rule set or analyzer owns the fact, a .swift-format rule when it owns it, none when no tool decides); then packages, each with its files (consumers included), declarations, callers rebuilt, deletions and the earlier packages that supply declarations it reads. Close with the counted code lines (loc), files and public declarations per module before and after. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarially refine ${PLAN} (author summary below) against the code, ${RULES}, the SDK and the checkouts. Find in every package: ${CHECKS}. ${TARGET} ${SOURCES}
Rewrite the plan in place so each package section is final. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add a declaration it reads).

${author}`, { label: 'challenge', phase: 'Plan', schema: GRAPH })

phase('Land')
const overlaps = (a, b) => a.files.some(f => b.files.some(g => f.startsWith(g) || g.startsWith(f)))
const land = (p, landed) => agent(`Land package ${p.id} (${p.title}) exactly as ${PLAN} section "${p.section}" states. Edit only these files: ${p.files.join(', ')}. ${BASIS} ${SOURCES}${landed.length ? `\nPackages it reads, landed:\n${landed.join('\n')}` : ''}
Return (structured): summary of what landed per plan item (at most 25 lines); files you edited.`, { label: `land:${p.id}`, phase: 'Land', schema: LANDED })
const results = await new Promise(resolve => {
  const ids = new Set(graph.packages.map(p => p.id))
  const queue = [...graph.packages], running = new Map(), done = new Map()
  const finish = () => resolve(graph.packages.map(q => ({ id: q.id, ...(done.get(q.id) || { summary: 'not started: its needs never landed', files: [] }) })))
  const pump = () => {
    for (const p of [...queue]) {
      if (!p.needs.every(n => done.has(n) || !ids.has(n)) || [...running.values()].some(r => overlaps(r, p))) continue
      queue.splice(queue.indexOf(p), 1)
      running.set(p.id, p)
      const landed = p.needs.filter(n => done.has(n)).map(n => `- ${n}: ${done.get(n).summary.slice(0, 600)}`)
      land(p, landed).then(r => r, () => null).then(r => {
        running.delete(p.id)
        done.set(p.id, r || { summary: 'agent died', files: [] })
        log(`${p.id} ${r ? 'landed' : 'died'}`)
        pump()
      })
    }
    if (running.size === 0) {
      if (queue.length) log(`${queue.map(p => p.id).join(', ')} never started: needs form a cycle`)
      finish()
    }
  }
  pump()
})

phase('Check')
const summaries = results.map(r => `- ${r.id}: ${r.summary.slice(0, 600)}`).join('\n')
const touched = [...new Set(results.flatMap(r => r.files))]
const adversary = await agent(`Adversarial agent. Packages landed:
${summaries}
Read these files against ${PLAN}: ${touched.join(', ')}. ${TARGET} ${BASIS} Find and fix in place: ${CHECKS}. Hunt across module seams: one concept in two modules, a codec written twice, a declaration deleted on its consumer count alone, a swiftlint:disable or a removed rule, a wire spelling changed against SWIFT.md item 15, a test proving nothing. ${SOURCES}
Return (structured): defects fixed (at most 25 lines); files you edited.`, { label: 'adversary', phase: 'Check', schema: LANDED })
const files = [...new Set([...touched, ...(adversary ? adversary.files : [])])]
const built = await agent(`Check-and-fix. Packages landed, then the adversary's fixes:
${summaries}
- adversary: ${adversary ? adversary.summary.slice(0, 1500) : 'agent died'}
Run in ${REPO}: \`nx run <project>:check\` for every Swift project (\`nx show projects --projects tag:language:swift\`, the Xcode apps included), \`nx run rasm:lint:swiftlint\`, \`nx run rasm:lint:swift-format\`, \`nx run rasm:lint:xcprojformatter\`, \`nx run rasm:lint:ast-grep\`, and \`ast-grep scan\` over ${files.join(', ')}. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a disabled rule or a swiftlint:disable); list each finding in other files with file and message. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, the counted code lines, files and public declarations per module against the plan's numbers, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, adversary, built }
