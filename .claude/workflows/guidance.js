export const meta = {
  name: 'guidance',
  description: 'Map the tree and its history, then write or rebuild the folder CLAUDE.md files that earn their place, each challenged in place',
  whenToUse: 'Once the tree settles after a round of rebuilds; args.areas the top-level folders to map. It edits only CLAUDE.md files outside the root and their AGENTS.md links.',
  phases: [
    { title: 'Basis', detail: 'the standard a folder guidance file meets, and the guidance that exists' },
    { title: 'Map', detail: 'one reader per area over its code and history' },
    { title: 'Select', detail: 'one judge picks the levels that earn a file' },
    { title: 'Write', detail: 'per chosen file a writer, then an adversary rewriting it in place' },
    { title: 'Close', detail: 'one pass across every file' },
  ],
}

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/.artifacts/workflows/guidance`
const STANDARD = `${DIR}/STANDARD.md`
const SELECTION = `${DIR}/SELECTION.md`
const BASIS = `Load Skill(clean-prose) first. A folder CLAUDE.md loads for every task under its folder, so each line must change how an agent builds there. ${STANDARD} states what a good one holds. The root CLAUDE.md, README.md and the skills already state repository-wide rules; a folder file never restates them. Edit nothing outside the files your step names.`

phase('Basis')
await parallel([
  () => agent(`Write ${STANDARD}: what a folder CLAUDE.md holds for current Claude models, from the Claude Code documentation (claudeCodeDocs MCP: memory, nested files, AGENTS.md). Content that earns a line: the folder's role and boundaries, the shape its code takes and why, recurring mistakes its history shows with the principle that prevents each, and settled decisions that look arbitrary. Content that earns none: what the root, README or a skill states, the obvious, file listings and counts, and today's names stated as law. Form: at most 125 lines in the repository's divider and label style. A level earns a file only for what the level above cannot say. Return at most 8 lines.`, { label: 'basis:standard', phase: 'Basis' }),
  () => agent(`Read the root CLAUDE.md, README.md, every skill's headings, and every CLAUDE.md and AGENTS.md outside the root. Write ${DIR}/EXISTING.md: per folder file, each line another file already states, each line the code contradicts, and what in it is folder-specific. Return at most 6 lines.`, { label: 'basis:existing', phase: 'Basis' }),
])

phase('Map')
const maps = await parallel(args.areas.map((area) => () => agent(`${BASIS}
Reader for ${area}. Read ${STANDARD}, ${DIR}/EXISTING.md, the code under ${area} and \`git log --stat -- ${area}\`, with \`git show\` of commits that rebuilt, reverted or fixed something there. Per folder level, state the principle that would have prevented the repeated fixes, what a file there would hold, and what share a level above covers. Report only levels with guidance the root, README and skills lack. Edit nothing. Return (structured) the findings.`, { label: `map:${area}`, phase: 'Map', schema: { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } } }, required: ['findings'] } }).then((m) => m && `## ${area}\n${m.findings.map((f) => `- ${f}`).join('\n')}`)))

phase('Select')
const chosen = await agent(`${BASIS}
Judge. From these maps, write ${SELECTION}: the folders that get a CLAUDE.md with the outline each holds, and the existing files to delete because a level above covers them. Choose the highest level that covers a group.
${maps.filter(Boolean).join('\n\n')}
Return (structured) the folders to write and the files to delete.`, { label: 'select', phase: 'Select', schema: { type: 'object', properties: { write: { type: 'array', items: { type: 'string' } }, remove: { type: 'array', items: { type: 'string' } } }, required: ['write', 'remove'] } })

phase('Write')
await pipeline(chosen?.write ?? [],
  (folder) => agent(`${BASIS}
Write ${REPO}/${folder}/CLAUDE.md whole from its outline in ${SELECTION}, within 125 lines, and make ${folder}/AGENTS.md a relative symlink to it. Return at most 4 lines.`, { label: `write:${folder}`, phase: 'Write' }),
  (_, folder) => agent(`${BASIS}
Adversary for ${REPO}/${folder}/CLAUDE.md. Cut each line that restates another file, states the obvious, freezes today's names or counts, or that the code contradicts, and sharpen each principle in place. Return at most 4 lines.`, { label: `challenge:${folder}`, phase: 'Write' }),
)

phase('Close')
return agent(`${BASIS}
Delete ${(chosen?.remove ?? []).join(', ') || 'no files'} with their AGENTS.md links and any AGENTS.md link that points nowhere. Read every folder CLAUDE.md with the root CLAUDE.md and keep each point once, at the highest level that holds it. Return at most 6 lines: the final file set with line counts.`, { label: 'close', phase: 'Close' })
