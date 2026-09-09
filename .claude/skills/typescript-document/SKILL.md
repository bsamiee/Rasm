---
name: typescript-document
description: "Use when writing or reviewing TSDoc on an exported TypeScript declaration, or deciding whether a signature and name already document a symbol."
---

# [TYPESCRIPT_DOCUMENT]

Covers TSDoc comments the TypeScript language service shows on hover and `tsc --build` copies into the `.d.ts` output, in the tag order Effect source uses and `@effect/docgen` parses.

[REFERENCES]:
- [01]-[REACT](references/react.md): Props interfaces, the component function, and the `Result` a hook returns

## [01]-[PRINCIPLES]

- `interface`, `type`, and signatures are the primary documentation. A comment adds the business rule, edge case, side effect, unit, or performance bound the type cannot state
- TSDoc syntax: `@param name - description` with a hyphen and no `{type}`, `@typeParam T - description` for a type parameter, `@remarks` for text longer than the summary
- A documentation run changes comments alone. The one refactor is extracting a named type when an inline object type blocks per-member doc comments

## [02]-[SCOPE]

Exported declarations (types, interfaces, enums, functions, constants) and internals with a contract the code does not show take a comment. Trivial getters, one-line helpers, test helpers, and anonymous components take none:

| [INDEX] | [SITUATION]                                                   | [ACTION]                                                       |
| :-----: | :------------------------------------------------------------ | :------------------------------------------------------------- |
|  [01]   | Signature and name state the whole behavior                   | No comment                                                     |
|  [02]   | Contract, unit, default, or side effect the type cannot state | Document                                                       |
|  [03]   | Boundary code throws an exception that escapes                | One `@throws` block per exception type                         |
|  [04]   | Returns `Effect.Effect<A, E, R>`, `Either`, or `Option`       | `@returns` names each `E` case and what `None` or `Left` means |
|  [05]   | Caller needs copy-paste usage                                 | `@example`                                                     |
|  [06]   | Inline object type with members that need doc comments        | Named `interface` or `type` with a doc comment per member      |

The `no-throw-outside-try` rule reports every `throw` in `libs/typescript` outside `Effect.try` and `Effect.tryPromise`. `no-nullable-return` reports `return null`, a `null` arrow body, and `undefined` returned under a value-type annotation. An unannotated `return undefined` is Biome's `noUselessUndefined`. A domain function fails through `E` and returns absence as `Option`. `@throws` applies to the boundary code that throws.

## [03]-[STYLE]

- Summary and every description (`@param`, `@returns`, `@remarks`, `@defaultValue`): one sentence in the third person that states what the declaration does or returns ("Runs", "Returns"), a noun phrase for a type, no trailing period. The signature is the type's statement, the sentence adds to it
- `@remarks`: observable behavior with one fact per sentence. A business rule appears when the code cannot show it and the author has its source

## [04]-[STRUCTURE]

| [INDEX] | [TAG]            | [REQUIRED_WHEN]                                                                                  |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | Summary          | Always, the text before the first block tag                                                      |
|  [02]   | `@param name -`  | Every parameter or none, a name that differs from the parameter is an editor suggestion in `.ts` |
|  [03]   | `@typeParam T -` | Every type parameter or none                                                                     |
|  [04]   | `@returns`       | Non-void return when the summary does not open with "Returns"                                    |
|  [05]   | `@since`         | Every export `@effect/docgen` parses, `enforceVersion` defaults to true                          |

Order: summary, `@remarks`, `@param` and `@typeParam`, `@returns` and `@throws`, `@example`, then `@category` and `@since` last:

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
 *
 * @category constructors
 * @since 0.5.0
 */
````

One blank line separates the summary from the tags and encloses each `@remarks` and `@example` block, TSDoc reads a block up to the next block or modifier tag with or without the blank line. Biome enforces no doc-comment rule, `noPrivateImports` reads `@public`, `@package`, and `@private`.

| [INDEX] | [TAG]                   | [RULE]                                                                                                 |
| :-----: | :---------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `@defaultValue`         | Block on an `interface` or `class` member, default in backticks                                        |
|  [02]   | `@throws`               | First line holds `{@link ErrorType}` alone as the block title, the condition follows on the next line  |
|  [03]   | `@example`              | Tag-line text is the title, the fenced `ts` block opens with its `import` lines, docgen type-checks it |
|  [04]   | `@see`                  | Takes an explicit `{@link}`, plain text after `@see` is not linked                                     |
|  [05]   | `@deprecated`           | Followed by the replacement in one sentence, applies to every member of the container                  |
|  [06]   | `{@inheritDoc Target}`  | Copies summary, `@remarks`, `@param`, `@typeParam`, and `@returns`, and forbids an own summary          |
|  [07]   | `@internal`             | Modifier on the last line, docgen omits the export, `tsc` keeps it in `.d.ts` without `stripInternal`  |
|  [08]   | `@packageDocumentation` | Modifier in the first `/**` comment of the entry file                                                  |
|  [09]   | `@category`             | Groups the export in docgen output, the default group is `utils`                                       |

## [05]-[INTERFACES]

Each member of a named `interface` or `type` takes a one-line doc comment, the language service shows it on hover for both forms:

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
1. Scope, the symbols touched
2. Edits, the exact comment blocks in context
3. Skipped, the symbols left alone and the reason (trivial, unclear, private)
4. Open questions, when missing intent makes the docs wrong

A comment states what the code proves. A summary states the rule the signature cannot:

```ts
/** Adds two amounts in minor currency units (cents) */
```
