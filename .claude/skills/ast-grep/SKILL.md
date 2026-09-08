---
name: ast-grep
description: "Use when reading, searching, or rewriting code by syntax tree, or deriving and integrating ast-grep rules from diffs, code, or principles."
---

# [AST_GREP]

MCP tools (`find_code`, `find_code_by_rule`, `dump_syntax_tree`, `test_match_code_rule`) answer searches and proofs with a match list or a tree. CLI runs `ast-grep outline`, `ast-grep scan`, `ast-grep test`, writes (`-U`, `-i`), and runs that need an exit code. Search rules stay inline, durable rules are files under `sgconfig.yml`. Language tooling resolves symbol identity, types, and behavior beyond syntax.

[REFERENCES]:
- [01]-[REWRITING](references/rewriting.md): Fix templates, transforms, rewriters, edit ranges, and custom languages
- [02]-[CONFIGURATION](references/configuration.md): `sgconfig.yml`, rule and util registration, languages, and suppression
- [03]-[RULE_BUILDING](references/rule-building.md): Deriving rules from diffs, code smells, and principles
- [04]-[RULE_HARDENING](references/rule-hardening.md): Widening, collapsing, and fixing existing rules
- [05]-[RULE_TESTING](references/rule-testing.md): Test cases, snapshots, and adversarial checks
- [06]-[OUTLINE](references/outline.md): `ast-grep outline` usage and extractor construction

[TEMPLATES]:
- [01]-[SGCONFIG](templates/sgconfig.template.yml): Project configuration
- [02]-[RULE](templates/rule.yml): Lint rule
- [03]-[RULE_REWRITE](templates/rule-rewrite.yml): On-demand rewrite
- [04]-[UTIL](templates/util.yml): Shared utility rule
- [05]-[RULE_TEST](templates/rule-test.yml): Rule test cases
- [06]-[OUTLINE_ITEM](templates/outline-item.yml): Outline item extractor
- [07]-[OUTLINE_MEMBER](templates/outline-member.yml): Outline member extractor

[AGENTS]:
- [01]-[RULE_BUILDER](../../agents/ast-grep-rule-builder.md): New rules, utils, and rewrites
- [02]-[RULE_HARDENER](../../agents/ast-grep-rule-hardener.md): Existing rules widened, collapsed, and fixed
- [03]-[RULE_TESTER](../../agents/ast-grep-rule-tester.md): Test cases against existing rules
- [04]-[OUTLINE_BUILDER](../../agents/ast-grep-outline-builder.md): Outline extractors

## [01]-[INTENT]

Rules, rewrites, and corrections:
- Enforce code standards through syntax
- Find recurring mistakes, block their return, and apply proven mechanical corrections
- Reduce nesting through stronger language constructs
- Merge types, schemas, strings, and files when separation adds no meaning
- Reduce code and complexity through stronger implementation, never shorter spelling or speculative restriction

Corrections weigh the whole operation, module declarations and consumers included, and apply when what remains means more with less:
- Helper extraction, a longer combinator chain, or a renamed literal removes no work
- Data-first calls and direct conditionals stay when they state the operation within the nesting limit
- Older forms and hand-written counterparts of a package capability take its documented direct form

## [02]-[QUALITY]

Rules under `tools/ast-grep/rules/yaml/ast-grep/` alone lint the rule tree:

| [INDEX] | [RULE]                    | [READS]                         | [CRITERION]                                                               |
| :-----: | :------------------------ | :------------------------------ | :------------------------------------------------------------------------ |
|  [01]   | `no-single-item-branch`   | Rules, utils, rewrites, outline | One-item lists state no conjunction or alternative                        |
|  [02]   | `no-kind-only-any`        | Rules, utils, rewrites, outline | Comma kind list states a kind union, both forms prune by one kind set     |
|  [03]   | `no-trailing-period`      | Every file                      | Comments and diagnostics are one statement                                |
|  [04]   | `no-second-sentence`      | Rules, rewrites                 | `message` and `note` are one sentence each                                |
|  [05]   | `no-listed-word`          | Every file                      | `clean-prose` tables own the words, the header names the terms kept       |
|  [06]   | `no-machine-path-in-case` | Tests                           | Machine paths are neither placeholder nor pinned literal                  |
|  [07]   | `require-schema-header`   | Rules, utils, rewrites          | Editor validates against the schema the templates name                    |
|  [08]   | `require-rewrite-id-form` | Rewrites                        | Id names the correction                                                   |

## [03]-[MATCHING]

Patterns are valid code under the language's tree-sitter grammar with whole-node metavariables: `$VAR` one named node, `$$VAR` one named or unnamed node (operators and keywords included), `$$$MULTI` lazy zero-or-more without backtracking, `$_` and `$_NAME` non-capturing. Patterns match more the less they specify.

Searches run in sequence:
1. Query fits one AST node: run `find_code` with the pattern and a bounded `max_results`
2. Structural query: start from the most specific positive rule, refine relationally, then filter captures
3. Unknown node kind: run `dump_syntax_tree` with `format=cst` on one top-level node, or `format=pattern` for a misparsed pattern
4. Multi-statement snippets take `ast-grep run -l <lang> -p '<code>' --debug-query=cst`, the tree prints on stderr before exit 8
5. Prove with `test_match_code_rule` on the matching snippet, then on the non-matching one, return to the tree on a miss
6. Failed rule call: inspect `printf '%s' '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $?`
7. Run `find_code_by_rule` with bounded `max_results`, `output_format=json` when captures or ranges feed the next step

Proof and search fold their outcomes into one failure, the `--stdin` scan exit code separates them:
- `[]` with 0 is no match, 8 a rejected rule with its parse error, and 1 with JSON an `error` diagnostic
- Matching snippet first proves the rule parses, a later miss reads as no match
- `languageGlobs` reach every path the command names, outside the root included
- Runs from outside the tree parse `.ts` as `typescript`
- Inside the tree `.ts` is `tsx`, `-l typescript` and MCP `language: typescript` match no file, and MCP `project_folder` is an absolute path
- `scan` takes no `-l`, each rule's `language` field parses its files
- `scan -r <file>` refuses `--filter` and loads no `utilDirs`, a registered rule proves by `scan --filter '^<id>$' <file>`
- `-U` writes fixes and prints no `--json`, and `-i` prompts on a terminal the Bash tool lacks
- `--stdin` takes one rule through `-r` or `--inline-rules`, a project config holding more rules exits 3
- `find_code` returns `No matches found` for an `ERROR` pattern, a missing path, and a `language` no glob maps, `ast-grep run` separates them
- Empty `run` results read the exit code: 1 no match, 8 a rejected pattern (a lone `$$$VAR`), 0 with a warning an `ERROR` root

```yaml
id: <query-id>
language: <language>
utils:
  <util-id>:
    kind: <kind-a>, <kind-b>
rule:
  all:
    - pattern: <code with $VAR and $$$ARGS>
    - has: { pattern: <sub-pattern>, stopBy: end }
    - not: { inside: { kind: <kind>, stopBy: end } }
constraints:
  <VAR>: { regex: '<rust-regex>' }
```

Rule objects are unordered `all`s with keys applied atomic, composite, then relational, `all:` keeps list order:
- Rules need a kind set from `pattern` or `kind`, `regex`, `range`, `nthChild`, `not`, `all: []`, or `any: []` alone fails the load
- `kind` takes a named node, an anonymous token (`then`) fails the load with `Cannot parse rule`, exit 8
- Kind-only structure uses ESQuery in `kind`: `<a> > <b>`, `<a> <b>`, `<a> + <b>`, `<a> ~ <b>`, `<a>:has(> <b>)`, `:not(<b>)`, `:is(<a>,<b>)`
- Rightmost compound is the subject, `<a> > <b>` matches `<b>`, `:has(> <a> > <b>)` makes `<b>` the direct child and matches nothing
- `run -k '<selector>' -l <lang>` runs a selector without a rule file
- `:nth-child(2n+1 of <b>)` counts named siblings of one shape
- `:nth-last-child(1 of <kind>)` selects the last matching sibling, combined with `:nth-child` for kind-only counts
- Comma kind lists load under `not`, `inside`, `stopBy`, `constraints`, and `nthChild.ofRule`, in block form or quoted
- `pattern` and `kind` never combine to reparse, a wrong-kind pattern takes `pattern: { context: <full-code>, selector: <kind> }`
- Prefix name searches capture whole nodes with `$NAME($$$)` and `constraints: NAME: { regex: '^<prefix>' }`
- `use$HOOK` is no metavariable
- `$F($$$)` binds a member callee (`a.pipe`) as `$F`, `has: {field: function, kind: identifier}` keeps the plain call alone
- `$$$` before a node ends at the first fitting sibling, `f($$$H, { $$$P })` misses `f(x, { a }, { b })` and `f($$$H, { $$$P }, $$$T)` matches it
- `smart` and `cst` require the comma before `$$$`, `f($A, $$$R)` misses `f(x)` and matches `f(x,)`, `ast` and below bind `R` empty on `f(x)`
- `not` guards over a capture sit after its binding clause in `all:`, the first clause naming `$VAR` binds it
- `not` beside `has` in one map runs first, an unbound `pattern: $NAME` under it matches any node of the kind
- Rules test one node, `has: {all: [<a>, <b>]}` demands one child with both shapes, one `has` per required child
- Repeated `not:` or `has:` keys in one map fail the load as `duplicate field <key>`, `not: {any: [<guards>]}` or an `all:` holds both
- `strictness` per value: `cst` skips nothing, `smart` skips unnamed target nodes and comments, `ast` skips unnamed nodes on both sides, no comment
- `relaxed` skips unnamed nodes on both sides and comments, `signature` skips like `relaxed` and matches leaves by kind alone, `template` drops kinds
- `signature` keeps capture identity, the form for duplicate-shape search and useless as a name ban
- `template` needs a `kind` beside it
- Comments inside the code take `kind` with relational rules over a lower strictness

Precision is the smallest predicate separating the correction family from its near misses.

`regex` narrows text after structure:
- Anchor `regex` where the contract requires it, unanchored it finds `Exemption` inside `NoExemptionHere`
- `regex` is a Rust regex with no look-around or back-reference
- `|` inside a name escapes
- `(?i)` sets flags inline
- `(?x)` in a `|-` block scalar holds one alternative per line with `\ ` for a space inside a phrase, `(?-x:...)` keeps a phrase verbatim
- Prefix of whole backtick pairs `^(?:[^`]*`[^`]*`)*[^`]*` keeps a word inside a code span out of a `regex` without look-around
- `not: {regex}` beside a positive `regex` reads the whole node
- Phrase exceptions (`rather than`) are a tail after the word naming the next word

Kind sets bound cost:
- `kind` and `pattern` restrict candidate nodes by kind, `regex` restricts none, `matches: <param>` prunes only beside a `kind`
- `has` and `inside` at `stopBy: end` walk the whole subtree or ancestor chain per candidate, `field` narrows `has` to one child first
- Kindless `any:` arms widen the kind set to every kind, `all:` narrows it to the intersection of its items

Captures unify:
- One `$VAR` across clauses proves sameness, `not:` on a rebound pattern proves difference, `$_VAR` skips both
- `has` binds `$VAR` to the earliest matching child, a later clause rejecting that child fails the rule, the narrower `has` precedes the wider
- Unification compares nodes, a name wrapped in another kind never unifies, `has: {field: name, pattern: $VAR}` reaches the identifier
- Named leaves unify by text, an `identifier` capture re-matches a `shorthand_property_identifier` or its `_pattern` form of the same text
- Strings unify by text under one quote style, a string with an `escape_sequence` child unifies by structure, `"\n<a>"` with `"\n<b>"`
- Captures bound on one node re-match their text inside a later `not: {has: ...}` or `not: {inside: ...}`, the form for a fact every sibling repeats
- Patterns inside a `constraints` entry unify with the rule's captures, `has: {pattern: $M.lift}` under one keeps a call on another module valid

`nthChild` counts named siblings:
- Counts include comments the block holds
- `{position: <n>, ofRule: <rule>, reverse: true}` counts from the end
- `nthChild: {position: 1, ofRule: {not: {kind: comment}}}` counts semantic slots without comments, statements and type arguments included
- `ofRule` sets the counted siblings, a kind added to it drops every earlier sibling from the count
- Roles at the Nth slot read `nthChild: {position: N, ofRule: {not: <separator>}}` beside `not: {matches: <role>}`, the count apart from the role
- Pattern bodies match by containment, `not: {has: {nthChild: 2}}` on the container proves exactly one child
- `nthChild: 1` with `nthChild: {position: 1, reverse: true}` under `all` proves the only child from the child's side
- Match one list element through `pattern: $ITEM` with a structural container guard, or transform the `$$$` capture through a rewriter
- `$$$` on both sides of one element never backtracks, the element takes `inside: {pattern: <list with $$$>, stopBy: end}`

`stopBy` sets the walk:
- Default neighbor serves direct relations, `end` the whole axis, and a rule a bounded walk including the stop node
- `stopBy: {kind: <owner-kind>}` pins the nearest enclosing owner and bounds a `has` walk where the grammar nests (C `case`)
- `stopBy: {kind: <function-kind>}` on `has`, or `any:` of such kinds, keeps an inner function's `return` from satisfying the outer owner
- `stopBy: {not: {any: [<kinds>]}}` walks up while every ancestor is an allowed kind
- Aliases of a callee resolve through `inside: {kind: <scope>, stopBy: end, has: {pattern: $ALIAS = <callee>, stopBy: end}}`, `$ALIAS` bound first
- `inside: {not: <shape>, stopBy: end}` matches when any ancestor lacks the shape, nearly every node, absence is `not: {inside: {<shape>}}` at `end`

`field:` binds a relation to one child:
- Binding names the final relation and survives `stopBy: end` (callee vs argument, key vs value)
- Relational objects hold a rule key beside `field` and `stopBy`, `has: {field: <role>, kind: <kind>}` parses and `has: {field: <role>}` alone aborts
- Unnamed token fields (`operator`) take `has: {field: <role>, pattern: $$_X}`, the double dollar reaching the unnamed node
- `has: {not: ...}` alone and `pattern: $_ANY` beside a `not` supply no positive matcher, a `kind` or `regex: '\S'` beside the `not` does
- Guards over an unnamed capture sit on the `any` arm
- `has` and `inside` alone accept `field:`, an `any:` arm under them refuses it as `unknown field`
- `has: {field: <role>, stopBy: end}` tests the field node, then its whole subtree, `stopBy: <rule>` its direct children alone
- `has: {stopBy: end}` without a field skips the target node
- `has: {field: <role>}` tests the first child with the field alone, a repeated field (`argument`) takes `nthChild` or a fieldless `has`

Sibling relations read the sibling list:
- `precedes` and `follows` include unnamed siblings and reject `field`
- Default `follows` reads the adjacent sibling alone, a comment between statements breaks it until `stopBy: end`
- Nested `follows` under `stopBy: end` count earlier siblings, three levels fire on the third counted sibling with no arithmetic
- Earlier statements of the same or an enclosing block read as `follows: {stopBy: end, matches: <statement>}` on the node and on each ancestor
- Expression patterns alone have no statement siblings, ordering rules bind statement nodes through `context`/`selector`
- Marker exemptions bind a comment to its owner through `precedes: { kind: <body> }`, a mark on a descendant proves nothing

Child guards read direct children:
- `has` visits unnamed children, restrict named children with `not: {has: {pattern: $_, not: <allowed>}}`
- Comments inside empty parentheses or braces satisfy `has: {pattern: $_}`, a presence guard reads `has: {pattern: $_, not: {kind: comment}}`
- Inside of an allowed member (an `elif` arm, a nested body) takes its own arm
- Self-nesting shapes (a chain of arms, a call nested as its own first argument) report once through `not: {inside: <the nesting position>}`

`constraints` and `any:` bind after the rule:
- `constraints:` needs no kind set
- Negative and relational capture guards belong under `constraints:`
- Capture guards cannot narrow a `not:`, the negation matches everything and the rule fails silently
- `kind` alone defines no metavariable, a fixable capture pairs `kind` with `pattern: $NODE`
- Metavariables bound in `all`, relational rules, and the matching `any:` arm export to `fix`, `message`, and `transform`
- Failed `any:` arms bind nothing, the next arm binds the same name again
- `any:` keeps the first structurally matching arm with no `constraints` retry, the wrapped pattern precedes the unwrapped one
- Bind captures required by `fix` or `constraints` in each applicable `any:` arm, local utils where the arms share bindings
- `constraints` entries over a capture one `any:` arm binds are skipped in the arms that leave it unbound

## [04]-[GRAMMAR]

Installed grammars identify node kinds, fields, wrappers, and unnamed tokens. Tree-sitter recovers with `ERROR` or zero-width `MISSING` nodes. Tree-sitter precedence can differ from the language parser's:
- `ERROR` searches alone prove neither complete parsing nor language validity
- `ERROR` constructs are unenforceable, the enclosing statement pattern matches them, language tooling selects or edits them
- `kind: ERROR` rules load and match nothing, `ast-grep run -k ERROR -l <lang>` finds the form

Python:

| [INDEX] | [CONSTRUCT]                      | [SHAPE]                                                                                               |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `except A, B:`                   | One `except_clause` with two `value` fields, trailing comma included                                  |
|  [02]   | `from __future__ import a, b`    | `future_import_statement` with one `name: dotted_name` per name                                       |
|  [03]   | `def f(x: int)`                  | `typed_parameter` with the name as its unfielded first `identifier` and `type` as the field           |
|  [04]   | `lambda p: k(1)`                 | `lambda` with `parameters` and `body`, no statement, a literal in it belongs to the enclosing one     |
|  [05]   | `(v := a if b else c)`           | Tree-sitter nests `named_expression` in the first arm, Python binds the whole conditional             |
|  [06]   | `[i async for i in g]`           | `for_in_clause` with `async` as an unnamed child                                                      |
|  [07]   | `match x:` with `case C() if g:` | `body: block` over `alternative: case_clause`, fields `guard: if_clause` and `consequence: block`     |
|  [08]   | `case Result()`                  | `class_pattern` with no `case_pattern` child                                                          |
|  [09]   | `t"a{b}"`                        | Every node of `f"a{b}"`, the `string_start` text differs, `has: {kind: string_start, regex: '^[tT]'}` |
|  [10]   | `Optional[X]`                    | `type` > `generic_type` > `type_parameter` in an annotation, `subscript` in a value position          |
|  [11]   | `except OSError as error:`       | `except_clause` with `value: as_pattern` holding `alias: as_pattern_target`, one `field: value` regex |
|  [12]   | `except:` then `# c` then body   | `comment` sibling of the `block` under the clause, counted by `nthChild`, outside a `$BODY` capture   |

- Fixes emit walrus conditionals as `(v := (a if b else c))`, `ast.parse` checks the precedence
- `[*items for items in groups]` parses as `list_comprehension` with a `body: list_splat`, set and generator forms with the same `body`

Bash:

| [INDEX] | [CONSTRUCT]                     | [SHAPE]                                                                                                |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `[ x = y ]`, `[[ x = y ]]`      | `test_command` with the bracket unnamed over a `binary_expression` with `left`, `operator`, `right`    |
|  [02]   | `$(cmd)`, backticks             | `command_substitution` with the delimiter unnamed and one `command` child per simple command           |
|  [03]   | `echo $x`, `echo "$x"`          | `simple_expansion` under `command` when unquoted and under `string` when double-quoted                 |
|  [04]   | `$1`, `${10}`, `$@`, `$?`       | `variable_name` under `simple_expansion` or `expansion`, `special_variable_name` for `$@`, `$#`, `$?`  |
|  [05]   | `${x#*/}`, `${x:-d}`            | `expansion` with `variable_name` and an `operator` child, the pattern a `regex` or `word` child        |
|  [06]   | `a[k,$b]`, `${a[k,$b]}`         | `subscript` with `index: concatenation`, `${a["k,$b"]}` a `string` index                               |
|  [07]   | `${a[$a,$b]}`, `${a[k,$b,$c]}`  | `subscript` with an `ERROR` child in place of `index`                                                  |
|  [08]   | `x=${ ls -1;}`, `x=${\| cmd;}`  | `ERROR` over the statement at top level, a lone `${ cmd;}` an `expansion` with `MISSING }`             |
|  [09]   | `cmd <<<"$x"`                   | `command` with `redirect: herestring_redirect`, the last stage's after a pipeline                      |
|  [10]   | `cmd > f`, `done < <(p)`        | `redirected_statement` with `body` and `redirect: file_redirect` wrapping the whole list or pipeline   |
|  [11]   | `a \| b > f \| c`               | `redirected_statement(body: pipeline)` over the earlier stages, the later stage a sibling of it        |
|  [12]   | `ls \| while ...; done`         | `pipeline` with a `while_statement` stage, `until` a `while_statement` with an unnamed `until` child   |
|  [13]   | `for f in $(ls); do`            | `for_statement` with `variable`, `value: command_substitution`, and `body: do_group`                   |
|  [14]   | `for ((i = 0; ...))`            | `c_style_for_statement` with `initializer`, `condition`, and `update`                                  |
|  [15]   | `(( x += 1 ))`, `(( x++ ))`     | `compound_statement` over a `binary_expression`, a `postfix_expression` for `x++`                      |
|  [16]   | `$((x + 1))`                    | `arithmetic_expansion` over the same expression kinds in a value position                              |
|  [17]   | `local -n r=$1`, `declare -A m` | `declaration_command` with the keyword, an option `word`, and `variable_assignment` children           |
|  [18]   | `f() { ...; }`                  | `function_definition` with `name: word` and `body: compound_statement`, `function` spelling included   |
|  [19]   | `(cd d && cmd)`                 | `subshell` holding a `list`, the first command under nested `list` and `redirected_statement` wrappers |
|  [20]   | `run: \|` block scalar          | `ERROR` token for the `\|` indicator before the first `command`, the commands after it parse           |
|  [21]   | `${{ inputs.x }}` in a step     | `ERROR` node, the yaml rule `no-expression-in-run-step` owns the form                                  |
|  [22]   | `select v in a b; do`           | `for_statement` with a `select` keyword child, every `for_statement` rule reads it                     |
|  [23]   | `[[ $n -eq -1 ]]`               | `unary_expression` over `number` for the negative literal                                              |
|  [24]   | `if c; then a; else b; fi`      | `if_statement` with the condition, the then statements, and `else_clause` as named children, no `then` |

- Scripts spell `$( )` and a quoted comma key, an `ERROR` node hides the whole tree
- Pipeline-wide `!` and `time` attach to the first command, `time npm install` is one `command` with `time` as its `command_name`
- `has: {field: argument}` tests the first argument alone, `echo "$x"` matches and `echo -n "$x"` misses, a guard over any argument drops `field`
- `follows` on an argument binds with `stopBy: end` alone, the neighbor default stops at the unnamed token between siblings

YAML, when descendants named `jobs`, `steps`, or `matrix` can be action input data, the owning mapping and sequence match first:

| [INDEX] | [CONSTRUCT]           | [SHAPE]                                                                                    |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | Block or flow mapping | `block_mapping_pair` or `flow_pair`, with `key` and `value` fields under the mapping       |
|  [02]   | Mapping value         | `block_node` or `flow_node` wraps the mapping, sequence, or scalar                         |
|  [03]   | Workflow jobs         | Root document mapping's `jobs` pair, then its direct mapping entries                       |
|  [04]   | Step collection       | Direct `steps` pair of a job or composite `runs`, then block-sequence items or flow nodes  |
|  [05]   | Quoted key            | Preserve scalar spelling, or match plain, single-quoted, and double-quoted forms           |
|  [06]   | Document-level pair   | Pair inside `block_mapping, flow_mapping` inside `block_node, flow_node` inside `document` |
|  [07]   | Comment before item 1 | Child of the enclosing pair between `:` and the value, a comment on the key line the same  |
|  [08]   | Comment before item N | Sibling of the sequence items, `follows: {kind: comment}`, trailing comments included      |
|  [09]   | `\"` in a quoted case | `double_quote_scalar` text holds the backslash, and a `regex` over it reads the escape     |
|  [10]   | `---` marker          | Comment before it is a `stream` child, after it a `document` child                         |
|  [11]   | `#` line after `- \|` | Case text inside the block scalar, no `comment` node                                       |

- Escaped quoted scalars unify by escape children despite different decoded values, deletion equality compares decoded values
- Quoted escapes and folded lines change the executed value, literal blocks keep indentation and chomping, a fix proves on the decoded command
- Anchors, aliases, tags, and merge keys stay where the predicate cannot read their meaning, an edited anchor changes every alias consumer
- Shell resolves per property at its owner, job defaults over workflow defaults, an explicit step shell over a merged one, composite steps their own
- Removing a repeated or overriding value proves equality against the resolved inherited value, unrelated jobs and nested input maps prove nothing
- Mapping lookup puts the key discriminator before the value predicate in an explicit `all`, an early value capture hides a later match
- Workflow `parallel` groups hold real steps recursively, composite actions lack them
- Preceding background steps have started, not completed

JSON:
- `//` and `/* */` comments parse as `comment` siblings of pairs and array items with no `ERROR` node, `nthChild` with `ofRule` counts past them
- Raw tabs inside `string_content` parse with no `ERROR` node, `\t` is a two-character `escape_sequence`, `[ \t]` matches the raw tab alone

TypeScript:

| [INDEX] | [CONSTRUCT]                       | [SHAPE]                                                                                   |
| :-----: | :-------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `run(index: Map = new Map())`     | `required_parameter` with `type` and `value` fields, the shape of a `variable_declarator` |
|  [02]   | `await using handle = open(path)` | `await_expression` over an `assignment_expression`                                        |

C#:

| [INDEX] | [CONSTRUCT]                         | [SHAPE]                                                                                          |
| :-----: | :---------------------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Match(Succ: x => x, Fail: e => 0)` | `$F` in `Match($I, $F)` binds the whole `argument` with its name, `Fail: $F` the lambda alone    |
|  [02]   | `delegate (int v, int i) { }`       | `anonymous_method_expression` with a `parameters: parameter_list` field like `lambda_expression` |
|  [03]   | `M(f: x, g /* b */ : y)`            | `comment` sibling inside `block`, and inside `argument` between the `name` field and `:`         |

## [05]-[INTEGRATIONS]

Hosts consume scans through exit codes, output formats, and bindings:

| [INDEX] | [HOST]        | [FORM]                                                                                                                  |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------------------------------- |
|  [01]   | CI annotation | `ast-grep scan --format github` prints `::error file=,line=,title=<rule-id>::` per finding above `hint`, no upload      |
|  [02]   | Code scanning | `ast-grep scan --format sarif > <file>` then `github/codeql-action/upload-sarif`, `--format` excludes `--json`          |
|  [03]   | Changed files | `git diff --name-only -z --diff-filter=ACMR <base>...` as NUL-delimited separate arguments, no scan on an empty list    |
|  [04]   | Pipeline      | `ast-grep scan --json=stream \| jq -c '<filter>'`, one match per line with its `ruleId`                                 |
|  [05]   | Baseline      | `ast-grep scan --filter '^<rule-id>$' --json=stream \| wc -l` against a recorded count, one rule's width over the tree  |
|  [06]   | Parse gate    | `ast-grep run -k ERROR -l <lang> --json=compact <paths>` exits 1 when every file parses                                 |
|  [07]   | Model text    | `--json=stream` selects the nodes, the model returns one replacement per match, edits splice by `byteOffset`            |
|  [08]   | Library       | `@ast-grep/napi` or `ast-grep-py` when a replacement is computed, arguments take per-position checks, or files cross    |

- Match objects hold `text`, `range` (`byteOffset`, zero-based `start`/`end`), `replacement`, `replacementOffsets`, and `metaVariables`
- One directory argument beats a batched file list, its walk parses in parallel with `--globs '!<glob>'` excluding inside it

Bindings expose syntax trees and text edits, with selection in ast-grep and computation in the host:
- `@ast-grep/napi` serves a JavaScript host and `ast-grep-py` a Python host
- Language hosts import the generated type map (`import type Tsx from '@ast-grep/napi/lang/Tsx'`) for `parse<Tsx>` and typed `find` results
- Sources parse once and `findAll` selects with a structural rule
- `parseAsync` serves independent sources, `findInFiles` one matcher over discovered files, and `parseFiles` selections sharing a file
- `findInFiles` and `parseFiles` resolve their file count first, results read after every callback is awaited
- `getMultipleMatches` returns separator tokens, `kind() !== ','` filters them before an argument list is indexed
- `NapiConfig` has no `fix`, `replace` substitutes no metavariable, expansion reads `getMultipleMatches` before `getMatch`
- `getTransformed(<name>)` reads a `transform` output, `namedChildren` elements without punctuation, and `fieldChildren` a repeated field
- Trees are immutable, edits collect against one parsed source as non-nested byte ranges and commit together on its root
- Insertions are zero-width edits, an outer edit consumes edits inside its range
- Identical findings deduplicate by file and range under overlapping input paths
- Returned source reparses before a dependent edit pass
- Declarations stay while their references, shorthand properties and destructuring targets included, cannot be resolved
- `registerDynamicLanguage` runs once per process with every `@ast-grep/lang-*` package in one call, an unregistered language fails at parse time
- `ast-grep-py` is `SgRoot(src, language)` with rules as keyword arguments (`find(pattern=<code>)`), file discovery is the caller's
- `@ast-grep/wasm` awaits `initializeTreeSitter` and registers each language's WASM parser through `registerDynamicLanguage`
- Host programs return the correction they compute, a CLI wrapper adds no capability
