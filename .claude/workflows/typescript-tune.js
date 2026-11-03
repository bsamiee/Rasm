export const meta = {
  name: 'typescript-tune',
  description: 'Mine committed TypeScript work into the TypeScript checks and skill snippets',
  whenToUse: 'args.base the commit before the TypeScript work it mines.',
  phases: [
    { title: 'Mine', detail: 'readers per package, then a checks writer and a skill writer in parallel' },
    { title: 'Close', detail: 'checks once over the changed rules, fixes once, checks once; a fresh clean-prose agent per changed skill' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const RUN = `\`git diff ${args.base}..HEAD\``
const SKILLS = ['typescript-coding', 'typescript-coding-effect', 'uxp-coding']
const BASIS = `Findings already recorded (TYPESCRIPT.md, .artifacts/workflows/, docs/research) are facts to act on, never to rediscover. Read package members from node_modules/.pnpm/ through Skill(search-code). Run no build, test or check outside the Close phase.`

phase('Mine')
const roots = await agent(`List every package directory under ${REPO}/libs/typescript/*/* and app with TypeScript that ${RUN} touches. Return their repo-relative paths.`, { label: 'mine:roots', phase: 'Mine', schema: { type: 'object', properties: { paths: { type: 'array', items: { type: 'string' } } }, required: ['paths'] } })
const readings = await parallel((roots?.paths ?? []).map((root) => () => agent(`${BASIS}
Read ${root} and its part of ${RUN} against ${REPO}/.claude/workflows/TYPESCRIPT.md and the TypeScript skills. Edit nothing. Report in at most 30 lines with file and line: each hand-rolled or bloated form with the stronger form an Effect or package member, Schema combinator or algebraic form states; each family of schemas, types or classes one shape states; each check in tools/ast-grep/rules/typescript or biome.json that misfires or that a naive form escapes; each strong form in the code the skills lack.`, { label: `mine:${root}`, phase: 'Mine' }).then((r) => r && `## ${root}\n${r}`)))
const mined = readings.filter(Boolean).join('\n\n')
const tuned = await parallel([
  () => agent(`${BASIS} Load Skill(use-ast-grep) and Skill(manage-repo) first.
Checks writer. From these readings, change a TypeScript check only where its tool decides the fact reliably: an ast-grep rule when syntax decides, a biome.json row when Biome owns it, a tsconfig option when the compiler owns it. Fix or remove each rule that misfires, tighten each rule a naive form escapes, and add a rule only for a recurring pattern. Each message names the stronger form. Edit no skill file.
${mined}
Return at most 10 lines: each rule changed and the pattern it decides.`, { label: 'mine:checks', phase: 'Mine' }),
  () => agent(`${BASIS} Load ${SKILLS.map((s) => `Skill(${s})`).join(', ')} whole first.
Skill writer. From these readings, improve the skills' code: replace a snippet whole where real code shows a stronger form, add one only for a principle a skill lacks, and delete what the readings show redundant, each in the skill and file that owns its topic. Each snippet is dense and advanced, integrates its packages implicitly, couples to no library of this repository, uses vars and tokens rather than literals, and adds no bloat. Fix a prose line only where a reading shows it wrong.
${mined}
Return at most 10 lines: each snippet and line changed.`, { label: 'mine:skills', phase: 'Mine' }),
])

phase('Close')
const [checked, ...prose] = await parallel([
  () => agent(`Load Skill(use-ast-grep) first. Checks close over the rules this launch changed:
${tuned[0] ?? ''}
Run \`nx run rasm:lint:biome\` and \`nx run rasm:lint:ast-grep -- libs/typescript apps plugins\` once, fix every finding at its cause once (never a suppression or a lowered severity), then run them once more. Return at most 8 lines: findings fixed and any that remain.`, { label: 'close:checks', phase: 'Close' }),
  ...SKILLS.map((skill) => () => agent(`Load Skill(clean-prose) and read its SKILL.md and every file under references/ whole, then read .claude/skills/${skill} whole. Clean only what this launch changed (\`git diff -- .claude/skills/${skill}\`): remove framing and restated context, rebuild each oversized or sheared line whole, keep each fact once, and rename coined terms in prose and in code snippets. Run \`python ${REPO}/.claude/skills/clean-prose/scripts/prose.py check .claude/skills/${skill}\` once and fix what it reports on changed lines. Return at most 5 lines.`, { label: `close:prose:${skill}`, phase: 'Close' })),
])
return { tuned, checked, prose }
