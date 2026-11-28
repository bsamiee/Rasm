# [EVENTS_TOOLS_AGENTS]

Hooks, declared tools, agent types, and slash commands share one `host` call and one runtime. Template `templates/runner-tool.ts` holds a model-callable tool that runs the mod's own agent type.

## [01]-[HOOKS]

`hooks` maps an engine event to `(e, next) => Effect` over the mod's services, and `next(e)` runs the plugins beneath and the engine:
- Keys with no hook register no `on()` line, so an unhooked event costs the engine nothing
- `next({ ...e, field })` rewrites what the rest of the chain sees, and an answer without `next` answers for the chain
- `tool.call` hears the model's and other plugins' calls; calls the mod raised itself pass by, so an `ask` inside a `tool.call` hook never reenters it
- `ui.press`, `ui.input`, and `ui.select` hear every plugin's controls to route or audit them, and a mod's own controls run their element handlers without them
- `classic.Stop` and `classic.SubagentStop` hooks pass every later stop of a chain once `stop_hook_active`, and a block already made for one agent and text passes on the second try
- `turn.step` takes a `Channel` of the engine's chunks ending in the step's result, the one streaming event
- Every hook failure logs `<event>:<tool> hook failed` to the debug log and passes the event on, and an Escape-interrupted hook settles with the abort reason
- `host(on, mod, ...parts)` merges further `{ layer, hooks }` parts after the mod's own, each a table of its own in chain order (policy enforcement joins this way)

## [02]-[BUDGETS]

Hooks run under engine budgets and inside one shared worker:
- Bodies waiting on a timer die at the 10 s hook budget, `tool.run` included, while bodies waiting on an engine call (a spawned child, an agent) run whole
- Synchronous stretches past about 5 s wedge the worker, so long work is an engine call or a forked fiber
- Runtimes start at `session.start` and end at `session.end`, and `/clear` and `--resume` rebuild one while the process stays, so layers hold no state that must outlive the session
- Module variables start over on every reload of the mod, and state that must survive goes in the plugin store (`KeyValueStore`)

## [03]-[TOOLS]

`tools` declares model-callable tools, each `Tool(schema, { description, isDeferred, run })` from `@rasm/claude-code/events`:
- JSON schemas derive from the Effect `Schema`, and `run(input, e, next)` gets the decoded input
- Tools take the name `mcp__<plugin>__<key>` at registration; `Plugin.tools` maps each key to that name for rows and agents that read it
- `isDeferred: false` puts the schema in the prompt's tool list, and absent it waits behind ToolSearch
- Failed runs answer `{ deny: '<key>: <reason>' }` and log the cause
- Registration runs as the runtime builds at `session.start`, and the hook matches the declared names alone, as an unmatched `tool.run` hook slows every tool

## [04]-[AGENTS]

`agents` declares agent types, each the engine's agent definition less `name` plus `isOffered`:
- Type names read `<plugin>:<key>`, held in `Plugin.agents`
- `isOffered: false` hides the type from the model's agent listing while the mod's own start still runs it
- `Engine.callTool({ tool: 'Agent', input: { subagent_type, description, prompt } })` starts it in the background and answers at once with its id, and its report reaches the model as a task notice when it ends
- Auto mode's check classifies a plugin's tool and each `Agent` call, so a `tool.check` hook allows the mod's own read-only tool and agent type
- Subagent events reach every hook, the mod's included, so a hook that starts an agent on an event the agent also raises checks `e.agentId`

## [05]-[COMMANDS]

`commands` declares slash commands, each the engine's command spec less `name` plus `run`:
- Commands run as `/<key>`, and `Plugin.commands` maps each key to the registered name `CommandOutput` rows carry
- `immediate: true` runs it while a turn streams, so a pane opener answers at once
- `run` answers `{}` when it drew elsewhere, `{ text }` for an output row, and `{ text, context, isTextDisplayOnly: true }` for a table the model reads as facts alone
- Commands are the person's input, so a pane one opens places at any width and takes `focus`

## [06]-[SERVICES]

Every layer and handler reads the library's services and the platform services built on the engine:
- `Engine`: `deliver`, `invalidate`, `focus`, `scroll`, `fetch`, `callTool`, `cwd`, `root`, `agents`, `limits`, `messages`, `isRunning`, `turns`
- `Panes`, `Placed`, `Theme`, `Draft`: the sites of their references
- `Plugin`: the plugin's name and the registered names of its tools, agent types, and commands
- `FileSystem`, `Path`, `ChildProcessSpawner`, `KeyValueStore`, `ConfigProvider`: Effect's platform services over the engine's calls, one argument vector per spawn and no shell
- `Watch` from `@rasm/claude-code/watch` opens a read-only viewer pane in a named Zellij session for each tmux server a wrapped call starts
- Service keys follow the deterministic form the Effect language service checks: `@rasm/<package>/<path>/<Name>`
