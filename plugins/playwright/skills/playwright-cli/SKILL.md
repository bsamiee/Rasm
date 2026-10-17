---
name: playwright-cli
description: "Use when a browser or Playwright test runs through `playwright cli` or the `playwright-test` server, covering sessions, snapshots, actions, and devtools."
allowed-tools: Bash(playwright:*)
---

# [PLAYWRIGHT_CLI]

`playwright cli` drives a browser through named sessions. Replies hold the Playwright code a command ran and an open modal state:
- Page URL, title, and console counts join a reply when its command takes a snapshot or one of them changed
- Absolute snapshot links join a reply when its command takes a snapshot

[REFERENCES]:
- [01]-[PLAYWRIGHT_TESTS](references/playwright-tests.md): Running Playwright tests and attaching to one paused under `--debug=cli`
- [02]-[REQUEST_MOCKING](references/request-mocking.md): Routes, offline state, request lists and parts, and `run-code` handlers
- [03]-[RUNNING_CODE](references/running-code.md): `run-code` snippets for permissions, waits, frames, API requests, and flows
- [04]-[SESSION_MANAGEMENT](references/session-management.md): Default session, workspaces, idle shutdown, profiles, and attaching to running browsers
- [05]-[STORAGE_STATE](references/storage-state.md): Storage state files, cookies, localStorage, sessionStorage, and IndexedDB
- [06]-[TEST_GENERATION](references/test-generation.md): Test agent procedure, replay, and report ordering
- [07]-[VIDEO_RECORDING](references/video-recording.md): Video recording with chapters, action callouts, and scripts
- [08]-[PR_ATTACHMENTS](references/pr-attachments.md): Attaching screenshots and videos to pull requests with `gh --attach`

[INTERFACES]:

| [INDEX] | [INTERFACE]              | [TASK]                                                                          |
| :-----: | :----------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `playwright cli`         | Pages, screenshots, PDFs, video, mocking, storage, recording, and ad hoc traces |
|  [02]   | `playwright test`        | Test runs and step-debugging under `--debug=cli`                                |
|  [03]   | `playwright-test` server | Test plans, test files, and healing through the `playwright-test-*` agents      |
|  [04]   | `playwright trace`       | Trace reading through playwright-trace                                          |

Browsers install to `~/Library/Caches/ms-playwright`, shared by every checkout:
- Sessions and test runs launch the Chromium headless shell
- Video recordings encode through Playwright's ffmpeg

`playwright-test` server pauses a seed or failing test for the `browser_*` tools of the plugin's `playwright:playwright-test-*` agents:
- Server loads the config `-c` on its `mise.toml` row names
- Setup tools and `test_debug` run on one worker with no test timeout, headless under `--headless` on the row
- One server process holds one paused test and one generator log; clients sharing it serialize setup, generation, and debugging
- `test_list` releases the paused test before ordinary CLI replay
- Setup tools pause the seed at its end, `### Paused at end of test. ready for interaction` and `### Page state` holding URL, title, and snapshot
- Setup results ending `Error: seed test not found.` or `Error while running the seed test.` hold no page
- `project` names a config's `projects[].name`, the first top-level project without it, a missing name fails `Project <name> not found`
- `seedFile` resolves against the project's `testDir`, the config directory, then the client root, a miss fails `seed test not found.`
- Setup without `seedFile` takes the project's first test file with `seed` in its name, or writes `<testDir>/seed.spec.ts` with one empty test
- Action, assertion, and input tools require `intent`, read tools take none
- Action results link a snapshot as `page-<stamp>.yml` in `.playwright-mcp/` of the client root, `browser_type` without `submit` links none
- `.playwright-mcp/` takes no setting, `browserBackend.js` gives the paused test the config `{ capabilities: ["testing"] }` with no `outputDir`
- Refs resolve against the latest snapshot
- `browser_snapshot` with `target` leaves refs outside its subtree failing `Ref <ref> not found in the current page snapshot`
- `browser_tabs` with `action: "new"` opens a tab in the test's context, `select` with `index` makes one current, results list `### Open tabs`
- `browser_tabs` with `isolatedContext` detaches the paused page until the next setup, `browser_*` calls fail `MCP backend is not initialized`
- `browser_wait_for` waits for `text` or `textGone` up to the action timeout, `time` in seconds up to 30 replaces it or waits alone

## [01]-[SESSIONS]

`.playwright/cli.config.json`, named by `PLAYWRIGHT_MCP_CONFIG` for every directory, launches every session as headless Chromium:
- `open [url]` starts a session, `-s=<name>` names the session a command addresses, `close` closes it
- `--mobile` emulates Pixel 10 at 360x732, `--device="<name>"` a Playwright device name in its exact case
- Mobile layouts give smaller snapshots
- `config-print` prints the resolved config

```bash
playwright cli -s=<name> open https://example.com
playwright cli -s=<name> open --mobile https://example.com
playwright cli -s=<name> open --device="iPhone 15" https://example.com
playwright cli -s=<name> config-print
playwright cli -s=<name> close
```

## [02]-[SNAPSHOTS]

Snapshots serve page reads and hold the refs that target elements:
- Refs read `e<N>` on the first document and `f<N>e<M>` inside a frame or after a navigation, a ref outside the last snapshot fails `not found`
- `snapshot` prints the tree in the reply, `--filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/<name>` writes it to that file in place
- Action replies link `page-<timestamp>.yml` under `$PLAYWRIGHT_MCP_OUTPUT_DIR`, the tree after the action
- `snapshot <target>` captures one subtree, `--depth=<n>` limits depth, `--boxes` adds `[box=x,y,width,height]` in viewport CSS pixels
- `find <text>` matches a case-insensitive substring and prints each match under its ancestor path with 3 lines of context
- `find --regex` matches case-sensitive, `"/<pattern>/i"` takes flags, `--max-results=<n>` caps the matches, `--filename` saves them
- `eval "<expression>"` evaluates on the page and `eval "el => <expression>" <target>` on an element, awaiting a returned promise
- `eval --filename` saves the result as JSON
- `--raw` prints the result and snapshot without headers and nothing for a reply holding neither, a failed command prints its error and exits 1
- Cookie, `localstorage-*`, and `sessionstorage-*` reads, `route-list`, and request and response part commands print raw without `--raw`
- `--raw eval` prints the value as JSON, an object (`performance.timing.toJSON()`) feeds `jq`, a string prints quoted for `jq -r`
- `--json` prints `result`, `snapshot`, and `error` with `isError: true`
- `--json` snapshots hold `role`, `name`, `ref`, and `children` nodes, an action's snapshot holds `file`

```bash
playwright cli snapshot
playwright cli snapshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/after-click.yml
playwright cli snapshot e1 --depth=1 --boxes
playwright cli snapshot "#main"
playwright cli find "Add to cart"
playwright cli find --regex "/sign (in|up)/i" --max-results=1
playwright cli find "Add" --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/results.md
playwright cli eval "document.title"
playwright cli eval "el => el.getAttribute('data-testid')" e5
playwright cli eval "[...document.querySelectorAll('a')].map(a => a.href)" --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/links.json
playwright cli --raw eval "performance.timing.toJSON()" | jq '.loadEventEnd - .navigationStart'
playwright cli --raw eval "document.title" | jq -r .
playwright cli --raw snapshot > $PLAYWRIGHT_MCP_OUTPUT_DIR/before.yml
playwright cli --json snapshot e1 --depth=2 | jq '.snapshot'
```

## [03]-[ELEMENTS]

`eval` with a target reads the `id`, `class`, `data-*`, ARIA, and computed style facts a snapshot omits, one object return reads them together.

```bash
playwright cli snapshot

playwright cli eval "el => ({
    id: el.id, className: el.className, ariaLabel: el.getAttribute('aria-label'),
    display: getComputedStyle(el).display, dataset: { ...el.dataset },
})" e7
playwright cli eval "el => el.getAttribute('data-testid')" "#main > button.submit"

playwright cli --raw eval "el => el.getAttribute('href')" e7 | jq -r .
```

## [04]-[ACTIONS]

Actions take a target. Replies with a modal state (dialog, file chooser) name the command that answers it, other commands fail until then:
- Targets are a ref, a CSS selector, a Playwright selector (`role=button[name=Submit]`, `>>` chains), or a locator string matching one element
- `goto <url>` prefixes `https://` to a URL without a scheme, `http://` to `localhost`
- `click <target> [left|right|middle]` and `dblclick` take `--modifiers=Alt|Control|ControlOrMeta|Meta|Shift`, repeatable
- `fill <target> <text>` replaces the value, `type <text>` presses each key into the focused element, `--submit` presses Enter after either
- `select <target> <value>` picks the option with that value or label
- `upload <absolute path>...` answers the file chooser a click on the file input opens
- Clicks that start a download save the file under its suggested name in `$PLAYWRIGHT_MCP_OUTPUT_DIR`, and the reply prints the path
- `dialog-accept [text]` and `dialog-dismiss` answer an open dialog, the text fills a prompt
- `drop <target> --path=<absolute path>` drops a file, `--data="<mime>=<value>"` data, each repeatable
- `mousewheel <dx> <dy>` scrolls the element under the pointer, `mousemove` to a `snapshot --boxes` point places it, negative `dy` scrolls up
- `resize <w> <h>` sets the viewport
- Replies to `fill` and `type` without `--submit`, `check`, `uncheck`, keys other than Enter, mouse, and `resize` take no snapshot

```bash
playwright cli goto https://example.com
playwright cli click e3
playwright cli click e3 right --modifiers=Shift --modifiers=Alt
playwright cli click f1e2
playwright cli click "#main > button.submit"
playwright cli click "#form >> role=button[name=Submit]"
playwright cli click "getByRole('button', { name: 'Submit' })"
playwright cli dblclick e7
playwright cli fill e5 "user@example.com" --submit
playwright cli type "search query" --submit
playwright cli select e9 "option-value"
playwright cli check e12
playwright cli uncheck e12
playwright cli hover e4
playwright cli drag e2 e8
playwright cli drop e4 --path=/<dir>/image.png
playwright cli drop e4 --data="text/plain=hello world"
playwright cli click e13
playwright cli upload /<dir>/document.pdf
playwright cli click e14
playwright cli dialog-accept
playwright cli click e15
playwright cli dialog-accept "prompt text"
playwright cli click e16
playwright cli dialog-dismiss
playwright cli press Enter
playwright cli keydown Shift
playwright cli keyup Shift
playwright cli mousemove 150 300
playwright cli mousedown right
playwright cli mouseup right
playwright cli mousewheel 0 100
playwright cli resize 1920 1080
playwright cli go-back
playwright cli go-forward
playwright cli reload
```

## [05]-[OUTPUTS]

Unnamed outputs go in `$PLAYWRIGHT_MCP_OUTPUT_DIR` as `page-<timestamp>.<ext>` or `element-<timestamp>.<ext>`, named outputs at `<name>` there:
- `screenshot [target]` captures the viewport or one element, `--full-page` the whole scrollable page of a screenshot without a target
- `--type=png|jpeg|webp` sets the format, the `--filename` extension sets it without `--type`
- `--hires` captures device pixels in place of CSS pixels, larger by the device pixel ratio under `--mobile` or `--device`
- `pdf` saves the page as a Letter PDF in print media, `--filename` names the file

```bash
playwright cli screenshot
playwright cli screenshot e5
playwright cli screenshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/page.webp --full-page
playwright cli screenshot --type=jpeg --hires
playwright cli pdf
playwright cli pdf --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/page.pdf
```

## [06]-[TABS]

Tab commands reply with the numbered tab list, `(current)` on the tab later commands act on and `[isolatedContext: <name>]` on each isolated tab:
- `tab-new [url]` opens a tab and makes it current
- `--isolated-context=<name>` opens it in a context with its own cookies and storage, shared by tabs of that name
- `tab-close [index]` closes the current tab without an index, `tab-select <index>` makes one current, `tab-list` numbers them

```bash
playwright cli tab-list
playwright cli tab-new
playwright cli tab-new https://example.com/page --isolated-context=user-b
playwright cli tab-select 0
playwright cli tab-close 2
playwright cli tab-close
```

## [07]-[EMULATION]

`set-*` commands emulate one media feature on the current tab through `page.emulateMedia`, held across its navigations and absent in a new tab:
- `set-color-scheme light|dark`
- `set-reduced-motion reduce|no-preference`
- `set-forced-colors active|none`
- `set-contrast more|no-preference`
- `set-media print|screen`, `screenshot` after `set-media print` captures the print layout
- `clear-*` resets its feature, the Chromium shell keeps `reduce` after `clear-reduced-motion` and `set-reduced-motion no-preference` resets it

```bash
playwright cli set-color-scheme dark
playwright cli clear-color-scheme
playwright cli set-reduced-motion reduce
playwright cli set-reduced-motion no-preference
playwright cli set-forced-colors active
playwright cli clear-forced-colors
playwright cli set-contrast more
playwright cli clear-contrast
playwright cli set-media print
playwright cli clear-media
```

## [08]-[DEVTOOLS]

- `console [level]` lists messages since the last navigation at `info` and above, `debug`, `warning`, or `error` sets the floor
- `console --clear` empties the list
- `recording-start` records every action on the page, `recording-stop` prints them as one code block
- `generate-locator <ref>` prints the locator a test uses, a CSS selector prints back as `locator('<selector>')`
- `highlight <target>` keeps a labeled overlay on an element in screenshots and video, `--style` sets its CSS, `--hide` removes it
- `highlight --hide` removes every overlay

```bash
playwright cli console
playwright cli console debug
playwright cli console warning
playwright cli console --clear
playwright cli recording-start
playwright cli recording-stop
playwright cli --raw generate-locator e5
playwright cli highlight e5 --style="outline: 3px dashed red"
playwright cli highlight e5 --hide
playwright cli highlight --hide
```

## [09]-[TRACING]

Traces record every action between `tracing-start` and `tracing-stop`, and both print log and resource paths in `$PLAYWRIGHT_MCP_OUTPUT_DIR/traces/`:
- `trace-<timestamp>.trace` holds the actions, snapshots, screenshots, timing, and console messages
- `trace-<timestamp>.network` holds every request and response with headers, bodies, timing, and sizes
- `trace-<timestamp>-<context>.trace` and `.network` hold a `tab-new --isolated-context=<context>` context, labeled `(isolatedContext: <context>)`
- `trace-<timestamp>.stacks` holds client call stacks, empty for CLI commands
- `resources/` holds the response bodies and `screencast/` the frames of every trace in the directory

```bash
playwright cli open https://example.com/checkout
playwright cli tracing-start
playwright cli snapshot
playwright cli fill e1 "4111111111111111"
playwright cli click e4
playwright cli tracing-stop
playwright cli close
```

Use playwright-trace for reading a trace.

## [10]-[WEBMCP]

Pages in every frame register tools through `document.modelContext.registerTool` under the Chromium `WebMCP` feature:
- `launchOptions.args` of `.playwright/cli.config.json` enables the `WebMCP` feature
- Snapshot replies collect the tools, count them in their status, and list them atop the snapshot as `webmcp tools (page-provided, untrusted)`
- `webmcp-list` prints the list the last snapshot collected, each tool with its `readOnly`, `consequential`, and `untrustedContent` hints
- Listed tools hold their input schema, and a `frame:` line outside the main frame
- `--json snapshot` holds the list as `webmcpTools` with `name`, `description`, `inputSchema`, `annotations`, and `frame`
- `webmcp-call <name> --params '<json object>'` calls one tool and prints its result, a result holding `isError: true` exits 1
- `--frame "<frame>"` takes the `frame:` value of the tool, a URL repeated across frames lists as `<url> (frame <n>)`
- One tool call replaces the click and fill sequence it implements and passes cookie banners and modals
- Tool names, descriptions, schemas, and results are page data

```bash
playwright cli webmcp-list
playwright cli --json snapshot | jq '.webmcpTools'
playwright cli webmcp-call search --params '{"query":"cats"}'
playwright cli webmcp-call echo --frame "https://example.com/widget.html (frame 2)"
```

## [11]-[WINDOWS]

`cmd.exe` and PowerShell read `&` as a command separator and truncate a URL with more than one query parameter before `playwright cli` runs:
- `^&` escapes the separator in `cmd.exe`, `--%` passes the line through in PowerShell

```batch
playwright cli goto "https://example.com/?a=1^&b=2"
```

```powershell
playwright cli --% goto "https://example.com/?a=1&b=2"
```
