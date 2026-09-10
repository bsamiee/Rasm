---
name: dotnet-roslyn-codelens
description: "Use when reading, navigating, diagnosing, or refactoring a C#, .csproj, or solution file through Roslyn in place of grep, Read, and dotnet build."
---

# [DOTNET_ROSLYN_CODELENS]

Covers the roslyn-codelens MCP server, from symbol navigation and diagnostics over a loaded solution to refactoring and solution management. Tools resolve every symbol through the compilation, text search matches characters and misses aliases, partial types, generic instantiations, and metadata symbols.

## [01]-[TOOL_SELECTION]

Before a text search on a `.cs`, `.razor`, or `.cshtml` file, the target decides the tool:
1. C# symbol (type, member, or namespace) → `search_symbols` or `find_references`
2. Attribute → `find_attribute_usages`
3. Reflection pattern → `find_reflection_usage`
4. String literal, comment, or other plain text → `rg`

Before `dotnet build` or `msbuild` through Bash, the wanted result decides:
1. Errors, warnings, or analyzer diagnostics → `get_diagnostics`
2. Binary, test run, or package → the build

Before `Read` on a `.cs` file, the wanted view decides:
1. File structure → `get_file_overview` or `get_type_overview`
2. One method's shape → `analyze_method`
3. Source of one or more members → `get_method_source` with every name in one call
4. Exact lines to edit → `Read`

## [02]-[TOOL_INDEX]

| [INDEX] | [TOOL]                       | [USE_WHEN]                                                                                  |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `find_implementations`       | "What implements this interface?" / "What extends this class?"                              |
|  [02]   | `find_callers`               | "Who calls this method?" / "What depends on this?"                                          |
|  [03]   | `find_event_subscribers`     | "Who subscribes to this event?"                                                             |
|  [04]   | `find_references`            | "Where is this symbol used?" / "Show all references" / "Who writes to it?" (`kinds` filter) |
|  [05]   | `find_tests_for_symbol`      | "What tests cover this method?" / "Which tests will break if I change X?"                   |
|  [06]   | `get_test_summary`           | "What does this test suite cover?"                                                          |
|  [07]   | `find_uncovered_symbols`     | "Which public members does no test reach?"                                                  |
|  [08]   | `go_to_definition`           | "Where is this defined?" / "Jump to source"                                                 |
|  [09]   | `search_symbols`             | "Find types or methods matching this name"                                                  |
|  [10]   | `get_type_hierarchy`         | "What is the inheritance chain?"                                                            |
|  [11]   | `get_symbol_context`         | "Give me everything about this type"                                                        |
|  [12]   | `get_public_api_surface`     | "What is the public API of this library?"                                                   |
|  [13]   | `find_breaking_changes`      | "Will this break consumers?"                                                                |
|  [14]   | `get_di_registrations`       | "Where is this registered?" / "What is the DI lifetime?"                                    |
|  [15]   | `get_project_dependencies`   | "What does this project reference?"                                                         |
|  [16]   | `get_nuget_dependencies`     | "What packages does this project use?" / "What is the assembly name?"                       |
|  [17]   | `find_reflection_usage`      | "Is this used dynamically?"                                                                 |
|  [18]   | `find_attribute_usages`      | "Find all [Authorize] controllers" / "What holds this attribute?"                           |
|  [19]   | `find_obsolete_usage`        | "What calls an `[Obsolete]` member?" / "Which deprecations are pending?"                    |
|  [20]   | `get_diagnostics`            | "Are there compiler errors?" / "Show warnings" / "Will this build?"                         |
|  [21]   | `get_code_fixes`             | "How do I fix this warning?"                                                                |
|  [22]   | `trust_solution`             | "Authorize analyzers for another solution"                                                  |
|  [23]   | `list_trusted_paths`         | "Is this solution trusted?"                                                                 |
|  [24]   | `revoke_trust`               | "Withdraw analyzer trust for this path"                                                     |
|  [25]   | `get_code_actions`           | "What refactorings are available here?"                                                     |
|  [26]   | `apply_code_action`          | "Apply this refactoring" / "Extract method"                                                 |
|  [27]   | `rename_symbol`              | "Rename this symbol everywhere" / "Change this name across the solution"                    |
|  [28]   | `change_signature`           | "Add, remove, or reorder a parameter and fix all the callers"                               |
|  [29]   | `resolve_stack_trace`        | "Where did this exception come from?" / "Resolve this stack trace"                          |
|  [30]   | `find_unused_symbols`        | "Is there dead code?"                                                                       |
|  [31]   | `get_complexity_metrics`     | "Which methods are too complex?" / "What do I refactor first?" (`metric: "cognitive"`)      |
|  [32]   | `find_naming_violations`     | "Check naming conventions"                                                                  |
|  [33]   | `find_async_violations`      | "Are there async bugs?" / "Find sync-over-async"                                            |
|  [34]   | `find_disposable_misuse`     | "Are there resource leaks?" / "Find a missing `using`"                                      |
|  [35]   | `get_exception_flow`         | "What can escape this method?" / "Where does this exception get caught?"                    |
|  [36]   | `find_throw_sites`           | "Where is this exception type thrown?"                                                      |
|  [37]   | `find_catch_blocks`          | "Who catches this?" / "What is swallowing exceptions?"                                      |
|  [38]   | `find_large_classes`         | "Find classes that need splitting"                                                          |
|  [39]   | `find_god_objects`           | "Which classes are doing too much?"                                                         |
|  [40]   | `find_circular_dependencies` | "Are there circular dependencies?"                                                          |
|  [41]   | `check_architecture`         | "Is anything violating our layering?" / "Does Domain reference Infrastructure?"             |
|  [42]   | `get_project_health`         | "How is this project doing?" / "Top hotspots across all dimensions"                         |
|  [43]   | `get_source_generators`      | "What source generators are active?"                                                        |
|  [44]   | `get_generated_code`         | "Show generated code"                                                                       |
|  [45]   | `inspect_external_assembly`  | "What does this NuGet package expose?" / "Show me the API of X assembly"                    |
|  [46]   | `peek_il`                    | "Show IL for this method" / "What does this external method do at bytecode level?"          |
|  [47]   | `list_solutions`             | "What solutions are loaded?"                                                                |
|  [48]   | `load_solution`              | "Load this .sln or .slnx at run time"                                                       |
|  [49]   | `unload_solution`            | "Free memory for this solution"                                                             |
|  [50]   | `set_active_solution`        | "Switch to project B"                                                                       |
|  [51]   | `rebuild_solution`           | "Reload the solution" / "Diagnostics are stale"                                             |
|  [52]   | `start_background_task`      | "Run a long rebuild without blocking"                                                       |
|  [53]   | `get_task_status`            | "Check on a queued background task"                                                         |
|  [54]   | `list_running_tasks`         | "Which background tasks are running?"                                                       |
|  [55]   | `analyze_data_flow`          | "Which variables are read or written here?"                                                 |
|  [56]   | `analyze_control_flow`       | "Is this code reachable?"                                                                   |
|  [57]   | `analyze_change_impact`      | "What breaks if I change this?"                                                             |
|  [58]   | `get_type_overview`          | "Give me everything about this type in one call"                                            |
|  [59]   | `analyze_method`             | "Show signature, callers, and outgoing calls"                                               |
|  [60]   | `get_method_source`          | "Show me this method's body" / "Give me the source of these members"                        |
|  [61]   | `get_overloads`              | "What overloads does this method have?"                                                     |
|  [62]   | `get_extension_methods`      | "What can I call on this type?" / "Is there an extension for X?"                            |
|  [63]   | `get_instantiation_options`  | "How do I construct this?" / "Why can I not `new` this up?"                                 |
|  [64]   | `get_operators`              | "What operators does this type define?"                                                     |
|  [65]   | `get_call_graph`             | "Transitive callers or callees, depth-bounded"                                              |
|  [66]   | `get_file_overview`          | "What types are in this file?"                                                              |

## [03]-[TOOL_REFERENCE]

### [03.1]-[SYMBOL_NAVIGATION]

- `go_to_definition` — the file and line where a symbol is declared
- `search_symbols` — case-insensitive substring lookup over types, methods, properties, and fields
- `find_references` — every reference across the solution, one item per occurrence tagged with a `ReferenceKind`, `kinds` filters on the server
- `resolve_stack_trace` — maps a pasted .NET stack trace to file, line, and symbol, items keep trace order
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
- Receivers are a `read`: `_map[k] = v` and `_map.Add(k, v)` read the field `_map`, the contents change and the field stays assigned

### [03.2]-[READING_TYPES_AND_MEMBERS]

- `get_file_overview` — types and diagnostics of a file, `.razor` and `.cshtml` resolve to their generated C# document
- `get_type_overview` — context, hierarchy, and file diagnostics in one call
- `get_symbol_context` — namespace, base type, interfaces, injected dependencies, and public members
- `get_type_hierarchy` — inheritance chains and extension points
- `analyze_method` — signature, callers, and outgoing calls in one call
- `get_call_graph` — transitive caller and callee graph with cycle detection, for a depth greater than 1
  - `direction` defaults to `callees`, `callers` or `both` walks inbound
  - `maxDepth` defaults to 3, `maxNodes` to 500
- `get_method_source` — full declaration source for one or many members in one call, whole types are out of scope
  - Members: methods (all overloads), constructors (`Type.Type`, nested types fully qualified), properties, fields, events
  - Indexers are requested as `Type.this` or `Type.this[]`
  - Each item carries a status (`ok`, `notFound`, `ambiguous`, `metadata`, `unsupportedKind`), one miss leaves the batch intact
  - `metadata` items carry `origin` for `peek_il`
- `get_overloads` — every overload of a method or constructor from source and metadata, with full parameter and modifier detail
- `get_operators` — every user-defined operator and conversion a type declares (operators do not inherit)
  - Synthesized record equality and checked variants are included
- `get_extension_methods` — every extension member applicable to a type from the solution and referenced assemblies, LINQ appears for an `IEnumerable`
  - Applicability follows the compiler's reduction: `this IEnumerable<T>` applies to `string`, `this IEnumerable<string>` does not
  - `signature` is the reduced call-site form with the return type first (`IEnumerable<int> Where<int>(Func<int, bool>)`)
  - `isStatic: false` is an instance call (`value.Doubled()`) and covers every classic `this` extension
  - `isStatic: true` is a C# 14 static extension member called on the type (`int.Zero`)
  - Receivers can be keywords, constructed generics, arrays, nullables, or tuples
  - Results ignore `using` scope, `namespace` is always reported, and the import can still be missing
  - Source sorts before metadata, `nameFilter` narrows by substring
- `get_instantiation_options` — how to construct a type in one call: `constructors`, `factories`, `diRegistrations`, and `requiredMembers`
  - `constructors` hold parameters, accessibility, `isImplicit` for the compiler-supplied parameterless constructor, and `isObsolete`
  - `factories` are static members anywhere in the solution that return the type (`WidgetFactory.Create()` for a type with a private constructor)
  - `Task<T>` and `ValueTask<T>` factories are unwrapped and flagged `isAsync`, instance builders are excluded
  - `fromProject` computes `accessible` from that project and honors `InternalsVisibleTo`, `accessible: null` means not computed
  - Interfaces, abstract classes, and static classes report `instantiable: false` with a `note`, `find_implementations` follows
- `get_public_api_surface` — every public and protected type and member declared in production projects
  - Test projects, generated code, internal symbols, protected members on sealed types, and inherited members are skipped
- `find_breaking_changes` — diffs the current public API surface against a baseline, a JSON snapshot from `get_public_api_surface` or a `.dll`
  - Return type changes, `sealed` changes, and nullable annotation changes are undetected, an empty diff proves no compatibility

### [03.3]-[USAGE_AND_DEPENDENCIES]

- `find_implementations` — every implementor of an interface and every type extending a class
- `find_callers` — every call site for a method
- `find_attribute_usages` — types and members decorated with a given attribute
- `find_event_subscribers` — every `+=` and `-=` site for an event with the resolved handler name and a subscribe or unsubscribe tag
- `find_reflection_usage` — coupling that no reference reports: `Activator.CreateInstance`, `MethodInfo.Invoke`, and assembly scanning
- `get_di_registrations` — `IServiceCollection` registrations of a type with lifetime, in generic, `typeof` pair, and factory-lambda forms
- `get_project_dependencies` — the direct and transitive project references of one project. `project` is required
- `get_nuget_dependencies` — NuGet packages and versions per project
- `find_obsolete_usage` — every call site of an `[Obsolete]` symbol, grouped by deprecation message and severity, errors first
  - Metadata deprecations from packages are included, symbols with no usage are omitted
- `find_tests_for_symbol` — xUnit, NUnit, and MSTest methods that exercise a production symbol
  - `transitive` walks through helper methods, bounded by `maxDepth` (default 3, maximum 5)
- `get_test_summary` — per-project inventory of test methods, from project to tests where `find_tests_for_symbol` goes from test to production
  - Each test reports framework, attribute kind, row count, location, and the production symbols it references
- `find_uncovered_symbols` — public methods and properties no test reaches within three helper hops, sorted by cyclomatic complexity
  - Reference-based static analysis, reads no runtime coverage data

### [03.4]-[DIAGNOSTICS_AND_REFACTORING]

- `get_diagnostics` — compiler errors, warnings, and analyzer diagnostics
  - `unreliable` in its summary means the solution loaded degraded, items can then name errors no real build reports
  - `rebuild_solution` then a rerun resolves it, a block that persists leaves the build as the verdict
- `get_code_fixes` — structured edits for one diagnostic at one location
- `get_code_actions` — every refactoring and fix available at a position, with an optional range
- `apply_code_action` — runs a refactoring by title, preview by default, the in-memory snapshot updates at once on success
  - Title matching falls back to a case-insensitive substring match in both directions, a near-miss title runs a different action
  - `title` in the result names the action that ran
  - Apply refuses to write when a file changed on disk after the snapshot loaded and names the stale files
  - `rebuild_solution` then a retry resolves a stale file
  - Actions that add a file write it to disk, and the watcher adds that file to the snapshot
- `rename_symbol` — solution-wide rename of a type or member through the Roslyn Renamer, preview by default, `apply_code_action` offers no rename
  - Cascades to references, overrides, `nameof`, and crefs
  - `renameInComments` defaults to `true`, `renameInStrings` to `false`, `renameOverloads` to `true`
  - Locals, parameters, file renames, constructors, and metadata symbols are rejected, a constructor renames through its containing type
  - Apply refuses on new-compiler-error conflicts unless `force=true` is passed, and refuses on files changed since the snapshot in every case
  - Generic types accept the arity-free name: `Data.Repository` finds `Repository<T>`
- `change_signature` — adds, removes, and reorders a method's parameters and rewrites every call site, `apply_code_action` offers no signature change
  - `operations` apply in order: `remove` takes a parameter name, `reorder` a full permutation of the surviving names
  - `add` takes `name`, `type`, and a required `callSiteValue`, the expression every existing call site passes
  - Optional `defaultValue` makes the parameter optional and leaves existing calls untouched
  - Named arguments, optional parameters, `params`, and the extension `this` are handled
  - Moving `this` off first position and leaving `params` anywhere but last are rejected
  - `cascadedTo` lists the rewritten overrides and interface implementations, read it in preview
  - Source-defined methods only, an overloaded name must be disambiguated, same refusals as `rename_symbol`
- `analyze_data_flow` — variable lifecycle over a statement range: declared, read, written, captured, and flowing in or out
- `analyze_control_flow` — reachability, return statements, and exit points over a statement range

Code generation runs through `apply_code_action` with the exact title `get_code_actions` reads, intents with the title each maps to:
- Implement missing interface or abstract members → "Implement abstract members" / "Implement interface"
- Generate a constructor from fields → "Generate constructor"
- Add null checks → "Add null checks for all parameters"
- Generate `Equals` and `GetHashCode` → "Generate Equals and GetHashCode"
- Encapsulate a field → "Encapsulate field"
- Extract a method → "Extract method"
- Inline a variable → "Inline variable"

#### [03.4.1]-[ANALYZER_TRUST]

`get_diagnostics` defaults to `includeAnalyzers=false` and returns compiler diagnostics only. Under a build that runs analyzers with warnings as errors, the default call reproduces almost nothing of what fails the build. Pass `includeAnalyzers=true` when the answer must match the build, and before `get_code_fixes` for an analyzer diagnostic.

Solutions named on the server's command line are trusted for that session, every other solution needs `trust_solution`. `get_code_fixes` loads the fix providers as analyzer code and answers `SolutionNotTrusted` on an untrusted solution, compiler diagnostics included.

`analyzerPolicy` in the trust file (`roslyn-codelens/trust.json` under the user's application data directory, read at server start) selects the analyzer assemblies that load. `nuget-and-solution-bin`, the default, accepts `~/.nuget/packages`, the SDK directory, and a `bin` or `obj` path under the solution, `strict` drops the solution paths, `all` accepts every path. The allowlist spells `~/.nuget/packages` itself and reads no `globalPackagesFolder` from `NuGet.config`. Package analyzers restored to a relocated folder load under `all` alone, and `get_diagnostics` misses their diagnostics under the other policies.

Analyzers run with no analyzer options: `.editorconfig` severities apply, its option values do not. Style analyzers that read an option (`IDE0055` formatting) report against Roslyn defaults, their items from `get_diagnostics` are no finding, and `dotnet format --verify-no-changes` is the formatting verdict.

When a call returns `SolutionNotTrusted`, call `trust_solution` and retry. `scope` defaults to `session`. `persistent` writes the path to the trust store, `addRoot` with a directory trusts every solution below it. `list_trusted_paths` reports the current state, `revoke_trust` removes an entry.

### [03.5]-[EXCEPTION_ANALYSIS]

- `get_exception_flow` — what can escape a method, with `escapes`, the `path`, and the catch site per exception
  - Walks callees, collects explicit throws, and propagates each one up through every enclosing `try`/`catch`
  - `origin` is `thrown` for a source throw site, or `documented` for an `exception` XML tag on a metadata callee
  - `includeDocumented: false` drops the `documented` items
  - `when`-filtered catches never count as catching, the filter can be false at run time
  - `hasFilter: true` marks an exception that passed such a clause, `escapes: false` always pairs with `hasFilter: false`
  - `maxDepth` defaults to 3, `maxNodes` to 500, either limit sets `truncated`
  - Throws inside a lambda or local function are excluded, they escape when that body runs
- `find_throw_sites` — every throw of an exception type across the solution, a throw inside a lambda or local function included
  - `includeDerived` matches subclasses, a bare `throw;` resolves to the enclosing catch's type
- `find_catch_blocks` — every `catch` for a type, each item with `hasFilter`, `rethrows`, and `isEmpty`
  - `includeBaseClauses` adds `catch (Exception)` and bare `catch`
  - Silent swallowing reads as `isEmpty: true, rethrows: false`

Tools see explicit `throw` only, implicit run-time exceptions (null dereference, division by zero) and reflection-invoked throws stay unreported. `get_exception_flow` alone walks calls, and it follows the declared symbol, not the run-time override. `get_exception_flow` models async as synchronous: a throw inside an `async` method propagates at the call site, and an enclosing `try` counts as catching it. Synchronous modeling is correct for an awaited call and wrong for fire-and-forget (`_ = M();`), where nothing enclosing sees it.

### [03.6]-[CODE_QUALITY]

- `get_project_health` — composite audit per project with counts and the top hotspots per dimension
  - Dimensions: complexity, large classes, naming, unused symbols, reflection, async violations, and disposable misuse
  - `hotspotsPerDimension` defaults to 5, 0 gives counts only
- `find_unused_symbols` — dead code, found by reference, with the exclusion counts in `summary.filteredOut`
  - Test methods, MCP tool entry points, source-generator output, MEF-composed services, and interop-laid-out fields are excluded
- `find_naming_violations` — .NET naming conventions
- `find_async_violations` — async misuse across production projects, each violation with a severity
  - Sync-over-async (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`) and `async void` outside an event handler
  - Missing `await` in an `async` method and fire-and-forget tasks
  - Test projects and generated code are skipped
- `find_disposable_misuse` — `IDisposable` and `IAsyncDisposable` locals at risk of leaking
  - Locals not wrapped in `using` or `await using`, returned, or assigned to a field or `out` parameter are a warning
  - Discarded disposable creator or factory calls are an error
  - Methods only, ownership transfer through an argument is undetected, test projects and generated code are skipped
- `find_large_classes` — types over a member count or line count threshold
- `find_god_objects` — types over all three size thresholds and at least one coupling threshold, a large isolated class is skipped
  - Defaults are 300 lines, 15 members, 10 fields, 5 incoming namespaces, and 5 outgoing namespaces, each configurable
- `find_circular_dependencies` — cycles in the project graph or the namespace graph
- `check_architecture` — layering rules supplied inline, `scope` selects `namespace` (default) or `project`
  - `forbid` (`Domain.*` must not depend on `Infrastructure.*`) catches the expected violation
  - `allowOnly` (`Api.*` can depend only on `Application.*` and `Domain.*`) catches the rest
  - Edges come from resolved symbols, not `using` directives
  - `allowOnly` evaluates only solution-internal, non-generated targets, an empty result proves nothing
  - Framework namespaces and generator output are ignored, a self-reference is never a violation, `forbid` restricts either of those
  - Results group per violated edge with a full `referenceCount` and the first `maxSitesPerViolation` sites

`get_complexity_metrics`: complexity per member (methods, constructors, properties, indexers, operators). Each row holds:
- `complexity` — cyclomatic, the number of independent paths through the member, a straight-line method scores 1
- `cognitive` — how hard the member is to follow, a 0 means nothing branches, it ranks refactoring work
  - Nesting costs extra, a whole `switch` costs 1, `else`/`else if` cost 1 with no nesting penalty
  - Flat 20-case dispatch and four nested `if` levels share one cyclomatic number and differ in cognitive
- `maxNesting` — the deepest control structure, lambda and local-function bodies included

`metric` (`"cyclomatic"` by default, or `"cognitive"`) selects the number `threshold` and the sort use. Both numbers always appear in the response.

### [03.7]-[SOURCE_GENERATORS]

- `get_source_generators` — the generators active per project and their outputs
- `get_generated_code` — the generated source, filtered by generator or by file path

Every location-returning result has an `isGenerated` flag, read it before editing a match, generator output is rewritten on the next compile.

### [03.8]-[EXTERNAL_ASSEMBLIES]

Only assemblies the loaded solution references can be inspected. Unreferenced assemblies need a `PackageReference` or a `Reference` in a project, then `rebuild_solution`.

- `get_nuget_dependencies` — first call, gives the exact assembly name the other tools take
- `inspect_external_assembly` — what an assembly exposes, drill in before concluding that a package exposes nothing
  - `mode` defaults to `summary` and returns the namespace tree with type counts alone
  - `mode: "namespace"` with `namespaceFilter` gives the public types and members of one namespace
- `go_to_definition`, `get_symbol_context`, `get_type_overview`, `get_type_hierarchy` — an external type by name
- `find_references`, `find_callers`, `find_implementations` — the code that uses an external type
- `peek_il` — a method's IL by fully qualified method name with parameter types

### [03.9]-[SOLUTION_MANAGEMENT]

Solutions named on the server's command line load with the first one active, with none named the `.sln` or `.slnx` found walking up from the working directory loads. `ROSLYN_CODELENS_OPEN_PROJECT_TIMEOUT_SECONDS` (default `300`) bounds each project's load, a project over it joins `skippedProjects` with `kind: "Timeout"` and the rest of the solution loads. Edits to `.cs`, `.csproj`, `.props`, and `.targets` files recompile the affected projects on the next tool call with no further action.

- `rebuild_solution` — a full reload: re-open the solution, recompile every project, rebuild every index
  - Serves a package or analyzer change, and results that stay stale after an edit
- `load_solution` — loads a `.sln` or `.slnx` at run time and makes it active, the current solution stays active until the load succeeds
  - `include` takes case-insensitive globs with `*` and `?` only, `rootProjects` takes exact, case-sensitive names
  - Both match the project file name without its extension, and both seed a transitive `ProjectReference` closure
  - Filters that match nothing are an error, load with no filter first and read the names from `list_solutions`
  - For a solution that takes minutes to open, `background: true` returns a `taskId` for `get_task_status`
- `list_solutions` — the loaded solutions and the active one, read when an expected symbol is missing
  - `skippedProjects` per entry names each skipped project's `kind` and `reason`
- `set_active_solution` — switches the active solution by partial name
- `unload_solution` — frees memory, the remaining loaded solution becomes active
- `start_background_task` — queues a long tool for `get_task_status`, `rebuild_solution` is the only allowed tool
- `get_task_status` — the status, result, or error of one background task
- `list_running_tasks` — background tasks running or finished within the last five minutes

## [04]-[CHANGE_WORKFLOW]

1. `get_type_overview` — context, hierarchy, and diagnostics
2. `analyze_change_impact` — every file, project, and call site the change affects
3. `find_references` / `find_callers` / `find_implementations` — the detailed dependency breakdown
4. `get_project_dependencies` — the position of the project in the reference graph
5. `get_di_registrations` — container registrations and lifetimes
6. `find_reflection_usage` — coupling that no reference reports
7. `find_attribute_usages` — attribute-driven behavior
8. `get_diagnostics` — existing errors and warnings
9. `get_code_fixes` / `get_code_actions` → `apply_code_action` — code fixes and refactorings
10. `find_unused_symbols` — dead code to delete

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

When `truncated` is `true`, `items` holds the top N in the tool's own sort order: severity-first, worst-first, or by-project. Raise `limit` when the missing items change the answer. Each tool sets its own `limit` default: `get_diagnostics` 1000, `find_references` 500, `get_complexity_metrics` 100, `list_solutions` 50.

Single-object tools (`get_type_overview`, `apply_code_action`) return their own shape.

Tools that add a `summary` aggregate:
- `get_diagnostics` — `{ error, warning, info, hidden }` counts, with an `unreliable` block when the solution loaded degraded
- `find_references` — `{ byProject: { name: count }, byKind: { kind: count } }`
- `find_callers`, `find_attribute_usages` — `{ byProject: { name: count } }`
- `search_symbols`, `find_reflection_usage` — `{ byKind: {...} }`
- `find_throw_sites`, `find_catch_blocks` — `{ byType: {...}, byProject: {...} }`
- `find_unused_symbols` — `{ byKind: {...}, filteredOut: { testMethod, testContainer, mcpTool, generated, composition, interop } }`
- `find_naming_violations` — `{ byRule: {...} }`
- `find_uncovered_symbols` — coverage counts with `riskHotspotCount`, the uncovered members with complexity 5 or more
- `check_architecture` — `{ byRule, totalReferences, rulesEvaluated }`
- `resolve_stack_trace` — `{ byOrigin: { source, metadata, unresolved }, exceptions, skippedFrameLike }`
  - `skippedFrameLike` counts frame-like lines that did not parse
- `get_complexity_metrics` — `{ max, avg, overThreshold, maxCognitive }`
  - `max`, `avg`, and `overThreshold` describe the selected `metric`, `maxCognitive` is always the cognitive number

## [06]-[ERROR_CODES]

When a tool cannot proceed, the response has `isError: true` and a JSON body of `{ code, message, details? }`. Switch on `code`:

| [INDEX] | [CODE]               | [MEANING]                                       | [COMMON_SOURCE]                                               |
| :-----: | :------------------- | :---------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `SymbolNotFound`     | Type, method, or property did not resolve       | `analyze_method`, `get_symbol_context`, `get_type_overview`   |
|  [02]   | `SolutionNotTrusted` | Analyzers requested before `trust_solution` ran | `get_diagnostics`, `includeAnalyzers: true`, `get_code_fixes` |
|  [03]   | `AmbiguousMatch`     | Many matches, listed in `details.matches`       | `rename_symbol`, `set_active_solution`, `unload_solution`     |
|  [04]   | `FileNotFound`       | File path or baseline does not exist            | `get_file_overview`, `find_breaking_changes`                  |
|  [05]   | `ProjectNotFound`    | Solution name did not match                     | `set_active_solution`, `unload_solution`                      |
|  [06]   | `InvalidArgument`    | Malformed or unsupported caller input           | `rename_symbol`, `change_signature`, `load_solution`          |
|  [07]   | `Internal`           | Unexpected, `message` has the exception text    | Any tool                                                      |

Fix the cause `code` names before the next call.
