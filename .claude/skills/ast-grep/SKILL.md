---
name: ast-grep
description: "Use when reading, searching, or rewriting code by syntax tree, or deriving and integrating ast-grep rules from diffs, code, or principles."
---

# [AST_GREP]

MCP tools (`find_code`, `find_code_by_rule`, `dump_syntax_tree`, `test_match_code_rule`) answer searches and proofs with a match list or a tree. CLI runs `ast-grep outline`, `ast-grep scan`, `ast-grep test`, writes (`-U`, `-i`), and any run that needs an exit code. Search rules stay inline, durable rules are project rule files under `sgconfig.yml`. Language tooling settles symbol identity, type information, and behavior syntax alone cannot.

[REFERENCES]:
- [01]-[REWRITING](references/rewriting.md): Fix templates, transforms, rewriters, and edit ranges
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

[SCRIPTS]:
- [01]-[RULE_CHECKS](scripts/rule-checks.sh): Rule tree checks, pairing, width, arms, parse, and measure

[AGENTS]:
- [01]-[RULE_BUILDER](../../agents/ast-grep-rule-builder.md): New rules, utils, and rewrites
- [02]-[RULE_HARDENER](../../agents/ast-grep-rule-hardener.md): Existing rules widened, collapsed, and fixed
- [03]-[RULE_TESTER](../../agents/ast-grep-rule-tester.md): Test cases against existing rules
- [04]-[OUTLINE_BUILDER](../../agents/ast-grep-outline-builder.md): Outline extractors

## [01]-[INTENT]

Rules, rewrites, and corrections serve one purpose:
- Enforce code standards through syntax, not text
- Find recurring mistakes, prevent their return, and apply proven mechanical corrections
- Remove indirection, forwarding wrappers, aliases, duplicated facts, and avoidable work
- Reduce nesting through stronger language constructs
- Merge types, schemas, strings, and files when separation adds no meaning
- Reduce code and complexity through stronger implementation, never shorter spelling or speculative restriction

Corrections weigh the whole operation, module declarations and consumers included, and apply when the remainder means more with less:
- Helper extraction, a longer combinator chain, or a renamed literal removes no work
- Data-first calls and direct conditionals stay when they state the operation within the nesting limit
- Older forms and hand-written counterparts of a package capability take its documented direct form

## [02]-[QUALITY]

Rules under `tools/ast-grep/rules/yaml/ast-grep/` lint the rule tree, because no other linter reads it. Single-file checks are rules, and cross-file checks stay in `rule-checks.sh`, which agents run and no project target names:

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

Searches follow one sequence:
1. Query fits one AST node: run `find_code` with the pattern and a bounded `max_results`
2. Structural query: start from the most specific positive rule, refine relationally, then filter captures
3. Unknown node kind: run `dump_syntax_tree` with `format=cst` on one top-level node, or `format=pattern` for a misparsed pattern
4. Multi-statement snippets take `ast-grep run -l <lang> -p '<code>' --debug-query=cst`, the tree prints on stderr before exit 8
5. Prove with `test_match_code_rule` on the matching snippet, then on the non-matching one, return to the tree on a miss
6. Failed rule call: inspect `printf '%s' '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $?`
7. Run `find_code_by_rule` with bounded `max_results`, `output_format=json` when captures or ranges feed the next step

Proof and search fold their outcomes into one failure, and the exit code of the `--stdin` scan separates them:
- `[]` with 0 is no match, 8 a rejected rule with its parse error, and 1 with JSON an `error` diagnostic
- Matching snippet first proves the rule parses, a later miss then reads as no match
- `languageGlobs` reach every path the command names, outside the root included, and a run from outside the tree parses `.ts` as `typescript`
- `scan -r <file>` refuses `--filter`, one file proves by `-r` alone and a tree rule by `scan --filter '^<id>$' <file>`
- `--stdin` takes one rule through `-r` or `--inline-rules`, and a project config holding more rules exits 3
- `find_code` returns `No matches found` for an `ERROR` pattern, a missing path, and a `language` no glob maps, `ast-grep run` separates them
- Empty `run` results read their exit code, 1 is no match, 8 a rejected pattern (a lone `$$$VAR`), and 0 with a warning an `ERROR` root

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

Rule objects are unordered `all`s, keys apply atomic, composite, then relational, and `all:` keeps its list order:
- Rules need a kind set from `pattern` or `kind`, and `regex`, `range`, `nthChild`, `not`, `all: []`, or `any: []` alone fails the load
- `kind` takes a named node, an anonymous token (`then`) fails the load with `Cannot parse rule`, exit 8
- Kind-only structure uses ESQuery in `kind`: `<a> > <b>`, `<a> <b>`, `<a> + <b>`, `<a> ~ <b>`, `<a>:has(> <b>)`, `:not(<b>)`, `:is(<a>,<b>)`
- Rightmost compound is the subject, `<a> > <b>` matches `<b>`, and `:has(> <a> > <b>)` matches nothing because `<b>` is then the direct child
- `run -k '<selector>' -l <lang>` runs a selector without a rule file, `:nth-child(2n+1 of <b>)` counts named siblings of one shape
- `:nth-last-child(1 of <kind>)` selects the last matching sibling, combined with `:nth-child` for kind-only counts
- Comma kind lists load under `not`, `inside`, `stopBy`, `constraints`, and `nthChild.ofRule`, in block form or quoted
- `pattern` and `kind` never combine to reparse, a wrong-kind pattern takes `pattern: { context: <full-code>, selector: <kind> }`
- Prefix name searches capture whole nodes with `$NAME($$$)` and `constraints: NAME: { regex: '^<prefix>' }`, and `use$HOOK` is no metavariable
- `$F($$$)` binds a member callee (`a.pipe`) as `$F` too, and `has: {field: function, kind: identifier}` keeps the plain call alone
- `$$$` before a node ends at the first sibling the node fits, `f($$$H, { $$$P })` misses `f(x, { a }, { b })` and `f($$$H, { $$$P }, $$$T)` hits it
- `$$$` after a comma needs the comma under `smart` and `cst`, `f($A, $$$R)` misses `f(x)` and hits `f(x,)`, and `ast` down binds `R` empty on `f(x)`
- `not` guards over a capture sit after its binding clause in `all:`, the first clause naming `$VAR` binds it
- `not` beside `has` in one map runs first, and an unbound `pattern: $NAME` under it matches any node of the kind
- Rules test one node, `has: {all: [<a>, <b>]}` demands one child with both shapes, one `has` per required child
- Two `not:` or `has:` keys in one map fail the load as `duplicate field <key>`, one `not: {any: [<guards>]}` or an `all:` holds both
- `strictness` per value: `cst` skips nothing, `smart` skips unnamed target nodes and comments, `ast` skips unnamed nodes on both sides, no comment
- `relaxed` skips unnamed nodes on both sides and comments, `signature` skips like `relaxed` and matches leaves by kind alone, `template` drops kinds
- `signature` keeps capture identity, the form for duplicate-shape search and useless as a name ban
- `template` needs a `kind` beside it, and a comment inside the code takes `kind` with relational rules over a lower strictness

Precision is the smallest predicate that separates the correction family from its near misses.

`regex` narrows text after structure:
- Anchor `regex` where the contract requires it, unanchored it finds `Exemption` inside `NoExemptionHere`
- `regex` is a Rust regex with no look-around or back-reference, `|` inside a name escapes, and `(?i)` sets flags inline
- `(?x)` in a `|-` block scalar holds one alternative per line with `\ ` for a space inside a phrase, and `(?-x:...)` keeps a phrase verbatim
- Prefix of whole backtick pairs `^(?:[^`]*`[^`]*`)*[^`]*` keeps a word inside a code span out of a `regex` without look-around
- `not: {regex}` beside a positive `regex` reads the whole node, and a phrase exception (`rather than`) is a tail after the word naming the next word

Kind sets bound cost:
- `kind` and `pattern` restrict candidate nodes by kind, `regex` restricts none, and `matches: <param>` prunes only beside a `kind`
- `has` and `inside` at `stopBy: end` walk the whole subtree or ancestor chain per candidate, and `field` narrows `has` to one child first
- Kindless `any:` branches widen the rule's kind set to every kind, and `all:` narrows it to the intersection of its items

Captures unify:
- One `$VAR` across clauses proves sameness, `not:` on a rebound pattern proves difference, and `$_VAR` skips both
- `has` binds `$VAR` to the earliest matching child, a later clause rejecting that child fails the rule, the narrower `has` precedes the wider
- Unification compares nodes, a name wrapped in another kind never unifies, `has: {field: name, pattern: $VAR}` reaches the identifier
- Named leaves unify by text, an `identifier` capture re-matches a `shorthand_property_identifier` or its `_pattern` form of the same text
- Strings unify by text under one quote style, and a string with an `escape_sequence` child unifies by structure, `"\nstoreDir"` with `"\ncacheDir"`
- Captures bound on one node re-match their text inside a later `not: {has: ...}` or `not: {inside: ...}`, the form for a fact every sibling repeats
- Patterns inside a `constraints` entry unify with the rule's captures, `has: {pattern: $M.lift}` under one keeps a call on another module valid

`nthChild` counts named siblings:
- Counts include comments the block holds, and `{position: <n>, ofRule: <rule>, reverse: true}` counts from the end
- `nthChild: {position: 1, ofRule: {not: {kind: comment}}}` counts semantic slots without comments, statements and type arguments included
- `ofRule` sets the counted siblings, and a kind added to it moves every earlier sibling out of the count
- Roles at the Nth slot read `nthChild: {position: N, ofRule: {not: <separator>}}` beside `not: {matches: <role>}`, the count apart from the role
- Pattern bodies match by containment, `not: {has: {nthChild: 2}}` on the container proves exactly one child
- `nthChild: 1` with `nthChild: {position: 1, reverse: true}` under `all` proves the only child from the child's side
- Match one list element through `pattern: $ITEM` with a structural container guard, or transform the `$$$` capture through a rewriter
- Two `$$$` around one element never backtrack, the element takes `inside: {pattern: <list with $$$>, stopBy: end}`

`stopBy` sets the walk:
- Default neighbor serves direct relations, `end` the whole axis, and a rule a bounded walk that includes the stop node
- Same-kind stoppers (`stopBy: {kind: <owner-kind>}`) pin the nearest enclosing owner, and bound a `has` walk where the grammar nests (C `case`)
- Closure stoppers on `has` (`stopBy: {kind: <function-kind>}`, or `any:` of them) keep an inner function's `return` from satisfying the outer owner
- `stopBy: {not: {any: [<kinds>]}}` walks up while every ancestor is an allowed kind, the stopper stated as the allowed set
- Aliases of a callee resolve through `inside: {kind: <scope>, stopBy: end, has: {pattern: $ALIAS = <callee>, stopBy: end}}`, `$ALIAS` bound first
- `inside: {not: <shape>, stopBy: end}` matches when any ancestor lacks the shape, nearly every node, absence is `not: {inside: {<shape>}}` at `end`

`field:` binds a relation to one child:
- Binding names the final relation and survives `stopBy: end` (callee vs argument, key vs value)
- Relational objects hold a rule key beside `field` and `stopBy`, `has: {field: <role>, kind: <kind>}` parses and `has: {field: <role>}` alone aborts
- Unnamed token fields (`operator`) take `has: {field: <role>, pattern: $$_X}`, the double dollar reaching the unnamed node
- `has: {not: ...}` alone and `pattern: $_ANY` beside a `not` supply no positive matcher, a `kind` or `regex: '\S'` beside the `not` does
- Guards over an unnamed capture sit on the `any` branch
- Only `has` and `inside` accept `field:`, and an `any:` branch under them refuses it as `unknown field`
- `has: {field: <role>, stopBy: end}` tests the field node, then its whole subtree, `stopBy: <rule>` its direct children alone
- `has: {stopBy: end}` without a field skips the target node
- `has: {field: <role>}` tests the first child with the field alone, a repeated field (`argument`) takes `nthChild` or a fieldless `has`

Sibling relations read the sibling list:
- `precedes` and `follows` include unnamed siblings and reject `field`
- Default `follows` reads the adjacent sibling alone, and a comment between statements breaks it until `stopBy: end`
- Nested `follows` under `stopBy: end` count earlier siblings, three levels fire on the third counted sibling with no arithmetic
- Earlier statements of the same or an enclosing block read as `follows: {stopBy: end, matches: <statement>}` on the node and on each ancestor
- Ordering rules bind statement nodes, an expression pattern alone has no statement siblings, wrap `precedes`/`follows` in `context`/`selector`
- Marker exemptions bind structurally: a comment `precedes: { kind: <body> }` marks its owner, a mark on a descendant proves nothing

Child guards read direct children:
- `has` visits unnamed children, restrict named children with `not: {has: {pattern: $_, not: <allowed>}}`
- Comments inside empty parentheses or braces satisfy `has: {pattern: $_}`, and a presence guard reads `has: {pattern: $_, not: {kind: comment}}`
- Inside of an allowed member (an `elif` arm, a nested body) takes its own arm
- Self-nesting shapes (a chain of arms, a call nested as its own first argument) report once through `not: {inside: <the nesting position>}`

`constraints` and `any:` bind after the rule:
- `constraints:` needs no kind set, negative and relational capture guards belong there
- Capture guards cannot narrow a `not:`, the negation matches everything and the rule fails silently
- `kind` alone defines no metavariable, a fixable capture pairs `kind` with `pattern: $NODE`
- Metavariables bound in `all`, relational rules, and the matching `any:` branch export to `fix`, `message`, and `transform`
- Failed `any:` branches bind nothing, and the next branch binds the same name afresh
- `any:` keeps the first arm that matches structurally and `constraints` retries no other, the wrapped pattern precedes the unwrapped one
- Bind captures required by `fix` or `constraints` in each applicable `any:` arm, local utils where the branches share bindings
- `constraints` entries over a capture one `any:` arm binds are skipped in the arms that leave it unbound

## [04]-[GRAMMAR]

Installed grammars identify node kinds, fields, wrappers, and unnamed tokens. Tree-sitter recovers with `ERROR` or zero-width `MISSING` nodes and can assign precedence differently from the language parser:
- `ERROR` searches alone prove neither complete parsing nor language validity
- `ERROR` constructs are unenforceable, the enclosing statement's pattern still matches them, and language tooling selects or edits them
- `kind: ERROR` rules load and match nothing, and `ast-grep run -k ERROR -l <lang>` finds the form

Python:

| [INDEX] | [CONSTRUCT]                      | [SHAPE]                                                                                               |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `except A, B:`                   | One `except_clause` with two `value` fields, the trailing comma form parses too                       |
|  [02]   | `from __future__ import a, b`    | `future_import_statement` with one `name: dotted_name` per name                                       |
|  [03]   | `def f(x: int)`                  | `typed_parameter` with the name as its unfielded first `identifier` and `type` as the field           |
|  [04]   | `lambda p: k(1)`                 | `lambda` with `parameters` and `body`, no statement, a literal in it belongs to the enclosing one     |
|  [05]   | `(v := a if b else c)`           | Tree-sitter nests `named_expression` in the first arm, Python binds the entire conditional            |
|  [06]   | `[i async for i in g]`           | `for_in_clause` with `async` as an unnamed child                                                      |
|  [07]   | `match x:` with `case C() if g:` | `body: block` over `alternative: case_clause`, fields `guard: if_clause` and `consequence: block`     |
|  [08]   | `case Result()`                  | `class_pattern` with no `case_pattern` child                                                          |
|  [09]   | `t"a{b}"`                        | Every node of `f"a{b}"`, the `string_start` text differs, `has: {kind: string_start, regex: '^[tT]'}` |
|  [10]   | `Optional[X]`                    | `type` > `generic_type` > `type_parameter` in an annotation, `subscript` in a value position          |
|  [11]   | `except OSError as error:`       | `except_clause` with `value: as_pattern` holding `alias: as_pattern_target`, one `field: value` regex |
|  [12]   | `except:` then `# c` then body   | `comment` sibling of the `block` under the clause, counted by `nthChild`, outside a `$BODY` capture   |

- Walrus conditionals are emitted as `(v := (a if b else c))`, and `ast.parse` checks the precedence
- Unpacking comprehensions (`[*items for items in groups]`) parse as `list_comprehension` with a `body: list_splat`, set and generator forms too

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
|  [18]   | `f() { ...; }`                  | `function_definition` with `name: word` and `body: compound_statement`, the `function` spelling too    |
|  [19]   | `(cd d && cmd)`                 | `subshell` holding a `list`, the first command under nested `list` and `redirected_statement` wrappers |
|  [20]   | `run: \|` block scalar          | `ERROR` token for the `\|` indicator before the first `command`, the commands after it parse           |
|  [21]   | `${{ inputs.x }}` in a step     | `ERROR` node, the yaml rule `no-expression-in-run-step` owns the form                                  |
|  [22]   | `select v in a b; do`           | `for_statement` with a `select` keyword child, every `for_statement` rule reads it                     |
|  [23]   | `[[ $n -eq -1 ]]`               | `unary_expression` over `number` for the negative literal                                              |
|  [24]   | `if c; then a; else b; fi`      | `if_statement` with the condition, the then statements, and `else_clause` as named children, no `then` |

- Scripts spell `$( )` and a quoted comma key, because the `ERROR` node hides the whole tree
- Pipeline-wide `!` and `time` attach to the first command, `time npm install` is one `command` with `time` as its `command_name`
- `has: {field: argument}` tests the first argument alone, `echo "$x"` hits and `echo -n "$x"` misses, a guard over any argument drops `field`
- `follows` on an argument binds with `stopBy: end` alone, because the neighbor default stops at the unnamed token between siblings

YAML, where the owning mapping and sequence match first because descendants named `jobs`, `steps`, or `matrix` can be action input data:

| [INDEX] | [CONSTRUCT]           | [SHAPE]                                                                                    |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | Block or flow mapping | `block_mapping_pair` or `flow_pair`, with `key` and `value` fields under the mapping       |
|  [02]   | Mapping value         | `block_node` or `flow_node` wraps the mapping, sequence, or scalar                         |
|  [03]   | Workflow jobs         | Root document mapping's `jobs` pair, then its direct mapping entries                       |
|  [04]   | Step collection       | Direct `steps` pair of a job or composite `runs`, then block-sequence items or flow nodes  |
|  [05]   | Quoted key            | Preserve scalar spelling, or match plain, single-quoted, and double-quoted forms          |
|  [06]   | Document-level pair   | Pair inside `block_mapping, flow_mapping` inside `block_node, flow_node` inside `document` |
|  [07]   | Comment before item 1 | Child of the enclosing pair between `:` and the value, a comment on the key line the same  |
|  [08]   | Comment before item N | Sibling of the items in the sequence, `follows: {kind: comment}`, a trailing comment too   |
|  [09]   | `\"` in a quoted case | `double_quote_scalar` text holds the backslash, and a `regex` over it reads the escape     |
|  [10]   | `---` marker          | Comment before it is a `stream` child, after it a `document` child                         |
|  [11]   | `#` line after `- \|` | Case text inside the block scalar, no `comment` node                                       |

- Escaped quoted scalars unify by escape children despite different decoded values, and deletion equality compares decoded values
- Quoted escapes and folded lines change the executed value, literal blocks keep indentation and chomping, a fix proves on the decoded command
- Anchors, aliases, tags, and merge keys stay where the predicate cannot read their meaning, and an edited anchor changes every alias consumer
- Shell resolves per property at its owner, job defaults over workflow defaults, an explicit step shell over a merged one, composite steps their own
- Removing a repeated or overriding value proves equality against the resolved inherited value, and unrelated jobs and nested input maps prove nothing
- Mapping lookup puts the key discriminator before the value predicate in an explicit `all`, because an early value capture hides a later match
- Workflow `parallel` groups hold real steps recursively and composite actions lack them, and a preceding background step has started, not completed

JSON:
- `//` and `/* */` comments parse as `comment` siblings of pairs and array items with no `ERROR` node, and `nthChild` with `ofRule` counts past them
- Raw tabs inside `string_content` parse with no `ERROR` node, `\t` is a two-character `escape_sequence`, and `[ \t]` matches the raw tab alone

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
|  [03]   | Hook          | `ast-grep scan --report-style short <paths>`                                                                            |
|  [04]   | Changed files | `git diff --name-only -z --diff-filter=ACMR <base>...` as NUL-delimited separate arguments, no scan on an empty list    |
|  [05]   | Pipeline      | `ast-grep scan --json=stream \| jq -c '<filter>'`, one match per line with its `ruleId`                                 |
|  [06]   | Baseline      | `ast-grep scan --filter '^<rule-id>$' --json=stream \| wc -l` against a recorded count, one rule's width over the tree  |
|  [07]   | Parse gate    | `ast-grep run -k ERROR -l <lang> --json=compact <paths>` exits 1 when every file parses                                 |
|  [08]   | Model text    | `--json=stream` selects the nodes, the model returns one replacement per match, edits splice by `byteOffset`            |
|  [09]   | Library       | `@ast-grep/napi` or `ast-grep-py` when a replacement is computed, arguments take per-position checks, or files cross    |

- Match objects hold `text`, `range` (`byteOffset`, zero-based `start`/`end`), `replacement`, `replacementOffsets`, and `metaVariables`
- One directory argument beats a batched file list, its walk parses in parallel with `--globs '!<glob>'` excluding inside it

Bindings expose syntax trees and text edits, with selection in ast-grep and computation in the host:
- `@ast-grep/napi` serves a JavaScript host and `ast-grep-py` a Python host
- Language hosts import the generated type map (`import type Tsx from '@ast-grep/napi/lang/Tsx'`) for `parse<Tsx>` and typed `find` results
- Each source parses once and `findAll` selects with a structural rule
- `parseAsync` serves independent sources, `findInFiles` one matcher over discovered files, and `parseFiles` selections sharing a file
- `findInFiles` and `parseFiles` resolve their file count first, and results read after every callback is awaited
- `getMultipleMatches` returns separator tokens, and `kind() !== ','` filters them before an argument list is indexed
- `NapiConfig` has no `fix`, `replace` substitutes no metavariable, and expansion reads `getMultipleMatches` before `getMatch`
- `getTransformed(<name>)` reads a `transform` output, `namedChildren` elements without punctuation, and `fieldChildren` a repeated field
- Trees are immutable, and edits collect against one parsed source as non-nested byte ranges and commit together on its root
- Insertions are zero-width edits, and an outer edit consumes edits inside its range
- Identical findings deduplicate by file and range under overlapping input paths, and returned source reparses before a dependent edit pass
- Declarations stay while their references, shorthand properties and destructuring targets included, cannot be resolved
- `registerDynamicLanguage` runs once per process with every `@ast-grep/lang-*` package in one call, and an unregistered language fails at parse time
- `ast-grep-py` is `SgRoot(src, language)` with rules as keyword arguments (`find(pattern=<code>)`), and file discovery is the caller's
- `@ast-grep/wasm` awaits `initializeTreeSitter`, then registers each language's WASM parser through `registerDynamicLanguage`
- Host programs return the correction they compute, and a CLI wrapper adds no capability
