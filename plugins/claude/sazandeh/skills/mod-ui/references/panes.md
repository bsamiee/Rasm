# [PANES]

Panes are framed regions the surface places and the mod draws: a `render.Pane` row per pane id, opened and closed through the `Panes` service. Template `templates/pane.ts` holds a list pane with a widget glance and a dialog form.

## [01]-[ROW]

`render.Pane` keys one row per pane id, and the row holds the open arguments less `id` beside `draw` and `closes`:

| [INDEX] | [FIELD]           | [EFFECT]                                                                          |
| :-----: | :---------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `title`           | Tab text while more than one pane is open, the id when absent                     |
|  [02]   | `columns`         | Dock width wanted, at most 45% of the terminal and at least 24                    |
|  [03]   | `rows`            | Inline height wanted, a third when absent; a size the person set wins             |
|  [04]   | `focus`           | Request for the keyboard, granted only over an empty composer that holds the keys |
|  [05]   | `closeOnEscape`   | Escape that hands the keys back also closes, origin `person`                      |
|  [06]   | `holdToasts`      | Every plugin's toasts wait undrawn while the pane shows                           |
|  [07]   | `backgroundColor` | Paint under the body: a theme key, a raw color, or `transparent`                  |
|  [08]   | `placement`       | `widget` asks the desktop app for the small form; other surfaces ignore it        |
|  [09]   | `draw`            | `(e, state, next)` answering an `Element`, the row every pane redraw runs         |
|  [10]   | `closes`          | `Effect<boolean>` deciding whether the person's close goes through                |

- `Panes.open({ id, ...override })` sends the row's fields under what the call names, so a call retitles with `{ id, title }` and the manners stay declared once
- Ids with no row fail `Panes.open` with `badArgument` (`no render row, pane <id>`), and another plugin's id fails through the engine's refusal
- Every open sets `focus`, `closeOnEscape`, `holdToasts`, `rows`, and `columns` anew, so a row's values return on each open
- `Panes.list` answers this plugin's open panes, placed ones first, from the engine's record, which outlives a module reload
- `Panes.close({ id })` settles once the pane is gone; the mod's own close raises no `closes` of its own

## [02]-[PLACEMENT]

`Panes.open` resolves `isPlaced: true` once a surface seats the pane, or `isPlaced: false` with a `reason`, an open pane waiting undrawn and never a failure:
- Asked opens (a command or prompt the person entered, a Button, Input, or Select they worked) place at any width
- Unasked opens (a timer, `session.start`, a turn event, a queued prompt) place from 144 columns, 110 for an id the person opened and never closed by hand
- Fullscreen from 110 columns docks the pane beside the transcript, narrower or main-screen layouts seat it inline above the prompt
- Desktop seats a `widget` (minimized) form on request, read on `e.props.placement`; VS Code and mobile place panes and report no widget
- `-p` runs and sessions with no attached surface answer `isPlaced: false` with a reason starting `no-surface`, and an attaching surface draws the pane later
- Commands opening a pane answer the reason as `{ text, isTextDisplayOnly: true }`, so the person learns why nothing drew (`templates/pane.ts`)

## [03]-[KEYS]

Keys belong to one site at a time, and `focus` is a request the surface refuses wherever it would take keys from the person:
- `focus` from a band press is refused, as the band holds the keys; the pane opens unfocused
- `focus` from a pane Button is refused for the same reason: the pane the person holds keeps the keys
- `focus` from a command reaches the pane, as the composer is empty once the command runs; template dialogs open from a command
- `focus` opens from a hook holding a tool call while no surface is attached are refused, so a decision there is an `ask`
- `ctrl+x tab` moves the keys into the band or a pane, a click on a pane focuses it, and `ctrl+x x` closes a focused pane
- First Escape in a `closeOnEscape` pane returns the keys, and a second at an idle empty prompt closes it
- Button `hotkey` arms only inside the focused site, and band Buttons take none by owner rule

## [04]-[DRAWING]

`draw` answers from `e` and the state value, never from a read of its own, as the one state stream redraws every pane:
- `e.props.bodyColumns` is the body less the engine's marks, the width to size a table to, never `e.viewport.columns`
- `e.props.placement` is `dock`, `inline`, or `widget`; a widget body is one glance line, as clicks may not reach a small form
- `e.props.isFocused`, `e.props.isPromptCovered`, and `e.props.scroll` (`offset`, `bodyRows`) are read-only, and `scroll` is the engine's window, moved by wheel and keys
- `Engine.scroll({ in: <id>, to: 'end' | 'start' | { key } })` moves the window, `Engine.focus({ requestId: <id>, key })` moves the ring; keys name keyed elements of this plugin
- Lists key every row (`Box` or `Button` `key`), so a redraw keeps the focus ring and a scroll finds the row
- `Panes.open` resolves before the body's first draw, so a scroll to a row right after it is refused until the row draws
- Openers retry that scroll on a bounded schedule (`templates/above-prompt.ts`), as the refusal reads `no element of its own is drawn under that key`
- Empty state is one dim line naming what would appear, never an empty frame
- Frames: the close mark sits on a row of the frame's own, and an inline frame also takes its border rows and the tab row while tabs show

## [05]-[CLOSING]

`closes` hears the person alone: the close mark, `ctrl+x x`, or Escape under `closeOnEscape`:
- `false` keeps the pane and hands the keys back, `true` lets it close, a failure logs `message="close failed"` with the `pane` id and closes
- Panes closed by their own plugin (`Panes.close`) or by an unload raise no `closes` row
- Work a pane started (a poll, a follow) stops in `closes`, as a closed pane's state no longer draws anywhere
- `holdToasts` belongs to a dialog the person answers and leaves: on a standing pane it hides every plugin's toasts while it shows

## [06]-[SURFACES]

`Pane` is raised on every attached surface, with each surface's element table:
- Terminal: every element draws; `Raster` and `Image` in a pane need the terminal's graphics, and tmux or Zellij draw an `Image` as its `alt`
- Desktop: `Svg` and `Shader` draw, `Raster` and `Image` lower to a fragment (an empty column `Box`)
- VS Code: `Svg` draws as an image, `Raster` and `Image` lower to a fragment
- Mobile: `Input` and `Select` draw as an empty column `Box` and `Progress` as `Box` and `Text` (session trees of `templates/pane.ts`), so a form keeps a `Text` line naming each chosen value
- Redraws fold to 30 a second for a shown pane on the terminal and 10 elsewhere, so the state stream redraws and no timer calls `invalidate`
