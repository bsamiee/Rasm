# [PY_API_LARK]

`lark` owns the EBNF grammar engine: a `Lark` parser folds a grammar string into a `Tree`/`Token` parse forest under an Earley/LALR/CYK algorithm and auto/basic/contextual/dynamic lexer, and a `Transformer`/`Visitor`/`Interpreter` family folds that forest to a typed value. Two branch rails author a grammar over it — runtime's transport filter compiles CESQL under `lalr` with an inline `transformer=` fold, and geometry's ifc-analysis owner compiles a selector query under `earley` before it reaches `ifcopenshell.util.selector.filter_elements`. Pure-Python, core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lark`
- package: `lark`
- import: `import lark`
- owner: `runtime` + `geometry`
- rail: cesql-grammar (runtime transport filter) / selector-grammar (geometry ifc-analysis)
- entry points: none (library); the standalone-parser CLI is `python -m lark.tools.standalone`
- capability: EBNF grammar definition, Earley/LALR/CYK parsing, contextual/dynamic lexing, `Transformer`/`Visitor`/`Interpreter` tree folds, typed-AST construction, error-recovery parsing, grammar composition, and parser caching/standalone generation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser, forest, and folds
- rail: cesql-grammar + selector-grammar

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]                                                                |
| :-----: | :------------------------------ | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Lark`                          | parser         | grammar-driven parser, algorithm/lexer/ambiguity/positions configurable     |
|  [02]   | `Tree` / `ParseTree`            | parse node     | a grammar-rule node with `data` and `children` (+ `meta` when positions on) |
|  [03]   | `Token`                         | terminal       | a matched terminal (`str` subclass) with `type`, line/column position       |
|  [04]   | `Transformer`                   | bottom-up fold | rewrite the tree bottom-up, one method per rule; merge via `\|` / chain     |
|  [05]   | `Transformer_NonRecursive`      | iterative fold | bottom-up fold without Python recursion (deep trees, no stack overflow)     |
|  [06]   | `Interpreter`                   | top-down fold  | `lark.visitors.Interpreter` — visit top-down with explicit `visit_children` |
|  [07]   | `Visitor` / `Visitor_Recursive` | tree visitor   | visit nodes in place without rewriting                                      |
|  [08]   | `v_args`                        | fold decorator | `inline=`/`meta=`/`tree=` — bind rule children to a transformer method      |
|  [09]   | `Discard`                       | fold sentinel  | return from a `Transformer`/`Visitor` method to drop the node from the tree |
|  [10]   | `UnexpectedInput`               | parse failure  | base of the lexer/parser error family below                                 |

[PUBLIC_TYPE_SCOPE]: failure family (`lark.exceptions`)
- rail: cesql-grammar + selector-grammar

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [CAPABILITY]                                                         |
| :-----: | :--------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `LarkError`            | root           | base of every lark error                                             |
|  [02]   | `GrammarError`         | grammar build  | malformed EBNF grammar — raised at `Lark(...)` construction          |
|  [03]   | `UnexpectedInput`      | parse failure  | base for parse-time failures; carries `pos_in_stream`, `get_context` |
|  [04]   | `UnexpectedToken`      | parser failure | a token the grammar did not expect (`expected`/`token` set)          |
|  [05]   | `UnexpectedCharacters` | lexer failure  | a character the lexer rejects                                        |
|  [06]   | `UnexpectedEOF`        | parser failure | input ended mid-rule                                                 |
|  [07]   | `VisitError`           | fold failure   | wraps any raise a `Transformer` method makes, inline fold included   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar construction
- rail: cesql-grammar + selector-grammar

`Lark` and the `open`/`open_from_package` factories carry the option family: `parser`(`earley`/`lalr`/`cyk`), `lexer`(`auto`/`basic`/`contextual`/`dynamic`), `ambiguity`(`resolve`/`explicit`/`forest`), `start`, `transformer` (inline fold during parse, LALR only), `postlex`, `propagate_positions`, `maybe_placeholders`, `keep_all_tokens`, `regex`, `cache`, `import_paths`.

| [INDEX] | [SURFACE]                                                           | [CALL_SHAPE]          | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------ | :-------------------- | :-------------------------------------- |
|  [01]   | `Lark(grammar, *, **options)`                                       | EBNF string + options | build a parser from a grammar           |
|  [02]   | `Lark.open(grammar_filename, rel_to=None, **options)`               | grammar file path     | build a parser from a `.lark` file      |
|  [03]   | `Lark.open_from_package(package, grammar_path, search_paths=("",))` | packaged grammar      | load a grammar shipped inside a package |
|  [04]   | `Lark.save(f, exclude_options=())` / `Lark.load(f)`                 | file object           | serialize/deserialize a built parser    |

[ENTRYPOINT_SCOPE]: parse and recovery
- rail: cesql-grammar + selector-grammar

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `parser.parse(text, start=None, on_error=None) -> ParseTree`           | input string  | parse into a `Tree`; `on_error` recovers    |
|  [02]   | `parser.parse_interactive(text=None, start=None) -> InteractiveParser` | input string  | incremental/interactive parse driver (LALR) |
|  [03]   | `parser.lex(text, dont_ignore=False) -> Iterator[Token]`               | input string  | run only the lexer stage                    |
|  [04]   | `parser.get_terminal(name) -> TerminalDef`                             | terminal name | look up a terminal definition               |

[ENTRYPOINT_SCOPE]: tree folds and typed AST
- rail: cesql-grammar + selector-grammar

| [INDEX] | [SURFACE]                                                    | [CALL_SHAPE]          | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------- | :-------------------- | :-------------------------------------------- |
|  [01]   | `Transformer().transform(tree)`                              | parse tree            | fold to a typed value, one method per rule    |
|  [02]   | `Transformer.__mul__` / `TransformerChain`                   | composition           | chain transformers with `t1 * t2`             |
|  [03]   | `@v_args(inline=True\|meta=True\|tree=True)`                 | method decorator      | bind children positionally / `meta` / `Tree`  |
|  [04]   | `Visitor().visit(tree)` / `Interpreter().visit(tree)`        | parse tree            | in-place visit / top-down interpret           |
|  [05]   | `CollapseAmbiguities().transform(tree)`                      | ambiguous tree        | expand `_ambig` forest into unambiguous trees |
|  [06]   | `ast_utils.create_transformer(ast_module, transformer=None)` | `Ast`-class module    | typed AST nodes (`Ast`/`AsList`/`WithMeta`)   |
|  [07]   | `Tree(data, children, meta=None)` / `tree.children`          | node                  | inspect or build a parse node                 |
|  [08]   | `tree.find_data(name)` / `tree.scan_values(pred)`            | node                  | search a node by rule / value                 |
|  [09]   | `Token(type, value)`                                         | terminal type + value | matched terminal (str subclass + position)    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `from lark import Lark, Token, Transformer, v_args` at MODULE scope, the branch's only admitted import form; a function-local import is what the manifest bans, and a `lazy` deferral suits a cold rail alone.
- grammar axis: one EBNF grammar string defines the vocabulary and is the closed language the validated result traces to; the algorithm (`earley` for ambiguous grammars, `lalr` for a layered precedence cascade whose every level is left-recursive) and the lexer (`contextual` only with LALR, `dynamic` only with Earley) are constructor knobs, never a parser-per-algorithm family, and `%import`/`open_from_package` compose shared sub-grammars rather than duplicating terminals. Two same-shaped terminals collide under the contextual lexer, so a grammar naming both a function and an attribute reads ONE terminal and separates them by the following token.
- fold axis: a `Transformer` subclass folds the `Tree` to a typed query value bottom-up, one method per grammar rule — table-driven rule dispatch, never an enumerated tree-walk. `@v_args(inline=True)` binds children positionally, `meta=True` threads positions, `Discard` drops a node; deep trees use `Transformer_NonRecursive`; a typed AST uses `ast_utils.create_transformer` over a module of `Ast`/`AsList` dataclasses. For LALR grammars the `transformer=` option folds inline during the parse pass.
- failure axis: a malformed query raises `UnexpectedInput` (`UnexpectedToken`/`UnexpectedCharacters`/`UnexpectedEOF`) at the parse boundary, lifted onto the runtime fault rail once, so an invalid query is rejected at admission rather than matching empty. `VisitError` wraps every raise an inline `transformer=` fold makes inside the parse, so a boundary fence naming `UnexpectedInput` alone lets each one escape unconverted. `GrammarError` raises at construction on a malformed grammar, and `parse(text, on_error=...)` collects partial-parse diagnostics without aborting on the first error.
- ambiguity axis: the `ambiguity` mode selects one derivation (`resolve`), an `_ambig`-wrapped forest `CollapseAmbiguities` expands (`explicit`), or the shared packed forest root (`forest`); the fold owner chooses one mode, never branches per derivation by hand.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): the folded typed query feeds `util.selector.filter_elements` — `lark` owns the string→validated `Tree`, the `Transformer` folds it into the typed selector structure, and that structure, not the raw string, is what reaches IfcOpenShell.
- within-lib: one module-level parser value compiles the grammar once and every call reuses it, so a per-call construction rebuilds the whole table. `save`/`load` and the `cache=` option serialize a built parser for `lalr` ALONE — every other algorithm raises `ConfigurationError` — and a cache file is a filesystem side effect a composition opts into rather than inherits; `python -m lark.tools.standalone` emits a dependency-free parser module when a grammar must ship without `lark` at runtime.

[RAIL_LAW]:
- Package: `lark`
- Owns: EBNF grammar definition, Earley/LALR/CYK parsing into a `Tree`/`Token` forest, contextual/dynamic lexing, `Transformer`/`Visitor`/`Interpreter` tree folds, typed-AST construction, error-recovery parsing, and parser caching/standalone generation
- Accept: a grammar plus a query string, feeding the validated structure into the runtime CESQL filter arm or the geometry ifc-analysis selector arm
- Reject: a hand-rolled regex/split query parser where the grammar owns the structure; a parser-per-algorithm function family over the `parser` knob; a parse-then-transform two-pass where the LALR `transformer=` option folds inline; an enumerated tree-walk where the `Transformer` rule-dispatch owns the fold; a boundary fence catching `UnexpectedInput` alone under an inline fold; `cache=` on any algorithm but `lalr`
