---
name: agent-builder
description: "Use when writing, rebuilding, or reviewing an agent definition, covering skill and agent divide, sections, integration, checks, and description."
argument-hint: "[agent-file]"
---

# [AGENT_BUILDER]

Agent definitions hold one role with its discovery steps, procedure, and owned files. Every sentence in a definition states what its agent acts on during a run.

[REFERENCES]:
- [01]-[TOOL_FORMS](references/tool-forms.md): Forms and facts per CLI tool, MCP tool, and Nx target

## [01]-[DIVIDE]

| [INDEX] | [HOLDER] | [CONTENT]                                                                         |
| :-----: | :------- | :-------------------------------------------------------------------------------- |
|  [01]   | Skill    | Knowledge of a subject every caller applies: intent, criteria, facts of its tools |
|  [02]   | Agent    | One role's purpose, scope its prompt supplies, order it works in                  |

Facts sit once, in skill or agent:
- Step 1 reads references the role applies as `<reference> of <skill>`, `Skill(<name>)` first for a skill missing from `skills`
- References one branch applies sit under its condition
- Numbered run orders with commands move from a reference into `procedure`, the reference keeps each step's criterion
- Steps name a reference sequence as `under the <name> sequence of <reference>` with the call or reading it lacks
- Mistakes a run showed enter as their implied criterion, a judgment in the skill or a tool behavior as a decision fact
- Steps repeated across agents with one reading become a rule or a target each agent names

Runs that repeat one set of discovery steps get an agent. Roles with inputs of different kinds or disjoint owned files are separate agents. When discovery steps hold for every value, one agent takes a difference the prompt names as scope or direction.

## [02]-[SECTIONS]

Markdown parses text against an opening tag as one HTML block, and a blank line follows each opening tag. Files open with frontmatter and a `# [NAME]` heading, then sections in run order as XML elements named for purpose:

| [INDEX] | [SECTION]           | [PURPOSE]                                                                     |
| :-----: | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `name`              | Lowercase and hyphens, equal to the file stem                                 |
|  [02]   | `description`       | Delegation sentence                                                           |
|  [03]   | `skills`            | Skills preloaded whole at spawn, every run applies each                       |
|  [04]   | `tools`             | Allowlist of tool names or `mcp__<server>` patterns, absent for every tool    |
|  [05]   | `disallowedTools`   | Denylist in `tools` form, applied before `tools` resolves                     |
|  [06]   | `color`             | Transcript color: red, blue, green, yellow, purple, orange, pink, cyan        |
|  [07]   | `role`              | Addresses the agent as you, states purpose, scope, owned files, and decisions |
|  [08]   | `context_gathering` | Discovery steps in order before the first edit                                |
|  [09]   | `sources`           | Question-to-source table                                                      |
|  [10]   | `decision`          | Tool facts that decide a reading                                              |
|  [11]   | `procedure`         | Imperative steps in run order, each judgment naming its criterion             |
|  [12]   | `done_when`         | Observable conditions of a finished run                                       |

[ROLES]:
- Role opens with purpose in one paragraph
- Prompts supply scope, an empty scope defaults to the set declared in a file the role reads
- Runs cover every file in scope and act on every fact they find
- User choices are reported with the options seen, work outside a reported choice completes
- Owned files sit in a `role` table with a content column, files outside the table stay as found
- Placeholders (`<logs>`, `<tempdir>`) name the command or file that supplies their value, with every separator written

[CONTEXT_GATHERING]:
- Discovery skips what spawn supplies: root instructions, git status, preloaded skills
- Discovery steps derive scope from a pattern over the files owning it (an extension, a diff filter, a graph query, an outline)
- Discovery reads a tool's effective set before the file configuring it
- Reads of a generated or large file locate a declaration by its spelled literal, then `Read` its range
- Under a shared directory, steps read and delete paths the run's own command printed or its prompt names

[SOURCES]:
- Sources rows hold question, source, and exact call
- MCP rows hold each answer-changing argument value, one row per reading
- Rows for a call past its result limit name a `jq` path over the file its result names
- Rows for a failure name a listing locating the failing unit before a log explaining it
- Tables end with a sentence ranking file on disk, installed declaration, or binary over a page or report

[DECISIONS]:
- Decision facts pair each tool behavior with the output line showing it

[PROCEDURES]:
- Steps write paths from repository root and name a tool in call form with a deciding argument
- Steps spell commands as the allow list grants them
- Each edit is one exact-string replacement
- Checkers of the role's files sit in one step that runs them over scope and fixes each finding
- Tables one step uses sit under it, tables more than one step names sit in their own element between `decision` and `procedure`
- Procedure bounds its fix cycles with a count
- Disposable files sit under a private directory the role table owns, procedure ends by deleting it

[DONE_WHEN]:
- Done conditions hold no partial edit, deferred value, workaround, or run residue

## [03]-[INTEGRATION]

Steps name tools in the form the harness runs:

[CLAUDE_CODE_TOOLS]:
- Bodies and frontmatter hold call form, injection sits in a preloaded skill when every caller of that skill consumes its output
- Commands with outputs one reading consumes join one `Bash` call, commands with output that decides the next step run alone
- MCP tools appear as `mcp__<server>__<tool>`, a plugin server's as `mcp__plugin_<plugin>_<server>__<tool>`, searches take a bounded result count
- Paths an MCP tool resolves outside the shell are absolute

[CLI_TOOLS]:
- Commands take the documented form with flags `--help` prints
- Steps hold no prefix or flag for a default a configuration file or the target owns
- Files the run edits or judges line by line read whole, with no outline beside the read
- Checks a target runs are one step naming the target
- Fix loops name a per-file command of each kind in scope

## [04]-[EXCLUSIONS]

Sentences that state what another file owns, or bind the run past its prompt, go from agents:
- Sentences a preloaded skill states
- `Skill` calls of a skill the `skills` list names
- Discovery steps that map a file the next step reads whole
- Sources rows that restate root instructions tool routing
- Scope enumerations (`one of <a>, <b>, <c>`), a pattern or the prompt supplies scope
- Output contracts, a run reports what changed and choices for the user
- Steps that write a test, spec, fixture, harness, or script checking the run's own work
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

Write descriptions last, after a full read of the finished file, its preloaded skills, and root instructions tool routing. Skills and agents share one form, `Use when <situation>, covering <topics>`, at most 25 words. Descriptions name a tool, language, or file kind a delegating prompt holds, the situation states what the routing line lacks, covering names section subjects.
