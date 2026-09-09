---
name: search-web
description: "Use when a task needs live web retrieval, discovery, page reading, site section, claim check, or cited research report."
---

# [SEARCH_WEB]

Exa ranks by meaning, one sentence describing the ideal page returns the owning issue, spec section, vendor page, or changelog entry first. `tvly search` ranks keywords with a score per hit, `tvly extract` reranks chunks to a question over up to twenty URLs, `tvly crawl` reads the pages a root links, `tvly research` and `agent_run` write a cited report. Use `search-code` for a dependency's API shape, usage, repository, and releases. Use the `github` MCP for a known repository's issues, pull requests, and runs.

- Proof: a claim holds when a vendor page, changelog, spec, or repository states it in returned text, a score, answer, summary, or report is a lead
- Output: every `tvly` call takes `--json`, stdout holds the response alone, `-o <file>` and `--output-dir <dir>` write to disk with one stderr notice
- Disk: `-o` files and `<dir>` sit under the session scratchpad, the step that ends a chain removes the directory it made
- Exit: a failing `tvly` call under `--json` prints `{"error": "<cause>"}` on stdout, exits nonzero, and writes no file
- Cache: a repeated `tvly` query text returns the cached response with `response_time` 0, a comparison takes two query texts
- Size: `highlightsMaxCharacters` bounds the highlights per result, `textMaxCharacters: 1` withholds the page text a highlight already quotes
- Size: `maxCharacters` per URL bounds `web_fetch_exa`, a batch multiplies it
- Versions: every domain filter matches subdomains, a versioned docs site returns each version host and the URL names it
- Freshness: Exa serves cached text, `maxAgeHours: 0` reads the live page within `livecrawlTimeout` milliseconds
- Code: `web_fetch_exa` and a page's `.md` URL keep code block contents, `tvly` raw content and chunks duplicate or drop them on some sites

Numbered lines chain, each consuming the line before, unnumbered lines are alternatives, one per case its comment names.

## [01]-[DISCOVERY]

One query shape per question shape, an Exa sentence names the page the answer sits on:

```text
# [EXA] Meaning ranked, one call per question shape
# Symptom with no known cause, vendor error page or owning issue ranks first
mcp__exa__web_search_advanced_exa {"query": "explanation of why <symptom> happens and how to <fix>", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 5}
# Recency in the sentence, vendor release post ranks first
mcp__exa__web_search_advanced_exa {"query": "<product> release notes for a version released in the last month", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 5}
# Site named in the sentence, vendor page ranks first, query strings duplicate a page
mcp__exa__web_search_advanced_exa {"query": "<vendor site> page on <topic>", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 5}
# Docs of one package rank first, no result using both means no recipe exists
mcp__exa__web_search_advanced_exa {"query": "<language> example composing <package> with <package> through <member>", "enableHighlights": true, "highlightsMaxCharacters": 800, "textMaxCharacters": 1, "numResults": 5}
# Advisory text with affected and patched versions, the package manager's audit decides whether one applies
mcp__exa__web_search_advanced_exa {"query": "GitHub security advisory for <package> stating affected and patched versions", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 3}
# Claim check over pages holding the literal, the highlight states the claim or the claim falls
mcp__exa__web_search_advanced_exa {"query": "<vendor> page stating that <claim>", "includeText": ["<literal>"], "enableHighlights": true, "highlightsMaxCharacters": 800, "textMaxCharacters": 1, "numResults": 3}
# What one site published in a window, publishedDate per result
mcp__exa__web_search_advanced_exa {"query": "<topic>", "includeDomains": ["<domain>"], "excludeDomains": ["<version host>"], "startPublishedDate": "<YYYY-MM-DD>", "endPublishedDate": "<YYYY-MM-DD>", "enableHighlights": true, "highlightsMaxCharacters": 1500, "textMaxCharacters": 1, "numResults": 5}
# Dated press coverage of a release, publishedDate and author per result, the vendor post is the proof
mcp__exa__web_search_advanced_exa {"query": "<product> release", "category": "news", "startPublishedDate": "<YYYY-MM-DD>", "textMaxCharacters": 300, "numResults": 5}
# Repository candidates, a repository URL's text holds stars, license, and created date, issue and tree URLs mix in
mcp__exa__web_search_advanced_exa {"query": "<capability> library, compared against <incumbent>", "category": "github", "textMaxCharacters": 300, "numResults": 6}
```

```bash
# [TAVILY] Keyword ranked, one call per question shape
# Scored hits for a symptom, scores sit close and the URL set decides
tvly search "<keywords>" --depth advanced --max-results 8 --json | jq -r '.results[] | "\(.score) \(.url)"'
# Domain allow list with version hosts removed
tvly search "<keywords>" --include-domains <domain> --exclude-domains <version host> --max-results 5 --json | jq -r '.results[].url'
# URL union of both engines without repeats, chunks reranked to the question in one call, FAILED names a URL web_fetch_exa reads
tvly extract $({ printf '%s\n' <exa-url> <exa-url>; tvly search "<keywords>" --max-results 5 --json | jq -r '.results[].url'; } | sort -u) --query "<question>" --chunks-per-source 2 --json | jq -r '(.results[] | "\(.url)\n\(.raw_content)"), (.failed_results[] | "FAILED \(.url)")'

# [TAVILY] Whole pages on disk
# 1. Hits with whole pages on disk, hit URLs into the window
tvly search "<keywords>" --include-raw-content markdown --max-results 3 --json -o hits.json; jq -r '.results[].url' hits.json
# 2. Cited lines of one hit into the window
jq -r '.results[] | select(.url == "<url>") | .raw_content' hits.json | rg -n -C2 '<literal>'
```

## [02]-[READING]

From known URLs or a site root to the text a claim needs, a site section through its `llms.txt` when the docs root serves one, through a crawl otherwise, onto disk when the section is read more than once:

```text
# [EXA] Whole pages as markdown, code fences intact, a GitHub issue with state and labels
# Read when a chunk stops short, CRAWL_LIVECRAWL_TIMEOUT on one URL of a batch clears on a repeat
mcp__exa__web_fetch_exa {"urls": ["<url>", "<url>"], "maxCharacters": 20000}
```

```bash
# [PAGES] Known URLs, one call per reading shape
# Chunks reranked to a question, [...] between chunks, FAILED names a URL web_fetch_exa reads
tvly extract <url> <url> --query "<question>" --chunks-per-source 3 --json | jq -r '(.results[] | "\(.url)\n\(.raw_content)"), (.failed_results[] | "FAILED \(.url)")'
# Whole page on disk, cited lines into the window, FAILED names a failed fetch
tvly extract <url> --json -o page.json; jq -r '.results[0].raw_content // "FAILED \(.failed_results[0].url)"' page.json | rg -n -C2 '<literal>|^FAILED'

# [SECTION] Docs root serving llms.txt, the root sits below the host on some sites (docs.astral.sh/uv)
# 1. Page URLs of a section from the llms.txt links, a .md URL is the page's markdown with code intact
curl -sf <docs-root>/llms.txt | rg -o '\((https?://[^)]*<section>[^)]*)\)' -r '$1'
# 2. Section on disk in one call, the brace list holds the page names step 1 listed, cited lines through rg
curl -sfZ --create-dirs --output-dir <dir> -o '#1.md' '<docs-root>/<section>/{<page>,<page>}/index.md'; rg -n -C2 '<literal>' <dir>
# 3. Section removed when reading ends
rm -rf <dir>

# [SECTION] Root without llms.txt, external links stay out, pages matching the goal, length 0 on every page means relative links resolved wrong
tvly crawl <root> --max-depth 1 --limit 20 --no-external --instructions "<goal>" --chunks-per-source 2 --json | jq -r '.results[] | "\(.url) \(.raw_content | length)\n\(.raw_content)"'

# [SECTION] Root without llms.txt, section read more than once
# 1. One .md per page on disk, cited lines through rg
tvly crawl <root> --max-depth 1 --limit 20 --no-external --select-paths "<path regex>" --output-dir <dir> --json; rg -n -C2 '<literal>' <dir>
# 2. Section removed when reading ends
rm -rf <dir>
```

## [03]-[RESEARCH]

A question spanning products or months takes a report, a question one search answers takes none. One program in one session, from a written question to a brief the next agents act on without repeating the reading:

1. Scope: write the question, the known facts, the decision the brief serves, and the output form, dates absolute
2. Start: run the report detached, the id returns at once and the run continues on the server
3. Read: discovery over each facet and one extract over the URL union while the report runs, a facet one vendor's docs answer reads as a site section
4. Collect: read the report when the reading ends, status without holding the call, poll holding it
5. Verify: a claim enters the brief after the claim check finds its literal on a vendor page, an API member confirms through `search-code`
6. Brief: question, decision with reasons, facts one per line with a source URL, gaps and where they were sought

`tvly research` returns `content` with numbered citations and `sources[]`. `agent_run` takes `query` or `runId`, never both, and returns `output.text` with inline links, `output.structured` under a schema, and `output.grounding` with citations per field:

```bash
# [TAVILY] Report on the server, request_id ties the calls
# 1. Detached start, request_id at once
tvly research run "<goal, known facts, output form>" --model mini --no-wait --json | jq -r .request_id
# 2. in_progress while running, the whole report once complete, no wait
tvly research status <request_id> --json
# 3. Holds until complete, report on disk
tvly research poll <request_id> --json -o report.json

# [TAVILY] Structured report, schema.json is {"required": [...], "properties": {...}} with a description per property, content holds the object
tvly research run "<goal>" --model mini --output-schema <schema.json> --json | jq .content

# [EXA] Run on the server, id ties the calls, REST default effort is auto
# 1. Detached start, run.json holds query, effort, systemPrompt, input.data, and outputSchema
curl -s https://api.exa.ai/agent/runs -H "Authorization: Bearer $EXA_API_KEY" -H "content-type: application/json" -d @<run.json> | jq -r .id
# 2. Poll without holding a call, running or completed in the envelope agent_run returns
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq '.status, .output.structured, .output.grounding'
```

```text
# [EXA] One call per run shape, MCP default effort is low
# Cross-product report held in the call, a run past the call window returns status running and its id
mcp__exa__agent_run {"query": "<goal, known facts, output form>", "systemPrompt": "Prefer official release notes and changelogs, one line per change with version and date"}
# Row enrichment, known rows in input.data, bounded list, nullable fields outside required, output.structured.rows with grounding per field
mcp__exa__agent_run {"query": "<fields to add per row, sources to prefer>", "input": {"data": [{"<key>": "<value>"}]}, "outputSchema": {"type": "object", "required": ["rows"], "properties": {"rows": {"type": "array", "maxItems": 20, "items": {"type": "object", "required": ["<key>", "verdict"], "properties": {"<key>": {"type": "string"}, "verdict": {"type": "string", "enum": ["<case>", "cannot_verify"]}, "evidence_url": {"type": ["string", "null"], "format": "uri"}}}}}}}
# New run over a finished run's rows, a date or version the prior run got wrong repeats or shifts, verify before the brief
mcp__exa__agent_run {"query": "<follow-up>", "previousRunId": "<id>", "outputSchema": {"type": "object", "required": ["rows"], "properties": {"rows": {"type": "array", "items": {"type": "object", "properties": {"<key>": {"type": "string"}}}}}}}
# Collect a run started here or by curl
mcp__exa__agent_run {"runId": "<id>"}
```

Effort `minimal` costs 0.012 dollars, `low` 0.025, `medium` 0.10, `high` 0.50, `xhigh` 1.00, `auto` is metered to a 5 dollar cap, `max` is metered to a 20 dollar cap over REST alone. `low` fills a row schema and can hold a stale version string, `medium` matched the registry on every row.
