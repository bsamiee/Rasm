# [TEST_GENERATION]

Native planner, generator, and healer roles use the owning suite's config, fixtures, and evidence.

## [01]-[ASSIGNMENT]

Dispatch names application, config/project, scenarios, writable files, session, application-state scope, and artifact destination.

- Read the config, assigned plan, seed, imported fixtures, and related tests before browser work
- One generator owns a complete feature file; `generator_write_test` replaces that file
- Keep one active Test MCP investigation per process, including inherited subagent connections
- Named CLI sessions isolate browsers, not application data; serialize replays that share configured runtime storage
- Portfolio replays serialize per checkout, including read-only selections
- Every browser launch stays headless through CLI, MCP, and suite configuration

Use `playwright-tests.md` for replay and debugging, `session-management.md` for CLI session ownership.

## [02]-[PLANNING]

Call `planner_setup_page` with the selected `seedFile` and `project`, then explore observed behavior through native browser tools.

- Seed and fixture source establish initial navigation, authentication, and state
- `browser_snapshot` supplies roles, names, states, and transient refs
- Named screenshots supply visual facts absent from snapshots
- Console and network tools expose failures and requests around each action
- `browser_evaluate` reads page values; `browser_run_code_unsafe` runs an `async (page) => {}` function in the test process
- Actions and evaluations supply `intent` describing their purpose
- `browser_tabs` with `action: "list"` identifies the current page

Scenarios describe independent user journeys with observable expected outcomes:

- Forms, buttons, lists, filters, menus, and dialogs
- Empty states, validation, long input, and boundary values
- Persistence across reload, storage, and navigation
- Keyboard, focus, responsive, visual, and accessibility behavior

Submit transient plans through `planner_submit_plan`. Use `planner_save_plan` when generation or maintenance consumes a Markdown file.

- Plan fields are `overview` and `suites`; saved plans add `name` and workspace-relative `fileName`
- Suites name `seedFile` and `tests`; tests name their feature `file`, scenario `name`, and `steps`
- Steps hold optional `perform` and expected outcomes in `expect`
- Test files stay beneath the config's `testDir`, with paths relative to workspace root
- Preserve evidence selected for delivery and release the paused runner through `test_list`

## [03]-[GENERATION]

Run every assigned scenario through the server before writing its feature file:

1. Call `generator_setup_page` with the scenario's `plan`, `seedFile`, and selected `project`
2. Execute each step with a current snapshot ref and its step text as `intent`
3. Record assertions through the appropriate `browser_verify_*` tool
4. Read `generator_read_log` after the last verification
5. Write the complete owned file through `generator_write_test`
6. Preserve selected evidence, call `test_list`, then replay through the suite's native command

- Setup opens the generator journal before running its seed; reading a log before setup fails
- Journal records seed source and successful action/assertion code beneath each `intent`
- List verification records `toMatchAriaSnapshot`; text verification records visible matching elements
- `fileName` is workspace-relative and must fall within a configured `testDir`
- Scenario and suite names come from the plan without ordinals
- Import the fixture required by the test; native `@playwright/test` suits tests that navigate or mount themselves
- Use web-first assertions and semantic locators; a locator read twice takes one local variable
- Group related scenarios in one feature file, adding `test.describe` only for shared suite behavior

## [04]-[HEALING]

Use `test_list` to select test IDs, then `test_run` for the assigned locations and projects.

1. Read each failing spec and use `test_debug` with its listed `id` and `title`
2. Inspect the paused snapshot, console messages, and requests around the failure
3. Correct the demonstrated cause within assigned source ownership
4. Release the paused runner with `test_list`, then replay the corrected scope
5. Update a consumed plan when user-visible steps or outcomes changed

- `test_run` accepts folders, files, and `file:line` locations, with native project names
- `test_debug` pauses on the first error with a five-second action timeout; a passing debug run leaves no page
- `browser_generate_locator` derives a locator from a current snapshot ref
- Network request indices select recorded headers or bodies; console reads can include messages since test start
- Locator, timing, data isolation, environment, and product defects need different corrections
- Product defects remain failing tests; attempt counts do not justify `test.fixme()` or skipped assertions
- Report findings outside assigned source ownership with retained evidence

## [05]-[EVIDENCE]

Test MCP setup, debug, and replay disable configured reporters. An ordinary suite replay supplies the retained report.

- Finish tracing and recording before closing their producer
- `test_list` stops the paused runner and completes teardown before ordinary replay clears configured output
- Await replay exit and server shutdown before retaining reports or deleting disposable task output
- Keep complete HTML reports with attachments and `open: 'never'`
- Return replay result, changed files, and retained evidence paths
- Close only the assigned CLI session; persistent profiles use native `delete-data`
