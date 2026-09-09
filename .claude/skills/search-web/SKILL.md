---
name: search-web
description: "Use when a task needs live web retrieval, discovery, page reading, site section, claim check, or cited research report."
---

# [SEARCH_WEB]

Exa ranks by meaning, a sentence describing the ideal page returns the owning issue, spec section, vendor page, or changelog entry first. `tvly search` ranks keywords with a relevance score per hit, `tvly extract` reranks up to twenty URLs to a question, `tvly research` writes a polled report. Use `search-code` for a dependency's API shape, usage, repository, and releases. Use the `github` MCP for a known repository's issues, pull requests, and runs.

- Cache: a repeated `tvly` query text returns the cached response at `response_time: 0.0`, a depth comparison takes two query texts
- Proof: a claim about a tool, limit, or release holds when its vendor page, changelog, spec, or repository states it, a score or report is a lead
- Output: `-o <file>` writes JSON, the spinner goes to stderr, every `tvly` call takes both `-o` and `2>/dev/null`
- Exit: a nonzero `tvly` exit writes no file, the rerun without `2>/dev/null` prints the message
- Size: `web_search_exa` bounds output by `numResults` alone, `web_search_advanced_exa` returns `textMaxCharacters` per result, 1 leaves highlights
- Size: `web_fetch_exa` returns `maxCharacters` per URL, a batch multiplies it
- Currency: Exa returns every version of a versioned page, the URL names the version, `includeDomains` matches subdomains
- Freshness: Exa serves cached text, `maxAgeHours: 0` reads the live page
- Sites: `tvly map` and `crawl` return navigation links or dropped-segment URLs on some docs sites, the docs root's `llms.txt` lists the pages

## [01]-[DISCOVERY]

One query shape per question shape:
- Reference page, spec, limit, advisory, or changelog: one Exa sentence
- Symptom with no known cause: both engines, one `extract` over the URL union
- Dated or scored list: `tvly search`

```text
mcp__exa__web_search_exa {"query": "explanation of why <symptom> happens and how to <fix>", "numResults": 5}                        # Open question, root-cause issue or spec section
mcp__exa__web_search_exa {"query": "<product> release notes for a version released in the last month", "numResults": 5}          # Recency stated in the sentence
mcp__exa__web_search_exa {"query": "<vendor site> page on <topic>", "numResults": 5}                                                # Site named in the sentence, the vendor page ranks first
mcp__exa__web_search_exa {"query": "<language> example composing <package> with <package> through <member>", "numResults": 5}     # Docs of one package rank first, a github.com domain filter drops them, no result may use both
mcp__exa__web_search_exa {"query": "GitHub security advisory for <package> stating affected and patched versions", "numResults": 3}   # Advisory text, the package manager's audit decides whether one applies
mcp__exa__web_search_advanced_exa {"query": "<vendor> page stating that <claim>", "includeText": ["<literal>"], "enableHighlights": true, "highlightsMaxCharacters": 800, "textMaxCharacters": 1, "numResults": 3}   # Claim check, pages holding the literal, the highlight states the claim or the claim falls
mcp__exa__web_search_advanced_exa {"query": "<topic>", "includeDomains": ["<domain>"], "startPublishedDate": "<YYYY-MM-DD>", "enableHighlights": true, "highlightsMaxCharacters": 1500, "textMaxCharacters": 300, "numResults": 5}   # Hard domain and date filter
mcp__exa__web_search_advanced_exa {"query": "<capability> library, compared against <incumbent>", "category": "github", "textMaxCharacters": 300, "numResults": 6}   # Repository candidates, text holds stars, license, and created date, the release date comes from search-code
```

```bash
tvly search "<keywords>" --depth advanced --max-results 8 -o hits.json 2>/dev/null                                            # Scored list, jq gates .results[].score
tvly search "<keywords>" --topic news --time-range week --max-results 5 -o news.json 2>/dev/null                               # Dated list, published_date per result in RFC 1123
tvly search "<keywords>" --include-domains <domain>,<domain> --depth advanced -o scoped.json 2>/dev/null                         # Domain allow list
tvly extract <exa-url> <exa-url> $(jq -r '.results[] | select(.score > 0.5) | .url' hits.json) --query "<question>" --chunks-per-source 2 -o chunks.json 2>/dev/null   # URL union, one extract
```

## [02]-[READING]

Known URLs read in one call, Exa for whole pages as markdown, `tvly extract` for a question over up to twenty URLs, a site section through its `llms.txt`:

```text
mcp__exa__web_fetch_exa {"urls": ["<url>"], "maxCharacters": 20000}                                                             # Whole page into the window, code fences intact, the read when a highlight stops short
```

```bash
tvly extract <url> <url> --query "<question>" --chunks-per-source 3 -o chunks.json 2>/dev/null                                 # Reranked chunks, [...] between them
tvly extract <url> -o page.json 2>/dev/null                                                                                     # Whole page on disk, jq prints the cited sentences
jq -r '.failed_results[].url' chunks.json                                                                                       # Sites extract refuses at exit 0, web_fetch_exa reads them
curl -sf <docs-root>/llms.txt | rg -n '<section>'                                                                              # Page list of a documentation site, the docs root sits below the host on some sites
tvly crawl <root> --max-depth 1 --limit 20 --instructions "<goal>" --chunks-per-source 2 -o crawl.json 2>/dev/null             # Focused chunks over the pages a root links, navigation pages on a docs site
```

## [03]-[RESEARCH]

A question spanning products or months takes a report, a question one search answers takes none. One program runs in one session from a written question to a brief the next agents act on without repeating the reading:

1. Scope: write the question, the known facts, the decision the brief serves, and the output form, dates absolute, one program per facet
2. Ground: discovery searches over each facet in parallel, one `extract` over the URL union
3. Run: start the report detached once the facet is grounded, keep reading, collect when the brief needs it
4. Verify: a claim enters the brief after the claim check finds its literal on a vendor page, changelog, spec, or repository
5. Resolve: disagreeing reports resolve at the source, an absence a report claims gets one Exa search, an API member confirms through `search-code`
6. Brief: question, decision with reasons, facts one per line with a source URL, gaps and where they were sought, files and commands, under 4 KB

`tvly research` returns `content` and `sources[]`. `agent_run` takes `query` or `runId`, never both, holds the call until the run ends or the call window closes, and returns `output.text` with inline links, `output.structured` under a schema, and `output.grounding` with citations per field:

```bash
tvly research run "<goal, known facts, output form>" --model mini --no-wait -o start.json 2>/dev/null                          # Bounded question, request_id returns at once
tvly research poll "$(jq -r .request_id start.json)" -o report.json 2>/dev/null                                                 # Blocks until the run completes
tvly research run "<goal>" --model mini --output-schema <schema.json> -o report.json 2>/dev/null                                # Structured result, the schema holds properties and required at the top alone, content holds the object
curl -s -X POST https://api.exa.ai/agent/runs -H "Authorization: Bearer $EXA_API_KEY" -H "content-type: application/json" -d '{"query": "<goal, known facts, output form>", "effort": "low"}' | jq -r .id   # Detached start, the id returns at once
```

```text
mcp__exa__agent_run {"query": "<goal, known facts, output form>", "systemPrompt": "Prefer official release notes and changelogs, list each change with version and date"}   # Cross-product coverage, effort defaults to low
mcp__exa__agent_run {"query": "<fields to add per row, sources to prefer>", "input": {"data": [{"package": "<id>", "version": "<current>", "ecosystem": "<registry>"}]}, "effort": "medium", "outputSchema": {"type": "object", "required": ["packages"], "properties": {"packages": {"type": "array", "maxItems": 20, "items": {"type": "object", "required": ["package", "verdict"], "properties": {"package": {"type": "string"}, "verdict": {"type": "string", "enum": ["supported", "unsupported", "cannot_verify"]}, "latest": {"type": ["string", "null"]}, "evidence_url": {"type": ["string", "null"], "format": "uri"}}}}}}}   # Row enrichment, known rows in input.data, bounded lists, nullable fields outside required, output.structured.packages
mcp__exa__agent_run {"query": "<follow-up>", "previousRunId": "<id>"}                                                           # New run over a finished run's context
mcp__exa__agent_run {"runId": "<id>"}                                                                                           # Collect a run started here or by curl, status running until it ends
```

Effort `low` costs 0.025 dollars, `medium` 0.10, `high` 0.50, `auto` is metered to a 5 dollar cap, a row schema takes `medium`.
