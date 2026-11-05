# [GRAMMARS]

Grammar syntax, lexical boundaries, and node shapes come from the selected parser. Use native grammar extension and build interfaces for missing syntax.

## [01]-[SELECTION]

Choose grammar capability from accepted source syntax:
- Read published grammar capability before extending a parser
- Preserve named nodes and fields, absence of `ERROR` or `MISSING` alone does not establish compatible parsing
- For ambiguous extensions, retain conflicts and prefer completed discriminating forms with dynamic precedence
- Embedded expressions need their source language's lexical context, brace-depth approximations lose that contract
- Injection configuration cannot mask arbitrary child expressions inside included source ranges

## [02]-[LEXING]

Native grammars allow at most one word token across composed languages:
- Derive shared and exclusive identifier languages from each owning grammar
- Constrain reserved contexts on shared-token aliases, inline role aliases when leaf kinds must stay unchanged
- Excluding keyword text from a regex can split the word into adjacent identifiers, preserve token boundaries
- Tree-sitter grammar escapes `\w`, `\s`, and `\d` expand as ASCII classes, rule regexes use separate Rust regex semantics
- Native normalization removes `u` and `v` before structural token deduplication, equivalent respellings can add lexer tokens
- Global extras affect every production, derive language-owned trivia instead of widening every grammar
- Foreign whitespace can become ordinary nodes, qualify ownership on the actual embedded construct rather than an outer group

## [03]-[SCANNERS]

Scanner composition preserves each external scanner's state and lifecycle:
- Select ownership through `valid_symbols` before consuming input, failed scanner calls can advance lexer state
- Preserve scanner lifecycle contracts, stateless upstream scanners need no added allocation or serialized state
- Derive scanner symbols and token counts from generated grammar metadata

## [04]-[BUILDS]

Native build dependencies include grammar sources and scanner implementations:
- Parser generation precedes compilation, generation failures end the command
- Native build metadata must name C++ scanner composition, generated `scanner.c` discovery alone does not register it
- Select scanner compatibility flags by compiler frontend and scope them to affected scanner translation units
- Derive build paths from source and target declarations, shell working directories do not define package layout
- Resolve package links to versioned source and include paths so path changes invalidate cached native objects
- Generate configs from owning registrations, publish with native atomic replacement, and cache and retain them with grammar libraries
- Keep formal token-language operations at build time, CLI and NAPI load one compiled grammar library
- Use grammar inheritance and supported upstream APIs instead of editing installed or generated dependency sources

## [05]-[NODE_SHAPES]

Tree-sitter recovers with `ERROR` or zero-width `MISSING` nodes, and its precedence can differ from the language parser's:
- Recovery nodes mark parser recovery, distinguish unsupported syntax from source errors through the language parser
- `kind: ERROR` and `ast-grep run -k ERROR -l <lang>` find the recovered form

| [INDEX] | [PYTHON]                         | [SHAPE]                                                                                           |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `except A, B:`                   | One `except_clause` with a `value` field per name, trailing comma included                        |
|  [02]   | `from __future__ import a, b`    | `future_import_statement` with one `name: dotted_name` per name                                   |
|  [03]   | `def f(x: int)`                  | `typed_parameter` with the name as its unfielded first `identifier` and `type` as the field       |
|  [04]   | `(v := a if b else c)`           | Tree-sitter nests `named_expression` in the first arm, Python binds the whole conditional         |
|  [05]   | `[i async for i in g]`           | `for_in_clause` with `async` as an unnamed child                                                  |
|  [06]   | `match x:` with `case C() if g:` | `body: block` over `alternative: case_clause`, fields `guard: if_clause` and `consequence: block` |
|  [07]   | `case Result()`                  | `class_pattern` with no `case_pattern` child                                                      |
|  [08]   | `Optional[X]`                    | `type` > `generic_type` > `type_parameter` in an annotation, `subscript` in a value position      |
|  [09]   | `[*items for items in groups]`   | `list_comprehension` with a `body: list_splat`, set and generator forms with the same `body`      |

Fixes emit walrus conditionals as `(v := (a if b else c))`.

| [INDEX] | [BASH]                           | [SHAPE]                                                                                               |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `[ x = y ]`, `[[ x = y ]]`       | `test_command` with the bracket unnamed over a `binary_expression` with `left`, `operator`, `right`   |
|  [02]   | `$(cmd)`, backticks              | `command_substitution` with the delimiter unnamed and one `command` child per simple command          |
|  [03]   | `echo $x`, `echo "$x"`           | `simple_expansion` under `command` when unquoted and under `string` when double-quoted                |
|  [04]   | `\$1`, `${10}`, `$@`, `$?`       | `variable_name` under `simple_expansion` or `expansion`, `special_variable_name` for `$@`, `$#`, `$?` |
|  [05]   | `${x#*/}`, `${x:-d}`             | `expansion` with `variable_name` and an `operator` child, the pattern a `regex` or `word` child       |
|  [06]   | `${x//a/$r}`                     | `expansion` with `operator` children `//` and `/`, the replacement follows the second `/`             |
|  [07]   | `cmd <<<"$x"`                    | `command` with `redirect: herestring_redirect`, the last stage's after a pipeline                     |
|  [08]   | `cmd > f`, `done < <(p)`         | `redirected_statement` with `body` and `redirect: file_redirect` wrapping the whole list or pipeline  |
|  [09]   | `for f in $(ls); do`             | `for_statement` with `variable`, `value: command_substitution`, and `body: do_group`                  |
|  [10]   | `(( x += 1 ))`, `(( x++ ))`      | `compound_statement` over a `binary_expression`, a `postfix_expression` for `x++`                     |
|  [11]   | `local -n r=\$1`, `declare -A m` | `declaration_command` with the keyword, option `word`, and `variable_name` or `variable_assignment`   |
|  [12]   | `f() { ...; }`                   | `function_definition` with `name: word` and `body: compound_statement`, `function` spelling included  |
|  [13]   | `if c; then a; else b; fi`       | `if_statement` with the condition, then statements, and `else_clause` as named children, no `then`    |

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
|  [03]   | `a ? b : c`, `a ?? throw e` alone   | `ERROR`, no statement form, expression patterns take `context: return <expr>;` with `selector`   |
|  [04]   | `extension<T>(Item<T> value) { }`   | `extension_declaration` in a `declaration_list` with `receiver_parameter` and `extension_body`   |

| [INDEX] | [XML]               | [SHAPE]                                                                                      |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `<Name>text</Name>` | Patterns bind whole elements, `<Name>$TEXT</Name>` captures the content                      |
|  [02]   | `Attribute="value"` | `AttValue` includes its quotes, a value that can hold the other quote matches each delimiter |

| [INDEX] | [SWIFT]                       | [SHAPE]                                                                                            |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | `if let a = b, c > 0 { }`     | `if_statement` holds each clause token in field `condition`, `field: condition` reads the first    |
|  [02]   | `lhs + receiver.member(args)` | `call_expression` over `additive_expression` with `rhs: navigation_expression`, call wraps the `+` |
|  [03]   | `if let a = unsafe b { }`     | `ERROR`, `unsafe` takes `b { }` as a trailing-closure call, `(unsafe b)` and `unsafe (b)` parse    |
|  [04]   | `f($A)` over `f(label: y)`    | `$A` binds whole `value_argument` with its label, `has: {field: value}` reads expression           |
|  [05]   | `items.map { $0.name }`       | `$0` stays literal, a captured shorthand parameter can take `regex: '^\$0$'`                       |

| [INDEX] | [SQL]                          | [SHAPE]                                                                                     |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `ifnull(a, b)` alone           | `ERROR`, expression patterns take `context: select <expr>` with `selector: function_call`   |
|  [02]   | `f(distinct a, b order by k)`  | `function_call` > `arguments: normal_function_arguments` > each argument, `order_by_clause` |
|  [03]   | `count(*) filter (where c)`    | `function_call` with `arguments: star_argument` and `filter: filter_clause`                 |
|  [04]   | `max(x) over (partition by p)` | `function_call` with `over: over_clause` > `window_definition`                              |
|  [05]   | `case x when w then v end`     | `case_expression` with fields `subject`, `when`, `then`, `else`, keywords unnamed           |
|  [06]   | `create view v as select ...`  | `create_view_statement` with `name: qualified_table_name` and `body: select_clause`         |
|  [07]   | `a not like 'p' escape '!'`    | `like_expression` with `operand`, `operator`, `pattern`, `escape` fields, `not` unnamed     |
|  [08]   | `.read f`, `:name`             | `dot_command` with `arguments: dot_command_arguments`, `bind_parameter`                     |
|  [09]   | `not a = b`                    | `binary_expression` with `left: unary_expression`, SQLite binds `not (a = b)`               |
|  [10]   | `a is not b`                   | `is_expression` with `right: unary_expression`, `is not distinct from` keeps `not` unnamed  |

- Top-level queries are `select_statement`, CTE, view, and subquery bodies `select_clause`
- `normal_function_arguments` holds `distinct` in unnamed field `modifier`, outside the arguments and `order_by_clause` `nthChild` counts
- Operator tokens of `binary_expression`, `unary_expression`, `like_expression`, and `is_null_expression` sit in field `operator`

| [INDEX] | [PGSQL]                                          | [SHAPE]                                                                                |
| :-----: | :----------------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `${name}` as a name or type                      | `ColId` or `Typename` > `substitution`, the hidden `_ident` slot                       |
|  [02]   | `${value}` as an operand                         | `c_expr` > `substitution`, `Sconst` > `substitution` in a string-constant position     |
|  [03]   | `a ${op} b`, `${from} ${op} ARRAY[...]`          | `a_expr` with `operator: qual_Op` > `substitution`, `left`/`right` or `operand`        |
|  [04]   | `(e)${cast}`                                     | `a_expr_prec` with a trailing `substitution` in place of `::` and `Typename`           |
|  [05]   | `FROM ${t}`, `${t} CROSS JOIN f(x) q`            | `table_ref` > `substitution`                                                           |
|  [06]   | `${source} ORDER BY ${keys}`                     | `select_no_parens` with `substitution` as its `select_clause` and `sort_clause`        |
|  [07]   | `WITH n AS (${query})`                           | `common_table_expr` with `preparable_stmt: substitution`                               |
|  [08]   | `INSERT INTO t ${rows}`, `UPDATE t SET ${pairs}` | `insert_rest` > `substitution`, `set_clause` > `substitution`                          |
|  [09]   | `ALTER TABLE ${t} ${commands}`                   | `AlterTableStmt` with one `substitution` for the relation and its commands             |
|  [10]   | `ALTER TABLE t ${command}`                       | `alter_table_cmd` > `substitution`, `ColConstraintElem` > `substitution` likewise      |
|  [11]   | `ADD CONSTRAINT ${name} ${body} NOT VALID`       | `TableConstraint` with one `substitution` and `ConstraintAttributeSpec`                |
|  [12]   | `ADD ${body} NOT VALID`                          | `ConstraintElem` with `substitution` and `ConstraintAttributeSpec`                     |
|  [13]   | `ADD COLUMN ${name} ${type} GENERATED ...`       | `columnDef` with one `substitution` for name and type, then `ColQualList`              |
|  [14]   | `${create} ON COMMIT DROP`                       | `CreateStmt` with a leading `substitution` and `OnCommitOption`                        |
|  [15]   | `COMMENT ON ${target} IS`, `COLUMN ${t}.${c}`    | `comment_target` > `substitution`, or the target alone as a fragment                   |
|  [16]   | `WITH (OLD AS ${o}) ${columns}`                  | `returning_list` fragment, `returning_clause` is `RETURNING` and `returning_list`      |
|  [17]   | `ALTER TEXT SEARCH CONFIGURATION ${n} ${cmd}`    | `AlterTSConfigurationStmt` with one `substitution`, `alter_ts_configuration_cmd` alone |

- Adjacent holes form one `substitution`, so a form whose consecutive members all take holes holds one `substitution` for the run
- A template whose text starts mid-statement parses as a fragment: `a_expr`, `target_el`, `table_ref`, `sortby`, `opt_on_conflict`, `alter_table_cmd`, `ColConstraintElem`, `TableConstraint`, `comment_target`, `returning_list`, `alter_ts_configuration_cmd`
- TSX template strings are the one composed host, C# holes reach `pgsql` through plain-string entries
