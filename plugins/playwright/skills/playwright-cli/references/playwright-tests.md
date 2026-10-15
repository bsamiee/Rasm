# [PLAYWRIGHT_TESTS]

`playwright test -c <project>` runs the tests of the `playwright.config.ts` in directory `<project>` from any directory:
- `webServer` of the config starts the served app before the tests run
- Filter arguments are regular expressions over absolute test file paths, `<file>:<line>` selects one test
- Runs write `.last-run.json` and the traces, screenshots, and videos `use` turns on under the config's `outputDir`
- Failures write `error-context.md` there with the error, the failing locator's aria snapshot, and the source
- `trace: 'retain-on-failure'` in the suite's `use` keeps `<outputDir>/<test>/trace.zip` of each failed test for playwright-trace
- List reporter prints each failure as `<file>:<line>:<column> › <describe> › <test>` with the error, call log, and `Error Context` path
- `--shuffle [seed]` runs test files in random order and prints the seed in the run header, the seed repeats one order
- `--last-failed` reruns the failures the previous run recorded in `.last-run.json`, after a run without failures it exits `No tests found`

```bash
playwright test -c <project>
playwright test -c <project> login.spec.ts:12
playwright test -c <project> --shuffle
playwright test -c <project> --last-failed
```

## [01]-[DEBUGGING]

`--debug=cli` runs one worker in the config's headless mode, pauses at the test's first Playwright call, and stops after the first failure:
- First calls include a fixture's call
- Paused runs print `- Run "playwright-cli attach tw-<id>" to attach to this test` and stay in the background while the CLI drives the page
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
