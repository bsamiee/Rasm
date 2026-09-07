# [TYPESCRIPT_COMPOSITION]

Use the resolved Effect package and TypeScript compiler to replace representations and execution scopes with their owning operations.
Compiler generation and language capability are separate facts. Resolve supported syntax, emit, and library overloads from primary sources.

## [01]-[SCHEMAS]

Trace each schema from its external input through decoding, encoding, and consumers. Retain fields that establish the boundary contract,
even when a consumer projects only part of the decoded value. Projection alone does not prove that the remaining validation is redundant.
Keep coercion, transformations, optionality, refinement, error paths, and error accumulation.

When nested constructors obscure a required wire structure, compose the schema from its innermost meaningful value outward. Each stage
states its object field and array boundary directly. A module initializer can compile the resulting decoder once. Stage callbacks run
during schema construction, not once per decoded row. Keep the same schema constructors and parse options. Unary runtime arity does
not establish generic inference through `pipe` or `flow`: overloaded constructors can select a different signature. Retain an inline
argument adapter when it selects the intended overload, and typecheck the composed decoder under the owning compiler options.

Nested benchmark data demonstrates the distinction: task, benchmark, assertion, and result arrays encode different wire nodes. Their
structure remains even when a flat initializer replaces the deeply nested expression. A table of field strings plus a generic wrapping
loop hides the contract and loses inference. Separate schema declarations add value when they name reused invariants or public contracts.

An interface that repeats a schema's decoded shape can use the schema's derived type when callers require the same contract. Keep types
that model an encoded value, a brand, a host interface, or an independent domain invariant. Apply `satisfies` when a literal must conform
without changing its inferred type, and retain annotations required by declaration emit or overloaded operations.

## [02]-[FUNCTIONS]

An arrow returning only `Effect.gen(function* () { ... })` adds a function around the package's function constructor. Consider
`Effect.fnUntraced(function* (input) { ... })` when the generator composes the operation and tracing is not part of the contract.
Keep acquisition and release inside the same scope. Removing the outer arrow reduces nesting without moving work into another helper.

Before converting, compare parameter defaults and destructuring, `this`, `arguments`, function identity, explicit return annotations,
generic parameters, and the timing of captured reads. Defaults can move from function invocation to effect execution. Work performed
before `Effect.gen` constructs its value can require the original wrapper. `Effect.fn` adds tracing and is not an interchangeable spelling.

For a callback that only feeds its input through `pipe`, consider `flow` with the same stages. The first stage receives the callback's
arguments. Resolve their arity before replacing an adapter: collection callbacks pass an index, promise adapters pass an AbortSignal,
and decoder or disposal functions can interpret either as an optional argument. Such argument adapters add behavior and remain.

Resolve forwarding declarations through their consumers. Keep a domain conversion, composed policy, required host signature, or useful
partial application. An alias can snapshot a mutable binding or evaluate a getter once. A stored callback or partially applied runtime
runner does not execute its body.

## [03]-[COMPOSITION]

Choose the operation by its complete result and execution contract:

- Keep computations inside map callbacks when they evaluate on execution, while `as` receives a value at composition
- Keep recovery chains when later handlers catch failures produced by earlier handlers, while `catchTags` dispatches the original failure once
- Derive concurrency from independence and error accumulation from the result contract, an explicit concurrency option establishes neither
- Keep direct data-first calls within the nesting limit and count source values separately from transformation steps
- Keep local accumulation when it expresses the result directly, while establishing ownership and escape through callers
- Resolve receiver types and bindings before changing collection methods, length tests, or await expressions
- Preserve enum reverse mappings and member types when replacing declarations
- Preserve host contracts, including command-model mutation and Promise results in framework callbacks
- Introduce Effect at a host callback when composition or services add behavior, an immediate Promise roundtrip adds a runtime layer
- Retain declaration forms required by emit, expression heritage and an extra base alias can complicate isolated declarations

Independent boundary fields can accumulate errors through the decoder's parse options. Dependent resource steps still short-circuit,
and finalizers remain attached to acquisition. Converting a sequential traversal to concurrent evaluation requires evidence that its
operands share no ordering, transaction, output, or mutable-state dependency.

## [04]-[RULES]

Use structural searches to find schema constructors, nested calls, function wrappers, and declaration aliases. Use language tooling to
resolve their bindings and consumers before removing a representation. A nested object or repeated type spelling is a review candidate,
not proof of a violation.

Refine an existing family when it already describes the correction. Execution-ownership rules must recognize the package's function
constructors after a wrapper disappears, including factories stored directly in module object properties. A function needs no extra
module declaration solely to establish its execution owner. Native host collection contracts must not require a class because only a class-field
annotation has an exemption. Keep the declaration's contract across its supported forms.

Automatic rewrites need syntax that preserves every changed operation. Keep semantic transformations in the reviewed source workflow
when argument roles, identity, laziness, or type inference decide correctness. Add distinguishing fixtures when changing a predicate.
`no-fold-by-loop` folds a `push` step as `map` for one element per item and `flatMap` for a spread, and a reassignment step as `reduce`.
