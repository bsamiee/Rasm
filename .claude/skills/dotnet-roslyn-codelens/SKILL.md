---
name: dotnet-roslyn-codelens
description: Use when reading, navigating, diagnosing, or refactoring any C#, .csproj, or solution file — Roslyn semantics replace grep, Read, and dotnet build.
---

# [DOTNET_ROSLYN_CODELENS]

## [01]-[IRON_LAW]

1. NEVER use `Grep`, `Glob`, or Bash `grep`/`rg` to locate C# symbols, types, methods, interfaces, references, callers, implementations, or usages.
2. NEVER run `dotnet build`, `dotnet msbuild`, `msbuild`, or any build command to surface compiler errors, warnings, or analyzer diagnostics.
3. NEVER manually read a `.cs` file to "grep in my head" or for who uses a symbol, or to check if code compiles.

The semantic tools (`find_callers`, `find_references`, `find_implementations`, `search_symbols`, `get_diagnostics`, `go_to_definition`, etc.) are ALWAYS more accurate than text search and ALWAYS faster than a build. There is no tradeoff to weigh.

## [02]-[PRE-ACTION_CHECKLIST]

Before calling `rg`, `Grep` or `Glob` on `.cs` / `.csproj` / `.sln` / `.slnx` / `.cshtml` files:
1. Is the target a C# symbol (type/member/namespace)?                 → Use `search_symbols` or `find_references`
2. Is the target an attribute?                                        → Use `find_attribute_usages`
3. Is the target a reflection pattern?                                → Use `find_reflection_usage`
4. Is it genuinely a string literal, comment, or non-semantic text? → `rg` is OK

Before running `dotnet build` / `msbuild` via Bash:
1. Am I looking for errors, warnings, or analyzer diagnostics?        → Use `get_diagnostics`. Stop
2. Am I actually trying to produce a binary / run tests / package?    → Build is OK. State why

Before `Read`ing a `.cs` file:
1. Do I just need structure (what's in it, what's defined)?           → `get_file_overview` / `get_type_overview`
2. Do I need a specific method's shape?                               → `analyze_method`
3. Do I need one or more members' actual source bodies?               → `get_method_source` (batch-friendly — pass all the names at once)
4. Do I need the actual source lines to edit?                         → `Read` is OK.

## [03]-[RESPONSE_SHAPE]

All list-returning tools wrap their results in an envelope:

```json
{
  "items": [...],
  "totalCount": 142,
  "truncated": false,
  "limit": 500,
  "summary": { ... }
}
```

When `truncated` is `true`, `items` are the top N by the tool's natural sort order (severity-first, worst-first, by-project, ...).

Tools that include a `summary` aggregate:
- `get_diagnostics` — `{ error, warning, info, hidden }` counts
- `find_references` — `{ byProject: { name: count }, byKind: { kind: count } }`
- `find_callers`, `find_attribute_usages` — `{ byProject: { name: count } }`
- `search_symbols`, `find_reflection_usage` — `{ byKind: {...} }`
- `find_unused_symbols` — `{ byKind: {...}, filteredOut: { testMethod, testContainer, mcpTool, generated, composition, interop } }`
- `find_naming_violations` — `{ byRule: {...} }`
- `get_complexity_metrics` — `{ max, avg, overThreshold, maxCognitive }` — `max`/`avg`/`overThreshold` describe what `metric` selected; `maxCognitive` is always the cognitive one
- `resolve_stack_trace` — `{ byOrigin: { source, metadata, unresolved }, exceptions, skippedFrameLike }` — `skippedFrameLike` counts frame-like-but-unparseable lines

## [04]-[ERRORS_RESPONSES]

When a tool can't proceed, the response is `isError: true` with content carrying a JSON body of `{ code, message, details? }`. Switch on `code`:

| [INDEX] | [CODE]               | [MEANING]                                               | [COMMON_SOURCE]                                                                   |
| :-----: | :------------------- | :------------------------------------------------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `SymbolNotFound`     | type/method/property not resolved                       | `analyze_method`, `get_symbol_context`, `get_type_overview`, `get_type_hierarchy` |
|  [02]   | `SolutionNotTrusted` | blocked until `trust_solution` is called                | `get_diagnostics` (`includeAnalyzers: true`), `get_code_fixes`                    |
|  [03]   | `AmbiguousMatch`     | multiple matches; `details.matches` lists candidates    | `set_active_solution`, `unload_solution`                                          |
|  [04]   | `FileNotFound`       | file path / baseline doesn't exist or isn't in solution | `get_file_overview`, `find_breaking_changes`                                      |
|  [05]   | `ProjectNotFound`    | solution name didn't match                              | `set_active_solution`, `unload_solution`                                          |
|  [06]   | `InvalidArgument`    | malformed / unsupported caller input                    | various                                                                           |
|  [07]   | `Internal`           | unexpected; `message` carries exception text            | fallback                                                                          |

If `code: SolutionNotTrusted`, the right next step is calling `trust_solution`. Don't catch and retry blindly.

## [05]-[PLANNING_A_CHANGE]

1. `get_type_overview` — context + hierarchy + diagnostics
2. `analyze_change_impact` — blast radius (files, projects, call sites)
3. `find_references` / `find_callers` / `find_implementations` — detailed dependency breakdown
4. `get_project_dependencies` — architectural position
5. `get_di_registrations` — wiring
6. `find_reflection_usage` — dynamic coupling
7. `find_attribute_usages` — attribute-driven behavior
8. `get_diagnostics` — existing issues
9. `get_code_fixes` / `get_code_actions` → `apply_code_action` — auto-fixes and refactorings
10. `find_unused_symbols` — dead code to delete instead of refactor

Name concrete types, interfaces, and call sites:
- [WRONG]: "the services that implement this...."
- [CORRECT]: "`IUserService` has three implementations: `UserService`, `CachedUserService`, `AdminUserService`"

## [06]-[WHEN_TO_USE_EACH_TOOL]

### [06.1]-[EXTERNAL_ASSEMBLIES]

Look up an external type by name   → `go_to_definition` / `get_symbol_context` / `get_type_overview` / `get_type_hierarchy`
Browse what a package exposes      → `inspect_external_assembly`
Who in my code uses an ext. type?  → `find_references` / `find_callers` / `find_implementations`
See a method's IL bytecode         → `peek_il` (pass fully-qualified method name with param types)
Inspect an arbitrary DLL           → add a <ProjectReference> to a throwaway project, `rebuild_solution`, then use normally

### [06.2]-[NAVIGATING_CODE]

- `go_to_definition` — jump to the definition.
- `search_symbols` — fuzzy symbol lookup.
- `find_references` — every reference across the solution, each tagged with a `referenceKind` and reported per occurrence. Pass `kinds` to filter server-side.

Reference kinds:
- `read`, `write` (assignment target, `out` argument), `readwrite` (compound assignment, `++`/`--`, `ref` argument)
- `invocation`, `method_group` (method used as a delegate, not called)
- `object_creation`, `cast` (`(T)x`, `x as T`)
- `type_check` (`x is T`, `is T v`, `case T v:`), `typeof`, `base_type`, `type_constraint`, `type_argument`,
- `declaration` (variable/parameter/return/field type positions), `attribute`, `nameof`, `xml_doc` (`<see cref=...>`)
- `usage` is a rare fallback. NOTE a receiver reads: in `_map[k] = v` the field `_map` is `read` (its contents change, the field isn't reassigned) — same as `_map.Add(k, v)`.

### [06.3]-[UNDERSTANDING_A_CODEBASE]

- `get_project_dependencies` — solution architecture, how projects relate.
- `get_public_api_surface` — Enumerate every public/protected type and member declared in projects, inherited members do NOT appear.
- `find_breaking_changes` — Diff the current public API surface against a baseline (JSON snapshot from `get_public_api_surface`, or `.dll` file).
- `get_file_overview` — types defined in a file + diagnostics, without reading it.
- `get_type_overview` — one-shot: context + hierarchy + diagnostics (replaces 3 calls).
- `get_type_hierarchy` — inheritance chains and extension points.
- `get_symbol_context` — full context for a type (namespace, base, interfaces, DI deps, public members).
- `get_overloads` — Every overload of a method or constructor (source + metadata) with full details. One call instead of N `analyze_method` calls.
- `get_operators` — every `+`, `-`, `==`, `<`, conversion. Includes synthesized record equality. Covers what `get_overloads` excludes.
- `get_call_graph` — Transitive caller/callee graph for a method, depth-bounded with cycle detection. Use when you need depth > 1 (`analyze_method` is depth=1).
- `analyze_method` — signature + callers + outgoing calls, all in one.
- `get_method_source` — full declaration source for one or many members in one call: methods (all overloads), constructors (`Type.Type`, nested types fully qualified), properties, indexers (`Type.this`), fields, events, explicit interface implementations. Per-item status (`ok`/`notFound`/`ambiguous`/`metadata`/`unsupportedKind`) so a batch never fails wholesale; `metadata` items carry `Origin` for `peek_il`. Whole types are out of scope (`get_type_overview` / `Read`).
- `get_extension_methods` — every extension member applicable to a type, from the solution AND referenced assemblies, so LINQ shows up for an `IEnumerable`.
Applicability is the compiler's reduction: `this IEnumerable<T>` applies to `string`, `this IEnumerable<string>` does not. `signature` is the reduced call-site form, return type first (`IEnumerable<int> Where<int>(Func<int, bool>)`). `isStatic: false` is an instance call (`value.Doubled()`, every classic `this` extension); `true` is a C# 14 static extension member called on the type (`int.Zero`). Receivers may be keywords, constructed generics, arrays, nullables, or tuples. Results ignore `using` scope, so `namespace` is always reported and the import may be missing. Source sorts before metadata; `nameFilter` narrows by substring.
- `get_instantiation_options` — how to construct a type in one call: `constructors` (parameters, accessibility, `isImplicit`, `isObsolete`), `factories`, `diRegistrations`, and `requiredMembers` for the object initializer. `isImplicit` marks the compiler-supplied parameterless constructor no source file mentions. `factories` are static members returning the type from anywhere in the solution (`WidgetFactory.Create()` for a private-constructor type); `Task<T>`/`ValueTask<T>` are unwrapped and flagged `isAsync`; instance builders are excluded. Pass `fromProject` to compute `accessible` from that project honouring `InternalsVisibleTo`; `accessible: null` means not computed, never inaccessible. Interfaces, abstract and static classes report `instantiable: false` with a `note`; follow with `find_implementations`.

### [06.4]-[FINDING_DEPENDENCIES_AND_USAGE]

- `get_di_registrations` — `IServiceCollection` registrations of a type, with lifetime.
- `find_reflection_usage` — hidden/dynamic coupling (`Activator.CreateInstance`, `MethodInfo.Invoke`, assembly scanning).
- `find_implementations` — all implementors of an interface / extenders of a class.
- `find_callers` — every call site for a method.
- `find_attribute_usages` — members decorated with a given attribute.
- `find_event_subscribers` — every `+=` / `-=` site for an event symbol, with resolved handler name and subscribe/unsubscribe tag. Use for UI-event audits or memory-leak hunts.
- `find_tests_for_symbol` — xUnit/NUnit/MSTest methods that exercise a production symbol; opt-in transitive walk through helpers.
- `find_uncovered_symbols` — Public methods and properties no test transitively reaches (≤ 3 helper hops). Strict reference-based.
- `get_test_summary` — Per-project inventory of test methods with framework, attribute kind, row count, location, and production symbols referenced. Project → tests direction; complements `find_tests_for_symbol` (test → production).
- `generate_test_skeleton` — test-class skeleton (parseable C#) for a type (one stub per public method) or a single method. Returns framework (auto-detected, override with `framework`), suggested file path, class name, file contents, and TodoNotes such as constructor dependencies. Stubs: happy-path Fact, Theory + InlineData for primitive parameters, `Assert.Throws<T>` per direct-throw type, async detection. Returns text only; `Write` it yourself. Pairs with `find_uncovered_symbols`.
- `get_nuget_dependencies` — NuGet packages and versions.
- `find_obsolete_usage` — Every [Obsolete] call site grouped by deprecation message and severity. Sharper than `find_attribute_usages` for migration planning.

### [06.5]-[DIAGNOSTICS]

- `get_diagnostics` — compiler errors, warnings, analyzer diagnostics. Replaces `dotnet build` output.
- `get_code_fixes` — structured edits for a diagnostic.

#### [06.5.1]-[TRUST_MODEL]

`get_diagnostics` defaults to `includeAnalyzers=false` (compiler diagnostics only). If the user asks for analyzer warnings (StyleCop, Microsoft.CodeAnalysis.Analyzers, CA-prefixed rules, etc.) — OR if you need to call `get_code_fixes` for an analyzer-rule diagnostic:

1. Call `get_diagnostics(includeAnalyzers=true)`
2. If the server returns "untrusted solution" -> call `trust_solution`
3. Use `scope="persistent"`, use `scope="addRoot"` to trust an entire directory tree (e.g., `c:\projects\`)
4. Use `list_trusted_paths` to inspect current state when the user asks "is X trusted?". Use `revoke_trust` to drop entries

Solutions passed on the CLI at server startup are auto-trusted in session scope — `get_diagnostics(includeAnalyzers=true)` against them works without an extra prompt.

- `get_code_actions` — all refactorings/fixes at a position (with optional range).
- `apply_code_action` — execute a refactoring by title. Preview by default. Refuses to overwrite files changed on disk since the snapshot (`rebuild_solution`, retry); on success the in-memory snapshot updates immediately. Actions that create a new file write it, but the file enters the snapshot only once the watcher sees it — the result's `warning` says so.
- `rename_symbol` — solution-wide rename of a type or member (Roslyn Renamer, not reachable via `apply_code_action`); cascades to references, overrides, `nameof`, and crefs. Locals, parameters, and file renames are unsupported. Preview by default; apply refuses on new-compiler-error conflicts or a degraded load unless `force=true`, and on files changed since the snapshot regardless (`rebuild_solution`, retry). Snapshot updates immediately on apply. Generic types accept the arity-free name (`Data.Repository` finds `Repository<T>`).
- `change_signature` — add, remove, and reorder a method's parameters, rewriting every call site (Roslyn engine, not reachable via `apply_code_action`). `operations` apply in order: `remove` by name, `reorder` with a full permutation of the surviving names, `add` with a required `callSiteValue` that every existing call passes — or a `defaultValue` that leaves existing calls untouched. Named arguments, optional parameters, `params`, and the extension `this` are handled; rejected: moving `this` off first position or leaving `params` anywhere but last. `cascadedTo` lists rewritten overrides and interface implementations — check it in preview. Same gates as `rename_symbol`.
- `resolve_stack_trace` — map a pasted .NET stack trace to file/line/symbol; demangles async/iterator state machines, lambdas, and local functions; handles log-prefixed lines, inner-exception chains, and Demystifier traces. Source frames get the declaration site, or the exact location when the trace carries `in file:line`; assemblies referenced by the solution resolve with `origin="metadata"`, anything else `origin="unresolved"`. Unparseable frame-like lines stay in place as `Kind="unknown"`, counted in `summary.skippedFrameLike`. Items keep trace order.
- `analyze_data_flow` — variable lifecycle over a statement range (declared/read/written/captured/flows-in/out).
- `analyze_control_flow` — reachability, returns, unreachable paths.

Code generation is in `apply_code_action` — do NOT look for dedicated generation tools. Use `get_code_actions` to find the title, then `apply_code_action`:
- Implement missing interface/abstract members → "Implement abstract members" / "Implement interface"
- Generate constructor from fields → "Generate constructor"
- Add null checks → "Add null checks for all parameters"
- Generate `Equals`/`GetHashCode` → "Generate Equals and GetHashCode"
- Encapsulate field → "Encapsulate field"
- Extract method → "Extract method"
- Inline variable → "Inline variable"

### [06.6]-[CODE_QUALITY_ANALYSIS]

- `find_unused_symbols` — dead code (reference-based). Auto-filters test methods, MCP tool entry points, source-generator output, MEF-composed services, and interop-laid-out fields; counts surface in `summary.filteredOut`.
- `get_complexity_metrics` — complexity per member (methods, constructors, properties, indexers, operators). Reports three numbers per row:
  - `complexity` — cyclomatic: how many independent paths run through the member. Starts at 1, so a straight-line method scores 1. Good for "how many tests do I need".
  - `cognitive` — how hard the member is to follow: nesting costs extra, a whole `switch` costs 1, `else`/`else if` cost 1 without a nesting penalty. Starts at 0 — 0 means nothing branches.
  - `maxNesting` — the deepest control structure, counting lambda and local-function bodies.
  Which to use: `cognitive` is the better refactoring-priority signal — it is what separates a flat 20-case dispatch (easy) from four levels of nested `if` (hard), which cyclomatic scores the same. Use `cyclomatic` for test-coverage budgeting. The `metric` parameter (`"cyclomatic"` default, or `"cognitive"`) selects which one `threshold` filters on and the sort orders by; both numbers are always in the response. `summary` reports `max`/`avg`/`overThreshold` over the selected metric plus `maxCognitive` alongside.
- `find_naming_violations` — .NET naming conventions.
- `find_async_violations` — sync-over-async (`.Result`/`.Wait()`/`GetAwaiter().GetResult()`), `async void` outside event handlers, missing awaits, and fire-and-forget tasks, with severity per violation. Skips test projects and generated code; static analysis only.
- `find_disposable_misuse` — `IDisposable`/`IAsyncDisposable` locals not wrapped in `using`/`await using`, returned, or assigned to a field or `out` parameter (warning), and discarded disposable creator/factory calls (error). Methods only; ownership transfer through an argument is not detected. Skips test projects and generated code; static analysis only.

### [06.7]-[EXCEPTION_ANALYSIS]

- `get_exception_flow` — what can escape a method. Walks callees to `maxDepth` (default 3, cycle-safe) collecting explicit throws and propagates each up through every enclosing `try`/`catch`, reporting `escapes`, the `path`, and where it was caught. `origin` is `thrown` (a source throw site) or `documented` (an `exception` XML tag on a metadata callee; `includeDocumented: false` drops them). A `when`-filtered catch never counts as catching, since the filter may decline at runtime; `hasFilter: true` means the exception escaped past such a clause, so `escapes: false` always pairs with `hasFilter: false`.
- `find_throw_sites` — every throw of an exception type, solution-wide; `includeDerived` matches subclasses. `throw;` rethrows resolve to the enclosing catch's type. Unlike `get_exception_flow`, throws inside lambdas and local functions count — they are throw sites, they just don't escape the enclosing method.
- `find_catch_blocks` — every `catch` for a type; `includeBaseClauses` adds `catch (Exception)` and bare `catch`. Each item carries `hasFilter`, `rethrows`, and `isEmpty`, so silent swallowing is `isEmpty: true, rethrows: false`.

Limits (all three): explicit `throw` only — no runtime-implicit exceptions (null deref, division by zero) and no reflection-invoked throws; virtual and interface calls follow the declared symbol, not runtime overrides. `get_exception_flow` explores each callee via the shallowest path it finds. Async is modelled as synchronous: a throw inside an `async` method propagates at the call site, so an enclosing `try` counts as catching it — right for awaited calls, wrong for fire-and-forget (`_ = M();`), where nothing enclosing sees it.
- `find_large_classes` — oversized types.
- `find_god_objects` — types crossing all three size thresholds (lines/members/fields) AND a coupling threshold (incoming or outgoing namespace count), each configurable. Sharper than size alone: a large isolated class isn't flagged; a 200-line class called from many namespaces is.
- `find_circular_dependencies` — project/namespace cycles.
- `check_architecture` — layering rules supplied inline: `forbid` (`Domain.*` must not depend on `Infrastructure.*`) catches the violation you expect; `allowOnly` (`Api.*` may depend only on `Application.*`, `Domain.*`) catches the rest. Edges come from resolved symbols, not `using` directives. Reading an empty result: `allowOnly` evaluates only solution-internal, non-generated targets (framework namespaces and generator output are ignored; use `forbid` to restrict either), and self-references are never violations. Results group per violated edge with a full `referenceCount` and the first `maxSitesPerViolation` sites.
- `get_project_health` — composite audit per project: complexity, large classes, naming, unused, reflection, async violations, disposable misuse — counts plus top-N hotspots per dimension. One call instead of seven audit tools.

### [06.8]-[SOURCE_GENERATORS]

- `get_source_generators` — list generators and their outputs.
- `get_generated_code` — inspect generated source (filter by generator or file path).

### [06.9]-[SOLUTION_MANAGEMENT]

`load_solution` — load a `.sln`/`.slnx` at runtime. `include` (glob) or `rootProjects` (exact) match project file names and load that subset plus its transitive `ProjectReference` closure; a filter matching nothing is an error. Skipped projects are summarised, with per-project reasons in `list_solutions`. For solutions taking minutes to open, pass `background: true` for a `taskId` and poll `get_task_status`; the current solution stays active until the load succeeds.

- `list_solutions` — loaded solutions, which is active. Includes a `SkippedProjects` array per solution.
- `rebuild_solution` — full reload (after `Directory.Build.props` changes, new analyzers/packages, or stale diagnostics).
- `set_active_solution` — switch active solution by partial name.
- `unload_solution` — free memory.

## [07]-[TOOL_QUICK_REFERENCE]

| [INDEX] | [TOOL]                       | [USE_WHEN]                                                                                                                |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `find_implementations`       | "What implements this interface?" / "What extends this class?"                                                            |
|  [02]   | `find_callers`               | "Who calls this method?" / "What depends on this?"                                                                        |
|  [03]   | `find_event_subscribers`     | "Who subscribes to this event?"                                                                                           |
|  [04]   | `find_references`            | "Where is this symbol used?" / "Show all references" / "Who writes to it?" (`kinds` filter)                               |
|  [05]   | `find_tests_for_symbol`      | "What tests cover this method?" / "Which tests will break if I change X?"                                                 |
|  [06]   | `get_test_summary`           | "What does this test suite cover?"                                                                                        |
|  [07]   | `find_uncovered_symbols`     | "What should I write tests for?" / "Where's our testing debt?"                                                            |
|  [08]   | `generate_test_skeleton`     | "Generate a test stub for this method" / "Bootstrap tests for this class"                                                 |
|  [09]   | `go_to_definition`           | "Where is this defined?" / "Jump to source"                                                                               |
|  [10]   | `search_symbols`             | "Find types/methods matching this name"                                                                                   |
|  [11]   | `get_type_hierarchy`         | "What's the inheritance chain?"                                                                                           |
|  [12]   | `get_symbol_context`         | "Give me everything about this type"                                                                                      |
|  [13]   | `get_public_api_surface`     | "What's the public API of this library?"                                                                                  |
|  [14]   | `find_breaking_changes`      | "Will this break consumers?"                                                                                              |
|  [15]   | `get_di_registrations`       | "How is this wired up?" / "What's the DI lifetime?"                                                                       |
|  [16]   | `get_project_dependencies`   | "How do projects relate?"                                                                                                 |
|  [17]   | `get_nuget_dependencies`     | "What packages does this project use?"                                                                                    |
|  [18]   | `find_reflection_usage`      | "Is this used dynamically?"                                                                                               |
|  [19]   | `find_attribute_usages`      | "What's marked [Obsolete]?" / "Find all [Authorize] controllers"                                                          |
|  [20]   | `find_obsolete_usage`        | "What deprecations do we still use?"                                                                                      |
|  [21]   | `get_diagnostics`            | "Any compiler errors?" / "Show warnings"                                                                                  |
|  [22]   | `get_code_fixes`             | "How do I fix this warning?"                                                                                              |
|  [23]   | `get_code_actions`           | "What refactorings are available here?"                                                                                   |
|  [24]   | `apply_code_action`          | "Apply this refactoring" / "Extract method"                                                                               |
|  [25]   | `rename_symbol`              | "Rename this symbol everywhere" / "Change this name across the solution"                                                  |
|  [26]   | `change_signature`           | "Add/remove/reorder a parameter and fix all the callers"                                                                  |
|  [27]   | `resolve_stack_trace`        | "Where did this exception come from?" / "Resolve this stack trace"                                                        |
|  [28]   | `find_unused_symbols`        | "Any dead code?"                                                                                                          |
|  [29]   | `get_complexity_metrics`     | "Which methods are too complex?" / "What should I refactor first?" (`metric: "cognitive"`) / "How deeply nested is this?" |
|  [30]   | `find_naming_violations`     | "Check naming conventions"                                                                                                |
|  [31]   | `find_async_violations`      | "Are there async bugs?" / "Find sync-over-async"                                                                          |
|  [32]   | `find_disposable_misuse`     | "Are there resource leaks?" / "Find missing `using`"                                                                      |
|  [33]   | `get_exception_flow`         | "What can escape this method?" / "Where does this exception get caught?"                                                  |
|  [34]   | `find_throw_sites`           | "Where is this exception type thrown?"                                                                                    |
|  [35]   | `find_catch_blocks`          | "Who catches this?" / "What's swallowing exceptions?"                                                                     |
|  [36]   | `find_large_classes`         | "Find classes that need splitting"                                                                                        |
|  [37]   | `find_god_objects`           | "Which classes are doing too much?"                                                                                       |
|  [38]   | `find_circular_dependencies` | "Any circular dependencies?"                                                                                              |
|  [39]   | `check_architecture`         | "Is anything violating our layering?" / "Does Domain reference Infrastructure?"                                           |
|  [40]   | `get_project_health`         | "How is this project doing?" / "Top hotspots across all dimensions"                                                       |
|  [41]   | `get_source_generators`      | "What source generators are active?"                                                                                      |
|  [42]   | `get_generated_code`         | "Show generated code"                                                                                                     |
|  [43]   | `inspect_external_assembly`  | "What does this NuGet package expose?" / "Show me the API of X assembly"                                                  |
|  [44]   | `peek_il`                    | "Show IL for this method" / "What does this external method do at bytecode level?"                                        |
|  [45]   | `list_solutions`             | "What solutions are loaded?"                                                                                              |
|  [46]   | `load_solution`              | "Load this .sln / .slnx at runtime"                                                                                       |
|  [47]   | `unload_solution`            | "Free memory for this solution"                                                                                           |
|  [48]   | `set_active_solution`        | "Switch to project B"                                                                                                     |
|  [49]   | `rebuild_solution`           | "Reload the solution" / "Diagnostics are stale"                                                                           |
|  [50]   | `start_background_task`      | "Kick off a long rebuild without blocking"                                                                                |
|  [51]   | `get_task_status`            | "Check on a queued background task"                                                                                       |
|  [52]   | `list_running_tasks`         | "What background work is in flight?"                                                                                      |
|  [53]   | `analyze_data_flow`          | "What variables are read/written here?"                                                                                   |
|  [54]   | `analyze_control_flow`       | "Is this code reachable?"                                                                                                 |
|  [55]   | `analyze_change_impact`      | "What breaks if I change this?"                                                                                           |
|  [56]   | `get_type_overview`          | "Give me everything about this type in one call"                                                                          |
|  [57]   | `analyze_method`             | "Show signature, callers, and outgoing calls"                                                                             |
|  [58]   | `get_method_source`          | "Show me this method's body" / "Give me the source of these members"                                                      |
|  [59]   | `get_overloads`              | "What overloads does this method have?"                                                                                   |
|  [60]   | `get_extension_methods`      | "What can I call on this type?" / "Is there an extension for X?"                                                          |
|  [61]   | `get_instantiation_options`  | "How do I construct this?" / "Why can't I `new` this up?"                                                                 |
|  [62]   | `get_operators`              | "What operators does this type define?"                                                                                   |
|  [63]   | `get_call_graph`             | "Transitive callers/callees, depth-bounded"                                                                               |
|  [64]   | `get_file_overview`          | "What types are in this file?"                                                                                            |
