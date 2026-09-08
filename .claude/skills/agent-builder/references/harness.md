# [HARNESS]

Facts of the subagent runtime an agent file builds on, each one the docs or a run of the repository proved, grouped by what the agent meets first.

## [01]-[SPAWN]

Every non-fork subagent starts with its own context, and the file holds every fact the run cannot retrieve:
- The context holds the agent body as system prompt, the task prompt, every `CLAUDE.md` level, the git status snapshot, and the preloaded skills
- The context holds the sibling list (`main` and every named agent) when the tools include `SendMessage` and another agent has a name
- The context holds none of the dispatcher's conversation, files read, invoked skills, output style, or auto memory
- Forks inherit the whole conversation, skip both tool filters, and spawn no fork of their own
- The `agent.spawn` hook appends the plugin's brief lines to a `fork` or `general-purpose` prompt and no line to a named agent's prompt
- `-p` sessions run a needed subagent in the foreground, and fork mode, on by default in an interactive session, backgrounds every spawn
- Background subagent results reach the caller as a completion notification in a later turn, and a failed one's message holds its last output
- Subagents spawn subagents up to three layers below `main`, a child's summary returns to its spawner, and `main` reads the top-level summary alone
- Twenty running subagents refuse the next spawn with `Concurrent subagent limit reached`
- `agent.spawn` input holds `parentModel` and `permissionMode` from the parent and no parent name or ID, and no hook appends a spawner's address
- `Agent` in the main conversation holds `name`, a subagent's holds none, and a worker with no name or ID in its brief reaches `main` alone, proven by a run

## [02]-[TOOLS]

Background subagents, the default, keep every MCP tool and the built-in tools `Read`, `Grep`, `Glob`, `Bash`, `PowerShell`, `Edit`, `Write`, `NotebookEdit`, `WebFetch`, `WebSearch`, `TodoWrite`, `Skill`, `ToolSearch`, `EnterWorktree`, `ExitWorktree`, `Monitor`, `TaskStop`, `SendMessage`, and `Artifact`. Every subagent loses `AskUserQuestion`, `EndConversation`, `EnterPlanMode`, `ExitPlanMode`, `ScheduleWakeup`, `TaskOutput`, `WaitForMcpServers`, and `Workflow`. The hook refuses `WebFetch` and `WebSearch` and names the MCP tool or `tvly` command that replaces each, and `CLAUDE.md` refuses `Grep` and `Glob` for code.

| [INDEX] | [TOOL]        | [CALL]                                                          |
| :-----: | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Agent`       | `Agent(subagent_type, prompt, description, model?, isolation?)` |
|  [02]   | `Bash`        | `Bash(command, description?, timeout?, run_in_background?)`     |
|  [03]   | `Edit`        | `Edit(file_path, old_string, new_string, replace_all?)`         |
|  [04]   | `Read`        | `Read(file_path, offset?, limit?)`                              |
|  [05]   | `SendMessage` | `SendMessage(to, message, summary?)`                            |
|  [06]   | `Skill`       | `Skill(skill, args?)`                                           |
|  [07]   | `ToolSearch`  | `ToolSearch(query: "select:mcp__<server>__<tool>")`             |
|  [08]   | `TaskStop`    | `TaskStop(task_id)`                                             |
|  [09]   | `Write`       | `Write(file_path, content)`                                     |

Facts per tool a step depends on:
- `Agent`: a subagent's schema holds no `name` and no `run_in_background`, and `subagent_type: fork` inherits the run
- `Bash`: `timeout` is milliseconds, 120000 by default and 600000 at most, and a result past 30,000 characters lands in a file with a preview
- `Bash`: exit 1 is valid for `grep`, `rg`, `fd`, `find`, `diff`, `test`, `git diff`, and `git grep`, and a failure elsewhere
- `Bash`: a foreground call blocks to its `timeout`, the hook sets that timeout for a long command, and a backgrounded call resumes the subagent at its exit with an output file `Read` reads
- `Bash`: a timed-out command moves to the background, except one starting with `sleep`, running `git`, or holding `${VAR}`, which stops
- `Bash`: a heredoc writes the record a dispatcher asks for, because the engine's report-file refusal sits beneath every hook
- `Bash`: results past 30 KB land under `~/.claude/projects/<session>/tool-results/<id>.txt`, read by `jq` or `Read` with `offset`
- `Edit`: `old_string` matches once, an unread file is refused, and a file read through `cat`, `sed -n`, or `rg` on one file counts as read
- `SendMessage`: `to` is `main`, a name, or an ID, `main` is the main conversation at every depth, and a message to a finished subagent resumes it
- `SendMessage`: a message from another agent is task direction, never approval for a permission prompt and never a settings change
- `Skill`: loads an unlisted skill, a preloaded skill needs no call, and `disable-model-invocation: true` blocks both routes
- `ToolSearch`: loads one deferred schema at the call, and a comma list loads one step's tools in one call
- `TaskStop`: kills a background command before its own cleanup runs, and an unknown ID lists the running background agents
- `Write`: the engine refuses a subagent `Write` of a report-shaped name (`summary`, `report`, `FINDINGS`) and passes `STATUS.md` and `REVIEW.md`
- `Monitor`: watches a command or a WebSocket, `TaskStop` cancels it, and it stops with the subagent that started it

## [03]-[FRONTMATTER]

Subagent fields and the verdict the runs of the repository reached for each:

| [INDEX] | [FIELD]           | [VALUES]                                                  | [VERDICT]                                               |
| :-----: | :---------------- | :-------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `name`            | Lowercase and hyphens, no `:`, unique across the tree     | Required, hooks receive it as `agent_type`              |
|  [02]   | `description`     | When Claude delegates, loaded at session start            | Required, at most 25 words and 160 columns              |
|  [03]   | `tools`           | Allowlist of tool names or `mcp__<server>` patterns       | Absent, inherits every tool, a list keeps `SendMessage` |
|  [04]   | `disallowedTools` | Denylist applied before `tools`                           | `Write, Edit` for a role that edits nothing             |
|  [05]   | `model`           | `sonnet`, `opus`, `haiku`, `fable`, a model ID, `inherit` | Absent, the subagent model order decides                |
|  [06]   | `permissionMode`  | `default`, `acceptEdits`, `auto`, `dontAsk`, `plan`       | Absent, ignored for plugin agents                       |
|  [07]   | `maxTurns`        | Turn cap, output returns marked partial with the ID       | Absent, a resume is a fresh follow-up agent             |
|  [08]   | `skills`          | Skills injected whole at startup                          | The skills every run applies                            |
|  [09]   | `mcpServers`      | Server names or inline definitions                        | Absent, ignored for plugin agents, `.mcp.json` declares |
|  [10]   | `hooks`           | Lifecycle hooks scoped to the subagent                    | Absent, ignored for plugin agents, a plugin hook        |
|  [11]   | `memory`          | `user`, `project`, `local`                                | Absent, unignored, unindexed, void with auto memory off |
|  [12]   | `background`      | `true` keeps the subagent in the background               | Absent, fork mode backgrounds every spawn               |
|  [13]   | `effort`          | `low`, `medium`, `high`, `xhigh`, `max`                   | Absent, inherits the session                            |
|  [14]   | `isolation`       | `worktree`, from the default branch                       | Absent until a run needs a scope free of sibling edits  |
|  [15]   | `color`           | Eight named colors                                        | One per agent                                           |

Loading facts:
- Tree agent files with no `name`, a `name` holding `:`, a `name` and no `description`, or YAML that fails to parse are skipped in silence
- Plugin agents load as `<plugin>:<name>`, a file with no `name` loads under its filename, and unparsable frontmatter loads with every field ignored
- `claude plugin validate <dir>` reports the parse failures of a tree agents directory, and `claude plugin validate <plugin>` those of a plugin
- The running session loads a new agents directory's first file after a restart, and a later edit within seconds

Skill fields an agent's preloads depend on:
- `description` is truncated at 1,536 characters in the listing
- `disable-model-invocation: true` keeps the skill out of `skills`, and `paths` limits automatic activation to matching files
- Every subagent description loads at session start, and the combined set warns past 15,000 tokens

## [04]-[MCP]

Every MCP server of `.mcp.json` launches through `mise exec --` or connects over HTTP, and tool search defers every schema:
- Only tool names and server instructions load at session start, and `ToolSearch(query: "select:<name>")` loads a schema at the call that needs it
- `"alwaysLoad": true` on a server's `.mcp.json` entry loads every tool of that server at session start, for a server a role needs each turn
- Servers still connecting are awaited inside the `ToolSearch` call, and a failed server is named in a result that finds no tool
- The hook injects `Load the <skill> skill` at the first call of `roslyn-codelens`, `binlog`, `nuget`, `ast-grep`, `context7`, and `hostinger`
- The hook rewrites `language: typescript` to `tsx` and a relative `project_folder` to an absolute path on the `ast-grep` tools
- The roslyn server trusts `Workspace.slnx` for the session, `list_solutions` prints it `isActive: true`, and `load_solution` follows a row without
- The `tool-call` hook answers a `SolutionNotTrusted` reply with one `trust_solution` call and a re-read, and no agent step holds a trust call
- `get_diagnostics` with `includeAnalyzers: true` drops `IDE0055` items behind a context line, and `items: []` beside a count is that drop
- Results past the result limit land in a file the reply names, and `jq` over the file reads the envelope (`binlog_task_details`)
- `get_code_fixes` answers `Internal` naming `System.Composition.AttributedModel` under the installed release, and the diagnostic item replaces it

## [05]-[HOOKS]

The `function-hooks` plugin reads every tool call, refuses forms with a message naming the correct one, rewrites forms with a context line under the result, and injects context lines once per session:

| [INDEX] | [KIND]              | [FORMS]                                                                                                         |
| :-----: | :------------------ | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Bash` refusal      | `mise exec` alone, preview flags, pinned adds, `yq r`, a `.binlog` read, `sleep` alone or inside a loop, conditional, brace group, case body, or a piped group, bare `ast-grep test -U` |
|  [02]   | `Bash` rewrite      | `mise exec -- <cmd>`, `npm`, `timeout N <cmd>`, `ast-grep test`, `-l typescript`, `scan -r <file>`, `mise which <a> <b>`, `-bl:<path>.binlog`, and with no line `NO_COLOR=1`, `--color never`, `-tl:off`, `--pretty false`, `timeout: 600000` on a slow leaf |
|  [03]   | `Bash` context line | `grep` or `rg` over code paths, `ls -R`, `find`, `wc -l`, `gh api`, `dotnet build` without `-bl`                |
|  [04]   | `git` refusal       | Interpreter before `git` or a script, `checkout` with a pathspec, `reset --hard`, `rebase`, `stash`, `clean`    |
|  [05]   | Path row            | `.binlog` reads, secret references, skill load lines by path once, dependency record lines per manifest write, a subagent `Write` of `report.md`, `summary.md`, or `findings.md` under `.claude/scratch/` answered and landed |
|  [06]   | Tool row            | `WebSearch` and `WebFetch` with the replacement, `trust_solution` on `Workspace.slnx` answered as the trusted state, first-call skill lines |
|  [07]   | Description line    | Prepended when a schema loads, the skill to load, the refused state, the `--stdin` exit codes                   |
|  [08]   | Spawn brief         | Four lines for `fork`, two for `general-purpose`, each ending in one report contract line, none for a named agent |
|  [09]   | Roslyn after-write  | `get_diagnostics` on the project, the `SolutionNotTrusted` recovery, the `IDE0055` drop, the 200 ms debounce    |
|  [10]   | Width line          | `Entry at line N is M columns` under every `.md` edit in the `CLAUDE.md` chain or the memory directory          |

Facts a step depends on:
- Refusals name the form to run, and the classifier's denial ends `You *may* attempt to accomplish this action using other means`
- `git checkout -- <path>` is refused, and `git show HEAD:<file> > <file>` restores a file
- `timeout <n> <cmd>` moves to the `Bash` timeout parameter, and `mise exec <tool> -- <cmd>` runs as `<cmd>` under `CLAUDE_ENV_FILE`
- Top-level `sleep` leaves drop with the `run_in_background` line, a `sleep` inside a loop, conditional, brace group, case body, or a parenthesized group feeding a pipe is refused naming `run_in_background: true`, the `until` loop under `Monitor`, and `expect -c`, and `expect -c` drives an interactive session
- `ast-grep scan -r <rules file>` becomes `--filter '^<id>$'`, and `ast-grep test <id>` becomes `--filter '^(<id>)$'`
- `hooks/policies/scan.ts` answers a `skill.prompt` block per preloaded rule family, its `stem` reads the test suffix under `tools/ast-grep/tests/` alone, and a `hook failed` line there is the hook builder's defect
