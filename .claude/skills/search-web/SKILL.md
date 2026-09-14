---
name: search-web
description: "Use when a task needs live web retrieval, discovery, page reading, site section, claim check, or cited research report."
---

# [SEARCH_WEB]

Exa ranks by meaning, one sentence describing the ideal page returns the owning issue, spec section, vendor page, or changelog entry first. `tvly search` ranks keywords with a score per hit, `tvly extract` reranks chunks to a question over up to twenty URLs, `tvly crawl` reads the pages a root links, `agent_run` writes a cited report.

- Proof: a claim holds when a vendor page, changelog, spec, or repository states it in returned text, a score, answer, summary, or report is a lead
- Output: every `tvly` call takes `--json`, stdout holds the response alone
- Exit: a failing `tvly` call under `--json` prints `{"error": "<cause>"}` on stdout and exits nonzero
- Cache: a repeated `tvly` query text returns the cached response with `response_time` 0, a comparison takes distinct query texts
- Size: `highlightsMaxCharacters` bounds the highlights per result, `textMaxCharacters: 1` withholds the page text a highlight already quotes
- Size: `maxCharacters` per URL bounds `web_fetch_exa`, a batch multiplies it
- Versions: a domain filter takes a hostname, a path prefix, or `*.<domain>`, a versioned docs site returns each version host and the URL names it
- Freshness: Exa search and `web_fetch_exa` serve cached text, `maxAgeHours: 0` reads live in search, `curl` reads the live page
- Code: `web_fetch_exa` and a page's `.md` URL keep code block contents, `tvly` raw content and chunks can drop them
- PDF: `web_fetch_exa` reads from the cover under `maxCharacters`, `pymupdf.open` takes piped bytes on `stream`

Numbered lines chain, each consuming the line before, unnumbered lines are alternatives, one per case its comment names.

## [01]-[DISCOVERY]

One query shape per question shape, an Exa sentence names the page the answer sits on:

```text
# [EXA] Meaning ranked, one call per question shape
# Symptom with no known cause, vendor error page or owning issue ranks first
mcp__exa__web_search_advanced_exa {"query": "explanation of why <symptom> happens and how to <fix>", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 5}

# Recency in the sentence, vendor release post ranks first
mcp__exa__web_search_advanced_exa {"query": "<product> release notes for a version released in the last month", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 5}

# Site named in the sentence, vendor page ranks first
mcp__exa__web_search_advanced_exa {"query": "<vendor site> page on <topic>", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 5}

# Docs of one package rank first, no result using both means no example exists
mcp__exa__web_search_advanced_exa {"query": "<language> example composing <package> with <package> through <member>", "enableHighlights": true, "highlightsMaxCharacters": 800, "textMaxCharacters": 1, "numResults": 5}

# Advisory text with affected and patched versions, the package manager's audit decides whether one applies
mcp__exa__web_search_advanced_exa {"query": "GitHub security advisory for <package> stating affected and patched versions", "enableHighlights": true, "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 3}

# Claim check, candidate pages by sentence, the literal on page text proves the claim
mcp__exa__web_search_advanced_exa {"query": "<vendor> page stating that <claim>", "enableHighlights": true, "highlightsMaxCharacters": 800, "textMaxCharacters": 1, "numResults": 3}

# Spec or paper by cover title, highlightsQuery quotes the section's contents line with its printed page
mcp__exa__web_search_advanced_exa {"query": "<cover title>", "category": "pdf", "enableHighlights": true, "highlightsQuery": "<section or table title>", "highlightsMaxCharacters": 600, "textMaxCharacters": 1, "numResults": 5}

# What one site published in a window, publishedDate per result
mcp__exa__web_search_advanced_exa {"query": "<topic>", "includeDomains": ["<domain>"], "excludeDomains": ["<version host>"], "startPublishedDate": "<YYYY-MM-DD>", "endPublishedDate": "<YYYY-MM-DD>", "enableHighlights": true, "highlightsMaxCharacters": 1500, "textMaxCharacters": 1, "numResults": 5}

# Dated press coverage of a release, publishedDate and author per result, the vendor post is the proof
mcp__exa__web_search_advanced_exa {"query": "<product> release", "category": "news", "startPublishedDate": "<YYYY-MM-DD>", "textMaxCharacters": 300, "numResults": 5}

# Repository candidates, a repository URL's text holds stars, license, and created date
mcp__exa__web_search_advanced_exa {"query": "<capability> library, compared against <incumbent>", "category": "github", "textMaxCharacters": 300, "numResults": 6}

# [CLAIM] Page with a live fetch returning an error status or JavaScript-rendered HTML, literal in the reader's markdown or through the site's API
mcp__exa__web_fetch_exa {"urls": ["<url>"], "maxCharacters": 20000}
```

```bash
# [TAVILY] Keyword ranked, one call per question shape
# Scored hits for a symptom
tvly search "<keywords>" --depth advanced --max-results 8 --json | jq -r '.results[] | "\(.score) \(.url)"'
# Domain allow list with version hosts removed
tvly search "<keywords>" --include-domains <domain> --exclude-domains <version host> --max-results 5 --json | jq -r '.results[].url'
# URL union of both engines without repeats, chunks reranked to the question in one call, FAILED names a URL web_fetch_exa reads
tvly extract $({ printf '%s\n' <exa-url> <exa-url>; tvly search "<keywords>" --max-results 5 --json | jq -r '.results[].url'; } | sort -u) --query "<question>" --chunks-per-source 2 --json | jq -r '(.results[] | "\(.url)\n\(.raw_content)"), (.failed_results[] | "FAILED \(.url)")'

# [TAVILY] Whole pages in one call, each hit URL before its cited lines
tvly search "<keywords>" --include-raw-content markdown --max-results 3 --json | jq -r '.results[] | "\(.url)\n\(.raw_content)"' | rg -n -C2 '<literal>|^https?://'

# [CLAIM] Cited lines holding the literal on the live page, last line is HTTP status and bytes, an error status or the byte count of JavaScript-rendered HTML shows there
curl -sL -w '\n%{http_code} %{size_download}\n' <url> | rg -n -C2 '<literal>|^[0-9]{3} [0-9]+$'
```

## [02]-[READING]

From known URLs or a site root to the text a claim needs, a site section through its `llms.txt` when the docs root serves one, through a crawl otherwise:

```text
# [EXA] Whole pages as markdown, code fences intact, a GitHub issue with state and labels
# Read when a chunk stops short
mcp__exa__web_fetch_exa {"urls": ["<url>", "<url>"], "maxCharacters": 20000}

# [PDF] Rendered pages as printed, tables, figures, and scanned pages, up to twenty pages per call
# <pdf> is the file `curl -sfL -o <pdf> <pdf-url>` writes, removed after the read
Read {"file_path": "<pdf>", "pages": "<page>-<page>"}
```

```bash
# [PAGES] Known URLs, one call per reading shape
# Chunks reranked to a question, [...] between chunks, FAILED names a URL web_fetch_exa reads
tvly extract <url> <url> --query "<question>" --chunks-per-source 3 --json | jq -r '(.results[] | "\(.url)\n\(.raw_content)"), (.failed_results[] | "FAILED \(.url)")'
# Whole page in one call, cited lines into the window, FAILED names a failed fetch
tvly extract <url> --json | jq -r '.results[0].raw_content // "FAILED \(.failed_results[0].url)"' | rg -n -C2 '<literal>|^FAILED'

# [PDF] Bytes piped per question from any host, a release asset served as application/octet-stream included, the PDF page a heading or table sits on
# PDF pages holding a heading, table title, or sentence, the contents page first, an empty result names a scanned document
curl -sfL <pdf-url> | uv run --frozen python -c "import sys, pymupdf; print(*(i + 1 for i, p in enumerate(pymupdf.open(stream=sys.stdin.buffer.read(), filetype='pdf')) if sys.argv[1] in p.get_text()))" '<literal>'
# Rows of one page's tables as text with cells separated by |, Read at the page shows the printed table
curl -sfL <pdf-url> | PYMUPDF_SUGGEST_LAYOUT_ANALYZER=0 uv run --frozen python -c "import sys, pymupdf; [print(' | '.join(map(str, r))) for t in pymupdf.open(stream=sys.stdin.buffer.read(), filetype='pdf')[int(sys.argv[1]) - 1].find_tables().tables for r in t.extract()]" <page>

# [PDF] Outline entries of a document with their PDF page, printed page numbers differ from PDF pages
curl -sfL <pdf-url> | uv run --frozen python -c "import sys, pymupdf; print(*(f'{p} {t}' for _, t, p in pymupdf.open(stream=sys.stdin.buffer.read(), filetype='pdf').get_toc()), sep='\n')" | rg '<heading>'

# [SECTION] Docs root serving llms.txt with .md links, the root sits below the host on some sites (docs.astral.sh/uv)
# Each URL prints before its cited lines, no line means zero links matched
curl -sf <docs-root>/llms.txt | rg -o '\((https?://[^)]*<section>[^)]*\.md)\)' -r '$1' | while read -r u; do echo "$u"; curl -sf "$u" | rg -n -C2 '<literal>'; done

# [SECTION] Docs root serving llms.txt with .html links (mise.jdx.dev), section pages through extract, cited lines through rg
tvly extract $(curl -sf <docs-root>/llms.txt | rg -o '\((https?://[^)]*<section>[^)]*)\)' -r '$1') --json | jq -r '.results[] | "\(.url)\n\(.raw_content)"' | rg -n -C2 '<literal>'

# [SECTION] Root at the host without llms.txt, external links stay out, pages matching the goal two links deep
tvly crawl <root> --max-depth 2 --limit 20 --no-external --instructions "<goal>" --chunks-per-source 2 --json | jq -r '.results[] | "\(.url)\n\(.raw_content)"'

# [SECTION] Root at the host without llms.txt, section pages by path through map, cited lines through rg
tvly extract $(tvly map <root> --max-depth 2 --limit 60 --no-external --json | jq -r '.results[]' | rg '<section>') --json | jq -r '.results[] | "\(.url)\n\(.raw_content)"' | rg -n -C2 '<literal>'
```

## [03]-[RESEARCH]

Questions naming more than one product, a span longer than one release cycle, or a state held across repositories take a report, a question one page answers takes one read. One program over any topic, from a written scope to a brief the next agent acts on without repeating the reading:
1. Scope: question, known facts with dates, decision, and output form as the query of the start call's body, the run starts detached at `high`
2. Understand: one Exa sentence per unknown (what it is, owner, primary sources, last change, open questions), a result's wording phrases the next
3. Deepen: `tvly extract --query` over the URL union names the page, `web_fetch_exa` reads it whole, `github` MCP lists issues and pull requests
4. Prove: `[PROVE]` prints each run row beside its registry, release, issue, or live page value, a row with no source value is a gap
5. Brief: question, decision with reasons, dated facts one per line with the primary source URL, gaps with where sought

Collect the run when the reading ends, run text names facets the reading missed, each one more read.

`agent_run` takes `query` or `runId`, never both, and returns `output.text` with inline links, `output.structured` under a schema, `output.grounding` with citations and a confidence per field, and `costDollars`:

```bash
# [EXA] Run on the server, id ties its calls, a fixed effort fixes the price
# 1. Detached start while the facets read, <body> holds query, effort, systemPrompt, input, and outputSchema as JSON, a rejected body prints its cause
curl -s https://api.exa.ai/agent/runs -H "Authorization: Bearer $EXA_API_KEY" -H "content-type: application/json" -d '<body>' | jq -r '.id // .error.message'
# 2. Status without holding a call, text, structured, grounding, and cost once completed, error names an unknown id
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq '{status, text: .output.text, structured: .output.structured, grounding: .output.grounding, cost: .costDollars.total, error: .error.message}'

# [GITHUB] Release date and whether its body names a pull request number, the tag from `git ls-remote --tags`
gh api repos/<owner>/<repo>/releases/tags/<tag> --jq '"\(.published_at) \(.body | test("<pr>"))"'

# [PROVE] Rows of the run beside their owning source, one line per row as row, run value, source value
# [TYPESCRIPT] Version rows at npm
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq -r '.output.structured.rows[] | select(.registry == "npm") | "\(.pkg) \(.version)"' | while read -r p v; do echo "$p $v $(pnpm view "$p" version)"; done
# [PYTHON] Version rows at PyPI
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq -r '.output.structured.rows[] | select(.registry == "pypi") | "\(.pkg) \(.version)"' | while read -r p v; do echo "$p $v $(curl -s "https://pypi.org/pypi/$p/json" | jq -r .info.version)"; done
# [DOTNET] Version rows at NuGet, the flat container index ends at the newest version, id lowercase
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq -r '.output.structured.rows[] | select(.registry == "nuget") | "\(.pkg | ascii_downcase) \(.version)"' | while read -r p v; do echo "$p $v $(curl -s "https://api.nuget.org/v3-flatcontainer/$p/index.json" | jq -r '.versions[-1]')"; done
# [RUST] Version rows at crates.io, the API takes a User-Agent
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq -r '.output.structured.rows[] | select(.registry == "crates") | "\(.pkg) \(.version)"' | while read -r p v; do echo "$p $v $(curl -s -A '<agent>' "https://crates.io/api/v1/crates/$p" | jq -r .crate.max_version)"; done
# Count or default rows on the live page, each literal as its page spells it, then status and bytes
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq -r '.output.structured.rows[] | select(.literal) | "\(.url) \(.literal)"' | while read -r u l; do t=$(curl -sL -w '\n%{http_code} %{size_download}\n' "$u"); echo "$u $l | $(printf '%s' "$t" | rg -oF -e "$l" | head -1) $(printf '%s' "$t" | tail -1)"; done
# Issue and pull request rows through gh api, state with the closing or last update date
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq -r '.output.structured.rows[] | select(.number) | "\(.repo) \(.number) \(.state)"' | while read -r r n s; do echo "$r#$n $s $(gh api "repos/$r/issues/$n" --jq '"\(.state) \(.closed_at // .updated_at | .[:10])"')"; done
# Release rows through gh api, the publish date at the tag
curl -s https://api.exa.ai/agent/runs/<id> -H "Authorization: Bearer $EXA_API_KEY" | jq -r '.output.structured.rows[] | select(.tag) | "\(.repo) \(.tag) \(.date)"' | while read -r r t d; do echo "$r@$t $d $(gh api "repos/$r/releases/tags/$t" --jq '.published_at[:10]')"; done
```

```text
# [EXA] One call per run shape, effort on every call, the MCP default is low
# Cross-facet report held in the call with one row per fact, a run past the call window returns status running and its id
mcp__exa__agent_run {"query": "<scope>", "effort": "high", "systemPrompt": "Prefer registry pages, releases, changelogs, specs, and issues, state every version with its date, say unknown when no page states a fact, one row per fact, a literal quoted as the page spells it", "outputSchema": {"type": "object", "required": ["rows"], "properties": {"rows": {"type": "array", "maxItems": 30, "items": {"type": "object", "properties": {"pkg": {"type": ["string", "null"]}, "registry": {"type": ["string", "null"], "enum": ["npm", "pypi", "nuget", "crates", null]}, "version": {"type": ["string", "null"]}, "repo": {"type": ["string", "null"]}, "number": {"type": ["integer", "null"]}, "state": {"type": ["string", "null"], "enum": ["open", "closed", null]}, "tag": {"type": ["string", "null"]}, "date": {"type": ["string", "null"]}, "url": {"type": ["string", "null"]}, "literal": {"type": ["string", "null"]}}}}}}}

# Row enrichment, known rows in input.data, nullable fields outside required, output.structured.rows with grounding per field
mcp__exa__agent_run {"query": "<fields to add per row, sources to prefer>", "effort": "high", "input": {"data": [{"<key>": "<value>"}]}, "outputSchema": {"type": "object", "required": ["rows"], "properties": {"rows": {"type": "array", "maxItems": 20, "items": {"type": "object", "required": ["<key>", "verdict"], "properties": {"<key>": {"type": "string"}, "verdict": {"type": "string", "enum": ["found", "cannot_verify"]}, "evidence_url": {"type": ["string", "null"], "format": "uri"}}}}}}}

# Further rows over a finished run, rows in input.exclusion stay out, every version verifies before the brief
mcp__exa__agent_run {"query": "<follow-up>", "previousRunId": "<id>", "effort": "high", "input": {"exclusion": [{"<key>": "<value>"}]}, "outputSchema": {"type": "object", "required": ["rows"], "properties": {"rows": {"type": "array", "maxItems": 10, "items": {"type": "object", "required": ["<key>", "source_url"], "properties": {"<key>": {"type": "string"}, "source_url": {"type": "string", "format": "uri"}}}}}}}

# Collect a run started here or by curl
mcp__exa__agent_run {"runId": "<id>"}
```

Effort `high` costs 0.50 dollars per run, an Exa search 0.007 dollars with each result above 10 at 0.001, `web_fetch_exa` 0.001 dollars per page, `tvly search` 1 credit at basic depth and 2 at advanced, `tvly extract` 1 credit per 5 URLs read.
