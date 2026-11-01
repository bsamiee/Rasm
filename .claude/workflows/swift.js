export const meta = {
  name: 'swift',
  description: 'Rebuild one Swift module cluster on SWIFT.md: its code, consumers, checks and package rows',
  whenToUse: 'One run per SWIFT.md [03] row, args.key its [KEY]. Swift runs go one at a time, each once the rows its [AFTER] cell names have landed, since every run edits Package.swift, Package.resolved and the Swift check configuration; one rust and one typescript run may go beside it.',
  phases: [
    { title: 'Plan', detail: 'parallel survey into a brief, then the cluster\'s shape from SWIFT.md, its frameworks and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'per file group an adversary, then a ground-up rebuild, then check-and-fix' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${REPO}/.claude/workflows/SWIFT.md`
const RELAY = ['apps/macos/relay']
const ATELIER = ['apps/creative-cloud/atelier/macos']
const owned = {
  core: ['libs/swift', 'tools/watch-apps', ...RELAY, ...ATELIER],
  'macos/process': ['libs/swift/macos/CurrentProcess', 'libs/swift/macos/Files', 'libs/swift/macos/Lockfiles', 'libs/swift/macos/Keychain', 'libs/swift/macos/Processes', 'libs/swift/macos/Applications', ...RELAY, ...ATELIER],
  'macos/applications': ['libs/swift/macos/Applications', 'libs/swift/macos/AppleEvents', 'libs/swift/macos/Fonts', 'libs/swift/creative-cloud', 'tools/watch-apps', ...ATELIER],
}[args.key]
const paths = [...owned, 'Package.swift', 'Package.resolved', 'Xcode.xcconfig', 'tools/ast-grep/rules/swift', 'tools/ast-grep/utils/swift', '.swiftlint.yml', '.swift-format', '.config/periphery.yml', '.config/periphery.json']
const DIR = `${REPO}/.artifacts/workflows/swift/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: Skill(swift-coding), loaded first, with rules and forms that decide every line's shape, and ${RULES} over all target code. The [03] row is the specification: modules own what it states, built on the frameworks and catalog packages used fully, with nothing hand-rolled or hand-wrapped that one provides. Current code is dismantled and rebuilt, never moved or shaped by its present form, and iterated until modules hold the fewest public and internal declarations and types the row allows, and nothing the current code or its consumers do is lost. Software engineering practice and Swift's own patterns decide layout: a file holds one concept's declarations, no folder holds a single file, no folder sprawls flat with many small files, no file grows oversized, and no lib, main, index or mod file collects a package's logic. Present consumers name behavior to keep and never shape the surface: each public declaration serves the kind of operation the hosts' apps do, a declaration with no present caller stays, a weak one is rebuilt, and one parameterized declaration states what two would. A constant, type, record or helper exists only where it states a fact no package member, generic, derived value or argument with a default states, and a consumer reaches a variant by changing an argument, never by declaring or instantiating its own. Logic built poorly is deleted whole and rebuilt in its best form, never kept beside its replacement, wrapped, aliased or bridged. A concept sits where the architecture places it: libraries hold general capability any future app composes, apps hold their own content, and a misplaced concept is rebuilt in its new place, never relocated as the same code. Each package depends only on packages with members it calls, with no cycle and no edge a misplaced concept creates. A change improves code only when its result does more with less than its base: every declaration earns its place through an operation a caller performs, never through a rule, a row cell or the old code alone. A library and its consumers are rebuilt together and neither shapes the other: design starts from the domain and the operations consumers perform, the library states each in its strongest form, and each consumer calls it directly with no adapter, wrapper or reshaping. A rules citation names code at the base tree, and where an earlier run moved that code, the rule applies at its new location. Every consumer the [03] row names and every Package.swift and .xcodeproj package row is rebuilt to the new form in this run.`
const SOURCES = `Read framework behavior from the macOS 27 SDK (\`xcrun --show-sdk-path\`, the .swiftinterface files and \`xcrun swift-synthesize-interface -include-submodules\`, the headers) and Apple documentation through the xcode MCP, cited by declared name and availability; package behavior from the pinned checkouts (swift-subprocess, swift-async-algorithms, swift-case-paths, swift-collections and swift-argument-parser under $SWIFTPM_BUILD_DIR/checkouts/) through the search-code skill, cited by file and line; the toolchain's forms from Package.swift settings and Xcode.xcconfig; the apps' kind of work from apps/ and docs/research/applications; the workspace rules from the manage-repo skill's Swift reference and ${REPO}/docs/research/coding/swift/; the checks from tools/ast-grep/rules/swift, .swiftlint.yml, .swift-format and Periphery, which decide what code may not say; the TypeScript wire readers under libs/typescript/creative-cloud/automation, libs/typescript/data/search and apps/creative-cloud/atelier read only.`
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const BRIEF = `${DIR}/brief`
const GROUND = [
  { key: 'code', ask: `the base surface, measured on \`git archive HEAD\` of the target paths extracted under ${DIR}/base: per file and module, lines \`loc\` counts and declarations and exports \`ast-grep outline --json=stream\` lists under the language's outline rules in tools/ast-grep/outline; then every declaration of the target modules with its file, line and purpose, every call site of each public declaration in the consumers the [03] row names, and the Package.swift and .xcodeproj rows naming the modules` },
  { key: 'members', ask: 'the framework and package members SWIFT.md cites and those the target\'s work reaches, read from the SDK and the pinned checkouts: purpose, signature and availability of each' },
  { key: 'wire', ask: 'every key, _tag, case and enum spelling the target writes, with the TypeScript reader file and line that decodes it' },
]
await parallel(GROUND.map(g => () => agent(`Survey for the rebuild of ${TARGET} Write ${BRIEF}/${g.key}.md: ${g.ask}. Facts with citations, no design. Edit nothing else. ${SOURCES}`, { label: `ground:${g.key}`, phase: 'Plan' })))
const author = await agent(`Read ${BRIEF}/*.md, the target's [03] row and the code where the design needs more than the brief states. ${TARGET}
${BASIS}
Write ${PLAN}: the target's final shape (module folders under libs/swift/<category>/<Module>/, files grouped by concept, each with what it holds), each concept's one location and form, the framework or package member it depends on, a consumer call, every deletion and merge; every public declaration with the app operation that composes it; the Package.swift rows (targets, dependencies per module, products) and the .xcodeproj package-product rows after; the wire spellings the target writes with their readers; then packages, each with its files (consumers included), declarations, callers rebuilt, deletions and the earlier packages that supply declarations it reads. Close with the public declarations per module before and after, and every behavior of the current code with the declaration that keeps it. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarial architect. Read ${BRIEF}/*.md and ${PLAN}, and treat the plan as a draft to challenge: every module boundary, file, public declaration, dependency edge and package stands only where the [03] row, the brief's facts and the Basis support it, and what they do not support is rewritten in the plan, never annotated or listed for later. Check each claim the brief cites against the code. ${TARGET}
${BASIS}
Rewrite the plan in place so each package section is final. Package.swift, Xcode.xcconfig, tools/ast-grep/rules/swift, tools/ast-grep/utils/swift, .swiftlint.yml, .swift-format, .config/periphery.yml, .config/periphery.json sit in packages that list no source file, source packages never queue on a shared file, and check configuration changes only where a rule misfires on correct code; no package lists a lock file, the Check phase resolves it. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add a declaration it reads).

Author summary:
${author}`, { label: 'challenge', phase: 'Plan', schema: { type: 'object', properties: { packages: { type: 'array', items: { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, needs: { type: 'array', items: { type: 'string' } } }, required: ['id', 'title', 'section', 'files', 'needs'] } } }, required: ['packages'] } })

phase('Land')
const ids = new Set(graph.packages.map(p => p.id))
const pending = [...graph.packages]
const done = new Map()
const died = new Set()
const pump = () => {
  const active = graph.packages.filter(q => !(pending.includes(q) || done.has(q.id) || died.has(q.id)))
  const overlaps = (a, b) => a.files.some(f => b.files.some(g => f.startsWith(g) || g.startsWith(f)))
  const ready = pending.reduce((picked, p) => {
    if (p.needs.every(n => done.has(n) || !ids.has(n)) && !active.concat(picked).some(q => overlaps(q, p))) {
      picked.push(p)
    }
    return picked
  }, [])
  return parallel(ready.map(p => () => {
    pending.splice(pending.indexOf(p), 1)
    const landed = p.needs.filter(n => done.has(n)).map(n => `- ${n}: ${done.get(n).summary.slice(0, 600)}`)
    return agent(`Land package ${p.id} (${p.title}) exactly as ${PLAN} section "${p.section}" states. Edit only these files: ${p.files.join(', ')}. ${BASIS} ${SOURCES}${landed.length ? `\nPackages it reads, landed:\n${landed.join('\n')}` : ''}
Run no build, test, checker, resolver or nx target: the Check phase resolves locks and runs every check once over the whole run.
Return (structured): summary of what landed per plan item (at most 25 lines); files you edited.`, { label: `land:${p.id}`, phase: 'Land', schema: LANDED }).then(r => {
      if (r) {
        done.set(p.id, r)
      } else {
        died.add(p.id)
      }
      log(`${p.id} ${r ? 'landed' : 'died'}`)
      return pump()
    })
  }))
}
await pump()
if (pending.length > 0) {
  log(`${pending.map(p => p.id).join(', ')} never started: a need died or the needs form a cycle`)
}
const results = graph.packages.map(q => ({ id: q.id, ...(done.get(q.id) || { summary: died.has(q.id) ? 'agent died' : 'not started: its needs never landed', files: [] }) }))

phase('Check')
const summaries = results.map(r => `- ${r.id}: ${r.summary.slice(0, 600)}`).join('\n')
const touched = [...new Set(results.flatMap(r => r.files))]
const near = (a, b) => a.some(f => b.some(g => f.startsWith(g) || g.startsWith(f)))
const groups = results.filter(r => r.files.length > 0).reduce((all, r) => {
  const joined = all.filter(g => near(g.files, r.files))
  return [...all.filter(g => !joined.includes(g)), { ids: [r.id, ...joined.flatMap(g => g.ids)], files: [...new Set([...r.files, ...joined.flatMap(g => g.files)])] }]
}, [])
const reviews = (await pipeline(groups, (g, _item, i) => agent(`Adversarial reviewer of packages ${g.ids.join(', ')}. Edit only these files: ${g.files.join(', ')}. Trust nothing the land agents wrote: read each file against ${PLAN} and its base (\`git show HEAD:<path>\`). ${TARGET} ${BASIS}
Fix in place: code moved as it stands; lines, declarations or exports grown beyond what a capability the [03] row names requires; code a caller's operation does not need; an exported constant, record or struct where a parameter, a derived value or a framework type states the fact; a consumer left on the old form; a form a framework member, a package member or a check states; a struct, enum, optional pair, Bool or string reason standing in for a shape a framework type, a Result or a typed failure states; a framework or package used beside hand code it provides; a product imported where the standard library or a framework states the member; one value or step written twice where one parameterized declaration states both; a caller's configuration overwritten or a fixed value where a parameter with a default belongs; app content in a library module; a public declaration that is weak, redundant or wrongly shaped, a wrapper or a forwarder; a wire spelling changed; a SWIFT.md or CLAUDE.md breach; behavior the current code or a consumer has that the new code drops; capability the [03] row names left unbuilt. Where a stronger approach exists (a framework or package member, an algebraic form, a parameter, a derived value), replace the file or block whole; extracting repeated code into a helper is no fix, and nothing poor survives softened or moved. Restore only what the base did objectively better. ${SOURCES}
Return (structured): defects fixed (at most 20 lines); each SWIFT.md, workflow or skill line that steered a defect, quoted (empty when none); files you edited.`, { label: `adversary:${i + 1}`, phase: 'Check', schema: { type: 'object', properties: { summary: { type: 'string' }, steering: { type: 'array', items: { type: 'string' } }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'steering', 'files'] } }), (first, g, i) => agent(`Ground-up rebuild of packages ${g.ids.join(', ')}, after their first adversary. Edit only these files: ${g.files.join(', ')}. For each file, work out what its callers do with it, then write the strongest form that serves exactly that: values derived from their owner, the fewest lines and members, flat control flow, no indirection, wrapper, forwarder or alias, and every package member used for what it states. Keep a rewrite only where it is objectively better than the file as it stands, with fewer lines or members and the same behavior and wire, and leave a file as it stands when nothing stronger exists. ${TARGET} ${BASIS} ${SOURCES}
Return (structured): what you rebuilt (at most 20 lines); each SWIFT.md, workflow or skill line that steered a defect, quoted (empty when none); files you edited.`, { label: `rebuild:${i + 1}`, phase: 'Check', schema: { type: 'object', properties: { summary: { type: 'string' }, steering: { type: 'array', items: { type: 'string' } }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'steering', 'files'] } }).then((second) => [first, second]))).flat().filter(Boolean)
const files = [...new Set([...touched, ...reviews.flatMap(r => r.files)])]
const built = await agent(`Check-and-fix. Packages landed, then the adversaries' fixes:
${summaries}
${reviews.map((r, i) => `- adversary ${i + 1}: ${r.summary.slice(0, 1200)}`).join('\n')}
Fix across module seams in place: one concept in two modules, a codec written twice, a declaration deleted on its consumer count alone, a swiftlint:disable or a removed rule, a wire spelling changed against SWIFT.md item 14. Fix each SWIFT.md or .claude/skills/swift-coding line the adversaries name as steering a defect, as a general principle: ${reviews.flatMap(r => r.steering).join(' | ') || 'none named'}.
Run in ${REPO}: when Package.swift or an .xcodeproj package row changed, \`swift package resolve\` then \`nx run-many -t upgrade:xcode --projects tag:build:xcode\`; then \`nx run-many -t check --projects tag:language:swift\` (the Xcode apps included), \`nx run rasm:lint:swiftlint\`, \`nx run rasm:lint:swift-format\`, \`nx run rasm:lint:xcprojformatter\`, and \`nx run rasm:lint:ast-grep -- <files>\` over ${files.join(', ')}. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a disabled rule or a swiftlint:disable); list each finding in other files with file and message. Against the plan's behavior list and the [03] row, restore each behavior the landed code drops and build each capability left unbuilt, in place. Measure the working tree's surface the way the brief measured base and write ${DIR}/surface.md with base and after per file and module; each module ends at or below its base in lines and public declarations, growth stands only for a capability the base lacked entirely and is stated per item in surface.md, and growth from restating, wrapping or splitting is removed in place before you return. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, each behavior restored or capability built, the public declarations per module before and after, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, reviews, built }
