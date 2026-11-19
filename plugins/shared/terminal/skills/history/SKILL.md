---
name: history
description: "Use when a task needs a past command, its output, or an agent session transcript, covering this session, its subagents, and earlier sessions through atuin history and threads by MCP and CLI"
---

# [TERMINAL_HISTORY]

Atuin records each shell command of the owner and of every agent as history, and each Claude Code and Codex session as a thread. Its MCP tools find commands and sessions by words, and the `atuin` CLI reads by time window, exit code, session id, and whole tool input and output.

- Source: files under `~/.claude/projects` and `~/.codex/sessions` are threads, a thread read replaces every read of a transcript file
- Expiry: Claude Code deletes transcripts after `cleanupPeriodDays` (14 in `~/.claude/settings.json`), threads outlive them
- Session: `$CLAUDE_CODE_SESSION_ID` and `$CODEX_THREAD_ID` hold the running session's id, a Claude Code subagent holds its parent's
- Id: a harness session id matches by unique prefix, an atuin id by its last 8 digits, its first digits are a time many threads share
- Scope: `atuin search` follows the configured filter and fuzzy mode, `--filter-mode global --search-mode fulltext` reads all history literally
- Author: `--author` takes `claude-code`, `codex`, `$all-agent`, or `$all-user`, a Claude Code row holds its Bash `description` as intent
- Gap: history holds no Claude Code background Bash or Monitor command and no Codex command left running, a thread holds each as a tool call
- Output: `atuin_output` reads what the owner's interactive shell printed, output of an agent command is its thread tool result
- Search: thread search matches messages, tool names, and tool inputs, a word a command printed matches no thread
- Subagents: thread search and list leave out subagents, which hold most findings, so a search for a finding or result sets `include_subagents: true`
- Roles: `roles: ["user"]` on thread search and read keeps the owner's requests and decisions
- Size: `atuin threads show` prints a whole thread, so `--style ndjson` goes through a `jq` filter

| [INDEX] | [TASK]                                            | [OPERATION]                                               |
| :-----: | :------------------------------------------------ | :-------------------------------------------------------- |
|  [01]   | Command by words, author, or failure              | `atuin_history`                                           |
|  [02]   | Command by time window, exit code, or directory   | `atuin search` with `--after`, `--exit`, `--cwd`          |
|  [03]   | Command by intent, duration, or count             | `atuin search -f '{intent}'`, `sqlite3` on the history db |
|  [04]   | Session by topic, file, or error it met           | `atuin_threads_search`                                    |
|  [05]   | Sessions of a directory or since a time           | `atuin_threads_list` with `cwd`, `since`                  |
|  [06]   | Sessions by tokens, model, or kind                | `atuin threads list --style ndjson \| jq`                 |
|  [07]   | Conversation of one session                       | `atuin_threads_read`, `query` or `start: -<n>` when long  |
|  [08]   | Tool inputs and outputs of one session            | `atuin threads show <id> --style ndjson`                  |
|  [09]   | This session before a compaction                  | `atuin threads show "$CLAUDE_CODE_SESSION_ID"`            |
|  [10]   | Subagents of a session                            | `children_atuin_ids` of its session line                  |
|  [11]   | Session continued, forked, handed off, or expired | `atuin resume <id> --print`                               |
|  [12]   | Commands or messages as another session runs      | `atuin history tail`, `atuin threads tail --style ndjson` |

## [01]-[HISTORY]

`atuin search` prints one row per command, a multiline command across rows, and `--limit` keeps its newest rows. `-f` takes `{time}`, `{exit}`, `{duration}`, `{author}`, `{intent}`, `{session}`, `{directory}`, `{uuid}`, and `{command}`, and `{uuid}` is the history id `atuin_output` reads.

- Window: `--after` and `--before` take a date, a time, or a phrase (`'yesterday 3pm'`), `--reverse` makes `--limit` keep oldest rows
- Exit: `--exit <code>` keeps one code, `--exclude-exit 0` keeps every failure
- Place: `--cwd <dir>` keeps one directory, `--filter-mode workspace` keeps commands run inside the current repository
- Query: an empty query `''` matches every command of the other filters
- Count: `--include-duplicates` keeps each run of a repeated command, which a count needs
- Stats: `atuin stats` takes no author filter and counts agent commands with the owner's, a count per author goes through `--author`

```bash
atuin search --filter-mode global --search-mode fulltext --author '$all-agent' --after '<time>' --before '<time>' --exclude-exit 0 \
    -f '{time}\t{exit}\t{duration}\t{author}\t{directory}\t{command}' '<words>'
atuin search --filter-mode global --search-mode fulltext --author claude-code --after '<time>' -f '{uuid}\t{intent}\t{command}' '' | rg -i '<intent words>'
```

History database of `atuin info` `client db path`, `timestamp` and `duration` in nanoseconds, for a sort or count no flag gives:

```bash
sqlite3 -readonly ~/.local/share/atuin/history.db "select datetime(timestamp / 1e9, 'unixepoch', 'localtime'), duration / 1e9, exit, author, command
    from history where timestamp > unixepoch('now', '-1 day') * 1e9 order by duration desc limit 10"
```

## [02]-[THREADS]

`atuin threads show <id> --style ndjson` prints one `session` line, then one line per message with `role`, `timestamp`, and `content` items, `text`, `tool_call`, and `tool_result` among them. A Claude Code `tool_call` holds `input` as a JSON string, a Codex one as script source, and a `tool_result` names its call in `tool_use_id`. `atuin threads list --style ndjson` prints one line per session with `model`, `tokens`, `parent_kind` (`subagent`, `continuation`), and `root_atuin_id`.

Thread reads, `<id>` a session id, an atuin id, or `latest`:
1. Every tool call of a session with its time and input, background and Monitor commands among them
2. Output of each call with input containing `<words>`, as the agent received it
3. Atuin ids of a session's subagents, each a thread `atuin_threads_read` and `atuin threads show` take
4. Output tokens and session count of each session with its subagents, largest first

```bash
atuin threads show <id> --style ndjson | jq -c 'select(.kind == "message") | .timestamp as $at | .content[]? | select(.type == "tool_call") | {$at, name, input: (.input | fromjson? // .)}'
atuin threads show <id> --style ndjson | jq -rn --arg words '<words>' '[inputs | .content[]?] | (map(select(.type == "tool_call" and (.input | contains($words))) | .id)) as $ids | .[] | select(.type == "tool_result" and (.tool_use_id | IN($ids[]))) | .content'
atuin threads show <id> --style ndjson | jq -r 'select(.kind == "session") | .children_atuin_ids[]'
atuin threads list --style ndjson | jq -sc 'group_by(.root_atuin_id // .atuin_id) | map({root: (.[0].root_atuin_id // .[0].atuin_id), sessions: length, output: (map(.tokens.output // 0) | add)}) | sort_by(-.output) | .[:10][]'
```

## [03]-[RESUME]

`atuin resume <id>` replaces its shell with the harness, so an agent shell passes `--print` and receives the `cd` and resume command.

- Same harness: `atuin resume <id> --print` continues a session, `--fork` writes a copy to continue while its original stays open elsewhere
- Other harness: `--in <harness>` writes the thread as a new session there on every call, `--print` included
- Expired: `--print` writes back a transcript Claude Code deleted, without images, with reasoning as a marker and tool inputs and outputs clipped to 64 KiB
