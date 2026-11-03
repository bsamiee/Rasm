export const meta = {
  name: 'rust-tune',
  description: 'Mine committed Rust runs into the Rust checks and skill',
  whenToUse: 'args.base the commit before the Rust runs it mines; no Rust run lands beside it.',
  phases: [
    { title: 'Mine', detail: 'readers per crate, then a checks writer and a skill writer in parallel' },
    { title: 'Close', detail: 'checks once over the changed rules, fixes once, checks once; prose over the changed skill lines' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RULES = `${REPO}/.claude/workflows/RUST.md`
const RUN = `\`git diff ${args.base}..HEAD\``
const BASIS = `Findings already recorded (RUST.md, .artifacts/workflows/, docs/research/coding/rust) are facts to act on, never to rediscover. Read crate members from ~/.cargo/registry/src/index.crates.io-*/ through Skill(search-code). Run no build, test or check outside the Close phase.`

phase('Mine')
const roots = await agent(`List every crate directory under ${REPO}/libs/rust/*/* that ${RUN} touches, plus tools/dylint. Return their repo-relative paths.`, { label: 'mine:roots', phase: 'Mine', schema: { type: 'object', properties: { paths: { type: 'array', items: { type: 'string' } } }, required: ['paths'] } })
const readings = await parallel((roots?.paths ?? []).map((root) => () => agent(`${BASIS}
Read ${root} and its part of ${RUN} against ${RULES} and Skill(rust-coding). Edit nothing. Report in at most 30 lines with file and line: each hand-rolled or bloated pattern with the stronger form a crate member, derive or algebraic form states; each family of structs, enums or types one shape states; each check in tools/dylint, tools/ast-grep/rules/rust, [workspace.lints] or clippy.toml that misfires on good code or that a naive form escapes; each strong form in the code the skill lacks.`, { label: `mine:${root}`, phase: 'Mine' }).then((r) => r && `## ${root}\n${r}`)))
const mined = readings.filter(Boolean).join('\n\n')
const tuned = await parallel([
  () => agent(`${BASIS} Load Skill(use-ast-grep) and Skill(manage-repo) first.
Checks writer. From these readings, change a Rust check only where its tool decides the fact reliably: an ast-grep rule when syntax decides, a dylint lint with its example and snapshot when types decide, a lint row when clippy owns it. Fix or remove each rule that misfires, tighten each rule a naive form escapes, and add a rule only for a recurring pattern. Each message names the stronger form. Edit no skill file.
${mined}
Return at most 10 lines: each rule changed and the pattern it decides.`, { label: 'mine:checks', phase: 'Mine' }),
  () => agent(`${BASIS} Load Skill(rust-coding) whole first.
Skill writer. From these readings, replace a snippet whole where real code shows a stronger form, add one only for a principle the skill lacks, delete what the readings show redundant, and fix a prose line only where a reading shows it wrong. Snippets use generic names. Edit no file outside .claude/skills/rust-coding.
${mined}
Return at most 10 lines: each snippet and line changed.`, { label: 'mine:skill', phase: 'Mine' }),
])

phase('Close')
const [checked, prose] = await parallel([
  () => agent(`Load Skill(use-ast-grep) first. Checks close over the rules this launch changed:
${tuned[0] ?? ''}
Run \`nx run lints:check\` and \`nx run rasm:lint:ast-grep -- libs/rust tools/bridge tools/dylint\` once, fix every finding at its cause once (never an allow, expect or lowered lint), then run them once more. Return at most 8 lines: findings fixed and any that remain.`, { label: 'close:checks', phase: 'Close' }),
  () => agent(`Load Skill(clean-prose) first. Prose close over the lines this launch changed in .claude/skills/rust-coding (\`git diff -- .claude/skills/rust-coding\`): remove framing and restated context, rebuild each oversized or sheared line whole, keep each fact once. Run \`python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check .claude/skills/rust-coding\` once and fix what it reports on changed lines. Return at most 6 lines.`, { label: 'close:prose', phase: 'Close' }),
])
return { tuned, checked, prose }
