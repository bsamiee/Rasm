---
name: cpp-coding
description: "Use when writing or reviewing C++ or Objective-C++, covering type and ownership design, failure as values, compile-time evaluation, sequences, memory layout, and concurrency"
---

# [CPP_CODING]

Translation units take the standard library as their first package, and a member it lacks comes from the package implementing its proposal.

[PRINCIPLES]:
- Properties a compiler can decide are types, constraints, and constant evaluation, run time decides what input alone decides
- Every pointer, reference, view, and handle states by type whether it owns, borrows, or shares, and no borrow outlives its owner
- Types are regular values built complete in one expression, indirection and identity enter where a resource or sharing needs them
- Failures a caller handles are outcome values, and an exception stops at the dependency call that throws it
- Closed sets are enumerations and variants, open sets constrained templates, and virtual dispatch serves implementations arriving at run time
- Clocks, random engines, environment values, and files enter as parameters, a function reading one from a static hides an input
- Guards answer a state their types admit: a null test on a reference, a `default` arm over a listed enumeration, and a `catch` supplying a fallback go
- Templates, concepts, policies, and options exist for a second present use, one written for a single caller folds back into it
- Standard and package members state each algorithm, container, parse, format, and wait before a hand-written loop or type does
- Hot paths get faster by less work, denser layout, and fewer allocations and copies, each measured, never by a dropped case, a lock, or a wait

[REFERENCES]:
- [01]-[TYPES](references/types.md): Constrained and tagged values, sum types, enumerations, and resource owners
- [02]-[OWNERSHIP](references/ownership.md): Bound views through an explicit object parameter, optional borrows, captures, and caller arenas
- [03]-[FAILURE](references/failure.md): `std::expected` pipelines, accumulated failures, foreign failures, messages, and the entry point
- [04]-[COMPILE_TIME](references/compile-time.md): Checked literals, computed tables, constraints, and packs
- [05]-[SEQUENCES](references/sequences.md): Projections, view pipelines, positions, and moved elements
- [06]-[MEMORY](references/memory.md): Field layout, column storage, generation handles, caller buffers, and checked arithmetic
- [07]-[CONCURRENCY](references/concurrency.md): Scoped workers, guarded state, and waits
- [08]-[MODULES](references/modules.md): Module units, partitions, the global module fragment, and a header face

## [01]-[CURRENT_FORMS]

Forms recalled from an older standard take the member that replaced them:

| [INDEX] | [OLDER_FORM]                                          | [CURRENT_FORM]                                                          |
| :-----: | :---------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | CRTP base, one overload per `const` or `&&` qualifier | Explicit object parameter (`this Self&& self`), `std::forward_like`     |
|  [02]   | Recursive pack template, `std::index_sequence`        | Pack indexing, fold expression, structured binding pack, `template for` |
|  [03]   | Tag dispatch, `static_assert` over a trait in a body  | Concept, `requires` clause, `if constexpr (requires { ... })`           |
|  [04]   | Output parameter, `std::pair<T, bool>`, sentinel      | `std::optional`, `std::expected`, aggregate return                      |
|  [05]   | Nullable `T*` parameter or return                     | `std::optional<T&>`                                                     |
|  [06]   | `std::stoi`, `strtol`, `sscanf`, stream extraction    | `std::from_chars`                                                       |
|  [07]   | Streams, `std::to_string`, string concatenation       | `std::format`, `std::print`, `std::format_to_n`                         |
|  [08]   | `std::map` or `std::set` read more than written       | `std::flat_map`, `std::flat_set`                                        |
|  [09]   | `std::visit` with a visitor class                     | Member `visit` over an overload set of lambdas                          |
|  [10]   | Thread with a flag and a manual `join`                | `std::jthread` with `std::stop_token`                                   |
|  [11]   | Pointer with a length, row and column arithmetic      | `std::span`, `std::mdspan` indexed `[row, column]`                      |
|  [12]   | Hand-written overflow or sign test                    | `ckd_add`, `ckd_sub`, `ckd_mul`, `std::in_range`, `std::cmp_less`       |
|  [13]   | `= delete` beside a comment                           | `= delete("<reason>")`                                                  |

- `std::function_ref`, `std::move_only_function`, `std::inplace_vector`, `std::generator`, and `std::execution` senders are absent from libc++
- `std::views::chunk`, `slide`, `cartesian_product`, and `as_const` are absent from libc++, contracts and reflection from Clang
- Absent members come from the package implementing their proposal (`stdexec` for `std::execution`), never a weaker stand-in or a hand copy
- Code inside a host's tree follows that host's dialect: its containers, naming, and macros, with exceptions and RTTI off where it sets them off
- Named modules, `import std`, and members a host's compiler lacks stay out of code that host builds

## [02]-[TYPES]

Each state a consumer inspects is one alternative, and a constructor or factory is the only path to a value:

| [INDEX] | [CONCERN]                             | [TYPE]                                                                 |
| :-----: | :------------------------------------ | :--------------------------------------------------------------------- |
|  [01]   | Closed set of labels                  | `enum class` over a stated underlying type                             |
|  [02]   | Closed set of cases with data         | `std::variant` of aggregates                                           |
|  [03]   | Value with an invariant               | Class with a private constructor and an `expected` factory             |
|  [04]   | Literal with an invariant             | `consteval` constructor                                                |
|  [05]   | Record without an invariant           | Aggregate with public fields, designated initializers                  |
|  [06]   | Quantities sharing a representation   | Tagged wrapper per meaning                                             |
|  [07]   | Duration or instant                   | `std::chrono` duration, `time_point` of `steady_clock` for an interval |
|  [08]   | Owned resource                        | Standard owner, else a move-only class releasing once                  |
|  [09]   | Implementation chosen at compile time | Template constrained by a concept                                      |
|  [10]   | Implementations arriving at run time  | Interface of pure virtual members behind `std::unique_ptr`             |

- Constructed objects are usable: collaborators, limits, and policy arrive as constructor parameters, and no `init` or setter sequence follows
- Types hold members that own their resources and declare no special member, a hand-written owner alone declares all five
- Move operations of a hand-written owner empty their source through `std::exchange`
- Types other objects point into (registered, intrusive, self-referential) delete copy and move
- Members are declared in lifetime order, an owner before each member borrowing from it
- Options are one aggregate with a default member initializer per field, set by designated initializers at its call
- `struct` holds fields no rule relates, `class` holds an invariant its private fields keep
- Flags with companion fields that mean something in one state become one alternative per state
- Layout and trait facts a wire format or a host relies on are a `static_assert` beside the type
- Use `references/types.md` for each form

## [03]-[OWNERSHIP]

Signatures state what a function does with each argument:

| [INDEX] | [USE]                          | [PARAMETER]                                                              |
| :-----: | :----------------------------- | :----------------------------------------------------------------------- |
|  [01]   | Read                           | `const T&`, by value for a trivially copyable type of 2 pointers or less |
|  [02]   | Read a sequence, text, or grid | `std::span<const T>`, `std::string_view`, `std::mdspan`                  |
|  [03]   | Kept or consumed               | By value, moved once into place                                          |
|  [04]   | Changed in place               | `T&`                                                                     |
|  [05]   | Absent or present borrow       | `std::optional<T&>`                                                      |
|  [06]   | Called before return           | Template parameter constrained by `std::invocable`                       |
|  [07]   | Stored callable                | `std::function` when copyable, else a type parameter of its owner        |
|  [08]   | Forwarded by generic code      | `T&&` with `std::forward`                                                |

Stored relations take the type naming them:

| [INDEX] | [RELATION]                          | [TYPE]                                                            |
| :-----: | :---------------------------------- | :---------------------------------------------------------------- |
|  [01]   | Sole owner                          | Value member, `std::unique_ptr` for an incomplete or virtual type |
|  [02]   | Owners with unrelated lifetimes     | `std::shared_ptr<const T>`                                        |
|  [03]   | C handle                            | `std::unique_ptr<T, Deleter>`, filled through `std::out_ptr`      |
|  [04]   | Element of a pool reusing its slots | Index with a generation                                           |
|  [05]   | Borrow held by a member             | Reference or view to an owner declared before it                  |

- Members returning a reference or view into their object mark its object parameter `[[clang::lifetimebound]]`
- Element references, iterators, and views end at their container's next insertion, erase, or move, an index or key survives it
- Lambdas stored, posted, or returned capture by value or by move in an init-capture, `[&]` stays for a lambda called before its scope ends
- Callbacks an object registers with a longer-lived source are removed by a handle it holds as a member
- Coroutines take each parameter by value: a reference, view, or pointer parameter dangles once the caller's full-expression ends
- Coroutine lambdas capture nothing, captures belong to a lambda object that can end before its frame resumes
- No lock, reference into a container, or iterator is held across a suspension point
- Callees reading a shared object take `const T&`, a `std::shared_ptr` copy is an atomic increment per call
- Use `references/ownership.md` for bound views, optional borrows, captures, and arenas

## [04]-[FAILURE]

Every fallible function returns an outcome type, and one type serves one concern:

| [INDEX] | [TYPE]                             | [CONCERN]                                        |
| :-----: | :--------------------------------- | :----------------------------------------------- |
|  [01]   | `std::optional<T>`                 | Absence without a reason                         |
|  [02]   | `std::expected<T, E>`              | One failure a caller branches on                 |
|  [03]   | `std::expected<void, E>`           | Effect that reports failure and returns no value |
|  [04]   | `std::expected<T, std::vector<E>>` | Every failure of independent checks              |

Dependent steps bind and stop at their first failure, independent checks all run and return every failure:

| [INDEX] | [STEP]                        | [MEMBER]               |
| :-----: | :---------------------------- | :--------------------- |
|  [01]   | `T -> U`                      | `transform`            |
|  [02]   | `T -> expected<U, E>`         | `and_then`             |
|  [03]   | `E -> F`                      | `transform_error`      |
|  [04]   | `E -> expected<T, F>`         | `or_else`              |
|  [05]   | Value at the composition root | `value_or`, `error_or` |

- Packages declare one refusal enumeration, a case per response a caller gives, and a failure with a subject is an aggregate of both
- Foreign failures stay `std::error_code`, compared against `std::errc` where a caller branches
- Calls into a dependency that throws take its non-throwing overload, else one function catches at that call and returns `std::expected`
- Units compiled with exceptions off call no member that reports by throwing (`at`, `value`, `std::stoi`)
- No exception crosses a module interface, a C boundary, a thread entry, a coroutine frame, or a destructor
- `noexcept` states a fact about every callee, an exception leaving a `noexcept` function ends the process
- Refusals inside constant evaluation call a declared, undefined function, `throw` fails to compile where exceptions are off
- Operations that can refuse run each check before their first write, a refusal leaving its subject as it was
- Use `references/failure.md` for pipelines, accumulation, and the entry point

## [05]-[COMPILE_TIME]

Functions a constant expression can evaluate are `constexpr`, and a value that must exist before run time comes from `consteval`:
- Templates constrain each parameter by the narrowest standard concept their body calls, a declared concept on its second use
- Types a template refuses fail its constraint, so the refusal is an overload no call selects and a `requires` expression can read
- Template bodies reading no template parameter are a non-template function the template calls, one instance serving every type
- Tables a formula fills are `constexpr` values built by a lambda called in place
- Objects with static storage are `constexpr`, `constinit` where they change
- Use `references/compile-time.md` for checked literals, constraints, and packs

## [06]-[SEQUENCES]

Containers follow their most frequent operation:

| [INDEX] | [OPERATION]                                | [CONTAINER]                                                 |
| :-----: | :----------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Ordered reads, appends                     | `std::vector`, reserved once at a known count               |
|  [02]   | Count fixed at compile time                | `std::array`                                                |
|  [03]   | Keyed lookups, read more than written      | `std::flat_map`, `std::flat_set`                            |
|  [04]   | Keyed lookups under steady insert or erase | `std::unordered_map`, `std::map` where key order is read    |
|  [05]   | Constant lookups                           | Sorted `constexpr` `std::array`, `std::ranges::lower_bound` |
|  [06]   | Insertion and removal at both ends         | `std::deque`                                                |
|  [07]   | Grid                                       | One `std::vector` under a `std::mdspan`                     |

- Views transform and one `std::ranges::to` collects, a collection between stages is a second pass
- Algorithms take a member pointer as projection in place of a comparator lambda reading one member
- Reductions take their named member (`fold_left`, `max`, `count_if`, `any_of`, `contains`), a fold restating one goes
- Positions come from `std::views::enumerate`, parallel sequences from `zip`, neighbors from `pairwise` or `adjacent`
- Views borrow their range: a pipeline over a temporary or a local returns collected values
- Views compute on each pass, a result more than one consumer reads is collected once
- Elements leave an owned sequence through `std::views::as_rvalue`
- `for` over a range stays where its body performs an effect per element
- Use `references/sequences.md` for projections, pipelines, and positions

## [07]-[MEMORY]

Hot paths change in order: remove work, copies, and allocations, densify layout, fuse passes, then widen to more threads:
- Element types are small trivially copyable aggregates stored contiguously, their fields in descending alignment
- Fields one pass reads without their neighbors are an array of their own, cold fields sit behind an index
- Allocations happen outside a loop: capacity is reserved once, and scratch storage belongs to a caller or a type and is cleared per call
- Functions take a `std::pmr::memory_resource` parameter where their caller picks the source of temporary storage
- Strings a C call or a formatter fills grow through `resize_and_overwrite`, which writes no zero fill
- Queues, caches, and buffers kept across calls hold a bound their owner passes
- Text on a hot path is formatted by `std::format_to_n` into caller storage and parsed by `std::from_chars`
- Run-time dispatch takes a batch: a virtual member or stored callable receives a `std::span`, never one element per call
- Values written by different threads are `alignas(std::hardware_destructive_interference_size)`
- Sizes and offsets multiply in their wide type, each operand widened before its operation
- Arithmetic an input can overflow goes through `ckd_add`, `ckd_sub`, or `ckd_mul` and returns an outcome
- Narrowing conversions follow a `std::in_range` test, mixed-sign comparisons go through `std::cmp_less` and its siblings
- Object bytes are read through `std::bit_cast` and `std::as_bytes`, wire order through `std::byteswap` against `std::endian`
- Use `references/memory.md` for layout, handles, buffers, and checked arithmetic

## [08]-[CONCURRENCY]

Shared state has one owner, and work has a scope that joins it:
- State and its mutex are one type, read and written inside a call holding the lock, a step returning a copy or a moved value
- Threads are `std::jthread` objects declared after the state they borrow, so their join precedes its end
- Work loops read a `std::stop_token`, and a stop request flows from an owner to each worker it started
- Waits take a predicate and a stop token (`std::condition_variable_any::wait`), a sleep or a poll standing for one hides a race
- Elements disjoint by index need no lock, a shared counter is a `std::atomic`
- Atomic orders are `relaxed` where a value publishes no other data, `release` with `acquire` where it does, `seq_cst` where threads agree on one order
- Reviews of a coroutine list each suspension point, what can run while it is suspended, and each value read before it and used after
- Use `references/concurrency.md` for scoped workers, guarded state, and waits

## [09]-[MODULES]

Libraries built by a toolchain that compiles named modules are one module, and each kind of unit has one job:

| [INDEX] | [UNIT]                 | [DECLARATION]             | [HOLDS]                                               |
| :-----: | :--------------------- | :------------------------ | :---------------------------------------------------- |
|  [01]   | Primary interface      | `export module m;`        | `export import :part;` for every interface partition  |
|  [02]   | Interface partition    | `export module m:part;`   | Exported declarations of one concept                  |
|  [03]   | Internal partition     | `module m:part;`          | Declarations units of `m` import and no consumer sees |
|  [04]   | Implementation unit    | `module m;`               | Definitions of functions an interface declares        |
|  [05]   | Global module fragment | `module;` then `#include` | Headers supplying macros or C declarations alone      |

- `import std;` follows the module declaration once per unit
- Modules export no macro: `errno`, `SIGTERM`, and `ckd_add` come from a header in the global module fragment
- Exported names are declarations a consumer writes, everything else stays attached to its module and unexported
- Functions a host calls are declared in a header, defined in an ordinary source, and re-exported by `using`
- Use `references/modules.md` for each unit and the header face

## [10]-[OBJECTIVE_CPP]

Objective-C++ units follow every rule above, and Objective-C objects cross into C++ types under ARC:
- Objective-C objects a C++ member holds are strong references: copying its value retains, and its destructor releases
- References back to an owner are `__weak` members, which read `nil` once that owner ends
- Core Foundation results of a `Create` or `Copy` function enter ARC through `CFBridgingRelease`
- Headers a C++ unit includes name no Objective-C type, an owner defined in a `.mm` unit holds them
- Loops and thread entries creating Objective-C temporaries run inside `@autoreleasepool`
