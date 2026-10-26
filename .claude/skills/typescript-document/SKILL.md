---
name: typescript-document
description: "Use when writing or reviewing TSDoc on TypeScript declarations or React components, covering tag syntax"
disable-model-invocation: true
---

# [TYPESCRIPT_DOCUMENT]

Covers TSDoc comments the TypeScript language service shows on hover.

[REFERENCES]:
- [01]-[REACT](references/react.md): React component and hook documentation

## [01]-[PRINCIPLES]

- `interface`, `type`, and signatures are the primary documentation
- Comments add contracts, business rules, side effects, units, defaults, and performance bounds the type cannot state
- `@param` and `@typeParam` descriptions follow the name and a hyphen, with no `{type}`
- Documentation runs change comments alone

## [02]-[STYLE]

- Function summaries open with a third-person verb (`Runs`), type and member summaries are noun phrases
- Summaries and tag descriptions end without a period
- `@remarks` holds observable behavior, and a business rule the code cannot show when the author has its source

Summary with the unit the signature cannot state:

```ts
/** Adds two amounts in minor currency units (cents) */
```

## [03]-[STRUCTURE]

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

## [04]-[WORKFLOW]

Runs cover declarations in the invocation paths ($ARGUMENTS) and report:
- Scope, the symbols touched
- Edits, the exact comment blocks in context
- Open questions, when missing intent makes the docs wrong
