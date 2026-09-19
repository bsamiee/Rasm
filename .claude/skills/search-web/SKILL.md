---
name: search-web
description: "Use when a task needs live web retrieval, discovery, page reading, site section, claim check, or cited research report."
---

# [SEARCH_WEB]

Choose retrieval by source location and required evidence.

- Evidence: cite returned source passages with dates and versions matching the question
- Coverage: missing search results leave a claim unresolved
- Selection: stop when source passages resolve the question
- Composition: use another index for missing evidence
- Output: pass `--json` for structured responses
- Failure: `failed_results` records extraction failures, not page absence
- Syntax: read complete source passages when code, table cells, or surrounding qualifications decide a claim

| [INDEX] | [TASK]                               | [OPERATION]                                      |
| :-----: | :----------------------------------- | :----------------------------------------------- |
|  [01]   | Web discovery with passages          | `tvly search` or `web_search_advanced_exa`       |
|  [02]   | Search by source category            | `web_search_advanced_exa` with `category`        |
|  [03]   | News with publication metadata       | `tvly search --topic news`                       |
|  [04]   | Question across selected URLs        | `tvly extract --query`                           |
|  [05]   | Page text with a character limit     | `web_fetch_exa`                                  |
|  [06]   | Complex page extraction              | `tvly extract --extract-depth advanced`          |
|  [07]   | Site URL selection                   | `tvly map`                                       |
|  [08]   | Site content selected by topic       | `tvly crawl --instructions`                      |
|  [09]   | Cited research synthesis             | `tvly research` or `agent_run`                   |
|  [10]   | Row enrichment or continued research | `agent_run` with `input.data` or `previousRunId` |

## [01]-[DISCOVERY]

Queries name the question, source type, and constraints that distinguish relevant pages. Tavily `content` and Exa `highlights` provide query-directed passages. Keep passages with URLs during selection.

Use domain paths for documentation sections and publication filters for dated sources. Confirm publication dates in source text before treating search metadata as release dates.

```bash
# Passages for a focused question
tvly search '<question>' --depth advanced --chunks-per-source 3 --max-results 5 --json

# Documentation section with version hosts excluded
tvly search '<question>' --include-domains '<domain/path>' --exclude-domains '<version-host>' \
    --depth advanced --chunks-per-source 3 --max-results 5 --json

# Dated coverage with publication metadata
tvly search '<topic>' --topic news --include-domains '<domain/path>' \
    --start-date '<YYYY-MM-DD>' --end-date '<YYYY-MM-DD>' \
    --depth advanced --chunks-per-source 3 --max-results 5 --json

# Search when full text is needed from every result
tvly search '<question>' --include-domains '<domain/path>' \
    --depth advanced --include-raw-content markdown --max-results 3 --json
```

`highlightsMaxCharacters` limits excerpts per result. `textMaxCharacters: 1` limits accompanying page text. For changed page content, `maxAgeHours: 0` requests a fresh fetch.

```text
# Sources describing a symptom or an unfamiliar concept
mcp__exa__web_search_advanced_exa {
    "query": "<description of pages explaining the question>",
    "enableHighlights": true, "highlightsMaxCharacters": 1500,
    "textMaxCharacters": 1, "numResults": 5
}

# PDF discovery with excerpts directed to a section
mcp__exa__web_search_advanced_exa {
    "query": "<document title or subject>", "category": "pdf",
    "enableHighlights": true, "highlightsQuery": "<section or table>",
    "highlightsMaxCharacters": 1500, "textMaxCharacters": 1, "numResults": 5
}

# Source section within a publication window
mcp__exa__web_search_advanced_exa {
    "query": "<question>", "includeDomains": ["<domain/path>"],
    "startPublishedDate": "<YYYY-MM-DD>", "endPublishedDate": "<YYYY-MM-DD>",
    "enableHighlights": true, "highlightsMaxCharacters": 1500,
    "textMaxCharacters": 1, "numResults": 5
}
```

## [02]-[READING]

Read source-provided Markdown URLs for complete text and code. For selected HTML pages, choose query-directed passages or full extraction. `tvly extract` accepts up to 20 URLs per call. `--chunks-per-source` accepts 1–5 with `--query`.

```bash
# Literal in source Markdown
curl -fsSL '<markdown-url>' | rg -nF -C3 -- '<literal>'

# Unique selected URLs when search passages leave a gap
tvly extract '<url>' '<url>' --query '<question>' --chunks-per-source 3 --extract-depth advanced --json

# Full extraction for complex markup, tables, or JavaScript-rendered pages
tvly extract '<url>' --extract-depth advanced --json
```

`web_fetch_exa` limits text per URL with `maxCharacters`. Increase the limit when a required passage ends at truncation.

```text
# Page context beyond search excerpts
mcp__exa__web_fetch_exa {"urls": ["<url>", "<url>"], "maxCharacters": 20000}
```

Use published documentation indexes to select page URLs. Use `tvly map` for URLs and `tvly crawl` for page content. Choose path filters from observed URLs and inspect returned paths before extraction.

```bash
# Documentation page index at the published root
curl -fsSL '<docs-root>/llms.txt' | rg -n -- '<section>'

# URL selection before content retrieval
tvly map '<root>' --select-paths '<path-regex>' --max-depth 2 --limit 20 --no-external --json

# Content selected by topic within known site paths
tvly crawl '<root>' --select-paths '<path-regex>' --max-depth 2 --limit 20 --no-external \
    --instructions '<question>' --chunks-per-source 3 --extract-depth advanced --json
```

PDF page numbers differ from printed page labels. Inspect rendered pages when extraction omits a figure, table structure, or searchable text.

```bash
# PDF pages containing a heading or literal
curl -fsSL '<pdf-url>' | uv run --frozen python -c \
    "import sys, pymupdf; print(*(i + 1 for i, p in enumerate(pymupdf.open(stream=sys.stdin.buffer.read(), filetype='pdf')) if sys.argv[1] in p.get_text()))" '<literal>'

# Table rows from a selected PDF page
curl -fsSL '<pdf-url>' | PYMUPDF_SUGGEST_LAYOUT_ANALYZER=0 uv run --frozen python -c \
    "import sys, pymupdf; [print(' | '.join(map(str, r))) for t in pymupdf.open(stream=sys.stdin.buffer.read(), filetype='pdf')[int(sys.argv[1]) - 1].find_tables().tables for r in t.extract()]" <page>

# Outline entries with PDF page numbers
curl -fsSL '<pdf-url>' | uv run --frozen python -c \
    "import sys, pymupdf; print(*(f'{p} {t}' for _, t, p in pymupdf.open(stream=sys.stdin.buffer.read(), filetype='pdf').get_toc()), sep='\n')" | rg -- '<heading>'

# Local PDF for rendered page inspection
curl -fsSL -o '<pdf>' '<pdf-url>'
```

```text
# Rendered pages from a downloaded PDF, up to twenty pages per call
Read {"file_path": "<pdf>", "pages": "<page>-<page>"}
```

## [03]-[RESEARCH]

Use research when discovery and synthesis require repeated searches. Read selected sources directly when pages answer the question. Scope names known facts, source constraints, dates, and required output.

Use Tavily `mini` for focused research and `pro` for broad synthesis. Schema `properties` entries require `type` and `description`. Optional `required` selects mandatory fields. Reports return `content` and `sources`.

```bash
# Detached report while independent sources are read
tvly research run '<scope>' --model mini --no-wait --json

# Schema for findings with supporting URLs
cat > '<schema.json>' <<'JSON'
{
    "properties": {
    "claims": {
        "type": "array", "description": "Findings required by the scope",
        "items": {
        "type": "object",
        "properties": {
            "claim": {"type": "string", "description": "Claim stated in cited source"},
            "source_url": {"type": "string", "description": "URL supporting the claim"}
        },
        "required": ["claim", "source_url"]
        }
    }
    },
    "required": ["claims"]
}
JSON

# Structured report with claim and source fields defined in the schema
tvly research run '<scope>' --model mini --output-schema '<schema.json>' --no-wait --json

# Collect by request_id after independent reading
tvly research status '<request_id>' --json
```

Exa `input.data` supplies rows for enrichment. Set `maxItems` to the requested row count. `output.structured` holds results with citations in `output.grounding`. Inspect citation coverage before accepting a row. `previousRunId` starts a new run from completed research. `runId` retrieves an existing run.

```text
# Enrichment of known rows with source URLs
mcp__exa__agent_run {
    "query": "<scope>", "effort": "medium",
    "input": {"data": [{"name": "<name>"}]},
    "outputSchema": {
        "type": "object", "required": ["rows"],
        "properties": {
        "rows": {
            "type": "array", "maxItems": 1,
            "items": {
            "type": "object", "required": ["name", "claim", "source_url"],
            "properties": {
                "name": {"type": "string"},
                "claim": {"type": ["string", "null"]},
                "source_url": {"type": ["string", "null"], "format": "uri"}
            }
            }
        }
        }
    }
}

# Follow-up with previously covered rows excluded
mcp__exa__agent_run {
    "query": "<follow-up>", "previousRunId": "<id>", "effort": "medium",
    "input": {"exclusion": [{"name": "<name>"}]}
}

# Collect by runId
mcp__exa__agent_run {"runId": "<id>"}
```

Research output proposes claims. Confirm versions at registries, releases at tags, issue state through repository APIs, and other facts in source text. Briefs state conclusions with supporting URLs and unresolved questions with sources examined.
