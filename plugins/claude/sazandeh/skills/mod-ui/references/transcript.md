# [TRANSCRIPT_ROWS]

Engine rows of the transcript take render rows that decorate, rewrite, or replace what the engine draws, and placed rows add the mod's own card at a moment. Template `templates/transcript.ts` holds a call-row line, a turn total, and a fenced report table.

## [01]-[VERBS]

Rows answer in one of these forms over `next`, the drawing beneath:

| [INDEX] | [FORM]   | [ANSWER]                                      | [USE_WHEN]                               |
| :-----: | :------- | :-------------------------------------------- | :--------------------------------------- |
|  [01]   | Pass     | `next()`                                      | Nothing of the mod's applies to this row |
|  [02]   | Rewrite  | `next({ prop: value })`                       | One prop the engine draws changes        |
|  [03]   | Decorate | `Effect.map(next(), (row) => Box[row, mine])` | Engine row stays, the mod adds a line    |
|  [04]   | Replace  | `Effect.succeed(tree)`                        | Mod draws the whole row                  |

- `next(rewrite)` merges the partial rewrite into `e.props`, and pinned props (`onScreen`, `view`, `plugin`, `tool_use_id`) pass as received
- Read-only props changed in a rewrite are refused, and the hook passing them fails
- Engine nodes from `next()` stand once in a tree, as a child like any element
- `e.props.isExpanded` true (ctrl+o, `--verbose`) -> answer `next()`, so the person's expanded view stays the engine's
- Rows run pure over `e` and the state value: a row past the engine's budget is skipped, and a spawn or fetch belongs in a hook that writes the state
- Failed rows log `message="draw failed"` with `component` and `request` and draw `next()`, and a row whose dispatch the engine supersedes is interrupted
- Rows of components every plugin shares hear other plugins' calls and text too, and answer `next()` for what is not the mod's

## [02]-[ROWS]

Engine rows and what each holds for a mod:

| [INDEX] | [COMPONENT]             | [HOLDS]                                             | [SURFACES]    |
| :-----: | :---------------------- | :-------------------------------------------------- | :------------ |
|  [01]   | `ToolUse`               | `tool`, `input`, `isRunning`, `isErrored`, `output` | Every surface |
|  [02]   | `ToolResult`            | Result block under a call, schema checked           | Every surface |
|  [03]   | `ToolGroup`             | Folded count line, `isExpanded` unfolds it          | Every surface |
|  [04]   | `UserMessage`           | Prompt, task notice, peer message, by `origin`      | Every surface |
|  [05]   | `AssistantMessage`      | Reply block or summary row                          | Every surface |
|  [06]   | `CommandOutput`         | `command`, `args`, `text`, `isErrored`              | Every surface |
|  [07]   | `CodeBlock`             | `language`, `source`, `isClosed`                    | Terminal      |
|  [08]   | `TurnDuration`          | `word`, `durationMs`, `waitingFor`                  | Terminal      |
|  [09]   | `ToolProgress`          | Background pill under a running call                | Terminal      |
|  [10]   | `AgentRow`              | Agent list row, label and body                      | Every surface |
|  [11]   | `Welcome`, `InfoNotice` | Header and its dim notice line                      | Terminal      |

- Fullscreen folds shell, read, and search calls into one `ToolGroup` line (`Ran 1 shell command`), hiding their `ToolUse` decoration
- Main layouts and remote surfaces draw each call's decoration, and fullscreen once the group expands
- `ToolUse` on mobile draws the mod's addition as one quiet line, and as a card under the row when the `Box` holding both sets `borderStyle` or `backgroundColor`
- `CodeBlock` is asked at the opening line (`isClosed` false), where `next()` streams the source and an own tree holds it back, and again at the close
- `CodeBlock` answers fast, as the reply streams behind it, and a source past 20000 characters is not asked again
- Fences of a language the mod names (`report`) draw as the mod's tree once `isClosed`, a table of the reply's `label: value` lines
- `UserMessage` rewrites draw delivered events apart from typed prompts, and the stored message the model reads stays
- `e.props.onScreen` holds the row's visible range per surface: `null` while drawn outside the viewport, absent on the main screen

## [03]-[PLACED]

`Placed.place(id)` adds one row at the transcript's end, drawn by `render.Placed` and scrolling with the transcript:
- Placing an id again answers the standing row with the same `requestId`, so a redraw comes from the state stream, never a second place
- `/clear` ends every placed row, and a main-screen `/compact` ends them too; the next place of the id answers a new `requestId`
- Placed rows stay out of the transcript file and `--resume`, so a fact the model or a later session needs goes through a channel as well
- `Placed.rows` streams this mod's standing rows by id: `requestId`, `onScreen` per surface, and whether the last terminal draw was fullscreen
- `paint({ key, columns, rows }, cell)` packs a `Raster` element's cells into its props, and `Placed.blit({ requestId, ...paint(props, cell) })` repaints the mounted raster without a redraw
- Blits of another size are refused (a resize is a redraw), and an `Image` inside tmux or Zellij draws its `alt` and refuses a blit
- Desktop, VS Code, and mobile lower `Raster` and `Image` to a fragment and refuse a blit, so a picture there is an `Svg`
- `-p` runs answer `isPlaced: false` with a reason
