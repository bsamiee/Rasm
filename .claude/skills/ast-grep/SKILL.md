---
name: ast-grep
description: "Use when listing a file's declarations, reading, searching, or rewriting code by syntax tree, or deriving ast-grep rules from diffs, code, or principles."
---

# [AST_GREP]

MCP tools (`find_code`, `find_code_by_rule`, `dump_syntax_tree`, `test_match_code_rule`) answer searches and proofs with a match list or a tree. CLI runs `ast-grep outline`, `ast-grep scan`, writes (`-U`, `-i`), and runs that need an exit code. Search rules stay inline, durable rules are files under `ruleDirs` of `sgconfig.yml`. Language tooling resolves symbol identity, types, and behavior beyond syntax.

[REFERENCES]:
- [01]-[RULE_BUILDING](references/rule-building.md): Deriving rules from diffs, code smells, and principles
- [02]-[RULE_HARDENING](references/rule-hardening.md): Widening, collapsing, and fixing existing rules
- [03]-[OUTLINE](references/outline.md): `ast-grep outline` usage and extractor construction

[TEMPLATES]:
- [01]-[RULE](templates/rule.yml): Lint rule

## [01]-[MATCHING]

Patterns are valid code under the language's tree-sitter grammar with whole-node metavariables:
- `$VAR` one named node, `$$VAR` one named or unnamed node (operators and keywords included)
- `$$$MULTI` lazy zero-or-more without backtracking, `$_` and `$_NAME` non-capturing
- A wrong-kind pattern takes `pattern: { context: <full-code>, selector: <kind> }`
- `$$$` before a node ends at the first fitting sibling, `f($$$H, { $$$P }, $$$T)` matches `f(x, { a }, { b })`
- `smart` and `cst` require the comma before `$$$`, `f($A, $$$R)` matches `f(x,)` and misses `f(x)`, `ast` and below bind `R` empty on `f(x)`
- `strictness` per value: `cst` skips nothing, `smart` skips unnamed target nodes and comments, `ast` skips unnamed nodes on both sides
- `relaxed` skips unnamed nodes on both sides and comments, `signature` matches leaves by kind alone and keeps capture identity
- `template` matches text alone with kinds ignored and needs a `kind` beside it
- `regex` is a Rust regex with no look-around or back-reference, `|` inside a name escapes, `(?i)` sets flags inline
- `(?x)` in a `|-` block scalar holds one alternative per line with `\ ` for a space inside a phrase, `(?-x:...)` keeps a phrase verbatim
- `dump_syntax_tree` with `format=cst` prints one top-level node's kinds, fields, and tokens, `format=pattern` how a pattern parsed
- Multi-statement snippets take `ast-grep run -l <lang> -p '<code>' --debug-query=cst`, the tree prints on stderr before exit 8

## [02]-[RULES]

- Rule objects are unordered `all`s with keys applied atomic, composite, then relational, `all:` keeps list order
- Rules need a kind set from `pattern` or `kind`, `regex`, `range`, `nthChild`, `not`, `all: []`, or `any: []` alone fails the load
- `kind` takes a named node, an anonymous token (`then`) fails the load with `Cannot parse rule`, exit 8
- `not` guards over a capture sit after its binding clause in `all:`, the first clause naming `$VAR` binds it
- Rules test one node, `has: {all: [<a>, <b>]}` demands one child with both shapes, one `has` per required child
- Repeated `not:` or `has:` keys in one map fail the load as `duplicate field <key>`, `not: {any: [<guards>]}` or an `all:` holds both
- `kind` and `pattern` restrict candidate nodes by kind, `regex` restricts none, `matches: <param>` prunes beside a `kind` alone
- `has` and `inside` at `stopBy: end` walk the whole subtree or ancestor chain per candidate, `field` narrows `has` to one child first
- Kindless `any:` arms widen the kind set to every kind, `all:` narrows it to the intersection of its items
- One `$VAR` across clauses proves sameness, `not:` on a rebound pattern proves difference, `$_VAR` skips both
- `has` binds `$VAR` to the earliest matching child, a later clause rejecting that child fails the rule, the narrower `has` precedes the wider
- Captures bound on one node re-match their text inside a later `not: {has: ...}` or `not: {inside: ...}`, the form for a fact every sibling repeats
- `nthChild` counts named siblings, counts include comments the block holds, `ofRule: {not: {kind: comment}}` counts semantic slots
- `stopBy` default neighbor serves direct relations, `end` the whole axis, a rule a bounded walk including the stop node
- `stopBy: {kind: <function-kind>}` on `has` keeps an inner function's `return` from satisfying the outer owner
- `field:` binding names the final relation and survives `stopBy: end` (callee vs argument, key vs value)
- Unnamed token fields (`operator`) take `has: {field: <role>, pattern: $$_X}`
- `has: {field: <role>, kind: <kind>}` tests the first child with the field alone, a repeated field (`argument`) takes `nthChild` or a fieldless `has`
- Relational objects hold a rule key beside `field` and `stopBy`, `has: {field: <role>}` alone fails the load
- Default `follows` reads the adjacent sibling alone, a comment between statements breaks it until `stopBy: end`
- `has` visits unnamed children, restrict named children with `not: {has: {pattern: $_, not: <allowed>}}`
- `constraints:` needs no kind set, negative and relational capture guards belong under it
- Metavariables bound in `all`, relational rules, and the matching `any:` arm export to `fix`, `message`, and `transform`
- Failed `any:` arms bind nothing, the next arm binds the same name again
- `any:` keeps the first structurally matching arm with no `constraints` retry, the wrapped pattern precedes the unwrapped one

## [03]-[UTILS]

- Global utils are named `<package>-<shape>` and hold `id`, `language`, `arguments`, `rule`, `constraints`, `utils`, and `transform`, no `fix`
- Global util ids share one namespace across languages, one id in two languages fails the load as `Duplicate rule id`
- Consumers name the util directory under `utilDirs`, `scan -r`, `--inline-rules`, and the MCP load none
- Utils under `not:` or `inside:` supply no kind to their caller, a kindless caller aborts with `Rule must specify a set of AST kinds to match`
- Parameterized utils take `arguments` at the global level alone, arguments are mandatory, a string `matches: <id>` of a parameterized util exits 8
- Rules call a parameterized util as `matches: {<util-id>: {<arg>: <rule>}}`, each argument a rule, calls under one `matches` combine as `all`
- Parameterized calls under `has`, `inside`, or `ofRule` in a util record no load-order edge and fail random loads as `Rule <id> is not defined`
- String `matches: <name>` resolves a parameter, then a local util, then a global util
- Global utils called under `constraints` alone need no kind set
- Local util captures reach the caller's `fix`, global util captures stay private
- Captures an argument rule binds reach the caller's `fix`, `message`, and `labels`

## [04]-[FIX]

`fix` replaces a proven match with a template, `transform` derives text, and `rewriters` change structure inside captures.
- Templates are unparsed text, captures substitute anywhere in them without a syntax or precedence check
- Unfixable variants (guards, discards, exports, valueless members) are `not:` arms before the template
- Undefined metavariables fail the rule load under `scan` and substitute empty under `run -r`
- Statement templates include their `;`, a template without the terminator drops it
- Multiline templates indent relative to the matched column and keep the relative indentation of substituted captures
- Outputs are named without `$` and read their source with `$`
- Dependent transforms take an earlier output as source and end in one `fix: $RESULT`
- `NEW: replace($OLD, replace=<regex>, by=<text>)` replaces text
- `NEW: substring($OLD, startChar=1, endChar=-1)` slices by character, negative indices count from the end
- `NEW: convert($OLD, toCase=snakeCase, separatedBy=[underscore])` converts case over the named separators
- `NEW: rewrite($OLD, rewriters=[<id>])` rewrites nodes
- Object forms (`replace: {source: $OLD, replace: <regex>, by: <text>}`) hold a regex or replacement with commas or quotes the string form splits
- `replace` reads Rust regex captures in its `by` field as `$1` or `${NAME}`, with `$$` for a literal dollar
- Templates read every `$NAME` as a metavariable and fail the load on an unbound one, a literal `$NAME` comes from a `replace` with `by: $$NAME`
- Rewriters sit in the document `rewriters` list, `rewrite()` selects them by id
- Each rewriter holds `id`, `rule`, and `fix`, with optional `constraints`, `transform`, and `utils`
- Traversal covers the captured root and its descendants, each node of a `$$$` capture included
- Rewriters run in listed order at each node, the first match replaces its subtree and blocks descendant matches in that pass
- `joinBy` omitted keeps unmatched text, separators and comments between rewritten nodes included, `joinBy` set joins the replacements alone
- Rewriters read captured syntax trees, enclosing rule captures, and enclosing local utilities, no transformed string or transform output
- `fix: {template, expandStart, expandEnd}` widens the replaced range to the sibling the sub-rule matches, a miss keeps the matched range
- Expansion stops at the first sibling the sub-rule matches, the adjacent sibling alone by default and any sibling under `stopBy: end`
- `fix` as a list of `{title, template}` offers alternatives, `-U` and `--json` take the first and `-i` offers each
- Overlapping fixes never compose, a `program`-level fix blocks every other fix

## [05]-[CONFIG]

- `ruleDirs` and `utilDirs` resolve relative to `sgconfig.yml`
- `languageGlobs.<language>` maps file globs to a language ahead of extension detection
- `customLanguages.<name>` holds `libraryPath` and `extensions`, with optional `expandoChar`, `languageSymbol`, and `outlineRules`
- `files:` and `ignores:` globs match paths relative to the config directory with no `./` prefix, `ignores:` reads first
- `scan -r` reads `files:` from the rule file
- Wildcard globs take an implied `**/` prefix, a plain file name matches the one file beside `sgconfig.yml`, `**/<name>` every file of that name
- Dot directories under a walked path need `--no-ignore hidden`, a dot path named on the command needs no flag
- Injection entries capture the embedded source as `$CONTENT` and name the parser in `injected`, a language or a candidate list with `$LANG`
- Injections parse source ranges without decoding host strings, plain and literal block YAML scalars and JSON strings with no `escape_sequence`
- Injection entries hold ownership predicates in their `utils` map beside `rule` and `injected`, injection compilation loads no `utilDirs`
- Kind lists inside a flow map are quoted (`{kind: 'block_mapping_pair, flow_pair'}`), an unquoted second kind reads as a key
- Under `expandoChar`, patterns spell metavariables with that character (`_VAR`, `___VAR`), fix templates and transform sources use `$`
- Unparsable rule files of any language and duplicate ids fail every load of the root config, an unknown key in `sgconfig.yml` loads silently
- `ast-grep-ignore` on the line before or on the line of the match suppresses every rule, `ast-grep-ignore: <id>, <id>` the listed rules alone
- `unused-suppression` reports a comment suppressing nothing at `hint`, `--filter`, `--off`, `--min-severity`, `-r`, or `--inline-rules` turns it off
- `no-suppress-all` reports an idless comment and is off, `--error=<id>` raises either built-in rule

## [06]-[GRAMMAR]

Tree-sitter recovers with `ERROR` or zero-width `MISSING` nodes, and its precedence can differ from the language parser's:
- `ERROR` constructs are unenforceable, the enclosing statement pattern matches them
- `kind: ERROR` and `ast-grep run -k ERROR -l <lang>` find the recovered form

| [INDEX] | [PYTHON]                         | [SHAPE]                                                                                               |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `except A, B:`                   | One `except_clause` with two `value` fields, trailing comma included                                  |
|  [02]   | `from __future__ import a, b`    | `future_import_statement` with one `name: dotted_name` per name                                       |
|  [03]   | `def f(x: int)`                  | `typed_parameter` with the name as its unfielded first `identifier` and `type` as the field           |
|  [04]   | `(v := a if b else c)`           | Tree-sitter nests `named_expression` in the first arm, Python binds the whole conditional             |
|  [05]   | `[i async for i in g]`           | `for_in_clause` with `async` as an unnamed child                                                      |
|  [06]   | `match x:` with `case C() if g:` | `body: block` over `alternative: case_clause`, fields `guard: if_clause` and `consequence: block`     |
|  [07]   | `case Result()`                  | `class_pattern` with no `case_pattern` child                                                          |
|  [08]   | `Optional[X]`                    | `type` > `generic_type` > `type_parameter` in an annotation, `subscript` in a value position          |
|  [09]   | `[*items for items in groups]`   | `list_comprehension` with a `body: list_splat`, set and generator forms with the same `body`          |

Fixes emit walrus conditionals as `(v := (a if b else c))`, `ast.parse` checks the precedence.

| [INDEX] | [BASH]                          | [SHAPE]                                                                                                |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `[ x = y ]`, `[[ x = y ]]`      | `test_command` with the bracket unnamed over a `binary_expression` with `left`, `operator`, `right`    |
|  [02]   | `$(cmd)`, backticks             | `command_substitution` with the delimiter unnamed and one `command` child per simple command           |
|  [03]   | `echo $x`, `echo "$x"`          | `simple_expansion` under `command` when unquoted and under `string` when double-quoted                 |
|  [04]   | `$1`, `${10}`, `$@`, `$?`       | `variable_name` under `simple_expansion` or `expansion`, `special_variable_name` for `$@`, `$#`, `$?`  |
|  [05]   | `${x#*/}`, `${x:-d}`            | `expansion` with `variable_name` and an `operator` child, the pattern a `regex` or `word` child        |
|  [06]   | `${x//a/$r}`                    | `expansion` with two `operator` children, the replacement follows the second `/`                       |
|  [07]   | `x=${ cmd; }`, `${\| cmd; }`    | `expansion` with an `ERROR` child at the `;`, the grammar has no node for the no-fork substitution     |
|  [08]   | `cmd <<<"$x"`                   | `command` with `redirect: herestring_redirect`, the last stage's after a pipeline                      |
|  [09]   | `cmd > f`, `done < <(p)`        | `redirected_statement` with `body` and `redirect: file_redirect` wrapping the whole list or pipeline   |
|  [10]   | `for f in $(ls); do`            | `for_statement` with `variable`, `value: command_substitution`, and `body: do_group`                   |
|  [11]   | `(( x += 1 ))`, `(( x++ ))`     | `compound_statement` over a `binary_expression`, a `postfix_expression` for `x++`                      |
|  [12]   | `local -n r=$1`, `declare -A m` | `declaration_command` with the keyword, an option `word`, and `variable_name` or `variable_assignment` |
|  [13]   | `f() { ...; }`                  | `function_definition` with `name: word` and `body: compound_statement`, `function` spelling included   |
|  [14]   | `if c; then a; else b; fi`      | `if_statement` with the condition, the then statements, and `else_clause` as named children, no `then` |

- `has: {field: argument}` tests the first argument alone, a guard over any argument drops `field`
- `follows` on an argument binds with `stopBy: end` alone, the neighbor default stops at the unnamed token between siblings

| [INDEX] | [YAML]                | [SHAPE]                                                                                    |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | Block or flow mapping | `block_mapping_pair` or `flow_pair`, with `key` and `value` fields under the mapping       |
|  [02]   | Mapping value         | `block_node` or `flow_node` wraps the mapping, sequence, or scalar                         |
|  [03]   | Quoted key            | Preserve scalar spelling, or match plain, single-quoted, and double-quoted forms           |
|  [04]   | Document-level pair   | Pair inside `block_mapping, flow_mapping` inside `block_node, flow_node` inside `document` |
|  [05]   | `\"` in a quoted case | `double_quote_scalar` text holds the backslash, a `regex` over it reads the escape         |

| [INDEX] | [CSHARP]                            | [SHAPE]                                                                                          |
| :-----: | :---------------------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Match(Succ: x => x, Fail: e => 0)` | `$F` in `Match($I, $F)` binds the whole `argument` with its name, `Fail: $F` the lambda alone    |
|  [02]   | `delegate (int v, int i) { }`       | `anonymous_method_expression` with a `parameters: parameter_list` field like `lambda_expression` |

| [INDEX] | [XML]               | [SHAPE]                                                                                      |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `<Name>text</Name>` | Patterns bind whole elements, `<Name>_TEXT</Name>` captures the content                      |
|  [02]   | `Attribute="value"` | `AttValue` includes its quotes, a value that can hold the other quote matches each delimiter |

## [07]-[OUTLINE]

`ast-grep outline` returns a declaration's parsed range, a text search returns every occurrence of its name with no boundary between definition, use, and body. An outline pays in a file too long to read whole and in a language with no richer navigator, an extractor over a file read whole earns no place.
