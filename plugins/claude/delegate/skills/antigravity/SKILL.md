---
name: antigravity
description: "Use when work needs what Google's agent does natively and this one does not: watching video or hearing audio, judging or generating images, reading scanned or long PDFs by sight, Google-grounded web research, cheap bulk passes over many files, a long build left running on Google's quota, or a second implementer, reviewer, or prose writer from another model family, covering headless `agy` runs, model choice, briefs, structured results, and result checks"
---

# [DELEGATE_ANTIGRAVITY]

`agy -p` runs Google's Antigravity agent headless in the current folder. It reads `~/.gemini/AGENTS.md` on every run, so a brief states the job and never the tools or rules that file holds.

## [01]-[JOBS]

Every job runs on `gemini-4-neon`:

| [INDEX] | [JOB]               | [WHAT_IT_DOES]                                                                | [EFFORT] |
| :-----: | :------------------ | :---------------------------------------------------------------------------- | :------- |
|  [01]   | Watch and listen    | Video and audio files up to 100 MB read directly: events, cuts, speech, sync  | `high`   |
|  [02]   | Look                | Images, renders against references, scanned pages, boards, UI screenshots     | `high`   |
|  [03]   | Make images         | Images generated and edited by Google's image model, copied to a named path   | `high`   |
|  [04]   | Research            | Google Search answers with cited links, a second search index                 | `high`   |
|  [05]   | Bulk                | One small judgment over many files: captions, alt text, tagging, OCR cleanup  | `high`   |
|  [06]   | Build               | Bounded change its brief states, proven by a command it runs                  | `xhigh`  |
|  [07]   | Grind               | Long build or fix loop under `/goal`, against a done condition and state file | `xhigh`  |
|  [08]   | Review              | Defects in a diff or module as `path:line` findings                           | `xhigh`  |
|  [09]   | Draft               | Prose for outside readers from supplied facts: announcement, README, notes    | `high`   |
|  [10]   | Hard second opinion | One difficult design or security question for a different model family        | `xhigh`  |

Mode is this session's call per run: `read` changes no file, `write` edits files in place in this checkout. Build, Grind, Make images, and Draft write, every other job reads, and findings this session confirmed go to a fresh write run.

Build and Grind take parallel work and problems this session stalled on. Quick questions, edits to files this session is editing, and anything waited on stay here: a run takes minutes.

## [02]-[RUN]

Every `agy` command runs through `mcp__delegate__run`, loaded once with `ToolSearch` `select:mcp__delegate__run`:

```json
{
    "command": "agy -p \"$(cat <brief>)\" --disable-slash-commands --model gemini-4-neon --effort <effort> <mode> --output-format json --print-timeout 60m > <result.json>",
    "description": "<job> <subject>",
    "limit": "70 minutes"
}
```

- Limit: the run ends at `limit`, so it sits above `--print-timeout`, which lets `agy` answer first
- Folder: run from the folder holding the files, `--add-dir <dir>` adds another with read and write access
- Skills: runs inside this checkout load its skills, a body past 24,000 bytes is cut, so a build brief names its coding skill file to read
- Slash: `-p` reads a prompt opening with `/` as a slash command, so a brief opening with a path needs `--disable-slash-commands`
- Tier: `/run-workflow` agents take `--model` unless the task names `flash_lite` (Gemini 3.5 Flash-Lite), `flash`, or `pro` (both Gemini 3.8 Flash)
- Results: `~/.gemini/antigravity-cli/brain/<conversation_id>/.system_generated/tasks/` holds workflow agent results, one failure cancels the rest
- Model: `agy models > <models.txt>` lists slugs, a base name (`gemini-4-neon`) takes medium effort
- Effort: `--effort <level>` picks a base name's variant, Neon and Hydrogen alone have `xhigh`, Orchestrator slugs take none
- Orchestrator: `gemini-pro-orchestrator` slugs plan on their named model (Carbon when unnamed) and edit on Gemini 4 Hydrogen
- Refusal: an unknown slug, an `--effort` the model lacks, or one conflicting with the slug's suffix exits 1 with no fallback
- Quota: `agy -p /usage --output-format json > <usage.json>` costs no turn and returns `remaining_fraction` per model group for 5h and weekly windows
- Data: `--json-schema '<schema>'` or a schema file returns the answer in `.structured_output`, the schema root is `"type":"object"`
- Read: `--settings '{"permissions":{"deny":["write_file(*)"]}}'` refuses file writes and keeps shell tools (`ffprobe`, `mutool`), which can still write
- Look-only: adding `"command(*)"` to that list refuses every shell command
- Write: no `--settings`, the brief alone bounds the run's task
- Timeout: without `--print-timeout` a run waits for every background task, a grind takes its length in hours
- Follow-up: `--conversation <conversation_id>` continues that run with its context, `-c` the last run of the current folder

One process answers many prompts with `--input-format stream-json --output-format stream-json < <prompts.jsonl>`: each line `{"event":"user","message":{"content":"<prompt>"}}` returns one `result` event, and the end of the file ends the run.

Slash commands opening a `-p` prompt start a turn without `--disable-slash-commands`:
- `/goal <objective>` re-prompts the agent to audit its work until it answers `<!-- GOAL_COMPLETE -->`, with no turn limit
- `/boost <task>` sends a coding or investigation task to an agent until its work checks out
- `/swarm <task>` runs agents on the run's model, each in its own conversation, over a shared message board
- `/run-workflow <task>` has the agent write a Python script that runs agents in parallel or in stages and merges results

## [03]-[BRIEF]

- Material first, question last: paths to the files, then what is wanted from them
- Answer form: a table of timecodes, `path:line` findings, a file written to a named path, a verdict with its evidence and doubts
- Answers this session reads as data take a `--json-schema` in place of a described form
- For a visual judge, the artifact and the reference alone, never the plan or the code that produced it
- For video holding small text (terminal, code, hashes), a copy cropped to that region and upscaled 2x: characters read wrong at 720p
- For a write, the task and the command that proves it done, and no commit
- For a review, the diff range and its intent alone, never findings this session already holds, as `agy` has no review command of its own
- For a draft, the facts and the reader, never an earlier draft
- For a grind, the done condition, a minimum number of passes, and a state file it rewrites each pass with the open problems
- Headless runs ask nothing: `agy` decides each open choice itself, so the brief states every choice that matters

## [04]-[RESULT]

Reports and the files they name decide a run's result, and `agy` exits with these codes:

| [INDEX] | [EXIT] | [MEANS]                                                                            |
| :-----: | :----- | :--------------------------------------------------------------------------------- |
|  [01]   | `0`    | Turn finished, refused tool actions included                                       |
|  [02]   | `1`    | Rejected before any turn: model, effort, schema, or `--settings` key               |
|  [03]   | `2`    | Conflicting flags, or a slash command on stream-json input                         |
|  [04]   | `3`    | Model failure or `--print-timeout` expiry, with an `AGY_ERROR` JSON line on stderr |

- Good results have exit `0`, field `status` `SUCCESS`, no `finished_reason`, `denied_actions` at 0 items, and a `response` of some length
- Timed-out runs return a partial `response` with `finished_reason` `print_timeout`
- Answers this session uses are read from the result file, and the report alone judges whether a run worked
- Files it reports writing exist at their paths and open
- Generated images sit in the run's artifacts until a shell or file write copies them, which a look-only run refuses
- Claims about code, documents, or audio are read against the source before they are used
- Neon checks code it changes and guesses about code it only describes: a build is judged by its diff and proving command rerun here
