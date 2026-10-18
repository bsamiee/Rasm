# [TEST_GENERATION]

Planner, generator, and healer agents drive the `playwright-test` server over the suite projects of the test MCP config.

## [01]-[SERVER]

Server loads `tests/typescript/playwright.config.ts`, one project per `tests/typescript/e2e-<app>` directory, named after it with `testDir` at it:
- Tools take no config, `project` of a setup tool, `projects` and `locations` of `test_run`, and the test id of `test_debug` select a suite
- `project` names a `projects[].name`, the first top-level project without it, a missing name fails `Project <name> not found`
- `seedFile` resolves against the project's `testDir`, the config directory, then the client root, a miss fails `seed test not found.`
- Setup without `seedFile` takes the project's first test file with `seed` in its name, or writes `<testDir>/seed.spec.ts` with one empty test
- Suite fixtures start one app instance per worker and set `baseURL`, a setup or debug run starts its own instance
- Setup tools and `test_debug` run one headless worker with no test timeout
- Setup pauses the seed at its end, `### Paused at end of test. ready for interaction` and `### Page state` holding URL, title, and snapshot
- Setup results ending `Error: seed test not found.` or `Error while running the seed test.` hold no page
- Action, assertion, and input tools require `intent`, read tools take none
- `browser_navigate` takes an absolute URL, a path without a scheme gains `https://`
- `browser_wait_for` with `text` or `textGone` waits up to the suite's `actionTimeout`, `time` in seconds up to 30 bounds it or waits alone

Each Streamable HTTP session to the server holds its own test runner, paused test, and generator log:
- Calls of one session queue, each setup, `test_list`, `test_run`, and `test_debug` releases the session's paused test first
- Plugin agents of one Claude Code session share its server session and run one at a time
- Sessions share the server process, its stdout capture and environment, and each project's `outputDir` a run clears at its start

Paused tests serve `browser_*` tools under the config `{ capabilities: ["testing"] }`, which no `PLAYWRIGHT_MCP_*` variable reaches:
- Unnamed outputs (action snapshots as `page-<stamp>.yml`, screenshots without `filename`) go to `.playwright-mcp/` of the client root
- `filename` resolves against the client root and must sit inside it
- `browser_take_screenshot` with `filename` writes the file and returns no image

## [02]-[PLANNING]

Plans sit at `<testDir>/specs/<name>.plan.md` and name each test file at `<testDir>/<suite>/<test>.spec.ts`, both relative to the client root:
- `planner_save_plan` with an absolute or escaping `fileName` fails `Plan file name must be a relative path inside the workspace: <fileName>`
- `suites` hold `name`, `seedFile`, and `tests`, each test `name`, `file`, and `steps`, each step an optional `perform` and an `expect` list
- Generators take each suite's `**Seed:**` line as every scenario's seed

Saved plans hold this form, a step without `perform` reading `-`:

```text
# <name>

## Application Overview

<overview>

## Test Scenarios

### 1. <suite>

**Seed:** `<seedFile>`

#### 1.1. <test>

**File:** `<file>`

**Steps:**
  1. <perform>
    - expect: <expect>
```

Scenarios describe independent user journeys with observable outcomes:
- Forms, buttons, lists, filters, menus, and dialogs
- Empty states, validation, long input, and boundary values
- Persistence across reload, storage, and navigation
- Keyboard, focus, responsive, visual, and accessibility behavior

## [03]-[TEST_FILES]

Generated files hold one `test.describe` named for the suite and one test named for the scenario, both without ordinals:
- `generator_setup_page` opens the log with `plan` before it runs the seed, `generator_read_log` before setup fails `Please setup page using "generator_setup_page" first.`
- Log holds `# Plan`, `# Seed file: <path>` relative to the client root with the seed source, `# Steps`, then `# Best practices`
- `# Steps` holds one `### <intent>` heading and `ts` fence per call that ran without error
- `browser_verify_list_visible` records `toMatchAriaSnapshot` over `page.locator('body')` with a `- list:` of the items
- `browser_verify_text_visible` records `toBeVisible()` on visible elements containing `text` in any case
- Locators and assertions come from the log as recorded, a locator read twice takes one local variable
- Files import `test` from the module the seed imports it from, the fixture opening the page and setting `baseURL`
- `generator_write_test` replaces the whole file, a `fileName` outside every project's `testDir` fails `Test file did not match any of the test dirs: <dirs>`

## [04]-[HEALING]

`test_list` prints one `[id=<id>] [project=<name>] › <file>:<line>:<column> › <title>` row per test, `<file>` relative to `tests/typescript/`, the config directory:
- `test_run` takes `locations` as a folder, a file, or `file:line`, and `projects` as `projects[].name` entries, every project without them
- `test_run` prints each failure with its id, error, call log, and source lines, then `<n> failed` with the failing rows and `<n> passed`
- `test_debug` runs one test by id with a 5 second action timeout and pauses at the first error
- Debug results hold `### Paused on error:` with each error, then `### Page state` with URL, title, logged console errors, and snapshot
- Passing tests under `test_debug` return their pass row with no page

Failures take the correction their cause decides:

| [INDEX] | [CAUSE]        | [CORRECTION]                                                    |
| :-----: | :------------- | :-------------------------------------------------------------- |
|  [01]   | Locator        | Locator `browser_generate_locator` derives from the paused page |
|  [02]   | Timing         | Web-first assertion on the state the page reaches               |
|  [03]   | Data isolation | Test state the test creates through its own fixture             |
|  [04]   | Environment    | Fixture or suite config line that serves the app                |
|  [05]   | Product defect | Test stays failing, the report holds the paused page evidence   |

Plans a test came from change with the steps and outcomes a correction changed.

## [05]-[EVIDENCE]

Server runs disable config reporters, an ordinary run `playwright test -c tests/typescript/e2e-<app>` writes the retained report:
- Suite runs write outputs under `.artifacts/playwright-test/e2e-<app>` and HTML reports under `.artifacts/playwright-report/e2e-<app>`, server runs write outputs under `.artifacts/playwright-test/typescript`
- Failed tests keep `trace.zip` with DOM, aria, and screen snapshots, read through playwright-trace
- `test_list` releases the paused test before an ordinary run of its suite clears the `outputDir` the paused test writes
