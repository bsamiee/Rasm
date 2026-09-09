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
- `Bash`: results past 30,000 characters go to `~/.claude/projects/<session>/tool-results/<id>.txt` with a preview
- `Bash`: a result file reads through `jq` or `Read` with `offset`
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

| [INDEX] | [FIELD]           | [VALUES]                                                  | [SETTING]                                               |
| :-----: | :---------------- | :-------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `name`            | Lowercase and hyphens, no `:`, unique across tree         | Required, hooks receive it as `agent_type`              |
|  [02]   | `description`     | Sentence loaded at session start                          | Required                                                |
|  [03]   | `tools`           | Allowlist of tool names or `mcp__<server>` patterns       | Absent, inherits every tool, a list keeps `SendMessage` |
|  [04]   | `disallowedTools` | Denylist applied before `tools`                           | `Write, Edit` for a role that edits nothing, MCP kept   |
|  [05]   | `model`           | `sonnet`, `opus`, `haiku`, `fable`, a model ID, `inherit` | Absent, `Agent` `model` argument or session decides     |
|  [06]   | `permissionMode`  | `default`, `acceptEdits`, `auto`, `dontAsk`, `plan`       | Absent, ignored for plugin agents                       |
|  [07]   | `maxTurns`        | Turn cap, output returns marked partial with the ID       | Absent, a resume is a fresh follow-up agent             |
|  [08]   | `skills`          | Skills injected whole at spawn                            | Preloaded with no `Skill` call                          |
|  [09]   | `mcpServers`      | Server names or inline definitions                        | Absent, ignored for plugin agents, `.mcp.json` declares |
|  [10]   | `hooks`           | Lifecycle hooks scoped to the subagent                    | Absent, ignored for plugin agents, plugin hooks apply   |
|  [11]   | `memory`          | `user`, `project`, `local`                                | Absent, directory unignored, unindexed by memory editor |
|  [12]   | `background`      | `true` keeps the subagent in the background               | Absent                                                  |
|  [13]   | `effort`          | `low`, `medium`, `high`, `xhigh`, `max`                   | Absent, inherits the session                            |
|  [14]   | `isolation`       | `worktree`, from the default branch                       | Absent until a run needs a scope free of sibling edits  |
|  [15]   | `color`           | Named color                                               | One per agent                                           |

Loading facts:
- `model` resolves in order: `Agent` `model` argument, `model` field, `CLAUDE_CODE_SUBAGENT_MODEL`, session model
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
- Hook injects `Load the <skill> skill` at the first call of `roslyn-codelens`, `binlog`, `nuget`, `ast-grep`, `context7`, `deepwiki`, and `hostinger`
- Roslyn server trusts `Workspace.slnx` per session, `list_solutions` prints `isActive: true` for it
- `load_solution` follows a `list_solutions` row without `isActive: true`
- Results past the limit go to a file the reply names, `jq` over it reads the envelope (`binlog_task_details`)
- `get_code_fixes` answers `Internal` naming `System.Composition.AttributedModel`, the diagnostic item replaces it

## [05]-[HOOKS]

`function-hooks` answers every tool call with a refusal naming the correct form, a rewrite, or a context line under the result:

| [INDEX] | [KIND]            | [FORMS]                                                                                                            |
| :-----: | :---------------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | Tool refusal      | `WebSearch`, `WebFetch`, `browser_run_code_unsafe`, each naming the tools in its place                             |
|  [02]   | Binlog refusal    | `.binlog` through `Read`, `Edit`, `Write`, `cat`, `head`, `tail`, `less`, `strings`, `xxd`, or `od`                |
|  [03]   | Hostinger refusal | `VPS_recreate*`, `VPS_restore*`, `VPS_delete*` before `VPS_createSnapshotV1` on the machine in the session         |
|  [04]   | Hostinger refusal | `DNS_reset*` before `DNS_getDNSRecordsV1` on the domain, a purchase with a target the prompt does not name         |
|  [05]   | Git refusal       | Interpreter before `git` or a script, `-c alias.`, `config alias.`, a command past 128 KiB                         |
|  [06]   | Git refusal       | `branch -d`, `-D`, `-M`, `--force`, `switch -f`, `checkout` with a discarding flag or a pathspec                   |
|  [07]   | Git refusal       | `reset --hard`, `--merge`, `--keep`, `<commit>`, `restore` without `--staged`, `stash` past `list` and `show`      |
|  [08]   | Git refusal       | `clean`, `rebase`, `revert`, `reflog delete`, `drop`, `expire`, `push --force`, `--delete`, `--mirror`, `--prune`  |
|  [09]   | Rewrite           | `mise exec -- <cmd>` and `eval "$(mise env)" && <cmd>` run as `<cmd>`, `npm` runs as the configured manager        |
|  [10]   | Rewrite           | `NX_DAEMON=<value>` drops, `uv add <pkg>==<v>` and `dotnet add package <pkg> --version <v>` drop the pin           |
|  [11]   | Rewrite           | `pnpm add <pkg>@<v>` runs at `@catalog:`, `timeout <n> <cmd>` runs as `<cmd>` with `<n>` on the `Bash` parameter   |
|  [12]   | Rewrite           | `nx run rasm:workflow` runs with `run_in_background: true`, top-level `sleep` beside a command drops               |
|  [13]   | Silent rewrite    | `nx run`, `dotnet build` or `test`, `ast-grep test`, `claude -p`, `act`, `uv sync`, `pnpm install` at 600000 ms    |
|  [14]   | Line once         | `grep`, `rg` over `.cs`, MSBuild, `.ts`, or `.py` files, `ls -R`, `find`, `wc -l`, `dotnet build` without `-bl`    |
|  [15]   | Line once         | `gh issue`, `gh run rerun`, `gh pr merge`, a preview flag, `sleep` alone or inside a compound command              |
|  [16]   | Line once         | `mise exec` with no command or an option before it, `eval "$(mise env)"` alone                                     |
|  [17]   | Skill line once   | `.cs`, `.csproj`, `.props`, `.targets`, `<Target>` in an MSBuild file, `PackageReference` in any file              |
|  [18]   | Skill line once   | `Directory.Packages.props`, `NuGet.config`, `.slnx`, `.env*`, `doppler.yaml`, first call of a routed MCP server    |
|  [19]   | Skill line once   | `eng/`, `infra/`, `tools/`, `.github/`, `mise.toml`, `mise.unix.toml`, `.miserc.toml`, `nx.json`                   |
|  [20]   | Docs line once    | `.claude/` paths name `mcp__claudeCodeDocs__search_claude_code_docs`                                               |
|  [21]   | Line              | `op://` in a `Bash` redirect or heredoc, or added by a write outside `.claude/`, names Doppler as the one source   |
|  [22]   | Manifest line     | `Directory.Packages.props`, `package.json`, `pnpm-workspace.yaml`, `pyproject.toml` added row names its README row |
|  [23]   | Manifest line     | Dropped row names the sibling records that keep the name, `Version=` names `mcp__nuget__get_package_context`       |
|  [24]   | Manifest line     | Pinned tool in `mise.toml` names its reason comment, pin in `pyproject.toml` or `package.json` names the lock file |
|  [25]   | Description line  | Server's owning skill prepended when a schema loads, `WebSearch` and `WebFetch` carry their replacements           |
|  [26]   | Prompt            | Secret values become their ids at `prompt.submit` with a line, `tool.call` restores them in the `Bash` command     |
|  [27]   | Record            | Successful `VPS_createSnapshotV1` and `DNS_getDNSRecordsV1` stamp their target id for the hostinger refusals       |

Facts a step depends on:
- Rewrites run first, refusals and lines read the rewritten command
- Git refusals end `destructive git actions are refused`, `git show HEAD:<file> > <file>` restores a file
- Classifier denials end `You *may* attempt to accomplish this action using other means`
- `mise exec` and `eval` lines state that the shell holds the mise environment, the `SessionStart` hook wrote it to `CLAUDE_ENV_FILE`
- `timeout` over the whole command keeps the smaller of its duration and the parameter, a partial prefix the larger, 600000 ms caps both
- Sleep lines name `run_in_background: true` or an `until` loop under `Monitor` as the wait forms
- Preview flag lines name the command without the flag as the proof
- Once lines key on a skill or tool name per session, a skill the session loaded raises no line
- Lines join the tool result as context and the call's notice
