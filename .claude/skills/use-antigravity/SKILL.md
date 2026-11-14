---
name: use-antigravity
description: "Use when work needs what Google's agent does natively and this one does not: watching video or hearing audio, judging or generating images, reading scanned or long PDFs by sight, Google-grounded web research, cheap bulk passes over many files, or a long build left running on Google's quota, covering headless `agy` runs, model choice, briefs, structured results, and result checks"
---

# [USE_ANTIGRAVITY]

`agy -p` runs Google's Antigravity agent headless in the current folder. It reads `~/.gemini/AGENTS.md` on every run, so a brief states the job and never the tools or rules that file holds.

## [01]-[JOBS]

| [INDEX] | [JOB]               | [WHAT_IT_DOES]                                                                | [MODEL]                    |
| :-----: | :------------------ | :---------------------------------------------------------------------------- | :------------------------- |
|  [01]   | Watch and listen    | Video and audio files up to 100 MB read directly: events, cuts, speech, sync  | `gemini-4-argon-high`      |
|  [02]   | Look                | Images, renders against references, scanned pages, boards, UI screenshots     | `gemini-4-carbon-high`     |
|  [03]   | Make images         | Images generated and edited by Google's image model, copied to a named path   | `gemini-4-argon-high`      |
|  [04]   | Research            | Google Search answers with cited links, a second search index                 | `gemini-4-carbon-high`     |
|  [05]   | Bulk                | One small judgment over many files: captions, alt text, tagging, OCR cleanup  | `gemini-4-hydrogen-medium` |
|  [06]   | Grind               | Long build or fix loop under `/goal`, against a done condition and state file | `gemini-pro-orchestrator`  |
|  [07]   | Hard second opinion | One difficult design or security question for a different model family        | `gemini-4-carbon-high`     |

Code edits in a checkout this session is working in, quick questions, and anything waited on stay here: `agy` is slower and often wrong about code.

## [02]-[RUN]

```bash
agy -p "$(cat <brief>)" --disable-slash-commands --model <model> --output-format json --print-timeout 30m > <result.json>
```

- Folder: run from the folder holding the files, `--add-dir <dir>` adds another
- Slash: `-p` reads a prompt opening with `/` as a slash command, so a brief opening with a path needs `--disable-slash-commands`
- Tier: `/run-workflow` agents take `--model` unless the task names `flash_lite` (Gemini 3.5 Flash-Lite), `flash`, or `pro` (both Gemini 3.8 Flash)
- Results: `~/.gemini/antigravity-cli/brain/<conversation_id>/.system_generated/tasks/` holds workflow agent results, one failure cancels the rest
- Model: `agy models` lists slugs, a base name (`gemini-4-carbon`) takes medium effort
- Effort: `--effort <level>` picks a base name's variant, Gemini 4 Hydrogen alone has `xhigh`, Orchestrator slugs take none
- Refusal: an unknown slug, an `--effort` the model lacks, or one conflicting with the slug's suffix exits 1 with no fallback
- Quota: `agy -p /usage --output-format json` costs no turn and returns `remaining_fraction` per model group for 5h and weekly windows
- Data: `--json-schema '<schema>'` or a schema file returns the answer in `.structured_output`, the schema root is `"type":"object"`
- Look-only: `--settings '{"permissions":{"deny":["write_file(*)","command(*)"]}}'` refuses file writes and every shell command
- Probes: denying `write_file(*)` alone keeps shell tools (`ffprobe`, `pdfinfo`), and a shell command can still write
- Background: start it as a background command and keep working, a result arrives in minutes
- Timeout: without `--print-timeout` a run waits for every background task, a grind takes its length in hours
- Follow-up: `--conversation <conversation_id>` continues that run with its context, `-c` the last run of the current folder

One process answers many prompts with `--input-format stream-json --output-format stream-json`: each stdin line `{"event":"user","message":{"content":"<prompt>"}}` returns one `result` event, and closing stdin ends the run.

Slash commands opening a `-p` prompt start a turn without `--disable-slash-commands`:
- `/goal <objective>` re-prompts the agent to audit its work until it answers `<!-- GOAL_COMPLETE -->`, with no turn limit
- `/boost <task>` hands a coding or investigation task to one DeepCoder or DeepInvestigator subagent and returns it until its work checks out
- `/teamwork <task>` hands a large project to a coordinator running planner, worker, synthesizer, and reviewer agents
- `/swarm <task>` runs worker agents on the run's model, each in its own conversation, over a shared message board
- `/run-workflow <task>` has the agent write a Python script that runs agents in parallel or in stages and merges results

## [03]-[BRIEF]

- Material first, question last: paths to the files, then what is wanted from them
- Answer form: a table of timecodes, `path:line` findings, a file written to a named path, a verdict with its evidence and doubts
- Answers this session reads as data take a `--json-schema` in place of a described form
- For a visual judge, the artifact and the reference alone, never the plan or the code that produced it
- For a grind, the done condition, a minimum number of passes, and a state file it rewrites each pass with the open problems
- Headless runs ask nothing: `agy` decides each open choice itself, so the brief states every choice that matters

## [04]-[RESULT]

| [INDEX] | [EXIT] | [MEANS]                                                                            |
| :-----: | :----- | :--------------------------------------------------------------------------------- |
|  [01]   | `0`    | Turn finished, refused tool actions included                                       |
|  [02]   | `1`    | Rejected before any turn: model, effort, schema, or `--settings` key               |
|  [03]   | `2`    | Conflicting flags, or a slash command on stream-json input                         |
|  [04]   | `3`    | Model failure or `--print-timeout` expiry, with an `AGY_ERROR` JSON line on stderr |

- Good results have exit `0`, `.status` `SUCCESS`, no `.finished_reason`, an empty `.denied_actions`, and a non-empty `.response`
- Timed-out runs return a partial `.response` with `.finished_reason` `print_timeout`
- Files it reports writing exist at their paths and open
- Generated images sit in the run's artifacts until a shell or file write copies them, which a look-only run refuses
- Claims about code, documents, or audio are read against the source before they are used
