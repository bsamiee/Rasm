export const meta = {
  name: 'rust',
  description: 'Build or rebuild one Rust crate on RUST.md: its code, consumers, checks and workspace rows',
  whenToUse: 'One run per RUST.md [03] row, args.key its [KEY]. Rust runs go one at a time, each once the rows its [AFTER] cell names have landed, since every run edits Cargo.toml, Cargo.lock and tools/dylint; one swift and one typescript run may go beside it.',
  phases: [
    { title: 'Plan', detail: 'parallel survey into a brief, then the crate\'s shape from RUST.md, its catalog crates and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'adversarial read, then check-and-fix' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${REPO}/.claude/workflows/RUST.md`
const BRIDGE = ['tools/bridge', 'tests/rust/tools/bridge']
const crate = (key, ...consumers) => [`libs/rust/${key}`, ...consumers, 'Cargo.toml', 'Cargo.lock']
const owned = {
  'services/telemetry': ['libs/rust', 'Cargo.toml', 'Cargo.lock'],
  'services/daemon': ['libs/rust/services', ...BRIDGE, 'Cargo.toml', 'Cargo.lock'],
  'services/mcp': crate('services/mcp', ...BRIDGE),
}[args.key] ?? crate(args.key)
const paths = [...owned, 'tools/dylint', 'tests/rust/tools/dylint', 'tools/ast-grep/rules/rust', 'tools/ast-grep/utils/rust', '.claude/workflows/RUST.md']
const DIR = `${REPO}/.artifacts/workflows/rust/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: Skill(rust-coding), loaded first, with rules and forms that decide every line's shape, and ${RULES} over all target code. The [03] row, its [01] family, rule 40's layout and the catalog rows it names are the specification and the architecture: the crate owns what the row states, built on those rows used fully, with nothing hand-rolled or hand-wrapped that a row provides. The architecture changes only where a crate source contradicts it, and the RUST.md row and family change with it in this run. Names come from the domain standard or the crate's own vocabulary, and an error variant exists only for a failure no catalog error states. Current code is dismantled and rebuilt, never moved or shaped by its present form, and iterated until the crate holds the fewest public and internal items and types the row allows. Present code and consumers hold no authority: each crate is built the way a published crate is, for downstream software no one has written yet, capability the current code holds is kept where it is general, and every consumer is rebuilt onto the new form. Software engineering practice and Rust's own patterns decide layout: a file holds one concept's items, no folder holds a single file, no folder sprawls flat with many small files, no file grows oversized, and no lib, main or mod file only collects modules. Each pub item serves an operation such software performs, never a host's, an item with no present caller stays and a weak one is rebuilt, and one parameterized item states what two would. A constant, type, record or helper exists only where it states a fact no package member, generic, derived value or argument with a default states, and a consumer reaches a variant by changing an argument, never by declaring or instantiating its own. Logic built poorly is deleted whole and rebuilt in its best form, never kept beside its replacement, wrapped, aliased or bridged. A concept sits where the architecture places it: libraries hold general capability any future app composes, apps hold their own content, and a misplaced concept is rebuilt in its new place, never relocated as the same code. Each package depends only on packages with members it calls, with no cycle and no edge a misplaced concept creates. A change improves code only when its result does more with less than its base, and no rule, row cell or old code alone justifies an item. A library and its consumers are rebuilt together and neither shapes the other: design starts from the domain and the operations downstream software performs, the crate states each in its strongest form, and each consumer calls it directly with no adapter, wrapper or reshaping. A rules citation names code at the base tree, and where an earlier run moved that code, the rule applies at its new location. Every consumer (bridge, tests, dylint examples, ast-grep rule scopes, catalog rows) is rebuilt to the new form in this run.`
const SOURCES = `Read crate behavior from each crate's source at the newest release \`cargo info <crate>\` names (~/.cargo/registry/src/index.crates.io-*/<crate>-<version>/, present once \`cargo fetch\` has run on the moved row) through the search-code skill, cited by path and line; the nightly's forms from ${REPO}/rust-toolchain.toml and ${REPO}/docs/research/coding/rust/ (language.md, language-features.md, crates.md, crate-members.md, mcp.md, toolchain.md, architecture.md, ast-grep/), where RUST.md and a lint row win over a sheet; workspace rules from Cargo.toml, clippy.toml, .cargo/config.toml, rustfmt.toml and the manage-repo skill's Rust reference; forbidden forms from the checks in tools/dylint and tools/ast-grep/rules/rust.`
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const BRIEF = `${DIR}/brief`
const GROUND = [
  { key: 'code', ask: `where code exists, the base surface, measured on \`git archive HEAD\` of the target paths extracted under ${DIR}/base: per file and crate, lines \`loc\` counts and items and exports \`ast-grep outline --json=stream\` lists under the language's outline rules in tools/ast-grep/outline; then every item of the target with its file, line and purpose, every call site of each pub item in its consumers, and the workspace rows that name it` },
  { key: 'members', ask: 'the members of every catalog row the [03] row and its [01] family name, read from each crate\'s source at the release Cargo.toml pins: purpose, signature and gating feature of each' },
]
await parallel(GROUND.map(g => () => agent(`Survey for the build of ${TARGET} Write ${BRIEF}/${g.key}.md: ${g.ask}. Facts with citations, no design. Edit nothing else. ${SOURCES}`, { label: `ground:${g.key}`, phase: 'Plan' })))
const author = await agent(`Read ${BRIEF}/*.md, the target's [03] row and the code where the design needs more than the brief states. ${TARGET}
${BASIS}
Write ${PLAN}: every file of the crate in the layout RUST.md states (crate directory under libs/rust/<category>/<crate>/), each mapped to the items it holds, the catalog members and nightly gates it calls, what derives and what stays by hand, a consumer call, every deletion; every \`pub\` item with the app operation that composes it; the manifest and the root Cargo.toml rows ([MEMBERS] row, catalog rows at their newest release with the features the role needs, rows its family's members in [01] or ${REPO}/docs/research/coding/rust/crate-members.md need); then packages, each with its files (consumers included), items, callers rebuilt, deletions and the earlier packages that supply items it reads. Close with the \`pub\` items per crate before and after, and every behavior of the current code with the item that keeps it. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarial architect. Read ${BRIEF}/*.md and ${PLAN}, and treat the plan as a draft to challenge within the architecture RUST.md states: every file, item, derive, error variant, name, dependency edge, catalog row and package stands only where the [03] row, its family, the brief's facts and the Basis support it; a file too small to stand alone, a god file, hand-rolled or repeated logic and an item with no capability behind it are rebuilt, and whatever the sources do not support is rewritten in the plan, never annotated or listed for later. Check each claim the brief cites against the code or crate source. ${TARGET}
${BASIS}
Rewrite the plan in place so each package section is final. Cargo.toml, tools/dylint, tests/rust/tools/dylint, tools/ast-grep/rules/rust, tools/ast-grep/utils/rust sit in packages that list no source file, source packages never queue on a shared file, and check configuration changes only where a rule misfires on correct code; no package lists a lock file, the Check phase resolves it. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add an item it reads).

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
const reviews = (await parallel(groups.map((g, i) => () => agent(`Adversarial reviewer of packages ${g.ids.join(', ')}. Edit only these files: ${g.files.join(', ')}. Trust nothing the land agents wrote: read each file against ${PLAN} and its base (\`git show HEAD:<path>\`). ${TARGET} ${BASIS}
Fix in place: code moved as it stands; lines, items or exports grown beyond what a capability the [03] row names requires; code no downstream operation needs; a pub constant, record or struct where a parameter, a derived value or a crate type states the fact; a consumer left on the old form; a form a crate member, a nightly gate or a workspace lint states; a struct, enum, bool, tuple or macro standing in for a shape a crate type, derive or generic fn states; a crate used beside hand code it provides; logic a catalog member states written by hand; a \`pub\` item no app operation composes, a wrapper, forwarder or re-export, or fields or functions repeated per kind; a fixed value in a composition where a default a caller overrides belongs; app or vendor content in a library crate; a RUST.md or CLAUDE.md breach; behavior the current code or a consumer has that the new code drops; capability the [03] row names left unbuilt. Where a stronger approach exists (a crate member, a derive, a trait or generic, an algebraic form, a parameter, a derived value), replace the file or block whole; extracting repeated code into a helper is no fix, and nothing poor survives softened or moved. Restore only what the base did objectively better. ${SOURCES}
Return (structured): defects fixed (at most 20 lines); each RUST.md, workflow or skill line that steered a defect, quoted (empty when none); files you edited.`, { label: `adversary:${i + 1}`, phase: 'Check', schema: { type: 'object', properties: { summary: { type: 'string' }, steering: { type: 'array', items: { type: 'string' } }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'steering', 'files'] } })))).filter(Boolean)
const files = [...new Set([...touched, ...reviews.flatMap(r => r.files)])]
const built = await agent(`Check-and-fix. Packages landed, then the adversaries' fixes:
${summaries}
${reviews.map((r, i) => `- adversary ${i + 1}: ${r.summary.slice(0, 1200)}`).join('\n')}
Fix across crate seams in place: one concept in two crates, a conversion written twice, a lint row lowered or an \`allow\` or \`expect\` attribute, a catalog row off the release rule 3 allows or out of its section. Fix each RUST.md or .claude/skills/rust-coding line the adversaries name as steering a defect, as a general principle: ${reviews.flatMap(r => r.steering).join(' | ') || 'none named'}.
Run in ${REPO}: \`cargo fetch\` when a manifest changed, then \`nx run <project>:check\` for the target crate and each workspace crate \`cargo tree --invert --workspace --package <target>\` lists, \`nx run lints:check\` when this run edited tools/dylint, \`nx run rasm:lint:rustfmt -- --package <crate>\` for each of those crates, \`nx run rasm:lint:tombi -- <the TOML files among these>\` and \`nx run rasm:lint:ast-grep -- <files>\` over ${files.join(', ')}, and \`nx run rasm-mcp-bridge:test:integration\` when that list holds the bridge. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a lowered lint, an \`allow\` or an \`expect\`); list each finding in other files with file and message. Against the plan's behavior list and the [03] row, restore each behavior the landed code drops and build each capability left unbuilt, in place. Measure the working tree's surface the way the brief measured base and write ${DIR}/surface.md with base and after per file and crate; where code existed, each crate ends at or below its base in lines and pub items, growth stands only for a capability the base lacked entirely and is stated per item in surface.md, and growth from restating, wrapping or splitting is removed in place before you return. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, each behavior restored or capability built, the \`pub\` items per crate before and after, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, reviews, built }
