---
name: audit
description: "Use when the owner asks for a debug or quality pass over paths, or work depends on the state of code this session has not read, covering shares of paths, headless readers on Neon, Sol, Astra, and Claude, one findings schema, overlap between providers, and triage"
argument-hint: "[readers] [paths]"
---

# [DELEGATE_AUDIT]

Audits split paths into shares, give each share to one fresh reader that reads it whole, and return findings in one schema this session triages. Arguments are a reader count and paths. `delegate:council` reviews a change, an audit reads code as it stands.

[SCHEMA]:
- [01]-[FINDINGS_SCHEMA](findings.schema.json): Fields of every reader's answer

## [01]-[SHARES]

`rg --files <paths>` is the file list, and every file sits in exactly one share:
- Workspace share: files at the checkout root, `.config/`, every project's manifests and build settings, and `docs/` without `docs/research`
- Project share: one project, the first folder holding a manifest, split by its category folders once it passes its size limit
- Size: a share holds at most 1 MB of text for Neon and Sol and 500 KB for Astra, half of each window, measured by `xargs wc -c < <share>`
- Count: the reader count is the number of shares, raised when a share would pass its size limit
- Files: each share is a file of paths, one per line, at `.artifacts/audit/<run>/share-<n>.txt`

## [02]-[READERS]

| [INDEX] | [READER] | [RUNS_AS]                             | [SERVES]                                                     |
| :-----: | :------- | :------------------------------------ | :----------------------------------------------------------- |
|  [01]   | Neon     | `agy` on `gemini-4-neon` at `xhigh`   | Default reader, recalls most of a codebase and guesses least |
|  [02]   | Sol      | `codex exec` on `gpt-6.2-sol`         | Second reader, fewest wrong findings                         |
|  [03]   | Astra    | `codex exec` on `gpt-6.2-astra`       | Hunt where a miss costs most, many false alarms              |
|  [04]   | Claude   | Fresh subagent given brief and schema | Share that needs this session's tools or MCP servers         |

Readers find different defects, so a second provider adds coverage:
- Split: each share goes to one reader, alternating providers when the owner names more than one
- Overlap: each share goes to one reader per provider, for code where a miss costs most
- Refute: a finding one provider alone reported on an overlapped share goes to the other to refute from code it reads, and stands unrefuted

## [03]-[RUN]

Each reader gets its share and the schema alone, never an earlier round's findings or this session's conclusions:

```text
Read every file listed in <share> whole, in this checkout, before judging any of them.

Report what is wrong in those files: behavior that is wrong today, behavior a named input or state makes wrong, a setting that contradicts another file or its tool, text describing something that no longer exists, and code that breaks a rule of this repository's AGENTS.md. Leave out formatting a formatter owns and anything you cannot place on a line. Each finding states the trigger and what is then observed, a command or read that shows it and whether you performed it, and the smallest change that removes the cause.

Change no file and start no agents. Put every listed file you did not read whole in `unread`, and observations that are not findings in `notes`.
```

Each share runs through `mcp__delegate__run`, loaded once with `ToolSearch` `select:mcp__delegate__run`, Neon first and Sol or Astra second. Each call answers at once with its runner agent's id, and the run's report arrives as that agent's notice with the answer file's size and its `findings` and `unread` counts:

```json
{ "command": "agy -p \"$(cat <brief>)\" --disable-slash-commands --model gemini-4-neon --effort xhigh --output-format json --print-timeout 60m --json-schema <schema> --settings '{\"permissions\":{\"deny\":[\"write_file(*)\"]}}' > .artifacts/audit/<run>/answer-<n>-neon.json", "description": "audit <run> share <n> neon", "limit": "70 minutes" }
{ "command": "codex exec --sandbox read-only -m <model> -c model_reasoning_effort='\"xhigh\"' -c service_tier='\"default\"' --output-schema <schema> -o .artifacts/audit/<run>/answer-<n>-<model>.json \"$(cat <brief>)\" </dev/null", "description": "audit <run> share <n> <model>", "limit": "90 minutes" }
```

Runs and Claude subagents share the session's concurrent agent limit, which a refusal's `agents:` line states, so shares past it launch as reports land.

`agy` holds its answer in `.structured_output` and Codex writes the answer itself, so one read joins both with their reader:

```bash
jq -n '[inputs | (.structured_output // .).findings[] + {reader: (input_filename | split("/") | last)}] | sort_by(.path, .line)' .artifacts/audit/<run>/answer-*.json
jq -n '[inputs | (.structured_output // .).unread[]] | unique' .artifacts/audit/<run>/answer-*.json
```

## [04]-[TRIAGE]

- Unread paths form the next share, and an audit with unread paths is unfinished
- Findings sharing a path and line are one finding
- Each finding is read against its file and its `verify` step run before any fix, and one failing either joins a pool file with its reader and doubt
- Pool files sit at `.artifacts/audit/<run>/questionable.md`, each finding read and run again after the last report, then fixed or discarded with its evidence
- Fixes go by reach: one file here, one project to one fresh writer (a subagent, a `delegate:codex` Task, a `delegate:antigravity` Build) briefed with its confirmed findings
- Writers run in place, each file assigned to one of them
- Use `delegate:antigravity` and `delegate:codex` for each harness's run options and result checks
