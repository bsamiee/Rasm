# [PY_GEOMETRY_API_LARK]

`lark` supplies the parsing-grammar engine the IfcOpenShell selector and IDS grammars are built on: an EBNF-defined `Lark` parser producing a `Tree`/`Token` parse forest, an Earley/LALR algorithm selector, and a `Transformer`/`Visitor` fold over the parse tree. The geometry ifc-analysis owner uses it to author and validate a typed selector/filter-query grammar — turning a free-form element-selection string into a validated, structured query before it reaches `util.selector.filter_elements` — rather than passing an unvalidated query string straight through. Pure-Python, cp315-clean.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lark`
- package: `lark`
- import: `import lark`
- owner: `geometry`
- rail: ifc-analysis / selector-grammar
- installed: `1.3.1`, pure-Python wheel, cp315-clean core (no native dependency)
- entry points: none (library only)
- capability: EBNF/context-free grammar definition, Earley and LALR(1) parsing, contextual lexing, `Tree`/`Token` parse forest, `Transformer`/`Visitor`/`Interpreter` tree folds, grammar composition via `%import`, and standalone-parser code generation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser and tree
- rail: selector-grammar

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                                                             |
| :-----: | :---------------- | :------------- | :----------------------------------------------------------------------- |
|   [1]   | `Lark`            | parser         | grammar-driven parser over a string, configurable algorithm              |
|   [2]   | `Tree`            | parse node     | a grammar rule node with `data` and `children`                           |
|   [3]   | `Token`           | terminal       | a matched terminal with `type` and string value                          |
|   [4]   | `Transformer`     | bottom-up fold | rewrite the tree bottom-up, one method per rule                          |
|   [5]   | `Visitor`         | tree visitor   | visit nodes in place without rewriting                                   |
|   [6]   | `v_args`          | fold decorator | configure how rule children bind to a transformer method                 |
|   [7]   | `UnexpectedInput` | parse failure  | the lexer/parser error family (`UnexpectedToken`/`UnexpectedCharacters`) |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar definition and parse
- rail: selector-grammar

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]                | [CAPABILITY]                       |
| :-----: | :---------------------------------------------- | :-------------------------- | :--------------------------------- |
|   [1]   | `Lark(grammar, *, start, parser='earley', ...)` | EBNF string plus start rule | build a parser from a grammar      |
|   [2]   | `Lark.open(grammar_filename, ...)`              | grammar file path           | build a parser from a `.lark` file |
|   [3]   | `parser.parse(text, start=None) -> Tree`        | input string                | parse a string into a `Tree`       |
|   [4]   | `Transformer().transform(tree)`                 | parse tree                  | fold the tree to a typed value     |
|   [5]   | `Tree(data, children)` / `tree.children`        | rule name plus nodes        | inspect or build a parse node      |
|   [6]   | `Token(type, value)`                            | terminal type plus value    | a matched terminal                 |

## [4]-[IMPLEMENTATION_LAW]

[GRAMMAR_TOPOLOGY]:
- import: `import lark` at boundary scope only; module-level import is banned by the manifest import policy.
- grammar axis: one EBNF grammar string defines the selector/filter vocabulary; the parser algorithm (`earley` for ambiguous grammars, `lalr` for fast unambiguous ones) is a constructor knob, never a parser-per-algorithm family. The grammar is the closed query vocabulary the validated query traces to.
- fold axis: a `Transformer` subclass folds the `Tree` to a typed query value bottom-up, one method per grammar rule — the table-driven dispatch over rule names, never an enumerated tree-walk. `v_args(inline=True)` binds rule children as positional arguments to the fold method.
- failure axis: a malformed query raises `UnexpectedInput` (`UnexpectedToken`/`UnexpectedCharacters`) at the parse boundary, lifted into the runtime fault rail once — so an invalid selector is rejected before it reaches `filter_elements`, never a silent empty match.
- boundary: `lark` owns grammar definition and parsing; a hand-rolled regex or split-based query parser is the deleted form where the grammar owns the structure; the parsed query feeds `ifcopenshell.util.selector.filter_elements`, never a second selection engine.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lark`
- Owns: EBNF grammar definition, Earley/LALR parsing into a `Tree`/`Token` forest, and `Transformer` tree folds to typed query values
- Accept: a selector/filter-query grammar and a query string, feeding the validated structured query into the ifc-analysis selector arm
- Reject: a hand-rolled regex/split query parser where the grammar owns the structure; a parser-per-algorithm function family over the `parser` knob

[CAPTURE_GAP]:
- floor: `lark==1.3.1` is pure-Python and cp315-clean, so reflection resolves on the project venv directly — no companion-lane gate
- members: `Lark`/`Tree`/`Token`/`Transformer`/`v_args`/`UnexpectedInput` and the `parse`/`transform` entries confirm by introspection against the installed cp315 distribution; every documented type and entry resolves — no phantom
