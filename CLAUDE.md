# [CLAUDE_MANIFEST]

@README.md

- Every language in project runs functional (FP), and pure expressions, imperative code is only allowed at boundary
- Dependency picks composition: dependent steps bind and short-circuit, independent steps combine and accumulate every failure
- Idiom varies per language, law never does; a language lacking a carrier authors one rather than a second failure path
- NEVER coin or invent terminology for anything (code files, folder, code value naming, prose in any context), use contextually appropriate domain terminology

## [01]-[STANDARDS_ROUTING]

Read the `docs/stacks/<language>/README.md` of a language when targeting a file owned by it, reading beyond that is scope of task driven

[TOOL_ROUTING]:
- ALWAYS use `exa` MCP to start open-web search with neural discovery
- ALWAYS use `search-tavily` skill on known targets, extract or crawl a site, or run a multi-source report, REPLACES `WebFetch` entirely
- ALWAYS use `search-context7` skill when working with external dependencies; never use training data, never guess SDK/framework/API capabilities
- ALWAYS use `nuget` MCP to validate the existence of a dotnet package, and for finding the newest version available
- ALWAYS use `binlog` MCP for all `.binlog` related work such as build failures, target/property/import questions, and timing, NEVER direct searching
- ALWAYS use `claudeCodeDocs`/`openaiDeveloperDocs` MCP for Claude Code or Codex usage, config, harness work, and understanding

[CLI_TOOLING]: All tools are available on machine from `Parametric_Forge`, use over standard Unix tooling where applicable

| [INDEX] | [TOOL]    | [GUIDANCE]                                                                                  |
| :-----: | :-------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `tree`    | `tree <dir>` lists all directory and files, `-D` for dirs only                              |
|  [02]   | `loc`     | `loc <dir>` for true LOC count with complexity score, folder total + per file               |
|  [03]   | `fd`      | `--hidden` is baked in, `-H` is noise                                                       |
|  [04]   | `rg`      | `--smart-case --hidden`                                                                     |
|  [05]   | `jq`/`yq` | `yq '.expr' f`, never `yq r`, `jq` needs `-r` for shell values and `[]?` on optional arrays |
|  [06]   | `gh`      | Non-TTY prints nothing when empty, count through `--json <fields> \| jq length`             |

## [02]-[IMPLEMENTATION_STANDARDS]

[TOTALITY]: NEVER use a wrapper, extraction, abstraction, or indirection for signature names or returns, they must be named in body and derivable
- ALWAYS give a failure value a case a consumer matches; recovery reads the case, never a rendered message or substring, failure values must be matchable
- ALWAYS name the failure vocabulary at the package that raises it; a consumer widens at the seam, never against one global list
- ALWAYS map every input to a value in the return type; a function that cannot answer returns the reason it cannot
- ALWAYS spell absence as an option-shaped value the consumer unwraps; null, sentinel, and magic defaults die at the admission gate
- ALWAYS make an invalid state unrepresentable at construction, every consumer reads one validated shape and re-validates nothing
- ALWAYS reserve exceptions for faults the process cannot continue past; an expected failure rides the outcome carrier

[FLOW]: Dependent steps sequence through one carrier that short-circuits on first failure
- ALWAYS chain a step that consumes the previous step's value by binding the carrier; the failure case skips the rest of the chain
- ALWAYS shape recovery as a function from failure to carrier, seated at the owner that names the fault, never at each call site
- ALWAYS choose the outcome carrier at admission, thread it unchanged through the interior, and collapse it only at the host edge
- ALWAYS branch by matching the carrier's cases; a status flag, out-parameter, or nullable companion field never selects the path
- ALWAYS hold one carrier per expression, lifting a call that returns a different carrier into the ambient one at the call site
- ALWAYS stack asynchrony onto the carrier rather than beside it, one bind chains an awaited step and a synchronous one alike

[INDEPENDENCE]: Results that do not consume each other combine in one step that collects every failure
- ALWAYS spell the failure value so two or more failures append into one, append order never changes the result
- ALWAYS combine independent results applicatively so the answer carries every failure, not the first one encountered
- ALWAYS fold a collection through one carrier-returning function into a single carrier, accumulating when the elements are independent.
- ALWAYS derive concurrency from independence: operands that do not consume each other evaluate together

[PURITY]: Interior functions read only their arguments and write only their return value
- ALWAYS pass the clock, randomness, environment, and configuration as arguments; interior code reads no ambient source
- ALWAYS pair acquisition and release in one bracket, so release runs unchanged on the failure path
- ALWAYS carry changing context forward as a returned value; so a shared mutable state never coordinates two steps
- ALWAYS confine mutation to a scope that owns it and publishes an immutable value; a buffer that never escapes stays pure

[BOUNDARY]: Boundaries own every conversion between foreign values and domain values
- ALWAYS emit logs, traces, and metrics from the collapsed outcome at the boundary; interior expressions stay pure and silent
- ALWAYS publish a `libs/` package surface already carrier-typed, a consuming package composes it without unwrapping
- ALWAYS admit host, wire, and file content once at the boundary into validated domain values; the interior sees no raw shape
- ALWAYS map foreign vocabulary to canonical names at the gate that validates, one owner holds both directions
- ALWAYS collapse the carrier at the boundary into the host's own vocabulary — exit code, status, host exception, UI state

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT] - External dependencies, SDK's, and APIs are first-class sources:
- ALWAYS keep C# MSBuild/NuGet manifests label-grouped by owner, cluster-sorted, with a precise and concise one-line maintenance comments at most
- ALWAYS align the package touch-point set both ways: central manager row and branch/folder `README.md` registries
- ALWAYS repair an orphaned touch-point member at its owner, never by removal
- ALWAYS assume the newest stable release
- ALWAYS spell ALL Python dependency rows as bare unpinned names, `uv.lock` alone fixes versions
- ALWAYS use `pnpm-workspace.yaml` for Typescript versioning, and align `package.json` with a catalog entry
