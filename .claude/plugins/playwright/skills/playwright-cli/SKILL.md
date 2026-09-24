---
name: playwright-cli
description: Automate browser interactions, test web pages and work with Playwright tests.
allowed-tools: Bash(playwright-cli:*) Bash(npx:*) Bash(npm:*)
---

# Browser Automation with playwright-cli

## Quick start

Snapshots serve most page reads, `screenshot` is rarely needed.

```bash
playwright-cli open
playwright-cli goto https://playwright.dev
playwright-cli click e15
playwright-cli type "page.click"
playwright-cli press Enter
playwright-cli screenshot
playwright-cli close
```

## Commands

### Core

- `fill --submit` presses Enter after filling the element
- `drop` delivers files or data to an element from outside the page
- `eval` with a ref reads an element's id, class, or any attribute the snapshot omits

```bash
playwright-cli open
playwright-cli open https://example.com/
playwright-cli goto https://playwright.dev
playwright-cli type "search query"
playwright-cli click e3
playwright-cli dblclick e7
playwright-cli fill e5 "user@example.com"  --submit
playwright-cli drag e2 e8
playwright-cli drop e4 --path=./image.png
playwright-cli drop e4 --data="text/plain=hello world"
playwright-cli hover e4
playwright-cli select e9 "option-value"
playwright-cli upload ./document.pdf
playwright-cli check e12
playwright-cli uncheck e12
playwright-cli snapshot
playwright-cli find "Sign in"
playwright-cli find --regex "Sign (in|up)"
playwright-cli find --regex "/sign (in|up)/i"
playwright-cli eval "document.title"
playwright-cli eval "el => el.textContent" e5
playwright-cli eval "el => el.id" e5
playwright-cli eval "el => el.getAttribute('data-testid')" e5
playwright-cli dialog-accept
playwright-cli dialog-accept "confirmation text"
playwright-cli dialog-dismiss
playwright-cli resize 1920 1080
playwright-cli close
```

### Navigation

```bash
playwright-cli go-back
playwright-cli go-forward
playwright-cli reload
```

### Keyboard

```bash
playwright-cli press Enter
playwright-cli press ArrowDown
playwright-cli keydown Shift
playwright-cli keyup Shift
```

### Mouse

```bash
playwright-cli mousemove 150 300
playwright-cli mousedown
playwright-cli mousedown right
playwright-cli mouseup
playwright-cli mouseup right
playwright-cli mousewheel 0 100
```

### Save as

Unnamed files and traces go to `$PLAYWRIGHT_MCP_OUTPUT_DIR`, relative file names resolve against the current directory.

```bash
playwright-cli screenshot
playwright-cli screenshot e5
playwright-cli screenshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/page.png
playwright-cli screenshot --hires
playwright-cli pdf --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/page.pdf
```

### Tabs

```bash
playwright-cli tab-list
playwright-cli tab-new
playwright-cli tab-new https://example.com/page
playwright-cli tab-close
playwright-cli tab-close 2
playwright-cli tab-select 0
```

### Storage

```bash
playwright-cli state-save
playwright-cli state-save $PLAYWRIGHT_MCP_OUTPUT_DIR/auth.json
playwright-cli state-load $PLAYWRIGHT_MCP_OUTPUT_DIR/auth.json

playwright-cli cookie-list
playwright-cli cookie-list --domain=example.com
playwright-cli cookie-get session_id
playwright-cli cookie-set session_id abc123
playwright-cli cookie-set session_id abc123 --domain=example.com --httpOnly --secure
playwright-cli cookie-delete session_id
playwright-cli cookie-clear

playwright-cli localstorage-list
playwright-cli localstorage-get theme
playwright-cli localstorage-set theme dark
playwright-cli localstorage-delete theme
playwright-cli localstorage-clear

playwright-cli sessionstorage-list
playwright-cli sessionstorage-get step
playwright-cli sessionstorage-set step 3
playwright-cli sessionstorage-delete step
playwright-cli sessionstorage-clear
```

### Network

```bash
playwright-cli route "**/*.jpg" --status=404
playwright-cli route "https://api.example.com/**" --body='{"mock": true}'
playwright-cli route-list
playwright-cli unroute "**/*.jpg"
playwright-cli unroute
```

### DevTools

- `recording-start` records user actions in the browser, `recording-stop` prints them as Playwright code
- `video-show-actions` marks each later action with a callout naming the action and highlighting its target
- `generate-locator` builds a Playwright locator for an element from its ref or selector
- `highlight` keeps an overlay on an element, `--style` sets its CSS, `--hide` clears the target's highlight or, without a target, every highlight

```bash
playwright-cli console
playwright-cli console warning
playwright-cli requests
playwright-cli request 5
playwright-cli run-code "async page => await page.context().grantPermissions(['geolocation'])"
playwright-cli run-code --filename=script.js
playwright-cli tracing-start
playwright-cli tracing-stop

playwright-cli recording-start
playwright-cli recording-stop

playwright-cli video-start $PLAYWRIGHT_MCP_OUTPUT_DIR/video.webm
playwright-cli video-chapter "Chapter Title" --description="Details" --duration=2000
playwright-cli video-stop

playwright-cli video-show-actions --duration=600 --position=top-right
playwright-cli video-hide-actions

playwright-cli show --annotate

playwright-cli generate-locator e5 --raw

playwright-cli highlight e5
playwright-cli highlight e5 --style="outline: 3px dashed red"
playwright-cli highlight e5 --hide
playwright-cli highlight --hide
```

## Raw output

The global `--raw` option strips page status, generated code, and snapshot sections from the output, returning only the result value. Use it to pipe command output into other tools. Commands that don't produce output return nothing.

```bash
playwright-cli --raw eval "JSON.stringify(performance.timing)" | jq '.loadEventEnd - .navigationStart'
playwright-cli --raw eval "JSON.stringify([...document.querySelectorAll('a')].map(a => a.href))" > links.json
playwright-cli --raw snapshot > before.yml
playwright-cli click e5
playwright-cli --raw snapshot > after.yml
diff before.yml after.yml
TOKEN=$(playwright-cli --raw cookie-get session_id)
playwright-cli --raw localstorage-get theme
```

For structured output wrapping every reply as JSON, pass --json
```bash
playwright-cli list --json
```

## Open parameters

- `--mobile` emulates a generic mobile device, Pixel 10 on Chromium and iPhone 17 on WebKit
- Prefer `--mobile` when a mobile layout is acceptable, mobile pages can give smaller snapshots
- Profiles are in-memory by default, `--persistent` persists one, `--profile` sets its directory when the user requests one
- `attach --extension` connects through the Playwright Extension, `attach --cdp` to a running Chrome or Edge channel or a CDP endpoint
- `detach` leaves an attached browser running

```bash
playwright-cli open --browser=chrome
playwright-cli open --browser=firefox
playwright-cli open --browser=webkit
playwright-cli open --browser=msedge

playwright-cli open --mobile
playwright-cli open --device="iPhone 15"

playwright-cli open --persistent
playwright-cli open --profile=/path/to/profile

playwright-cli attach --extension=chrome

playwright-cli attach --cdp=chrome
playwright-cli attach --cdp=msedge

playwright-cli attach --cdp=http://localhost:9222

playwright-cli open --config=my-config.json

playwright-cli close
playwright-cli -s=msedge detach
playwright-cli delete-data
```

## URLs with `&` on Windows

On Windows, `cmd.exe` and PowerShell treat `&` as a command separator, so URLs with multiple query parameters get truncated before `playwright-cli` runs. Escape `&` with `^&` in `cmd.exe`, or use `--%` in PowerShell:

```batch
playwright-cli goto "https://example.com/?a=1^&b=2"
```

```powershell
playwright-cli --% goto "https://example.com/?a=1&b=2"
```

## Snapshots

After each command, playwright-cli provides a snapshot of the current browser state.

```bash
> playwright-cli goto https://example.com
### Page
- Page URL: https://example.com/
- Page Title: Example Domain
### Snapshot
[Snapshot](.artifacts/playwright/page-2026-02-14T19-22-42-679Z.yml)
```

`playwright-cli snapshot` takes a snapshot on demand, and its options combine:
- `snapshot` saves to a file with a timestamp-based name, `--filename` names it when the snapshot is part of the workflow result
- A selector or ref snapshots one element in place of the whole page
- `--depth` limits snapshot depth, a ref snapshot afterwards reads one subtree
- `--boxes` adds each element's bounding box as `[box=x,y,width,height]`
- `find` searches a large snapshot in place of capturing it whole, and returns each matching node with 3 lines of context
- `find --regex` takes flags when the pattern sits in slashes, `/i` for case-insensitive

```bash
playwright-cli snapshot

playwright-cli snapshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/after-click.yaml

playwright-cli snapshot "#main"

playwright-cli snapshot --depth=4
playwright-cli snapshot e34

playwright-cli snapshot --boxes

playwright-cli find "Add to cart"
playwright-cli find --regex "\\$[0-9]+\\.[0-9]{2}"
```

## Targeting elements

By default, use refs from the snapshot to interact with page elements.

```bash
playwright-cli snapshot

playwright-cli click e15
```

You can also use css selectors or Playwright locators.

```bash
playwright-cli click "#main > button.submit"

playwright-cli click "getByRole('button', { name: 'Submit' })"

playwright-cli click "getByTestId('submit-button')"
```

## Browser Sessions

- `-s=<name>` names a session, `delete-data` deletes its user data, the default session's without `-s`
- `kill-all` force-kills every browser process

```bash
playwright-cli -s=mysession open example.com --persistent
playwright-cli -s=mysession open example.com --profile=/path/to/profile
playwright-cli -s=mysession click e6
playwright-cli -s=mysession close
playwright-cli -s=mysession delete-data

playwright-cli list
playwright-cli close-all
playwright-cli kill-all
```

## Installation

If global `playwright-cli` command is not available, try a local version via `npx playwright cli`:

```bash
npx --no-install playwright --version
```

When local version is available, use `npx playwright cli` in all commands. Otherwise, install `playwright-cli` as a global command:

```bash
npm install -g @playwright/cli@latest
```

## Example: Form submission

```bash
playwright-cli open https://example.com/form
playwright-cli snapshot

playwright-cli fill e1 "user@example.com"
playwright-cli fill e2 "password123"
playwright-cli click e3
playwright-cli snapshot
playwright-cli close
```

## Example: Multi-tab workflow

```bash
playwright-cli open https://example.com
playwright-cli tab-new https://example.com/other
playwright-cli tab-list
playwright-cli tab-select 0
playwright-cli snapshot
playwright-cli close
```

## Example: Debugging with DevTools

```bash
playwright-cli open https://example.com
playwright-cli click e4
playwright-cli fill e7 "test"
playwright-cli console
playwright-cli requests
playwright-cli close
```

```bash
playwright-cli open https://example.com
playwright-cli tracing-start
playwright-cli click e4
playwright-cli fill e7 "test"
playwright-cli tracing-stop
playwright-cli close
```

## Example: Interactive session

Ask the user for UI review or design feedback. The user draws boxes on the live page and types comments; you receive the annotated screenshot, the snapshot of the marked region, and the user's notes. Use this whenever the user asks for "UI review", "design feedback", or to "ask the user what they think / want / mean":

```bash
playwright-cli open https://example.com
playwright-cli show --annotate
```

## Specific tasks

* **Running and Debugging Playwright tests** [references/playwright-tests.md](references/playwright-tests.md)
* **Request mocking** [references/request-mocking.md](references/request-mocking.md)
* **Running Playwright code** [references/running-code.md](references/running-code.md)
* **Browser session management** [references/session-management.md](references/session-management.md)
* **Storage state (cookies, localStorage)** [references/storage-state.md](references/storage-state.md)
* **Test generation (plan / generate / heal)** [references/test-generation.md](references/test-generation.md)
* **Tracing** [references/tracing.md](references/tracing.md)
* **Video recording** [references/video-recording.md](references/video-recording.md)
* **Inspecting element attributes** [references/element-attributes.md](references/element-attributes.md)
