# Run ledger — <workflow-name>

Written the moment `Workflow` returns; updated on every resume or restart. Lives in the session scratchpad (harness temp dir), never the repo. Cache-key composition, resume verification, cross-session transplant, and ledger-vs-journal custody: recovery reference.

## Run identity

- Workflow: <workflow-name>
- Scope / args: <scope-or-args, or "none">
- Run ID: <wf\_...>
- Launched scriptPath: <abs-path-to-the-.js-that-was-launched>
- Transcript dir: <~/.claude/projects/.../subagents/workflows/wf\_<id>/ — holds journal.jsonl>
- Run scratch: <.claude/scratch/<name>-<slug>-<hash>/ — the instance-minted dir from the launch log; lane report files live here>

## Resume (same session only)

- Resume: `Workflow({ scriptPath: "<launched scriptPath>", resumeFromRunId: "<wf_...>" })`
- Do NOT edit the launched script while resumable; verify every resume immediately — fresh `started` records belong only to the next pending stage.

## Expected shape (sanity check on resume)

- Phases / agent count: <e.g. discover -> work[pool N] -> reconcile; ~N agents>
- Return value: <what the workflow returns>

## Status log

- <marker>: launched
- <marker>: resumed from <wf\_...> after <reason>
