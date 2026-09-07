# [GRAMMAR]

Use the installed grammar to identify node kinds, fields, wrappers, and unnamed tokens. Tree-sitter can recover with `ERROR` or zero-width `MISSING` nodes and can assign different precedence from the language parser. An `ERROR` search alone establishes neither complete parsing nor language validity. When supported language syntax produces `ERROR` or `MISSING`, use language tooling to select or edit that structure. A rewrite selected from supported nodes can emit newer syntax accepted by the language parser. Keep the configured language version and its supported features.

## [01]-[PYTHON]

| [INDEX] | [CONSTRUCT]                      | [SHAPE]                                                                                               |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `except A, B:`                   | One `except_clause` with two `value` fields, the trailing comma form parses too                       |
|  [02]   | `from __future__ import a, b`    | `future_import_statement` with one `name: dotted_name` per name                                       |
|  [03]   | `def f(x: int)`                  | `typed_parameter` with the name as its unfielded first `identifier` and `type` as the field           |
|  [04]   | `lambda p: k(1)`                 | `lambda` with `parameters` and `body`, no statement, a literal in it belongs to the enclosing one     |
|  [05]   | `(v := a if b else c)`           | Tree-sitter nests `named_expression` in the first arm, Python binds the entire conditional            |
|  [06]   | `[i async for i in g]`           | `for_in_clause` with `async` as an unnamed child                                                      |
|  [07]   | `match x:` with `case C() if g:` | `body: block` over `alternative: case_clause`, fields `guard: if_clause` and `consequence: block`     |
|  [08]   | `x=${ ls -1;}`, `x=${\| cmd;}`  | `ERROR` over the statement at top level (issue 301), a bare `${ cmd;}` an `expansion` with `MISSING }` |
|  [09]   | `case Result()`                  | `class_pattern` with no `case_pattern` child                                                          |
|  [10]   | `t"a{b}"`                        | Every node of `f"a{b}"`, the `string_start` text differs, `has: {kind: string_start, regex: '^[tT]'}` |
|  [11]   | `a \| b > f \| c`               | `redirected_statement(body: pipeline)` over the earlier stages, the later stage a sibling of it        |
|  [12]   | `ls \| while ...; done`         | `pipeline` with a `while_statement` stage, `until` a `while_statement` with an unnamed `until` child   |
|  [13]   | `Optional[X]`                    | `type` > `generic_type` > `type_parameter` in an annotation, `subscript` in a value position          |

Check assignment-expression precedence with Python `ast.parse`. Parenthesize the complete conditional as `(v := (a if b else c))` when selecting or emitting it through tree-sitter. A matching import can follow an unsupported `lazy` modifier, inspect the enclosing source before replacing the import. Ruff `TID254` enforces the project's lazy-import policy.

Unpacking comprehensions (`[*items for items in groups]`) parse as `list_comprehension` with a `body: list_splat`. Check replacements with Python 3.15, which accepts the syntax.

## [02]-[BASH]

Node shapes and gaps apply to tree-sitter-bash 0.25.0. Use `bash -n` for Bash syntax acceptance. The Bash manual defines the execution environment and trailing-newline behavior of each command substitution form independently of the grammar.

| [INDEX] | [CONSTRUCT]                     | [SHAPE]                                                                                                |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `[ x = y ]`, `[[ x = y ]]`      | `test_command` with the bracket unnamed over a `binary_expression` with `left`, `operator`, `right`    |
|  [02]   | `$(cmd)`, backticks             | `command_substitution` with the delimiter unnamed and one `command` child per simple command           |
|  [03]   | `echo $x`, `echo "$x"`          | `simple_expansion` under `command` when unquoted and under `string` when double-quoted                 |
|  [04]   | `$1`, `$@`, `$?`                | `simple_expansion` over `variable_name` for `$1`, `special_variable_name` for `$@`, `$*`, `$#`, `$?`   |
|  [05]   | `${x#*/}`, `${x:-d}`            | `expansion` with `variable_name` and an `operator` child, the pattern a `regex` or `word` child        |
|  [06]   | `a[k,$b]`, `${a[k,$b]}`         | `subscript` with `index: concatenation`, `${a["k,$b"]}` a `string` index                               |
|  [07]   | `${a[$a,$b]}`, `${a[k,$b,$c]}`  | `subscript` with an `ERROR` child in place of `index` (issue 268)                                      |
|  [08]   | `x=${ ls -1;}`, `x=${\| cmd;}`  | `ERROR` over the statement at top level (issue 301), a bare `${ cmd;}` an `expansion` with `MISSING }` |
|  [09]   | `cmd <<<"$x"`                   | `command` with `redirect: herestring_redirect`                                                         |
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

Pipeline-wide `!` and `time` modifiers can attach to the first command in the tree. Preserve their whole-pipeline meaning when moving stages or changing redirections.

A field read tests the first child of that field alone: `has: {field: argument, ...}` matched `echo "$x"` and missed `echo -n "$x"`, and a guard over any argument reads `has: {kind: ...}` without `field`. `follows` on an argument binds with `stopBy: end` alone, because the default neighbor search stops at the unnamed token between siblings.

## [03]-[YAML]

Match the owning mapping and sequence before interpreting a key as workflow configuration. A descendant named `jobs`, `steps`, or `matrix` can be action input data.

| [INDEX] | [CONSTRUCT]           | [SHAPE]                                                                                    |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | Block or flow mapping | `block_mapping_pair` or `flow_pair`, with `key` and `value` fields under the mapping       |
|  [02]   | Mapping value         | `block_node` or `flow_node` wraps the mapping, sequence, or scalar                         |
|  [03]   | Workflow jobs         | Root document mapping's `jobs` pair, then its direct mapping entries                       |
|  [04]   | Step collection       | Direct `steps` pair of a job or composite `runs`, then block-sequence items or flow nodes  |
|  [05]   | Quoted key            | Preserve scalar spelling or match plain, single-quoted, and double-quoted forms explicitly |

Read script scalars through YAML before interpreting their shell text. Escaped quoted scalars can unify by escape children despite different decoded values. Exclude those forms from deletion equality or compare decoded values. Quoted escapes and folded lines change the executed value, and literal blocks retain their indentation and chomping behavior. Test the decoded command after applying a fix, not only the host file's parse. Keep anchors, aliases, tags, and merge keys when their resolved meaning is needed but unavailable to the predicate.

Resolve inherited settings at their effective owner. Job shell defaults override workflow defaults, while composite steps declare their own shell. Use the repository's explicit Bash shell contract instead of inferring an interpreter from runner labels. Mise provisions the executable, and the effective shell setting selects it. A rule removing a repeated value must establish equality and preserve a more specific override. Unrelated jobs and nested input maps cannot supply or invalidate that proof.

For mapping lookup, put the key discriminator before the capture-bearing value predicate in an explicit `all`, including inside parameterized utilities. Test a different first property followed by the intended property; otherwise an early value capture can hide a later match.

Workflow `parallel` groups contain real steps recursively; composite actions do not support those groups. Match their documented owners rather than every sequence named `steps` or `parallel`. A preceding background step has started, not necessarily completed. Preserve dependency order when proposing concurrency, and include `wait`, `wait-all`, and `cancel` operations in structural outlines.

An anchor on an edited mapping, sequence, or enclosing job can change every alias consumer. Preserve context-dependent edits to those definitions unless all consumers satisfy the correction. An unrelated anchor elsewhere does not invalidate a local proof. Resolve inherited settings per property: an alias in another setting cannot override a known literal. A step merge can supply an otherwise absent shell, while an explicit Bash shell overrides that merged value. Deleting an explicit mapping override can expose a merged value, so resolve that inherited value before removal. A singleton matrix axis is still a value consumed by matrix expressions, and cardinality alone proves no unnecessary wrapper.
