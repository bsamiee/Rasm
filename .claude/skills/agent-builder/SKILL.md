---
name: agent-builder
description: "Use when writing, rebuilding, or reviewing an agent definition, covering skill and agent divide, sections, integration, checks, and description."
---

# [AGENT_BUILDER]

Agent definitions hold one role with its procedure, ownership, proof, and report, and every sentence in one is a sentence its agent acts on during a run. Every command in it ran from the repository root in the session that wrote it, with its result line beside the step in the change record.

- [01]-[HARNESS](references/harness.md): The spawn context, the tool pool, the frontmatter fields, MCP loading, and the hook rows that change a step
- [02]-[TOOL_FORMS](references/tool-forms.md): Proven forms and facts per CLI tool, Nx target, and script behind the integration principles

## [01]-[DIVIDE]

Skills hold knowledge and agents hold roles:

| [INDEX] | [HOLDER] | [CONTENT]                                                                                         |
| :-----: | :------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | Skill    | Knowledge of a subject every caller applies, the intent, the criteria, and the facts of its tools |
|  [02]   | Agent    | One role's run, the inputs it needs, the order it works in, the proof it gives, and its report    |

Improvements land in the skill when the mistake was a judgment, and in the agent when the mistake was ordering, discovery, proof, or scope. Agents preload the skills their role applies through `skills`, and a reader with both files open finds each fact once.

Building an agent on the skill it preloads:
- Step 1 reads the reference the role applies, `Load <skill>, read <reference>`, one per route or scope kind, because the spawn injects `SKILL.md`
- Preloaded skills load at spawn, the step adds no `Skill` call, and a skill one route applies is `Skill(<name>)` in the step that applies it
- Skills that own a tree the scope can enter are a `Skill` call under the condition that names the tree, because the preloaded pointer loads nothing
- Criteria stay in the skill, and the report names the section or rule id each finding met, and the agent restates none
- Numbered run orders with commands per step move from the reference into the procedure, and the reference keeps each step's criterion
- Sequences a reference holds appear in a step as `under the <name> sequence of <reference>` with the call or reading the reference lacks

Fact placement:
- Facts about a run's order, proof, or scope land in the agent beside the step or reading they change, with the behavior as the reason clause
- Facts about a tool's output shape, attribution, or refusal land in the skill that documents the tool, because every caller meets them
- Form errors the engine sees before a call runs (an arity, a flag pair, a rejected name) are hook rewrite rows, kept in `decision` until they land
- Facts a reference states leave the agent, and a fact a release defect causes goes to the record and to no skill
- Steps that repeat across agents with one reading become a rule family at `lint`, a skill script, or a target, and each agent names the command

Agents with no skill of their own preload the standards skills (`clean-prose`, `ast-grep`), read on each route the references of the skill owning the subject, hold their criteria as `decision` facts a run proved, and name in `suggestions:` a judgment the run repeated, because a repeated judgment is a criterion for the owning skill, or for a new skill when none owns the subject.

Skills with no agent hold the intent, criteria, API facts, and standard, hold each sequence as a numbered reference with its criterion per step that reads whole without an agent, and gain an agent when runs repeat the same discovery steps, gate, and report.

Two roles are two agents when their prompts name inputs of different kinds, their gates differ, or their owned files are disjoint, and one agent takes a difference the prompt names as the scope or the direction, with the same discovery steps and gate for every value.

## [02]-[SECTIONS]

The file opens with the frontmatter and a `# [NAME]` heading, then holds the sections in run order, each an XML element named for its purpose with a blank line inside each tag, because a markdown rule then reports one hit per entry:

| [INDEX] | [SECTION]           | [PURPOSE]                                                                                               |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `name`              | The file stem in kebab case, unique across the tree, because one of two duplicates loads, by read order |
|  [02]   | `description`       | The dispatch signal, `Use when <situation>, covering <topics>`, at most 25 words and 160 columns        |
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
- `tools` lists keep `SendMessage`, because a message to the dispatcher reaches the run through it alone
- Roles build on the tools a background run, the default, keeps (every MCP tool, `Read`, `Bash`, `Edit`, `Write`, `Skill`, `SendMessage`)
- Capabilities (`memory`, `hooks`, `effort`, `maxTurns`, `isolation`, `background`, `mcpServers`) join when a run shows the need the docs line names
- The change record holds each capability verdict with its run, and a field no run justifies stays absent
- `memory` stays absent, because a learned fact lands through `suggestions:`, the directory escapes the memory editor, and auto memory gates it
- Plugin agents load as `<plugin>:<name>` with `hooks`, `mcpServers`, and `permissionMode` ignored, and their lifecycle behavior is a plugin hook

[ROLE]:
- The spawn supplies the body as system prompt, the task prompt, the root instructions hierarchy, and the git status from the session start
- The spawn supplies the preloaded skills whole and the named agents as a snapshot, and an agent started later is absent from it
- The spawn supplies none of the dispatcher's conversation, files read, or auto memory, and the prompt holds every fact the run cannot retrieve
- The spawn appends the plugin's brief to a `fork` or `general-purpose` prompt and none to a named agent, with `output` as its whole contract
- The role states each input as a fact the run needs, in the form the prompt holds it (text, a file, a document set), else `result: not started`
- Defaults derive from the repository at the first step (`git rev-parse HEAD`, the scoped `check` target), and no input names a file an agent creates
- The role acts on every fact the run finds, extends its domain (an action, folder, rule, or package) under the root `README.md`, and hedges nowhere
- Sentences that defer a fact to a later run, another owner, or a reviewer leave, and the fact lands as a rule with its proof or in the report rows
- Choices that are the user's (a manifest outside every scope, a registry write, a daemon setting) are `open:` rows with the options seen, the rest lands
- `AskUserQuestion` reaches no subagent, and a question for the user is an `open:` row with the options seen
- The owned files sit in a table in `role` with a content column, and a change outside the table is a finding row in the round it arises
- Findings, open items, and suggestions travel in report rows, and a message goes to the dispatcher alone where a run blocks on its answer, addressed as its brief supplies it, else as `main`
- Every `role` holds that rule as one sentence, the same in every file, and no other sentence in the file names a message or an address
- Findings name file, current text, proposed text, and reason, and a blocking message names its question with every option seen
- Dispatching roles read a child's report as the `Agent` result or a completion notification
- Briefs to a worker hold the work facts (scope, steps or intent, commit, checks), and the report shape comes from the worker's file or the hook
- Paths a role derives (`<logs>`, `<build>`) name the query and its scope input, with every separator written, because trees differ in the value
- Repository names the file's own paths need appear as their value (`rasm`), because a placeholder a command fills is a step and once ran as a path

[CONTEXT_GATHERING]:
- Discovery steps skip the root instructions file and the files it imports, present at spawn
- Discovery commands name a shape (`fd -e <ext> <dir>`, `git diff --name-only --diff-filter=ACMR <commit>`, a graph query, the outline target), no list
- Scopes from git list files on disk, the filtered diff with `git ls-files --others --exclude-standard`, because a deleted path fails the read
- Discovery holds no `ToolSearch` step and no read of a preloaded `SKILL.md` section, because the schema loads at the call and the spawn injects it
- Discovery reads the effective set a tool evaluates before the file that configures it, because the effective set holds the implied values and files
- Reads of a generated or large file locate a declaration by the literal the code spells, then `Read` the range, and no step prints a whole tree
- Steps select an input by the line the run's own command printed or the path the prompt names, because a shared directory holds other sessions' work
- Measurements write under a private path (`mktemp -d <artifacts>/scratch-XXXXXX`) the procedure deletes, because another session builds between them
- Steps that build a solution derive the build target from the solution's project list, because a project outside the solution takes its own build
- Discovery over a language with a rule family is one scan of the family over the scope, and a criterion with no rule takes a probe in the rule form
- Discovery over a mixed scope runs once over one file of each kind, because a target reads the kinds its commands accept
- Skills every fork must hold load through `Skill` before the first fork, because a fork inherits the run and a fresh agent reads its brief alone
- Runs on a tree other agents edit record `git diff --numstat <commit> -- <scope>` before the first dispatch, because the start has no commit
- Steps chain into one call when a later call consumes the earlier output alone, and stay apart when a reading decides the next step
- Facts the harness returns under a tool result are read where they land, and facts the engine computes at spawn are no discovery step
- The last discovery step runs every gate command that names no file the run creates, in the whole-language form of each check, as the baseline
- Gates that read an artifact the run creates get it from the last discovery step, and the procedure starts on the artifact that step names
- Measured durations decide where a command runs, per cycle for seconds and the baseline with the gate for minutes

[SOURCES]:
- Sources rows hold the question, the source, and the exact call, ordered per question: the file on disk, `<tool> --help`, then the documentation
- Sources rows name a source the spawn supplies or the run retrieves, and the dispatcher's memory is neither
- Sources rows name an MCP tool as `mcp__<server>__<tool>` with the argument value that changes its answer, one row per reading, loaded at the call
- Sources rows for a result a hook filters or a limit truncates name the form that returns items, because a count alone is no baseline
- Sources rows for a check with a severity a repository setting raises name the list the raised form lands in, because the warning list prints empty
- Sources rows for an MCP search name the bounded text form and read the count from the tool's own header, because the JSON form lands in a file
- Sources rows for a call that returns past the token limit name the `jq` path that reads the file the result names
- Sources rows for a failure name the listing that locates the failing unit before the log that explains it, because a log over every unit costs each
- Tools that answered an error, an empty envelope, or a value the build contradicts leave the table, and the `decision` names each with its reading
- Sources rows that restate the tool routing of the root instructions leave, because that routing is present at spawn and a hook names the skill
- The sources table ends with its precedence sentence: the installed declaration, type, or binary decides over a page or a report

[DECISION]:
- Decision facts are the ones a run got wrong once, each with the right form beside it
- Every `decision` holds the evidence sentence: an empty scope is a valid result with the commands that proved it, and unseen output is no evidence
- Hook context lines under a tool result are readings the run records as decision facts beside the tool they change
- Decision lines name the scope and the cache of a cached checker, because a cached target answers from the cache when its `inputs` miss the scope

[PROCEDURE]:
- Procedure steps write paths from the repository root, because `cd` does not persist between `Bash` calls in a subagent
- Steps name a tool in its call form: the tool with its deciding argument, `mcp__<server>__<tool>`, the command line, or `nx run <project>:<target>`
- Steps spell a command in the form the allow list of `.claude/settings.json` grants, because another spelling of the binary prompts in default mode
- Each edit is one exact-string replacement with the result read, and a table one step alone uses sits under that step
- Tables more than one step names sit in their own element named for their content, between `decision` and `procedure`
- The procedure bounds its fix-and-prove cycles with a count, and the remainder goes under `open:` with its evidence
- Deletions name the path the tool printed, because a glob under a shared directory deletes a concurrent run's capture
- Probes that can print nothing for two reasons prove their positive case first, because an empty result over the scope is then evidence
- Proofs of an interactive program run under `expect -c`, waiting on a rendered marker before each `send`, because unpaced terminal input is lost
- The procedure ends with the deletion of every probe directory, temporary environment, and disposable input a proof wrote, then the gate

[GATE]:
- Gate lines pair the command with its result line: exit 0 and no output, `N passed; 0 failed`, a named line present in the output
- Gate commands over a formatter run the check form the lint target runs, or compare `git diff | shasum` hashes, because own edits fail `--exit-code`
- Gate lines that prove work happened read the artifact or the task record, never the duration, and a line matched as text names the plain form
- Gate lines for a scan or check with a silent clean result hold the coverage count or the line the tool prints when it runs, silence proving none
- Gate lines over a path name the missing-input line beside the exit code, because a tool that reports a missing input at exit 0 proves by absence
- Gate commands state no default and name their selector, because a restated default is a second place for the fact and a committed tree selects none
- Gate lines hold over the tree as it stands, proven by one run over the owned files, and a line the tree fails names the subset the run owns
- Gates on a shared tree read the scope diff past the recorded rows, then `git diff --name-only <commit> -- . ':(exclude)<path>'` per scope path
- Gate lines that bound a measured time hold no number three runs measured apart, and read the lines and report the maximum
- Proofs of a `claude` run name `--debug-file <path>` under an ignored directory with the transcript redirected beside it, read by `jq` and `rg`
- Skill tables a gate proves run as a rule family at `lint` where the language has a grammar, and the gate names `ast-grep scan` over the scope
- Cached targets prove a scope when their `inputs` hash it, read through `pnpm exec nx show project <project> --json | jq '.targets.<target>.inputs'`

[DONE_WHEN]:
- Done conditions are observable by a later reader: a file at a path, a line in an output, a form absent from a scope, a proof in the transcript
- Done conditions name the replaced form and the command with the empty output that proves its absence (`rg -n -F '<old>'`, a scan with no hit)
- Done conditions hold every gate result line in the transcript and no partial edit, deferred value, or workaround, and the empty gate is one of them

[OUTPUT]:
- The report names each row and its columns, and a report holds no narration
- Rows are `result:` from a closed set, `changes:`, `open:` with evidence and the fix, `sent:` with the confirmation, `gate:` with each result line
- The report grows during the run, because a run cut at `maxTurns` returns the report as it stands, marked partial
- Every finding, path, and proof line the dispatching agent acts on has a row, because the transcript stays unread
- Weaknesses in the agent or a preloaded skill go in a `suggestions:` row, and the run edits neither file
- Records a dispatcher asks for land through `Bash` with a heredoc, because the engine refuses a subagent `Write` of a report-shaped file
- `output` owns a named agent's report shape, a hook owns a `fork` or `general-purpose` return, and no skill, reference, or brief states rows

Agents of one family share the report rows, the message sentence, the baseline step, and the evidence sentence, and differ in the rest.

## [03]-[INTEGRATION]

Every step names its tool in the form the harness runs it, that form follows from where a tool's knowledge sits, references hold the proven forms per tool, and one principle here covers every tool of its category:

[CLAUDE_CODE_TOOLS]:
- Tools appear by name with the argument that decides the call, `Read` with `offset` and `limit` at a located line, `Edit` with one exact string
- Commands with outputs one reading consumes join one `Bash` call, a command with an output deciding the next step stays alone, and 30 KB makes a file
- The tool-call hook sets the `Bash` timeout of a long command and backgrounds the one form past the ceiling, and a step names the command alone
- `TaskStop` kills a background process before its own cleanup, and the step names the cleanup the agent runs afterward
- Language tooling renames a symbol and a fixed-string text search proves the old spelling absent, because syntax tools read no fixture or prose
- Dispatches appear in call form, `Agent(subagent_type: "<name>")` with the brief source named, and the child's report returns as result or notice

[MCP_SERVERS]:
- MCP tools appear as `mcp__<server>__<tool>` with the argument the server reads, because the harness, the allow list, and every hook use that name
- Tool search defers every schema and the call site loads it, and a discovery step that loads schemas ahead costs every schema and finds nothing
- Servers a role needs on every turn take `"alwaysLoad": true` on their `.mcp.json` entry, the repository setting for that cost
- The hook names the owning skill at a server's first call, and the agent holds no routing row and reads the skill it preloads
- Searches take the bounded text form with the count from the header, and a relative `project_folder` resolves to `<top>/<scope>` from the git top
- Exact filters (`project`) answer zero where the unfiltered call lists, and the unfiltered call is the first call
- Tools that fail under the installed release leave the sources table, and `decision` names the failure text with the form that replaces the call

[CLI_TOOLS]:
- Commands take the direct form the tool documents with the flags its `--help` prints, one argument per call where the tool takes one
- Flags the help omits stay out of the step even when the binary accepts them, because the next release drops them in silence
- Prefixes and flags around a tool default (`NO_COLOR=1`, `--color never`, `-tl:off`, `timeout N`, `mise exec`) leave the step
- Defaults the repository owns are fixed at their source, `mise.toml` `[env]`, the target, the tool's configuration file, or a hook rewrite row
- Steps derive a selection from the shape that owns it (`fd ... -x basename {} .yml | paste -sd'|' -`) and read one field through the printing query
- Files the run edits or judges line by line read whole, and an outline beside that read is a second read of the same file
- Values a run derives come from the declaring file through the evaluating tool (`dotnet msbuild Directory.Build.props -getProperty:<name>`)
- Paths a tool resolves outside the shell are absolute, and every command names its path from the repository root
- Binaries resolve through `mise which <name>` one name per call or `command -v` for a package binary, and every step assumes the newest release

[NX_TARGETS]:
- Targets run as `pnpm exec nx run <project>:<target> -- <scope>` from the allow list, the tree form for the tree, and `run-many` per language
- Checkers over a source scope run as `pnpm exec nx affected -t <target> --files=<path>[,<path>]`, because the graph names the owning project
- Cached targets prove a scope when their `inputs` hash it, and a target exists when `nx show project <project> --json | jq '.targets|keys'` lists it
- Fix loops name the file command of each kind in scope where a target runs the tree, and the target proves the whole at the gate

[SCRIPTS]:
- Repository scripts run through their target with the arguments, and `--help` through that target is the usage source
- Skill scripts run by their path with a subcommand, refuse input outside their domain at exit 1 naming the domain, and the step states the domain
- Disposable files sit under `.cache/<agent-name>/`, owned in the table and deleted at the close, and a scoping proof sits at the path the glob names
- Rule families land with an id absent from every `ruleDirs` entry, `severity: error` under `rules/`, a test and snapshot, and the whole test run

[HOOKS]:
- Refusals name the form to run, rewrites name what ran in a context line under the result, and a step reads no policy source ahead of a call
- Refusals an agent reads are steps it narrates, and a refusal over a state the run needs becomes an answer row that returns the state, with no retry
- Sentences a hook appends to every spawn of the agent's type leave the file, and a brief template holds the task alone
- Preload proofs read the `Preloaded skill '<name>'` line per `skills` entry, and a `hook failed` line in that file is a finding for the hook builder

## [04]-[MISTAKES]

Build a section from the mistakes runs showed, and an agent holds no anti-pattern table, because each row is a positive rule in its section:
1. List every mistake seen in transcripts, review findings, and reports as one line with the run that showed it
2. Cluster the lines by category: judgment, ordering, discovery, proof, scope, repetition
3. Move each judgment mistake to the skill as a criterion
4. Write each other mistake as its positive form in the section where the agent meets it
5. Delete the mistake list when every line landed, and the sections hold it

| [INDEX] | [CATEGORY] | [SECTION]                               |
| :-----: | :--------- | :-------------------------------------- |
|  [01]   | Discovery  | `context_gathering` step                |
|  [02]   | Proof      | `gate` line                             |
|  [03]   | Scope      | `role` sentence                         |
|  [04]   | Ordering   | `procedure` step                        |
|  [05]   | Repetition | Rule, script, or target the agents name |

## [05]-[EXCLUSIONS]

Sentences that state what another file owns leave the agent:
- Sentences a preloaded skill states, and the agent names the skill in `skills` alone
- Sentences a hook appends to every spawn of the agent's type, present in the prompt at spawn
- `Skill` calls of a skill the `skills` list names, injected whole at spawn, its references read as `Load <skill>, read <reference>` with a reading
- Delegation judgment (who forks, who starts fresh, who reviews, and why), owned by the dispatching skill, and the agent names the call and its brief
- Discovery steps that map a file the next step reads whole, because the read holds the map
- Sources rows that restate the tool routing of the root instructions, present at spawn
- Soft bounds (`relevant`, `as needed`, `when the task permits`, `with judgment`), replaced by the condition, the command, or the criterion
- Repository, product, and path names in a generic skill, and an agent file names its own paths

## [06]-[CHECKS]

Every check runs before the description is written:
- `ast-grep scan --no-ignore hidden <agent>` prints no hit, and `typos <agent>` prints no line
- `rg -n --pcre2 '^(- |[0-9]+\. |\| +)?(A|An) [a-z`]' <agent>` prints nothing, because an entry or sentence opens with its subject
- `awk '{n=split($0,s,/[.:] /); for(i=1;i<=n;i++){t=tolower(" " s[i] " "); c=gsub(/ the /,"&",t); d=gsub(/ and /,"&",t); if(c>2||d>2) print FNR": the="c" and="d}}' <agent>` lists the sentences to judge, at most two `the` and two `and` each
- `rg -n 'relevant|as needed|when the task permits|testing scope|with judgment|appropriate' <agent>` prints nothing
- `awk '/^<context_gathering>$/,/^<\/context_gathering>$/' <agent> | rg -n ToolSearch` and `rg -n 'rows? [0-9]' <agent>` print nothing, a load step and a row number drift
- Every path the file names exists: `rg -o '[A-Za-z0-9_./-]+\.(md|sh|yml|ts|py|json)' <agent> | sort -u` against `ls`
- Every agent the file names exists as a file under an agents directory, and every `subagent_type` value sits in the spawn's agent type list
- Every MCP tool the file names loads, `rg -o 'mcp__[a-z-]+__[a-z_]+' <agent> | sort -u`, each through `ToolSearch(query: "select:<name>")`
- Every target the file names exists, `pnpm exec nx show project <project> --json | jq '.targets|keys'` prints it
- Every `skills` entry exists as a skill directory, because a missing one is skipped with a warning in the debug log alone
- Every command a step or gate holds ran from the repository root in the session that wrote it, over a real and a missing input, with its line kept
- Every cached gate target ran once over a planted defect, and every route ran its first tool on a symptom produced without a repository edit
- `claude plugin validate <agents dir>` reports no parse error for a tree agent and `claude plugin validate <plugin>` for a plugin agent
- `git diff <commit> -- <agent>` on a rebuilt file, and every command, row, and threshold the earlier revision held returns or the report names it
- `claude --debug-file <path> -p 'Use the <name> agent with this exact prompt: "Return result: not started. Run no other tool."'` prints one `Preloaded skill` line per entry and no `hook failed` line
- The running session loads the first file of a new agents directory after a restart and a later edit within seconds, and a `-p` run starts fresh

## [07]-[DESCRIPTION]

The description is written last, from a full read of the finished file, its preloaded skills, and the router file that lists agents. Skills and subagents share one form, `Use when <situation>, covering <topics>`, one sentence of at most 25 words on a `description:` line of at most 160 columns, with no `when_to_use` field. Descriptions name the tool, language, or file kind a dispatch prompt holds, state in the situation clause what the router line lacks, and name the section subjects in the covering list, a procedure step named there drifts with the next edit. Every subagent description loads in the dispatching session at every start, and the harness warns when the set passes 15,000 tokens:
1. Write 3 candidates and grade each against the form, with the failure named per candidate
2. Start one fresh agent with the same brief and files to write and grade its own 3 candidates and grade yours
3. Implement the candidate both readings rank first
4. When the top picks differ, start a third fresh agent that reads the file and both candidate sets and picks from its own reading
