export const meta = {
  name: 'rust',
  description: 'Build or rebuild one Rust crate on RUST.md: its code, tests, consumers, checks and workspace rows',
  whenToUse: 'One run per RUST.md [03] row, args.key its [KEY]. Rust runs go one at a time, each once the rows its [AFTER] cell names have landed, since every run edits Cargo.toml, Cargo.lock and tools/dylint; one swift and one typescript run may go beside it.',
  phases: [
    { title: 'Plan', detail: 'the crate\'s shape from RUST.md, its catalog crates and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'adversarial read, then check-and-fix' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${REPO}/.claude/workflows/RUST.md`
const BRIDGE = ['tools/bridge', 'tests/rust/tools/bridge']
const crate = (key, ...consumers) => [`libs/rust/${key}`, `tests/rust/libs/${key}`, ...consumers, 'Cargo.toml', 'Cargo.lock']
const owned = {
  'services/telemetry': ['libs/rust', 'tests/rust/libs', 'Cargo.toml', 'Cargo.lock'],
  'services/daemon': ['libs/rust/services', 'tests/rust/libs/services', ...BRIDGE, 'Cargo.toml', 'Cargo.lock'],
  'services/mcp': crate('services/mcp', ...BRIDGE),
}[args.key] ?? crate(args.key)
const paths = [...owned, 'tools/dylint', 'tests/rust/tools/dylint', 'tools/ast-grep/rules/rust', 'tools/ast-grep/utils/rust']
const DIR = `${REPO}/.artifacts/workflows/rust/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: Skill(rust-coding) loaded first, whose rules and forms decide the shape of every line, and ${RULES} over all the target's code. The [03] row and the catalog rows it names are the specification: the crate owns what the row states, built on those rows used fully, with nothing hand-rolled or hand-wrapped that a row provides. The current code is torn apart and rebuilt, never moved or shaped by its present form, and iterated until the crate holds the fewest public and internal items and types the row allows; nothing the current code or its consumers do is lost. Files hold one concept each: no file collects a package's logic (a lib, main, index or mod file holding more than its root), and no folder holds a single file. Present consumers name the behavior to keep and never shape the surface: each public declaration serves the kind of operation the hosts' apps do, a declaration with no present caller stays and a weak one is rebuilt, and one parameterized declaration states what two would. A rules citation names code at the base tree; where an earlier run moved that code, the rule reads the form where it now stands. Every consumer (bridge, tests, dylint examples, ast-grep rule scopes, catalog rows) is rebuilt to the new form in this run.`
const SOURCES = `Read crate behavior from each crate's source at the newest release \`cargo info <crate>\` names (~/.cargo/registry/src/index.crates.io-*/<crate>-<version>/, present once \`cargo fetch\` has run on the moved row) through the search-code skill, cited by path and line; the apps' kind of work from apps/, libs/dotnet, libs/python and docs/research/applications; the nightly's forms from ${REPO}/rust-toolchain.toml and ${REPO}/docs/research/coding/rust/ (language.md, language-features.md, crates.md, crate-members.md, mcp.md, toolchain.md, architecture.md, ast-grep/), where RUST.md and a lint row win over a sheet; workspace rules from Cargo.toml, clippy.toml, .cargo/config.toml, rustfmt.toml and the manage-repo skill's Rust reference; the checks from tools/dylint and tools/ast-grep/rules/rust, which decide what code may not say.`
const CHECKS = `code moved as it stands, other than the relocation the [03] row names; a consumer left on the old form; a form a crate member, a nightly gate or a workspace lint states; a struct, enum, bool, tuple or macro standing in for a shape a crate type, derive or generic fn states; a crate used beside hand code it provides; logic a catalog member states written by hand; a \`pub\` item no app operation composes, a wrapper, forwarder or re-export, or fields or functions repeated per kind; a fixed value in a composition where a default one value overrides belongs; app or vendor content in a library crate; a RUST.md or CLAUDE.md breach; behavior the current code or a consumer has that the new code drops; capability the [03] row names left unbuilt`
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const author = await agent(`Read the target's [03] row, the sources of every catalog row it names, the apps' kind of work it serves, and, where code exists, the target whole with every consumer of its public items and the workspace rows that name it. ${TARGET}
${BASIS}
Write ${PLAN}: the target's shape after this work (crate directory under libs/rust/<category>/<crate>/, the crate root and one module per concept, each with what it holds), each concept's one location and form, the crate member or nightly gate it depends on, a consumer call, every deletion; every \`pub\` item with the app operation that composes it; the manifest and the root Cargo.toml rows ([MEMBERS] row, catalog rows at their newest release with the features the role needs, rows its family's members in [01] or ${REPO}/docs/research/coding/rust/crate-members.md need); the tests under tests/rust/libs/<category>/<crate>/ with their [[test]] rows; one check package for each recurring mistake the plan finds that no check reports, in the one tool that decides it (a tools/dylint lint with its example and UI snapshot when types or resolution decide, a tools/ast-grep/rules/rust rule when syntax decides, a [workspace.lints] row when clippy or rustc owns the fact, none when no tool decides); then packages, each with its files (consumers included), items, callers rebuilt, deletions and the earlier packages that supply items it reads. Close with the \`pub\` items per crate before and after, and every behavior of the current code with the item that keeps it. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarially refine ${PLAN} (author summary below) against the code, ${RULES} and the crate sources. Find in every package: ${CHECKS}. ${TARGET} ${SOURCES}
Rewrite the plan in place so each package section is final. Cargo.toml, tools/dylint, tests/rust/tools/dylint, tools/ast-grep/rules/rust, tools/ast-grep/utils/rust sit in packages that list no source file, one holding the manifest rows and one holding every check row, so source packages never queue on a shared file; no package lists a lock file, the Check phase resolves it. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add an item it reads).

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
const adversary = await agent(`Adversarial agent. Packages landed:
${summaries}
Read these files against ${PLAN}: ${touched.join(', ')}. ${TARGET} ${BASIS} Find and fix in place: ${CHECKS}. Then compare each rebuilt file with its base version (\`git show HEAD:<path>\`, the tree before this run): restore only what the base did objectively better, and name each skill rule the base outperformed, so the skill gets fixed. Hunt across crate seams: one concept in two crates, a conversion written twice, an item deleted on its consumer count alone, a lint row lowered or an \`allow\` or \`expect\` attribute, a catalog row off its newest release or out of its section, a test proving nothing. ${SOURCES}
Return (structured): defects fixed (at most 25 lines); files you edited.`, { label: 'adversary', phase: 'Check', schema: LANDED })
const files = [...new Set([...touched, ...(adversary ? adversary.files : [])])]
const built = await agent(`Check-and-fix. Packages landed, then the adversary's fixes:
${summaries}
- adversary: ${adversary ? adversary.summary.slice(0, 1500) : 'agent died'}
Run in ${REPO}: \`cargo fetch\` when a manifest changed, then \`nx run <project>:check\` for the target crate and each workspace crate \`cargo tree --invert --workspace --package <target>\` lists, \`nx run lints:check\` when this run edited tools/dylint, \`nx run rasm:lint:rustfmt -- --package <crate>\` for each of those crates, \`nx run rasm:lint:tombi -- <the TOML files among these>\` and \`nx run rasm:lint:ast-grep -- <files>\` over ${files.join(', ')}, and \`nx run rasm-mcp-bridge:test:integration\` when that list holds the bridge. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a lowered lint, an \`allow\` or an \`expect\`); list each finding in other files with file and message. Against the plan's behavior list and the [03] row, name each behavior the landed code drops and each capability left unbuilt. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, each dropped behavior or unbuilt capability, the \`pub\` items per crate before and after, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, adversary, built }
