# [CLAUDE_MANIFEST]

@README.md

- Every language in project runs functional (FP) and railway-oriented (ROP).
- Pure expressions, immutable data, total functions, and one outcome carrier per boundary; stack idiom varies, this law never does.

## [01]-[STANDARDS_ROUTING]

[TOOL_ROUTING]:
- ALWAYS use `exa` MCP to start open-web search with neural discovery, the right page, repo, paper, or entity; REPLACES `WebFetch` entirely
- ALWAYS use `search-tavily` skill on known targets — extract or crawl a site, or run a cited multi-source report
- ALWAYS use `search-context7` skill on code/fences with external libraries; never guess SDK/framework/API capabilities or implementations
- ALWAYS use `nuget` MCP to validate the existence of a package and newest version available
- ALWAYS use `binlog` MCP over a `.binlog` (`dotnet build -bl`) for build failures, target/property/import questions, and timing; never replay to text
- ALWAYS use `claudeCodeDocs`/`openaiDeveloperDocs` MCP for Claude Code or Codex config and harness work, modifications, and questions

[CLI_TOOLING]: All tools are installed on machine from the `Parametric_Forge` project, use over standard Unix tooling where applicable

| [INDEX] | [TOOL]    | [GUIDANCE]                                                                                                                 |
| :-----: | :-------- | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `tree`    | `tree <dir>`; hidden files and gitignore are baked in, `-a` is noise; `-L n` sets depth (default 4); `-D` lists dirs only  |
|  [02]   | `loc`     | `loc <dir>` to read; `--json \| jq` over `.folders[].rows[]` is the only cross-folder rank; rows carry `folder`+`file`     |
|  [03]   | `fmt`     | `fmt --check <dir>` verifies, `fmt <dir>` writes, routes each file to its formatter by extension; `.cs` has no lane        |
|  [04]   | `fd`      | `--hidden` is baked in `-H` is noise; pattern is regex — `*.md` errors, take `-e md` or `-g`; `-I` admits ignored          |
|  [05]   | `rg`      | `--smart-case --hidden`; `-s` pins case; types `docs agent config data lock`; `-U` spans `\n`; `-r`=replace, `-E`=encoding |
|  [06]   | `jq`/`yq` | `yq` is mikefarah v4 — `yq '.expr' f`, never `yq r`; `jq` needs `-r` for shell values and `[]?` on optional arrays         |
|  [07]   | `gh`      | Non-TTY prints nothing when empty — never read the table; count through `--json <fields> \| jq length`                     |

## [02]-[IMPLEMENTATION_STANDARDS]

[TOTALITY]: Signatures name every way a call ends, so no outcome escapes the return type
- ALWAYS map every input to a value in the return type; a function that cannot answer returns the reason it cannot
- ALWAYS spell absence as an option-shaped value the consumer unwraps; null, sentinel, and magic defaults die at the admission gate
- ALWAYS make an invalid state unrepresentable at construction, so every consumer reads one regime and re-validates nothing
- ALWAYS reserve exceptions for faults the process cannot continue past; an expected failure rides the outcome carrier

[FLOW]: Dependent steps sequence through one carrier that short-circuits on first failure
- ALWAYS choose the outcome carrier at admission, thread it unchanged through the interior, and collapse it only at the host edge
- ALWAYS chain a step that consumes the previous step's value by binding the carrier; the failure case skips the rest of the chain
- ALWAYS branch by matching the carrier's cases; a status flag, out-parameter, or nullable companion field never selects the path
- ALWAYS hold one carrier per expression, lifting a call that returns a different carrier into the ambient one at the call site
- ALWAYS shape recovery as a function from failure to carrier, seated at the owner that names the fault, never at each call site

[INDEPENDENCE]: Results that do not consume each other combine in one step that collects every failure
- ALWAYS combine independent results applicatively so the answer carries every failure, not the first one encountered
- ALWAYS spell the failure value so two failures append into one, giving accumulation its meaning
- ALWAYS fold a collection through one carrier-returning function into a single carrier over the whole collection
- ALWAYS derive concurrency from independence: operands that do not consume each other evaluate together

[EFFECTS]: Effects ride as values and execute once, at the host entry point
- ALWAYS return an effect as a description; a function that touches the clock, filesystem, network, or host document returns the plan
- ALWAYS run effect descriptions at the composition root and hand the collapsed outcome to the host
- ALWAYS pair acquisition and release inside one bracketed description, so release runs on the failure path unchanged
- ALWAYS thread mutable context as state carried forward through the chain, each step returning the next value

[BOUNDARY]: Boundaries own every conversion between foreign material and domain values
- ALWAYS emit logs, traces, and metrics from the collapsed outcome at the boundary; interior expressions stay pure and silent
- ALWAYS publish a `libs/` package surface already carrier-typed, so a consuming package composes it without unwrapping
- ALWAYS admit host, wire, and file material once at the boundary into validated domain values; the interior sees no raw shape
- ALWAYS map foreign vocabulary to canonical names at the gate that validates, so one owner holds both directions
- ALWAYS collapse the carrier at the boundary into the host's own vocabulary — exit code, status, host exception, UI state

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT] - External libraries, manifests, and host APIs are implementation surfaces:
- ALWAYS keep C# MSBuild/NuGet manifests label-grouped by owner, cluster-sorted, with one-line maintenance comments at most
- ALWAYS align the package touch-point set both ways: central manager row and branch/folder README registries
- ALWAYS repair an orphaned touch-point member at its owner, never by removal
- ALWAYS assume the newest stable release; pin only while incompatible, dropping it when compatibility lands (verify via `nuget`/pnpm/uv)
- ALWAYS spell Python dependency rows as bare unpinned names — workspace-root groups and member manifests alike; `uv.lock` alone fixes versions
- ALWAYS keep a member `pyproject.toml` to identity and bare-name edges; bounds and `python_version` markers seat at the root and drop once stale
