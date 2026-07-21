# External Lanes

A workflow routes a leg to an external model through a thin native wrapper — `agent()` accepts only native models: codex carries self-contained work legs (repo sweeps, audits, research, mechanical edits), agy (Gemini) the read-only review lane. A codex lane IS one supervised run of the codex skill's lane script: law and task ride files, one backgrounded Bash call is the wait, the receipt JSON is the result — each lane its own process, sibling-isolated and wedge-bounded. Dispatch law is the codex skill's, the Gemini contract the agy skill's; workflow composition lives here.

## [01]-[WRAPPER]

Each wrapper runs `model: 'sonnet', effort: 'low'` with a label prefixed by the real worker, because the workflow UI shows the wrapper's own model. Its whole job is write-run-receipt:

1. Write the LANE LAW block to `<lane-dir>/law.md` and the TASK block to `<lane-dir>/task.md` with the Write tool, both verbatim — composing neither.
2. Run the lane ONCE: one Bash call with `run_in_background: true` to `<repo>/.claude/skills/codex/scripts/codex-lane.sh --task <task-file> --law <law-file> --dir <lane-dir> --cwd <repo> --sandbox <mode>`, adding `--model`/`--effort`/`--web` only where the lane deviates; the harness re-invokes the wrapper when the lane exits, and the wrapper then Reads `<lane-dir>/receipt.json` — the wait is the harness's, never a polling loop.
3. Land a write lane's report from the codex side: codex writes its OWN report as its final act (the prompt names the path) and the wrapper only verifies it — a `jq -e` probe on the keys the prompt's product contract names, never bare parseability, which any wrong-shaped JSON passes — falling back to extracting the last `agent_message` text from `<lane-dir>/events.jsonl`, writing that, and re-probing; a fallback that still fails returns `ok: false` with the probe's miss in `failure`.
4. Land a read lane's report from the script: `--out <report>` materializes the final message at the report path at completion — the wrapper verifies the same way, with the same `events.jsonl` fallback; no wrapper re-emits the product through its own Write on the happy path.
5. Return the thin receipt `{ok, report, entries, headline, failure}` — mechanical counts and one tally headline, never a judgment or a lifted summary; add a `threadId` field carrying the script receipt's `thread_id` when a later stage may continue the thread.

No wrapper performs, edits, re-judges, softens, or relays the work itself. Recovery is two-branch and ONCE-only: a `reason: "crash"` receipt (session live on disk) re-runs with `--resume <thread_id>` and a follow-up task "continue and complete the lane, then land the receipt"; every other failed receipt — `idle-timeout`, `max-timeout`, `turn-failed`, refusal — re-runs the same files verbatim in a fresh lane. A still-failed lane returns `ok: false` with the receipt's `reason` and `failure`. Lane law rides `--law` into `developer_instructions`; the `--task` file stays task-only.

Orchestrator code builds that pair codex-shaped — the leak lives in a shared prompt-builder: one builder feeding both native `agent()` lanes and codex lanes hands codex the native register — hostile-stance paragraphs, intensifier stacks, multi-law prose blocks the codex skill's prompt contract bans. Codex obeys that register literally: it over-uses tools, probes out of territory into files the lane never named, and doubles tokens and latency for zero depth gain.

A codex lane's `prompt` carries the task and its output-contract substance alone; the adversarial posture — burden of proof on the work, both naivety axes, the cold pass — rides `developer-instructions` de-conflicted, one directive per concern. A shared builder forks a `forCodex` arm at the routing point — hostile-register blocks swap for a neutral posture clause, intensifiers drop — while native lanes keep the full estate register. Stance substance is conserved across the fork; only phrasing is model-shaped (execution-standard reference, the stance-register law).

QUOTA FALLBACK: usage exhaustion fails the call loudly with no partial output, so the receipt path already degrades safely — but a run that must FINISH re-dispatches the dead leg natively at the caller: the dispatch helper matches `/usage|quota|limit/i` in `failure` and re-runs the SAME task through its native branch at the role's native twin. That helper owns this, never the wrapper — a wrapper shell must never become the implicit executor:

```js conceptual
const laneWithFallback = (task, o) => lane(task, o).then((r) =>
    !r.ok && /usage|quota|limit/i.test(r.failure) ? lane(task, { ...o, native: true }) : r,
);
```

One option-plumbing law on a parameterized dispatch helper: the helper option carrying a deviated codex reasoning tier is named `codexEffort`, never `effort` — the workflow linter scans every `effort:` key in the file against the agent tier vocabulary, so a codex tier in a helper-option literal fails validation even though it never reaches `agent()`.

```js conceptual
// Wrapper schema is the thin RECEIPT (product body never crosses the wire); the backgrounded
// lane run is legal waiting, so there is no launch receipt and no harvest loop.
const receipt = await agent(codexLane('audit-auth', task, /*writes*/ false), {
    model: 'sonnet',
    effort: 'low',
    label: 'terra:audit-auth',
    schema: RECEIPT,
});
```

## [02]-[PRODUCTS]

Each heavy product lands on disk codex-side — the delegate's own write or the lane script's `--out` — and only the receipt crosses the wire; the patterns reference's report-file pattern owns the receipt-and-roster contract and the terminal read. A codex prompt states the product shape as a prose JSON contract ("Final message: ONLY a JSON object with keys …"); the wrapper's `schema` option is the validation boundary — no schema files on this path. Failure lives in the receipt envelope, never as sentinel values inside data rows. Codex tokens never meter against the token budget (api reference).

## [03]-[SCALE]

Wrapper economics rule the lane count: every wrapper is a full context spin-up (~75k tokens) regardless of effort, so a wrapper per lane pays only when the leg fills it. Short legs batch — one wrapper makes several sequential lane runs and returns one combined receipt; a batched run depending on an earlier product is the same chain, the wrapper relaying the product forward mechanically or continuing the thread. A row-shaped batch collapses into a single lane whose codex session runs `spawn_agents_on_csv`.

An iterative chain continues one thread through `--resume` with the `thread_id` the first receipt returned, never re-paying the exploration cost. A chain spanning wrappers carries `threadId` as an extra receipt field — live interpolation couples the later wrapper's resume key to the earlier receipt, so a replayed run stays coherent, and a resume against a dead thread fails into the `ok: false` receipt like any lane error. Concurrent lanes are concurrent wrapper agents — each holds its own lane process, and the workflow's concurrency cap is the scheduler.

Concurrent write lanes against overlapping paths collide — partition write scopes, or keep codex lanes as read lanes and let one native writer apply the edits.

## [04]-[WRITER_REVIEW_LANE]

A review stage mid-chain (a critique between an implement and a red-team) can itself be a codex lane that WRITES — at the operator-default effort, editing the unit's pages in place under the same review prompt a native agent takes. One composition contract keeps the chain coherent:

- This lane's product is its fixlog (files, verdict, deltas, deferred rows, index rows) written to the lane's report path; only the thin receipt crosses the wire, so the orchestrator never re-serializes review claims.
- That NEXT native stage receives the fixlog as a report PATH framed as unverified prior claims ([20] handoff law), reads it in full from disk, and carries the FOLD-FORWARD DUTY: the fixlog's surviving `deferred`/`indexRows` rows are re-verified and folded into that stage's own return — the chain's terminal stage returns the unit's consolidated record, because the orchestrator cannot read the disk product itself.
- A chain whose terminal stage dies leaves an ORPHANED fixlog: the orchestrator collects `{critReport, rtLanded}` per unit and hands orphaned report paths to the run's terminal fixer to drain — a fold-forward contract with no orphan drain silently loses every row a dead stage was carrying.
- Write scopes stay disjoint by construction (the lane edits only its unit's pages); cross-unit needs ride the deferral rows, never a foreign edit.
- Each stage prompt orders the lane's work per item, edits landing as derived: a review lane whose read ladder (law corpus + unit pages + catalogs) exceeds the codex window compacts mid-run, and a batch fully materialized before its first edit forfeits its earliest findings — the stable law reads once first, then each page closes before the next opens.

## [05]-[GEMINI_REVIEW_LANE]

An agy lane is the third perspective in a critique or red-team stage, beside the native reviewer and a codex lane: a wrapper labeled `gemini:<label>` — the label is the only signal the real worker is Gemini, since the workflow UI shows the wrapper's own model — whose whole job is ONE blocking Bash call to the agy skill's wrapper:

```sh conceptual
uv run <repo>/.claude/skills/agy/scripts/agy.py prompt "<frozen-evidence review prompt>" --add-dir <evidence-dir> --timeout 5m
```

That lane is read-only by CONTRACT — agy print mode can write, so the prompt forbids edits and the wrapper treats any evidence mutation as lane failure (`ok: false`). It therefore takes no write partition, never collides with concurrent write lanes, and never repairs: the wrapper returns the typed findings array as its receipt (small enough to cross the wire — no report file, no polling), and the consuming native stage adjudicates each finding against disk before applying any fix.

Prompt shape — frozen evidence in, falsifiable `{severity, invariant, evidence, failure_path, minimal_fix}` findings out — and the model policy are the agy skill's law; the wrapper adds nothing beyond the call and the receipt.
