---
name: playwright-trace
description: Inspect Playwright trace files from the command line — list actions, view requests, console, errors, snapshots and screenshots.
allowed-tools: Bash(npx:*)
---

# Playwright Trace CLI

Inspect `.zip` trace files produced by Playwright tests without opening a browser.

## Workflow

1. Start with `trace open <trace.zip>` to extract the trace and see its metadata.
2. Use `trace actions` to see all actions with their action IDs.
3. Use `trace action <action-id>` to drill into a specific action — see parameters, logs, source location, and available snapshots.
4. Use `trace requests`, `trace console`, or `trace errors` for cross-cutting views.
5. Use `trace snapshot <action-id>` to get the DOM snapshot, or run a browser command against it.
6. Use `trace close` to remove the extracted trace data when done.

All commands after `open` operate on the currently opened trace — no need to pass the trace file again. Opening a new trace replaces the previous one.

## Commands

### Open a trace

Trace metadata holds browser, viewport, duration, and action and error counts:

```bash
npx playwright trace open <trace.zip>
```

### Close a trace

```bash
npx playwright trace close
```

### Actions

- `trace actions` lists every action as a tree with action IDs and timing
- `--grep` filters by action title as a case-insensitive regex, `--errors-only` keeps failed actions

```bash
npx playwright trace actions

npx playwright trace actions --grep "click"

npx playwright trace actions --errors-only
```

### Action details

`trace action` prints one action's parameters, result, logs, source, and snapshots:

```bash
npx playwright trace action <action-id>
```

The `action` command displays available snapshot phases (before, action, after) and the exact command to extract them.

### Requests

- `trace requests` lists every network request with start time on the `trace actions` clock, method, status, URL, duration, and size
- `--grep` filters by URL pattern, `--method` by HTTP method, `--failed` keeps requests with status 400 or above

```bash
npx playwright trace requests

npx playwright trace requests --grep "api"

npx playwright trace requests --method POST

npx playwright trace requests --failed
```

### Request details

`trace request` prints one request's headers, body, and security details:

```bash
npx playwright trace request <request-id>
```

### Console

- `trace console` lists every console message with stdout and stderr
- `--errors-only` keeps errors, `--browser` the browser console, `--stdio` stdout and stderr, `--grep` messages matching a text pattern

```bash
npx playwright trace console

npx playwright trace console --errors-only

npx playwright trace console --browser

npx playwright trace console --stdio

npx playwright trace console --grep "failed to fetch"
```

### Errors

`trace errors` lists every error with its stack trace and associated action:

```bash
npx playwright trace errors
```

### Snapshots

The `snapshot` command loads the DOM snapshot for an action into a headless browser and runs a single browser command against it. Without a browser command, it returns the accessibility snapshot. An `eval` with a ref targets that element of the accessibility snapshot.

```bash
npx playwright trace snapshot <action-id>

npx playwright trace snapshot <action-id> --phase before

npx playwright trace snapshot <action-id> -- eval "document.title"
npx playwright trace snapshot <action-id> -- eval "document.querySelector('#error').textContent"

npx playwright trace snapshot <action-id> -- eval "el => el.getAttribute('data-testid')" e5

npx playwright trace snapshot <action-id> -- screenshot

npx playwright trace snapshot <action-id> -- eval "document.body.outerHTML" --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/page.html
npx playwright trace snapshot <action-id> -- screenshot --filename=$PLAYWRIGHT_MCP_OUTPUT_DIR/screenshot.png
```

Only three browser commands are useful on a frozen snapshot: `snapshot`, `eval`, and `screenshot`.

### Attachments

`trace attachment` extracts an attachment by its number:

```bash
npx playwright trace attachments

npx playwright trace attachment 1
npx playwright trace attachment 1 -o out.png
```

## Typical investigation

An investigation finds the failed action, reads its details and snapshot, then checks failed requests and console errors:

```bash
npx playwright trace open test-results/my-test/trace.zip

npx playwright trace actions

npx playwright trace actions --errors-only

npx playwright trace action 12

npx playwright trace snapshot 12

npx playwright trace snapshot 12 -- eval "document.querySelector('.error-message').textContent"

npx playwright trace requests --failed

npx playwright trace console --errors-only
```
