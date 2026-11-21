---
name: history
description: "Use before answering any question about earlier work (what a command printed, returned, or took, what a session or subagent found, what the owner decided), after a compaction, and before rerunning a slow command or asking the owner something already settled, covering atuin history and threads by MCP and CLI"
---

# [TERMINAL_HISTORY]

Atuin records each shell command of the owner and of every agent as history, and each Claude Code and Codex session with its tool calls as a thread, so earlier work is read back in place of rerun, re-derived, or asked.

## [01]-[MOMENTS]

History reads come before these steps:
- Task naming earlier work (`again`, `still`, `continue`, `last time`, `we decided`) -> owner requests of earlier threads through `roles: ["user"]`
- Session after a compaction or resume -> this session's thread
- Rerun of a slow or side-effecting command to see its output again -> earlier tool result in this session's thread
- Question to the owner an earlier session can answer -> thread search before asking
- Failure that looks familiar -> failed history rows, then the thread that fixed it
- Findings of an ended subagent -> its thread through the parent's `children_atuin_ids`

## [02]-[RECORDS]

What each store holds decides where a fact sits:
- Thread tool results hold what the agent received, after a `| tail` and clipped to 64 KiB, so output worth recalling goes to a file
- `history_filter` and `cwd_filter` matches keep a call's tool name alone, so an empty input and result mean a filter dropped them
- `atuin_output` and `atuin_output_search` read the owner's interactive panes alone, agent output sits in thread tool results
- History lacks Claude Code background and Monitor commands, running Codex commands, and commands spanning a daemon restart, threads hold each
- Claude Code deletes transcripts after `cleanupPeriodDays`, threads outlive them and `atuin resume <id> --print` restores one
- Live capture lags seconds behind a running call

## [03]-[FINDING]

Search and read match different text:
- Thread search matches messages, tool names, and inputs, never output -> search a command word, then `atuin_threads_read` `query` matches output
- Thread search and list leave out subagents, which hold most findings, unless `include_subagents: true`
- MCP tools read global history, the CLI follows configuration (`workspace` scope, fuzzy match), so CLI reads pass `--filter-mode global --search-mode fulltext`
- `atuin_history` pads few hits with fuzzy matches, `'<word>` asks for an exact word and `^<text>` a prefix
- Ids: a harness session id or its unique prefix, exactly the last 8 digits of an atuin id, `agent-<id>`, or `latest`, the leading digits being a time many threads share
- `$CLAUDE_CODE_SESSION_ID` in a subagent and Codex hook ids name the parent, whose id a subagent thread holds as `parent.session_id`
- MCP tools take no time window, exit code, folder, or intent, `atuin search` takes `--after`, `--exit`, `--cwd`, and `-f` fields `{author}`, `{intent}`, `{session}`, `{uuid}` its help omits
- `-f '{duration}'` rounds to whole units, the history database below holds nanoseconds for an exact duration

## [04]-[READS]

`atuin threads show <id> --style ndjson` prints one `session` line, then one line per message with `content` items `text`, `tool_call`, and `tool_result`. A Claude Code `tool_call` holds `input` as a JSON string, a Codex one as script source, and a `tool_result` names its call in `tool_use_id`.

Reads, in order: failed agent commands in a window, every tool call of one session, output of calls whose input holds `<words>`, subagent ids of a session, longest commands of a day:

```bash
atuin search --filter-mode global --search-mode fulltext --author '$all-agent' --after '<time>' --exclude-exit 0 -f '{time}\t{exit}\t{author}\t{directory}\t{command}' '<words>'
atuin threads show <id> --style ndjson | jq -c 'select(.kind == "message") | .timestamp as $at | .content[]? | select(.type == "tool_call") | {$at, name, input: (.input | fromjson? // .)}'
atuin threads show <id> --style ndjson | jq -rn --arg words '<words>' '[inputs | .content[]?] | (map(select(.type == "tool_call" and (.input | contains($words))) | .id)) as $ids | .[] | select(.type == "tool_result" and (.tool_use_id | IN($ids[]))) | .content'
atuin threads show <id> --style ndjson | jq -r 'select(.kind == "session") | .children_atuin_ids[]'
sqlite3 -readonly "$HOME/.local/share/atuin/history.db" "select datetime(timestamp / 1e9, 'unixepoch', 'localtime'), duration / 1e9, exit, author, command from history where timestamp > unixepoch('now', '-1 day') * 1e9 order by duration desc limit 10"
```

`atuin resume <id> --print` prints the folder change and resume command an agent shell runs, `--fork` continues a copy, and `--in <harness>` writes the thread into another harness as a new session.
