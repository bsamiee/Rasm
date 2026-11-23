---
name: web
description: "Use when a task needs an answer from the open web, covering tool selection by source and evidence"
argument-hint: "[question]"
---

# [SEARCH_WEB]

Choose retrieval by source location and required evidence.

- Evidence: cite returned source passages with dates and versions matching the question
- Coverage: missing search results leave a claim unresolved
- Selection: stop when source passages resolve the question
- Composition: use another index for missing evidence
- Output: `tvly` takes `--json`, Exa returns JSON, `-o .artifacts/search/<name>.json` keeps a response
- Failure: Tavily `failed_results` and Exa `statuses[].error` record extraction failures, not page absence
- Syntax: read complete source passages when code, table cells, or surrounding qualifications decide a claim

Exa calls send `xh -I -b` to `https://api.exa.ai` with header `"x-api-key:$EXA_API_KEY"`: `k=v` sets a string field, `k:=<json>` a JSON field, and a 4xx exits 4 with a body naming valid values.

| [INDEX] | [TASK]                               | [OPERATION]                                                      |
| :-----: | :----------------------------------- | :--------------------------------------------------------------- |
|  [01]   | Web discovery with passages          | `tvly search` or Exa `POST /search` with `contents.highlights`   |
|  [02]   | Search by source category            | Exa `POST /search` with `category`                               |
|  [03]   | News with publication metadata       | `tvly search --topic news`                                       |
|  [04]   | Fact fields with cited sources       | Exa `POST /search` with `outputSchema`                           |
|  [05]   | Question across selected URLs        | `tvly extract --query` or Exa `POST /contents` with `highlights` |
|  [06]   | Page text with a character limit     | Exa `POST /contents` with `text.maxCharacters`                   |
|  [07]   | Page as stored on a past date        | Exa `POST /contents` with `snapshotAsOf`                         |
|  [08]   | Complex page extraction              | `tvly extract --extract-depth advanced`                          |
|  [09]   | Site URL selection                   | `tvly map`                                                       |
|  [10]   | Site content selected by topic       | `tvly crawl --instructions`                                      |
|  [11]   | Cited research synthesis             | `tvly research` or Exa `POST /agent/runs`                        |
|  [12]   | Row enrichment or continued research | Exa `POST /agent/runs` with `input.data` or `previousRunId`      |
|  [13]   | Page refusing Exa and Tavily fetches | `curl_chrome146 -fsSL '<url>' \| defuddle parse - -m -f`         |
|  [14]   | Page gone from its URL               | `trafilatura -u '<url>' --archived --markdown`                   |

## [01]-[DISCOVERY]

Queries name the question, source type, and constraints that distinguish relevant pages. Tavily `content` and Exa `highlights` provide query-directed passages. Keep passages with URLs during selection.

Use domain paths for documentation sections and publication filters for dated sources. Release dates come from source text, search metadata holds a page's publication date.

Depth follows what decides the answer: `advanced` (2 credits) returns passages from inside long pages, `basic` (1 credit) finds which pages exist and takes no `--chunks-per-source`.

```bash
# Passage inside a long page decides the answer: advanced
tvly search '<question>' --depth advanced --chunks-per-source 3 --max-results 5 --json

# Passage inside a documentation section, version hosts excluded: advanced
tvly search '<question>' --include-domains '<domain/path>' --exclude-domains '<version-host>' \
    --depth advanced --chunks-per-source 3 --max-results 5 --json

# Which dated pages exist, with publication metadata: basic
tvly search '<topic>' --topic news --include-domains '<domain/path>' \
    --start-date '<YYYY-MM-DD>' --end-date '<YYYY-MM-DD>' \
    --depth basic --max-results 5 --json

# Full text from every result, raw content arriving whole at either depth: basic
tvly search '<question>' --include-domains '<domain/path>' \
    --depth basic --include-raw-content markdown --max-results 3 --json
```

Exa search returns `id`, `title`, and `url` alone until `contents` names `highlights` or `text`. `highlights.maxCharacters` sets excerpt length per result, and `highlights.query` directs excerpts to a section. For changed page content, `contents.maxAgeHours: 0` requests a fresh fetch. `type: lite` serves cached pages at lower cost and rejects `outputSchema`, `systemPrompt`, and freshness options. `category` takes `company`, `people`, `publication`, `news`, `personal site`, or `financial report`.

```bash
# Sources describing a symptom or an unfamiliar concept
xh -I -b POST https://api.exa.ai/search "x-api-key:$EXA_API_KEY" \
    query='<description of pages explaining the question>' numResults:=5 \
    contents:='{"highlights": {"maxCharacters": 1500}}' \
    | jq '.results[] | {url, title, publishedDate, highlights}'

# Source section within a publication window, excerpts directed to a section or table
xh -I -b POST https://api.exa.ai/search "x-api-key:$EXA_API_KEY" \
    query='<question>' includeDomains:='["<domain/path>"]' \
    startPublishedDate='<YYYY-MM-DD>' endPublishedDate='<YYYY-MM-DD>' numResults:=5 \
    contents:='{"highlights": {"query": "<section or table>", "maxCharacters": 1500}}' \
    | jq '.results[] | {url, title, publishedDate, highlights}'

# Profiles, filings, or publications from one source category
xh -I -b POST https://api.exa.ai/search "x-api-key:$EXA_API_KEY" \
    query='<description of the entity or document>' category='<category>' numResults:=5 \
    contents:='{"highlights": {"maxCharacters": 1500}}' \
    | jq '.results[] | {url, title, highlights}'
```

`outputSchema` fills fields from search results into `output.content`, with citations and `confidence` per field in `output.grounding`. Object schemas hold at most 10 properties over 2 nesting levels. `type: deep-lite` synthesizes at low latency, `type: deep` searches again for fields one search leaves empty.

```bash
# Fact fields filled from cited sources
xh -I -b POST https://api.exa.ai/search "x-api-key:$EXA_API_KEY" \
    query='<question>' type=deep-lite numResults:=5 \
    outputSchema:='{"type": "object", "required": ["<field>"], "properties": {"<field>": {"type": "string"}}}' \
    | jq '.output'
```

## [02]-[READING]

Read source-provided Markdown URLs for complete text and code. For selected HTML pages, choose query-directed passages or full extraction. `tvly extract` accepts up to 20 URLs per call. `--chunks-per-source` accepts 1–5 with `--query`.

```bash
# Literal in source Markdown
xh -F '<markdown-url>' | rg -nF -C3 -- '<literal>'

# Unique selected URLs when search passages leave a gap
tvly extract '<url>' '<url>' --query '<question>' --chunks-per-source 3 --extract-depth advanced --json

# Full extraction for complex markup, tables, or JavaScript-rendered pages
tvly extract '<url>' --extract-depth advanced --json
```

Exa `/contents` takes search `contents` options at its top level. `text.maxCharacters` limits text per URL, increase it when a required passage ends at truncation. `snapshotAsOf` returns the newest stored version on or before a date and takes no freshness option, older snapshots return 403 `FEATURE_DISABLED` until Exa support enables them.

```bash
# Passages from selected URLs directed to a question
xh -I -b POST https://api.exa.ai/contents "x-api-key:$EXA_API_KEY" \
    urls:='["<url>", "<url>"]' highlights:='{"query": "<question>", "maxCharacters": 2000}' \
    | jq '{results: [.results[] | {url, highlights}], statuses}'

# Page context beyond search excerpts
xh -I -b POST https://api.exa.ai/contents "x-api-key:$EXA_API_KEY" \
    urls:='["<url>", "<url>"]' text:='{"maxCharacters": 20000}' \
    | jq '{results: [.results[] | {url, text}], statuses}'

# Page as stored on a past date
xh -I -b POST https://api.exa.ai/contents "x-api-key:$EXA_API_KEY" \
    urls:='["<url>"]' text:='{"maxCharacters": 20000}' snapshotAsOf='<YYYY-MM-DD>' \
    | jq '{results: [.results[] | {url, text}], statuses}'
```

Use published documentation indexes to select page URLs. Use `tvly map` for URLs and `tvly crawl` for page content. Choose path filters from observed URLs and inspect returned paths before extraction.

```bash
# Documentation page index at the published root
xh -F '<docs-root>/llms.txt' | rg -n -- '<section>'

# URL selection before content retrieval
tvly map '<root>' --select-paths '<path-regex>' --max-depth 2 --limit 20 --no-external --json

# Content selected by topic within known site paths
tvly crawl '<root>' --select-paths '<path-regex>' --max-depth 2 --limit 20 --no-external \
    --instructions '<question>' --chunks-per-source 3 --extract-depth advanced --json
```

PDF page numbers differ from printed page labels. Inspect rendered pages when extraction omits a figure, table structure, or searchable text.

```bash
# Local PDF every later step reads
curl -fsSL --create-dirs -o '.artifacts/search/<name>.pdf' '<pdf-url>'

# PDF pages containing a heading or literal
mutool draw -q -F stext.json -o - '.artifacts/search/<name>.pdf' \
    | jq -r --arg lit '<literal>' '.pages | to_entries[] | select([.value.blocks[] | .lines[]?.text] | join("\n") | contains($lit)) | .key + 1'

# Table rows from a selected PDF page as tab-separated cells
mutool draw -q -F stext.json -O segment,table-hunt -o - '.artifacts/search/<name>.pdf' <page> \
    | jq -r '.. | objects | select(.std? == "TR") | [.contents[] | [.. | .text? // empty] | join(" ")] | @tsv'

# Outline entries with PDF page numbers
mutool show '.artifacts/search/<name>.pdf' outline | rg -- '<heading>'
```

```text
# Rendered pages from a downloaded PDF, up to 20 pages per call
Read {"file_path": ".artifacts/search/<name>.pdf", "pages": "<page>-<page>"}
```

## [03]-[RESEARCH]

Use research when discovery and synthesis require repeated searches. Read selected sources directly when pages answer the question. Scope names known facts, source constraints, dates, and required output.

Tavily research passes `--model pro`. Schema `properties` entries require `type` and `description`. Optional `required` selects mandatory fields. Reports return `content` and `sources`.

```bash
# Detached report while reading independent sources
tvly research run '<scope>' --model pro --no-wait --json

# Schema for findings with supporting URLs
cat > '.artifacts/search/<schema>.json' <<'JSON'
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
tvly research run '<scope>' --model pro --output-schema '.artifacts/search/<schema>.json' --no-wait --json

# Collect by request_id after independent reading
tvly research poll '<request_id>' -o '.artifacts/search/<name>.json' --json
```

Exa runs take `effort` from `minimal` to `ultra` on every call, as API default `auto` is metered, and `budget.maxCostDollars` caps `auto` and `ultra`. `input.data` supplies rows for enrichment with `maxItems` set to the requested row count, and `input.exclusion` names rows to skip. `previousRunId` starts a new run from completed research. Header `Accept:text/event-stream` holds the call until the run ends, its last `data:` line holding the final run, and a call without it returns the run `id` at once. Only `status: completed` carries `output`, with results in `output.structured` and citations in `output.grounding`.

```bash
# Enrichment of known rows with source URLs, held until the run ends
xh -I -b POST https://api.exa.ai/agent/runs "x-api-key:$EXA_API_KEY" Accept:text/event-stream \
    query='<scope>' effort=medium input:='{"data": [{"name": "<name>"}]}' \
    outputSchema:='{"type": "object", "required": ["rows"], "properties": {"rows": {"type": "array", "maxItems": 1,
        "items": {"type": "object", "required": ["name", "claim", "source_url"], "properties": {
            "name": {"type": "string"}, "claim": {"type": ["string", "null"]},
            "source_url": {"type": ["string", "null"], "format": "uri"}}}}}}' \
    | rg -o '^data: (.*)' -r '$1' | tail -n1 > '.artifacts/search/<name>.json'
jq '{id, status, stopReason, structured: .output.structured, grounding: .output.grounding}' '.artifacts/search/<name>.json'

# Detached follow-up with previously covered rows excluded
xh -I -b POST https://api.exa.ai/agent/runs "x-api-key:$EXA_API_KEY" \
    query='<follow-up>' previousRunId='<id>' effort=medium input:='{"exclusion": [{"name": "<name>"}]}' \
    | jq -r '.id'

# Collect by run id after independent reading
xh -I -b GET 'https://api.exa.ai/agent/runs/<id>' "x-api-key:$EXA_API_KEY" -o '.artifacts/search/<name>.json'
```

Research output proposes claims. Versions come from registries, releases from tags, issue state from repository APIs, and other facts from source text. Reports state conclusions with supporting URLs and unresolved questions with sources examined.
