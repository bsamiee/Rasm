---
name: agent-builder
description: "Use when writing, rebuilding, or reviewing an agent definition, covering skill and agent divide, sections, integration, checks, and description."
---

# [AGENT_BUILDER]

Agent definitions hold one role with its discovery steps, procedure, and owned files. Every sentence in one is a sentence its agent acts on during a run.

[REFERENCES]:
- [01]-[TOOL_FORMS](references/tool-forms.md): Forms and facts per CLI tool, MCP tool, and Nx target

## [01]-[DIVIDE]

| [INDEX] | [HOLDER] | [CONTENT]                                                                         |
| :-----: | :------- | :-------------------------------------------------------------------------------- |
|  [01]   | Skill    | Knowledge of a subject every caller applies: intent, criteria, facts of its tools |
|  [02]   | Agent    | One role's purpose, scope its prompt supplies, order it works in                  |

Facts sit in the skill or the agent, once:
- Step 1 reads the references the role applies as `<reference> of <skill>`, `Skill(<name>)` first for a skill the `skills` list lacks
- References one branch applies sit under that condition
- Numbered run orders with commands move from a reference into the procedure, the reference keeps each step's criterion
- Steps name a reference sequence as `under the <name> sequence of <reference>` with the call or reading it lacks
- Mistakes a run showed enter as the criterion they imply: a judgment in the skill, a tool behavior as a decision fact
- Steps repeated across agents with one reading become a rule or a target each agent names

Runs that repeat the same discovery steps get an agent. Roles with inputs of different kinds or disjoint owned files are separate agents, one agent takes a difference the prompt names as scope or direction when discovery steps hold for every value.

## [02]-[SECTIONS]

Files open with frontmatter and a `# [NAME]` heading, then sections in run order as XML elements named for purpose, with a blank line after each opening tag, text against the tag parses as one HTML block:

| [INDEX] | [SECTION]           | [PURPOSE]                                                                     |
| :-----: | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `name`              | Lowercase and hyphens, equal to the file stem                                 |
|  [02]   | `description`       | Delegation sentence                                                           |
|  [03]   | `skills`            | Skills preloaded whole at spawn, every run applies each                       |
|  [04]   | `tools`             | Allowlist of tool names or `mcp__<server>` patterns, absent for every tool    |
|  [05]   | `color`             | Transcript color: red, blue, green, yellow, purple, orange, pink, cyan        |
|  [06]   | `role`              | Addresses the agent as you, states purpose, scope, owned files, and decisions |
|  [07]   | `context_gathering` | Discovery steps in order before the first edit                                |
|  [08]   | `sources`           | Question-to-source table                                                      |
|  [09]   | `decision`          | Tool facts that decide a reading                                              |
|  [10]   | `procedure`         | Imperative steps in run order, each judgment naming its criterion             |
|  [11]   | `done_when`         | Observable conditions of a finished run                                       |

[ROLES]:
- Role opens with purpose in one paragraph, scope comes from the prompt, an empty scope defaults to the set a file the role reads declares
- Runs cover every file in scope and act on every fact they find
- User choices are reported with the options seen, work outside a reported choice completes
- Owned files sit in a `role` table with a content column, files outside the table stay as found
- Placeholders (`<logs>`, `<tempdir>`) name the command or file that supplies their value, with every separator written

[CONTEXT_GATHERING]:
- Discovery skips what spawn supplies: root instructions, git status, preloaded skills
- Discovery steps derive scope from a pattern over the files owning it (an extension, a diff filter, a graph query, an outline)
- Discovery reads the effective set a tool evaluates before the file configuring it
- Reads of a generated or large file locate a declaration by the literal the code spells, then `Read` that range
- Under a shared directory, steps read and delete the path the run's own command printed or the prompt names

[SOURCES]:
- Sources rows hold question, source, and exact call
- MCP rows hold the argument value that changes the answer, one row per reading
- Rows for a call past the result limit name a `jq` path over the file the result names
- Rows for a failure name a listing locating the failing unit before a log explaining it
- Tables end with the precedence sentence: file on disk, installed declaration, or binary decides over a page or report

[DECISIONS]:
- Decision facts are tool behaviors that decide a reading, each with the output line that shows it

[PROCEDURES]:
- Steps write paths from repository root and name a tool in call form with a deciding argument
- Steps spell commands as the allow list grants them
- Each edit is one exact-string replacement
- Checkers of the role's files sit in one step that runs them over scope and fixes each finding
- Tables one step uses sit under that step, tables more than one step names sit in their own element between `decision` and `procedure`
- Procedure bounds its fix cycles with a count
- Disposable files sit under a private directory the role table owns, procedure ends by deleting it

[DONE_WHEN]:
- Done conditions hold no partial edit, deferred value, workaround, or run residue

## [03]-[INTEGRATION]

Steps name tools in the form the harness runs:

[CLAUDE_CODE_TOOLS]:
- Commands with outputs one reading consumes join one `Bash` call, commands with output that decides the next step run alone
- MCP tools appear as `mcp__<server>__<tool>`, searches take a bounded result count
- Paths an MCP tool resolves outside the shell are absolute

[CLI_TOOLS]:
- Commands take the documented form with flags `--help` prints
- Steps hold no prefix or flag for a default a configuration file or the target owns
- Files the run edits or judges line by line read whole, with no outline beside the read
- Checks run through the existing target, a check a target already runs is one line naming the target
- Fix loops name a per-file command of each kind in scope

## [04]-[EXCLUSIONS]

Sentences that state what another file owns, or bind the run past its prompt, go from the agent:
- Sentences a preloaded skill states
- `Skill` calls of a skill the `skills` list names
- Discovery steps that map a file the next step reads whole
- Sources rows that restate root instructions tool routing
- Scope enumerations (`one of <a>, <b>, <c>`), a pattern or the prompt supplies scope
- Output contracts, a run reports what changed and choices for the user
- Steps that write a test, spec, fixture, harness, or proof script to verify the run's own work
- Soft bounds, replaced by condition, command, or criterion
- Narration of a past run, replaced by the criterion it showed

## [05]-[CHECKS]

Checks on a finished file:
- `ast-grep scan --no-ignore hidden <agent>` prints nothing
- `claude plugin validate <agents dir>` for a project agent, `claude plugin validate <plugin>` for a plugin agent, prints `Validation passed`
- Every path and `skills` entry the file names exists on disk
- Every target and MCP tool the file names exists, `nx show project <project> --json | jq '.targets|keys'` and `ToolSearch(query: "select:<tool>")`
- Rebuilt files keep every command and row of the earlier revision, `git diff <commit> -- <agent>`, or the report names the drop

## [06]-[DESCRIPTION]

Descriptions are written last, from a full read of the finished file, its preloaded skills, and root instructions tool routing. Skills and agents share one form, `Use when <situation>, covering <topics>`, at most 25 words. Descriptions name a tool, language, or file kind a delegating prompt holds, the situation states what the routing line lacks, covering names section subjects.
