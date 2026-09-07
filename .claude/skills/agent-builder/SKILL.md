---
name: agent-builder
description: "Use when writing or rebuilding an agent definition, covering the agent and skill divide, the sections, discovery steps, sources, gates, done conditions, checks, and the description."
---

# [AGENT_BUILDER]

An agent definition holds one role with its procedure, ownership, proof, and report, and every sentence in it is one the agent acts on during a run.

## [01]-[DIVIDE]

Skills hold knowledge and agents hold roles:

| [INDEX] | [HOLDER] | [CONTENT]                                                                                       |
| :-----: | :------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | Skill    | Intent, criteria, API facts, and the standard a file must meet                                  |
|  [02]   | Agent    | Role, input contract, discovery steps, hard-won facts, procedure, gate, done conditions, report |

An improvement lands in the skill when the mistake was a judgment, and in the agent when the mistake was ordering, discovery, proof, or scope. The agent preloads the skills its role applies through the `skills` list, and a reader with both files open finds each fact once.

Rules on the divide:
- Skills one route of the procedure applies are a `Skill` call in that step, and the references of a preloaded skill are reads in `context_gathering`
- Workflows of a skill take one reference with the criteria and one agent with the procedure, and the reference reads whole without the agent
- Hard-won facts are tool behavior a run observed that changes a command or a reading (`Edit` refuses a file `Read` did not open)
- Hard-won facts land in the agent beside the step, the read, or the reading they change, with the behavior as the reason clause, and in no skill
- Two roles are two agents when their prompts name inputs of different kinds, their gates differ, or their owned files are disjoint
- One agent takes a difference the prompt names as the scope or the direction, with the same discovery steps and gate for every value

## [02]-[SECTIONS]

The file opens with the frontmatter and a `# [NAME]` heading, then holds the sections in run order, each an XML element named for its purpose:

| [INDEX] | [SECTION]           | [PURPOSE]                                                                                               |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `name`              | The file stem in kebab case, unique across the tree, because one of two duplicates loads, by read order |
|  [02]   | `description`       | The signal the dispatching model matches, 20 to 25 words, `Use when <situation>, covering <topics>`     |
|  [03]   | `skills`            | The skills every run applies, each `SKILL.md` injected whole at spawn                                   |
|  [04]   | `color`             | The transcript color, one of red, blue, green, yellow, purple, orange, pink, cyan                       |
|  [05]   | `role`              | Addresses the agent as you, states the input contract, the owned files, the decisions, and the messages |
|  [06]   | `context_gathering` | Discovery steps in order before the first edit, built on commands that survive growth                   |
|  [07]   | `sources`           | Question-to-source table, the call or path that settles each question the run asks                      |
|  [08]   | `decision`          | Hard-won facts that settle a reading: evidence, valid empty results, precedence, what a plan yields to  |
|  [09]   | `procedure`         | Imperative steps in run order, each command named and each judgment naming its criterion                |
|  [10]   | `gate`              | Commands with the expected result line, every one run before the report                                 |
|  [11]   | `done_when`         | Observable conditions: the proof in the transcript, the replaced form absent, the probe artifacts gone  |
|  [12]   | `output`            | The report shape, bounded in lines, with named rows, the one text the dispatching agent reads           |

The rules of each section sit under its label:

[FRONTMATTER]:
- `model` stays absent unless the role needs one model, and the run takes the dispatch's model, else `CLAUDE_CODE_SUBAGENT_MODEL`, else the session's
- `tools` stays absent and inherits every tool, and a role that edits nothing sets `disallowedTools: Write, Edit` and keeps its MCP tools
- A `tools` list keeps `SendMessage`, because the named agents and messages to `main` reach the run through it alone
- Roles build on the tools a background run, the default, keeps (every MCP tool, `Read`, `Bash`, `Edit`, `Write`, `Skill`, `SendMessage`)

[ROLE]:
- The spawn supplies the body as the system prompt, the task prompt, the root instructions hierarchy, and the git status from the session start
- The spawn supplies the preloaded skills whole and the named agents as a snapshot, and a finding for an agent started later goes to `main`
- The spawn supplies none of the dispatcher's conversation, files read, or auto memory, and the prompt carries every fact the run cannot retrieve
- The role states each input the prompt names, the default for an omitted one, and the `result: not started` reply for an unusable prompt
- `AskUserQuestion` reaches no subagent, and a role that meets a question for the user sends it to `main` with the options it sees
- The owned files sit in a table in `role` with a content column, and a change outside the table goes to `main` in the round it arises
- Messages to the dispatching agent are one sentence in `role`: what the agent sends, when, and to which address
- A message names the file, the current text, the proposed text, and the reason

[CONTEXT_GATHERING]:
- Discovery steps skip the root instructions file and the files it imports, present at spawn
- Discovery commands name a shape (`fd -e <ext> <dir>`, `git diff --name-only <commit>`, the graph query, the outline target), a file list goes stale
- The last discovery step runs every gate command once as the baseline, and the report attributes the agent's lines alone

[SOURCES]:
- Sources rows hold the question, the source, and the exact call, ordered per question: the file on disk, `<tool> --help`, then the documentation
- Sources rows name a source the spawn supplies or the run retrieves, and the dispatcher's memory is neither
- The sources table ends with its precedence sentence: the installed declaration, type, or binary decides over a page or a report

[DECISION]:
- Decision facts are the ones a run got wrong once, each with the right form beside it
- Every `decision` holds the evidence sentence: an empty scope is a valid result with the commands that proved it, and unseen output is no evidence

[PROCEDURE]:
- Procedure steps write paths from the repository root, because `cd` does not persist between `Bash` calls in a subagent
- Each edit is one exact-string replacement with the result read, and a table one step alone uses sits under that step
- A table more than one step names sits in its own element named for its content, between `decision` and `procedure`
- The procedure bounds its fix-and-prove cycles with a count, and the remainder goes under `open:` with its evidence
- The procedure ends with the deletion of every probe directory, temporary environment, and disposable input a proof wrote, then the gate

[GATE]:
- Gate lines pair the command with its result line: exit 0 and no output, `N passed; 0 failed`, a named line present in the output
- Gate commands that rewrite files prove they rewrote nothing, by `git diff --exit-code` or equal `git diff | shasum` hashes before and after

[DONE_WHEN]:
- Done conditions are observable by a later reader: a file at a path, a line in an output, a form absent from a scope, a proof in the transcript
- Done conditions name the replaced form and the command with the empty output that proves its absence (`rg -n '<old>'`, a scan with no hit)
- Done conditions hold every gate result line in the transcript and no partial edit, deferred value, or workaround, and the empty gate is one of them

[OUTPUT]:
- The report names each row and its columns, and a report holds no narration
- Rows are `result:` from a closed set, `changes:`, `open:` with evidence and the fix, `sent:` with the confirmation, `gate:` with each result line
- The report grows during the run, because a run cut at `maxTurns` returns the report as it stands, marked partial
- Every finding, path, and proof line the dispatching agent acts on has a row, because the transcript stays unread
- Weaknesses in the agent or a preloaded skill go in a `suggestions:` row, and the run edits neither file

Agents of one family share the report rows, the message sentence, the baseline step, and the evidence sentence, and differ in the rest.

## [03]-[MISTAKES]

Build a section from the mistakes runs showed, and an agent holds no anti-pattern table, because each row is a positive rule in its section:
1. List every mistake seen in transcripts, review findings, and reports as one line with the run that showed it
2. Cluster the lines by category: judgment, ordering, discovery, proof, scope
3. Move each judgment mistake to the skill as a criterion
4. Write each other mistake as its positive form in the section where the agent meets it
5. Delete the mistake list when every line landed, and the sections carry it

| [INDEX] | [CATEGORY] | [SECTION]                  |
| :-----: | :--------- | :------------------------- |
|  [01]   | Discovery  | A `context_gathering` step |
|  [02]   | Proof      | A `gate` line              |
|  [03]   | Scope      | A `role` sentence          |
|  [04]   | Ordering   | A `procedure` step         |

## [04]-[EXCLUSIONS]

Sentences that state what another file owns leave the agent:
- Sentences a preloaded skill states, and the agent names the skill in `skills` alone
- A `Skill` call or a read of a skill the `skills` list names
- Delegation, fork, and review-assignment mechanics, owned by the dispatching skill
- Soft bounds (`relevant`, `as needed`, `when the task permits`, `with judgment`), replaced by the condition, the command, or the criterion
- Repository, product, and path names in a generic skill, and an agent file names its own paths

## [05]-[CHECKS]

Every check runs before the description is written:
- `awk 'length >= 150 && /^(- |\| |[0-9]+\. )/ {print FILENAME": "FNR}' <agent>` lists the entries to judge, each on its own
- The scan table of the prose skill over every line prints no hit
- `rg -n 'relevant|as needed|when the task permits|testing scope|with judgment|appropriate' <agent>` prints nothing
- Every path the file names exists: `rg -o '[A-Za-z0-9_./-]+\.(md|sh|yml|ts|py|json)' <agent> | sort -u` against `ls`
- Every agent the file names exists as a file under an agents directory
- Every `skills` entry exists as a skill directory, because a missing one is skipped with a warning in the debug log alone
- `claude plugin validate <agents dir>` reports no parse error, and a file without `name` is skipped as documentation in silence
- `git diff <commit> -- <agent>` on a rebuilt file, and every command, row, and threshold the earlier revision held returns or the report names it
- `claude -p 'Use the <name> agent to <task>' --output-format stream-json --verbose` shows the discovery steps before the first edit, then the rows
- The running session loads the first file of a new agents directory after a restart and a later edit within seconds, and a `-p` run starts fresh

## [06]-[DESCRIPTION]

The description is written last, from a full read of the finished agent, its preloaded skills, and the router file that lists agents. It names the tool or language, states in the situation clause what the router line lacks, and names the section subjects in the covering list, because a procedure step named there drifts with the next edit. Every description loads in the dispatching session at every start, and the harness warns when the set passes 15,000 tokens:
1. Write the 3 best candidates and grade each against the form, with the failure named per candidate
2. Start one fresh agent with the same brief and files to write and grade its own 3 candidates and grade yours
3. Implement the candidate both readings rank first
4. When the top picks differ, start a third fresh agent that reads the agent and both candidate sets and picks from its own reading
