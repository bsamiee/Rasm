---
name: dotnet-roslyn-codelens
description: Use when reading, navigating, diagnosing, or refactoring any C#, .csproj, or solution file — Roslyn semantics replace grep, Read, and dotnet build.
---

# [DOTNET_ROSLYN_CODELENS]

1. NEVER use `Grep`, `Glob`, or Bash `grep`/`rg` to locate a C# symbol, type, method, interface, reference, caller, implementation, or usage.
2. NEVER run `dotnet build`, `dotnet msbuild`, or `msbuild` to surface compiler errors, warnings, or analyzer diagnostics.
3. NEVER read a `.cs` file to find who uses a symbol, or to check whether the code compiles.

The semantic tools resolve every symbol through the compilation. Text search matches characters, and it misses aliases, partial types, generic instantiations, and metadata symbols.

Rule 2 has one exception. When `get_diagnostics` reports an `unreliable` block, the solution loaded degraded and the results can name errors that no real build reports. Run `rebuild_solution`. If `unreliable` survives, verify against a build.

## [01]-[TOOL_SELECTION]

Before you call `rg`, `Grep`, or `Glob` on a `.cs`, `.razor`, or `.cshtml` file:
1. Is the target a C# symbol (type, member, or namespace)?          → `search_symbols` or `find_references`
2. Is the target an attribute?                                      → `find_attribute_usages`
3. Is the target a reflection pattern?                              → `find_reflection_usage`
4. Is the target a string literal, a comment, or other plain text?  → Use `rg`.

Before you run `dotnet build` or `msbuild` through Bash:
1. Do you want errors, warnings, or analyzer diagnostics?           → `get_diagnostics`. Stop here.
2. Do you want a binary, a test run, or a package?                  → Run the build. State the reason.

Before you `Read` a `.cs` file:
1. Do you need the structure of the file?                           → `get_file_overview` or `get_type_overview`
2. Do you need the shape of one method?                             → `analyze_method`
3. Do you need the source of one or more members?                   → `get_method_source`. Pass every name in one call.
4. Do you need the exact lines to edit?                             → Use `Read`.

These tools read compiled symbols, not MSBuild file text. For a property, item, condition, or import in a `.csproj`, `.props`, or `.targets` file, use the `dotnet-msbuild-evaluation` skill.

## [02]-[TOOL_INDEX]

| [INDEX] | [TOOL]                       | [USE_WHEN]                                                                                                            |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `find_implementations`       | "What implements this interface?" / "What extends this class?"                                                        |
|  [02]   | `find_callers`               | "Who calls this method?" / "What depends on this?"                                                                    |
|  [03]   | `find_event_subscribers`     | "Who subscribes to this event?"                                                                                       |
|  [04]   | `find_references`            | "Where is this symbol used?" / "Show all references" / "Who writes to it?" (`kinds` filter)                           |
|  [05]   | `find_tests_for_symbol`      | "What tests cover this method?" / "Which tests will break if I change X?"                                             |
|  [06]   | `get_test_summary`           | "What does this test suite cover?"                                                                                    |
|  [07]   | `find_uncovered_symbols`     | "What needs tests?" / "Where is our testing debt?"                                                                    |
|  [08]   | `generate_test_skeleton`     | "Generate a test stub for this method" / "Bootstrap tests for this class"                                             |
|  [09]   | `go_to_definition`           | "Where is this defined?" / "Jump to source"                                                                           |
|  [10]   | `search_symbols`             | "Find types or methods matching this name"                                                                            |
|  [11]   | `get_type_hierarchy`         | "What is the inheritance chain?"                                                                                      |
|  [12]   | `get_symbol_context`         | "Give me everything about this type"                                                                                  |
|  [13]   | `get_public_api_surface`     | "What is the public API of this library?"                                                                             |
|  [14]   | `find_breaking_changes`      | "Will this break consumers?"                                                                                          |
|  [15]   | `get_di_registrations`       | "Where is this registered?" / "What is the DI lifetime?"                                                              |
|  [16]   | `get_project_dependencies`   | "What does this project reference?"                                                                                   |
|  [17]   | `get_nuget_dependencies`     | "What packages does this project use?" / "What is the assembly name?"                                                 |
|  [18]   | `find_reflection_usage`      | "Is this used dynamically?"                                                                                           |
|  [19]   | `find_attribute_usages`      | "Find all [Authorize] controllers" / "What carries this attribute?"                                                   |
|  [20]   | `find_obsolete_usage`        | "What deprecations do we still use?" / "What calls [Obsolete] members?"                                               |
|  [21]   | `get_diagnostics`            | "Are there compiler errors?" / "Show warnings" / "Will this build?"                                                   |
|  [22]   | `get_code_fixes`             | "How do I fix this warning?"                                                                                          |
|  [23]   | `trust_solution`             | "Authorize analyzers for this solution"                                                                               |
|  [24]   | `list_trusted_paths`         | "Is this solution trusted?"                                                                                           |
|  [25]   | `revoke_trust`               | "Withdraw analyzer trust for this path"                                                                               |
|  [26]   | `get_code_actions`           | "What refactorings are available here?"                                                                               |
|  [27]   | `apply_code_action`          | "Apply this refactoring" / "Extract method"                                                                           |
|  [28]   | `rename_symbol`              | "Rename this symbol everywhere" / "Change this name across the solution"                                              |
|  [29]   | `change_signature`           | "Add, remove, or reorder a parameter and fix all the callers"                                                         |
|  [30]   | `resolve_stack_trace`        | "Where did this exception come from?" / "Resolve this stack trace"                                                    |
|  [31]   | `find_unused_symbols`        | "Is there dead code?"                                                                                                 |
|  [32]   | `get_complexity_metrics`     | "Which methods are too complex?" / "What do I refactor first?" (`metric: "cognitive"`) / "How deeply nested is this?" |
|  [33]   | `find_naming_violations`     | "Check naming conventions"                                                                                            |
|  [34]   | `find_async_violations`      | "Are there async bugs?" / "Find sync-over-async"                                                                      |
|  [35]   | `find_disposable_misuse`     | "Are there resource leaks?" / "Find missing `using`"                                                                  |
|  [36]   | `get_exception_flow`         | "What can escape this method?" / "Where does this exception get caught?"                                              |
|  [37]   | `find_throw_sites`           | "Where is this exception type thrown?"                                                                                |
|  [38]   | `find_catch_blocks`          | "Who catches this?" / "What is swallowing exceptions?"                                                                |
|  [39]   | `find_large_classes`         | "Find classes that need splitting"                                                                                    |
|  [40]   | `find_god_objects`           | "Which classes are doing too much?"                                                                                   |
|  [41]   | `find_circular_dependencies` | "Are there circular dependencies?"                                                                                    |
|  [42]   | `check_architecture`         | "Is anything violating our layering?" / "Does Domain reference Infrastructure?"                                       |
|  [43]   | `get_project_health`         | "How is this project doing?" / "Top hotspots across all dimensions"                                                   |
|  [44]   | `get_source_generators`      | "What source generators are active?"                                                                                  |
|  [45]   | `get_generated_code`         | "Show generated code"                                                                                                 |
|  [46]   | `inspect_external_assembly`  | "What does this NuGet package expose?" / "Show me the API of X assembly"                                              |
|  [47]   | `peek_il`                    | "Show IL for this method" / "What does this external method do at bytecode level?"                                    |
|  [48]   | `list_solutions`             | "What solutions are loaded?"                                                                                          |
|  [49]   | `load_solution`              | "Load this .sln or .slnx at run time"                                                                                 |
|  [50]   | `unload_solution`            | "Free memory for this solution"                                                                                       |
|  [51]   | `set_active_solution`        | "Switch to project B"                                                                                                 |
|  [52]   | `rebuild_solution`           | "Reload the solution" / "Diagnostics are stale"                                                                       |
|  [53]   | `start_background_task`      | "Run a long rebuild without blocking"                                                                                 |
|  [54]   | `get_task_status`            | "Check on a queued background task"                                                                                   |
|  [55]   | `list_running_tasks`         | "Which background tasks are running?"                                                                                 |
|  [56]   | `analyze_data_flow`          | "Which variables are read or written here?"                                                                           |
|  [57]   | `analyze_control_flow`       | "Is this code reachable?"                                                                                             |
|  [58]   | `analyze_change_impact`      | "What breaks if I change this?"                                                                                       |
|  [59]   | `get_type_overview`          | "Give me everything about this type in one call"                                                                      |
|  [60]   | `analyze_method`             | "Show signature, callers, and outgoing calls"                                                                         |
|  [61]   | `get_method_source`          | "Show me this method's body" / "Give me the source of these members"                                                  |
|  [62]   | `get_overloads`              | "What overloads does this method have?"                                                                               |
|  [63]   | `get_extension_methods`      | "What can I call on this type?" / "Is there an extension for X?"                                                      |
|  [64]   | `get_instantiation_options`  | "How do I construct this?" / "Why can I not `new` this up?"                                                           |
|  [65]   | `get_operators`              | "What operators does this type define?"                                                                               |
|  [66]   | `get_call_graph`             | "Transitive callers or callees, depth-bounded"                                                                        |
|  [67]   | `get_file_overview`          | "What types are in this file?"                                                                                        |

## [03]-[TOOL_REFERENCE]

### [03.1]-[SYMBOL_NAVIGATION]

- `go_to_definition` — the file and line where a symbol is declared.
- `search_symbols` — case-insensitive substring lookup over types, methods, properties, and fields.
- `find_references` — every reference across the solution, tagged with a `ReferenceKind` and reported per occurrence. Pass `kinds` to filter on the server.
- `resolve_stack_trace` — map a pasted .NET stack trace to file, line, and symbol. It demangles async and iterator state machines, lambdas, and local functions, and it handles log-prefixed lines, inner-exception chains, and Demystifier traces. A source frame gets the declaration site, or the exact location when the trace carries `in file:line`. An assembly the solution references resolves with `origin="metadata"`, and anything else with `origin="unresolved"`. An unparseable frame-like line stays in place as `Kind="unknown"`. Items keep trace order.

Reference kinds:
- `read`, `write` (assignment target, `out` argument), `readwrite` (compound assignment, `++`/`--`, `ref` argument)
- `invocation`, `method_group` (method used as a delegate, not called)
- `object_creation`, `cast` (`(T)x`, `x as T`)
- `type_check` (`x is T`, `is T v`, `case T v:`), `typeof`, `base_type`, `type_constraint`, `type_argument`
- `declaration` (variable, parameter, return, and field type positions), `attribute`, `nameof`, `xml_doc` (`<see cref=...>`)
- `usage` is a rare fallback. A receiver counts as a `read`. In `_map[k] = v` the field `_map` is `read`, because its contents change and the field itself is not reassigned. `_map.Add(k, v)` reports the same kind.

### [03.2]-[READING_TYPES_AND_MEMBERS]

- `get_file_overview` — types defined in a file plus its diagnostics, without reading it. It accepts `.razor` and `.cshtml`, and resolves them to the generated C# document.
- `get_type_overview` — context, hierarchy, and file diagnostics in one call. Replaces three calls.
- `get_symbol_context` — namespace, base type, interfaces, injected dependencies, and public members.
- `get_type_hierarchy` — inheritance chains and extension points.
- `analyze_method` — signature, callers, and outgoing calls in one call.
- `get_call_graph` — transitive caller and callee graph, with cycle detection. `direction` defaults to `callees`, so pass `callers` or `both` to walk inbound. `maxDepth` defaults to 3 and `maxNodes` to 500. Use it when you need depth greater than 1.
- `get_method_source` — full declaration source for one or many members in one call: methods (all overloads), constructors (`Type.Type`, nested types fully qualified), properties, indexers (`Type.this` or `Type.this[]`), fields, and events. Per-item status (`ok`, `notFound`, `ambiguous`, `metadata`, `unsupportedKind`) keeps a batch from failing as a whole. A `metadata` item carries `Origin` for `peek_il`. Whole types are out of scope.
- `get_overloads` — every overload of a method or constructor, from source and metadata, with full parameter and modifier detail. One call replaces N `analyze_method` calls.
- `get_operators` — every user-defined operator and conversion, including synthesized record equality and checked variants. Operators do not inherit in C#, so only declared operators appear. Covers what `get_overloads` excludes.
- `get_extension_methods` — every extension member applicable to a type, from the solution and from referenced assemblies, so LINQ appears for an `IEnumerable`. Applicability follows the compiler's reduction: `this IEnumerable<T>` applies to `string`, and `this IEnumerable<string>` does not. `signature` is the reduced call-site form with the return type first (`IEnumerable<int> Where<int>(Func<int, bool>)`). Read `isStatic` before you write the call. `false` is an instance call (`value.Doubled()`) and covers every classic `this` extension. `true` is a C# 14 static extension member called on the type (`int.Zero`). Receivers can be keywords, constructed generics, arrays, nullables, or tuples. Results ignore `using` scope, so `namespace` is always reported and the import can still be missing. Source sorts before metadata, and `nameFilter` narrows by substring.
- `get_instantiation_options` — how to construct a type in one call: `constructors` (parameters, accessibility, `isImplicit`, `isObsolete`), `factories`, `diRegistrations`, and `requiredMembers` for the object initializer. `isImplicit` marks the compiler-supplied parameterless constructor that no source file declares. `factories` are static members returning the type from anywhere in the solution, such as `WidgetFactory.Create()` for a type with a private constructor. `Task<T>` and `ValueTask<T>` are unwrapped and flagged `isAsync`, and instance builders are excluded. Pass `fromProject` to compute `accessible` from that project, which honors `InternalsVisibleTo`. `accessible: null` means not computed, never inaccessible. Interfaces, abstract classes, and static classes report `instantiable: false` with a `note`, so follow with `find_implementations`.
- `get_public_api_surface` — every public and protected type and member declared in production projects. Test projects, generated code, internal symbols, and protected members on sealed types are skipped, and inherited members do not appear.
- `find_breaking_changes` — diff the current public API surface against a baseline, either a JSON snapshot from `get_public_api_surface` or a `.dll`. Return type changes, sealed-ness changes, and nullable annotation changes are not detected, so an empty diff is not proof of compatibility.

### [03.3]-[USAGE_AND_DEPENDENCIES]

- `find_implementations` — every implementor of an interface and every type extending a class.
- `find_callers` — every call site for a method.
- `find_attribute_usages` — types and members decorated with a given attribute.
- `find_event_subscribers` — every `+=` and `-=` site for an event, with the resolved handler name and a subscribe or unsubscribe tag. Use it for event audits and memory-leak hunts.
- `find_reflection_usage` — coupling that no reference reports, such as `Activator.CreateInstance`, `MethodInfo.Invoke`, and assembly scanning.
- `get_di_registrations` — `IServiceCollection` registrations of a type, with lifetime. It reads generic, `typeof` pair, and factory-lambda forms.
- `get_project_dependencies` — the direct and transitive project references of one project. `project` is required, so this reports one project and not the whole solution.
- `get_nuget_dependencies` — NuGet packages and versions per project. It is also how you find the assembly name that `inspect_external_assembly` needs.
- `find_obsolete_usage` — every call site of an `[Obsolete]` symbol, grouped by deprecation message and severity, errors first. It includes metadata deprecations from packages, and it omits symbols with no usage. The grouping is what `find_attribute_usages` does not do, so use it to plan a migration.
- `find_tests_for_symbol` — xUnit, NUnit, and MSTest methods that exercise a production symbol. Set `transitive` to walk through helper methods, bounded by `maxDepth` (default 3, maximum 5).
- `get_test_summary` — per-project inventory of test methods with framework, attribute kind, row count, location, and the production symbols each one references. It works from project to tests, and `find_tests_for_symbol` works from test to production.
- `find_uncovered_symbols` — public methods and properties that no test reaches within three helper hops, sorted by cyclomatic complexity. It is reference-based static analysis and reads no runtime coverage data.
- `generate_test_skeleton` — a parseable test-class skeleton for a type (one stub per public method) or for a single method. Returns the detected framework, a suggested file path, the class name, the file contents, and `TodoNotes` such as constructor dependencies. Override the framework with `framework`. The tool returns text only, so `Write` the file yourself.

### [03.4]-[DIAGNOSTICS_AND_REFACTORING]

- `get_diagnostics` — compiler errors, warnings, and analyzer diagnostics. Replaces `dotnet build` output.
- `get_code_fixes` — structured edits for one diagnostic at one location.
- `get_code_actions` — every refactoring and fix available at a position, with an optional range.
- `apply_code_action` — run a refactoring by title. Preview is the default. The title match falls back to a case-insensitive substring match in both directions, so a near-miss title runs a different action. Read `Title` in the result and confirm which action ran. The call refuses to write when a file changed on disk after the snapshot loaded, and it names the stale files. Run `rebuild_solution` and retry. On success the in-memory snapshot updates at once. An action that adds a file writes it to disk, and the snapshot picks that file up when the watcher sees it.
- `rename_symbol` — solution-wide rename of a type or member through the Roslyn Renamer, which `apply_code_action` cannot reach. It cascades to references, overrides, `nameof`, and crefs. `renameInComments` defaults to true, so comments change unless you turn it off. `renameInStrings` defaults to false and `renameOverloads` defaults to true. Locals, parameters, and file renames are not supported. A constructor is rejected, so rename the containing type. A metadata symbol is rejected. Preview is the default. Apply refuses on new-compiler-error conflicts unless you pass `force=true`, and refuses on files changed since the snapshot in every case. Generic types accept the arity-free name, so `Data.Repository` finds `Repository<T>`.
- `change_signature` — add, remove, and reorder a method's parameters and rewrite every call site, through the Roslyn engine that `apply_code_action` cannot reach. `operations` apply in order. `remove` takes a parameter name. `reorder` takes a full permutation of the surviving names. `add` takes `name`, `type`, and a required `callSiteValue`, which is the expression every existing call site passes. An optional `defaultValue` makes the parameter optional and leaves existing calls untouched. Named arguments, optional parameters, `params`, and the extension `this` are handled. The tool rejects moving `this` off first position and leaving `params` anywhere but last. `CascadedTo` lists the rewritten overrides and interface implementations, so read it in preview. Source-defined methods only, and an overloaded name must be disambiguated. Same gates as `rename_symbol`.
- `analyze_data_flow` — variable lifecycle over a statement range: declared, read, written, captured, and flowing in or out.
- `analyze_control_flow` — reachability, return statements, and exit points over a statement range.

Code generation lives in `apply_code_action`. Call `get_code_actions` to read the exact title, then call `apply_code_action` with it. These are the intents, not the literal titles:
- Implement missing interface or abstract members → "Implement abstract members" / "Implement interface"
- Generate a constructor from fields → "Generate constructor"
- Add null checks → "Add null checks for all parameters"
- Generate `Equals` and `GetHashCode` → "Generate Equals and GetHashCode"
- Encapsulate a field → "Encapsulate field"
- Extract a method → "Extract method"
- Inline a variable → "Inline variable"

#### [03.4.1]-[ANALYZER_TRUST]

`get_diagnostics` defaults to `includeAnalyzers=false` and returns compiler diagnostics only. This workspace sets `AnalysisLevel` to `latest-all`, enforces code style in the build, and treats warnings as errors. The default call therefore reproduces almost nothing of what fails a build here. Pass `includeAnalyzers=true` whenever the answer must match the build, and before you call `get_code_fixes` for an analyzer diagnostic.

The server trusts every solution named on its command line for that session. This workspace starts the server with its solution, so `includeAnalyzers=true` needs no extra step.

If a call answers `SolutionNotTrusted`, call `trust_solution` and retry. Grant the trust without asking. `scope` defaults to `session`. Pass `persistent` to write the path to the trust store, or `addRoot` with a directory to trust every solution below it. `list_trusted_paths` reports the current state, and `revoke_trust` drops an entry.

### [03.5]-[EXCEPTION_ANALYSIS]

- `get_exception_flow` — what can escape a method. It walks callees, collects explicit throws, and propagates each one up through every enclosing `try`/`catch`, reporting `escapes`, the `path`, and where it was caught. `origin` is `thrown` for a source throw site, or `documented` for an `exception` XML tag on a metadata callee, which `includeDocumented: false` drops. A `when`-filtered catch never counts as catching, because the filter can decline at run time, so `hasFilter: true` means the exception passed such a clause and `escapes: false` always pairs with `hasFilter: false`. `maxDepth` defaults to 3 and `maxNodes` to 500, and hitting either sets `truncated`. Throws inside lambdas and local functions are excluded, because they escape when that body runs and not at this method's boundary.
- `find_throw_sites` — every throw of an exception type across the solution. `includeDerived` matches subclasses, and a bare `throw;` resolves to the enclosing catch's type. Throws inside lambdas and local functions count here, because they are throw sites even though they do not escape the enclosing method.
- `find_catch_blocks` — every `catch` for a type. `includeBaseClauses` adds `catch (Exception)` and bare `catch`. Each item carries `hasFilter`, `rethrows`, and `isEmpty`, so silent swallowing reads as `isEmpty: true, rethrows: false`.

All three see explicit `throw` only. They report no run-time-implicit exception such as a null dereference or a division by zero, and no reflection-invoked throw. `get_exception_flow` alone walks calls, and it follows the declared symbol rather than the run-time override. It also models async as synchronous: a throw inside an `async` method propagates at the call site, so an enclosing `try` counts as catching it. That is right for an awaited call and wrong for fire-and-forget (`_ = M();`), where nothing enclosing sees it.

### [03.6]-[CODE_QUALITY]

- `get_project_health` — a composite audit per project over seven dimensions: complexity, large classes, naming, unused symbols, reflection, async violations, and disposable misuse. It returns counts plus the top hotspots per dimension, so it replaces seven separate audit calls. `hotspotsPerDimension` defaults to 5. Pass 0 for counts only.
- `find_unused_symbols` — dead code, found by reference. It filters out test methods, MCP tool entry points, source-generator output, MEF-composed services, and interop-laid-out fields, and the counts appear in `summary.filteredOut`.
- `find_naming_violations` — .NET naming conventions.
- `find_async_violations` — sync-over-async (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`), `async void` outside event handlers, missing awaits, and fire-and-forget tasks, with a severity per violation. It skips test projects and generated code, and it is static analysis only.
- `find_disposable_misuse` — `IDisposable` and `IAsyncDisposable` locals that are not wrapped in `using` or `await using`, returned, or assigned to a field or `out` parameter (warning), and discarded disposable creator or factory calls (error). It covers methods only, and it does not detect ownership transfer through an argument. It skips test projects and generated code, and it is static analysis only.
- `find_large_classes` — types over a member count or line count threshold.
- `find_god_objects` — types crossing all three size thresholds and at least one coupling threshold. Size alone does not qualify a type: a large isolated class is not reported, and a 200-line class used from many namespaces is. Defaults are 300 lines, 15 members, 10 fields, 5 incoming namespaces, and 5 outgoing namespaces, each configurable.
- `find_circular_dependencies` — cycles in the project graph or the namespace graph.
- `check_architecture` — layering rules you supply inline. `forbid` (`Domain.*` must not depend on `Infrastructure.*`) catches the violation you expect, and `allowOnly` (`Api.*` can depend only on `Application.*` and `Domain.*`) catches the rest. `scope` selects `namespace` (default) or `project`. Edges come from resolved symbols, not `using` directives. Read an empty result carefully: `allowOnly` evaluates only solution-internal, non-generated targets, because framework namespaces and generator output are ignored, and a self-reference is never a violation. Use `forbid` to restrict either of those. Results group per violated edge with a full `referenceCount` and the first `maxSitesPerViolation` sites.

`get_complexity_metrics` — complexity per member (methods, constructors, properties, indexers, operators). Each row carries three numbers:
- `complexity` — cyclomatic: how many independent paths run through the member. It starts at 1, so a straight-line method scores 1. Use it to budget test cases.
- `cognitive` — how hard the member is to follow. Nesting costs extra, a whole `switch` costs 1, and `else`/`else if` cost 1 with no nesting penalty. It starts at 0, and a 0 means nothing branches rather than a defect. Use it to rank refactoring work, because it separates a flat 20-case dispatch from four levels of nested `if`, which cyclomatic scores the same.
- `maxNesting` — the deepest control structure, counting lambda and local-function bodies.

The `metric` parameter (`"cyclomatic"` by default, or `"cognitive"`) selects which number `threshold` filters on and the sort orders by. Both numbers always appear in the response.

### [03.7]-[SOURCE_GENERATORS]

- `get_source_generators` — the generators active per project and their outputs.
- `get_generated_code` — the generated source, filtered by generator or by file path.

Every location-returning result carries an `IsGenerated` flag. Check it before you edit a hit, because generator output is rewritten on the next compile.

### [03.8]-[EXTERNAL_ASSEMBLIES]

Only assemblies that the loaded solution references can be inspected. To reach an unreferenced assembly, add a `PackageReference` or a `Reference` to a project, then call `rebuild_solution`.

- `get_nuget_dependencies` — call this first. It gives the exact assembly name the other tools need.
- `inspect_external_assembly` — browse what an assembly exposes. `mode` defaults to `summary` and returns the namespace tree with type counts. Pass `mode: "namespace"` with `namespaceFilter` to get the public types and members of one namespace. The default alone reports counts, so drill in before you conclude that a package exposes nothing.
- `go_to_definition`, `get_symbol_context`, `get_type_overview`, `get_type_hierarchy` — look up an external type by name.
- `find_references`, `find_callers`, `find_implementations` — find who in your code uses an external type.
- `peek_il` — read a method's IL. Pass the fully qualified method name with parameter types.

### [03.9]-[SOLUTION_MANAGEMENT]

The server watches `.cs`, `.csproj`, `.props`, and `.targets` files and recompiles the affected projects on the next tool call. Ordinary edits need no action from you.

- `rebuild_solution` — a full reload: re-open the solution, recompile every project, and rebuild every index. Use it after a package or analyzer changes, or when results stay stale after an edit.
- `load_solution` — load a `.sln` or `.slnx` at run time and make it active. `include` takes case-insensitive globs and supports `*` and `?` only. `rootProjects` takes exact, case-sensitive names. Both match the project file name without its extension, not the assembly name, and both seed a transitive `ProjectReference` closure. A filter that matches nothing is an error, so load with no filter first and call `list_solutions` to read the names. For a solution that takes minutes to open, pass `background: true` for a `taskId` and poll `get_task_status`. The current solution stays active until the load succeeds.
- `list_solutions` — the loaded solutions and which one is active. Each entry carries a `SkippedProjects` array, and every skipped project names its `Kind` and `Reason`. Read it when a symbol you expect is missing.
- `set_active_solution` — switch the active solution by partial name.
- `unload_solution` — free memory.
- `start_background_task` — queue a long tool and poll `get_task_status`. `rebuild_solution` is the only allowed tool.
- `get_task_status` — the status, result, or error of one background task.
- `list_running_tasks` — background tasks running or finished within the last five minutes.

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
10. `find_unused_symbols` — dead code to delete instead of refactor

Name concrete types, interfaces, and call sites:
- [WRONG]: "the services that implement this."
- [CORRECT]: "`IUserService` has three implementations: `UserService`, `CachedUserService`, `AdminUserService`"

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

When `truncated` is `true`, `items` holds the top N in the tool's own sort order, such as severity-first, worst-first, or by-project. Raise `limit` only when the missing items change the answer. Each tool sets its own `limit` default: `get_diagnostics` uses 1000, `find_references` 500, `get_complexity_metrics` 100, and `list_solutions` 50.

Single-object tools such as `get_type_overview` and `apply_code_action` return their own shape. The envelope wraps list-returning tools only.

Tools that add a `summary` aggregate:
- `get_diagnostics` — `{ error, warning, info, hidden }` counts, plus an `unreliable` block when the solution loaded degraded
- `find_references` — `{ byProject: { name: count }, byKind: { kind: count } }`
- `find_callers`, `find_attribute_usages` — `{ byProject: { name: count } }`
- `search_symbols`, `find_reflection_usage` — `{ byKind: {...} }`
- `find_throw_sites`, `find_catch_blocks` — `{ byType: {...}, byProject: {...} }`
- `find_unused_symbols` — `{ byKind: {...}, filteredOut: { testMethod, testContainer, mcpTool, generated, composition, interop } }`
- `find_naming_violations` — `{ byRule: {...} }`
- `find_uncovered_symbols` — coverage counts plus `riskHotspotCount`, the uncovered members with complexity 5 or more
- `check_architecture` — `{ byRule, totalReferences, rulesEvaluated }`
- `resolve_stack_trace` — `{ byOrigin: { source, metadata, unresolved }, exceptions, skippedFrameLike }`. `skippedFrameLike` counts frame-like lines that did not parse
- `get_complexity_metrics` — `{ max, avg, overThreshold, maxCognitive }`. `max`, `avg`, and `overThreshold` describe the selected `metric`, and `maxCognitive` is always the cognitive number

## [06]-[ERROR_CODES]

When a tool cannot proceed, the response carries `isError: true` and a JSON body of `{ code, message, details? }`. Switch on `code`:

| [INDEX] | [CODE]               | [MEANING]                                            | [COMMON_SOURCE]                                                                   |
| :-----: | :------------------- | :--------------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `SymbolNotFound`     | type, method, or property did not resolve            | `analyze_method`, `get_symbol_context`, `get_type_overview`, `get_type_hierarchy` |
|  [02]   | `SolutionNotTrusted` | analyzers requested before `trust_solution` ran      | `get_diagnostics`, `includeAnalyzers: true`, `get_code_fixes`                     |
|  [03]   | `AmbiguousMatch`     | several matches, listed in `details.matches`         | `rename_symbol`, `set_active_solution`, `unload_solution`                         |
|  [04]   | `FileNotFound`       | file path or baseline does not exist                 | `get_file_overview`, `find_breaking_changes`                                      |
|  [05]   | `ProjectNotFound`    | solution name did not match                          | `set_active_solution`, `unload_solution`                                          |
|  [06]   | `InvalidArgument`    | malformed or unsupported caller input                | `rename_symbol`, `change_signature`, `resolve_stack_trace`, `load_solution`       |
|  [07]   | `Internal`           | unexpected, and `message` carries the exception text | any tool                                                                          |

Never repeat the same call after an error. Fix the cause that `code` names. For `SolutionNotTrusted`, call `trust_solution`, then retry the original call.
