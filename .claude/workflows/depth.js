export const meta = {
  name: 'depth',
  description: 'Deepen one landed library folder in Rust, TypeScript or Python: lens readers, one adversarial lander, a diff review, then fix and check',
  whenToUse: 'One run per committed library folder: args.language rust, typescript or python, args.key its path under libs/<language>/ (a RUST.md or TYPESCRIPT.md [03] [KEY] where one exists), args.base the commit it starts from. At most five at once beside the row runs, on folders sharing no logic with a running run or a row building on them, started at least 10 minutes apart. Its commit goes with any overlapping run, then that language\'s tune.',
  phases: [
    { title: 'Read', detail: 'parallel readers, one lens each, over the folder, its consumers and its dependencies' },
    { title: 'Land', detail: 'one adversarial lander judges every finding and lands the accepted ones' },
    { title: 'Review', detail: 'parallel adversaries over the diff against the base' },
    { title: 'Check', detail: 'the review findings fixed, then every check' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const LANGUAGES = {
  rust: {
    skills: 'Skill(rust-coding)',
    rules: `${REPO}/.claude/workflows/RUST.md`,
    manifests: ['Cargo.toml', 'Cargo.lock'],
    consumers: '`cargo tree --invert --workspace --package <crate>`',
    sources: `each dependency's source at the newest release \`cargo info <crate>\` names, under ~/.cargo/registry/src/index.crates.io-*/ after \`cargo fetch\`, through Skill(search-code); the nightly's forms from rust-toolchain.toml and docs/research/coding/rust/; forbidden forms from tools/dylint and tools/ast-grep/rules/rust`,
    checks: (files) => `\`cargo fetch\` when a manifest changed, \`nx run <project>:check\` for the crate and each consumer, \`nx run lints:check\` when tools/dylint changed, \`nx run rasm:lint:rustfmt -- --package <crate>\` for each, \`nx run rasm:lint:tombi\` over edited TOML, \`nx run rasm:lint:ast-grep -- ${files}\`, \`nx run rasm-mcp-bridge:test:integration\` when the bridge changed`,
  },
  typescript: {
    skills: 'Skill(typescript-coding) and Skill(typescript-coding-effect)',
    rules: `${REPO}/.claude/workflows/TYPESCRIPT.md`,
    manifests: ['package.json', 'pnpm-workspace.yaml', 'pnpm-lock.yaml'],
    consumers: '`pnpm ls --filter \'...<package name>\' --depth -1` and the apps importing it',
    sources: `each package's source under node_modules/.pnpm/<name>@<version>/node_modules/<name>/ (effect ships src, .d.ts elsewhere) through Skill(search-code); host types from the uxp-coding skill; forbidden forms from tools/ast-grep/rules/typescript and biome.json`,
    checks: (files) => `\`pnpm install --prefer-offline\` when a manifest changed, \`nx run <project>:check\` for the package and each consumer, \`nx run rasm:typecheck:tsc\`, \`nx run rasm:lint:biome -- ${files}\`, \`nx run rasm:lint:ast-grep -- ${files}\`, \`nx run <project>:build\` where a build target exists`,
  },
  python: {
    skills: 'Skill(python-coding) and each python-coding-* skill its imports name',
    rules: `${REPO}/pyproject.toml`,
    manifests: ['pyproject.toml', 'uv.lock'],
    consumers: '`rg -l` over libs, apps and tests for imports of the package',
    sources: `each package's source under .venv/lib/python*/site-packages/ through Skill(search-code); forbidden forms from the ruff, refurb and ast-grep rules`,
    checks: (files) => `\`nx run <project>:check\` for the package and each consumer, \`nx run rasm:lint:ruff -- ${files}\`, \`nx run rasm:lint:ast-grep -- ${files}\`, \`nx run rasm:lint:deptry\``,
  },
}
const lang = LANGUAGES[args.language]
const ROOT = `libs/${args.language}/${args.key}`
const DIR = `${REPO}/.artifacts/workflows/depth/${args.language}/${args.key}`
const TARGET = `Target: ${ROOT} in ${REPO}, its consumers (${lang.consumers}) and its rows in ${lang.manifests.join(', ')}; base commit ${args.base}.`
const BASIS = `Load ${lang.skills} and Skill(clean-prose) first; they and ${lang.rules} decide every line, and the folder's [03] row there, where one exists, is its specification. The folder is a library for downstream software no one has written yet: present code and consumers hold no authority, general capability it holds stays, and every consumer moves to the new form. A consumer states what it wants and gets a finished result: the library derives, defaults and composes internally whatever a caller would otherwise write, and the direct form is the only form. It holds the full capability its domain needs under advanced use in the fewest public and internal items: a struct, type, enum, error variant or validation exists only where a caller acts on it differently, and the rest collapses into parameters, data or a value derived from its owner (host, package, declaration). Errors are one type per package, each variant a different caller response with its subject as data. Every dependency works through its full members, and nothing restates one. Names, file names and messages are domain terms in clean prose, messages built from their values. Thread safety and performance hold, with no indirection. A change lands only when its result does more with less than its base; growth stands only for capability the base lacked or held shallowly. An item with no present caller stays, a weak one is rebuilt. Read behavior from ${lang.sources}. Hosts run in the background through Atelier's MCP tools and watch-apps; nothing activates an app or takes the owner's focus.`
const SILENT = 'Report only what changes code, each with file and line; say nothing about what holds.'
const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'object', properties: { file: { type: 'string' }, line: { type: 'integer' }, change: { type: 'string' }, gain: { type: 'string' } }, required: ['file', 'change', 'gain'] } }, steering: { type: 'array', items: { type: 'string' } } }, required: ['findings', 'steering'] }
const listed = (results) => results.filter(Boolean).flatMap((r) => r.findings.map((f) => `- [${r.lens}] ${f.file}:${f.line ?? ''} ${f.change} (${f.gain})`)).join('\n')
const steering = (results) => results.filter(Boolean).flatMap((r) => r.steering)


phase('Read')
const readings = await parallel([
  { lens: 'capability', ask: 'Work out the domain and the operations downstream software performs in it under real, advanced use (degenerate, large, malformed and partial input, precision and tolerance, concurrency and cancellation, every variant the domain holds), and name each capability the folder lacks or holds shallowly, as stronger logic inside an operation or an operation the domain needs, never convenience surface.' },
  { lens: 'dependencies', ask: 'Read each dependency at its newest release for every member and feature that states a capability the folder needs or replaces logic it writes by hand, and each package the folder should add, replace or drop.' },
  { lens: 'reduction', ask: 'Name each struct, type, enum, error variant and validation no caller acts on differently, each step a consumer writes that the library could derive or compose itself, and each piece of hand logic, restated value, stringly type, constant, wrapper, alias and helper, with the collapsed, derived or parameterized form that replaces it.' },
  { lens: 'correctness', ask: 'Name each defect: wrong results at edges, malformed or partial input, precision, error paths that lose their cause or repeat it as separate variants, thread safety, blocking or starved work, needless allocation, copies and slow paths.' },
  { lens: 'shape', ask: 'Name each file that holds more than one concept or a concept split across files, each folder that sprawls or holds one file, nested or branching control flow that flattens, each coined or vague name, file name or message, and each consumer that adapts, wraps or reshapes the folder instead of calling it directly.' },
].map((r) => () => agent(`Reader, ${r.lens} lens. Read ${ROOT} whole, its consumers and its dependencies. ${r.ask} Treat the folder as a shallow draft: a file earns no finding only where you can name the advanced use it handles and the dependency members you read. ${SILENT} Edit nothing. ${TARGET} ${BASIS}
Return (structured): findings, and each ${lang.rules} or skill line that steered a defect, quoted (empty when none).`, { label: `read:${r.lens}`, phase: 'Read', schema: FOUND }).then((found) => found && { lens: r.lens, ...found })))

phase('Land')
const landed = await agent(`Lander. The readers' findings:
${listed(readings)}
Judge each as an adversary: reject a weak, cosmetic or regressive finding, one that grows surface without new capability or adds a stringly type, constant or wrapper, and one whose gain the code does not show, each with one line why. Land the rest without relitigating them, merging overlaps, in their strongest form across ${ROOT} and its consumers, rebuilding a file whole where its findings reshape it. ${TARGET} ${BASIS}
Return at most 25 lines: what landed with the capability or form each adds, what you rejected and why, and the files you edited.`, { label: 'land', phase: 'Land' })

phase('Review')
const reviews = await parallel([
  { lens: 'additions', ask: 'For each item the diff adds: it serves an operation a consumer could not perform before, holds under advanced use, carries no stringly type, needless constant, type or record, and stands in its strongest form; otherwise name its fix or its removal.' },
  { lens: 'regressions', ask: 'For each item the diff removes or changes: name each capability or behavior the base held that is now lost, each lowered lint or suppression, each dropped Send, Sync or cancellation, each slower path, and each consumer left on the old form.' },
  { lens: 'reduction', ask: 'Over the folder as it now stands, name what still collapses: surface, parameterization, derivation, file shape and control flow.' },
].map((r) => () => agent(`Reviewer, ${r.lens} lens. Read \`git diff ${args.base} -- ${ROOT}\` and the consumers it touched, then the files whole. ${r.ask} The lander reported:
${landed}
${SILENT} Edit nothing. ${TARGET} ${BASIS}
Return (structured): findings, and each ${lang.rules} or skill line that steered a defect, quoted (empty when none).`, { label: `review:${r.lens}`, phase: 'Review', schema: FOUND }).then((found) => found && { lens: r.lens, ...found })))

phase('Check')
const built = await agent(`Fix and check. The reviewers' findings on the landed diff:
${listed(reviews)}
Judge each as the lander did and fix the ones that hold, at their root. Fix each ${lang.rules} or skill line named as steering a defect, as a general principle in the file's own form: ${[...steering(readings), ...steering(reviews)].join(' | ') || 'none named'}.
Then run in ${REPO}: ${lang.checks('the files this run edited')}. Fix every finding at its root in the files this run edited (code or the rule, never a lowered severity or a suppression), and list each finding in other files with file and message. Measure lines and public items per file at ${args.base} and now, and write ${DIR}/surface.md with both and the capability behind each growth; growth from restating, wrapping or splitting is removed before you return. ${TARGET} ${BASIS}
Return at most 20 lines: each command's result, each fix, each remaining finding, public items before and after, and \`git diff --shortstat ${args.base}\` over the target and its consumers.`, { label: 'check', phase: 'Check' })
return { readings: readings.filter(Boolean).map((r) => `${r.lens}: ${r.findings.length}`), landed, reviews: reviews.filter(Boolean).map((r) => `${r.lens}: ${r.findings.length}`), built }
