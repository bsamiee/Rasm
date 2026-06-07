# [CSHARP_LANGUAGE]

C# 14.0 on `net10.0` is the active language surface. `Directory.Build.props` owns `TargetFramework`, `LangVersion`, `Nullable`, and `ImplicitUsings`; this page owns syntax and expression selection for C# code in this repository.

## [1]-[ACTIVE_SURFACE]

[ACTIVE_SURFACE]:
- Target framework: `net10.0`
- Language version: `14.0`
- Nullable: `enable`
- Implicit usings: `enable`
- Compiler: .NET 10 SDK C# compiler
- Analyzer authoring: `Microsoft.CodeAnalysis.CSharp` with project analyzer packages

Use the active language surface directly. Replace older syntax when the compiler expresses the same behavior with less ceremony.

## [2]-[REQUIRED_PATTERNS]

| [INDEX] | [PATTERN]                         | [USE]                           |
| :-----: | :-------------------------------- | :------------------------------ |
|   [1]   | extension blocks                  | receiver-owned members          |
|   [2]   | `field` accessors                 | property-local invariants       |
|   [3]   | `ReadOnlySpan<T>` boundaries      | hot read-only inputs            |
|   [4]   | collection expressions            | literal composition             |
|   [5]   | `params` collections              | arity without overload families |
|   [6]   | `nameof(Generic<>)`               | generic identity                |
|   [7]   | switch and pattern expressions    | total value dispatch            |
|   [8]   | `required`, `init`, and records   | immutable carriers              |
|   [9]   | static abstract interface members | generic math                    |

[RULES]:
- Use extension blocks when a wrapper type only forwards or decorates receiver behavior.
- Use `field` when a manual backing field exists only for one property accessor invariant.
- Prefer one `ReadOnlySpan<T>` hot-path boundary over parallel array, string, and span overloads when implicit conversions keep call intent clear.
- Use collection expressions and `params` collections before adding array/list overload families.
- Use switch expressions, property patterns, logical patterns, and list patterns for value-returning domain decisions.

## [3]-[SCOPED_PATTERNS]

[NULL_CONDITIONAL_ASSIGNMENT]:
- Use: nullable host, UI, event, and indexer boundaries with `target?.Prop = value` or `target?.[i] += delta`.
- Reject: domain logic on nullable mutation chains.
- Note: `++` and `--` are not valid on a null-conditional left side.

[PARTIAL_MEMBERS]:
- Use: source-generator, analyzer, binding, or hand/generated splits.
- Reject: splitting files only to make a type look smaller.

[STACK_ONLY_GENERIC_ALGORITHMS]:
- Use: `ref struct` interface implementations, `allows ref struct`, and scoped `ref` locals when a span-like algorithm needs stack-only values.
- Reject: boxing stack-only values through interface conversions or carrying them across `await` or `yield`.

[SYSTEM_THREADING_LOCK]:
- Use: new synchronous boundary gates.
- Reject: holding a lock across `await`.

[REF_READONLY]:
- Use: API contracts that require a variable by reference without mutation.
- Reject: blanket large-struct performance policy.

[OVERLOADRESOLUTIONPRIORITYATTRIBUTE]:
- Use: public overload sets that remain ambiguous after span and `params` shape cleanup.
- Reject: unclear overload design hidden by priority.

[USING_ALIAS_ANY_TYPE]:
- Use: dense tuple, pointer, array, and nested generic signatures.
- Reject: parallel domain type names.

[DEFAULT_LAMBDA_PARAMETERS_AND_SIMPLE_LAMBDA_MODIFIERS]:
- Use: delegate adapters where the target delegate fixes the shape.
- Reject: replacing a coherent method surface with lambda-only defaults.

## [4]-[REPLACEMENTS]

[REPLACEMENT_1]:
- Accepted: `ReadOnlySpan<T>` input plus collection expressions and `params` collections where call shape requires arity.
- Rejected: `T[]`, `IEnumerable<T>`, `List<T>`, and span overload families for the same operation.
- Reason: span conversions and `params` collections carry common call shapes without overload drift.

[REPLACEMENT_2]:
- Accepted: `field` accessor validation.
- Rejected: private backing fields used only by one property setter.
- Reason: the invariant belongs to the property when no cross-property state is involved.

[REPLACEMENT_3]:
- Accepted: extension blocks on the receiver type.
- Rejected: wrapper-only types that rename receiver behavior.
- Reason: the receiver owns the behavior; a wrapper adds shape without capability.

[REPLACEMENT_4]:
- Accepted: `System.Threading.Lock` at new synchronous boundaries.
- Rejected: private `object` locks for new code.
- Reason: the runtime lock type is the synchronization owner.

[REPLACEMENT_5]:
- Accepted: collection expressions with spread.
- Rejected: `new[]`, manual list construction, and concat-then-add boilerplate.
- Reason: the expression form carries construction and composition directly.

[REPLACEMENT_6]:
- Accepted: `nameof(List<>)`.
- Rejected: closed-generic dummy names or string literals for generic type definitions.
- Reason: diagnostics and telemetry should follow symbol identity.

## [5]-[REJECTIONS]

- Interceptors are not a production baseline.
- File-based app preprocessor directives are not a project-library pattern.
- Floating `LangVersion` values are not allowed; use explicit `14.0`.
- Extension conversion operators are not part of the C# 14 extension-operator surface.
- Inline arrays are runtime and library-author machinery unless a measured local owner proves direct use.
- `ExperimentalAttribute` is not a normal authoring pattern for repository domain code.
