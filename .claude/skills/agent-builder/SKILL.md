---
name: agent-builder
description: "Use when writing, rebuilding, or reviewing an agent definition, covering skill and agent divide, sections, integration, checks, and description."
---

# [AGENT_BUILDER]

Agent definitions hold one role with procedure, owned files, and proof. Every sentence in one is a sentence its agent acts on during a run.

- [01]-[TOOL_FORMS](references/tool-forms.md): Proven forms and facts per CLI tool and Nx target

## [01]-[DIVIDE]

| [INDEX] | [HOLDER] | [CONTENT]                                                                        |
| :-----: | :------- | :------------------------------------------------------------------------------- |
|  [01]   | Skill    | Knowledge of a subject every caller applies: intent, criteria, facts of its tools |
|  [02]   | Agent    | One role's purpose, scope its prompt supplies, order it works in, proof it gives  |

A fact sits in the skill or the agent, once.

Building an agent on a preloaded skill:
- Step 1 reads the references the role applies as `Load <skill>, read <reference>`, one per branch or scope kind
- A skill one branch applies loads as `Skill(<name>)` in the step under that condition
- Numbered run orders with commands per step move from the reference into the procedure, the reference keeps each step's criterion
- Steps name a reference sequence as `under the <name> sequence of <reference>` with the call or reading it lacks

Fact placement:
- Facts about run order, proof, or scope go in the agent beside the step they change
- Facts about a tool's output format, attribution, or refusal go in the skill documenting the tool
- Steps repeated across agents with one reading become a rule family at `lint`, a skill script, or a target each agent names

Agents with no skill of their own preload `clean-prose` and `ast-grep`. A judgment repeated across runs becomes a criterion of the subject's skill, or of a new skill when none owns the subject.

Skills with no agent hold each sequence as a numbered reference with a criterion per step. Runs that repeat the same discovery steps and gate get an agent.

Roles with inputs of different kinds, different gates, or disjoint owned files are separate agents. One agent takes a difference the prompt names as scope or direction when discovery steps and gate hold for every value.

## [02]-[SECTIONS]

Files open with frontmatter and a `# [NAME]` heading, then sections in run order as XML elements named for purpose. A blank line inside each tag gives markdown rules one hit per entry:

| [INDEX] | [SECTION]           | [PURPOSE]                                                                                     |
| :-----: | :------------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `name`              | File stem                                                                                     |
|  [02]   | `description`       | Delegation sentence                                                                           |
|  [03]   | `skills`            | Skills every run applies                                                                      |
|  [04]   | `color`             | Transcript color: red, blue, green, yellow, purple, orange, pink, cyan                        |
|  [05]   | `role`              | Addresses the agent as you, states purpose, scope, owned files, and decisions                 |
|  [06]   | `context_gathering` | Discovery steps in order before the first edit                                                |
|  [07]   | `sources`           | Question-to-source table                                                                      |
|  [08]   | `decision`          | Proven facts that decide a reading                                                            |
|  [09]   | `procedure`         | Imperative steps in run order, each judgment naming its criterion                             |
|  [10]   | `gate`              | Proof commands run at close                                                                   |
|  [11]   | `done_when`         | Observable conditions of a finished run                                                       |

[ROLE]:
- Role opens with purpose in one paragraph, scope comes from the prompt
- Runs span every file, row, and pass in scope
- Defaults derive from the repository at the first step (default commit, scoped `check` target)
- Inputs name no file an agent creates
- Role acts on every fact the run finds
- User choices are reported with the options seen when found, work outside a reported choice completes
- Owned files sit in a `role` table with a content column, files outside the table stay as found
- Prompts to a worker hold scope, steps or intent, commit, checks, and every fact the run cannot retrieve
- Paths that differ per tree (`<logs>`, `<build>`) name their query and scope input, with every separator written
- Repository names in an agent's own paths appear as their value (`rasm`)

[CONTEXT_GATHERING]:
- Discovery skips facts present at spawn
- Discovery commands name a pattern (an `fd` extension, a diff filter, a graph query, the outline target)
- Git scopes list files present on disk
- Discovery reads the effective set a tool evaluates before the file configuring it
- Reads of a generated or large file locate a declaration by the literal the code spells, then `Read` that range
- Under a shared directory, steps select an input by a line the run's own command printed or a path the prompt names
- Measurements write under a private directory the procedure deletes
- Build steps derive `<build>` from the solution's project list
- Discovery over a language with a rule family is one scan of the family over scope
- Discovery over a mixed scope runs once over one file of each kind
- Skills every fork needs load through `Skill` before the first fork
- On a tree other agents edit, runs record `git diff --numstat` over scope before the first spawn
- The last discovery step runs every gate command naming no file the run creates, in whole-language form, as baseline
- Measured duration decides where a command runs: per cycle when seconds, at baseline and gate when minutes

[SOURCES]:
- Sources rows hold question, source, and exact call
- Sources per question rank file on disk, then `<tool> --help`, then documentation
- MCP rows hold the argument value that changes the answer, one row per reading
- Rows for a result a limit truncates name the form that returns items
- Checks with severity raised by a repository setting print in the error list, rows name that list
- Rows for a call past the result limit name a `jq` path over the file the result names
- Rows for a failure name a listing locating the failing unit before a log explaining it
- The table ends with the precedence sentence: installed declaration, type, or binary decides over a page or report

[DECISION]:
- Decision facts are ones a run got wrong once, each with failure text and the right form
- Every `decision` holds the evidence sentence: an empty scope is a valid result with the commands that proved it, unseen output is no evidence

[PROCEDURE]:
- Steps write paths from repository root
- Steps name a tool in call form with a deciding argument
- Steps spell commands as the `.claude/settings.json` allow list grants them
- Each edit is one exact-string replacement with its result read
- Tables one step uses sit under that step, tables more than one step names sit in their own element between `decision` and `procedure`
- Procedure bounds its fix-and-prove cycles with a count
- Under a shared directory, deletions name the path the tool printed
- Checks that print nothing both on a miss and on a broken form prove their positive case first
- Proofs of an interactive program run under `expect -c`
- Procedure ends by deleting every probe directory, temporary environment, and disposable input a proof wrote, then runs the gate

[GATE]:
- Gate lines pair command with result line: exit 0 and no output, `N passed; 0 failed`, a named line in the output
- Formatter gates run the lint target's check form or compare `git diff | shasum` before and after
- Gate lines proving work happened read the artifact or task record
- Gates for a scan or check silent on clean hold a coverage count or a line the tool prints when it runs
- Tools that report a missing input at exit 0 get the missing-input line named beside the exit code
- Gate commands name their selector and restate no default
- Gate lines hold over the current tree, a line failing over the tree names the run's owned subset
- Gates on a shared tree read scope diff past recorded rows, then its complement
- Timed gate lines hold a bound three runs agree on and report the maximum
- Gates over a rule family name `ast-grep scan` over scope
- Cached targets prove a scope when `inputs` hash it

[DONE_WHEN]:
- Done conditions name the replaced form with the command whose empty output proves absence
- Done conditions hold every gate result line in the transcript, the empty gate included
- Done conditions hold no partial edit, deferred value, workaround, or probe artifact

Agents of one family share the baseline step and evidence sentence.

## [03]-[INTEGRATION]

Steps name tools in the form the harness runs:

[CLAUDE_CODE_TOOLS]:
- Commands with outputs one reading consumes join one `Bash` call, commands with output that decides the next step run alone
- Steps that `TaskStop` a background process name the cleanup the process skipped
- Spawns appear as `Agent(subagent_type: "<name>")` with the prompt source named

[MCP_SERVERS]:
- MCP tools appear as `mcp__<server>__<tool>`
- Searches take bounded text form, count from the header
- Unfiltered calls come first, an exact filter (`project`) answers zero where the unfiltered call lists

[CLI_TOOLS]:
- Commands take the documented form with flags `--help` prints
- Tools taking one argument get one per call
- Repository defaults sit at their source: `mise.toml` `[env]`, the target, or the tool's configuration file
- Selections derive from the files that own them
- Files the run edits or judges line by line read whole
- Values a run derives come from the declaring file through the evaluating tool
- Paths a tool resolves outside the shell are absolute
- Binary paths come from `mise which <name>` or `command -v <name>`

[NX_TARGETS]:
- Targets run as `nx run <project>:<target>`
- Checkers over a source scope run through `nx affected --files`
- Fix loops name a per-file command of each kind in scope

[SCRIPTS]:
- Skill scripts run by path with a subcommand, steps state the domain a script accepts
- Disposable files sit under `.cache/<agent-name>/` as owned files
- Scoping proofs sit at the path the glob names
- Use `ast-grep` for rule family layout

## [04]-[MISTAKES]

Sections build from mistakes runs showed, each as a positive rule in its section:
1. List every mistake from transcripts, review findings, and reports, one line with the run that showed it
2. Cluster lines by category: judgment, ordering, discovery, proof, scope, repetition
3. Move each judgment mistake to the skill as a criterion
4. Write each other mistake in positive form in the section of its category
5. Delete the list once every line sits in a section

| [INDEX] | [CATEGORY] | [SECTION]                               |
| :-----: | :--------- | :-------------------------------------- |
|  [01]   | Discovery  | `context_gathering` step                |
|  [02]   | Proof      | `gate` line                             |
|  [03]   | Scope      | `role` sentence                         |
|  [04]   | Ordering   | `procedure` step                        |
|  [05]   | Repetition | Rule, script, or target the agents name |

## [05]-[EXCLUSIONS]

Sentences that state what another file owns go from the agent:
- Sentences a preloaded skill states
- `Skill` calls of a skill the `skills` list names
- Delegation judgment (who forks, who starts fresh, who reviews), the call and prompt stay
- Discovery steps that map a file the next step reads whole
- Sources rows that restate root instructions tool routing
- Soft bounds, replaced by condition, command, or criterion
- Repository, product, and path names in a generic skill

## [06]-[CHECKS]

Checks on a finished file:
- `ast-grep scan --no-ignore hidden <agent>` and `typos <agent>` print nothing
- `rg -n --pcre2 '^(- |[0-9]+\. |\| +)?(A|An) [a-z`]' <agent>` prints nothing
- `awk '{n=split($0,s,/[.:] /); for(i=1;i<=n;i++){t=tolower(" " s[i] " "); c=gsub(/ the /,"&",t); d=gsub(/ and /,"&",t); if(c>2||d>2) print FNR": the="c" and="d}}' <agent>` lists sentences to judge, at most two `the` and two `and` each
- `rg -n 'relevant|as needed|when the task permits|testing scope|with judgment|appropriate' <agent>` prints nothing
- `awk '/^<context_gathering>$/,/^<\/context_gathering>$/' <agent> | rg -n ToolSearch` and `rg -n 'rows? [0-9]' <agent>` print nothing
- Every path the file names exists, `rg -o '[A-Za-z0-9_./-]+\.(md|sh|yml|ts|py|json)' <agent> | sort -u` against `ls`
- Every `subagent_type` value the file names exists under an agents directory and in the spawn's agent type list
- Every MCP tool the file names loads, `rg -o 'mcp__[a-z-]+__[a-z_]+' <agent> | sort -u`, each through `ToolSearch(query: "select:<name>")`
- Every target the file names exists, `nx show project <project> --json | jq '.targets|keys'` prints it
- Every `skills` entry exists as a skill directory
- Every command a step or gate holds ran from repository root in the writing session, over a real and a missing input, with its result line kept
- Every cached gate target ran once over a planted defect
- `claude plugin validate <agents dir>` for a project agent, `claude plugin validate <plugin>` for a plugin agent, reports no parse error
- `git diff <commit> -- <agent>` on a rebuilt file, every command, row, and threshold the earlier revision held stays or the report names it
- Every fact proven on one case lands after a case of another shape confirms it

## [07]-[DESCRIPTION]

The description is written last, from a full read of the finished file, its preloaded skills, and root instructions tool routing. Skills and subagents share one form, `Use when <situation>, covering <topics>`. Sentences hold at most 25 words on a `description:` line of at most 160 columns. Descriptions name a tool, language, or file kind a delegating prompt holds, situation clauses state what the routing line lacks, covering lists name section subjects.
