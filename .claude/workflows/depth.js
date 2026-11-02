export const meta = {
  name: 'depth',
  description: 'Deepen one landed library folder: lens readers, one lander, adversaries fixing in place, one check',
  whenToUse: 'args.language rust, typescript or python, args.key the folder under libs/<language>/, args.base the commit it starts from.',
  phases: [
    { title: 'Read', detail: 'readers in parallel, one lens each' },
    { title: 'Land', detail: 'one lander lands the findings' },
    { title: 'Deepen', detail: 'adversaries in parallel, each fixing its lens in place' },
    { title: 'Check', detail: 'checks once over the folder, fixes once, checks once' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const LANGUAGES = {
  rust: {
    skills: 'Skill(rust-coding)',
    rules: `${REPO}/.claude/workflows/RUST.md`,
    sources: 'dependency sources under ~/.cargo/registry/src/index.crates.io-*/ through Skill(search-code)',
    checks: '`nx run <project>:check` for the crate and `nx run rasm:lint:ast-grep -- <files>` over the files the run edited',
  },
  typescript: {
    skills: 'Skill(typescript-coding) and Skill(typescript-coding-effect)',
    rules: `${REPO}/.claude/workflows/TYPESCRIPT.md`,
    sources: 'package sources under node_modules/.pnpm/ through Skill(search-code)',
    checks: '`nx run <project>:check` for the package and `nx run rasm:lint:biome -- <files>` and `nx run rasm:lint:ast-grep -- <files>` over the files the run edited',
  },
  python: {
    skills: 'Skill(python-coding) and the python-coding-* skills its imports name',
    rules: `${REPO}/pyproject.toml`,
    sources: 'package sources under .venv/lib/python*/site-packages/ through Skill(search-code)',
    checks: '`nx run <project>:check` for the package and `nx run rasm:lint:ruff -- <files>` over the files the run edited',
  },
}
const lang = LANGUAGES[args.language]
const ROOT = `libs/${args.language}/${args.key}`
const BASIS = `Load ${lang.skills} and Skill(clean-prose) first; they and ${lang.rules} decide every line. ${ROOT} in ${REPO} is a library for software not yet written, base commit ${args.base}; its consumers move to every new form. Findings already recorded (the folder's ${lang.rules} row, .artifacts/workflows/, docs/research) are facts to act on, never to rediscover. The library derives and composes what a caller would otherwise write. A struct, type, enum, error variant, constant or validation exists only where a caller acts on it differently; every other one is deleted and folds into parameters, data or a value its owner states. One error type per package. Dependencies work through their own members (${lang.sources}). Names and messages are domain terms, messages built from their values. A change stands only when the result does more with less. Run no build, test or check outside the Check phase.`
const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } } }, required: ['findings'] }
const LENSES = [
  ['capability', 'capability the domain needs under advanced use that the folder lacks or holds shallowly'],
  ['dependencies', 'dependency members that replace hand logic, and packages to add, replace or drop'],
  ['reduction', 'structs, types, enums, variants, constants, validations, wrappers and helpers to delete, collapse or derive'],
  ['correctness', 'wrong results, lost error causes, thread safety, blocking work, needless allocation and slow paths'],
  ['shape', 'files holding more than one concept or a concept split across files, nested control flow, coined names, and consumers that wrap the folder'],
]

phase('Read')
const readings = await parallel(LENSES.map(([lens, ask]) => () => agent(`${BASIS}
Reader, ${lens} lens. Read ${ROOT} whole and its consumers. Name each change in ${ask}, one line each with file and line. Edit nothing. Return (structured) the findings.`, { label: `read:${lens}`, phase: 'Read', schema: FOUND })))
const found = readings.filter(Boolean).flatMap((r) => r.findings).map((f) => `- ${f}`).join('\n')

phase('Land')
const landed = await agent(`${BASIS}
Lander. Land these findings across ${ROOT} and its consumers in their strongest form, rebuilding a file whole where its findings reshape it:
${found}
Push back only on a finding that grows surface without capability or regresses behavior, one line each. Return at most 15 lines: what landed and what you pushed back on.`, { label: 'land', phase: 'Land' })

phase('Deepen')
await parallel(LENSES.map(([lens, ask]) => () => agent(`${BASIS}
Adversary, ${lens} lens. The lander reported:
${landed}
Read ${ROOT} as it now stands and fix in place every remaining ${ask}. Other adversaries edit beside you; re-read a file before each edit. Return at most 6 lines: what you changed.`, { label: `deepen:${lens}`, phase: 'Deepen' })))

phase('Check')
return agent(`${BASIS}
Check ${ROOT}: run ${lang.checks} once, fix every finding at its cause once (never a suppression or a lowered lint), then run them once more. Return at most 10 lines: findings fixed and any that remain.`, { label: 'check', phase: 'Check' })
