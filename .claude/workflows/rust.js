export const meta = {
  name: 'rust',
  description: 'Build or rebuild one Rust crate on RUST.md: survey, plan, package landers, adversaries fixing in place, one check',
  whenToUse: 'One run per RUST.md [03] row, args.key its [KEY], once the rows its [AFTER] cell names have landed.',
  phases: [
    { title: 'Plan', detail: 'survey in parallel, a plan, an adversarial architect rewriting it into a package graph' },
    { title: 'Land', detail: 'package landers over disjoint files, each once the packages it reads land' },
    { title: 'Deepen', detail: 'one adversary per file group, fixing in place' },
    { title: 'Check', detail: 'checks once over the crate and its consumers, fixes once, checks once' },
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
const DIR = `${REPO}/.artifacts/workflows/rust/${args.key}`
const PLAN = `${DIR}/PLAN.md`
const TARGET = `Target: row \`${args.key}\` of ${RULES} [03]; ${owned.join(', ')} in ${REPO}.`
const BASIS = `Load Skill(rust-coding) first; it and ${RULES} decide every line. The [03] row, its [01] family and the catalog rows it names are the specification: the crate builds on those crates through their own members and hand-writes nothing they provide. Findings already recorded (RUST.md, ${DIR}, .artifacts/workflows/findings, docs/research/coding/rust) are facts to act on, never to rediscover. Current code holds no authority: it is rebuilt, never moved, and every consumer moves to the new form. The crate is for software not yet written and holds its domain's full capability under advanced use in the fewest items. A struct, type, enum, error variant, constant or validation exists only where a caller acts on it differently; every other one is deleted and folds into a parameter, a derived value or a crate type. One error type per crate. The library derives and composes what a consumer would otherwise write. A file holds one concept, no folder holds one file, and no root only collects modules. Names and messages are domain terms, messages built from their values. A value a standard or dataset states comes from that source. A change stands only when the result does more with less than its base. Hosts run in the background through watch-apps. Read crate members from ~/.cargo/registry/src/index.crates.io-*/ through Skill(search-code). Run no build, test or check outside the Check phase.`
const LANDED = { type: 'object', properties: { summary: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } }, required: ['summary', 'files'] }

phase('Plan')
const BRIEF = `${DIR}/brief`
await parallel([
  ['code', 'every item of the current target code with file, line and purpose, every call site of each pub item in its consumers, and the workspace rows that name it'],
  ['members', 'the members of every catalog crate the [03] row and its family name, at the release Cargo.toml pins: purpose, signature and feature of each'],
].map(([key, ask]) => () => agent(`${BASIS} ${TARGET}
Write ${BRIEF}/${key}.md: ${ask}. Facts with citations, no design. Edit nothing else.`, { label: `ground:${key}`, phase: 'Plan' })))
const author = await agent(`${BASIS} ${TARGET}
Read ${BRIEF}/*.md and the target's [03] row. Write ${PLAN}: every file of the crate with the items it holds and the catalog members it calls, every pub item with the operation that composes it, every deletion, the manifest and root Cargo.toml rows, every behavior of the current code with the item that keeps it, then packages over disjoint files with the earlier packages each reads. Edit no source file. Return a summary of at most 20 lines.`, { label: 'plan', phase: 'Plan' })
const graph = await agent(`${BASIS} ${TARGET}
Adversarial architect. Rewrite ${PLAN} in place: every file, item, variant, dependency edge and package stands only where the [03] row and the brief support it, and each one they do not support is deleted from the plan. The manifest package (root Cargo.toml rows, the crate's Cargo.toml and root file) is a need of every source package; no package lists a lock file. The author summarized:
${author}
Return the packages as a dependency graph: section (exact heading), files (every path it edits, consumers included), needs (ids of packages it reads).`, { label: 'challenge', phase: 'Plan', schema: { type: 'object', properties: { packages: { type: 'array', items: { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, section: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, needs: { type: 'array', items: { type: 'string' } } }, required: ['id', 'title', 'section', 'files', 'needs'] } } }, required: ['packages'] } })

phase('Land')
const ids = new Set(graph.packages.map(p => p.id))
const pending = [...graph.packages]
const done = new Map()
const died = new Set()
const overlaps = (a, b) => a.some(f => b.some(g => f.startsWith(g) || g.startsWith(f)))
const pump = () => {
  const active = graph.packages.filter(q => !(pending.includes(q) || done.has(q.id) || died.has(q.id)))
  const ready = pending.reduce((picked, p) => (p.needs.every(n => done.has(n) || !ids.has(n)) && !active.concat(picked).some(q => overlaps(q.files, p.files)) ? [...picked, p] : picked), [])
  return parallel(ready.map(p => () => {
    pending.splice(pending.indexOf(p), 1)
    const read = p.needs.filter(n => done.has(n)).map(n => `- ${n}: ${done.get(n).summary.slice(0, 600)}`).join('\n')
    return agent(`${BASIS}
Land package ${p.id} (${p.title}) as ${PLAN} section "${p.section}" states, editing only ${p.files.join(', ')}.${read ? `\nPackages it reads, landed:\n${read}` : ''}
Return (structured): summary of what landed (at most 15 lines); files you edited.`, { label: `land:${p.id}`, phase: 'Land', schema: LANDED }).then(r => {
      r ? done.set(p.id, r) : died.add(p.id)
      return pump()
    })
  }))
}
await pump()
if (pending.length > 0) log(`${pending.map(p => p.id).join(', ')} never started: a need died`)
const results = graph.packages.map(q => ({ id: q.id, ...(done.get(q.id) ?? { summary: 'not landed', files: [] }) }))

phase('Deepen')
const groups = results.filter(r => r.files.length > 0).reduce((all, r) => {
  const joined = all.filter(g => overlaps(g.files, r.files))
  return [...all.filter(g => !joined.includes(g)), { ids: [r.id, ...joined.flatMap(g => g.ids)], files: [...new Set([...r.files, ...joined.flatMap(g => g.files)])] }]
}, [])
await parallel(groups.map((g, i) => () => agent(`${BASIS} ${TARGET}
Adversary for packages ${g.ids.join(', ')}, editing only ${g.files.join(', ')}. Read each file against ${PLAN} and its base (\`git show HEAD:<path>\`), and fix in place: every item no caller acts on differently, hand logic a crate member states, wrappers, forwarders and re-exports, a consumer left on the old form, behavior the base held that the new code drops, and logic that breaks under advanced use. Replace a weak file or block whole. Return at most 8 lines: what you changed.`, { label: `deepen:${i + 1}`, phase: 'Deepen' })))

phase('Check')
const built = await agent(`${BASIS} ${TARGET}
Check: run \`cargo fetch\` when a manifest changed, then \`nx run <project>:check\` for the crate and each consumer \`cargo tree --invert --workspace --package <crate>\` lists, and \`nx run rasm:lint:ast-grep -- <files>\` over the files this run edited, once. Fix every finding at its cause once (never an allow, expect or lowered lint), then run them once more. Return at most 10 lines: findings fixed and any that remain.`, { label: 'check', phase: 'Check' })
return { results, built }
