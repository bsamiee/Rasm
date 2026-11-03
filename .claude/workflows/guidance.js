export const meta = {
  name: 'guidance',
  description: 'Map the tree and its history, then write or rebuild folder CLAUDE.md files holding facts no level above states, each challenged in place',
  whenToUse: 'After a committed round of rebuilds; args.areas the top-level folders to map; edits only CLAUDE.md files outside the root and their AGENTS.md links',
  phases: [
    { title: 'Basis', detail: 'existing folder guidance, line by line' },
    { title: 'Map', detail: 'one reader per area over its code and history' },
    { title: 'Select', detail: 'one judge picks levels needing a file' },
    { title: 'Write', detail: 'per chosen file a writer, then an adversary judging every line in place, a nested file after the files above it and others at once' },
    { title: 'Close', detail: 'one pass across every file' },
  ],
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPO = '/Users/bardiasamiee/Developer/Rasm'
const DIR = `${REPO}/.artifacts/workflows/guidance`
const STANDARD = `${REPO}/.claude/skills/agent-builder/references/folder-guidance.md`
const EXISTING = `${DIR}/EXISTING.md`
const SELECTION = `${DIR}/SELECTION.md`
const BASIS = `Load Skill(clean-prose) and read the folder CLAUDE.md standard ${STANDARD} first.`
const PLURALS = new Intl.PluralRules('en')
const NOUNS = { file: { one: 'folder file', other: 'folder files' } }

// --- [MODELS] --------------------------------------------------------------------------

const FOUND = { type: 'object', properties: { findings: { type: 'array', items: { type: 'string' } } }, required: ['findings'] }

// --- [OPERATIONS] ----------------------------------------------------------------------

if (!Array.isArray(args?.areas) || args.areas.length === 0 || !args.areas.every((area) => typeof area === 'string')) throw new Error(`args.areas takes the top-level folders to map, got ${JSON.stringify(args?.areas ?? null)}`)
const relative = (path) => path.replace(`${REPO}/`, '').replace(/^\.\//, '').replace(/\/+$/, '')
const counted = (count, noun) => `${count} ${noun[PLURALS.select(count)]}`
const live = (results, role) => {
  const dead = results.filter((result) => !result).length
  if (dead > 0) log(`${dead} ${role} returned nothing`)
  return results.filter(Boolean)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

log(`guidance over ${args.areas.join(', ')}`)
phase('Basis')
const existing = await agent(`${BASIS}
Read every skill's headings and every CLAUDE.md and AGENTS.md outside the root. Write ${EXISTING}: per folder file, each line the root CLAUDE.md, README.md or a skill states, each line the code contradicts, and what in it is folder-specific. Return at most 6 lines.`, { label: 'basis:existing', phase: 'Basis' })
if (!existing) throw new Error(`basis agent returned nothing, ${EXISTING} is unwritten`)

phase('Map')
const maps = live(await pipeline(args.areas,
  (area) => agent(`${BASIS}
Reader for ${area}. Read ${EXISTING}, the code under ${area} and \`git log --stat -- ${area}\`, with \`git show\` of commits that rebuilt, reverted or fixed something there. Per folder level, state the principle preventing the repeated fixes, what a file there holds, and what share a level above covers. Report each level holding guidance the root, README and skills lack. Edit nothing. Return (structured) the findings.`, { label: `map:${area}`, phase: 'Map', schema: FOUND }),
  (map, area) => map && `## ${area}\n${map.findings.map((finding) => `- ${finding}`).join('\n')}`,
), 'readers')
if (maps.length === 0) throw new Error(`every reader of ${args.areas.join(', ')} returned nothing`)

phase('Select')
const chosen = await agent(`${BASIS}
Judge. From ${EXISTING} and the maps below write ${SELECTION}: the folders that get a CLAUDE.md with the outline each holds, and the existing files to delete.
${maps.join('\n\n')}
Return (structured) the folders to write and the files to delete, as paths from the repository root.`, {
  label: 'select',
  phase: 'Select',
  schema: { type: 'object', properties: { write: { type: 'array', items: { type: 'string' } }, remove: { type: 'array', items: { type: 'string' } } }, required: ['write', 'remove'] },
})
if (!chosen) throw new Error('select agent returned nothing')
const folders = [...new Set(chosen.write.map(relative))].filter(Boolean)
const removed = chosen.remove.map(relative)

phase('Write')
const chains = folders.toSorted((left, right) => left.split('/').length - right.split('/').length).reduce((held, folder) => {
  const parents = folders.filter((other) => other !== folder && folder.startsWith(`${other}/`))
  const reads = parents.length > 0 ? ` Read ${parents.map((parent) => `${REPO}/${parent}/CLAUDE.md`).join(', ')} as they stand, and state no fact they hold.` : ''
  return held.set(folder, Promise.all(parents.map((parent) => held.get(parent))).then(() => agent(`${BASIS}
Write ${REPO}/${folder}/CLAUDE.md whole from its outline in ${SELECTION}, and make ${folder}/AGENTS.md a relative symlink to it.${reads} Return at most 4 lines.`, { label: `write:${folder}`, phase: 'Write' }))
    .then((wrote) => wrote && agent(`${BASIS}
Adversary for ${REPO}/${folder}/CLAUDE.md. Read the file whole and the code under ${folder}, and judge every line against the standard and the code: cut each line the standard gives no place, rewrite each the code contradicts, and sharpen each principle in place.${reads} Return at most 4 lines.`, { label: `challenge:${folder}`, phase: 'Write' })))
}, new Map())
const written = live(await Promise.all(chains.values()), 'writers and adversaries')
log(`${written.length} of ${counted(folders.length, NOUNS.file)} written and challenged`)

phase('Close')
const closed = await agent(`${BASIS}
${removed.length > 0 ? `Delete ${removed.join(', ')} with their AGENTS.md links. ` : ''}Delete each dangling AGENTS.md link. Read every folder CLAUDE.md with the root CLAUDE.md and place each fact as the standard places it. Return at most 6 lines: the final file set with line counts.`, { label: 'close', phase: 'Close' })
return closed ?? 'close agent returned nothing'
