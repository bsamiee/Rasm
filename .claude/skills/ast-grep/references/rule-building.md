# [RULE_BUILDING]

Derive rules from a refactor diff, existing code, or a project principle, and integrate each with the existing rules and utilities.

## [01]-[SOURCES]

Read the supplied evidence, then the dependencies deciding the correction:
1. Git diff with its before and after context, the named code, or the stated principle with one conforming and one violating form
2. Language skills, manifests, resolved versions, and installed sources of the packages the correction uses
3. Exported functions and types of the internal packages the scope depends on
4. Scoped checker rules (ruff, biome, analyzers)
5. Surrounding logic and callers, enough to establish the correction's behavior and scope

## [02]-[SMELLS]

Structural search locates candidates, language and package contracts judge the correction. Matching shapes alone prove no redundant type, repeated effect, unused declaration, or interchangeable library call.

- Repeated string spelling proves no shared domain fact
- Library families derive from installed exports and overloads, positional arguments in documented order and keyword arguments by name
- Every branch's return is known before a fold becomes map or bind
- Package operations import from their documented submodule

Before searching a category, read the scope's linter and analyzer rules. Categories a package member decides need the package source beside the code, the search finds call sites once the member is named.

## [03]-[FIX]

Rewrites edit semantically verified selections alone, unverified forms violating the rule stay findings, an invalid rule is deleted unweakened:
- Count matches before and after under the same rule and paths over the whole affected scope
- Counts locate unnecessary structure and justify no deletion of a domain invariant or hiding of complexity in another file
- Resolve warnings from scoped checkers before deriving a rule

## [04]-[BAR]

A correction earns a rule by one criterion and is refused by one, a correction under the bar ends as findings:
- Rules enforce a newest-language form a configured checker lacks
- Rules report a bad logic pattern, an indirection layer, or a hand-written counterpart of an installed library's operation
- Patterns with one instance and no sibling are findings, width alone earns no rule
- Rules restating a repository fact are refused, the manifest or owner file states it once
- Rules banning a form tree-sitter parses wrong are refused, the enclosing statement pattern matches the recovered form
- Rules cataloging one tool's option grammar are refused
- Rules duplicating a configured checker are refused, a project condition or a mechanical correction the checker lacks earns one

## [05]-[DERIVATION]

Findings group by correction and reason, instances with both in common become siblings under one rule:
- Shared shapes split when corrections differ, or when different reasons change which near misses are valid
- Diff supplies instances, language and package contracts decide the rest of the family
- Siblings enumerate per module function, exporting module, overload, container kind, spelling, position, and callback form
- Siblings are real when the after form, written once per sibling, is the same
- Criterion derives from the diff's forms, package overloads, and near misses
- Fixed predicates with one caller stay in the rule, repeated predicates share a utility refined at each caller
- Default-parameter corrections preserve evaluation timing and argument-binding errors, repeated positional, keyword, and unpacked values included
- Guards over a position drop when the shape fixes the position
- Returns in both arms make every later statement dead
- Notes state every operation a fix selects by shape, the map and the bind of one match
- Absence rules derive from an operation returning Option and reconstructing its result by hand
- Direct boolean conditionals over plain values stay, a match combinator adds value when it removes existing structure
