# [RULE_BUILDING]

Derive each rule from one correction and the contracts deciding where the correction applies.

## [01]-[EVIDENCE]

Read original code and its consumers before selecting a matcher:
1. Before and after forms with the original diff, or conforming and violating forms of a stated policy
2. Language semantics, configured grammar, resolved package declarations, and implementation deciding the correction
3. Callers and internal APIs establishing intended behavior and supported forms
4. Configured checkers establishing which tool owns the violation

## [02]-[APPLICABILITY]

Diagnostics require a violation established by syntax and its contract. Fixes implement the stated correction and preserve unrelated behavior.

- State the violated contract, required change, deciding preconditions, and conforming near misses
- Use semantic tooling for type resolution, control and data flow, constant propagation, and ownership or lifetime claims
- Nullable and optional types state representable values, not the role of an error field
- Repeated text establishes spelling alone; declarations, native decoding and normalization, or explicit policy establish meaning
- Unused bound values do not prove independent operations; earlier success can establish a later precondition
- Policy rules name the restriction without claiming the supported API or syntax is defective
- Recurrence follows the accepted grammar or API contract, occurrence counts alone decide no rule's validity
- Keep facts in their owning declaration, rules enforce consumers' required use of that owner

Use `manage-repo` for checker integration.

## [03]-[CONSTRUCTION]

Choose a matched construct that identifies the violation and contains the required edit. Related syntax supplies evidence without enlarging that construct.

- Report a shared declaration once when one edit corrects every dependent use
- Report uses separately when each requires an independent correction
- Select whole statements for statement removal, expression matches keep enclosing statement syntax
- Labels pinpoint captured causes without changing match or replacement ranges
- Parse patterns and targets with the configured grammar, explicit context selects ambiguous fragments
- Match modifier and wrapper differences that change the correction's applicability
- Count control flow within its callable owner; a nested callable starts its own depth
- `finally` alone establishes neither a handler nor error recovery

- Use [rule-hardening.md](rule-hardening.md) for correction families and utility composition
- Use [performance.md](performance.md) for matcher cost and scan comparisons

## [04]-[REWRITES]

Replacement preconditions follow the language and owning API contracts:
- Preserve evaluation count, order, timing, short-circuiting, and argument-binding behavior outside the intended correction
- Preserve types, null behavior, collation, and numeric association when refactoring expressions
- Preserve substitution precedence, literal contents, delimiters, and comments owned by edited syntax
- Use native narrow matches or range edits, omit unsafe fixes or separate fixed forms from other violations
- Messages name the violation, notes state the required form and deciding contract
