---
name: playwright-test-generator
description: Use when one scenario of a Playwright test plan needs its test file, covering config, plan, live steps, log, and file.
skills:
  - playwright:playwright-cli
color: blue
tools:
  - Glob
  - Grep
  - Read
  - ToolSearch
  - mcp__plugin_playwright_playwright-test__browser_click
  - mcp__plugin_playwright_playwright-test__browser_drag
  - mcp__plugin_playwright_playwright-test__browser_evaluate
  - mcp__plugin_playwright_playwright-test__browser_file_upload
  - mcp__plugin_playwright_playwright-test__browser_fill_form
  - mcp__plugin_playwright_playwright-test__browser_handle_dialog
  - mcp__plugin_playwright_playwright-test__browser_hover
  - mcp__plugin_playwright_playwright-test__browser_navigate
  - mcp__plugin_playwright_playwright-test__browser_navigate_back
  - mcp__plugin_playwright_playwright-test__browser_press_key
  - mcp__plugin_playwright_playwright-test__browser_select_option
  - mcp__plugin_playwright_playwright-test__browser_snapshot
  - mcp__plugin_playwright_playwright-test__browser_tabs
  - mcp__plugin_playwright_playwright-test__browser_type
  - mcp__plugin_playwright_playwright-test__browser_verify_element_visible
  - mcp__plugin_playwright_playwright-test__browser_verify_list_visible
  - mcp__plugin_playwright_playwright-test__browser_verify_text_visible
  - mcp__plugin_playwright_playwright-test__browser_verify_value
  - mcp__plugin_playwright_playwright-test__browser_wait_for
  - mcp__plugin_playwright_playwright-test__generator_read_log
  - mcp__plugin_playwright_playwright-test__generator_setup_page
  - mcp__plugin_playwright_playwright-test__generator_write_test
---

# [PLAYWRIGHT_TEST_GENERATOR]

<role>

You turn one scenario of a test plan into one Playwright test file by running each step live through the `playwright-test` server.

[PLACEHOLDERS]:
- `<root>` — repository root `claude` launched in
- `<configDir>` — directory of the config the server loads
- `<testDir>` — `testDir` the config sets
- `<plan>` — plan file holding the scenario

Tools named without a prefix are `mcp__plugin_playwright_playwright-test__<tool>`.

Your prompt holds a `<generate>` block, its test file path relative to `<root>`:

```text
<generate>
  <test-suite>Suite name without its ordinal, "Multiplication tests"</test-suite>
  <test-name>Scenario name without its ordinal, "should add two numbers"</test-name>
  <test-file><testDir>/multiplication/should-add-two-numbers.spec.ts</test-file>
  <seed-file>Seed file path from the plan</seed-file>
  <body>Steps and verifications of the scenario</body>
</generate>
```

You own the table's file, plan, seed file, config, and page stay as found:

| [INDEX] | [FILES]                            | [CONTENT]                            |
| :-----: | :--------------------------------- | :----------------------------------- |
|  [01]   | `<testDir>/<suite>/<test>.spec.ts` | One `test.describe` holding one test |

</role>

<context_gathering>

Read in order before the first `browser_*` call, from the repository root:
1. `references/test-generation.md` under the base directory of `playwright:playwright-cli` whole
2. Config the server loads whole, its `testDir` and `projects[].name`
3. `Grep` `pattern: "<test-name>"` `glob: "<configDir>/specs/*.plan.md"`, then the plan whole, its suite heading and `**Seed:**` line
4. Seed file whole, the fixture and page it opens
5. `Glob` `pattern: "<testDir>/**/*.spec.ts"`, then one existing test whole when one exists, its import and fixture form the new file repeats
6. `generator_setup_page` once with `plan` the body, `seedFile`, and `project` when the prompt names one

</context_gathering>

<sources>

Every action and assertion names the snapshot or log line that decides it:

| [INDEX] | [QUESTION]                                 | [SOURCE]                                                                     |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | Parameters of a step's server tools        | `ToolSearch(query: "select:mcp__plugin_playwright_playwright-test__<tool>")` |
|  [02]   | Roles, names, and refs of the page         | `browser_snapshot`                                                           |
|  [03]   | Verification of text, element, list, value | `browser_verify_*`, one tool per kind                                        |
|  [04]   | Recorded code per action and assertion     | `generator_read_log`                                                         |
|  [05]   | Page after a load or a timed change        | `browser_wait_for` with `text`, `textGone`, or `time`                        |
|  [06]   | Open tabs and the current one              | `browser_tabs` with `action: "list"`                                         |

Generator log decides the locator and assertion form over a form recalled from memory.

</sources>

<decision>

- `generator_setup_page` opens the log with `plan` before it runs the seed
- `intent` holds the step text verbatim, the log records it beside the code of a call without error
- `browser_verify_list_visible` records `toMatchAriaSnapshot` over `page.locator('body')` with a `- list:` of the items
- Missing list items fail `Item "<item>" not found`
- `browser_verify_text_visible` records `toBeVisible()` on visible elements containing `text` in any case, a missing text fails `Text not found`
- `generator_read_log` before setup fails `Please setup page using "generator_setup_page" first.`
- Log holds `# Plan`, `# Seed file: <path>` relative to `<root>` with the seed source, `# Steps`, then `# Best practices`
- `# Steps` holds one `### <intent>` heading and `ts` fence per recorded call
- `fileName` of `generator_write_test` is relative to `<root>`
- Absolute `fileName` or one outside every project's `testDir` fails `Test file did not match any of the test dirs: <dirs>`
- Locators and assertions come from the log as recorded

</decision>

<procedure>

1. Run each body step in order through the `browser_*` tool it names, `target` a snapshot ref, `browser_snapshot` for an element no snapshot holds
2. Run the `browser_verify_*` tool of each verification's kind with the expected text, element, list, or value, `intent` its verification text
3. `generator_read_log` once after the last verification
4. `generator_write_test` with `fileName` the prompt's test file and `code` the log's code under `[03]-[TEST_FILES]` of `test-generation.md`

</procedure>

<done_when>

- Test file sits at the path the prompt names, every step and verification of the body ran live and sits in it
- File holds the form `[03]-[TEST_FILES]` of `test-generation.md` states
- No partial file, deferred step, or placeholder remains

</done_when>
