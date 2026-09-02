# [REVIEW]

Covers the review of existing XML documentation: the severity taxonomy the findings classify against, the source-first procedure, the factual, example, and quality checks, and the report that ends a review.

## [01]-[SCOPE]

Audit existing docs for accuracy, freshness, examples, and hygiene — by scope. Review improves what is already filled. Review is report-only by default; fixing is a separate, gated step.

AI doc writers confidently state "facts" without checking source — wrong parameter constraints, wrong channel names, wrong byte layouts, invented overloads, wrong defaults — and frequently write examples that call undeclared variables. Assume errors exist. A review that finds 0 issues across many files almost certainly skimmed.

- Cite a source `path:line` for every factual contradiction. No citation → not a finding.
- Never invent a finding; never downgrade a CRITICAL to make a report look cleaner.

## [02]-[SEVERITY]

Based on the official .NET API documentation guidelines. Classify issues by severity when reviewing documentation.

### [02.1]-[CRITICAL]

Issues that damage credibility or break functionality (must fix):
- Fabricated APIs — code examples that reference methods, overloads, or types that don't exist
- Spelling errors in public-facing text (teh, recieve, seperate, occured, paramter, retreive, initalize) and domain-word misuse
- Repeated words ("the the", "a a", "an an")
- Offensive or inappropriate content - vulgar language, problematic terminology (master/slave, blacklist/whitelist), dismissive language (stupid, dumb, hack)
- Malformed XML - unescaped `<`, `>`, `&`; missing closing tags; mismatched tag names
- Security-sensitive information - credentials, internal URLs, PII

### [02.2]-[IMPORTANT]

Issues that violate standards or leave gaps (should fix):
- Placeholders remaining - TODO, FIXME, TBD, bracketed remarks scaffolds like `[Describe …]`
- Empty tags - `<value />`, `<summary />`, `<returns />`, `<remarks />`, or `<summary></summary>` with only whitespace, `RCS1228` fails the build on them
- .NET guideline violations of the summary, constructor, property, method, parameter, and return-value phrasing rules
- Invalid cref references - a target the compiler cannot resolve (`CS1574`)
- Documented members without a `<summary>`
- Incomplete overloads - params filled on one overload but empty on another overload of the same method
- Wrong default-value claims — stating "the default is X" for a struct property that has no field initializer. A "typical" constant exposed elsewhere is NOT the struct's default and must be documented separately.
- Examples that won't compile

### [02.3]-[MINOR]

Style improvements that enhance quality (nice to have):
- Inconsistent patterns between similar APIs
- Summaries that could be more descriptive
- Missing `<paramref>` when referring to parameters
- Whitespace issues (trailing spaces, space before period)
- Missing `<remarks>` that would add value

## [03]-[PROCEDURE]

1. Review each file (source first). From the filename, find and READ the C# source BEFORE reading the docs and build a fact sheet: constructors, overloads, accessor kind per property (`{ get; }` vs `{ get; set; }`), validation logic (throws/clamps/pads/truncates), numeric constants, defaults. Then read each doc comment and run the checks. A file reviewed with no source read is incomplete.
2. Collect and dedupe findings. Deduplicate by `(file, docId, class)` plus fuzzy message match; when the linter and your own review report the same defect, keep one row. On a severity disagreement, take the highest.
3. (Gated) Fix. If fixing is approved, edit the docs directly for CRITICAL (and chosen IMPORTANT) findings, and expand examples where types are example-poor.

## [04]-[CHECKS]

Each finding takes one class:

| [INDEX] | [CLASS]             | [MEANING]                                                                              |
| :-----: | :------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `factual`           | Stated fact contradicts source                                                         |
|  [02]   | `fabricated-member` | Docs or an example reference a type, member, or overload that does not exist in source |
|  [03]   | `example`           | Finding from the examples check                                                        |
|  [04]   | `quality`           | Finding from the quality check                                                         |

### [04.1]-[FACTUAL]

Claims vs source (cite `path:line`):
- Parameter constraints ("exactly N", "must be", "cannot be"): does the method body actually validate/reject, or silently accept/pad/truncate? Read the body, not just the signature.
- Defaults: only assert a documented default is wrong when you have read the source and seen the field initializer — or confirmed there is none (a struct/auto-property field with no initializer is `0`/`null`/`false`). If you cannot read the source, do not assume `0` — a stated default you can't verify is `UNVERIFIED`, not a finding. Inventing "should be 0" for a default you never checked in source is a false positive.
- Data-format claims (bit layouts, channel order, byte order): verify against the native header if present; check the bit math adds up.

### [04.2]-[EXAMPLES]

For every ```` ```csharp ```` block in `<remarks>`, extract each constructor call, method call, and property access, and:
- Confirm it exists in the C# source with that exact signature/overload, and that the overload accepts those argument types.
- Flag C# reserved words used as identifiers (`override`, `base`, `event`, `class`, `struct`, …).
- Null safety: if a call returns nullable, is null handled before use?
- Self-contained: every identifier referenced must be declared in the snippet (using `bitmap2` when only `bitmap` was declared is a compile error).
- Ownership: never `using`/`Dispose` a parent-owned object.

### [04.3]-[QUALITY]

.NET conventions, completeness, style:
- Type-level `<remarks>` has real content, not a template blank.
- Remarks make no false cross-type comparisons ("Unlike X, which is immutable…" — verify first).

## [05]-[REPORT]

End reviews with:
1. Files reviewed count
2. Issues by severity (Critical: N, Important: N, Minor: N)
3. Assessment: Ready for release / Needs fixes / Major issues
