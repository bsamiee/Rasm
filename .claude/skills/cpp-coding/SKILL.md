---
name: cpp-coding
description: "Use when writing or reviewing C++, covering functional design with named modules, concepts, expected results, and value types"
---

# [CPP_CODING]

Code uses each module's types wherever they state a value:
- Toolchain: C++26 under Clang with compiler extensions off, `import std`, libc++ on macOS and libstdc++ on Linux
- Build: CMake file sets (`FILE_SET CXX_MODULES`, `FILE_SET HEADERS`), conda-forge packages found by `find_package`
- Tests: Catch2 v3

[PRINCIPLES]:
- Facts are stated once where they belong, and everything else derives from that source
- Packages and hosts are used directly through what they provide, with no layer that restates or wraps them
- Every type, export, and helper states something nothing else does, and a private helper with one reader or a forwarder merges into its caller
- Libraries give callers composable parts over a dependency's own types, callers choosing entry point, runtime, transport, and configuration
- Modules export declarations a consumer names, everything else stays attached to its module and unexported
- Values that vary are parameters, a value every caller passes alike folds into its owner, and a field no reader acts on goes
- Invalid states are refused at construction, by a `consteval` constructor for a literal and a factory returning `std::expected` for a runtime value
- Error cases exist where a caller branches on them, every other failure joins its owner's error
- Checker findings and disagreements resolve by a stronger shape or at the cause's owner, never by a suppression, a lowered level, or a narrower rule
- Hot paths get faster by fewer passes, allocations, and parses over one input, never by a dropped case, a lock, or a wait
- Changes leave code smaller or more capable, a move, split, rename, or extracted helper changes nothing

[REFERENCES]:
- [01]-[MODULES](references/modules.md): Module units, partitions, the global module fragment, and a header face for consumers outside the toolchain
- [02]-[TYPES](references/types.md): Sum types, resource owners, deducing `this`, and compile-time checks
- [03]-[RESULTS](references/results.md): `std::expected` pipelines, accumulated refusals, and refusal policies
- [04]-[RANGES](references/ranges.md): View pipelines, folds, projections, and collection
- [05]-[BOUNDARIES](references/boundaries.md): Formatting, the entry point, C headers, and Catch2 tests

## [01]-[MODULES]

Libraries are named modules, and each kind of module unit has one job:

| [INDEX] | [UNIT]                 | [DECLARATION]             | [HOLDS]                                                 |
| :-----: | :--------------------- | :------------------------ | :------------------------------------------------------ |
|  [01]   | Primary interface      | `export module m;`        | `export import :part;` for every interface partition    |
|  [02]   | Interface partition    | `export module m:part;`   | Exported declarations of one concept                    |
|  [03]   | Internal partition     | `module m:part;`          | Declarations units of `m` import and no consumer sees   |
|  [04]   | Implementation unit    | `module m;`               | Definitions of functions an interface declares          |
|  [05]   | Global module fragment | `module;` then `#include` | Headers carrying macros or C declarations, nothing else |

- `import std;` follows the module declaration once per unit, `import std.compat;` where code names a global C function
- Modules carry no macro: `stderr`, `errno`, `assert`, and `SIGTERM` come from a header in the global module fragment, and `std::cerr` and `std::error_code` stand where a standard member states the same fact
- Headers of a package come before every `import` in a translation unit, a textual standard header after `import std;` redeclares the library
- Interface units sit in `FILE_SET CXX_MODULES`, implementation units in the target's private sources, and a module-providing source outside the file set fails the build
- Partitions imported by an exported declaration are interface partitions, a `PRIVATE` module reaches implementation units alone
- Declarations of a module unit attach to the module, a function a header also declares belongs to the global module and is re-exported by `using`

## [02]-[FUNCTIONS]

Signatures state every value a function accepts and every outcome its caller handles, with a trailing return type:
- Total functions narrow their input to a validated type or widen their output to `std::optional` or `std::expected`
- Pure functions read their arguments alone, the clock, randomness, environment, and files enter as parameters
- `[[nodiscard]]` marks every function returning a value, `noexcept` every function that reports no failure by exception
- `constexpr` marks every function a constant expression can evaluate, `consteval` one that must run at compile time
- Options are aggregates with a default on each field, set by designated initializers at the call
- Callables are template parameters constrained by `std::invocable` or `std::predicate`, and `std::function` stores one (libc++ holds no `std::move_only_function` or `std::function_ref`)
- Members reading `*this` in more than one value category take an explicit object parameter (`this Self&& self`) in place of an overload per qualifier
- Lambdas without captures are `static`, and a lambda bound to a name with one reader moves into that reader

## [03]-[EXPRESSIONS]

Bindings are `const`, and values build in one expression that states each field at creation:
- `switch` over a scoped enumeration lists every enumerator with no `default`, so `-Wswitch` reports a missing case, and `std::unreachable()` follows a `switch` whose every case returns
- `std::variant` folds through member `visit` with one arm per alternative, an index test or `std::get_if` chain restating it
- Conditions choosing a value are a conditional expression or a table, statements stay for effects
- `if` with an initializer scopes a value to its branch, structured bindings name the parts of a pair, tuple, or aggregate
- Loops with a body that accumulates are a range algorithm or a fold, a `for` over a range stays where the body performs an effect
- Recursion bounds its depth by its input (a parsed tree), unbounded work is a fold or an explicit stack

## [04]-[TYPES]

Each valid state a consumer inspects is one alternative, and a type's constructor is the only way to an invalid-free value:

| [INDEX] | [CONCERN]                             | [TYPE]                                                      |
| :-----: | :------------------------------------ | :---------------------------------------------------------- |
|  [01]   | Closed set of labels                  | `enum class` over a stated underlying type                  |
|  [02]   | Closed set of cases with data         | `std::variant` of aggregates                                |
|  [03]   | Value with an invariant               | Class with a private constructor and an `expected` factory  |
|  [04]   | Record without an invariant           | Aggregate with public fields and defaulted comparison       |
|  [05]   | Owned resource                        | Move-only class releasing once, or `std::unique_ptr`        |
|  [06]   | Borrowed sequence, text, or grid      | `std::span`, `std::string_view`, `std::mdspan`              |
|  [07]   | Implementation chosen at run time     | Virtual base behind `std::unique_ptr`, or a stored callable |
|  [08]   | Implementation chosen at compile time | Template constrained by a concept                           |

- Comparisons are `operator<=>` or `operator==` declared `= default`, hand-written relational operators restating them
- Copy operations of an owner are `= delete("<reason>")`, the reason reaching the diagnostic
- Quantities with a unit are a type per unit, a plain number holds a count or a dimensionless ratio
- Integer arithmetic that can overflow returns `std::expected`, and mixed-sign comparison goes through `std::cmp_less`
- Layout and trait facts a consumer relies on are a `static_assert` beside the type

## [05]-[RESULTS]

Every function returns an explicit outcome type, and one type serves one concern:

| [INDEX] | [TYPE]                             | [CONCERN]                                        |
| :-----: | :--------------------------------- | :----------------------------------------------- |
|  [01]   | `std::optional<T>`                 | Absence without error                            |
|  [02]   | `std::expected<T, E>`              | One failure a caller branches on                 |
|  [03]   | `std::expected<T, std::vector<E>>` | Every failure of independent checks              |
|  [04]   | `std::expected<void, E>`           | Effect that reports failure and returns no value |

Dependent steps bind and stop at the first failure, independent checks run in full and return every failure:

| [INDEX] | [STEP]                        | [MEMBER]          |
| :-----: | :---------------------------- | :---------------- |
|  [01]   | `T -> U`                      | `transform`       |
|  [02]   | `T -> expected<U, E>`         | `and_then`        |
|  [03]   | `E -> F`                      | `transform_error` |
|  [04]   | `E -> expected<T, F>`         | `or_else`         |
|  [05]   | Value at the composition root | `value_or`        |

- Each package declares one refusal enumeration, a case per response a caller gives, with a `label` function over it
- Foreign failures stay `std::error_code` unchanged, compared against `std::errc` where a caller branches
- `operator*` and `value()` on an outcome appear after a test of that outcome in the same expression, never as the test's replacement
- Exceptions cross no module boundary: an interface function is `noexcept` or returns `std::expected`
- `catch (...)` with an empty body hides a cause and goes

## [06]-[CONCEPTS]

Templates constrain every parameter by the narrowest concept their body calls, a standard concept before a declared one:
- Concepts are declared for a requirement more than one template states, a requirement with one reader is a `requires` clause on that template
- Abbreviated parameters (`std::integral auto value`) state a constraint no later parameter repeats
- Policies are a type parameter constrained by a concept over a static member, chosen by the caller
- `if constexpr (requires { <expression>; })` branches on a capability inside one body
- Types a template must refuse are a concept of negated requirements asserted once by `static_assert`
- Parameter packs index by `Types...[N]`, fold by a fold expression, and expand in place of recursion
- `std::enable_if`, tag dispatch, and `static_assert` over a type trait in a template body restate a constraint

## [07]-[CONCURRENCY]

Shared state has one owner, and work has a scope that joins it:
- `std::jthread` owns a thread and takes a `std::stop_token`, a detached thread or a boolean flag restating them
- `std::scoped_lock` guards a mutex for one block, and a value handed out of the block is a copy or a move
- `std::atomic` operations name their memory order where it is weaker than sequential consistency
- Condition waits take a predicate, a sleep standing for synchronization hides a race
- Coroutines returning a value type return `std::expected` for a failure a caller branches on
- Namespace-scope and function-static mutable objects go, state enters through a parameter

## [08]-[HOSTS]

Unreal Engine modules compile under UnrealBuildTool with Apple's or Microsoft's toolchain at the engine's own standard, and import no named module:
- Host code follows the host's dialect in its own tree: Epic's naming, reflection macros, and containers
- Libraries reach a host as a static library and `FILE_SET HEADERS`, installed by `cmake --install` under `Source/ThirdParty/<library>` of the plug-in
- External modules (`ModuleType.External`) list that include folder and library, and the plug-in builds for the architectures the library holds
- Header-face functions belong to the global module, compile at the host standard, and pass types with one layout in both standard libraries
- Use `references/modules.md` for the header face
