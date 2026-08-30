# [MONOREPO_STANDARDS]

@README.md

- Every language uses functional programming: domain logic stays pure and expression-oriented; imperative code remains at system boundaries
- Data dependency determines composition: dependent operations bind and short-circuit; independent operations combine and accumulate every error
- Language-specific idioms may differ, but the composition rules do not; define a result type when a language has no suitable one instead of introducing another error mechanism
- Use established, context-appropriate domain terminology in file names, directory names, identifiers, and prose

## [01]-[LANGUAGE_STANDARDS]

Read `docs/stacks/<language>/README.md` before changing a file owned by that language; let the task scope determine any additional reading

Never use `Grep`, `Glob`, Bash `grep`/`rg` to navigate code source files, languages have specialized skills/mcp servers, and `ast-grep` skill and mcp for general usage.

[TOOL_ROUTING]:
- ALWAYS use `exa` MCP to start open-web search with neural discovery
- ALWAYS use `search-tavily` skill on known targets, extract or crawl a site, or run a multi-source report, REPLACES `WebFetch` entirely
- ALWAYS use `search-context7` skill when working with external dependencies; never use training data, never guess SDK/framework/API capabilities
- ALWAYS use `github` MCP to explore, read, and search repositories on GitHub and to work their issues, pull requests, and runs
- ALWAYS use `dotnet-roslyn-codelens` skill to read, navigate, diagnose, and refactor C# files and code
- ALWAYS use `dotnet-msbuild-evaluation` skill for property, item, condition, and import placement across `.props`, `.targets`, and `.csproj` files
- ALWAYS use `binlog` MCP for all `.binlog` related work such as build failures, target/property/import questions, and timing, NEVER direct searching
- ALWAYS use `nuget` MCP to validate a NuGet package and find its newest available version
- ALWAYS use `claudeCodeDocs`/`openaiDeveloperDocs` MCP for Claude Code or Codex usage, config, harness work, and understanding

[CLI_TOOLING]: All tools are available from `Parametric_Forge`; prefer them to standard Unix tools where applicable

| [INDEX] | [TOOL]    | [GUIDANCE]                                                                                                  |
| :-----: | :-------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `tree`    | `tree <dir>` lists all directory and files, `-D` for dirs only                                              |
|  [02]   | `loc`     | `loc <dir>` for true LOC count with complexity score, folder total + per file                               |
|  [03]   | `fd`      | Use for ALL normal filesystem queries/actions, superseded by specialized skills/mcp depending on context    |
|  [04]   | `rg`      | REPLACES `grep`, NEVER use for code surface search, superseded by language skills/mcp depending on context  |
|  [05]   | `gh`      | Local checkout work: PR from HEAD, checks, checkout, releases, secrets; `gh api` for any uncovered endpoint |
|  [06]   | `jq`/`yq` | `yq '.expr' f`, never `yq r`, `jq` needs `-r` for shell values and `[]?` on optional arrays                 |

## [02]-[IMPLEMENTATION_STANDARDS]

[TOTALITY]: Function signatures name parameter and return types directly; function bodies construct every return case explicitly
- ALWAYS define error variants that consumers pattern match; recovery reads a variant instead of a rendered message or substring
- ALWAYS define error types in the package that raises them; consumers map those errors at the package boundary instead of extending one global error list
- ALWAYS map every input to a value in the return type; a function that cannot answer returns the reason it cannot
- ALWAYS represent absence with an option type the consumer unwraps; reject nulls, sentinels, and magic defaults at the input boundary
- ALWAYS make invalid states unrepresentable at construction; every consumer receives one validated value without re-validating it
- ALWAYS reserve exceptions for unexpected defects the process cannot continue past; expected errors use the result type

[FLOW]: Dependent operations use one result type that short-circuits on the first error
- ALWAYS bind an operation that consumes the previous operation's value; the error case skips the remaining operations
- ALWAYS implement recovery as a function from an error to the same result type, owned by the package that defines the error
- ALWAYS choose the result type at the input boundary, preserve it through domain logic, and translate it only at the host boundary
- ALWAYS select control flow by pattern matching the result cases; status flags, out parameters, and nullable companion fields do not select the path
- ALWAYS use one result type per expression; adapt a call returning a different result type at the call site
- ALWAYS compose asynchrony with the result type so the same bind chains asynchronous and synchronous operations

[INDEPENDENCE]: Results that do not consume each other combine in one step that collects every error
- ALWAYS define a non-empty error type with associative combination so independent errors accumulate deterministically
- ALWAYS combine independent results applicatively so the result carries every error instead of only the first one encountered
- ALWAYS traverse a collection with one result-returning function and accumulate errors when the elements are independent
- ALWAYS derive concurrency from independence: operands that do not consume each other may evaluate concurrently while result ordering remains deterministic

[PURITY]: Domain functions read only their arguments and write only their return value
- ALWAYS pass the clock, randomness, environment, and configuration as arguments; domain code reads no ambient source
- ALWAYS pair acquisition and release in one resource scope so release also runs on the error path
- ALWAYS carry changing context forward as a returned value; shared mutable state does not coordinate operations
- ALWAYS confine mutation to a scope that owns it and publishes an immutable value; a buffer that never escapes stays pure

[BOUNDARY]: Boundaries own every conversion between external values and domain values
- ALWAYS emit logs, traces, and metrics when translating the result at the boundary; domain expressions stay pure and silent
- ALWAYS publish `libs/` package APIs that return the shared result type so consumers compose them without unwrapping
- ALWAYS validate host, protocol, and file input once at the boundary into domain values; domain logic receives no raw input
- ALWAYS map external names to canonical domain names at the validating boundary; one module owns both directions
- ALWAYS translate the result at the boundary into the host's vocabulary: exit code, status, host exception, or UI state

## [03]-[DEPENDENCY_POLICY]

[DEPENDENCY_SOURCES]: External dependencies, SDKs, and APIs are primary sources
- ALWAYS keep .NET MSBuild and NuGet manifests grouped by responsibility, order entries consistently within each group, and limit maintenance notes to one precise line
- ALWAYS record each package in both the central package manager and the owning language or package `README.md` dependency list
- ALWAYS add a missing dependency record to its owning manifest or `README.md` dependency list instead of deleting the corresponding record
- ALWAYS assume the newest stable release
- ALWAYS spell ALL Python dependency rows as bare unpinned names, `uv.lock` alone fixes versions
- ALWAYS use `pnpm-workspace.yaml` for TypeScript dependency versions and align each `package.json` entry with its catalog entry
