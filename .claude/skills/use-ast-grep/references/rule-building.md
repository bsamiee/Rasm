# [RULE_BUILDING]

Derive rules from a refactor diff, existing code, or a project principle, and integrate each with the existing rules and utilities.

## [01]-[SOURCES]

Read the supplied evidence, then the dependencies deciding the correction:
1. Git diff with its before and after context, the named code, or the stated principle with one conforming and one violating form
2. Language skills, project files, resolved versions, and installed sources of the packages the correction uses
3. Exported functions and types of the internal packages the scope depends on
4. Scoped checker rules (ruff, biome, analyzers)
5. Surrounding logic and callers, enough to establish the correction's behavior and scope

## [02]-[SMELLS]

Structural search locates candidates, language and package contracts judge the correction. Matching shapes alone establish no redundant type, repeated effect, unused declaration, or interchangeable library call.

- Repeated string spelling establishes no shared domain fact
- Library families derive from installed exports and overloads, positional arguments in documented order and keyword arguments by name
- Every branch's return is known before a fold becomes map or bind
- Package operations import from their documented submodule

Categories a package member decides need package source beside code, searches find call sites once the member is named.

## [03]-[FIX]

Rewrites edit only selections where the fix keeps behavior, other forms violating the rule stay findings, an invalid rule is deleted unweakened:
- `ast-grep scan --no-ignore hidden --filter '^(<ids>)$' --globs '<globs>' --json=stream . > <state>.jsonl` records the before or after state
- `<ids>` joins with `|` the id of each rule the round edits, places, or collapses and of each caller of a util it edits
- `jq -r '[.ruleId, .file, (.range.start.line + 1 | tostring)] | join(":")' <state>.jsonl | sort > <state>.keys` writes each state's keys
- Rows grouped by `ruleId` count each rule, `comm -3` over both `<state>.keys` files lists gained and lost rows
- Counts locate unnecessary structure and justify no deletion of a domain invariant or hiding of complexity in another file
- Resolve warnings from scoped checkers before deriving a rule

`<globs>` is the row of the round's language, injection hosts included:

| [INDEX] | [LANGUAGE]   | [GLOBS]                                                  |
| :-----: | :----------- | :------------------------------------------------------- |
|  [01]   | `bash`       | `{*.sh,*.yml,*.yaml}`                                    |
|  [02]   | `csharp`     | `*.cs`                                                   |
|  [03]   | `java`       | `*.java`                                                 |
|  [04]   | `javascript` | `*.{js,jsx,mjs,cjs}`                                     |
|  [05]   | `python`     | `*.py`                                                   |
|  [06]   | `sql`        | `{*.sql,*.ts,*.tsx}`                                     |
|  [07]   | `swift`      | `*.swift`                                                |
|  [08]   | `tsx`        | `*.{ts,tsx}`                                             |
|  [09]   | `xml`        | `{*.xml,*.csproj,*.props,*.targets,*.slnx,NuGet.config}` |
|  [10]   | `yaml`       | `*.{yml,yaml}`                                           |

## [04]-[BAR]

Corrections earn a rule by one criterion and are refused by one, each a verdict token with `earns` 1 or 0, a correction under the bar ends as findings:

| [INDEX] | [VERDICT]         | [EARNS] | [CRITERION]                                                               |
| :-----: | :---------------- | :-----: | :------------------------------------------------------------------------ |
|  [01]   | `newest-form`     |    1    | Newest-language form a configured checker lacks                           |
|  [02]   | `bad-logic`       |    1    | Bad logic pattern                                                         |
|  [03]   | `indirection`     |    1    | Indirection layer                                                         |
|  [04]   | `library-op`      |    1    | Hand-written counterpart of an installed library's operation              |
|  [05]   | `repository-fact` |    0    | Repository fact, the project file or owner file states it once            |
|  [06]   | `misparse`        |    0    | Form tree-sitter parses wrong, matched by the enclosing statement pattern |
|  [07]   | `option-grammar`  |    0    | One tool's option grammar                                                 |
|  [08]   | `accepted-form`   |    0    | Accepted form, rewrite adds members, lines, or changes behavior           |

- Patterns with one instance and no sibling are findings, width alone earns no rule
- Rules duplicating a configured checker are refused, a project condition or a mechanical correction the checker lacks earns one

## [05]-[DERIVATION]

Findings group by correction and reason, instances with both in common become siblings under one rule:
- Shared shapes split when corrections differ, or when different reasons change which near misses are valid
- Diff supplies instances, language and package contracts decide the rest of the family
- Siblings enumerate per module function, exporting module, overload, container kind, spelling, position, and callback form
- Siblings are real when the after form, written once per sibling, is the same
- Siblings include each form that silences the rule and keeps its cause (relayed value, restated type, rename, wrapper)
- Criterion derives from the diff's forms, package overloads, and near misses
- Fixed predicates with one caller stay in the rule, repeated predicates share a utility refined at each caller
- Default-parameter corrections preserve evaluation timing and argument-binding errors, repeated positional, keyword, and unpacked values included
- Guards over a position drop when the shape fixes the position
- Returns in both arms make every later statement dead
- Notes state every operation a fix selects by shape, the map and the bind of one match
- Absence rules derive from an operation returning Option and reconstructing its result by hand
- Direct boolean conditionals over plain values stay, a match combinator adds value when it removes existing structure
