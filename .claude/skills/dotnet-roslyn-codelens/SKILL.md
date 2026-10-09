---
name: dotnet-roslyn-codelens
description: "Use when reading, navigating, diagnosing, or refactoring a C#, .csproj, or solution file through Roslyn in place of grep, Read, and dotnet build."
---

# [DOTNET_ROSLYN_CODELENS]

Covers the roslyn-codelens MCP server over a loaded solution. Tools resolve every symbol through the compilation, and text search matches characters and misses aliases, partial types, generic instantiations, and metadata symbols.

## [01]-[TOOL_SELECTION]

Server tools are deferred, one `ToolSearch(query: "select:mcp__roslyn-codelens__<tool>,mcp__roslyn-codelens__<tool>")` loads the schema of each tool a task calls before the first call.

Before a text search on a `.cs`, `.razor`, or `.cshtml` file, the target decides the tool:
1. C# symbol (type, member, or namespace) → `mcp__roslyn-codelens__search_symbols` or `mcp__roslyn-codelens__find_references`
2. Attribute → `mcp__roslyn-codelens__find_attribute_usages`
3. Reflection pattern → `mcp__roslyn-codelens__find_reflection_usage`
4. String literal, comment, or other plain text → `rg`

Before `Read` on a `.cs` file, the wanted view decides:
1. File structure → `mcp__roslyn-codelens__get_file_overview` or `mcp__roslyn-codelens__get_type_overview`
2. One method's shape → `mcp__roslyn-codelens__analyze_method`
3. Source of one or more members → `mcp__roslyn-codelens__get_method_source` with every name in one call
4. Exact lines to edit → `Read`

## [02]-[TOOL_INDEX]

| [INDEX] | [TOOL]                                             | [USE_WHEN]                                                                      |
| :-----: | :------------------------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `mcp__roslyn-codelens__find_implementations`       | "What implements this interface?" / "What extends this class?"                  |
|  [02]   | `mcp__roslyn-codelens__find_callers`               | "Who calls this method?" / "What depends on this?"                              |
|  [03]   | `mcp__roslyn-codelens__find_event_subscribers`     | "Who subscribes to this event?"                                                 |
|  [04]   | `mcp__roslyn-codelens__find_references`            | "Where is this symbol used?" / "Who writes to it?" (`kinds` filter)             |
|  [05]   | `mcp__roslyn-codelens__find_tests_for_symbol`      | "What tests cover this method?" / "Which tests will break if I change X?"       |
|  [06]   | `mcp__roslyn-codelens__get_test_summary`           | "What does this test suite cover?"                                              |
|  [07]   | `mcp__roslyn-codelens__find_uncovered_symbols`     | "Which public members does no test reach?"                                      |
|  [08]   | `mcp__roslyn-codelens__go_to_definition`           | "Where is this defined?" / "Jump to source"                                     |
|  [09]   | `mcp__roslyn-codelens__search_symbols`             | "Find types or methods matching this name"                                      |
|  [10]   | `mcp__roslyn-codelens__get_type_hierarchy`         | "What is the inheritance chain?"                                                |
|  [11]   | `mcp__roslyn-codelens__get_symbol_context`         | "Give me everything about this type"                                            |
|  [12]   | `mcp__roslyn-codelens__get_public_api_surface`     | "What is the public API of this library?"                                       |
|  [13]   | `mcp__roslyn-codelens__find_breaking_changes`      | "Will this break consumers?"                                                    |
|  [14]   | `mcp__roslyn-codelens__get_di_registrations`       | "Where is this registered?" / "What is the DI lifetime?"                        |
|  [15]   | `mcp__roslyn-codelens__get_project_dependencies`   | "What does this project reference?"                                             |
|  [16]   | `mcp__roslyn-codelens__get_nuget_dependencies`     | "What packages does this project use?" / "What is the assembly name?"           |
|  [17]   | `mcp__roslyn-codelens__find_reflection_usage`      | "Is this used dynamically?"                                                     |
|  [18]   | `mcp__roslyn-codelens__find_attribute_usages`      | "Find all [Authorize] controllers" / "What holds this attribute?"               |
|  [19]   | `mcp__roslyn-codelens__find_obsolete_usage`        | "What calls an `[Obsolete]` member?" / "Which deprecations are pending?"        |
|  [20]   | `mcp__roslyn-codelens__get_diagnostics`            | "Are there compiler errors?" / "Show warnings" / "Will this build?"             |
|  [21]   | `mcp__roslyn-codelens__get_code_fixes`             | "How do I fix this warning?"                                                    |
|  [22]   | `mcp__roslyn-codelens__trust_solution`             | "Authorize analyzers for another solution"                                      |
|  [23]   | `mcp__roslyn-codelens__list_trusted_paths`         | "Is this solution trusted?"                                                     |
|  [24]   | `mcp__roslyn-codelens__revoke_trust`               | "Withdraw analyzer trust for this path"                                         |
|  [25]   | `mcp__roslyn-codelens__get_code_actions`           | "What refactorings are available here?"                                         |
|  [26]   | `mcp__roslyn-codelens__apply_code_action`          | "Apply this refactoring" / "Extract method"                                     |
|  [27]   | `mcp__roslyn-codelens__rename_symbol`              | "Rename this symbol everywhere" / "Change this name across the solution"        |
|  [28]   | `mcp__roslyn-codelens__change_signature`           | "Add, remove, or reorder a parameter and fix all the callers"                   |
|  [29]   | `mcp__roslyn-codelens__resolve_stack_trace`        | "Where did this exception come from?" / "Resolve this stack trace"              |
|  [30]   | `mcp__roslyn-codelens__find_unused_symbols`        | "Is there dead code?"                                                           |
|  [31]   | `mcp__roslyn-codelens__get_complexity_metrics`     | "Which methods are too complex?" / "What do I refactor first?"                  |
|  [32]   | `mcp__roslyn-codelens__find_naming_violations`     | "Check naming conventions"                                                      |
|  [33]   | `mcp__roslyn-codelens__find_async_violations`      | "Are there async bugs?" / "Find sync-over-async"                                |
|  [34]   | `mcp__roslyn-codelens__find_disposable_misuse`     | "Are there resource leaks?" / "Find a missing `using`"                          |
|  [35]   | `mcp__roslyn-codelens__get_exception_flow`         | "What can escape this method?" / "Where does this exception get caught?"        |
|  [36]   | `mcp__roslyn-codelens__find_throw_sites`           | "Where is this exception type thrown?"                                          |
|  [37]   | `mcp__roslyn-codelens__find_catch_blocks`          | "Who catches this?" / "What is swallowing exceptions?"                          |
|  [38]   | `mcp__roslyn-codelens__find_large_classes`         | "Find classes that need splitting"                                              |
|  [39]   | `mcp__roslyn-codelens__find_god_objects`           | "Which classes are doing too much?"                                             |
|  [40]   | `mcp__roslyn-codelens__find_circular_dependencies` | "Are there circular dependencies?"                                              |
|  [41]   | `mcp__roslyn-codelens__check_architecture`         | "Is anything violating our layering?" / "Does Domain reference Infrastructure?" |
|  [42]   | `mcp__roslyn-codelens__get_project_health`         | "How is this project doing?" / "Top hotspots across all dimensions"             |
|  [43]   | `mcp__roslyn-codelens__get_source_generators`      | "What source generators are active?"                                            |
|  [44]   | `mcp__roslyn-codelens__get_generated_code`         | "Show generated code"                                                           |
|  [45]   | `mcp__roslyn-codelens__inspect_external_assembly`  | "What does this NuGet package expose?" / "Show me the API of X assembly"        |
|  [46]   | `mcp__roslyn-codelens__peek_il`                    | "Show IL for this method" / "What does this external method do?"                |
|  [47]   | `mcp__roslyn-codelens__list_solutions`             | "What solutions are loaded?"                                                    |
|  [48]   | `mcp__roslyn-codelens__load_solution`              | "Load this .sln or .slnx at run time"                                           |
|  [49]   | `mcp__roslyn-codelens__unload_solution`            | "Free memory for this solution"                                                 |
|  [50]   | `mcp__roslyn-codelens__set_active_solution`        | "Switch to project B"                                                           |
|  [51]   | `mcp__roslyn-codelens__rebuild_solution`           | "Reload the solution" / "Diagnostics are stale"                                 |
|  [52]   | `mcp__roslyn-codelens__start_background_task`      | "Run a long rebuild without blocking"                                           |
|  [53]   | `mcp__roslyn-codelens__get_task_status`            | "Check on a queued background task"                                             |
|  [54]   | `mcp__roslyn-codelens__list_running_tasks`         | "Which background tasks are running?"                                           |
|  [55]   | `mcp__roslyn-codelens__analyze_data_flow`          | "Which variables are read or written here?"                                     |
|  [56]   | `mcp__roslyn-codelens__analyze_control_flow`       | "Is this code reachable?"                                                       |
|  [57]   | `mcp__roslyn-codelens__analyze_change_impact`      | "What breaks if I change this?"                                                 |
|  [58]   | `mcp__roslyn-codelens__get_type_overview`          | "Give me everything about this type in one call"                                |
|  [59]   | `mcp__roslyn-codelens__analyze_method`             | "Show signature, callers, and outgoing calls"                                   |
|  [60]   | `mcp__roslyn-codelens__get_method_source`          | "Show me this method's body" / "Give me the source of these members"            |
|  [61]   | `mcp__roslyn-codelens__get_overloads`              | "What overloads does this method have?"                                         |
|  [62]   | `mcp__roslyn-codelens__get_extension_methods`      | "What can I call on this type?" / "Is there an extension for X?"                |
|  [63]   | `mcp__roslyn-codelens__get_instantiation_options`  | "How do I construct this?" / "Why can I not `new` this up?"                     |
|  [64]   | `mcp__roslyn-codelens__get_operators`              | "What operators does this type define?"                                         |
|  [65]   | `mcp__roslyn-codelens__get_call_graph`             | "Transitive callers or callees, depth-bounded"                                  |
|  [66]   | `mcp__roslyn-codelens__get_file_overview`          | "What types are in this file?"                                                  |

## [03]-[TOOL_REFERENCE]

### [03.1]-[SYMBOL_NAVIGATION]

- `mcp__roslyn-codelens__go_to_definition` — the file and line where a symbol is declared
- `mcp__roslyn-codelens__search_symbols` — case-insensitive substring lookup over types, methods, properties, and fields
- `mcp__roslyn-codelens__find_references` — each occurrence in the solution as one item with a `ReferenceKind`, `kinds` filters on the server
- `mcp__roslyn-codelens__resolve_stack_trace` — maps a pasted .NET stack trace to file, line, and symbol, items keep trace order
    - Demangles async and iterator state machines, lambdas, and local functions
    - Parses log-prefixed lines, inner-exception chains, and Demystifier traces
    - Source frames get their declaration site, or the exact location when the trace holds `in file:line`
    - Frames in a referenced assembly resolve with `origin="metadata"`, every other frame with `origin="unresolved"`
    - Unparsable frame-like lines stay in place as `kind="unknown"`

Reference kinds:
- `read`, `write` (assignment target, `out` argument), `readwrite` (compound assignment, `++`/`--`, `ref` argument)
- `invocation`, `method_group` (method used as a delegate)
- `object_creation`, `cast` (`(T)x`, `x as T`)
- `type_check` (`x is T`, `is T v`, `case T v:`), `typeof`, `base_type`, `type_constraint`, `type_argument`
- `declaration` (variable, parameter, return, and field type positions), `attribute`, `nameof`, `xml_doc` (`<see cref=...>`)
- `usage` is a rare fallback
- Receivers count as `read`, `_map[k] = v` and `_map.Add(k, v)` read field `_map` while its contents change

### [03.2]-[READING_TYPES_AND_MEMBERS]

- `mcp__roslyn-codelens__get_file_overview` — types and diagnostics of a file, `.razor` and `.cshtml` resolve to their generated C# document
- `mcp__roslyn-codelens__get_type_overview` — context, hierarchy, and file diagnostics in one call
- `mcp__roslyn-codelens__get_symbol_context` — namespace, base type, interfaces, injected dependencies, and public members
- `mcp__roslyn-codelens__get_type_hierarchy` — inheritance chains and extension points
- `mcp__roslyn-codelens__analyze_method` — signature, callers, and outgoing calls in one call
- `mcp__roslyn-codelens__get_call_graph` — transitive caller and callee graph with cycle detection, for a depth greater than 1
    - `direction` defaults to `callees`, `callers` or `both` walks inbound
    - `maxDepth` defaults to 3, `maxNodes` to 500
- `mcp__roslyn-codelens__get_method_source` — full declaration source for one or many members in one call, whole types are out of scope
    - Members: methods (all overloads), constructors (`Type.Type`, nested types fully qualified), properties, fields, events
    - Indexers take `Type.this` or `Type.this[]`
    - Each item holds a status (`ok`, `notFound`, `ambiguous`, `metadata`, `unsupportedKind`), one miss leaves the batch intact
    - `metadata` items hold `origin` for `mcp__roslyn-codelens__peek_il`
- `mcp__roslyn-codelens__get_overloads` — every overload of a method or constructor from source and metadata, with full parameter and modifier detail
- `mcp__roslyn-codelens__get_operators` — every user-defined operator and conversion a type declares (operators do not inherit)
    - Synthesized record equality and checked variants are included
- `mcp__roslyn-codelens__get_extension_methods` — every extension member that applies to a type, from the solution and referenced assemblies
    - LINQ appears for an `IEnumerable`
    - Applicability follows the compiler's reduction: `this IEnumerable<T>` applies to `string`, `this IEnumerable<string>` does not
    - `signature` is the reduced call-site form with the return type first (`IEnumerable<int> Where<int>(Func<int, bool>)`)
    - `isStatic: false` is an instance call (`value.Doubled()`) and covers every classic `this` extension
    - `isStatic: true` is a C# 14 static extension member called on the type (`int.Zero`)
    - Receivers can be keywords, constructed generics, arrays, nullables, or tuples
    - Results ignore `using` scope, `namespace` is always reported, and the import can still be missing
    - Source sorts before metadata, `nameFilter` narrows by substring
- `mcp__roslyn-codelens__get_instantiation_options` — a type's `constructors`, `factories`, `diRegistrations`, and `requiredMembers` in one call
    - `constructors` hold parameters, accessibility, `isImplicit` for the compiler-supplied parameterless constructor, and `isObsolete`
    - `factories` are static members anywhere in the solution that return the type (`WidgetFactory.Create()` for a type with a private constructor)
    - `Task<T>` and `ValueTask<T>` factories are unwrapped and flagged `isAsync`, instance builders are excluded
    - `fromProject` computes `accessible` from that project and honors `InternalsVisibleTo`, `accessible: null` means not computed
    - Interfaces, abstract classes, and static classes report `instantiable: false` with a `note`, `mcp__roslyn-codelens__find_implementations` follows
- `mcp__roslyn-codelens__get_public_api_surface` — every public and protected type and member declared in production projects
    - Test projects, generated code, internal symbols, protected members on sealed types, and inherited members are skipped
- `mcp__roslyn-codelens__find_breaking_changes` — diffs the current public API surface against a baseline
    - Baselines are a JSON snapshot from `mcp__roslyn-codelens__get_public_api_surface` or a `.dll`
    - Return type changes, `sealed` changes, and nullable annotation changes are undetected, an empty diff leaves compatibility unknown

### [03.3]-[USAGE_AND_DEPENDENCIES]

- `mcp__roslyn-codelens__find_implementations` — every implementor of an interface and every type extending a class
- `mcp__roslyn-codelens__find_callers` — every call site for a method
- `mcp__roslyn-codelens__find_attribute_usages` — types and members decorated with a given attribute
- `mcp__roslyn-codelens__find_event_subscribers` — every `+=` and `-=` site of an event, with the resolved handler and a subscribe or unsubscribe tag
- `mcp__roslyn-codelens__find_reflection_usage` — coupling no reference reports: `Activator.CreateInstance`, `MethodInfo.Invoke`, and assembly scanning
- `mcp__roslyn-codelens__get_di_registrations` — a type's `IServiceCollection` registrations and lifetimes in generic, `typeof` pair, and factory forms
- `mcp__roslyn-codelens__get_project_dependencies` — direct and transitive project references of the required `project`
- `mcp__roslyn-codelens__get_nuget_dependencies` — NuGet packages and versions per project
- `mcp__roslyn-codelens__find_obsolete_usage` — every call site of an `[Obsolete]` symbol, grouped by deprecation message and severity, errors first
    - Metadata deprecations from packages are included, symbols with no usage are omitted
- `mcp__roslyn-codelens__find_tests_for_symbol` — xUnit, NUnit, and MSTest methods that exercise a production symbol
    - `transitive` walks through helper methods, bounded by `maxDepth` (default 3, maximum 5)
- `mcp__roslyn-codelens__get_test_summary` — test methods per project, the reverse of `mcp__roslyn-codelens__find_tests_for_symbol`
    - Each test reports framework, attribute kind, row count, location, and the production symbols it references
- `mcp__roslyn-codelens__find_uncovered_symbols` — public methods and properties no test reaches within 3 helper hops, sorted by cyclomatic complexity
    - Reference-based static analysis, reads no runtime coverage data

### [03.4]-[DIAGNOSTICS_AND_REFACTORING]

- `mcp__roslyn-codelens__get_diagnostics` — compiler errors, warnings, and analyzer diagnostics
    - `unreliable` in its summary means the solution loaded degraded, items can then name errors no real build reports
    - `mcp__roslyn-codelens__rebuild_solution` then a rerun clears `unreliable`, a block that persists leaves diagnostics to the build
- `mcp__roslyn-codelens__get_code_fixes` — structured edits for one diagnostic at one location
- `mcp__roslyn-codelens__get_code_actions` — every refactoring and fix available at a position, with an optional range
- `mcp__roslyn-codelens__apply_code_action` — runs a refactoring by title, preview by default, the in-memory snapshot updates at once on success
    - Title matching falls back to a case-insensitive substring match in both directions, a near-miss title runs a different action
    - `title` in the result names the action that ran
    - Apply refuses to write when a file changed on disk after the snapshot loaded and names the stale files
    - `mcp__roslyn-codelens__rebuild_solution` then a retry resolves a stale file
    - Actions that add a file write it to disk, and the watcher adds it to the snapshot
- `mcp__roslyn-codelens__rename_symbol` — solution-wide rename of a type or member through the Roslyn Renamer, preview by default
    - `mcp__roslyn-codelens__apply_code_action` offers no rename
    - Cascades to references, overrides, `nameof`, and crefs
    - `renameInComments` defaults to `true`, `renameInStrings` to `false`, `renameOverloads` to `true`
    - Locals, parameters, file renames, constructors, and metadata symbols are rejected, a constructor renames through its containing type
    - Apply refuses on new-compiler-error conflicts unless `force=true`, and refuses on files changed since the snapshot in every case
    - Generic types accept the arity-free name: `Data.Repository` finds `Repository<T>`
- `mcp__roslyn-codelens__change_signature` — adds, removes, and reorders a method's parameters and rewrites every call site
    - `mcp__roslyn-codelens__apply_code_action` offers no signature change
    - `operations` apply in order: `remove` takes a parameter name, `reorder` a full permutation of the surviving names
    - `add` takes `name`, `type`, and a required `callSiteValue`, the expression every existing call site passes
    - Optional `defaultValue` makes the parameter optional and leaves existing calls untouched
    - Named arguments, optional parameters, `params`, and the extension `this` are handled
    - Moving `this` off first position and leaving `params` anywhere but last are rejected
    - `cascadedTo` lists the rewritten overrides and interface implementations, read it in preview
    - Source-defined methods only, an overloaded name must be disambiguated, same refusals as `mcp__roslyn-codelens__rename_symbol`
- `mcp__roslyn-codelens__analyze_data_flow` — variable lifecycle over a statement range: declared, read, written, captured, and flowing in or out
- `mcp__roslyn-codelens__analyze_control_flow` — reachability, return statements, and exit points over a statement range

Code generation runs through `mcp__roslyn-codelens__apply_code_action` with the exact title `mcp__roslyn-codelens__get_code_actions` reads, each task with its title:
- Implement missing interface or abstract members → "Implement abstract members" / "Implement interface"
- Generate a constructor from fields → "Generate constructor"
- Add null checks → "Add null checks for all parameters"
- Generate `Equals` and `GetHashCode` → "Generate Equals and GetHashCode"
- Encapsulate a field → "Encapsulate field"
- Extract a method → "Extract method"
- Inline a variable → "Inline variable"

#### [03.4.1]-[ANALYZER_TRUST]

`mcp__roslyn-codelens__get_diagnostics` defaults to `includeAnalyzers=false` and returns compiler diagnostics only. Pass `includeAnalyzers=true` when the answer must match the build, and before `mcp__roslyn-codelens__get_code_fixes` for an analyzer diagnostic.

Solutions named on the server's command line are trusted for the session, every other solution needs `mcp__roslyn-codelens__trust_solution`. `mcp__roslyn-codelens__get_code_fixes` loads the fix providers as analyzer code and answers `SolutionNotTrusted` on an untrusted solution, compiler diagnostics included.

`analyzerPolicy` in the trust file (`roslyn-codelens/trust.json` under the user's application data directory, read at server start) selects the analyzer assemblies that load. `nuget-and-solution-bin`, the default, accepts `~/.nuget/packages`, the SDK directory, and a `bin` or `obj` path under the solution, `strict` drops the solution paths, `all` accepts every path. `nuget-and-solution-bin` spells `~/.nuget/packages` literally and reads no `globalPackagesFolder` from `NuGet.config`. Package analyzers restored to a relocated folder load under `all` alone, and `mcp__roslyn-codelens__get_diagnostics` misses their diagnostics under the other policies.

Analyzers run under `.editorconfig` severities with no analyzer option values. Style analyzers that read an option (`IDE0055` formatting) report against Roslyn defaults, their `mcp__roslyn-codelens__get_diagnostics` items are no finding. `nx run rasm:lint:dotnet-format` decides style findings, and `nx run rasm:format` writes formatting.

Analyzer assemblies built against a newer Roslyn than the server bundles do not load, and the server's startup log names each one as `ReferencesNewerCompiler`. Under a preview SDK, `mcp__roslyn-codelens__get_diagnostics` omits `CodeStyle` (`IDE*`) diagnostics, the build reports them until a server release bundles the SDK's Roslyn.

When a call returns `SolutionNotTrusted`, call `mcp__roslyn-codelens__trust_solution` and retry. `scope` defaults to `session`, `persistent` writes the path to the trust store, and `addRoot` with a directory trusts every solution below it. `mcp__roslyn-codelens__list_trusted_paths` reports the current state, `mcp__roslyn-codelens__revoke_trust` removes an entry.

### [03.5]-[EXCEPTION_ANALYSIS]

- `mcp__roslyn-codelens__get_exception_flow` — what can escape a method, with `escapes`, the `path`, and the catch site per exception
    - Walks callees, collects explicit throws, and propagates each one up through every enclosing `try`/`catch`
    - `origin` is `thrown` for a source throw site, or `documented` for an `exception` XML tag on a metadata callee
    - `includeDocumented: false` drops the `documented` items
    - `when`-filtered catches never count as catching, the filter can be false at run time
    - `hasFilter: true` marks an exception that passed such a clause, `escapes: false` always pairs with `hasFilter: false`
    - `maxDepth` defaults to 3, `maxNodes` to 500, either limit sets `truncated`
    - Throws inside a lambda or local function are excluded, they escape when the body runs
- `mcp__roslyn-codelens__find_throw_sites` — every throw of an exception type across the solution, a throw inside a lambda or local function included
    - `includeDerived` matches subclasses, a rethrowing `throw;` resolves to the enclosing catch's type
- `mcp__roslyn-codelens__find_catch_blocks` — every `catch` for a type, each item with `hasFilter`, `rethrows`, and `isEmpty`
    - `includeBaseClauses` adds `catch (Exception)` and general `catch`
    - Silent swallowing reads as `isEmpty: true, rethrows: false`

Tools see explicit `throw` only, implicit run-time exceptions (null dereference, division by zero) and reflection-invoked throws stay unreported. `mcp__roslyn-codelens__get_exception_flow` alone walks calls, and it resolves each call to its declared symbol in place of a run-time override. `mcp__roslyn-codelens__get_exception_flow` models an `async` method as synchronous, a throw inside it propagates at the call site and an enclosing `try` counts as catching it. Synchronous modeling matches an awaited call and misses fire-and-forget (`_ = M();`), where nothing enclosing sees the throw.

### [03.6]-[CODE_QUALITY]

- `mcp__roslyn-codelens__get_project_health` — composite audit per project with counts and the top hotspots per dimension
    - Dimensions: complexity, large classes, naming, unused symbols, reflection, async violations, and disposable misuse
    - `hotspotsPerDimension` defaults to 5, 0 gives counts only
- `mcp__roslyn-codelens__find_unused_symbols` — dead code, found by reference, with the exclusion counts in `summary.filteredOut`
    - Test methods, MCP tool entry points, source-generator output, MEF-composed services, and interop-laid-out fields are excluded
- `mcp__roslyn-codelens__find_naming_violations` — .NET naming conventions
- `mcp__roslyn-codelens__find_async_violations` — async misuse across production projects, each violation with a severity
    - Sync-over-async (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`) and `async void` outside an event handler
    - Missing `await` in an `async` method and fire-and-forget tasks
    - Test projects and generated code are skipped
- `mcp__roslyn-codelens__find_disposable_misuse` — `IDisposable` and `IAsyncDisposable` locals at risk of leaking
    - Locals not wrapped in `using` or `await using`, returned, or assigned to a field or `out` parameter are a warning
    - Discarded disposable creator or factory calls are an error
    - Methods only, ownership transfer through an argument is undetected, test projects and generated code are skipped
- `mcp__roslyn-codelens__find_large_classes` — types over a member count or line count threshold
- `mcp__roslyn-codelens__find_god_objects` — types over all 3 size thresholds and at least one coupling threshold, a large isolated class is skipped
    - Defaults are 300 lines, 15 members, 10 fields, 5 incoming namespaces, and 5 outgoing namespaces, each configurable
- `mcp__roslyn-codelens__find_circular_dependencies` — cycles in the project graph or the namespace graph
- `mcp__roslyn-codelens__check_architecture` — layering rules supplied inline, `scope` selects `namespace` (default) or `project`
  - `forbid` (`Domain.*` must not depend on `Infrastructure.*`) catches the expected violation
    - `allowOnly` (`Api.*` can depend only on `Application.*` and `Domain.*`) catches the rest
    - Edges come from resolved symbols
    - `allowOnly` evaluates solution-internal, non-generated targets alone, edges to framework namespaces and generator output stay unchecked unless `forbid` names them
    - Self-references are never a violation
    - Results group per violated edge with a full `referenceCount` and the first `maxSitesPerViolation` sites

`mcp__roslyn-codelens__get_complexity_metrics` rows hold complexity per member (methods, constructors, properties, indexers, operators):
- `complexity` — cyclomatic, the number of independent paths through the member, a straight-line method scores 1
- `cognitive` — how hard the member is to follow, a 0 means nothing branches, it ranks refactoring work
    - Nesting costs extra, a whole `switch` costs 1, `else`/`else if` cost 1 with no nesting penalty
    - Flat 20-case dispatch and 4 nested `if` levels share one cyclomatic number and differ in cognitive
- `maxNesting` — the deepest control structure, lambda and local-function bodies included

`metric` (`"cyclomatic"` by default, or `"cognitive"`) selects the number `threshold` and the sort use. Both numbers always appear in the response.

### [03.7]-[SOURCE_GENERATORS]

- `mcp__roslyn-codelens__get_source_generators` — the generators active per project and their outputs
- `mcp__roslyn-codelens__get_generated_code` — the generated source, filtered by generator or by file path

Location-returning results hold an `isGenerated` flag, and the next compile rewrites generator output. Read the flag before editing a match.

### [03.8]-[EXTERNAL_ASSEMBLIES]

Only assemblies the loaded solution references can be inspected. Unreferenced assemblies need a `PackageReference` or a `Reference` in a project, then `mcp__roslyn-codelens__rebuild_solution`.

- `mcp__roslyn-codelens__get_nuget_dependencies` — first call, gives the exact assembly name the other tools take
- `mcp__roslyn-codelens__inspect_external_assembly` — what an assembly exposes, drill in before concluding that a package exposes nothing
    - `mode` defaults to `summary` and returns the namespace tree with type counts alone
    - `mode: "namespace"` with `namespaceFilter` gives the public types and members of one namespace
- `mcp__roslyn-codelens__peek_il` — a method's IL by fully qualified method name with parameter types

External types resolve by name in `mcp__roslyn-codelens__go_to_definition`, `mcp__roslyn-codelens__get_symbol_context`, `mcp__roslyn-codelens__get_type_overview`, and `mcp__roslyn-codelens__get_type_hierarchy`, and `mcp__roslyn-codelens__find_references`, `mcp__roslyn-codelens__find_callers`, and `mcp__roslyn-codelens__find_implementations` find the code that uses them.

### [03.9]-[SOLUTION_MANAGEMENT]

Solutions named on the server's command line load with the first one active. With none named, the server loads the `.sln` or `.slnx` found walking up from the working directory. `ROSLYN_CODELENS_OPEN_PROJECT_TIMEOUT_SECONDS` (default `300`) bounds each project's load, a project over it joins `skippedProjects` with `kind: "Timeout"` and the rest of the solution loads. Edits to `.cs`, `.csproj`, `.props`, and `.targets` files recompile the affected projects on the next tool call with no further action.

- `mcp__roslyn-codelens__rebuild_solution` — a full reload: re-open the solution, recompile every project, rebuild every index
    - Serves a package or analyzer change, and results that stay stale after an edit
- `mcp__roslyn-codelens__load_solution` — loads a `.sln` or `.slnx` at run time and makes it active
    - Current solution stays active until the load succeeds
    - `include` takes case-insensitive globs with `*` and `?` only, `rootProjects` takes exact, case-sensitive names
    - Both match the project file name without its extension, and both seed a transitive `ProjectReference` closure
    - Filters that match nothing are an error, load with no filter first and read the names from `mcp__roslyn-codelens__list_solutions`
    - For a solution that takes minutes to open, `background: true` returns a `taskId` for `mcp__roslyn-codelens__get_task_status`
- `mcp__roslyn-codelens__list_solutions` — the loaded solutions and the active one, read when an expected symbol is missing
    - `skippedProjects` per entry names each skipped project's `kind` and `reason`
- `mcp__roslyn-codelens__set_active_solution` — switches the active solution by partial name
- `mcp__roslyn-codelens__unload_solution` — frees memory, the remaining loaded solution becomes active
- `mcp__roslyn-codelens__start_background_task` — queues a long tool for `mcp__roslyn-codelens__get_task_status`
    - `mcp__roslyn-codelens__rebuild_solution` is the only allowed tool
- `mcp__roslyn-codelens__get_task_status` — the status, result, or error of one background task
- `mcp__roslyn-codelens__list_running_tasks` — background tasks running or finished within the last 5 minutes

## [04]-[CHANGE_WORKFLOW]

1. `mcp__roslyn-codelens__get_type_overview` — context, hierarchy, and diagnostics
2. `mcp__roslyn-codelens__analyze_change_impact` — every file, project, and call site the change affects
3. `mcp__roslyn-codelens__find_references` — every reference by kind
4. `mcp__roslyn-codelens__find_callers` — every call site
5. `mcp__roslyn-codelens__find_implementations` — every implementor and derived type
6. `mcp__roslyn-codelens__get_project_dependencies` — the position of the project in the reference graph
7. `mcp__roslyn-codelens__get_di_registrations` — container registrations and lifetimes
8. `mcp__roslyn-codelens__find_reflection_usage` — coupling that no reference reports
9. `mcp__roslyn-codelens__find_attribute_usages` — attribute-driven behavior
10. `mcp__roslyn-codelens__get_diagnostics` — existing errors and warnings
11. `mcp__roslyn-codelens__get_code_fixes` / `mcp__roslyn-codelens__get_code_actions` — code fixes and refactorings
12. `mcp__roslyn-codelens__apply_code_action` — the chosen fix or refactoring
13. `mcp__roslyn-codelens__find_unused_symbols` — dead code to delete

Name concrete types, interfaces, and call sites: "`IUserService` implementations: `UserService`, `CachedUserService`, `AdminUserService`".

## [05]-[RESPONSE_ENVELOPE]

Every list-returning tool wraps its results in one envelope:

```json
{
    "items": [...],
    "totalCount": 142,
    "truncated": false,
    "limit": 500,
    "summary": { ... }
}
```

When `truncated` is `true`, `items` holds the top N in the tool's own sort order: severity-first, worst-first, or by-project. Raise `limit` when the missing items change the answer. Each tool sets its own `limit` default: `mcp__roslyn-codelens__get_diagnostics` 1000, `mcp__roslyn-codelens__find_references` 500, `mcp__roslyn-codelens__get_complexity_metrics` 100, `mcp__roslyn-codelens__list_solutions` 50.

Single-object tools (`mcp__roslyn-codelens__get_type_overview`, `mcp__roslyn-codelens__apply_code_action`) return their own shape.

Tools that add a `summary` aggregate:
- `mcp__roslyn-codelens__get_diagnostics` — `{ error, warning, info, hidden }` counts, with an `unreliable` block when the solution loaded degraded
- `mcp__roslyn-codelens__find_references` — `{ byProject: { name: count }, byKind: { kind: count } }`
- `mcp__roslyn-codelens__find_callers`, `mcp__roslyn-codelens__find_attribute_usages` — `{ byProject: { name: count } }`
- `mcp__roslyn-codelens__search_symbols`, `mcp__roslyn-codelens__find_reflection_usage` — `{ byKind: {...} }`
- `mcp__roslyn-codelens__find_throw_sites`, `mcp__roslyn-codelens__find_catch_blocks` — `{ byType: {...}, byProject: {...} }`
- `mcp__roslyn-codelens__find_unused_symbols` — `{ byKind, filteredOut: { testMethod, testContainer, mcpTool, generated, composition, interop } }`
- `mcp__roslyn-codelens__find_naming_violations` — `{ byRule: {...} }`
- `mcp__roslyn-codelens__find_uncovered_symbols` — coverage counts with `riskHotspotCount`, the uncovered members with complexity 5 or more
- `mcp__roslyn-codelens__check_architecture` — `{ byRule, totalReferences, rulesEvaluated }`
- `mcp__roslyn-codelens__resolve_stack_trace` — `{ byOrigin: { source, metadata, unresolved }, exceptions, skippedFrameLike }`
    - `skippedFrameLike` counts frame-like lines that did not parse
- `mcp__roslyn-codelens__get_complexity_metrics` — `{ max, avg, overThreshold, maxCognitive }`
    - `max`, `avg`, and `overThreshold` describe the selected `metric`, `maxCognitive` is always the cognitive number

## [06]-[ERROR_CODES]

When a tool cannot proceed, the response has `isError: true` and a JSON body of `{ code, message, details? }`. Switch on `code`:

| [INDEX] | [CODE]               | [MEANING]                                                             |
| :-----: | :------------------- | :-------------------------------------------------------------------- |
|  [01]   | `SymbolNotFound`     | Type, method, or property did not resolve                             |
|  [02]   | `SolutionNotTrusted` | Analyzers requested before `mcp__roslyn-codelens__trust_solution` ran |
|  [03]   | `AmbiguousMatch`     | Many matches, listed in `details.matches`                             |
|  [04]   | `FileNotFound`       | File path or baseline does not exist                                  |
|  [05]   | `ProjectNotFound`    | Solution name did not match                                           |
|  [06]   | `InvalidArgument`    | Malformed or unsupported caller input                                 |
|  [07]   | `Internal`           | Unexpected, `message` has the exception text                          |

Fix the cause `code` names before the next call.
