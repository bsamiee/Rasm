---
name: history
description: "Use before answering what earlier work ran, printed, failed, found, or decided, after a compaction, and before rerunning a slow command or asking the owner something an earlier session settled, covering atuin threads and history"
---

# [TERMINAL_HISTORY]

Threads hold each Claude Code and Codex session (prompts, replies, each tool call with input, output clipped to 64 KiB, and failure), and history holds each owner and agent shell command with exit code and duration.

## [01]-[READS]

MCP calls answer each need, the CLI adds time, folder, and exit bounds, and DuckDB adds counts:

| [INDEX] | [NEED]                                     | [CALL]                                                                       |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | Recent sessions of a project               | `atuin_threads_list` `cwd: "."`, `since: "today"`                            |
|  [02]   | Session naming a topic, file, error        | `atuin_threads_search` `terms`, `include_subagents: true`                    |
|  [03]   | Owner prompts, or messages holding words   | `atuin_threads_read` `roles: ["user"]`, or `query` over output too           |
|  [04]   | Session end, or record with `← error`      | `atuin_threads_read` `start: -10`, or pages from `start: 0`                  |
|  [05]   | Subagent final output                      | `atuin_threads_read` id from page header `children`, `start: -1`, `limit: 1` |
|  [06]   | Failed or owner commands                   | `atuin_history` `only_failed: true`, `authors` `$all-agent` or `$all-user`   |
|  [07]   | Owner command output, never agent output   | `atuin_output` `ranges: [[-80, -1]]`, `atuin_output_search` `terms`          |
|  [08]   | Commands in a time window, folder, or exit | `atuin search` `--after`, `--before`, `--cwd`, `-e <code>`, `--author`       |
|  [09]   | Counts, rates, durations, and token use    | DuckDB `READ_ONLY` attach, `sqlite` extension loads itself                   |

## [02]-[LIMITS]

- `atuin_threads_search` matches text, tool names, and inputs, never output -> search a command word, then `atuin_threads_read` `query`
- `atuin_threads_read` pages show inputs and 400 characters of each successful output against its tool description, `limit: 1` one message whole
- List, search, and page headers cut `children` to 3, `atuin_threads_list` `include_subagents: true` and ndjson `children_atuin_ids` list each
- Calls in a `cwd_filter` directory or matching `history_filter` keep the tool name alone, `command_filter` matches keep input and failure without output
- Ids: harness session id or unique prefix, last 8 digits of an atuin id, `agent-<id>`, or `latest`
- `atuin search` takes `--filter-mode global --search-mode fulltext` over workspace and fuzzy configuration, `--exclude-exit -1` beside `--exclude-exit 0` to drop running commands, and `-f '{uuid}'` as `atuin_output` id
- CLI and DuckDB runs record themselves in history, `rg -v 'atuin search'` and `command NOT LIKE '%share/atuin%'` drop them, MCP calls record nothing

## [03]-[COUNTS]

```bash
duckdb -c "ATTACH '$HOME/.local/share/atuin/history.db' AS a (TYPE sqlite, READ_ONLY);
SELECT author, to_timestamp(timestamp / 1e9)::DATE AS day, count(*) AS n, count(*) FILTER (exit <> 0) AS failed, round(quantile_cont(duration / 1e9, 0.9), 1) AS p90_s
FROM a.history WHERE deleted_at IS NULL AND exit <> -1 AND timestamp > epoch_ns(now() - INTERVAL 7 DAY) AND command LIKE '%<word>%' AND command NOT LIKE '%share/atuin%' GROUP BY ALL ORDER BY day DESC"
```

- `history` `timestamp` and `duration` count nanoseconds, a running command holds `exit` and `duration` `-1`, `author_kind` `2` marks an agent and `NULL` the owner, and `session` names a hook session, never a thread
- `threads.db` `sessions` holds `harness` (`1` Claude Code, `2` Codex), `parent_kind` `0` for a subagent, `cwd`, `model`, `last_reply`, and `usage_*` tokens with millisecond `started_at`, and messages stay compressed behind `atuin threads show`

## [04]-[TIMELINE]

`atuin_threads_read` shows no durations. One line per prompt, reply, and call, with duration and error text on failure alone:

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
