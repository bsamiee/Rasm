# [REVIEW]

Review of existing XML documentation comments against their source, from severity classes to the closing report.

## [01]-[SCOPE]

Reviews compare each doc comment with the source it documents and report findings, the fix step runs when the caller approved fixes:
- Every finding cites the source `path:line` that contradicts the doc text
- Every finding keeps its severity class
- Files reviewed without their source read are incomplete

## [02]-[SEVERITY]

### [02.1]-[CRITICAL]

Fix step edits CRITICAL findings without a per-finding choice:
- Fabricated members: a doc or example names a type, member, or overload the source does not declare
- Malformed XML: an unescaped `<`, `>`, or `&`, a missing closing tag, or mismatched tag names (`CS1570`)
- Credentials, internal URLs, or personal data inside doc text

### [02.2]-[IMPORTANT]

Fix step edits IMPORTANT findings the caller selects:
- Placeholders: TODO, FIXME, TBD, or a bracketed template (`[Describe ...]`)
- Diagnostics the build reports on the comment, the `dotnet build` output is the evidence
- Phrasing that departs from the phrase for the member kind, the declaration (accessor list, `abstract`, return type) is the evidence
- Overloads with descriptions on one overload and none on another overload of the same member
- Default-value claims with no initializer in the source
- Examples that do not compile

### [02.3]-[MINOR]

Report lists MINOR findings and the fix step leaves them:
- Patterns that differ between members of the same kind
- Parameter names in text without `<paramref>`
- Trailing spaces or a space before a period

## [03]-[PROCEDURE]

1. Find the C# source for the file under review and read it before the doc comments
2. Record each documented member's constructors, overloads, accessor lists, body validation (throw, clamp, pad, truncate), constants, and initializers
3. Read each doc comment and run the factual, example, and quality checks against the record
4. Deduplicate findings by file, member, and class, keep one row at the higher severity when the build and the review report the same defect
5. When the caller approved fixes, edit the doc comments for every CRITICAL finding and for the IMPORTANT findings the caller selected

## [04]-[CHECKS]

Report rows name one class:

| [INDEX] | [CLASS]             | [MEANING]                                                                              |
| :-----: | :------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `factual`           | Stated fact contradicts the source                                                     |
|  [02]   | `fabricated-member` | Docs or an example reference a type, member, or overload that does not exist in source |
|  [03]   | `example`           | Finding from the examples check                                                        |
|  [04]   | `quality`           | Finding from the quality check                                                         |

### [04.1]-[FACTUAL]

- Constraint claims ("exactly N", "must be") hold when the method body rejects the input, a padding, truncating, or clamping body falsifies them
- Default claims need the field or property initializer read, a member with no initializer has the CLR default (`0`, `null`, `false`)
- Defaults with unread source are `UNVERIFIED`
- Data-format claims (bit layout, channel order, byte order) match the native header when one exists, the bit widths sum to the stated size

### [04.2]-[EXAMPLES]

For each `<code>` block inside `<example>`, extract each constructor call, method call, and property access:
- Member and overload exist in the C# source, the overload accepts the argument types
- No C# reserved word (`override`, `base`, `event`, `class`, `struct`) is an identifier
- Block checks a nullable return before use
- Block declares every identifier it uses
- Block disposes no object a parent owns

### [04.3]-[QUALITY]

- Type-level `<remarks>` holds facts
- Cross-type comparisons ("Unlike X, which is immutable") match the other type's source

## [05]-[REPORT]

End the review with one list:
- Files reviewed, each file reviewed without its source marked incomplete
- Findings by severity (CRITICAL, IMPORTANT, MINOR) and by class
- Fixes applied, when the fix step ran
