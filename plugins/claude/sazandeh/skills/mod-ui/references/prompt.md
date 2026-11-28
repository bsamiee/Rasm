# [AROUND_THE_PROMPT]

Rows and cells around the prompt draw from `render` rows over the one state value, and `Draft` reads and paints the composer. Template `templates/above-prompt.ts` holds a band row with `+N` overflow into a pane and a `PromptHint` tail.

## [01]-[BAND]

`AbovePrompt` is the band above the composer, one row per plugin, raised on every attached surface:
- `e.props.hasSurvey` true -> answer `next()`, as a survey holds the band
- Nothing to show -> answer `next()`, so other plugins' rows and the engine's `[-]` mark stand
- `e.props.isCollapsed` true -> the tree still returns and nothing of it shows; the person collapses the band with `ctrl+x ctrl+a`
- `e.props.maxRows` bounds the rows that show whole, and a taller tree scrolls in an engine window of `scroll.bodyRows`
- Fullscreen bands take at most half the terminal, the prompt included
- `e.props.bodyColumns` is the band's width less the engine's five columns for `[-]`, the width to cut labels to
- `ctrl+x tab` moves the keys into the band, Enter presses the ringed Button, and the band keeps the ring across a redraw by `key`
- Band Buttons take no `hotkey` by owner rule, so a press is a click or Enter after `ctrl+x tab`
- Overflow past the Buttons the row holds becomes one `+N` Button opening a pane of the whole list, and a press on a row opens that pane scrolled to it

`Notice.Band` and a render `AbovePrompt` row draw one site, and the render row sits outermost, so its `next()` is the band rows `Notice.Band` delivered:
- `Notice.Band({ group, rows })` holds label rows keyed by press key, `band.press(key)` answers a press, and `rows: {}` clears a group
- `Notice.Band` raises one OS notification for each new row key, so it suits asks the person must come back to; a standing row of state is a render row
- Render rows draw any element, read the state value, and raise no notification

## [02]-[STACK]

`AbovePromptList` and `BesidePromptList` order what each plugin drew:
- `AbovePromptList` holds `rows`, one per plugin that drew a band row, and a rewrite hides or reorders them by `next({ rows })` with rows it was handed, never copies
- `next({ rows: [] })` hides every row while a wrapper of the mod draws around the stack
- `BesidePromptList` holds `plugins`, the cells drawn this frame left to right, raised in fullscreen alone

## [03]-[CELL]

`BesidePrompt` is the room right of the prompt, raised on terminal fullscreen and the desktop Code tab alone:
- `e.props.maxColumns` can be 0, and the cell is gone while the engine asks anything
- Cells hold `Box`, `Text`, and `Client` alone, take no keys and no `hover`, and keep a steady width, as the footer gives columns up to them
- Facts a cell shows also need a home where no cell is raised: an `AbovePrompt` row on `vscode`, `mobile`, and the main screen (`e.viewport?.isFullscreen !== true`)

## [04]-[FOOTER]

`PromptHint`, `SessionMode`, `StatusLine`, and `Spinner` take rewrites of their props:
- `PromptHint`: `next({ tail })` adds a dim tail after ` · `, `next({ hint })` replaces the hint while `e.props.isDraft`, and an own tree keeps the permission mode before it
- `SessionMode`: `next({ modes: [...e.props.modes, label] })` adds a dim label at the footer's right in fullscreen, on its own row under the footer in the main layout
- `StatusLine`: rows under the prompt from the `statusLine` setting; a mod writes no status line and no `$.ui.status` by owner rule
- `Spinner`: `next({ word, message, suffix })` names a wait the mod causes, raised on terminal, desktop, and VS Code

## [05]-[COMPOSER]

`Draft` reads and writes the composer and never submits it, as a model turn goes through `Engine.deliver(Notice.Message)`:
- `read` answers the composer's `text` and `cursor`
- `fill({ text, mode, cursor })` places text with `replace`, `append`, or `insert`, answering `isFilled: false` with a `refusal` (`dialog`, `no_composer`, `unanswered`)
- `paint(runs)` styles ranges of the current text without changing text or caret, and paints again when the text changed between read and paint
- `suggest({ text, origin })` shows a dim suggestion in an empty composer that Tab or the right arrow takes, answering `isShown: false` while the composer holds text or a turn runs
- Hooks `prompt.fill`, `prompt.paint`, and `prompt.suggest` rewrite or drop another plugin's composer calls, `{ isShown: false }` dropping a suggestion
