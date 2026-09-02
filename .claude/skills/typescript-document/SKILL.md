---
name: typescript-document
description: "Use when writing or reviewing TSDoc on an exported TypeScript declaration, or when deciding whether a signature and name already document a symbol."
---

# [TYPESCRIPT_DOCUMENT]

Write minimal TypeScript documentation for IntelliSense-ready comments that TypeDoc and TSDoc-aware tools can parse.

[REFERENCES]:
- [01]-[REACT](references/react.md): Props interfaces, the component function, and hook return unions and product context

## [01]-[PRINCIPLES]

- Types first — `interface`, `type`, and signatures are the primary docs
- Why, not what — comment business rules, edge cases, side effects, and performance, and skip restating the type
- TSDoc over classic JSDoc — no `{string}` in `@param`, `@typeParam` for generics, and `@remarks` for long notes
- Comments only — docs-only runs touch comments only, the exception is extracting a named type when an inline object type blocks clear field docs, with no renames, signature changes, or other refactors

## [02]-[SCOPE]

Document exported APIs (types, interfaces, enums, utilities) and non-obvious internals:
- Skip trivial getters and self-explanatory one-liners unless asked
- Skip test helpers and anonymous components

| [INDEX] | [SITUATION]                                         | [ACTION]                               |
| :-----: | :-------------------------------------------------- | :------------------------------------- |
|  [01]   | Signature + name fully explain behavior             | No comment                             |
|  [02]   | Non-obvious contract, units, defaults, side effects | Document                               |
|  [03]   | Throws an exception that escapes                    | `@throws`                              |
|  [04]   | Returns a `Result` or `Effect` failure              | `@returns` states each failure case    |
|  [05]   | Caller needs copy-paste usage                       | `@example`                             |
|  [06]   | Complex object shape                                | Named `interface`/`type` + field JSDoc |

## [03]-[STYLE]

- Summary: one sentence with no trailing period, a third-person verb for functions ("Runs", "Returns"), a shape description for types
- `@remarks`: observable behavior only, never implementation details

## [04]-[ANATOMY]

Required tags by symbol type:

| [INDEX] | [TAG]            | [REQUIRED_WHEN]                                               |
| :-----: | :--------------- | :------------------------------------------------------------ |
|  [01]   | Summary          | Always                                                        |
|  [02]   | `@param name -`  | Every parameter or none — names must match exactly            |
|  [03]   | `@typeParam T -` | Every type parameter or none                                  |
|  [04]   | `@returns`       | Non-void return when the summary does not open with "Returns" |

Order: summary → `@remarks` → params/typeParams → returns/throws → examples.

````ts
/**
 * Summary sentence
 *
 * @remarks
 * Longer explanation (optional)
 *
 * @param name - meaning, units, constraints (not the type)
 * @typeParam T - role of the generic
 * @returns meaning of the result (not the type)
 * @throws {@link ErrorType} when …
 * @example
 * ```ts
 * …
 * ```
 */
````

- One blank line between summary and tag group
- `@remarks` is its own group (blank lines before and after)
- Each `@example` is its own group

## [05]-[INTERFACES]

Prefer a named `interface` or `type` over large inline objects:

```ts
/**
 * Configuration options for the payment processor
 */
interface PaymentConfig {
  /** API key obtained from the dashboard */
  apiKey: string;
  /** Timeout in milliseconds. Default: 5000 */
  timeout?: number;
}
```

## [06]-[WORKFLOW]

Each documentation run reports:
1. Scope — the symbols touched
2. Edits — apply or propose exact comment blocks in context
3. Skipped — symbols left alone and why (trivial / unclear / private)
4. Open questions — only when missing intent makes the docs wrong

Keep the output short, with no lecture on documentation theory.

## [07]-[AVOID]

Tag and summary forms to avoid, each beside its fix:

```ts
// BAD: redundant type annotations in tags
/** @param {string} name - The name */

// GOOD: meaning, not the type
/** @param name - The user's full name */

// BAD: restates the signature
/** Adds two numbers and returns a number. */

// GOOD: only if there is a real rule
/** Adds two amounts in minor currency units (cents) */
```

- Never invent APIs, links, or behaviors not present in code
- When intent is unclear, ask one short question or document only what the code proves
