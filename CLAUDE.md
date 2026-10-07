# [MONOREPO_STANDARDS]

@README.md

- Work runs to completion, guidance files and existing standards decide every open question in place of user input, never hedge or defer, remove/replace tools/code/etc
- One call per unit of work covers its full scope once and reads the result whole
- Every language uses functional programming: domain logic stays pure and expression-oriented, imperative code stays at system boundaries
- Data dependency decides composition: dependent operations bind and short-circuit, independent operations combine and accumulate every error
- Language idioms differ but composition rules do not, when a language lacks a result type, adopt a dependency's, else define one
- Fix defects at root cause, a wrapper, fallback, guard, or retry that hides the cause is a defect
- Code smells are fixed as found, existing code has no authority, a pattern that bends new code or a refactor out of shape is rebuilt from its root
- `tools/` shares no dependency in either direction with `libs/`, `apps/`, or agent harness code, tools read project files, skills run tools
- Tool settings and rules that push code against the coding standards are removed at their owner
- Tests are made at the user's request alone
- Removals delete every mention and adjust each consumer to the absence, nothing stands in for removed content
- Languages join in one change with toolchain, tag, targets and inputs, checker, writer, parser, rules, outline, CI runner, and README sections
- Audits, security scans, supply-chain pins, and approval gates are added on user request alone
- Commits and pushes happen on user request alone

- Non-trivial work starts with a task file under tmp outside the project, holding every task with its facts and nothing else
- Proactively delegate work to sub-agents for search, research, ideation, and writing, never tackle large solo when total workload is non-trivial
- Any `.md` file that is durable, and part of the project receives a focused adversarial review with `clean-prose` fully read and focused on the git-diff/modified content
- All code files are adversarially reviewed to rebuild aggressively, initial code is always slop, skills, memory, `CLAUDE/AGENTS.md`, and similar determine quality
- Reviews are adversarial, reviewers assume code is wrong and commit corrections, a report stands where a task or skill asks for one

## [01]-[LANGUAGE_STANDARDS]

Navigate code through its language's skill and MCP server, else the `use-ast-grep` skill and `ast-grep` MCP, then project CLI tooling

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
- ALWAYS use `playwright:playwright-cli` skill for a browser, run as `playwright cli`, the plugin's `playwright` MCP server when each step depends on the last snapshot
- ALWAYS use `xcode` MCP for Apple documentation and Xcode, `lldb` MCP for a debug session `xcode` MCP did not start
- ALWAYS use `use-rhino` skill for Rhino and Grasshopper
- ALWAYS use `use-blender` skill for Blender

[CLI_TOOLING]:

| [INDEX] | [TOOL]      | [GUIDANCE]                                                                                                  |
| :-----: | :---------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `tree`      | `tree <dir>` lists directories and files, `-D` only directories                                             |
|  [02]   | `loc`       | `loc <dir>` counts code lines with complexity score per file and folder                                     |
|  [03]   | `fd`        | `fd <pattern> <path>` finds files and directories, a skill or MCP covering that area supersedes it          |
|  [04]   | `rg`        | `rg <pattern> <paths>` for literals, comments, and prose alone                                              |
|  [05]   | `gh`        | Local checkout work: PR from HEAD, checks, checkout, releases, secrets, `gh api` for any uncovered endpoint |
|  [06]   | `jq`/`yq`   | `yq '.expr' f`, `yq` has no `r` subcommand, `jq` needs `-r` for shell values and `[]?` on optional arrays   |
|  [07]   | `sd`        | `sd '<regex>' '<replacement>' <files>` by line, `-A` for a find across lines, `-F` fixed, `--` before a `-` |
|  [08]   | `difft`     | `difft <before> <after>` for a syntax-tree diff, `GIT_EXTERNAL_DIFF=difft git diff` over a change           |
|  [09]   | `hyperfine` | `hyperfine -r <runs> '<command>'` times commands under the same controls, `-N` skips the shell              |
|  [10]   | `duckdb`    | `duckdb -c '<sql>'` queries CSV, Parquet, and JSON files in place, `-json` for machine output               |

## [02]-[IMPLEMENTATION_STANDARDS]

[TOTALITY]: Function signatures name parameter and return types directly, function bodies construct every return case explicitly
- ALWAYS define error variants that consumers pattern match, recovery reads a variant instead of a rendered message or substring
- ALWAYS define error types in the package that raises them, consumers map them at the package boundary instead of extending a global error list
- ALWAYS map every input to a value in the return type, a function that cannot produce a value returns the reason it cannot
- ALWAYS represent absence with an option type the consumer unwraps, a host null, sentinel, or magic default becomes an option at the boundary
- ALWAYS make invalid states unrepresentable at construction, every consumer receives a validated value without re-validating it
- ALWAYS reserve exceptions for defects, expected failures use the result type

[FLOW]: Dependent operations use one result type that short-circuits on the first error
- ALWAYS bind an operation that consumes the previous operation's value, the error case skips the remaining operations
- ALWAYS recover through a function from an error to the same result type, raising packages classify, consumers holding an alternative recover
- ALWAYS choose the result type at the input boundary and preserve it through domain logic
- ALWAYS select control flow by pattern matching result cases, no status flag, out parameter, or nullable field paired with a flag selects the path
- ALWAYS use one result type per expression, adapt a call returning a different result type at the call site
- ALWAYS compose asynchrony with the result type, asynchronous and synchronous operations chain through one result type

[INDEPENDENCE]: Results that do not consume each other combine in one step that collects every error
- ALWAYS accumulate independent errors in a non-empty error type with associative combination, accumulation order stays deterministic
- ALWAYS combine independent results applicatively, the result holds every error instead of only the first one encountered
- ALWAYS traverse a collection with one result-returning function and accumulate errors when the elements are independent
- ALWAYS derive concurrency from independence, operands that do not consume each other can evaluate concurrently, result order stays deterministic

[PURITY]: Domain functions read only their arguments and write only their return value
- ALWAYS pass the clock, randomness, environment, and configuration as arguments, domain code reads no ambient source
- ALWAYS pair acquisition and release in one resource scope, release runs on the error path
- ALWAYS pass changing context to the next domain operation as a returned value, shared mutable state does not coordinate domain operations
- ALWAYS confine mutation to a scope that owns it and publishes an immutable value, a buffer that never escapes stays pure
- ALWAYS describe a target's desired state as data, interpreters reconcile the data into typed change rows against the live or stored target

[BOUNDARY]: Boundaries own every conversion between external values and domain values
- ALWAYS emit logs, traces, and metrics from boundary and service code, domain expressions stay pure and emit none
- ALWAYS return the language's result type from every fallible package and project API, consumers compose without unwrapping
- ALWAYS validate host, protocol, and file input once at the boundary into domain values, domain logic receives no raw input
- ALWAYS map external names to canonical domain names at the validating boundary, one module owns both directions
- ALWAYS translate the result at the boundary into the host's representation: exit code, status, host exception, or UI state

[DIRECTNESS]: Code calls the owning API in the direct form, and every layer between a caller and that API adds a fact the API lacks
- ALWAYS call the owning type or package directly, a wrapper exists only to add a domain type, a boundary conversion, or a composed policy
- ALWAYS name the real type, an alias renaming a declared type exists only to resolve a name collision between referenced namespaces
- ALWAYS define a custom operator, implicit conversion, or extension method only for a domain meaning or a composition the language lacks
- ALWAYS reach a dependency through the function or runtime that supplies it, a service locator or a layer forwarding a call unchanged is a defect

[DERIVATION]: Values a system exposes come from its API and metadata at run time, a literal or table holds a fact no API states
- ALWAYS declare a fact the API hides once at its owner, every other use derives it
- ALWAYS type configuration values with their owner's range
- ALWAYS reach members through typed handles instead of member-name strings

## [03]-[DEPENDENCY_POLICY]

[DEPENDENCY_SOURCES]: External dependencies, SDKs, and APIs are primary sources
- ALWAYS group project file items and dependency rows by responsibility and order each group consistently
- ALWAYS add a dependency's missing record to the project file or catalog that owns it, the present record stays
- ALWAYS use the newest release, prereleases included
- ALWAYS pin versions in lock files and version catalogs alone
- Packages, workflows, tooling, and scripts hold no fallback, guard, retry, or cooldown
- ALWAYS reference a package directly in every project that names its types, a transitive reference supplies no global using, alias, or analyzer

## [04]-[FILE_ORGANIZATION]

Source files group declarations into sections and an owner's members into operation families, `<marker>` is the language's line comment:

```text
<marker> --- [<SECTION>] -----------------------------------------------------------------
<owner>
    <marker> --- [<FAMILY>]
    <member>
```

[SECTIONS]: Sections follow table order, and a file omits a section it holds no declaration for

| [INDEX] | [LABEL]         | [HOLDS]                                                        |
| :-----: | :-------------- | :------------------------------------------------------------- |
|  [01]   | `[TYPES]`       | Aliases, interfaces, protocols, delegates, enums               |
|  [02]   | `[CONSTANTS]`   | Literals, limits, and error codes reading no later declaration |
|  [03]   | `[MODELS]`      | Records, unions, value objects, schemas, data classes          |
|  [04]   | `[ERRORS]`      | Error variants                                                 |
|  [05]   | `[SERVICES]`    | Types owning a resource, handle, or host dependency            |
|  [06]   | `[OPERATIONS]`  | Pure transforms, effects, and algorithms over models           |
|  [07]   | `[COMPOSITION]` | Host entry points, command registration, layers, wiring        |
|  [08]   | `[EXPORTS]`     | Export lists, `__all__`                                        |

[DIVIDERS]: One label names one concept in every file
- ALWAYS open a section with a full divider: comment marker, `---`, one `[UPPER_SNAKE]` label, dashes to column 90
- ALWAYS open a subsection with a divider without dashes, indented with its members, when one section holds more than one operation family
- ALWAYS place every declaration under a divider, the preamble (imports, namespace, shebang, `set` flags, assembly attributes) precedes the first one
- ALWAYS place every member of a subdivided section under a `[<FAMILY>]` subsection that names what its members do
- ALWAYS add a `[<SECTION>]` label outside the table after its closest core section when the label names what its declarations own
- ALWAYS keep a type's members, nested types, union cases, and companion factories in the section of the type
- ALWAYS order inside a section by owner, then dependency, then domain order (lifecycle, severity, case order), then public before private
- ALWAYS place a registry, lookup table, or decoder that reads a later declaration in a section after it, `[CONSTANTS]` holds no such value

[LANGUAGES]: Each language maps its constructs to the same labels
- C#: a static class of extension or IO functions is `[OPERATIONS]`, a `PlugIn` or `Command` subclass `[COMPOSITION]`
- C#: static fields keep declaration order when a later initializer reads an earlier field
- Python: `TYPE_CHECKING` blocks and conditional imports join the preamble, `__all__` closes the file under `[EXPORTS]`
- TypeScript: runtime schemas and data classes are `[MODELS]`, Effect services `[SERVICES]`, `Layer` values `[COMPOSITION]`
- Swift: `@main` types are `[COMPOSITION]`, an `extension` sits in the section of the type it extends
- Bash: `readonly` values are `[CONSTANTS]`, `declare -Ar` maps `[TABLES]`, `main` `[COMPOSITION]`
- SQL: types and domains are `[TYPES]`, tables `[MODELS]`, functions `[OPERATIONS]`, indexes, triggers, and grants `[COMPOSITION]`
- Configuration files: sections name the tool or table they configure (`[RUFF]`, `[TOOLS]`)
