# [FOLDER_GUIDANCE]

Folder `CLAUDE.md` files under the repository root, with their `AGENTS.md` links.

Use `clean-prose` for headings, labels, and sentences

## [01]-[LOADING]

Claude Code reads each file as context, and loading decides which facts a folder file can hold:
- Sessions load `CLAUDE.md` from the working directory and every directory above it at launch, subagents other than Explore and Plan included
- Subdirectory files load once Claude reads, writes, or edits a file there, a fact needed before the first file read sits a level above
- Loaded files concatenate from root to leaf with no override, and Claude follows either side of a contradiction
- Lines are context and Claude decides whether to follow them, a rule without exceptions becomes a check
- Claude reads `AGENTS.md` in a directory where no `CLAUDE.md` sits at or above it, Codex reads `AGENTS.md` alone
- `AGENTS.md` beside each folder `CLAUDE.md` is the relative symlink `AGENTS.md -> CLAUDE.md`, Claude reads its content once
- Edit and Write refuse a path through a symlink, edits go to `CLAUDE.md`

## [02]-[CONTENT]

Lines hold what an agent cannot find by reading the folder's code, or rediscovers every task:

| [INDEX] | [HOLDS]                                                          | [OMITS]                                                         |
| :-----: | :--------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Folder's role and boundaries against its neighbors               | Facts root `CLAUDE.md`, `README.md`, a skill, or a level states |
|  [02]   | Shape its code takes and the reason, where code alone misleads   | Layout, architecture, and conventions the code shows            |
|  [03]   | Principle preventing a mistake its fix and revert commits repeat | Narration of one fix or commit                                  |
|  [04]   | Decision absent from the code that reads as arbitrary            | Member, file, and count enumerations that drift with the code   |
|  [05]   | Command confirming work where the folder's targets differ        | Multi-step procedure a skill holds                              |
|  [06]   | Old pattern still present that new code takes no form from       | Emphasis, and instructions for a limit current models lack      |

Each line changes what an agent writes in the folder, and a line no task under it acts on goes.

## [03]-[PLACEMENT]

Levels hold a file only for facts the level above cannot state:
- Facts sit once, at the highest level holding them whole
- Groups of sibling folders sharing a fact take one file at their common parent
- Files a level above covers go with their `AGENTS.md` link

## [04]-[FORM]

Files hold at most 125 lines, every line read on every task under the folder:
- Files open with the folder's role in one sentence
- Lines state one concrete, checkable fact (command, path, form)
