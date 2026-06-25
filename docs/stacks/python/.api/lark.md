# [PY_GEOMETRY_API_LARK]

`lark` supplies the parsing-grammar engine the IfcOpenShell selector and IDS grammars are built on: an EBNF-defined `Lark` parser producing a `Tree`/`Token` parse forest, an Earley/LALR/CYK algorithm selector with contextual/dynamic lexing, a `Transformer`/`Visitor`/`Interpreter` family of tree folds, an `on_error` recovery hook plus `parse_interactive` for incremental parsing, grammar composition via `%import`, parser caching/serialization, and `ast_utils.create_transformer` for typed-AST construction. The geometry ifc-analysis owner authors a typed selector/filter-query grammar that turns a free-form element-selection string into a validated structured query before it reaches `util.selector.filter_elements`, rather than passing an unvalidated string straight through. Pure-Python, cp315-clean.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lark`
- package: `lark`
- import: `import lark`
- owner: `geometry`
- rail: ifc-analysis / selector-grammar
- installed: `1.3.1`; pure-Python `py3-none-any` wheel, no native dependency, cp315-clean core; license `MIT`
- entry points: none (library); the standalone-generator CLI is `python -m lark.tools.standalone`
- members: introspected against the installed cp315 distribution; the top-level `Lark`/`Tree`/`ParseTree`/`Token`/`Transformer`/`Transformer_NonRecursive`/`Visitor`/`v_args`/`Discard` exports, the `lark.exceptions` failure family, the `lark.visitors` `Interpreter`/`Visitor_Recursive`/`TransformerChain`/`CollapseAmbiguities`, and the `lark.ast_utils` typed-AST helpers resolve against live signatures — no phantom
- capability: EBNF/context-free grammar definition, Earley/LALR(1)/CYK parsing, auto/basic/contextual/dynamic lexing, ambiguity resolution (`resolve`/`explicit`/`forest`), `Tree`/`Token` parse forest with position propagation, `Transformer`/`Visitor`/`Interpreter` tree folds, error-recovery `on_error` hook and `parse_interactive`, grammar composition (`%import`/`open_from_package`), parser save/load caching, standalone-parser generation, and typed-AST construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser, forest, and folds
- rail: selector-grammar

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :------------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Lark`                     | parser             | grammar-driven parser, algorithm/lexer/ambiguity/positions configurable     |
|  [02]   | `Tree` / `ParseTree`       | parse node         | a grammar-rule node with `data` and `children` (+ `meta` when positions on) |
|  [03]   | `Token`                    | terminal           | a matched terminal (`str` subclass) with `type`, line/column position       |
|  [04]   | `Transformer`              | bottom-up fold     | rewrite the tree bottom-up, one method per rule; merge via `\|` / chain      |
|  [05]   | `Transformer_NonRecursive` | iterative fold     | bottom-up fold without Python recursion (deep trees, no stack overflow)     |
|  [06]   | `Interpreter`              | top-down fold      | `lark.visitors.Interpreter` — visit top-down with explicit `visit_children` |
|  [07]   | `Visitor` / `Visitor_Recursive` | tree visitor  | visit nodes in place without rewriting                                      |
|  [08]   | `v_args`                   | fold decorator     | `inline=`/`meta=`/`tree=` — bind rule children to a transformer method      |
|  [09]   | `Discard`                  | fold sentinel      | return from a `Transformer`/`Visitor` method to drop the node from the tree |
|  [10]   | `UnexpectedInput`          | parse failure      | base of the lexer/parser error family below                                 |

[PUBLIC_TYPE_SCOPE]: failure family (`lark.exceptions`)
- rail: selector-grammar

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [CAPABILITY]                                                       |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `LarkError`            | root             | base of every lark error                                          |
|  [02]   | `GrammarError`         | grammar build    | malformed EBNF grammar — raised at `Lark(...)` construction        |
|  [03]   | `UnexpectedInput`      | parse failure    | base for parse-time failures; carries `pos_in_stream`, `get_context` |
|  [04]   | `UnexpectedToken`      | parser failure   | a token the grammar did not expect (`expected`/`token` set)        |
|  [05]   | `UnexpectedCharacters` | lexer failure    | a character the lexer could not match                              |
|  [06]   | `UnexpectedEOF`        | parser failure   | input ended mid-rule                                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar construction
- rail: selector-grammar

`Lark` options drive the closed query vocabulary: `parser`(`earley`/`lalr`/`cyk`), `lexer`(`auto`/`basic`/`contextual`/`dynamic`), `ambiguity`(`resolve`/`explicit`/`forest`), `start`, `transformer` (inline fold during parse, LALR only), `postlex`, `propagate_positions`, `maybe_placeholders`, `keep_all_tokens`, `regex`, `g_regex_flags`, `cache`, `import_paths`.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]                | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------------- | :-------------------------- | :------------------------------------------------ |
|  [01]   | `Lark(grammar, *, start='start', parser='earley', lexer='auto', ambiguity='auto', transformer=None, propagate_positions=False, maybe_placeholders=True, ...)` | EBNF string + options | build a parser from a grammar |
|  [02]   | `Lark.open(grammar_filename, rel_to=None, **options)`          | grammar file path           | build a parser from a `.lark` file                |
|  [03]   | `Lark.open_from_package(package, grammar_path, search_paths=("",), **options)` | packaged grammar | load a grammar shipped inside a package           |
|  [04]   | `Lark.save(f, exclude_options=())` / `Lark.load(f)`            | file object                 | serialize/deserialize a built parser (cache)      |

[ENTRYPOINT_SCOPE]: parse and recovery
- rail: selector-grammar

| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE]            | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------- | :---------------------- | :---------------------------------------------------- |
|  [01]   | `parser.parse(text, start=None, on_error=None) -> ParseTree`      | input string            | parse into a `Tree`; `on_error(UnexpectedInput)->bool` recovers |
|  [02]   | `parser.parse_interactive(text=None, start=None) -> InteractiveParser` | input string        | incremental/interactive parse driver (LALR)           |
|  [03]   | `parser.lex(text, dont_ignore=False) -> Iterator[Token]`          | input string            | run only the lexer stage                              |
|  [04]   | `parser.get_terminal(name) -> TerminalDef`                        | terminal name           | look up a terminal definition                         |

[ENTRYPOINT_SCOPE]: tree folds and typed AST
- rail: selector-grammar

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]            | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------- | :---------------------- | :---------------------------------------------------- |
|  [01]   | `Transformer().transform(tree)`                               | parse tree              | fold the tree to a typed value, one method per rule    |
|  [02]   | `Transformer.__mul__` / `TransformerChain`                    | transformer composition | chain transformers with `t1 * t2`                      |
|  [03]   | `@v_args(inline=True\|meta=True\|tree=True)`                   | method decorator        | bind children positionally / pass `meta` / pass `Tree` |
|  [04]   | `Visitor().visit(tree)` / `Interpreter().visit(tree)`         | parse tree              | in-place visit / top-down interpret with manual descent |
|  [05]   | `CollapseAmbiguities().transform(tree)`                       | ambiguous tree          | expand an `_ambig` forest into a list of unambiguous trees |
|  [06]   | `ast_utils.create_transformer(ast_module, transformer=None)`  | module of `Ast` classes | build a `Transformer` that instantiates typed AST nodes (`Ast`/`AsList`/`WithMeta`) |
|  [07]   | `Tree(data, children, meta=None)` / `tree.children` / `tree.find_data(name)` / `tree.scan_values(pred)` | node | inspect or build a parse node, search by rule/value |
|  [08]   | `Token(type, value)`                                          | terminal type + value   | a matched terminal (str subclass, carries position)   |

## [04]-[IMPLEMENTATION_LAW]

[GRAMMAR_TOPOLOGY]:
- import: `import lark` at boundary scope only; module-level import is banned by the manifest import policy.
- grammar axis: one EBNF grammar string defines the selector/filter and IDS vocabulary; the parser algorithm (`earley` for ambiguous grammars, `lalr` for fast unambiguous ones, `cyk` rarely) and the lexer (`contextual` only with LALR, `dynamic` only with Earley) are constructor knobs, never a parser-per-algorithm family. The grammar is the closed query vocabulary the validated query traces to; `%import` and `open_from_package` compose shared sub-grammars rather than duplicating terminal definitions.
- fold axis: a `Transformer` subclass folds the `Tree` to a typed query value bottom-up, one method per grammar rule — the table-driven dispatch over rule names, never an enumerated tree-walk. `@v_args(inline=True)` binds rule children as positional arguments; `meta=True` threads position info; `Discard` drops a node from the result. Deep trees use `Transformer_NonRecursive` to avoid recursion limits; a typed AST uses `ast_utils.create_transformer` over a module of `Ast`/`AsList` dataclasses so the fold target is a typed node hierarchy, not bare tuples. For LALR grammars the `transformer=` option folds inline during the parse pass (one traversal, not parse-then-transform).
- failure axis: a malformed query raises `UnexpectedInput` (`UnexpectedToken`/`UnexpectedCharacters`/`UnexpectedEOF`) at the parse boundary, lifted into the runtime fault rail once — so an invalid selector is rejected before it reaches `filter_elements`, never a silent empty match. A malformed grammar raises `GrammarError` at `Lark(...)` construction (build-time, not parse-time). `parse(text, on_error=...)` supplies an error-recovery callback for partial-parse/diagnostic collection without aborting on the first error.
- ambiguity axis: `ambiguity='resolve'` (default) picks the simplest derivation; `'explicit'` returns an `_ambig`-wrapped forest that `CollapseAmbiguities` expands to candidate trees; `'forest'` returns the shared packed parse forest root — the fold owner chooses one mode, never branches per derivation by hand.

[STACKING_LAW]:
- the parsed query feeds `ifcopenshell.util.selector.filter_elements` (and the IDS facet vocabulary): `lark` owns turning the string into a validated `Tree`, the `Transformer` folds it into the typed selector/filter structure, and that structure — not the raw string — is what reaches IfcOpenShell. A hand-rolled regex/split query parser is the deleted form where the grammar owns the structure.
- a built parser serializes via `save`/`load` (or the `cache=` option) so the grammar is compiled once and the boundary reuses the parser handle across queries rather than rebuilding it per call; the standalone generator (`python -m lark.tools.standalone`) emits a dependency-free parser module when the grammar must ship without `lark` at runtime.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lark`
- Owns: EBNF grammar definition, Earley/LALR/CYK parsing into a `Tree`/`Token` forest, contextual/dynamic lexing, `Transformer`/`Visitor`/`Interpreter` tree folds, typed-AST construction, error-recovery parsing, and parser caching/standalone generation
- Accept: a selector/filter-query and IDS grammar plus a query string, feeding the validated structured query into the ifc-analysis selector arm
- Reject: a hand-rolled regex/split query parser where the grammar owns the structure; a parser-per-algorithm function family over the `parser` knob; a parse-then-transform two-pass where the LALR `transformer=` option folds inline; an enumerated tree-walk where the `Transformer` rule-dispatch owns the fold

[CAPTURE_GAP]:
- floor: `lark==1.3.1` is pure-Python `py3-none-any`, license `MIT`, cp315-clean, so reflection resolves on the project venv directly — no companion-lane gate
- members: verified by introspection against the installed cp315 distribution; the `Lark`/`Tree`/`ParseTree`/`Token`/`Transformer`/`Transformer_NonRecursive`/`Visitor`/`v_args`/`Discard` top-level exports, the `lark.exceptions` family, the `lark.visitors` `Interpreter`/`Visitor_Recursive`/`TransformerChain`/`CollapseAmbiguities`, the `parse(on_error=)`/`parse_interactive`/`lex`/`save`/`load`/`open_from_package` entries, and the `lark.ast_utils.create_transformer` typed-AST helper all resolve — no phantom (`Interpreter` and `create_transformer` live under `lark.visitors`/`lark.ast_utils`, not the top-level `lark` namespace)
