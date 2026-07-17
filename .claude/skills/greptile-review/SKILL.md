---
name: greptile-review
description: >-
    Greptile review of the current branch's committed commits against its base — no PR, no push
    — via `greptile review --json`, with MCP result retrieval (`mcp__greptile__*`), the CLI
    runId to MCP codeReviewId correlation, PR-side trigger and merge-request tools, and the
    `.greptile/` configuration cascade. Use for Greptile feedback on committed local work
    before a PR, retrieving or searching past Greptile reviews, or tuning `.greptile/` rules
    and context. CodeRabbit review of working-tree changes belongs to code-review; hosted PR
    reviewer round-trips belong to pr-loop.
---

# [GREPTILE_REVIEW]

Greptile reviews committed work from the local checkout: `greptile review` diffs the current branch's unmerged commits against its base with no PR and no push, and the first-party MCP server (`mcp__greptile__*`) owns retrieval and everything PR-side. Authority rule: the shipped binary outranks the docs — vendor docs describe surfaces (`review status`, `settings set apiBaseUrl`) the installed CLI lacks, so every command claim below comes from `greptile --help` surfaces, re-verifiable at any time. Runbook sections run in order; the configuration reference and economy law that follow carry standing facts the review consults when it calls for them.

## [01]-[PREREQUISITES]

```bash copy-safe
greptile whoami
```

`greptile whoami` proves the signed-in account and organizations. A missing CLI is never installed silently: ask the user first; the vendor install lands the raw binary at `~/.local/bin/greptile`, and `greptile update` owns currency thereafter. Missing authentication runs `greptile login` and waits for the browser flow; a headless environment uses `greptile login --api-key` — key read from a prompt or stdin — or the `GREPTILE_API_KEY` environment variable, and `greptile logout` clears the device. Auth state persists at `~/.greptile/auth.json` as `{method, apiKey, subject, email}`. Reviews require the repository indexed in Greptile cloud; a run on an unindexed repository starts indexing in the background without blocking.

## [02]-[RUN]

Committed-changes law: `greptile review` reviews committed unmerged commits on the current branch against the base — the repository default, or `-b/--branch <base>`, which accepts remote-tracking refs (`origin/main`) — and uncommitted changes never enter the review, so commit first; the exclusion is structural, not ceremony. Work sitting on the base branch with nothing to diff moves to a branch first or passes `-b`. A never-pushed branch reviews cleanly — the run needs no PR and no remote head.

Size law: the CLI refuses an oversized diff client-side — `error: this review is too large to send. Split it into smaller commits and try again.` on stderr with empty stdout, exit 1, no ledger row, no credit spent. A large campaign lands as slice commits and reviews incrementally, each run passing `-b` at the prior slice boundary; a single monolithic commit spanning a whole campaign is unreviewable by this surface.

```bash copy-safe
greptile review --json
```

| [INDEX] | [FLAG]                            | [EFFECT]                                                                    |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `-b, --branch <base>`             | Base branch override; omitted, the repository default applies               |
|  [02]   | `--resume`                        | Continues the latest unfinished review for this repository                  |
|  [03]   | `--include <paths...>`            | Force-includes files held back as sensitive                                 |
|  [04]   | `--json`                          | Machine output, first-class — never a fallback behind another flag          |
|  [05]   | `--text` / `--agent`              | Plain text; `--agent` is a pure alias of `--text`                           |
|  [06]   | `--layout` / `--diff`             | Finding layout `comments` (default) or `diff`; `--diff` shorthands the latter |
|  [07]   | `--context <N>`                   | Nearby code lines around findings, default 15                               |
|  [08]   | `--width`, `--color`/`--no-color` | Render width and color                                                      |

Background law: the run always rides a background task with stdout to a file — never a foreground call holding the session, never a sleep-probe waiting on it. Completion or refusal arrives as the task notification; results read from the output file, the ledger, or `review show` after the fact.

`greptile review show [id]` reopens a finished review. Persistent defaults ride `greptile settings list|get|set|unset|path` over the keys `color`, `review.output` (`auto`|`text`|`json`), `review.layout` (`comments`|`diff`), `review.context`, and `review.width`. A review completes in roughly 60 seconds to a few minutes; `review show`, the local ledger, and MCP retrieval cover status and results, so no supervisor script exists.

## [03]-[RESULTS_AND_RETRIEVAL]

`~/.greptile/reviews.json` is the CLI-local ledger: rows carry `runId` (a CLI-local UUID), `remoteUrl`, `baseRef`/`headRef`, `baseSha`/`headSha`, `createdAt`/`completedAt`, `commentCount`, and `status` (`IN_FLIGHT`|`COMPLETED`).

ID trap: the CLI `runId` UUID is never the MCP `codeReviewId` — `mcp__greptile__get_code_review` takes the numeric `id` from `mcp__greptile__list_code_reviews`, and feeding either identifier into the other surface fails. Correlate a CLI run to its MCP row through `list_code_reviews` by `headSha`/`baseSha` plus timestamp; CLI runs surface there as `source: "headless"` with `mergeRequest` null.

- `list_code_reviews`: rows of `{id, source ("pr"|"headless"), status, commitSha, createdAt/completedAt, metadata {branch, baseSha, headSha, changedFiles, strictness, correlationId}, dispatchUserId, mergeRequest}`.
- `get_code_review`: full rendered summary body plus `summaryCitations`, keyed by the numeric `codeReviewId` only.
- `search_greptile_comments`: searches past Greptile review comments.

Acting on findings holds the review discipline: every finding verifies against current disk and settled corpus law before any edit — a refuted finding is pushed back with its falsifiable citation and kept as output, a confirmed finding fixes at the root and exceeds it, leaving the owner denser than the finding demanded. Refutations and end-state lessons distill at close into the repo's reviewer instruction surfaces — into the durable law corpus only where no surface already owns the rule — one owner per fact.

## [04]-[PR_OPERATIONS]

`mcp__greptile__trigger_code_review` starts PR/MR reviews only — `name` (`owner/repo`), `remote` (`github`|`gitlab`), `defaultBranch`, `prNumber` — and cannot start a local review; the CLI owns local runs. `list_merge_requests`, `get_merge_request`, and `list_merge_request_comments` read hosted merge-request state, and when a hosted reviewer round-trip is in play the pr-loop skill owns the lifecycle.

## [05]-[CONFIGURATION_REFERENCE]

Four routing channels carry review context, each authored and consumed independently:

- [CASCADE]: `.greptile/` directories — the repo-authored channel, detailed below.
- [SINGLE_FILE]: `greptile.json` — a single-file alternative a same-directory `.greptile/` directory beats; this repo uses `.greptile/`.
- [DASHBOARD]: org custom context at `app.greptile.com/review/custom-context`, readable and writable through the MCP custom-context tools (`list_custom_context`, `get_custom_context`, `search_custom_context`, `create_custom_context`); org `CUSTOM_INSTRUCTION` rows ride this channel.
- [MEMORY]: automatic learning from PR comment interactions, reactions, and commit follow-through — auto-generates rules and stays distinct from every authored channel.

A `.greptile/` directory in any directory of the repository configures reviews for that directory tree; all three files are optional.

- [CONFIG]: `config.json` — review settings, run filters, structured rules, cross-repo context.
- [RULES]: `rules.md` — free-prose review charter for the directory tree; severities and rule IDs live only in `config.json` rules.
- [FILES]: `files.json` — `{"files": [{path, description, scope}]}` points the reviewer at load-bearing repo files; `path` resolves relative to the directory containing the `.greptile/` folder (repo root for the root config), so referenced files live anywhere in the repository — doctrine pointing without duplication is the sanctioned use, and references accumulate down the cascade.

### [05.1]-[CONFIG_JSON_FIELDS]

| [INDEX] | [FIELD]                               | [SHAPE_AND_EFFECT]                                                         |
| :-----: | :------------------------------------ | :-------------------------------------------------------------------------- |
|  [01]   | `strictness`                          | `1` verbose, `2` default, `3` critical-only                                |
|  [02]   | `commentTypes`                        | Subset of `syntax`, `logic`, `style`, `info`                               |
|  [03]   | `ignorePatterns`                      | Newline-separated gitignore-syntax file patterns                           |
|  [04]   | `labels` / `disabledLabels`           | Glob filters on PR labels gating whether reviews run                       |
|  [05]   | `includeAuthors` / `excludeAuthors`   | Glob filters on PR authors                                                 |
|  [06]   | `includeBranches` / `excludeBranches` | Glob filters on target branches                                            |
|  [07]   | `includeKeywords` / `ignoreKeywords`  | Newline-separated title/description keywords                               |
|  [08]   | `triggerOnUpdates` / `triggerOnDrafts` | Re-review on PR updates; review draft PRs                                 |
|  [09]   | `statusCheck` / `statusCommentsEnabled` | Commit status check; status comments on the PR                           |
|  [10]   | `fixWithAI`                           | Append AI-fix prompts to findings                                          |
|  [11]   | `instructions`                        | Free-form reviewer instructions; concatenates down the cascade             |
|  [12]   | `rules`                               | Rows of `{rule, id, severity, scope, enabled}`                             |
|  [13]   | `disabledRules`                       | Inherited rule IDs disabled for this tree                                  |
|  [14]   | `context.repos`                       | `owner/repo` list on the same SCM host, readable with the same credentials |
|  [15]   | Section toggles                       | Section objects, each `{included, collapsible, defaultOpen}`               |

- `rules` fields: severity `low`/`medium`/`high`; `scope` globs relative to the `.greptile/` directory.
- Section toggles: `summarySection`, `issuesTableSection`, `confidenceScoreSection`, `sequenceDiagramSection`.

### [05.2]-[CASCADING]

- Greptile walks from the repository root to each reviewed file, collecting every `.greptile/` directory on the path.
- Merge semantics: scalars take the most specific value, arrays replace parent arrays, section objects merge field-wise.
- Accumulation: rules, file references, and instructions accumulate down the cascade.
- A child disables an inherited rule by listing its `id` in `disabledRules`; a rule without an `id` cannot be selectively disabled.
- Precedence low to high: dashboard, org default rules, root `.greptile/`, intermediate `.greptile/`, most specific `.greptile/`, org enforced rules.
- Org enforced rules always apply; the repository cannot override them.

No dedicated security mode or flag exists — security standards land as high-severity `rules` rows, `rules.md` sections, or org enforced dashboard rules; a security-focused pass is a rules change, never a CLI switch.

## [06]-[ECONOMY_AND_CONCURRENCY]

Billing is credit-metered per review — one credit per standard review, three per T-Rex review — so a re-run is a spend, never a free retry: scope the branch before running, batch fixes and re-review once, and `--resume` continues an unfinished run instead of buying a new one. Greptile and CodeRabbit reviews are fully independent — disjoint state directories, no shared locks, different services — and run safely at the same time.
