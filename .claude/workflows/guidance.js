export const meta = {
  name: 'guidance',
  description: 'Map the tree and its history, then write or rebuild the folder CLAUDE.md files that earn their place, each challenged against a measured standard',
  whenToUse: 'Once the tree has settled after a round of rebuilds; args.areas the top-level folders to map. It edits only CLAUDE.md files outside the root and their AGENTS.md links.',
  phases: [
    { title: 'Basis', detail: 'the standard a folder guidance file meets, and the guidance that already exists' },
    { title: 'Map', detail: 'one reader per area over its code and its history' },
    { title: 'Select', detail: 'one judge picks the levels that earn a file' },
    { title: 'Write', detail: 'one writer per chosen file' },
    { title: 'Challenge', detail: 'one adversary per file, then one pass across all of them' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/.artifacts/workflows/guidance`
const STANDARD = `${DIR}/STANDARD.md`
const SELECTION = `${DIR}/SELECTION.md`
const BASIS = `Load Skill(clean-prose) first. A folder CLAUDE.md loads when an agent works under that folder, so each line costs every task there and must change how an agent builds in that folder. ${STANDARD} states what a good one holds and how it is measured. The root CLAUDE.md, README.md and the skills under .claude/skills already state repository-wide rules and language practice; a folder file never repeats or restates them. Edit nothing outside the files your step names.`
const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } } }, required: ['findings'] }

phase('Basis')
const [docs, existing] = await parallel([
  () => agent(`Write ${STANDARD}: what a folder-level CLAUDE.md should hold for current Claude models, grounded in the Claude Code documentation (the claudeCodeDocs MCP: memory, CLAUDE.md loading and imports, nested and subdirectory files, AGENTS.md) and in Anthropic's guidance on prompting current models. State: how and when nested files load; what content earns a line (the folder's role and its boundaries with neighbours, the shape its code takes and why, the recurring mistakes the history shows and the principle that prevents each, the decisions that look arbitrary but are settled); what never earns one (anything the root file, README or a skill states, self-evident or obvious statements, file listings and counts that go stale, today's names and shapes stated as law, instructions about the file itself); the form (at most 100 to 125 lines, sections per the repository's divider and label style, a mermaid diagram only where the folder's relations need one, every line clean prose with no split or garbled sentence); when a level earns a file at all (a level above covering 80 to 90 percent of what a lower level would say means one file above, with lower files only for the rest); and a scoring rubric a reviewer applies line by line. Return at most 10 lines: the rubric's criteria.`, { label: 'basis:standard', phase: 'Basis' }),
  () => agent(`Read the root CLAUDE.md, README.md, every skill under .claude/skills (SKILL.md headings and what each covers) and every CLAUDE.md and AGENTS.md outside the root (\`fd -H '^(CLAUDE|AGENTS).md$'\`, links included, noting any AGENTS.md that points nowhere). Write ${DIR}/EXISTING.md: per existing folder file, each line that repeats something the root, README or a skill states, each stale line against the code, and what in it is genuinely folder-specific. Return at most 8 lines.`, { label: 'basis:existing', phase: 'Basis' }),
])

phase('Map')
const maps = await parallel(args.areas.map((area) => () => agent(`Reader for ${area}. Read ${STANDARD} and ${DIR}/EXISTING.md, then the code under ${area} and its history in depth: \`git log --stat -- ${area}\` across its whole life, and \`git show\` of every commit that rebuilt, reverted, renamed, collapsed or fixed something there. Infer, per folder level from ${area} down, the guidance that would have prevented the repeated fixes and regressions (mini files, declaration files, mixed levels of abstraction, coupling a library to today's consumer, restated package facts, wrappers, anchoring to the present shape), as higher-order principles rather than today's names. For each candidate level, state what a file there would hold under the standard, what share of it a level above would already cover, and whether it earns a file. Report only levels with guidance a reader could not get from the root, README or skills. Edit nothing. ${BASIS}
Return (structured): findings, each naming its folder, what it would hold, and its overlap with the levels above.`, { label: `map:${area}`, phase: 'Map', schema: FOUND }).then((m) => m && `## ${area}\n${m.findings.map((f) => `- ${f}`).join('\n')}`)))

phase('Select')
const chosen = await agent(`Judge. From the maps below, write ${SELECTION}: the folders that get a CLAUDE.md (existing ones rebuilt, new ones created), each with the content outline it holds and the history behind each point, and the existing files to delete because a level above covers them or nothing in them earns its place. Choose the highest level that covers a group; add a lower file only for what the higher one cannot say. Reject candidates that would repeat the root, README or a skill, state the obvious, or freeze today's shape. Maps:
${maps.filter(Boolean).join('\n\n')}
${BASIS}
Return (structured): the folders to write and the files to delete.`, { label: 'select', phase: 'Select', schema: { type: 'object', properties: { write: { type: 'array', items: { type: 'string' } }, remove: { type: 'array', items: { type: 'string' } } }, required: ['write', 'remove'] } })

phase('Write')
const written = await parallel((chosen?.write ?? []).map((folder) => () => agent(`Writer for ${REPO}/${folder}/CLAUDE.md. Follow ${SELECTION} for this folder and ${STANDARD}. Read the folder's code and its history again yourself before writing; verify every claim against the current code. Write the file whole (rebuild it if one exists), at most 125 lines, and make ${folder}/AGENTS.md a relative symlink to it. ${BASIS}
Return at most 6 lines: line count, sections, and what you left out and why.`, { label: `write:${folder}`, phase: 'Write' })))

phase('Challenge')
const challenged = await parallel((chosen?.write ?? []).map((folder) => () => agent(`Adversary for ${REPO}/${folder}/CLAUDE.md. Score every line against the rubric in ${STANDARD}. Cut each line that repeats the root, README, a skill or a level above, states the obvious, freezes today's names or counts, or that the code contradicts; sharpen each principle to its higher-order form; fix any split, garbled or self-referencing sentence. Rewrite in place and keep it within 125 lines. ${BASIS}
Return at most 6 lines: lines before and after, and what you cut or sharpened.`, { label: `challenge:${folder}`, phase: 'Challenge' })))
const closed = await agent(`Cross-file pass. Delete the files ${SELECTION} names for removal (${(chosen?.remove ?? []).join(', ') || 'none'}) and their AGENTS.md links, and remove any AGENTS.md link that points nowhere. Read every folder CLAUDE.md together with the root CLAUDE.md: remove any point two files state, keeping it at the higher level, and any line a skill or the README already states. Confirm every AGENTS.md is a relative symlink to its CLAUDE.md, and run the clean-prose check over every file. ${BASIS}
Return at most 8 lines: the final file set with line counts, and what moved or went.`, { label: 'close', phase: 'Challenge' })
return { docs, existing, chosen, written, challenged, closed }
