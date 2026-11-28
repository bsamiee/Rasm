# [EVENTS_TOOLS_AGENTS]

Hooks, declared tools, agent types, and slash commands share one `host` call and one runtime. Template `templates/runner-tool.ts` holds a model-callable tool that starts the mod's own agent type and follows it to its report.

## [01]-[HOOKS]

`hooks` maps an engine event to `(e, next) => Effect` over the mod's services, and `next(e)` runs the plugins beneath and the engine:
- Keys with no hook register no `on()` line, so an unhooked event costs the engine nothing
- `next({ ...e, field })` rewrites what the rest of the chain sees, and an answer without `next` answers for the chain
- `tool.call` and `agent.register` hear the model's and other plugins' calls; calls the mod raised itself pass by, so an `ask` inside a `tool.call` hook never reenters it
- `ui.press`, `ui.input`, and `ui.select` hear every plugin's controls to route or audit them, and a mod's own controls run their element handlers without them
- `classic.Stop` and `classic.SubagentStop` hooks pass every later stop of a chain once `stop_hook_active`, and a block already made for one agent and text passes on the second try
- `turn.step` takes a `Channel` of the engine's chunks ending in the step's result, the one streaming event, and `reply(e, { answer, toolUses })` answers a step with no model request
- Hook failures log `<event>[:<tool>] hook failed: throw|timeout[: <message>]` to the debug log and pass the event on, and an Escape-interrupted hook settles with the abort reason, logging nothing

## [02]-[PARTS]

`host(on, mod, ...parts)` chains `{ layer, hooks }` parts a library ships after the mod's own table:
- Tables chain in order, the mod's outermost, so the mod's `next(e)` reaches each part's hook before the engine
- Part layers merge with the mod's layer into one runtime, and part hooks take every hosted event but `turn.step`
- `enforce(policy)` from `@rasm/claude-code/enforcement` is a part: it judges tool calls and their output by a `@rasm/policy` policy, its refusals and notes in report lines

## [03]-[BUDGETS]

Hooks run under engine budgets and inside one shared worker:
- Each dispatch has 10 s of the hook's own time, and the clock stops while `next(e)` or an engine call is in flight
- Bodies waiting on an Effect timer die at the 10 s budget, `tool.run` included
- Bodies waiting on an engine call (a spawned child, an agent, an ask) run whole
- Synchronous stretches past about 5 s wedge the worker, so long work is an engine call or a forked fiber
- Runtimes build at the session's first hosted event and end at `session.end`, so layers hold no state that must outlive the session
- `/clear` and `--resume` end a runtime while the process stays, and the next event builds a new one
- Module variables start over on every reload of the mod, and state that must survive goes in the plugin store (`KeyValueStore`)

## [04]-[TOOLS]

`tools` declares model-callable tools, each `Tool(schema, { description, isDeferred, run })` from `@rasm/claude-code/events`:
- JSON schemas derive from the Effect `Schema`, and `run(input, e, next)` gets the decoded input and the call (`e.tool_use_id`, `e.agentId`)
- Tools take the name `mcp__<plugin>__<key>` at registration, drawn as `<plugin> - <key> (MCP)`; `Plugin.tools` maps each key to that name for rows and agents that read it
- `isDeferred: false` puts the schema in the prompt's tool list, and absent it waits behind ToolSearch
- Failed runs answer `{ deny: '<key>: <reason>' }` and log `message="tool failed"` with the `tool` key and cause
- Registration runs as the runtime builds, and the hook matches the declared names alone, as an unmatched `tool.run` hook slows every tool

## [05]-[AGENTS]

`agents` declares agent types, each the engine's agent definition less `name` plus `isOffered`:
- Type names read `<plugin>:<key>`, held in `Plugin.agents`
- `isOffered: false` hides the type from the model's agent listing while the mod's own start still runs it
- `Engine.callTool({ tool: 'Agent', input: { subagent_type, description, prompt } })` starts the agent in the background, answering `result.agentId` at once
- Engines tell the calling loop when the agent ends, and an `agent.report` hook reads its `status` and `text`
- Auto mode judges a mod's own tool and `Agent` call like the model's, so a `tool.check` hook allows them, or the call reads `Not run`
- Subagent events reach every hook, the mod's included, so a hook that starts an agent on an event the agent also raises checks `e.agentId`

## [06]-[COMMANDS]

`commands` declares slash commands, each the engine's command spec less `name` plus `run`:
- Commands run as `/<key>`, and `Plugin.commands` maps each key to the registered name `CommandOutput` rows carry
- `immediate: true` runs it while a turn streams, so a pane opener answers at once
- `run` answers `{}` when it drew elsewhere, `{ text }` for an output row, and `{ text, context, isTextDisplayOnly: true }` for a table the model reads as facts alone
- Failed runs answer `{ text: '<key>: <reason>', isTextDisplayOnly: true }` and log `message="command failed"` with the `command` key
- Commands are the person's input, so a pane one opens places at any width and takes `focus`

## [07]-[SERVICES]

Every layer and handler reads the library's services and the platform services built on the engine:
- `Engine`: `deliver`, `invalidate`, `focus`, `scroll`, `fetch`, `callTool`, `cwd`, `root`, `repo`, `agents`, `limits`, `messages`, `isRunning`, `turns`
- `Engine.agents` keys the session's live agents by id, `limits` reads agent bounds, `messages(agent)` a conversation, `repo` the repository
- `Panes`, `Placed`, `Theme`, `Draft`: the sites of their references
- `Plugin`: the plugin's name and the registered names of its tools, agent types, and commands
- `FileSystem`, `Path`, `ChildProcessSpawner`, `KeyValueStore`, `ConfigProvider`: Effect's platform services over the engine's calls, one argument vector per spawn and no shell, each handle's `pid` 0
- `Session` from `@rasm/claude-code/session`: `shell(agent)` answers the agent's shell dialect, folder, an anchored `Path`, and `exec(script)`
- `Session.exec` runs one parsed command as the Bash tool's shell would, its handle holding the program's pid, and needs the plugin's `hooks/main.wasm`
- `Peer.list` from `@rasm/claude-code/peers` lists the repository's other local Claude sessions (`claude agents --json`), the session itself left out, and fails with the listing's exit or decode error
- `Watch` from `@rasm/claude-code/watch` opens a read-only viewer pane in a named Zellij session for each tmux server a wrapped call starts
- Service keys follow the deterministic form the Effect language service checks: `@rasm/<package>/<path>/<Name>`
