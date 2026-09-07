# [IDEATION]

Each hook makes one move on one event, the parts of the plugin compose the moves into one hook, and the questions settle a new design before its row is written.

## [01]-[CATEGORIES]

| [INDEX] | [MOVE]                   | [EVENT]                                        | [FORM]                                                 |
| :-----: | :----------------------- | :--------------------------------------------- | :----------------------------------------------------- |
|  [01]   | Refuse                   | `tool.call`, `prompt.submit`, `agent.spawn`    | Row with `deny`, the reason naming the correct form    |
|  [02]   | Rewrite                  | `tool.call`, `prompt.submit`, `skill.prompt`   | Row with `rewrite` and a `context` line, by span       |
|  [03]   | Add context              | `tool.call`, `prompt.submit`, `prompt.context` | Row with `context`, `once` per session or `each` call  |
|  [04]   | Answer for the engine    | `tool.describe`, `prompt.section`              | Rule returning `answer`, a function of the tables      |
|  [05]   | Draw                     | `ui.render`                                    | Matched hook on one component, `$.ui.resolve(e)`       |
|  [06]   | Record state             | Any event, after `next`                        | One `$.store.set` per row, decoded by its reader       |
|  [07]   | Serve a tool             | `session.start`, `tool.call`                   | `$.tool.register` and a matched hook, the handler pure |
|  [08]   | Spawn and brief an agent | `session.start`, `agent.spawn`                 | `$.agent.spawn` from a timer, the brief from `AGENTS`  |
|  [09]   | Classify                 | `turn.complete`                                | `$.model.classify` the kind, `$.model.fork` the fields |
|  [10]   | Speak                    | `turn.complete`                                | `$.model.complete` for one line, `$.audio.speak`       |
|  [11]   | Schedule                 | `session.start`                                | `$.clock.every`, a tick that reads the store and holds |

## [02]-[POTENTIALITIES]

The declarations open moves no row uses yet, each a row or an arm away:
- `$.ui.ask(question, options)` lets a rule consult the person
- `prompt.section` with a matcher on `name` drops or replaces one section of the system prompt for the session
- `tool.describe` rows over more built-ins prepend the routing line the model reads before its first call
- `$.mcp.call(server, tool, args)` answers a server's tool inside a rule's facts, a NuGet version checked before a manifest edit lands
- `$.http.fetch(url)` reads a page through the host, a release note compared with a pinned version
- `attribution.text` with a matcher on `kind` sets the commit trailer or the pull request footer the model writes
- `turn.start` holds the `turnId` and `$.turn.abort` ends a turn that a later hook judges wrong
- `engine.create` withholds a noun from the plugins beneath, and the model's own tools keep their reach
- `ui.render` on `ToolUse`, `ToolGroup`, or `AssistantMessage` redraws a row from its props, and `ui.resolve` restyles an element
- `Input` and `Select` elements in the band take typed text and picks through `ui.input` and `ui.select`
- `$.prompt.submit({ text })` wakes an idle session with a prompt under the plugin's name, a finding turned into a task
- `$.session.messages()` reads the transcript for a rule over the whole session, a repeated refusal counted
- `of` on `$.fs.ancestors` walks the instruction chain down to one file, a nested `CLAUDE.md` applied per path
- `agent.offer` withholds an agent type from the model in a session with no use for it
- `$.ui.toast(text)` shows a line under the prompt for a few seconds, a fact that needs no turn and no band
- `$.audio.play(clip)` plays a plugin asset, a URL, or bytes beside speech
- `$.tool.call(input)` runs a tool under the plugin's own id through the permission check, and `$.tool.list()` reads what the model can call
- `$.agent.list()` names every subagent of the session with its status
- `$.fs.listDir`, `$.fs.stat`, and `$.fs.writeFile` read a neighbor's size or write a file under the working directory
- `$.clock.after(ms, fn)` runs one tick once, a deferred check started in `session.start`
- `$.session.turnCount()`, `model()`, and `cwd()` answer the session as data for a rule that counts or routes by them
- Matchers on `prompt.submit` `origin.kind` (`task-notification`, `peer`, `plugin`) read a report or a peer's message before the model does
- Arms on `tool.call` over `ReportFindings` or an `Agent` result's `content[].text` turn a review or a report into findings
- Matchers on `ui.press` over another plugin's `plugin` and `element` intercept its button
- The `Link` element draws a URL in the band, and `$.plugin.root` locates a plugin asset

## [03]-[COMPOSITION]

Complex hooks compose the parts, one adapter, one fold, and the store as the one state between events:
- Store read feeding a fold: the adapter decodes `injected/`, `snapshot/`, and `prompt/` rows into a facts record the rules take as an argument
- Rewrite feeding a guard: restoration puts the secret value back before the git guard reads the command
- Recording feeding a requirement: a successful snapshot call stamps `snapshot/<session>/<vm>`, and the hostinger family requires the stamp
- Timer feeding a spawn: the tick batches the open findings by kind, or a due part, under `dispatch/<batchId>` and spawns the editor
- Spawn feeding a close: the editor's final message is the close input, decoded through `decodeJson` and `decodeClose` into the handler
- Classifier feeding a served tool: `turn.complete` writes `findings/<id>`, `close` lists and lands the rows
- Skill load feeding a stop: `skill.prompt` stamps `loaded/<session>/<skill>`, and the once lines that route to the skill stop
- Session fact feeding a measurement: `session.start` writes the guidance directories, and the markdown row measures edits under them

## [04]-[QUESTIONS]

Settle each before the row is written, in order:
1. Which event holds the fact at the moment the decision is needed, and whether it fires under `-p`
2. What `$` must supply beyond `e`, read in the hook body and handed to the rule as an argument
3. Whether the move is a deny (the correct form named), a rewrite (the context line), a context line (`once` or `each`), or an answer
4. Which table holds the row, and whether the row type needs a field every consumer of the table then reads
5. Which proof shows it: the spec over a literal event, the `--plugin-dir --debug -p` run, and the debug line it must print
6. Whether the answer is cached, and which writer invalidates it
7. Whether a person acts on what a surface shows, and which surface: the notice under the call, the band, the status line, or none
7. Whether the work outlives the dispatch, and then whether it starts in `session.start`
8. Whether the call reaches the plugin's own hooks, and whether a spawned agent's calls reach a served tool
