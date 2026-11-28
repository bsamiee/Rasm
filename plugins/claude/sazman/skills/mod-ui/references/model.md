# [ASKS_AND_CHANNELS]

Everything a mod draws reaches the person alone, and the model reads only hook answers and the messages a mod sends. Template `templates/ask.ts` holds one decision asked before a tool call, its refusal text, and a tree above the dialog.

## [01]-[ASK]

`ask(question)` from `@rasm/claude-code/ask` opens the engine's AskUserQuestion dialog and answers the plugin alone:
- `question` is the dialog's text, `header` its chip of at most 12 characters, `options` a tuple of 2 to 4 `{ label, description?, preview? }`, `multiSelect` optional
- `Answer` is a tagged enum folded by `Answer.$match`: `Picked` (`labels`, `other`, `notes`), `Other` (typed text alone), `Noted` (notes alone), `Declined`
- `Effect.timeoutOption(ask(...), <duration>)` takes the dialog down when its moment passed, as interruption aborts the call
- `-p` runs have no dialog, and `ask` answers `Declined` there, as for a dismissal
- Asks run from a hook, a press, or a forked fiber alike, as each calls the engine through `Engine.callTool`; `$.tool.call` of AskUserQuestion is refused
- Answers reach the model only through a channel the hook then uses: a `deny` naming what the person chose, or `next(e)` running the call
- `render.AskUserQuestion` decorates the dialog with the evidence (`Box[evidence, next()]`), in at most 12 rows on the terminal and 48 elsewhere
- Every plugin's and the model's dialogs raise `AskUserQuestion`, so the row gates on the mod's own state

## [02]-[NOTICES]

`Engine.deliver(notice)` sends one `Notice` case, each picked by who reads it, how long it lasts, and whether it interrupts:

| [INDEX] | [CASE]         | [READER] | [LIFETIME]                          | [USE_WHEN]                                    |
| :-----: | :------------- | :------- | :---------------------------------- | :-------------------------------------------- |
|  [01]   | `Toast`        | Person   | 4 s, hover holds, click removes     | Outcome nobody acts on                        |
|  [02]   | `Notification` | Person   | OS banner until `text: none` by key | Ask or alarm for a person who may be away     |
|  [03]   | `Band`         | Person   | Until the group's rows go           | Row the person presses, one notification each |
|  [04]   | `Note`         | Model    | Next request of its loop            | Fact for an agent, busy or idle               |
|  [05]   | `Message`      | Model    | Starts a turn when idle             | Work an agent must start now                  |

- `Notification` with `text: Option.none()` retracts by key, and a send that finds no channel logs `notification <key> not sent: <reason>`; under Zellij the `auto` channel finds none
- `Note` appends a user row the model reads on its loop's next request and the person never sees as typed
- `Message` to a busy agent becomes a `Note`, to an idle main a submitted prompt, to an idle subagent a `session.send`; delivered means queued, never read
- Toasts outside fullscreen draw as one notification-bar line, and a shown `holdToasts` pane queues them undrawn

## [03]-[MODEL_CHANNELS]

Hook answers the model reads, each with what the screen shows of it:

| [INDEX] | [CHANNEL]                      | [SCREEN]                              | [MODEL_READS]                              |
| :-----: | :----------------------------- | :------------------------------------ | :----------------------------------------- |
|  [01]   | `tool.call` `{ deny }`         | Errored result row with the deny text | Tool error result                          |
|  [02]   | `tool.call` `{ result }`       | Drawn as the tool's own result        | Tool result                                |
|  [03]   | `context` on a result          | Nothing                               | Text after the output                      |
|  [04]   | `tool.check` reason            | Dialog text on an ask                 | Reason on a deny                           |
|  [05]   | `prompt.context` blocks        | Nothing                               | Blocks beside the prompt until invalidated |
|  [06]   | `prompt.section`               | Nothing                               | System prompt section                      |
|  [07]   | `prompt.attachment`            | No drawn row                          | Engine reminder, rewritten or dropped      |
|  [08]   | `command.run` `{ text }`       | Output row                            | Its text, unless `isTextDisplayOnly`       |
|  [09]   | `command.run` `{ context }`    | Nothing                               | Hidden user messages after the output      |
|  [10]   | `tool.describe`, `agent.offer` | Nothing                               | Tool descriptions, agent listing           |

- Rewrites of transcript rows (`UserMessage`, `AssistantMessage`, `ToolResult`, `CommandOutput`) change the drawing alone, and the stored row stays what the model reads
- `context` on a plugin's own `Engine.callTool` answer is dropped, so a tool body passes context it needs on in its own answer
- `prompt.context` blocks stay until `Engine.invalidate('prompt.context')`, compaction, or `/clear`, and an invalidate asks every hook again, so it runs only when the facts changed
- Command tables for the person answer `{ text, context, isTextDisplayOnly: true }`: the table draws and stays out of requests, and `context` carries the facts as report lines

## [04]-[TEXT]

Program text an agent reads (`deny`, `context`, notes, tool results) is report lines:
- Subject line first, then lowercase `label: value` facts, one fact per line, the action that answers it first
- Refusals read subject, reason, values, and the form that works: `WebFetch <url>: refused, the person skipped it`
- Teaching notes go once per agent and edits once per call, as each line stays in history and costs tokens on every later request
- Person-only records stay out of model channels: a toast, a placed row, or the debug log
- Faults go to the debug log through `Effect.logWarning`, which the library's logger writes as `$.ui.log` with `{ to: 'debug' }`
