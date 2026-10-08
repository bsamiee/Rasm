---
name: playwright-trace
description: "Use when a Playwright trace needs reading from the command line, covering actions, requests, console, errors, snapshots, screenshots, and attachments."
allowed-tools: Bash(playwright:*)
---

# [PLAYWRIGHT_TRACE]

Read one open trace through `playwright trace` subcommands run from one working directory.

- Source: `playwright test --trace on` writes `<outputDir>/<test>/trace.zip`, `tracing-stop` `$PLAYWRIGHT_MCP_OUTPUT_DIR/traces/trace-<timestamp>.trace`
- State: `open` extracts a zip or links a `.trace` file under `.playwright-cli/trace/` of the working directory, a fixed path no setting moves
- Lifecycle: `open` prints browser, version, title, duration, viewport, and counts, a second `open` replaces the trace, `close` removes it
- Clock: action, request, and console rows share one clock of offsets from trace start
- Identity: `action`, `snapshot`, `screenshot`, `request`, and `attachment` take the number a listing prints, filters keep each number
- Output: `screenshot`, `attachment`, and `snapshot` write unnamed files under `.playwright-cli/` of the working directory, `close` leaves them
- Bodies: `request` prints body paths relative to the working directory, a zip's bodies sit in `.playwright-cli/trace/resources/` until `close`
- CLI traces: `playwright cli` traces hold the CLI's own reads around each command

| [INDEX] | [TASK]                                 | [OPERATION]                                       |
| :-----: | :------------------------------------- | :------------------------------------------------ |
|  [01]   | Failed action                          | `actions --errors-only`                           |
|  [02]   | Action parameters, log, source, phases | `action <action-id>`                              |
|  [03]   | Test error with call log               | `errors`                                          |
|  [04]   | Page state at an action                | `snapshot <action-id> --phase after`              |
|  [05]   | DOM value at an action                 | `snapshot <action-id> -- eval "<expression>"`     |
|  [06]   | Element value at an action             | `snapshot <action-id> -- eval "<fn>" "<locator>"` |
|  [07]   | Session on one snapshot                | `snapshot <action-id> --serve`                    |
|  [08]   | Screencast frame of an action          | `screenshot <action-id> -o`                       |
|  [09]   | Failed request                         | `requests --failed`                               |
|  [10]   | Request headers and bodies             | `request <request-id>`                            |
|  [11]   | Console errors and warnings            | `console --warnings`                              |
|  [12]   | Attached file                          | `attachment <attachment-id> -o`                   |

## [01]-[INVESTIGATION]

Investigations open the trace, read the failed action with its page state, then failed requests and console errors, and close:

```bash
playwright trace open <outputDir>/<test>/trace.zip
playwright trace actions --errors-only
playwright trace action <action-id>
playwright trace errors
playwright trace snapshot <action-id> --phase after
playwright trace snapshot <action-id> --phase after -- eval "el => el.textContent" "<selector>"
playwright trace requests --failed
playwright trace console --warnings
playwright trace close
```

## [02]-[ACTIONS]

`actions` lists the action tree depth first with number, start, title, duration, and `✗` on a failure, the locator or URL on the line below, hooks, fixtures, and steps indented under their parent. `--grep` keeps actions with a title and locator matching a case-insensitive regex, `--errors-only` failed actions. `action` prints one action's start and duration, parameters, return value, error with its call log, timed log, up to five source frames, and the snapshot phases it holds.

```bash
playwright trace actions
playwright trace actions --grep "click|getByTestId"
playwright trace actions --errors-only
playwright trace action <action-id>
```

## [03]-[SNAPSHOTS]

`snapshot` replays an action's DOM snapshot in headless Chromium, target highlighted in the `action` phase, and runs `snapshot`, or `eval` or `screenshot` after `--`:
- `--phase` picks `before`, `action`, or `after` among the phases `action` lists, the first phase the action holds without it
- `eval "<fn>" <target>` takes a CSS selector or a locator string, refs of a printed snapshot belong to that run alone
- `--filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/<name>` saves the `eval` value as JSON or the `screenshot` image
- `screenshot <action-id>` writes the JPEG screencast frame recorded nearest the action, `-o $PLAYWRIGHT_MCP_OUTPUT_DIR/<name>.jpg` names it

```bash
playwright trace snapshot <action-id>
playwright trace snapshot <action-id> --phase after
playwright trace snapshot <action-id> -- eval "document.title"
playwright trace snapshot <action-id> -- eval "el => el.getAttribute('data-testid')" "getByRole('button', { name: '<name>' })"
playwright trace snapshot <action-id> -- eval "document.body.outerHTML" --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/<name>.html
playwright trace snapshot <action-id> -- screenshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/<name>.png
playwright trace screenshot <action-id> -o $PLAYWRIGHT_MCP_OUTPUT_DIR/<name>.jpg
```

`--serve` runs in the background, prints `Serving snapshot at http://localhost:<port>`, and serves until its process stops. `playwright cli open` on the URL replies on the loading page, the next `snapshot` reads the action's snapshot with refs for later commands:

```bash
playwright trace snapshot <action-id> --phase after --serve
playwright cli open http://localhost:<port>
playwright cli snapshot
playwright cli eval "el => el.getAttribute('data-testid')" <ref>
playwright cli close
```

## [04]-[REQUESTS]

`requests` lists each request with number, start, method, status (`ERR` without a response), name, duration, size, and route outcome (`continued`, `fulfilled`, `aborted`, `api`):
- `--grep` keeps URLs matching a case-insensitive regex, `--method` one method in any case, `--status` one status code
- `--failed` keeps status 400 and above and `ERR`
- `request` prints method and URL, status, timing, size, type, route, server, and network error, both header sets, and each body
- Bodies print as a file path, a body without a file inline up to 2000 characters

```bash
playwright trace requests
playwright trace requests --grep "/api/"
playwright trace requests --method POST
playwright trace requests --status 404
playwright trace requests --failed
playwright trace request <request-id>
```

## [05]-[CONSOLE]

`console` lists browser console messages, uncaught page errors, and test stdout and stderr with time, source, level, text, and `<file>:<line>` of a console call:
- `--errors-only` keeps browser errors and stderr, `--warnings` adds browser warnings
- `--browser` keeps browser rows, `--stdio` stdout and stderr, `--grep` text matching a case-insensitive regex
- `errors` prints each test error with its `at <file>:<line>:<column>` frame and message with call log, page errors list under `console`

```bash
playwright trace console
playwright trace console --errors-only
playwright trace console --warnings
playwright trace console --browser
playwright trace console --stdio
playwright trace console --grep "<pattern>"
playwright trace errors
```

## [06]-[ATTACHMENTS]

`attachments` lists each attachment with number, name, content type, and action number, a failed test adds `error-context` with the error, its aria snapshot, and the test source. `attachment` writes one by number, `-o` names the file.

```bash
playwright trace attachments
playwright trace attachment <attachment-id> -o $PLAYWRIGHT_MCP_OUTPUT_DIR/<name>
```
