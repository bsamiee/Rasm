---
name: perplexity-tools
user-invocable: false
description: >-
  Queries Perplexity AI via sonar/sonar-pro/sonar-deep-research/sonar-reasoning-pro models. Use when conducting cited web research, deep technical reports, or complex reasoning problems requiring up-to-date sources.
---

# [H1][PERPLEXITY-TOOLS]

Execute Perplexity AI queries via Python CLI. API key auto-injected via 1Password.

[IMPORTANT] Sonar family models (Feb 2026): `sonar` (lightweight search), `sonar-pro` (deeper retrieval, 2x more results), `sonar-reasoning` (real-time reasoning), `sonar-reasoning-pro` (DeepSeek-R1, visible reasoning), `sonar-deep-research` (long-form reports). Citation tokens no longer billed for sonar/sonar-pro.

## [1][COMMANDS]

| [CMD]    | [ARGS]                    | [MODEL]             |
| -------- | ------------------------- | ------------------- |
| ask      | `<query>`                 | sonar               |
| pro      | `<query>`                 | sonar-pro           |
| research | `<query> [strip]`         | sonar-deep-research |
| reason   | `<query> [strip]`         | sonar-reasoning-pro |
| search   | `<query> [max] [country]` | sonar               |

## [2][USAGE]

```bash
# Quick question with citations (lightweight)
uv run .claude/skills/perplexity-tools/scripts/perplexity.py ask "What is Effect-TS?"

# Pro question with deeper retrieval
uv run .claude/skills/perplexity-tools/scripts/perplexity.py pro "Compare the current Vite release with Turbopack bundling strategies"

# Deep research (10min timeout)
uv run .claude/skills/perplexity-tools/scripts/perplexity.py research "React 19 new features"

# Deep research, strip thinking tags
uv run .claude/skills/perplexity-tools/scripts/perplexity.py research "Vite migration guide" strip

# Reasoning task
uv run .claude/skills/perplexity-tools/scripts/perplexity.py reason "Compare Effect vs RxJS"

# Web search with max results
uv run .claude/skills/perplexity-tools/scripts/perplexity.py search "Nx 22 features" 5
```

## [3][ARGUMENTS]

**ask**: `<query>`
- `query` — Question to ask (required)
- Model: `sonar` (fast, lightweight search)

**pro**: `<query>`
- `query` — Question requiring deeper retrieval (required)
- Model: `sonar-pro` (enhanced search, 2x results)

**research**: `<query> [strip]`
- `query` — Research topic (required)
- `strip` — Pass `strip` to remove `<think>` tags from response
- Model: `sonar-deep-research` (long-form, source-dense reports)

**reason**: `<query> [strip]`
- `query` — Reasoning problem (required)
- `strip` — Pass `strip` to remove `<think>` tags from response
- Model: `sonar-reasoning-pro` (DeepSeek-R1, visible chain-of-thought)

**search**: `<query> [max] [country]`
- `query` — Search query (required)
- `max` — Max results (default: `10`)
- `country` — Country code to focus results (e.g., `US`, `GB`)

## [4][OUTPUT]

Commands return: `{"status": "success|error", ...}`.

| [INDEX] | [CMD]      | [RESPONSE]                       |
| :-----: | ---------- | -------------------------------- |
|   [1]   | `ask`      | `{query, response, citations[]}` |
|   [2]   | `pro`      | `{query, response, citations[]}` |
|   [3]   | `research` | `{query, response, citations[]}` |
|   [4]   | `reason`   | `{query, response}`              |
|   [5]   | `search`   | `{query, results[]}`             |

## [5][ENVIRONMENT]

| [VAR]                | [REQUIRED] | [DESCRIPTION]                           |
| -------------------- | ---------- | --------------------------------------- |
| `PERPLEXITY_API_KEY` | Yes        | Perplexity API key (1Password injected) |

## [6][ERROR_HANDLING]

- HTTP errors print `[ERROR] <status>: <body>` and exit 1
- Rate limit (429): print retry guidance and exit 1
- `research` command uses 10-minute timeout; long-running queries may time out
- `reason` output includes `<think>` tags by default; pass `strip` to remove
