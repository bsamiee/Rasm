---
name: codex-orchestration
description: Compose substantial multi-source tool work in Codex Code Mode and coordinate fresh workers through shared briefs and scoped handoffs. Use when independent retrieval needs a join or reduction, or parallel assignments share requirements and feed an integration owner.
---

# [CODEX_ORCHESTRATION]

## [01]-[COMPOSITION]

Choose execution boundaries from data dependencies before batching calls. Keep mechanical retrieval and transformation inside Code Mode; return to model judgment when evidence decides a later action.

Compose independent reads with `Promise.allSettled` and inspect every result:
- Preserve input identity and source location beside each result
- Record rejected promises as failures
- Inspect fulfilled responses for tool-reported failure through each live output schema, including MCP `isError`
- Keep failed inputs distinct from successful empty results
- Stop dependent work when a required input fails

Transform, join, and reduce results inside `functions.exec`. Return evidence needed for the next decision and failed inputs. Keep quotations, identifiers, conflicting evidence, and source locations required to support that decision. Retain raw responses by input identity when later decisions can need evidence omitted from output.

Order dependent operations and writes to shared resources. Authorized mutations can run inside Code Mode when live tool instructions permit them. Calls requiring a dedicated boundary keep that boundary.

## [02]-[CODE_MODE]

Live tool declarations govern available operations, arguments, return types, and call restrictions. Filter `ALL_TOOLS` by relevant names or descriptions to discover deferred nested tools. Read matching declarations before calling them.

`functions.exec` evaluates raw JavaScript in a fresh V8 isolate. Access external systems through `tools`; Node, filesystem, network, console, and module imports are unavailable inside the isolate.

Await every tool promise before cell completion. Unawaited work disappears when the isolate ends. Emit results with runtime output helpers.

Cell variables disappear at completion. Use `store` and `load` for serializable results needed by later cells in the same session. Session state is not durable storage.

Preserve operation handles before yielding. Outer `cell_id` resumes a running Code Mode cell through `functions.wait`; nested shell `session_id` resumes a shell process through `write_stdin`. Use `functions.wait` only after `functions.exec` reports a running cell. A running shell process can outlast a completed cell.

Use live yield and wait controls within communication requirements. A first-line `// @exec: {...}` pragma controls cell yield interval and output budget. `yield_control()` exposes accumulated output while a cell continues running.

Direct collaboration namespace APIs run outside `functions.exec` when the current runtime excludes them from nested tools. Use each API on its exposed surface.

## [03]-[SHARED_CONTEXT]

When parallel assignments share requirements or decisions, publish one authoritative brief on the native message board when available. Coordinator owns the brief:
- Goal and expected behavior
- Requirements with sources
- Constraints and interfaces
- Accepted decisions with sources
- Open questions
- Inferred intent marked separately from stated requirements

Give fresh workers self-contained assignments with `fork_turns="none"`:
- Objective and owned files or responsibility
- Relevant facts and constraints
- Required upstream handoffs
- Expected output
- Brief post ID and instruction to read the full post before work
- Instruction to preserve other agents' edits

Pass a brief's post ID instead of repeating its full text. When board tools are unavailable, include the same brief in each fresh assignment. Forward authoritative context to nested assignments.

Give workers only obligations they can satisfy from supplied inputs and upstream handoffs. One integration owner combines outputs and owns downstream decisions. Workers producing drafts must finish without waiting for later reviews of those drafts.

Keep candidate findings and verdicts in subject threads separate from the brief. Give independent reviewers requirements, raw scope, and available evidence without candidate findings or expected verdicts.

When authoritative context changes, publish a complete replacement brief and pass its new post ID to affected assignments. Notify agents whose dependencies changed. Routine progress belongs in assignment reports.

## [04]-[MESSAGE_BOARD]

Use message-board tools only when exposed by the current runtime. Tool namespace comes from the host.

Posts return metadata with message IDs. Thread previews do not replace full posts. Read full text with `read_post`, continuing at `next_offset_chars` while less than `n_chars`.

Subscribe to relevant brief or subject threads when replies affect ongoing work. Notifications reach running turns only; missed notifications are not saved. `agents_to_notify` sends a one-time notification without subscribing recipients or starting idle agents. Assignments must supply required post IDs even when a notification was sent.
