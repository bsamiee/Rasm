export const meta = {
  name: 'rust',
  description: 'Build or rebuild one Rust crate on RUST.md: its code, tests, consumers, checks and workspace rows',
  whenToUse: 'One run per RUST.md [03] row, args.key its [KEY]; a run starts once the rows its [AFTER] cell names have landed and runs beside every other run, future-swift and future-typescript included.',
  phases: [
    { title: 'Plan', detail: 'the crate\'s shape from RUST.md, its catalog crates and its consumers, challenged into a package graph' },
    { title: 'Land', detail: 'package agents by disjoint files, each as the packages it reads land' },
    { title: 'Check', detail: 'adversarial read, then check-and-fix' },
  ],
}

const W = '/Users/bardiasamiee/.claude/projects/-Users-bardiasamiee-Developer-Rasm/host-ui-rework'
const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${W}/future-langs/RUST.md`
const CHECK_TREES = ['tools/dylint', 'tests/rust/tools/dylint', 'tools/ast-grep/rules/rust', 'tools/ast-grep/utils/rust']
const BRIDGE = ['tools/bridge', 'tests/rust/tools/bridge']
const crate = (key, ...consumers) => [`libs/rust/${key}`, `tests/rust/libs/${key}`, ...consumers, 'Cargo.toml', 'Cargo.lock']
const TARGETS = {
  'services/telemetry': { title: 'rasm-telemetry, with the relocation of every crate into its category', paths: ['libs/rust', 'tests/rust/libs', 'Cargo.toml', 'Cargo.lock'] },
  'services/daemon': { title: 'rasm-daemon, the served process carved out of rasm-mcp::run', paths: ['libs/rust/services', 'tests/rust/libs/services', ...BRIDGE, 'Cargo.toml', 'Cargo.lock'] },
  'services/mcp': { title: 'rasm-mcp with its client and proxy endpoint, the bridge reduced to Args and main', paths: crate('services/mcp', ...BRIDGE) },
  'cli/command': { title: 'rasm-command', paths: crate('cli/command') },
  'cli/output': { title: 'rasm-output', paths: crate('cli/output') },
  'cli/screen': { title: 'rasm-screen', paths: crate('cli/screen') },
  'geometry/mesh': { title: 'rasm-mesh', paths: crate('geometry/mesh') },
  'geometry/proximity': { title: 'rasm-proximity', paths: crate('geometry/proximity') },
  'geometry/planar': { title: 'rasm-planar', paths: crate('geometry/planar') },
  'geometry/nurbs': { title: 'rasm-nurbs', paths: crate('geometry/nurbs') },
  'geometry/implicit': { title: 'rasm-implicit', paths: crate('geometry/implicit') },
  'interchange/cad': { title: 'rasm-cad', paths: crate('interchange/cad') },
  'interchange/ifc': { title: 'rasm-ifc', paths: crate('interchange/ifc') },
  'interchange/scene': { title: 'rasm-scene', paths: crate('interchange/scene') },
  'interchange/facets': { title: 'rasm-facets', paths: crate('interchange/facets') },
  'interchange/pointcloud': { title: 'rasm-pointcloud', paths: crate('interchange/pointcloud') },
  'geospatial/site': { title: 'rasm-site', paths: crate('geospatial/site') },
  'geospatial/solar': { title: 'rasm-solar', paths: crate('geospatial/solar') },
  'geospatial/terrain': { title: 'rasm-terrain', paths: crate('geospatial/terrain') },
  'geospatial/features': { title: 'rasm-features', paths: crate('geospatial/features') },
  'geospatial/weather': { title: 'rasm-weather', paths: crate('geospatial/weather') },
  'numerics/fitting': { title: 'rasm-fitting', paths: crate('numerics/fitting') },
  'numerics/spectra': { title: 'rasm-spectra', paths: crate('numerics/spectra') },
  'numerics/stats': { title: 'rasm-stats', paths: crate('numerics/stats') },
  'numerics/optimize': { title: 'rasm-optimize', paths: crate('numerics/optimize') },
  'numerics/dynamics': { title: 'rasm-dynamics', paths: crate('numerics/dynamics') },
  'graphs/dependency': { title: 'rasm-dependency', paths: crate('graphs/dependency') },
  'graphs/network': { title: 'rasm-network', paths: crate('graphs/network') },
  'graphs/layout': { title: 'rasm-layout', paths: crate('graphs/layout') },
  'data/tables': { title: 'rasm-tables', paths: crate('data/tables') },
  'data/query': { title: 'rasm-query', paths: crate('data/query') },
  'data/lake': { title: 'rasm-lake', paths: crate('data/lake') },
  'data/fields': { title: 'rasm-fields', paths: crate('data/fields') },
  'data/summaries': { title: 'rasm-summaries', paths: crate('data/summaries') },
  'imaging/color': { title: 'rasm-color', paths: crate('imaging/color') },
  'imaging/raster': { title: 'rasm-raster', paths: crate('imaging/raster') },
  'imaging/grade': { title: 'rasm-grade', paths: crate('imaging/grade') },
  'imaging/codecs': { title: 'rasm-codecs', paths: crate('imaging/codecs') },
  'imaging/filters': { title: 'rasm-filters', paths: crate('imaging/filters') },
  'drawing/linework': { title: 'rasm-linework', paths: crate('drawing/linework') },
  'drawing/symbols': { title: 'rasm-symbols', paths: crate('drawing/symbols') },
  'drawing/type': { title: 'rasm-type', paths: crate('drawing/type') },
  'documents/pdf': { title: 'rasm-pdf', paths: crate('documents/pdf') },
  'documents/typeset': { title: 'rasm-typeset', paths: crate('documents/typeset') },
  'documents/workbooks': { title: 'rasm-workbooks', paths: crate('documents/workbooks') },
  'documents/charts': { title: 'rasm-charts', paths: crate('documents/charts') },
}
const { title, paths: owned } = TARGETS[args.key]
const paths = [...owned, ...CHECK_TREES]
const DIR = `${W}/future-langs/rust/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: ${title}, row \`${args.key}\` of ${RULES} [03]; ${paths.join(', ')} in ${REPO}.`
const BASIS = `Basis: ${RULES} over all the target's code, with ${REPO}/CLAUDE.md. The [03] row and the catalog rows it names are the specification: the crate owns what the row states, built on those rows used fully, and holds nothing those rows and the apps' kind of work do not state. Code that exists is remade, never moved: current code and its callers have no authority, its idioms are the quality floor and not the architecture, every consumer (bridge, tests, dylint examples, ast-grep rule scopes, catalog rows) is rebuilt to the new form in this run, and counted lines, files and public items fall over existing code and stay at the number the plan names for new code. Logic derives from a catalog member or from the structure a caller supplies, and a hand form beside a member that states it is a defect. The public surface is the few operations an app composes over catalog types: no wrapper, re-export, forwarder, per-option function family, or field or function repeated per kind, and every \`pub\` item names the app operation that composes it.`
const SOURCES = `Read crate behavior from each crate's source at the newest release \`cargo info <crate>\` names (~/.cargo/registry/src/index.crates.io-*/<crate>-<version>/, present once \`cargo fetch\` has run on the moved row) through the search-code skill, cited by path and line; the apps' kind of work from apps/, libs/dotnet, libs/python and docs/research/applications; the nightly's forms from ${REPO}/rust-toolchain.toml and ${REPO}/docs/research/coding/rust/ (language.md, language-features.md, crates.md, mcp.md, toolchain.md, architecture.md, ast-grep/), where RUST.md and a lint row win over a sheet; workspace rules from Cargo.toml, clippy.toml, .cargo/config.toml, rustfmt.toml and the manage-repo skill's Rust reference; the checks from tools/dylint and tools/ast-grep/rules/rust, which decide what code may not say.`
const CHECKS = `code moved as it stands, other than the relocation the target title names; a consumer left on the old form; a form a crate member, a nightly gate or a workspace lint states; a struct, enum, bool, tuple or macro standing in for a shape a crate type, derive or generic fn states; a crate used beside hand code it provides; logic a catalog member states written by hand; a \`pub\` item no app operation composes, a wrapper, forwarder or re-export, or fields or functions repeated per kind; a fixed value in a composition where a default one value overrides belongs; app or vendor content in a library crate; a RUST.md or CLAUDE.md breach; counted lines, files or public items over the plan's numbers`
const PKG = { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, needs: { type: 'array', items: { type: 'string' } } }, required: ['id', 'title', 'section', 'files', 'needs'] }
const GRAPH = { type: 'object', properties: { packages: { type: 'array', items: PKG } }, required: ['packages'] }
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const author = await agent(`Read the target's [03] row, the sources of every catalog row it names, the apps' kind of work it serves, and, where code exists, the target whole with every consumer of its public items and the workspace rows that name it. ${TARGET}
${BASIS}
Write ${PLAN}: the target's shape after this work (crate directory under libs/rust/<category>/<crate>/, the crate root and one module per concept, each with what it holds), each concept's one location and form, the crate member or nightly gate it depends on, a consumer call, every deletion; every \`pub\` item with the app operation that composes it; the manifest and the root Cargo.toml rows ([MEMBERS] row, catalog rows at their newest release with the features the role needs, rows the [01] item for its family adds); the tests under tests/rust/libs/<category>/<crate>/ with their [[test]] rows; one check package for each recurring mistake the plan finds that no check reports, in the one tool that decides it (a tools/dylint lint with its example and UI snapshot when types or resolution decide, a tools/ast-grep/rules/rust rule when syntax decides, a [workspace.lints] row when clippy or rustc owns the fact, none when no tool decides); then packages, each with its files (consumers included), items, callers rebuilt, deletions and the earlier packages that supply items it reads. Close with the counted code lines (loc), files and \`pub\` items per crate before and after. Edit no source file. ${SOURCES}
Return a summary of at most 30 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`Adversarially refine ${PLAN} (author summary below) against the code, ${RULES} and the crate sources. Find in every package: ${CHECKS}. ${TARGET} ${SOURCES}
Rewrite the plan in place so each package section is final. Edit no source file. Return its packages as a dependency graph: section (exact heading text), files (every repo-relative path the package edits, consumers included, folders allowed; two packages that share a path run one after the other), needs (ids of packages that add an item it reads).

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
Read these files against ${PLAN}: ${touched.join(', ')}. ${TARGET} ${BASIS} Find and fix in place: ${CHECKS}. Hunt across crate seams: one concept in two crates, a conversion written twice, an item deleted on its consumer count alone, a lint row lowered or an \`allow\` or \`expect\` attribute, a catalog row off its newest release or out of its section, a test proving nothing. ${SOURCES}
Return (structured): defects fixed (at most 25 lines); files you edited.`, { label: 'adversary', phase: 'Check', schema: LANDED })
const files = [...new Set([...touched, ...(adversary ? adversary.files : [])])]
const built = await agent(`Check-and-fix. Packages landed, then the adversary's fixes:
${summaries}
- adversary: ${adversary ? adversary.summary.slice(0, 1500) : 'agent died'}
Run in ${REPO}: \`nx run <project>:check\` for the target crate and each workspace crate \`cargo tree --invert --workspace --package <target>\` lists, \`nx run rasm:lint:rustfmt\`, \`nx run rasm:lint:tombi\`, \`nx run rasm:lint:ast-grep\`, \`ast-grep scan\` over ${files.join(', ')}, and \`nx run rasm-mcp-bridge:test:integration\` when that list holds the bridge. Fix every finding at its root against ${PLAN} in the files this run edited (code or the rule, never a lowered lint, an \`allow\` or an \`expect\`); list each finding in other files with file and message. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, the counted code lines, files and \`pub\` items per crate against the plan's numbers, and \`git diff --shortstat\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { results, adversary, built }
