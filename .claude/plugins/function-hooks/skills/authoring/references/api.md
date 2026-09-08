# [API]

`.claude/types/claude-code.d.ts` declares every event input and result, every `$` noun and method, and every element a surface draws. `claude-code-mcp.d.ts` beside it declares the connected servers' tool inputs, and `e.tool === 'mcp__<server>__<tool>'` narrows `e`.

## [01]-[EVENTS]

| [INDEX] | [EVENT]            | [USE_WHEN]                                                       |
| :-----: | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `*`                | Facts every dispatch shares, `next.event` before any `$` call    |
|  [02]   | `engine.create`    | Noun added to or withheld from `$`, and none replaced            |
|  [03]   | `session.start`    | Work that outlives a dispatch, a tool listed by turn one         |
|  [04]   | `prompt.submit`    | The user's text rewritten or dropped, under `-p` too             |
|  [05]   | `prompt.section`   | System-prompt section replaced or left out, cached               |
|  [06]   | `prompt.context`   | Block on the first message, cached until a re-read or invalidate |
|  [07]   | `tool.describe`    | What the model reads for a tool, cached                          |
|  [08]   | `turn.start`       | `turnId` held for `$.turn.abort`                                 |
|  [09]   | `tool.call`        | Call refused, rewritten, annotated, answered, or recorded        |
|  [10]   | `agent.offer`      | Agent type withheld from the listing and from dispatch           |
|  [11]   | `agent.spawn`      | Subagent's prompt, model, or directory changed, or refused       |
|  [12]   | `skill.prompt`     | Skill's text replaced, or its load recorded                      |
|  [13]   | `attribution.text` | Commit or pull request text the model reads                      |
|  [14]   | `turn.step`        | Each model response observed with its tool calls                 |
|  [15]   | `turn.complete`    | Answer acted on, a line beneath it, no hold on the turn          |
|  [16]   | `ui.render`        | Component drawn, wrapped, or its props rewritten                 |
|  [17]   | `ui.resolve`       | Element restyled or withheld for the plugins beneath             |
|  [18]   | `ui.press`         | Element intercepted, `ui.input` and `ui.select` alike            |
|  [19]   | `PreToolUse`       | Settings hooks alone, a plugin hooks `tool.call`                 |

- Event files answer the declared union alone
- `next(e)` on `tool.call` resolves to core's record with `ref`, `result`, and `text`, and a record returned as received is used verbatim
- `tool.call` answers are validated against the tool's result shape: `BuiltinToolResults[<tool>]` for a built-in, `McpToolResult` blocks for a server tool
- `turn.complete` resolves after `next(e)`, and its arms run beside the returned result
- `agent.spawn` fires before the subagent runs, and `$.agent.spawn` resolves with `text` once it ran
- Subagent Bash calls raise `tool.call` in the one module with `command, description, tool, tool_use_id` and no agent id (`ToolCallInput`)
- `next.origin` is `engine` for a model's call (`Next` in `claude-code.d.ts`), and `e.origin` exists on `prompt.submit` alone
- `turn.start`, `turn.step`, and `turn.complete` fire for the main turn alone, a subagent's Bash appears in no `turn.step` line of the debug file
- `next({ ...e, run_in_background: true })` on a Bash call runs it in the background, `BuiltinToolResults.Bash.backgroundTaskId` names the task

## [02]-[ENGINE_INTERFACE]

| [INDEX] | [NOUN]      | [USE_WHEN]                                 | [FACT]                                                                   |
| :-----: | :---------- | :----------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `$.ui`      | Line without a turn, a redraw, a question  | `notice` draws under an open call alone, `status` is one line per plugin |
|  [02]   | `$.model`   | Judgment over text, the small model        | `complete` has no history, `fork` reads the session's own transcript     |
|  [03]   | `$.audio`   | Speech or a clip beside the answer         | `speak` runs the platform synthesizer and rejects without one            |
|  [04]   | `$.mcp`     | Server tool inside a rule's facts          | `call` needs no permission prompt, the plugin's call is the grant        |
|  [05]   | `$.session` | Session facts as data                      | `messages()` answers 4096 rows at most, `surface()` is `null` under `-p` |
|  [06]   | `$.turn`    | Running turn cancelled                     | `abort` takes the `turnId` of `turn.start` and rejects on any other      |
|  [07]   | `$.prompt`  | Prompt handed to an idle session           | `submit` runs `prompt.submit` under origin `{ kind: 'plugin', name }`    |
|  [08]   | `$.tool`    | A call of the plugin's own                 | `call` runs the permission check and the tool                            |
|  [09]   | `$.agent`   | Subagent seen through its resolved text    | `spawn` runs under the plugin's origin and answers `text` or `deny`      |
|  [10]   | `$.fs`      | File under the working directory           | Reads reach the temp directory, writes stay under the working directory  |
|  [11]   | `$.store`   | Fact that outlives a call                  | One JSON file per plugin under `~/.claude/plugins/store/`                |
|  [12]   | `$.clock`   | Stamp, a wait, or work on a schedule       | `now` is milliseconds since the epoch                                    |
|  [13]   | `$.http`    | URL fetched through the host               | `fetch` answers `{ status, ok, headers, text }` once the body is read    |
|  [14]   | `$.process` | Host command, a path `$.fs` cannot reach   | argv, no shell, `stdin` text, 30 s default, 10 min cap, session `env`    |
|  [15]   | `$.plugin`  | Plugin's own name or directory             | `name` and `root` are the `$` reads the loader accepts                   |

- `$.model.complete` answers 256 tokens by default, and `$.model.fork` answers `null` on a cold snapshot or an API error
- `$.model.classify` answers one label, `undefined` when the reply names none, and rejects on a failed request
- `$.session.messages()` rows are `{ role, text, toolUses }`, and `$.session.repo()` is `null` outside a repository
- `$.fs.readFile` of a missing file rejects and logs an engine error, `$.fs.exists` gates each read
- Home files go through `$.process.run`, which reads the whole output and runs git with repository hooks off
- `$.tool.call` runs the permission check and the tool
- `$.ui.ask` resolves to the chosen label or the text typed under Other, and rejects when the dialog is dismissed

## [03]-[CACHED_ANSWERS]

`ui.render`, `prompt.section`, `prompt.context`, and `tool.describe` are answered once per input for the session, and `$.ui.invalidate(event)` drops the answer:
- Renders redraw at most ten times a second, and a redraw follows a store write that changes what the band shows
- Invalidations of `prompt.context` or `prompt.section` take effect on the next turn, and the block is a store snapshot at that moment
- Describe rows read the tables alone, and the description cache holds with no invalidation
- Policy files that own rows a cached answer reads list the answers (`FINDING_VIEWS`), and every writer maps `$.ui.invalidate` over the list
- Cached answers read the tables or a store snapshot alone, an unstable answer spends the model's prompt cache on every call
- `Date`, `Math.random`, `crypto.randomUUID`, `performance.now`, `$.clock.now`, and the session count stay outside a cached answer

## [04]-[LOADER]

The loader reads `register.ts` and its relative imports at load into an environment of web globals and element tags, the `claude-code` import empty at run time:
- Globals: `URL`, `URLSearchParams`, `TextEncoder`, `TextDecoder`, `atob`, `btoa`, `structuredClone`, `AbortController`, `AbortSignal`
- Globals: `crypto.subtle.digest`, `crypto.randomUUID`, `crypto.getRandomValues`, `performance.now`
- Tags: `h`, `Fragment`, `Box`, `Text`, `Button`, `Input`, `Select`, `Link`, and the JSX namespace
- `lib` names no DOM, the DOM `Text` shadows the element
- Matchers are objects of the event's fields

## [05]-[STORE]

`$.store` is kept across sessions and reloads:
- `get`, `set`, `delete`, and `keys`, with no compare-and-set and no transaction, and parallel writers take distinct keys
- `keys()` answers every key in insertion order, the last key under a namespace is its newest, and a namespace prefix with a filter is the query
- Values are JSON data

## [06]-[WAITS]

`$.clock.sleep(ms, { signal: next.signal })` ends a wait with its dispatch, and a hook's budget bounds the rest. `$.clock.every` and `$.clock.after` run in the plugin's own environment, and a reload drops them with the old environment.

## [07]-[SPAWNS]

`$.agent.spawn(input)` runs a subagent under the plugin's own origin for its whole life:
- Subagent calls reach other plugins' hooks and skip the plugin's own
- Calls resolve with `{ model, text }` once the subagent ran, `isError` set when it failed, or `{ deny }` for a refused spawn
- `text` is the plugin's one view of a run, its final message, or why it failed under `isError`
- Prompts hold every fact the agent cannot fetch (rows, batch id, scope)
- `$.agent.spawn` answers no agent id, and `$.tool.call({ tool: 'Agent', ... })` answers `{ agentId }` as `$.agent.list()` names it
- `$.session.id()` and `$.session.cwd()` inside a subagent's call answer the main session's id and root
- `$.agent.list()` names running agents as `{ id, description, type, status }` (`AgentInfo`), no name and no link to a call
- `agent.spawn` holds `name` (undefined when the caller set none) and its `tool_use_id` (`AgentSpawnInput`)
