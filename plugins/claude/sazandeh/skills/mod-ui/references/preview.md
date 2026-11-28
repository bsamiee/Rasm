# [PREVIEW]

`scripts/preview.py` runs a built mod beside a recorder plugin and reports what each surface drew, called, and refused, as report lines. Read the report before claiming a draw works on a surface.

## [01]-[MOUNT]

Templates and draft modules become mod bundles in a scratch kit under the plugin's ignored `.cache/`, never committed:
1. Kit: `plugins/claude/sazandeh/.cache/mount/<name>/` holding `.claude-plugin/plugin.json` (`name`, `version`, `description`), `hooks/hooks.json` (`{ "modules": ["./register.ts"] }`), `hooks/register.ts` (the module), `package.json` (`name`, `private`, `type: module`), and `vite.config.ts` exporting `defineConfig(claude(import.meta.dirname))` from `@rasm/vite/claude`
2. Build: `../../../node_modules/.bin/vite build --outDir dist --emptyOutDir` inside the kit, as the kit resolves `@rasm/claude-code`, `effect`, and `vite` through the plugin's own `node_modules`
3. Validate: `claude plugin validate <kit>/dist`, whose `author` warning changes nothing for a local mod
4. Preview: `uv run --locked --script scripts/preview.py <kit>/dist [--step KIND:VALUE]...`
5. Delete the kit once the report is read

States reached only past auto mode take a kit `hooks/register.ts` wrapping the template in `template.ts`, one matched allow per tool, as validate refuses an unmatched `tool.check` beside another hook on it in one bundle:

```ts
import type { On } from 'claude-code';
import { register as template } from './template';

function register(on: On) {
    on('tool.check', { tool: 'Bash' }, () => ({ decision: 'allow' }));
    on('tool.check', { tool: 'Agent' }, () => ({ decision: 'allow' }));
    template(on);
}

export { register };
```

Mods of the repository take their Nx build output in place of a kit: `.artifacts/vite/@rasm/<project>` after `nx build @rasm/<project>`.

## [02]-[RUN]

`preview.py <bundle> [--step KIND:VALUE]... [--layout fullscreen|main]... [--empty-layout] [--size COLUMNS ROWS] [--limit SECONDS]`:

| [INDEX] | [STEP]               | [EFFECT]                                                                     |
| :-----: | :------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `type:<text>`        | Types the text and Enter: a prompt, or `/<command>`                          |
|  [02]   | `key:<tmux keys>`    | Sends keys (`C-x Tab`, `Enter`, `Down`, `Escape`) through the control client |
|  [03]   | `call:<tool> <json>` | Has the fake model call the tool with that input on the next turn            |
|  [04]   | `say:<text>`         | Has the fake model reply the text, a JSON string body (`\n`, `\u0060`)       |
|  [05]   | `wait:<regex>`       | Waits for a debug-log line matching the pattern                              |
|  [06]   | `resize:<c> <r>`     | Resizes the terminal                                                         |

- Both layouts run by default, at 120x40; `--size 160 40` docks panes and raises `BesidePrompt` in fullscreen, 110 columns being the dock's floor
- `--limit` bounds each step, and a step under an animation or a dialog reads `unsettled after`
- `--empty-layout` skips the terminal and runs validate, mount, and session alone, about 5 s
- Fake model replies answer each `call:` and `say:` step once, in step order, and every other request with `PREVIEW-REPLY`, spending no tokens
- Steps typed while a call runs join that call's result message, and each still gets its own answer
- Agents read their prompt as a step, so `call:Agent {"prompt":"call:Bash {…}","run_in_background":true}` runs a call inside a background agent; four such steps hold four calls running at once
- `say:` reaches rows of a reply's own words (`CodeBlock`, `AssistantMessage`), backticks written `\u0060`, as typed ones reached the model unmatched
- Auto mode's check gets no usable answer from the fake model, so a call it checks reads `Not run`: `echo` runs, `ping`, `find /`, and `tail -f` take the kit's allow
- Engines refuse a standalone `sleep`, so a call held running is `ping -c <seconds> 127.0.0.1` under the allow
- Plugin names join the work folder's path, and tmux sockets sit there under short names, so long plugin names run

## [03]-[STAGES]

Each stage owns one kind of evidence:

| [INDEX] | [STAGE]    | [EVIDENCE]                                                                                          |
| :-----: | :--------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `validate` | Engine refusals with file and line, hooked events, sites                                            |
|  [02]   | `live`     | Terminal paint per layout: changed rows with style runs, recorded draws and calls, debug-log lines  |
|  [03]   | `mount`    | `claude plugin test` of each site on each surface that raises it, and a press of every keyed Button |
|  [04]   | `session`  | Headless session attached as desktop, VS Code, and mobile: trees, pushes, refusals                  |
|  [05]   | `trees`    | Mount and session trees grouped, `same as live` where they equal the terminal's                     |
|  [06]   | `model`    | Request count and bytes, and the mod's strings found in request bodies                              |

- Library mods name no component literally (`component=?`), so mount and session ask the components their rows drew or rewrote live
- `--empty-layout` runs have no live draws, so mount and session ask every raised component with empty props
- Trees reading `<engine/>` are `next()` passed through: the row had no state worth drawing at mount
- `reads:` under `model` lists the mod's strings a request carried, the check that a person-only drawing stayed out of requests

## [04]-[VERDICT]

Subject lines read `preview <name>: passed|failed, N faults, S s`:
- Faults are `error:`, `warn:`, `refused`, `fault:`, and `fault press` lines, and `log debug:` lines of the mod holding `level=WARN` or `hook failed:`; exit 1 on a validation refusal, a load failure, or any fault, 130 on interrupt
- `render hook skipped: returned a Button without an onPress function` is a fault validate passes; the engine then draws its own site
- `ui.render hook failed: timeout` is a row past its budget, `message="draw failed"` with `component` and `request` a row that failed and drew `next()`
- Reports show the screen as drawn, so a command racing its own pane's first render shows rows in either order
- Out of reach: desktop, VS Code, and mobile paint, images, kitty keys, OS notifications, sound, mouse, and hover, where trees stand for paint
- Runs leave nothing behind: the next claim of a slot sweeps a killed run's tmux servers and `claude purge` removes its transcripts
