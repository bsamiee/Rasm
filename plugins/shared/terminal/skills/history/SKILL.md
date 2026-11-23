---
name: history
description: "Use before answering what earlier work ran, printed, failed, found, or decided, after a compaction, and before rerunning a slow command or asking the owner something an earlier session settled, covering atuin threads and history"
---

# [TERMINAL_HISTORY]

Threads hold each Claude Code and Codex session (prompts, replies, each tool call with input, output clipped to 64 KiB, and failure), and history holds each owner and agent shell command with exit code and duration.

## [01]-[READS]

MCP calls answer each need:

| [INDEX] | [NEED]                               | [CALL]                                                                       |
| :-----: | :----------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | Recent sessions of a project         | `atuin_threads_list` `cwd: "."`, `since: "today"`                            |
|  [02]   | Session naming a topic, file, error  | `atuin_threads_search` `terms`, `include_subagents: true`                    |
|  [03]   | Owner prompts and corrections        | `atuin_threads_read` `roles: ["user"]`                                       |
|  [04]   | Session end and last reply           | `atuin_threads_read` `start: -10`                                            |
|  [05]   | Session record, `← error` on failure | `atuin_threads_read` pages from `start: 0`                                   |
|  [06]   | Subagent final output                | `atuin_threads_read` id from page header `children`, `start: -1`, `limit: 1` |
|  [07]   | Message holding words, output too    | `atuin_threads_read` `query`                                                 |
|  [08]   | Failed commands across sessions      | `atuin_history` `only_failed: true`, `authors: ["$all-agent"]`               |
|  [09]   | Owner's own commands                 | `atuin_history` `authors: ["$all-user"]`, `'<word>` exact, `^<text>` prefix  |
|  [10]   | Owner command output                 | `atuin_output` `ranges: [[-80, -1]]`, `atuin_output_search` `terms`          |

## [02]-[LIMITS]

- `atuin_threads_search` matches text, tool names, and inputs, never output -> search a command word, then `atuin_threads_read` `query`
- `atuin_threads_read` pages show inputs and outputs, against its tool description
- Page headers cut `children` short, `atuin_threads_list` `include_subagents: true` and `children_atuin_ids` of an ndjson `session` line list each
- Calls in a `cwd_filter` directory or matching `history_filter` keep the tool name alone, `command_filter` matches keep input and failure without output
- `atuin_output` holds the owner's interactive commands alone, agent output sits in thread tool results
- Ids: harness session id or unique prefix, last 8 digits of an atuin id, `agent-<id>`, or `latest`
- `atuin search --filter-mode global --search-mode fulltext` adds `--after`, `--before`, `--cwd`, and `--exclude-exit 0`, as configuration scopes the CLI to the workspace

## [03]-[TIMELINE]

`atuin_threads_read` shows 400 characters of each successful result and no durations. One line per prompt, reply, and call, with duration and error text on failure alone:

```bash
atuin threads show <id> --style ndjson | jq -rn --argjson w 200 '
  def secs: (.[0:19] + "Z" | fromdateiso8601) + ((.[20:23] | tonumber? // 0) / 1000);
  def line: tostring | gsub("\\s+"; " ") | .[0:$w];
  [inputs | select(.kind == "message")] as $ms
  | ($ms | map(.timestamp as $t | .content[]? | select(.type == "tool_result") | {key: .tool_use_id, value: {t: $t, e: .is_error, c: .content}}) | from_entries) as $res
  | $ms[] | .timestamp as $t | .role as $r | .content[]?
  | if .type == "text" and ($r == "user" or $r == "assistant") then "\($t[11:19]) \($r): \(.text | line)"
    elif .type == "tool_call" then $res[.id] as $x
      | "\($t[11:19]) → \(.name) \(.input | fromjson? // . | line)"
        + (if $x then " \((($x.t | secs) - ($t | secs)) * 10 | round / 10)s" else "" end)
        + (if $x.e then " ✗ \($x.c | line)" else "" end)
    else empty end'
```

`atuin resume <id> --print` prints the folder change and resume command, `--fork` continues a copy, and `--in <harness>` writes the thread into another harness.
