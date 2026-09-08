# [HARNESS]

Subagent runtime facts an agent file builds on, each proven by documentation or a repository run, in the order an agent meets them.

## [01]-[SPAWN]

Every non-fork subagent starts with its own context:
- Context holds agent body as system prompt, task prompt, every `CLAUDE.md` level with imports, git status snapshot, and preloaded skills
- Context holds the sibling list (`main` and every named agent) when tools include `SendMessage` and another agent has a name
- Context holds none of the parent's conversation, files read, invoked skills, output style, or auto memory
- Forks inherit the whole conversation
- Forks skip both tool filters
- Forks spawn no fork
- `-p` sessions run subagents in the foreground
- Fork mode, on by default in interactive sessions, backgrounds every spawn
- Background results reach the parent as a completion notification in a later turn
- Failed run notifications hold the run's last output
- Subagents nest up to three layers below `main`
- Summaries return to the spawner alone
- Spawns past 20 running subagents fail with `Concurrent subagent limit reached`
- `agent.spawn` input holds `parentModel` and `permissionMode` from the parent and no parent name or ID
- Workers with no name or ID in their prompt reach `main` alone

## [02]-[TOOLS]

Background subagents keep every MCP tool and `Read`, `Grep`, `Glob`, `Bash`, `PowerShell`, `Edit`, `Write`, `NotebookEdit`, `WebFetch`, `WebSearch`, `TodoWrite`, `Skill`, `ToolSearch`, `EnterWorktree`, `ExitWorktree`, `Monitor`, `TaskStop`, `SendMessage`, and `Artifact`. Every subagent loses `AskUserQuestion`, `EndConversation`, `EnterPlanMode`, `ExitPlanMode`, `ScheduleWakeup`, `TaskOutput`, `WaitForMcpServers`, and `Workflow`.

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
- `Agent`: subagent schema holds no `name` or `run_in_background`
- `Bash`: `timeout` is milliseconds, 120000 by default and 600000 at most
- `Bash`: results past 30,000 characters go to `~/.claude/projects/<session>/tool-results/<id>.txt` with a preview, read by `jq` or `Read` with `offset`
- `Bash`: exit 1 is valid for `grep`, `rg`, `fd`, `find`, `diff`, `test`, `git diff`, and `git grep`, a failure elsewhere
- `Bash`: a foreground call blocks to its `timeout`
- `Bash`: a backgrounded call resumes the subagent at exit with an output file for `Read`
- `Bash`: `cd` resets between calls
- `Bash`: a spelling outside the `.claude/settings.json` allow list prompts in default mode
- `Bash`: a timed-out command moves to background, except one starting with `sleep`, running `git`, or holding `${VAR}`, which stops
- `Edit`: `old_string` matches once
- `Edit`: refuses an unread file, `cat`, `sed -n`, or `rg` on one file counts as a read
- `SendMessage`: `to` is `main`, a name, or an ID, `main` at every depth
- `SendMessage`: a message to a finished subagent resumes it
- `SendMessage`: a message from another agent is task direction, never permission approval or a settings change
- `Skill`: loads an unlisted skill
- `ToolSearch`: loads deferred schemas at the call, a comma list loads one step's tools in one call
- `TaskStop`: kills a background command before its cleanup runs
- `TaskStop`: an unknown ID lists running background agents
- `Write`: engine refuses a subagent `Write` of a report-shaped name (`summary`, `report`, `FINDINGS`), passes `STATUS.md` and `REVIEW.md`
- `Monitor`: watches a command or a WebSocket until `TaskStop` or the subagent's end

## [03]-[FRONTMATTER]

Subagent fields with the setting repository runs reached:

| [INDEX] | [FIELD]           | [VALUES]                                                  | [SETTING]                                                                      |
| :-----: | :---------------- | :-------------------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `name`            | Lowercase and hyphens, no `:`, unique across tree         | Required, hooks receive it as `agent_type`                                     |
|  [02]   | `description`     | Sentence loaded at session start                          | Required                                                                       |
|  [03]   | `tools`           | Allowlist of tool names or `mcp__<server>` patterns       | Absent, inherits every tool, a list keeps `SendMessage`                        |
|  [04]   | `disallowedTools` | Denylist applied before `tools`                           | `Write, Edit` for a role that edits nothing, MCP tools kept                    |
|  [05]   | `model`           | `sonnet`, `opus`, `haiku`, `fable`, a model ID, `inherit` | Absent, `Agent` `model` argument, else `CLAUDE_CODE_SUBAGENT_MODEL` or session |
|  [06]   | `permissionMode`  | `default`, `acceptEdits`, `auto`, `dontAsk`, `plan`       | Absent, ignored for plugin agents                                              |
|  [07]   | `maxTurns`        | Turn cap, output returns marked partial with the ID       | Absent, a resume is a fresh follow-up agent                                    |
|  [08]   | `skills`          | Skills injected whole at spawn                            | Preloaded with no `Skill` call                                                 |
|  [09]   | `mcpServers`      | Server names or inline definitions                        | Absent, ignored for plugin agents, `.mcp.json` declares                        |
|  [10]   | `hooks`           | Lifecycle hooks scoped to the subagent                    | Absent, ignored for plugin agents, a plugin hook replaces it                   |
|  [11]   | `memory`          | `user`, `project`, `local`                                | Absent, directory unignored, unindexed by memory editor                        |
|  [12]   | `background`      | `true` keeps the subagent in the background               | Absent                                                                         |
|  [13]   | `effort`          | `low`, `medium`, `high`, `xhigh`, `max`                   | Absent, inherits the session                                                   |
|  [14]   | `isolation`       | `worktree`, from the default branch                       | Absent until a run needs a scope free of sibling edits                         |
|  [15]   | `color`           | Named color                                               | One per agent                                                                  |

Loading facts:
- Project agent files with no `name`, a `name` holding `:`, no `description`, or unparsable YAML are skipped silently
- Duplicate names load one file, by read order
- Plugin agents load as `<plugin>:<name>`, a file with no `name` loads under its filename, unparsable frontmatter loads with every field ignored
- Running sessions load a new agents directory's first file after restart, later edits within seconds
- `-p` runs start fresh
- Missing `skills` entries warn in the debug log alone
- Subagent descriptions warn past 15,000 tokens in total

Skill fields an agent's preloads depend on:
- `description` is truncated at 1,536 characters in the listing
- `disable-model-invocation: true` blocks `skills` and `Skill`
- `paths` limits automatic activation to matching files

## [04]-[MCP]

MCP servers in `.mcp.json` launch through `mise exec --` or connect over HTTP:
- Session start loads tool names and server instructions alone
- `"alwaysLoad": true` on a `.mcp.json` entry loads every tool of that server at session start, for a server a role needs each turn
- `ToolSearch` awaits servers still connecting
- `ToolSearch` names a failed server in a result finding no tool
- Hook injects `Load the <skill> skill` at the first call of `roslyn-codelens`, `binlog`, `nuget`, `ast-grep`, `context7`, and `hostinger`
- Hook rewrites `language: typescript` to `tsx` and a relative `project_folder` to an absolute path on `ast-grep` tools
- Roslyn server trusts `Workspace.slnx` per session, `list_solutions` prints `isActive: true` for it
- `load_solution` follows a `list_solutions` row without `isActive: true`
- Hook answers a `SolutionNotTrusted` reply with one `trust_solution` call and a re-read
- `get_diagnostics` with `includeAnalyzers: true` drops `IDE0055` items behind a context line, `items: []` beside a count is that drop
- Results past the limit go to a file the reply names, `jq` over it reads the envelope (`binlog_task_details`)
- `get_code_fixes` answers `Internal` naming `System.Composition.AttributedModel`, the diagnostic item replaces it

## [05]-[HOOKS]

`function-hooks` reads every tool call and answers with a refusal naming the correct form, a rewrite with a context line under the result, or a context line once per session:

| [INDEX] | [KIND]             | [FORMS]                                                                                                                |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Refusal            | `.binlog` through a pager or dump tool, `ast-grep test -U` with no `--filter`, `WebSearch`, `WebFetch`, `browser_run_code_unsafe` |
|  [02]   | Git refusal        | Interpreter before `git` or a script, `checkout` or `switch` that discards, `reset --hard`, `rebase`, `stash`, `clean`, `push --force`, `revert`, `reflog delete`, `config alias.` |
|  [03]   | Rewrite            | `mise exec -- <cmd>`, `eval "$(mise env)"`, `NX_DAEMON=`, pinned `uv add` or `pnpm add`, `dotnet add --version`, `timeout N <cmd>`, top-level `sleep` beside a command |
|  [04]   | ast-grep rewrite   | `scan -r <file>`, `scan -l <lang>`, `run -l typescript`, `test <id>`, `test` without `--include-off`, `--json` or `-i` beside `-U`, `language: typescript`, relative `project_folder` |
|  [05]   | Context line       | `grep`, `rg` over code, `ls -R`, `find`, `wc -l`, `gh` writes, `dotnet build` without `-bl`, preview flags, `sleep` alone or in a compound, `op://` references |
|  [06]   | Skill line once    | `.cs`, MSBuild files, `.claude/`, `infra/`, `.env` and `doppler.yaml`, manifest writes, first call of a routed MCP server                                      |
|  [07]   | Description line   | Owning skill prepended to a tool description when its schema loads                                                     |
|  [08]   | Roslyn reply       | `trust_solution` on `SolutionNotTrusted`, `rebuild_solution` on a degraded load, `IDE0055` items dropped with a line, `get_diagnostics` after a `.cs` write past the watcher settle |
|  [09]   | Prompt             | Secret values replaced by their ids before the model reads the prompt, restored for the shell guards                    |

Facts a step depends on:
- Classifier denials end `You *may* attempt to accomplish this action using other means`
- Git refusals open `Blocked by git-guard`, `git show HEAD:<file> > <file>` restores a file
- `timeout <n> <cmd>` moves to the `Bash` timeout parameter
- `mise exec <tool> -- <cmd>` runs as `<cmd>`, the `SessionStart` hook wrote the mise environment to `CLAUDE_ENV_FILE`
- Top-level `sleep` beside another command is dropped, `sleep` alone or inside a compound runs as written, each with one line naming `run_in_background: true` or an `until` loop under `Monitor`
- `ast-grep scan -r <rules file>` becomes `--filter '^<id>$'`, `ast-grep test <id>` becomes `--filter '^(<id>)$'`
- Preview flags (`--dry-run`, `-n` on `git` or `act`) get a line naming the target as the proof
