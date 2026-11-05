# [GRAMMARS]

Rules read only what a grammar parses, so a shape comes from parsing and a gap takes a grammar change derived from upstream rule objects.

## [01]-[SELECTION]

Parser choice follows the syntax the language's own parser accepts:
- Truth sets come from that parser (`tsc`, `swiftc -parse`, `javac`, `bash -n`, libpg_query), fidelity outranks node readability
- Grammars generated from a language's parser grammar keep its coverage, a shape layer over their rule objects fixes readability
- Dialects with their own keywords and statements take their own language, a dialect flag over one parser mixes the syntaxes
- Registering a second name over one library serves a language with different rules and outline, its catalog compiles beside the first
- Unextended grammars compile their published `src/parser.c`, extending one regenerates its whole table
- `LANGUAGE_VERSION` in `src/parser.c` fixes a parser's ABI, a fork built for its own runtime loads in no stock tree-sitter
- Facts only scope decides (C# `field` inside an accessor) stay out of grammars, a separate kind duplicates every production reaching it

## [02]-[EXTENSION]

`grammar(base, options)` inherits one base, and `base.grammar` exposes its rules as the plain objects `src/grammar.json` holds:
- Overrides take `($, original)` and edit `original`, a restated rule misses each later upstream change, installed sources stay unedited
- Selection goes by predicate (string value, symbol name, field name), positional reads (`members[1]`) change meaning after an upgrade
- Transforms compose at one point, an explicit override under a transformed key discards the transform unless it receives its result
- `externals`, `extras`, `conflicts`, `inline`, `supertypes`, `precedences`, and `reserved` sets take `original`, `name` stays required
- Rules unreachable from the start rule, `word`, `extras`, `externals`, or `reserved` drop silently with config entries naming them
- `alias(seq(a, b), $.x)` aliases each member into sibling `x` nodes, aliasing a hidden rule yields one node
- `field('x', seq(a, b))` labels every child, tokens included, and marks the field `multiple`
- Rules in both `inline` and `supertypes` lose the supertype, extensions filter `supertypes` by the base's `inline`
- Ambiguities keep a `conflicts` entry and prefer the completed discriminating form through `prec.dynamic`, `prec` decides at generation
- Conflict sets come from the generator's report, each set it reports unnecessary goes
- Aliasing a frequent leaf to a rare kind at one role gives rules and outlines a cheap anchor

Shape layers over generated grammars run as ordered walks over rule objects, with no table of names:
1. Keywords rendered as named nodes become unnamed aliases, arms in identifier positions alias to the identifier kind
2. Rules with one symbol per alternative become hidden supertypes
3. Operator sequences take `left`, `operator`, `right`, or `operator`, `operand`
4. Remaining single references to hidden rules take a field from the rule name, a visible rule's kind already states its role
5. Optional wrappers absent from `conflicts` join `inline`

## [03]-[LEXING]

Lexing runs before parse precedence and considers only tokens valid in the parse state:
- Ties resolve through `token(prec(n, …))`, longest match, string over regex, then rule order, `prec` outside `token` never reaches the lexer
- Longest match beats parse context when both tokens are valid (`<<` over `<`), a scanner token with lookahead separates them
- Grammar `\w`, `\s`, `\d` expand textually to `[0-9A-Za-z_]`, `[\t-\r ]`, `[0-9]`, rule regexes use Rust's Unicode classes
- Grammar patterns reject anchors, `\b`, and look-around, `new RustRegex('<pattern>')` takes Rust syntax with inline flags
- Regex literals keep `i` and drop `u` and `v`, other flags report a diagnostic, `i` folds without `ſ` or the Kelvin sign
- Token identity is structural, respellings (`[ab]`, `[a-b]`) add competing lexer tokens
- Under `word`, a new string literal stops parsing as an identifier everywhere, `alias('<literal>', $.identifier)` restores each position
- First `reserved` set applies in every state, `reserved(<set>, $.identifier)` replaces it at one position
- Excluding keyword text from an identifier regex lexes it as adjacent identifiers (`i`, `f`), `reserved` sets keep the word whole
- `extras` applies between tokens of every production, whitespace comes from the language's specification (Rust `Pattern_White_Space`)
- Complex extras take a named rule, an inline pattern copies into many lex states
- Identifier classes follow the language's lexer, mapping automaton transitions widens a narrow upstream class and keeps its token shape
- Terminators optional at end of input take `choice(';', eof())`, an unterminated copy of each statement restates it
- ASCII-only identifiers force an ASCII `expandoChar`, literal names spelled `<char><UPPER>` then read as captures

## [04]-[SCANNERS]

External scanners run first at each position where the parse state holds an external token, before the generated lexer skips extras:
- Leading whitespace reaches the scanner, `advance(lexer, true)` skips it before a token starts
- `scan` returning false resets position and falls to the generated lexer, nothing resets between calls a wrapper makes inside one `scan`
- `valid_symbols` selects ownership before consuming input, scanners cannot backtrack
- Recovery calls mark every external valid (row 1 of `ts_external_scanner_states`), a trailing unused sentinel in `externals` detects them
- Zero-width tokens with no state change drop during recovery, before the parser advances past an error, and when marked extra
- `deserialize` runs before each scan and `serialize` after each token, all state fits `TREE_SITTER_SERIALIZATION_BUFFER_SIZE`
- Deep nesting overflows that buffer, a scanner bounds what it stores
- `mark_end` fixes the token end before lookahead reads further
- `eof` decides end of input, `lookahead` 0 is also a valid character
- `isw*` functions follow the process locale, character tests read code point ranges
- Stateless upstream scanners need no added allocation or serialized state
- Token order and counts derive from `externals` in the generated `src/grammar.json`
- `ts_external_scanner_states` in `src/parser.c` lists each state's valid externals, a row mixing components is a state dispatch must serve

Extension scanners wrap one upstream scanner by including its source:
- Each upstream function it wraps renames to `<name>_upstream_<fn>` before the include, the rest point at `SCANNER(fn)`
- Own five go through one macro, `SCANNER(fn)` defaulting to `tree_sitter_<name>_external_scanner_##fn`, a composite redefines it per unit
- Renames go through `push_macro`/`pop_macro` or function-like macros, an `#undef` erases a command-line rename
- Function-like renames need parentheses at each own definition (`bool (SCANNER(scan))(…)`), a bare form re-expands into the rename
- Appended externals continue upstream's token enum after its last member, in the order `grammar.js` appends them to `externals`

Composite scanners run their components as separate translation units:
- Shared static names (`advance`, `skip`, `TokenType`) break one unit, one unit per component needs no rename beyond the ABI
- Upstream scanners take `-D tree_sitter_<name>_external_scanner_<fn>=<name>_component_<fn>` per function
- Extension scanners take `-DSCANNER(fn)=<name>_component_##fn`
- Components can share one state, choosing one owner starves the others' tokens
- Dispatch offers a scan to each component holding a valid non-extra external in composition order, extras when no non-extra is valid
- Wrapper lexers record commitment, a refusal before a consuming `advance` or `mark_end` passes to the next, a committed refusal ends the call
- Whitespace a component skips commits nothing, dispatch skips the remaining whitespace and offers once more when every component refuses
- Dispatch shifts `valid_symbols` and `result_symbol` by the scanning component's offset
- Composites hold at most one stateful component, which receives the whole buffer while stateless components receive `NULL`

## [05]-[COMPOSITION]

ast-grep hands an injected parser one contiguous range per match, holes reach it as source, and only host lexing parses them:
- Hosts with holes rules read compose into the dialect's grammar, every other host injects plain source
- Brace-depth approximations of a hole lose the host's lexical context (strings, nested templates, comments)
- Host rules rename under one prefix across `SYMBOL`, named `ALIAS`, string `PREC`, and `RESERVED` context values
- Components share one `word`, boolean regions over their identifier languages supply it, each identifier a `choice` of its regions
- Reserved contexts sit on shared-token aliases, role aliases inline when leaf kinds must stay unchanged
- Entries resolve hidden symbols under the host literal and replace each `REPEAT` reaching the hole, delimiters stay host members
- Holes take one kind over every host's hole, `repeat1` folds adjacent holes no production slots apart, so one node can fill more than one role
- Hole slots widen named fields and symbols, harvested from real templates with each hole replaced by a marker
- Mid-statement templates parse through `source_file` arms over upstream rule names, restated fragments lose upstream forms
- Fragment arms open behind the host's delimiter, a document injected as plain text holds whole statements alone
- Overlapping arms resolve to one tree whatever the site meant (PostgreSQL `` `a DESC` `` as a labelled `target_el`, never `sortby`), a rule reads the printed tree
- Fragment arms led by another fragment make their leading optional mandatory
- Host extras overlapping another component's token prefixes drop (Python `#` against TSX `#private`)
- Host strings prefixing a dialect trivia opener (`--`) take `token(prec(1, …))`
- Surviving host trivia aliases to neutral kinds, foreign whitespace becomes nodes that rules counting children skip
- Languages nested inside an injected document stay opaque, embedding at grammar level is the one route

## [06]-[PACKAGES]

Automata run while `grammar.js` evaluates, libraries hold the resulting patterns, and CLI and NAPI load one compiled grammar:

| [INDEX] | [WORK]             | [PACKAGE]                                                | [REPLACES]               |
| :-----: | :----------------- | :------------------------------------------------------- | :----------------------- |
|  [01]   | Grammar base       | `tree-sitter-<language>` `grammar.js`, `src/`            | Copied rules             |
|  [02]   | Language algebra   | `refa` `DFA.fromIntersection`, `complement`, `prefixes`  | Regex string surgery     |
|  [03]   | Overlap decision   | `refa` `isDisjointWith`                                  | Literal special cases    |
|  [04]   | Pattern output     | `refa` `toRegex`, `JS.toLiteral`                         | Concatenated regex text  |
|  [05]   | Character classes  | `refa` `CharSet`                                         | Range arithmetic         |
|  [06]   | Unicode properties | `@unicode/unicode-<version>` `ranges.mjs`                | Transcribed range tables |
|  [07]   | JavaScript regexes | `refa` `JS.Parser` over `@eslint-community/regexpp`      | Regex source parsers     |
|  [08]   | Regex facts        | `regexp-ast-analysis` first character, length, emptiness | Walks over regex source  |

- `grammar(base, …)` takes one base and no package merges grammars, composition walks rule objects over these packages
- Extensions read a grammar package's `grammar.js` and `src/` alone, its Node binding stays unbuilt
- `refa` `JS.Parser` holds older Unicode tables than the runtime, property classes come from a data package or the runtime's `RegExp`
- `JS.Parser` rejects Rust-only syntax (inline flags, POSIX classes, `&&`), a parser for it targets the AST `NFA.fromRegex` takes

## [07]-[BUILDS]

Native builds take grammar sources, scanner implementations, and their imports as inputs:
- Targets derive from the transitive imports of each `grammar.js`, a hand-kept row restates them
- Sibling grammar imports add that build as a dependency and its scanner as a component, upstream imports add their `src/scanner.c`
- Empty `externals` in the generated `grammar.json` means no scanner
- `tree-sitter build -o <lib> <dir>` compiles one grammar root with `src/scanner.c`, per-unit defines take one build system over every grammar
- Each build configures, generates, and compiles in `<language>/$NX_TASK_HASH-$NX_INVOCATION_ANCESTOR_PIDS`, so no cached variable outlives a changed `grammar.js`
- `NX_TASK_HASH` alone collides, concurrent `nx` processes missing the cache run one task under one hash, the ancestor pids add the process
- Generated `grammar.json` of a grammar's own scanner reads from its work folder, a published copy belongs to an earlier build
- Outputs publish after the build through `cmake -E rename` on one volume, overwriting a mapped library can crash running scans
- Libraries compile with hidden visibility and export `tree_sitter_<name>` alone
- Compatibility flags follow the compiler frontend and scope to affected scanner units
- GNU takes `-fno-strict-aliasing` for `tree_sitter/array.h` casts, scanners widening `char` take `-funsigned-char` or `/J`
- Build paths derive from source and target declarations, shell working directories do not define package layout
- Package links resolve to versioned source and include paths so path changes invalidate cached native objects
- Generated configs derive from owning registrations and cache with their libraries

`tree-sitter generate` fails above 65,535 parse actions or states, and actions reach the limit first:
- Each composed host and fragment adds actions, hosts beyond the limit inject plain source
- Last `ts_parse_actions` index in `src/parser.c` reads the count, a proxy omitting fragments and slots understates it
- `--report-states-for-rule <rule>` attributes states, factoring a `choice` can cost more than per-alternative forms
- Generation memory and time follow the table, the largest composite sets a build host's memory floor, `--no-parser` skips the table

## [08]-[NODE_SHAPES]

Shapes read from the configured library, first command prints a tree with unnamed tokens, second a kind's contract, third each parent admitting a kind:

```bash
tree-sitter parse --no-ranges --cst -l <lib> --lang-name <name> <file>
jq -c '.[] | select(.type == "<kind>") | {fields, children, subtypes}' <src>/node-types.json
jq -r --arg kind <kind> '.[] | select([(.fields // {})[].types[], (.children.types // [])[]] | any(.type == $kind)) | .type' <src>/node-types.json
```

Contract keys decide a rule's form:
- `multiple: true` marks a repeated field (Java `declarator`, Python `subscript`)
- `named: false` under a field marks a token field (Swift `declaration_kind`, TypeScript `operator`)
- `subtypes` marks a supertype, `kind: <supertype>` loads and matches nothing, the rule lists its subtypes
- Hidden `_name` rules hold no kind, `kind: _name` fails the load

Trees depart from source structure in recurring classes:

| [INDEX] | [CLASS]                 | [EXAMPLE]                                                   | [RULE_FORM]                    |
| :-----: | :---------------------- | :---------------------------------------------------------- | :----------------------------- |
|  [01]   | One kind, many forms    | Swift `class_declaration` for `struct`, `enum`, `extension` | Test the token field           |
|  [02]   | One concept, many kinds | Python `generic_type` in annotations, `subscript` in values | Kind list per position         |
|  [03]   | Capture binds a wrapper | C# `argument` with its name, Swift `value_argument`         | Read the value inside          |
|  [04]   | Definition a level down | Python `decorated_definition`, YAML `block_node`            | Relation through the wrapper   |
|  [05]   | Flat where source nests | Swift `#if` `directive`, Java `switch_label`                | Sibling relation               |
|  [06]   | Opaque leaf             | Rust `token_tree`, PL/pgSQL `sql_expression`                | `regex` over text or injection |
|  [07]   | Left-nested repetition  | Python `binary_operator`, list rules of generated grammars  | `has` with `stopBy`, no `$$$`  |
|  [08]   | Single-child chain      | Expression rules of generated grammars                      | Util over the chain kinds      |
|  [09]   | Literal holds delimiter | Python `string_start` text `t"`, YAML `double_quote_scalar` | Content child, else `regex`    |
|  [10]   | Precedence differs      | Python `(v := a if b else c)`, Rust `a..*b`                 | Grammar override, parentheses  |
|  [11]   | Syntax read as a call   | Bash `time git push` as `command` named `time`              | Grammar override               |
