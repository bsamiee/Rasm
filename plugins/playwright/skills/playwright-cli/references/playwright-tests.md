# [PLAYWRIGHT_TESTS]

`playwright test -c <project>` runs the tests of the `playwright.config.ts` in directory `<project>` from any directory. Filter arguments are regular expressions over absolute test file paths, `<file>:<line>` selects one test. Runs write each failure's `error-context.md` (error, aria snapshot of the failing locator, source), `.last-run.json`, and the traces, screenshots, and videos `use` turns on under the config's `outputDir`. `--reporter=html` writes the HTML report to the folder and open mode `mise.toml` `[env]` sets:
- List reporter prints each failure as `<file>:<line>:<column> › <describe> › <test>` with the error, call log, and `Error Context` path
- `--last-failed` reruns the failures the previous run recorded in `.last-run.json`, after a run without failures it exits `No tests found`

```ts
import { defineConfig } from 'playwright/test';

export default defineConfig({
    testDir: 'tests',
    outputDir: `${process.env.PLAYWRIGHT_MCP_OUTPUT_DIR}/<project>`,
});
```

```bash
playwright test -c <project>
playwright test -c <project> login.spec.ts:12
playwright test -c <project> --last-failed
```

## [01]-[DEBUGGING]

`--debug=cli` runs one worker, stops after the first failure, keeps the config's headless mode, pauses at the test's first Playwright call (a fixture's call included), and prints `- Run "playwright-cli attach tw-<id>" to attach to this test`. The run stays in the background while the CLI drives the paused page:
- `attach tw-<id>` from the repository root creates session `tw-<id>`, every later command takes `-s=tw-<id>`
- Replies while paused end with `### Paused` naming the next call and its absolute `<file>:<line>`
- `step-over` runs the next call, `pause-at <file>:<line>` runs to the first call on that line, the file matching as a path suffix
- `pause-at` on a line holding no Playwright call never pauses, the test runs to its end
- `step-over` on the test's last call or a failing call pauses at `Close context` with the page in its final state
- Test and expect timeouts stop while paused and run while a stepped call executes
- `snapshot`, `console`, `requests`, and `eval` read the page at the pause, each action prints `### Ran Playwright code` for the test
- `resume` runs the test to its end, the run exits and session `tw-<id>` closes
- `detach` leaves the test paused for a later `attach`, a kill of the background run removes the paused browser

```bash
playwright test -c <project> login.spec.ts:12 --debug=cli

playwright cli attach tw-<id>
playwright cli -s=tw-<id> step-over
playwright cli -s=tw-<id> pause-at login.spec.ts:20
playwright cli -s=tw-<id> snapshot
playwright cli -s=tw-<id> console
playwright cli -s=tw-<id> requests
playwright cli -s=tw-<id> resume
```
