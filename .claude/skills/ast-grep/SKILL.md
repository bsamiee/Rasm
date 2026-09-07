---
name: ast-grep
description: Use when reading, searching, or rewriting code by its syntax tree, or deriving and integrating ast-grep rules from diffs, code, or project principles.
---

# [AST_GREP]

Covers structural code work on ast-grep: maps, searches, proofs, project rules, and text rewrites by syntax tree. The MCP tools (`find_code`, `find_code_by_rule`, `dump_syntax_tree`, `test_match_code_rule`) run every search and proof that answers with a match list or a tree, and the CLI runs what maps (`ast-grep outline`), scans project rules (`ast-grep scan`), tests (`ast-grep test`), writes (`-U`, `-i`), or needs an exit code, because a failed tool call reaches the agent as `Error executing tool` with no cause. Search rules stay inline, and durable rules are project rule files discovered through `sgconfig.yml`. Language tooling settles symbol identity, type information, and behavior that syntax alone cannot establish.

- Use `dotnet-coding` for the C# criteria a correction is judged by
- Use `dotnet-msbuild-antipatterns` for the build-file criteria a correction is judged by

[REFERENCES]:
- [01]-[MATCHING](references/matching.md): Structural search, captures, relations, and match precision
- [02]-[REWRITING](references/rewriting.md): Replacement templates, transformations, rewriters, edit ranges, and the guard stack of a fix
- [03]-[CONFIGURATION](references/configuration.md): Rule registration, utilities, file scope, parsers, execution, labels, and suppression
- [04]-[RULE_BUILDING](references/rule-building.md): Correction criteria per language and the derivation of a family from evidence
- [05]-[TYPESCRIPT_COMPOSITION](references/typescript-composition.md): TypeScript representations and execution scopes under their Effect contracts
- [06]-[RULE_HARDENING](references/rule-hardening.md): Weakness table, widening, collapse, mechanisms, and maintained sets
- [07]-[RULE_TESTING](references/rule-testing.md): Runner outcomes, case criteria, snapshots, and disproving cases
- [08]-[GRAMMAR](references/grammar.md): Node shapes and parser gaps per language
- [09]-[OUTLINE](references/outline.md): Structural maps and custom item and member extractors
- [10]-[SKILL_IMPROVEMENT](references/skill-improvement.md): Sources, sequence, and consolidation criteria for the skill's own text

[TEMPLATES]: One template per file kind, placeholders in angle brackets:
- [01]-[SGCONFIG](templates/sgconfig.template.yml): Project rule and parser configuration
- [02]-[RULE](templates/rule.yml): Lint rule with its fix
- [03]-[RULE_REWRITE](templates/rule-rewrite.yml): On-demand composed rewrite
- [04]-[UTIL](templates/util.yml): Parameterized shared predicate
- [05]-[RULE_TEST](templates/rule-test.yml): Cases bound to a rule id
- [06]-[OUTLINE_ITEM](templates/outline-item.yml): Top-level structural extractor
- [07]-[OUTLINE_MEMBER](templates/outline-member.yml): Member extractor attached to an owning item

[SCRIPTS]:
- [01]-[RULE_CHECKS](scripts/rule-checks.sh): Registration, fixture, syntax, and mutation checks over the rules tree, no argument lists its commands

## [01]-[INTENT]

The rule families exist for one purpose, and every rule, rewrite, and correction is judged against it:
- Ast-grep enforces the code standards structurally, through syntax and never through text matching
- The rules find recurring mistakes, prevent their return, and apply proven mechanical corrections
- Domain logic in every language stays pure, expression-oriented, and directly composed
- Dependent operations short-circuit, and independent operations combine and retain their errors
- Unnecessary indirection, forwarding wrappers, aliases, duplicated facts, and avoidable work leave the code
- Nesting falls through better language constructs, and never through helper extraction or a longer combinator chain
- Types, schemas, strings, and files merge when their separate existence adds no meaning, and stay apart otherwise
- The newest supported language features and the owning library's direct, documented APIs stand over their older forms
- Each language follows its own idioms: Bash process semantics, TypeScript Effect, Python validation libraries, and .NET composition
- Rules target categories of mistakes, and their tests distinguish actual violations from legitimate code
- Rewrites preserve behavior, evaluation order, failure handling, ownership, and resource lifetime
- The goal is less code and complexity through stronger implementation, and never shorter spelling or a speculative restriction

## [02]-[CORRECTIONS]

A correction is judged over the complete operation, its module declarations and its consumers included, and lands when the remaining declarations and calls mean more with less:
- Moving a branch into a helper, replacing nesting with a longer chain, or renaming a literal removes no work
- A domain type, a boundary conversion, or a composed policy stays when it adds meaning, and locally owned mutation stays when it remains pure
- Data-first calls and direct conditionals stay when they state the operation within the nesting limit
- Library defaults stay unless a dependency or execution policy requires an override
- A value states its fact at its site, and a flag states a fact the manifest lacks
- The older form and the native counterpart of a package capability are reported and rewritten where the fix re-parses
- A naive, missed, or incorrect spelling of a package member is reported with the correct call
- Types, classes, constants, helpers, and aliases have a second reader or add a domain type, a boundary conversion, or a composed policy
- The standard package (Effect, msgspec, anyio, httpx, structlog, pydantic, LanguageExt, Thinktecture) stands in its documented direct form
- Rules derive from a correction made to real code, and never from a security policy, an audit requirement, or a catalog check

## [03]-[RULES]

A rule lands from a proven correction generalized to its category, after the scope is clean under every checker of its language, and then the rule is tested:
- The before form, the after form, and the reason are stated in one line before any file is written
- An existing rule or util that overlaps the correction is extended, and no sibling rule with the same correction and reason is added
- A loose or over-reaching rule is refused, and a scan over real code counts the matches before the file lands
- Every rule carries a `fix`, and a correction that sweeps many sites is a rewrite under `rewrites/` that the rewrite target applies
- `message`, `note`, and `fix` are one concise line each with no coined term, the message states the finding and the note the correct form
- Diagnostics a linter, compiler, analyzer, or generator reports stay with that checker, and a rewrite can supply the correction it lacks
- Siblings are every form the correction applies to, near misses are the forms it does not apply to, and each becomes a case
- Lint rules under `rules/` are `severity: error`, and the scan exits nonzero and blocks

## [04]-[INTEGRATIONS]

Hosts consume the scan through its exit codes, its output formats, and the bindings, and each host takes the row for the result it needs:

| [INDEX] | [HOST]        | [FORM]                                                                                                                  |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------------------------------- |
|  [01]   | CI annotation | `ast-grep scan --format github` prints `::error file=,line=,title=<rule-id>::` per finding above `hint`, no upload      |
|  [02]   | Code scanning | `ast-grep scan --format sarif > <file>` then `github/codeql-action/upload-sarif`, `--format` excludes `--json`          |
|  [03]   | Hook          | `ast-grep scan --report-style short --color never <paths>`                                                              |
|  [04]   | Changed files | `git diff --name-only -z --diff-filter=ACMR <base>...` as NUL-delimited separate arguments, no scan on an empty list    |
|  [05]   | Pipeline      | `ast-grep scan --json=stream \| jq -c '<filter>'`, one match per line with its `ruleId`                                 |
|  [06]   | Baseline      | `ast-grep scan --filter '^<rule-id>$' --json=stream \| wc -l` against a recorded count, one rule's width over the tree  |
|  [07]   | Parse gate    | `ast-grep run -k ERROR -l <lang> --json=compact <paths>` exits 1 when every file parses                                 |
|  [08]   | Editor        | `ast-grep lsp` over the root `sgconfig.yml`: diagnostics, `labels`, a code action per `fix`, reload on any YAML change  |
|  [09]   | Model text    | `--json=stream` selects the nodes, the model returns one replacement per match, edits splice by `byteOffset`            |
|  [10]   | Library       | `@ast-grep/napi` or `ast-grep-py` when a replacement is computed, arguments take per-position checks, or files cross    |
|  [11]   | Edit scan     | `hooks/policies/scan.ts` in the function-hooks plugin runs `ast-grep scan --json=compact <file>` after an Edit or Write |
|  [12]   | Edit context  | Each hit reaches the model as `<file>:<line> <ruleId>: <note>` and writes one `scan/<id>` row the telemetry block reads |
|  [13]   | Tree edit     | `ast-grep test --include-off` runs after an edit under `tools/ast-grep/`, `--filter` on a rule, test, or snapshot id    |
|  [14]   | Tree pairing  | `rule-checks.sh pairing` runs after the same edit over the whole tree, no extension and no test run                    |

- Match objects hold `text`, `range` (`byteOffset`, zero-based `start`/`end`), `replacement`, `replacementOffsets`, and `metaVariables`
- A fix that expands the matched range applies through `replacementOffsets`
- One directory argument beats a batched file list, the walk parses in parallel and `--globs '!<glob>'` excludes inside it
- An `ERROR` search exits 1 on no matching node, incomplete syntax can still hold `MISSING` nodes, and the language parser settles acceptance

The bindings take a replacement that needs computation, per-position decisions, or coordinated edits, with the structural selection in ast-grep and the computation in the host, and they expose syntax trees and text edits and no type inference or symbol resolution:
- `@ast-grep/napi` serves a JavaScript host and `ast-grep-py` a Python host
- A language host imports its generated type map (`import type Tsx from '@ast-grep/napi/lang/Tsx'`) for `parse<Tsx>` and typed `find` results
- Each source parses once and `findAll` selects with a structural rule
- `parseAsync` serves independent sources, `findInFiles` one matcher over discovered files, and `parseFiles` selections sharing a file
- `findInFiles` and `parseFiles` resolve their file count before every callback ran, and the callbacks are awaited before the results are read
- `getMultipleMatches` returns separator tokens, and `kind() !== ','` filters them before an argument list is indexed
- `NapiConfig` has no `fix`, `replace` substitutes no metavariable, and expansion reads `getMultipleMatches` before `getMatch`
- `getTransformed(<name>)` reads a `transform` output, `namedChildren` elements without punctuation, and `fieldChildren` a repeated field
- Trees are immutable, and edits collect against one parsed source as non-nested byte ranges and commit together on its root
- An insertion is a zero-width edit, and an outer edit consumes the edits inside its range
- Identical findings deduplicate by file and range when input paths overlap, and the returned source reparses before a dependent edit pass
- A removed node's comments stay unless a moved subtree carries them
- A declaration stays while its references, shorthand properties and destructuring targets included, cannot be resolved
- `registerDynamicLanguage` runs once per process with every `@ast-grep/lang-*` package in one call, and an unregistered language fails at parse time
- `ast-grep-py` is `SgRoot(src, language)` with rules as keyword arguments (`find(pattern=<code>)`), and file discovery is the caller's
- `@ast-grep/wasm` awaits `initializeTreeSitter`, then registers each language's WASM parser through `registerDynamicLanguage`
- A host program exists while its correction needs computation or coordinated edits and leaves with it, and a CLI wrapper adds no capability

## [05]-[AGENTS]

Each agent runs one scope per pass, and the operation selects it:

| [INDEX] | [OPERATION]                            | [AGENT]                   |
| :-----: | :------------------------------------- | :------------------------ |
|  [01]   | Derive and integrate rules             | `ast-grep-rule-builder`   |
|  [02]   | Widen, collapse, and fix existing rules | `ast-grep-rule-hardener`  |
|  [03]   | Disprove rules with cases              | `ast-grep-rule-tester`    |
|  [04]   | Build and refine structural outlines   | `ast-grep-outline-builder` |
|  [05]   | Improve the skill's own text           | `ast-grep-skill-improver` |
