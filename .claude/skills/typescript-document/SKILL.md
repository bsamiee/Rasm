---
name: typescript-document
description: "Use when writing or reviewing TSDoc on an exported TypeScript declaration, or deciding whether a signature and name already document a symbol."
---

# [TYPESCRIPT_DOCUMENT]

Covers TSDoc comments the TypeScript language service shows on hover and `tsc --build` copies into `.d.ts` output.

[REFERENCES]:
- [01]-[REACT](references/react.md): React component and hook documentation

## [01]-[PRINCIPLES]

- `interface`, `type`, and signatures are the primary documentation
- Comments add contracts, business rules, side effects, units, defaults, and performance bounds the type cannot state
- `@param` and `@typeParam` descriptions follow the name and a hyphen, with no `{type}`
- Documentation runs change comments alone, except extracting a named type when an inline object type blocks per-member doc comments

## [02]-[SCOPE]

Exported declarations (types, interfaces, enums, functions, constants) and internals with a contract the code does not show take a comment. Trivial getters, one-line helpers, test helpers, and anonymous components take none:

| [INDEX] | [SITUATION]                                             | [ACTION]                                                       |
| :-----: | :------------------------------------------------------ | :------------------------------------------------------------- |
|  [01]   | Signature and name state the whole behavior             | No comment                                                     |
|  [02]   | Boundary code throws an exception that escapes          | One `@throws` block per exception type                         |
|  [03]   | Returns `Effect.Effect<A, E, R>`, `Either`, or `Option` | `@returns` names each `E` case and what `None` or `Left` means |
|  [04]   | Caller needs copy-paste usage                           | `@example`                                                     |

## [03]-[STYLE]

- Function summaries open with a third-person verb (`Runs`), type and member summaries are noun phrases
- Summaries and tag descriptions end without a period
- `@remarks` holds observable behavior, and a business rule the code cannot show when the author has its source

Summary with the unit the signature cannot state:

```ts
/** Adds two amounts in minor currency units (cents) */
```

## [04]-[STRUCTURE]

| [INDEX] | [TAG]            | [REQUIRED_WHEN]                                                                                  |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | Summary          | Always, text before the first block tag                                                          |
|  [02]   | `@param name -`  | Every parameter or none, a name that differs from the parameter is an editor suggestion in `.ts` |
|  [03]   | `@typeParam T -` | Every type parameter or none                                                                     |
|  [04]   | `@returns`       | Non-void return when the summary does not open with "Returns"                                    |

Order: summary, `@remarks`, `@param` and `@typeParam`, `@returns` and `@throws`, then `@example`:

````ts
/**
 * Summary sentence
 *
 * @remarks
 * Observable behavior the summary does not state
 *
 * @param name - Meaning, unit, or constraint
 * @typeParam T - Role of the type parameter
 * @returns Meaning of the value, each failure case for an `Effect`
 * @throws {@link ErrorType}
 * Condition that raises it, boundary code only
 *
 * @example
 * ```ts
 * import { Effect } from 'effect';
 * ```
 */
````

One blank line separates summary from tags and encloses each `@remarks` and `@example` block. Biome reads `@public`, `@package`, and `@private` through `noPrivateImports` and enforces no other doc-comment rule.

| [INDEX] | [TAG]                   | [RULE]                                                                                                       |
| :-----: | :---------------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `@defaultValue`         | Block on an `interface` or `class` member, default in backticks                                              |
|  [02]   | `@throws`               | First line holds `{@link ErrorType}` alone as block title, condition on the next line                        |
|  [03]   | `@example`              | Tag line text is the title, fenced `ts` block opens with its `import` lines                                  |
|  [04]   | `@see`                  | Takes an explicit `{@link}`, plain text after `@see` stays unlinked                                          |
|  [05]   | `@deprecated`           | Replacement follows in one sentence, applies to every member of the container                                |
|  [06]   | `{@inheritDoc Target}`  | Copies summary, `@remarks`, `@param`, `@typeParam`, and `@returns`, an own summary or `@remarks` is an error |
|  [07]   | `@internal`             | Modifier on the last line, `tsc` keeps the export in `.d.ts` without `stripInternal`                         |
|  [08]   | `@packageDocumentation` | Modifier in the first `/**` comment of the entry file                                                        |

## [05]-[INTERFACES]

Members of a named `interface` or `type` take a one-line doc comment the language service shows on hover:

```ts
/**
 * Options for the retry policy
 */
interface RetryOptions {
    /** Attempts before the effect fails with the last error */
    attempts: number;
    /**
     * Delay between attempts in milliseconds
     * @defaultValue `5000`
     */
    delay?: number;
}
```

## [06]-[WORKFLOW]

Each documentation run reports:
- Scope, the symbols touched
- Edits, the exact comment blocks in context
- Skipped, the symbols left alone and the reason (trivial, unclear, private)
- Open questions, when missing intent makes the docs wrong
