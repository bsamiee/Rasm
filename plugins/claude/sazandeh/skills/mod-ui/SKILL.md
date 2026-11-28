---
name: mod-ui
description: "Use when building, changing, or reviewing a Claude Code mod's interface on @rasm/claude-code, covering where each site draws, who reads it, how rows, panes, asks, tools, and themes compose, templates, and the preview of every surface"
---

# [SAZANDEH_MOD_UI]

Mods are hooks modules the engine loads from a plugin, written as Effect programs on `@rasm/claude-code`: `host(on, mod, ...parts)` takes data and Effects, and the library lowers them through the engine's `$`, which a mod never sees. The engine's own `plugin-authoring` skill holds the raw `$` API, its JSX examples, and colors; translate its examples to `Element` values and library services before using one.

Owner rules bind every mod this skill builds:
- Every engine call goes through `@rasm/claude-code`, and a mod holds no `$`
- Faults go to the debug log through `Effect.logWarning`, never a toast, status line, or transcript row
- No plugin status line: no `StatusLine` row of the mod's facts and no `$.ui.status`
- Band Buttons take no `hotkey`
- Program text an agent reads (`deny`, `context`, notes, tool results) is report lines

[REFERENCES]:
- [01]-[PANES](references/panes.md): Panes and panels, their placement, keys, drawing, and closing
- [02]-[PROMPT](references/prompt.md): Band rows, cells, footer rows, and the composer around the prompt
- [03]-[TRANSCRIPT](references/transcript.md): Engine rows of the transcript and placed rows
- [04]-[MODEL](references/model.md): Asks, notices, and the channels the model reads
- [05]-[THEMES](references/themes.md): Themes, backdrop scenes, and underlays
- [06]-[EVENTS](references/events.md): Hooks, parts, budgets, declared tools, agent types, and services
- [07]-[PREVIEW](references/preview.md): Building a scratch mount and reading the preview of every surface

[TEMPLATES]:
- [01]-[PANE](templates/pane.ts): List pane with summary, sections, empty state, and widget glance, and a dialog form, opened from a band row
- [02]-[ABOVE_PROMPT](templates/above-prompt.ts): Band row whose press opens a pane scrolled to its row, `+N` overflow, and a hint tail
- [03]-[ASK](templates/ask.ts): One decision before a tool call, its refusal text, and evidence above the dialog
- [04]-[TRANSCRIPT](templates/transcript.ts): Call-row line, turn total, and fenced report table
- [05]-[RUNNER_TOOL](templates/runner-tool.ts): Model-callable tool running the mod's own hidden agent type

[SCRIPTS]:
- [01]-[PREVIEW](scripts/preview.py): Validates a built mod, paints it in both terminal layouts, mounts its sites on every surface, and reports

[ASSETS]:
- [01]-[MOD_UI](assets/mod-ui.html): Wireframes, mockups, and tables of every site, state, and reader, standalone in light and dark
- [02]-[PAGES](assets/pages/): One PNG per figure of `mod-ui.html`, named by figure id

## [01]-[PLACEMENT]

Sites around a fullscreen terminal session; the main screen seats a pane inline above the band and raises no `BesidePrompt` or `Underlay`:

```text
┌ transcript ─────────────────────────────────────┬ dock ────────────────┐
│ Welcome, InfoNotice                     toasts  │ Pane (from 110 cols) │
│ UserMessage                                     │   tab row: title     │
│ AssistantMessage, CodeBlock                     │   body: draw         │
│ ToolUse  ⎿ ToolResult   ToolGroup  ToolProgress │   window: scroll     │
│ CommandOutput                                   │                      │
│ TurnDuration                                    │                      │
│ Placed (row at the transcript's end)            │                      │
│ Spinner                                         │                      │
│ AskUserQuestion tree over the engine's dialog   │                      │
│ AbovePrompt band: one row per plugin       [-]  │                      │
│ ❯ composer (Draft)                 BesidePrompt │                      │
│ StatusLine, AgentRow                            │                      │
│ PromptHint · tail                  SessionMode  │                      │
└ Underlay beneath the whole window ──────────────┴──────────────────────┘
desktop, mobile: Backdrop scene behind the conversation
desktop: ContextMenu on right-click, Pane as a widget
```

Surfaces raise different sites, each component's declaration naming where, and a site a surface never raises needs a second home there:

| [INDEX] | [SITE]                                                                             | [RAISED_ON]                  |
| :-----: | :--------------------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `AbovePrompt`, `AbovePromptList`, `Pane`, `Placed`, `AskUserQuestion`              | Every surface                |
|  [02]   | `ToolUse`, `ToolResult`, `ToolGroup`, `CommandOutput`, `AgentRow`                  | Every surface                |
|  [03]   | `UserMessage`, `AssistantMessage`                                                  | Every surface                |
|  [04]   | `PromptHint`                                                                       | Terminal, desktop, mobile    |
|  [05]   | `Spinner`                                                                          | Terminal, desktop, VS Code   |
|  [06]   | `SessionMode`                                                                      | Terminal, desktop            |
|  [07]   | `BesidePrompt`, `BesidePromptList`                                                 | Terminal fullscreen, desktop |
|  [08]   | `CodeBlock`, `TurnDuration`, `ToolProgress`, `Welcome`, `InfoNotice`, `StatusLine` | Terminal                     |
|  [09]   | `Underlay`, `UnderlayList`                                                         | Terminal fullscreen          |
|  [10]   | `ContextMenu`                                                                      | Desktop                      |
|  [11]   | `Backdrop`                                                                         | Desktop, mobile              |

## [02]-[READERS]

Every site draws for the person alone, and the model reads a fact only when the mod also writes it to a model channel:
- Person: render rows, panes, placed rows, the band, toasts, notifications, themes, sounds
- Model: `tool.call` `deny` and `result`, `context` after a result, `prompt.context` blocks, `prompt.section`, `Notice.Note` and `Notice.Message`
- Rewrites of transcript rows change the drawing alone, and the stored row stays what the model reads
- Asks answer the plugin, and the model learns the answer only from what the hook then returns

## [03]-[COMPOSITION]

One `host` call holds every part, and each part composes with the others through one state value and the engine's `next` chain:

```ts
host(on, {
    layer,      // services the hooks and rows read: the mod's state owner
    hooks,      // engine events: (e, next) => Effect over the services
    tools,      // model-callable tools: Tool(schema, { description, isDeferred, run })
    agents,     // agent types: definition less name, plus isOffered
    band,       // press(key) answering Notice.Band rows
    render: {
        state,  // one Stream of the state value every row reads
        // one row per RenderComponent, Pane rows keyed by id, Backdrop rows keyed by scene
    },
}, ...parts); // { layer, hooks } tables a library ships (enforce(policy)), chained after the mod's own
```

[STATE]:
- One service owns a `SubscriptionRef`, hooks and handlers write it, and `render.state` streams it (`ref.pipe(SubscriptionRef.changes)`)
- Each distinct value redraws every site the plugin draws, band, panes, and rows together, so no row reads services and no timer redraws
- Before the first value every row passes `next()`, and a stream that ends keeps its last value until the session's runtime ends
- Rows run pure over `e` and the state value; slow work belongs in a hook that writes the state

[ROWS]:
- Rows are `(e, state, next) => Effect<Element | RenderElement>`: `next()` passes, `next({ prop })` rewrites, `Effect.map(next(), (row) => Box[row, mine])` decorates, an own tree replaces
- Render hooks hear only the components the table names, so a component left out costs the engine nothing
- Engine nodes from `next()` stand once in a tree, as any child
- Render `AbovePrompt` rows sit outermost, so its `next()` is the band rows `Notice.Band` delivered
- Components whose props name a plugin (`Placed`, `Backdrop`) draw only the mod's own instances, and per-plugin sites (`AbovePrompt`, `BesidePrompt`, `Pane`, `ContextMenu`, `Underlay`) hear the mod's alone
- Failed rows log `message="draw failed"` with `component` and `request` annotations and draw `next()`; rows past their budget are skipped, and a row whose dispatch the engine supersedes is interrupted

[ELEMENTS]:
- `Element<R, F>` is one case per engine element, discriminated by `_tag`, with each surface's props and `children` as one child or a list
- Handlers (`onPress`, `onSubmit`, `onInput`, `onSelect`, `onLinkPress`) are Effects over `R` failing with `F`: `Element<Panes, PlatformError.PlatformError>`
- Handlers run awaited inside the engine's press, input, or select, a failure logging `message="handler failed"` with the event's fields
- `band.press` alone runs after the engine's press returns
- `Button` takes a `label`, or children of strings and `Text` alone, and its type requires an `onPress`
- `key` sits on `Box`, `Button`, `Input`, `Select`, never `Text`, and every list row takes one, so a redraw keeps the ring and a scroll finds it
- Elements a surface's table lacks draw as a fragment (a column `Box` of its children), never a refusal, as `Raster` off the terminal
- `Text` `wrap: 'truncate-end'` cuts to the width the layout gives, so no label is cut by hand

[TYPES]:
- Tree helpers state their return type (`(call: Call): Element<Calls> =>`), or `_tag` widens to `string` and the tree fails to compile
- Branches of `next()` and an own tree inside `Service.use` or `Effect.flatMap` state the union (`Effect.Effect<Element | RenderElement, F>`) or move into `Effect.gen`
- Service keys follow the deterministic form `@rasm/<package>/<path>/<Name>`, which the Effect language service checks
- Nested calls take the pipe form the language service asks for: `held.pipe(SubscriptionRef.changes)`, `held.pipe(form, Effect.succeed)`
- Log lines are a fixed subject and `Effect.annotateLogs` values (`Effect.logWarning('ask failed', cause).pipe(Effect.annotateLogs({ url }))`), never an interpolated subject

## [04]-[STEPS]

Build from the smallest site that carries the fact, and add a site only when the fact outgrows it:
1. Pick the reader and lifetime of each fact in `references/model.md`: a person's glance, a person's decision, a lasting view, or a model's fact
2. Start the plugin from `plugins/claude/risman` (`package.json`, `vite.config.ts`, `hooks/hooks.json`, tsconfigs), keeping `@rasm/claude-code`, `@rasm/vite`, `effect`, `vite`, tag `host:claude-code`, and `nx.sync.ignoredDependencies: ["@rasm/vite"]`; `hooks/register.ts` declares `function register(on: On)` calling `host` and ends with `export { register }`
3. Write the state owner: one service with a `SubscriptionRef` in `layer`, written by the hooks that observe the facts
4. Draw one row: a decoration of an engine row (`references/transcript.md`, `templates/transcript.ts`) or a band row (`references/prompt.md`, `templates/above-prompt.ts`)
5. Move a list that outgrows one row into a pane opened by a band press (`references/panes.md`, `templates/pane.ts`)
6. Ask a decision only the person holds with `ask`, and send the answer on through the hook's own answer (`references/model.md`, `templates/ask.ts`)
7. Declare model-callable tools and agent types the mod serves (`references/events.md`, `templates/runner-tool.ts`)
8. Set a theme only on the person's ask, previewed before it stays (`references/themes.md`)
9. Check: `nx run @rasm/<project>:check--tsc`, then `nx build @rasm/<project>`, then `scripts/preview.py` on the build with steps reaching every state the mod draws (`references/preview.md`)
10. Read each surface's trees and the `model` line before claiming a site works there, as the preview paints the terminal alone

Templates compile against the library on every change through `nx run @rasm/sazandeh:check--tsc`; a template that fails is a library change to answer, never a template to delete.

## [05]-[ASSETS]

`assets/mod-ui.html` opens in any browser and holds its text, tables, and figures by id; `assets/pages/<id>.png` is the same figure as one picture:

| [INDEX] | [ID]             | [OPEN_FOR]                                                                    |
| :-----: | :--------------- | :---------------------------------------------------------------------------- |
|  [01]   | `readers`        | Which call or tree reaches the person alone, which reaches the model          |
|  [02]   | `wf1`            | Transcript rows and their states: tool rows, placed rows, onScreen, fallbacks |
|  [03]   | `wf2`            | Prompt area: band, cell, footer rows, spinner, asks, composer calls           |
|  [04]   | `wf3`            | Docked and inline panes: elements, focus, close origins, scroll, underlay     |
|  [05]   | `wf4`            | Main screen, desktop, VS Code, mobile, OS notifications, the raised matrix    |
|  [06]   | `mk1` to `mk4`   | Whole moments of real sessions with every mod element tagged                  |
|  [07]   | `ideas-calls`    | Which call for which message, and our plugins' channel rules                  |
|  [08]   | `ideas-surfaces` | Placement map, model-facing answers, and site ideas for our plugins           |

Figures show the engine's `$` calls by name, as the artifact maps the engine; a mod reaches each through the library service its reference names.
