# [MONOREPO_STANDARDS]

@README.md

- Every language uses functional programming: domain logic stays pure and expression-oriented, imperative code stays at system boundaries
- Data dependency decides composition: dependent operations bind and short-circuit, independent operations combine and accumulate every error
- Language idioms differ but the composition rules do not, define a result type when a language lacks one instead of adding another error mechanism
- Use established, context-appropriate domain terminology in file names, directory names, identifiers, and prose
- Fix a defect at its cause, a wrapper, fallback, guard, or retry around it is the defect
- Test files exist when the user asks for a test
- Checks, rules, and tests stay read-only during a code change, editing one is its own task the user asks for
- Checks run over the files a change touched, never the full tree
- Removals delete every mention and adjust each consumer to the absence, nothing stands in for removed content
- Languages join in one change with toolchain, tag, targets and inputs, checker, writer, parser, rules, outline, CI runner, and README sections
- Refactors become ast-grep rules, before form as pattern and after form as fix, once a second instance exists and no checker reports it
- Skills, memories, and docs hold the principle that decides a case, a project's state is its files
- Paths git ignores hold no project state, a file under one is read and never edited, checked, or created
- Every tool, package, and server in the tree is a capability, its current docs decide a solution's form before one is written
- Audits, security scans, supply-chain pins, and approval gates join a manifest, target, workflow, or program at the user's request alone

## [01]-[LANGUAGE_STANDARDS]

Navigate code through its language's skill and MCP server, or the ast-grep skill and MCP for every other language, then project CLI tooling

[TOOL_ROUTING]:
- ALWAYS use `search-web` skill for a question the open web answers
- ALWAYS use `search-code` skill for any fact about a dependency
- ALWAYS use `github` MCP for a known repository's issues, pull requests, and runs
- ALWAYS use `dotnet-roslyn-codelens` skill for any file in a .NET solution
- ALWAYS use `dotnet-coding` skill for any C# code
- ALWAYS use `dotnet-msbuild-evaluation` skill for an MSBuild declaration
- ALWAYS use `dotnet-msbuild-antipatterns` skill before changing an MSBuild file
- ALWAYS use `dotnet-msbuild-execution` skill for a `<Target>`
- ALWAYS use `dotnet-msbuild-diagnostics` skill for diagnosing a .NET build
- ALWAYS use `dotnet-msbuild-packaging` skill for NuGet and .slnx
- ALWAYS use `manage-repo` skill for Nx targets, tooling, infrastructure, and CI
- ALWAYS use `nuget` MCP to validate a NuGet package and find its newest version
- ALWAYS use `claudeCodeDocs`/`openaiDeveloperDocs` MCP for a question about Claude Code or Codex
- ALWAYS use `playwright:playwright-cli` skill for a browser, run as `playwright cli`, `playwright` MCP when each step depends on the last snapshot
- ALWAYS use `computer-use` MCP for a native application window, its wait and screenshot tools stand where a shell would sleep
- ALWAYS read a foreground command's exit code or the completion notification of a background command or agent, no sleep, poll, or monitor loop waits

[CLI_TOOLING]:

| [INDEX] | [TOOL]      | [GUIDANCE]                                                                                                  |
| :-----: | :---------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `tree`      | `tree <dir>` lists all directories and files, `-D` for dirs only                                            |
|  [02]   | `loc`       | `loc <dir>` for true LOC count with complexity score, folder total + per file                               |
|  [03]   | `fd`        | Use for ALL normal filesystem queries/actions, superseded by specialized skills/mcp depending on context    |
|  [04]   | `rg`        | `rg <pattern> <paths>` for literals, comments, and prose, never for code search                             |
|  [05]   | `gh`        | Local checkout work: PR from HEAD, checks, checkout, releases, secrets, `gh api` for any uncovered endpoint |
|  [06]   | `jq`/`yq`   | `yq '.expr' f`, never `yq r`, `jq` needs `-r` for shell values and `[]?` on optional arrays                 |
|  [07]   | `sd`        | `sd '<regex>' '<replacement>' <files>` for a literal or regex rewrite over files, `-F` for a fixed string   |
|  [08]   | `difft`     | `difft <before> <after>` for a syntax-tree diff, `GIT_EXTERNAL_DIFF=difft git diff` over a change           |
|  [09]   | `hyperfine` | `hyperfine -r <runs> '<command>'` times commands under the same controls, `-N` skips the shell              |
|  [10]   | `duckdb`    | `duckdb -c '<sql>'` queries CSV, Parquet, and JSON files in place, `-json` for machine output               |

## [02]-[IMPLEMENTATION_STANDARDS]

[TOTALITY]: Function signatures name parameter and return types directly, function bodies construct every return case explicitly
- ALWAYS define error variants that consumers pattern match, recovery reads a variant instead of a rendered message or substring
- ALWAYS define error types in the package that raises them, consumers map them at the package boundary instead of extending a global error list
- ALWAYS map every input to a value in the return type, a function that cannot produce a value returns the reason it cannot
- ALWAYS represent absence with an option type the consumer unwraps, reject nulls, sentinels, and magic defaults at the input boundary
- ALWAYS make invalid states unrepresentable at construction, every consumer receives a validated value without re-validating it
- ALWAYS reserve exceptions for unexpected defects the process cannot continue past, expected errors use the result type

[FLOW]: Dependent operations use one result type that short-circuits on the first error
- ALWAYS bind an operation that consumes the previous operation's value, the error case skips the remaining operations
- ALWAYS recover through a function from an error to the same result type, the raising package classifies, the consumer with the alternative recovers
- ALWAYS choose the result type at the input boundary, preserve it through domain logic, and translate it only at the host boundary
- ALWAYS select control flow by pattern matching result cases, no status flag, out parameter, or nullable field paired with a flag selects the path
- ALWAYS use one result type per expression, adapt a call returning a different result type at the call site
- ALWAYS compose asynchrony with the result type, the same bind chains asynchronous and synchronous operations

[INDEPENDENCE]: Results that do not consume each other combine in one step that collects every error
- ALWAYS define a non-empty error type with associative combination, independent errors accumulate deterministically
- ALWAYS combine independent results applicatively, the result holds every error instead of only the first one encountered
- ALWAYS traverse a collection with one result-returning function and accumulate errors when the elements are independent
- ALWAYS derive concurrency from independence, operands that do not consume each other can evaluate concurrently, result order stays deterministic

[PURITY]: Domain functions read only their arguments and write only their return value
- ALWAYS pass the clock, randomness, environment, and configuration as arguments, domain code reads no ambient source
- ALWAYS pair acquisition and release in one resource scope, release runs on the error path
- ALWAYS pass changing context to the next operation as a returned value, shared mutable state does not coordinate operations
- ALWAYS confine mutation to a scope that owns it and publishes an immutable value, a buffer that never escapes stays pure

[BOUNDARY]: Boundaries own every conversion between external values and domain values
- ALWAYS emit logs, traces, and metrics when translating the result at the boundary, domain expressions stay pure and emit none
- ALWAYS return the language's one result type from every package/project API, consumers compose without unwrapping
- ALWAYS validate host, protocol, and file input once at the boundary into domain values, domain logic receives no raw input
- ALWAYS map external names to canonical domain names at the validating boundary, that module owns both directions
- ALWAYS translate the result at the boundary into the host's representation: exit code, status, host exception, or UI state

[DIRECTNESS]: Code calls the owning API in the direct form, and every layer between a caller and that API adds a fact the API lacks
- ALWAYS call the owning type or package directly, a wrapper exists only to add a domain type, a boundary conversion, or a composed policy
- ALWAYS name the real type, a type alias exists only to resolve a name collision between referenced namespaces
- ALWAYS define a custom operator, implicit conversion, or extension method for a domain meaning, never to shorten a call the direct form states
- ALWAYS reach a dependency through the function or runtime that supplies it, never a service locator or a layer that forwards a call unchanged

## [03]-[DEPENDENCY_POLICY]

[DEPENDENCY_SOURCES]: External dependencies, SDKs, and APIs are primary sources
- ALWAYS group .NET MSBuild and NuGet manifest entries by responsibility, order each group consistently, and keep maintenance notes to one line
- ALWAYS record each package as one row of its central manifest with a one-line purpose comment
- ALWAYS add a missing dependency record to its owning manifest instead of deleting the corresponding record
- ALWAYS assume the newest release, prereleases included, and pin nothing outside `uv.lock`, `pnpm-lock.yaml`, and `Directory.Packages.props`
- ALWAYS let a manifest, a lock, or a check state a fact once, packages, workflows, tooling, and scripts hold no fallback, guard, retry, or cooldown
- ALWAYS reference a package directly in every project that names its types, a transitive reference supplies no global using, alias, or analyzer
- ALWAYS map every package id to one source in `NuGet.config`

## [04]-[FUNCTION_HOOKS]

[TOOL_CALL]: Function hooks hold one decision the harness takes on every tool call before the tool runs, `deny` or `next`
- ALWAYS use `plugin-authoring` skill for writing or changing a function hook
- ALWAYS write a policy as a pure function from the parsed call to a decision, the registered hook alone reads `$` and answers `deny` or `next`
- ALWAYS fold every policy under the one `tool.call` registration, the first refusal is the call's answer
