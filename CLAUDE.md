# [MONOREPO_STANDARDS]

@README.md

- Commits, pushes, and writing or running tests happen on user request only
- Facts in repo, docs, tools, or session prompts are acted on, never asked; guidance and briefs never mandate asking the user
- Every language uses functional programming: domain logic stays pure and expression-oriented, imperative code stays at system boundaries
- Data dependency decides composition: dependent operations bind and short-circuit, independent operations combine and accumulate every error
- Language idioms differ but composition rules do not, when a language lacks a result type, adopt a dependency's, else define one
- Defects and checker findings are fixed at their cause, never hidden by a suppression, lowered level, guard, fallback, or retry
- Removals delete every mention and adjust each consumer to the absence, nothing stands in for removed content
- Docs and repo content answer first, applications and hosts run live when a question needs the running program
- Apps and windows an agent opens take no focus and stay behind the user's work, inside or outside the project
- Messages, docstrings, descriptions, and annotations are agent-facing: exact subject, values over prose, one action, no word that adds nothing
- Hooks rewrite a wrong command form or refuse it with one short reason, cover forms the user named, and block nothing the user asks for
- Settings differing by project, platform, or build mode use the tool's own switch (profile, condition, glob, per-OS file), never a script or flag
- Tools agents call carry the capability inside: one parameterized entry, no flag or step an agent could get wrong, settled result returned
- Adding a project, tool, or setting edits only its own file, no central list or second file changes with it
- Changes extend the target, setting, or package already doing that job, never a parallel script or one-off flag

## [01]-[TOOLING]

[TOOL_ROUTING]:
- ALWAYS load the coding skill of each language a change writes or reviews, and the host's skill for code a host runs
- ALWAYS use `ast-grep outline <path>` to choose what to read in an unread file or folder
- ALWAYS use `watch-apps` skill whenever work touches a desktop app's process or windows
- ALWAYS use `refactor-code` skill whenever code or configuration is written, reduced, rebuilt, or judged for quality
- ALWAYS use `search-web` skill for any fact outside the repository and its installed packages, never answered from memory
- ALWAYS use `search-code` skill before writing or judging code over a dependency, and before hand-writing what a package could supply
- ALWAYS use `secrets` skill whenever a command, build, service, or agent needs a credential
- ALWAYS use `github` MCP for any GitHub fact or action, `gh` for work on the local checkout
- ALWAYS use `clean-prose` skill for any English a change writes, names and agent-facing text included
- ALWAYS use `dotnet-roslyn-codelens` skill to read, trace, or change any file in a .NET solution
- ALWAYS use `LSP` tool for references, implementations, call hierarchy, and type of a Python, Rust, or Swift symbol before changing or removing it
- ALWAYS use `xcode` MCP for any Apple platform fact or Xcode build, `lldb` MCP for a debug session `xcode` MCP did not start
- ALWAYS use `claudeCodeDocs`/`openaiDeveloperDocs` MCP for any fact about Claude Code or Codex, their configuration included
- ALWAYS use `use-rhino` skill for any work against a live Rhino document, `.3dm` file, or Grasshopper 2 canvas
- ALWAYS use `use-blender` skill for any work against a live Blender or a `bpy` member of the installed build

[CLI_TOOLING]:

| [INDEX] | [TOOL]      | [GUIDANCE]                                                                                                  |
| :-----: | :---------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `outline`   | `ast-grep outline <paths> --view expanded` lists signatures and members, `--items exports` the public API   |
|  [02]   | `tree`      | `tree <dir>` lists directories and files, `-D` only directories                                             |
|  [03]   | `loc`       | `loc <dir>` counts code lines with complexity score per file and folder                                     |
|  [04]   | `fd`        | `fd <pattern> <path>` finds files and directories, a skill or MCP covering that area supersedes it          |
|  [05]   | `rg`        | `rg <pattern> <paths>` for literals, comments, and prose, code structure goes to `ast-grep`                 |
|  [06]   | `jq`/`yq`   | `yq '.expr' f`, `yq` has no `r` subcommand, `jq` needs `-r` for shell values and `[]?` on optional arrays   |
|  [07]   | `sd`        | `sd '<regex>' '<replacement>' <files>` by line, `-A` for a find across lines, `-F` fixed, `--` before a `-` |
|  [08]   | `difft`     | `difft <before> <after>` for a syntax-tree diff, `GIT_EXTERNAL_DIFF=difft git diff` over a change           |
|  [09]   | `hyperfine` | `hyperfine -r <runs> '<command>'` times commands under the same controls, `-S default` adds a shell         |
|  [10]   | `duckdb`    | `duckdb -c '<sql>'` queries CSV, Parquet, and JSON files in place, `-json` for machine output               |

## [02]-[IMPLEMENTATION_STANDARDS]

Code calls the API that provides a capability directly, and any layer in between adds something that API lacks

- ALWAYS make invalid states unrepresentable at construction, every consumer receives a validated value without re-validating it
- ALWAYS take a dependency as a parameter or from the runtime providing it, never through a service locator

Values come from the code, host, or package that already states them:
- ALWAYS declare a fact no host or package exposes once, in the code responsible for it, and compute every other use from it
- ALWAYS reference members by compiler-checked symbol or generated accessor, never a name string, hand-written lookup per member, or custom generator
- ALWAYS derive values from the operation and its target, a caller passing what the operation knows (value, flag, description) is a defect
- ALWAYS let behavior and layout follow from how many parts callers supply, of which kind, and how nested, callers pass content and no mode
- ALWAYS set a per-user host setting through that host's API at plug-in load
- ALWAYS fetch a third-party file no package holds by pinned commit and checksum, each variant of a hand-made master (font, SVG mark) builds from it

## [03]-[DEPENDENCY_POLICY]

[DEPENDENCY_SOURCES]: External dependencies, SDKs, and APIs are primary sources
- ALWAYS group project file items and dependency rows by responsibility and order each group consistently
- ALWAYS close a dependency gap with a package, its configuration, a newer release, or a typed decode at the boundary, never a copy of its types
- ALWAYS target the newest package, host, and language release, prereleases included, an older release from memory is a defect
- ALWAYS add a missing dependency entry to the project file or catalog that declares it, existing entries stay
- ALWAYS reference a package directly in every project that names its types, a transitive reference supplies no global using, alias, or analyzer

## [04]-[CODE_QUALITY]

Architecture: no folder holding one file, no flat folders, no tiny or oversized files, no `.d.ts`, no `src/`, no coined names (clean-prose):
- All code is library-shaped, written as a package another project imports, with API for integration that couples to no caller
- Rebuild code with its consumers, never move, alias, re-export, forward, wrap, or extract helpers, and leave no adapter or old path
- Reduce surface (internal and external), lines, and logic by removing content and folding its capability into a smaller surface, never into nesting
- Public API never goes for lacking readers: reader count folds private and package-internal declarations alone, weak public API is rebuilt in place
- New code adds a capability the existing code lacked, at the depth and quality of the code it joins, nothing speculative

- ALWAYS treat external packages as first class: newest release, every capability integrated, deep usage over surface usage
- ALWAYS derive values and logic from the host, packages, and declarations, a hand-rolled, hardcoded, or wrapped form of them is a defect
- NEVER let a string decide behavior: rebuild the logic over the value it stands for, never an enum renaming the strings
- NEVER nest code more than 3 levels, flatten it by rebuilding the logic chain, never by extracting helpers or forwarding
- Declared structs, enums, types, constants, and strings stay only when no host, package, or declaration states their fact and they remove logic
- Fewer, richer objects: one object replaces objects of different types, and consumers are rebuilt to its shape
- Values and control flow are algebraic: one parameterized value, field, or member replaces a family of near-identical ones
- Logic walks and parses its input once, bounds recursion, and looks keyed data up by key, never a repeated pass or linear search

## [05]-[FILE_ORGANIZATION]

Source files group declarations into sections and a type's members into groups named by what they do, `<marker>` is the language's line comment:

```text
<marker> --- [<SECTION>] -----------------------------------------------------------------

<type>

    <marker> --- [<GROUP>]

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
- ALWAYS keep code files free of comments other than section and subsection dividers
- ALWAYS open a section with a full divider: comment marker, `---`, one `[UPPER_SNAKE]` label, dashes to column 90
- ALWAYS open a subsection with a divider without dashes, indented with its members, when one type or section holds more than one member group
- ALWAYS place every declaration under a divider, the preamble (imports, namespace, shebang, `set` flags, assembly attributes) precedes the first one
- ALWAYS place every member of a subdivided type or section under a `[<GROUP>]` subsection that names what its members do
- ALWAYS keep a type's fields in one group with the members that only set or compute them, and each constant or helper in the group reading it
- ALWAYS place every declaration and table of one concept in one section, each group under a subsection in place of a separate section
- ALWAYS add a `[<SECTION>]` label the table lacks only when it names its declarations better, placed after the closest table section
- ALWAYS keep a type's members, nested types, union cases, and companion factories in the section of the type
- ALWAYS order inside a section by type, then dependency, then domain order (lifecycle, severity, case order), then public before private
- ALWAYS place a registry, lookup table, or decoder that reads a later declaration in a section after it, `[CONSTANTS]` holds no such value

[LANGUAGES]: Each language maps its constructs to the same labels
- C#: a static class of extension or IO functions is `[OPERATIONS]`, a `PlugIn` or `Command` subclass `[COMPOSITION]`
- C#: static fields keep declaration order when a later initializer reads an earlier field
- Python: `TYPE_CHECKING` blocks and conditional imports join the preamble, `__all__` under `[EXPORTS]` closes a module other modules import
- TypeScript: runtime schemas and data classes are `[MODELS]`, Effect services `[SERVICES]`, `Layer` values `[COMPOSITION]`
- Swift: `@main` types are `[COMPOSITION]`, an `extension` sits in the section of the type it extends
- Rust: traits and unit enums are `[TYPES]`, payload enums `[MODELS]`, error enums `[ERRORS]`
- Rust: `impl` blocks sit in the section of their type, a lint pass `impl` in `[COMPOSITION]`
- Rust: `mod` lines are `[COMPOSITION]`, `pub use` lists `[EXPORTS]`
- Bash: `readonly` values are `[CONSTANTS]`, `declare -Ar` maps `[TABLES]`, `main` `[COMPOSITION]`
- SQL: types and domains are `[TYPES]`, tables `[MODELS]`, functions `[OPERATIONS]`, indexes, triggers, and grants `[COMPOSITION]`
- Configuration files: sections name the tool or table they configure (`[RUFF]`, `[TOOLS]`)
