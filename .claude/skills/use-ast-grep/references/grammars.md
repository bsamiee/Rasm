# [GRAMMARS]

Rules read only what a grammar parses, a shape comes from parsing and a gap takes a grammar change derived from upstream rule objects.

## [01]-[SELECTION]

Parser choice follows the syntax the language's own parser accepts, fidelity outranks node readability:

| [INDEX] | [NEED]                                    | [FORM]                                 | [COST]               |
| :-----: | :---------------------------------------- | :------------------------------------- | :------------------- |
|  [01]   | Upstream parses every accepted form       | Compile published `src/parser.c`       | One C compile        |
|  [02]   | Package holds `grammar.json` alone        | `generate --output <dir> grammar.json` | Table build          |
|  [03]   | Upstream misses or misreads forms         | `grammar(base, …)` override            | Whole table rebuilt  |
|  [04]   | Generated grammar with unreadable kinds   | Ordered walks over rule objects        | Whole table rebuilt  |
|  [05]   | Dialect with its own keywords, statements | Own language, rules, and catalog       | One more library     |
|  [06]   | Other rules over one parse                | Second name over one library           | One more catalog     |
|  [07]   | Embedded source without host holes        | Injection into its parser              | One parse per region |
|  [08]   | Embedded source with host holes           | Hole token over the dialect            | Actions per slot     |

- `grammar(base, …)` takes one base and no package merges grammars, a dialect gains hosts through hole tokens alone
- Dialect flags over one parser mix syntaxes, rules hold one `language` and kinds differ across dialects
- `LANGUAGE_VERSION` in `src/parser.c` fixes a parser's ABI, tree-sitter loads 13 through 15 and generates 14 or 15, a fork built for its own runtime loads in no stock runtime
- Facts only scope decides (C# `field` inside an accessor) stay out of grammars, a separate kind duplicates every production reaching it
- Sets try each new literal at every identifier position

## [02]-[PIPELINE]

`tree-sitter generate` evaluates `grammar.js` under `dsl.js` and writes `src/grammar.json`, the plain objects `base.grammar` exposes:

| [INDEX] | [TYPE]                            | [FIELDS]                     | [WRITTEN]                                 |
| :-----: | :-------------------------------- | :--------------------------- | :---------------------------------------- |
|  [01]   | `STRING`                          | `value`                      | `'x'`                                     |
|  [02]   | `PATTERN`                         | `value`, `flags?`            | `/x/`, `new RustRegex('x')` (no `flags`)  |
|  [03]   | `SYMBOL`                          | `name`                       | `$.x`, a `GrammarSymbol` copying as plain |
|  [04]   | `BLANK`, `EOF`                    | None                         | `blank()`, `eof()`                        |
|  [05]   | `SEQ`, `CHOICE`                   | `members`                    | `optional(x)` is `CHOICE[x, BLANK]`       |
|  [06]   | `REPEAT`, `REPEAT1`               | `content`                    | `repeat(x)`, `repeat1(x)`                 |
|  [07]   | `TOKEN`, `IMMEDIATE_TOKEN`        | `content`                    | `token(x)`, `token.immediate(x)`          |
|  [08]   | `FIELD`                           | `name`, `content`            | `field('n', x)`                           |
|  [09]   | `ALIAS`                           | `content`, `named`, `value`  | `alias(x, $.n)`, `alias(x, 'n')`          |
|  [10]   | `PREC`, `PREC_LEFT`, `PREC_RIGHT` | `value` int or name, content | `prec('n', x)`                            |
|  [11]   | `PREC_DYNAMIC`                    | `value` int, `content`       | `prec.dynamic(1, x)`                      |
|  [12]   | `RESERVED`                        | `context_name`, `content`    | `reserved('n', x)`                        |

Extension semantics:
- `rules` and `name` are required, `grammar(base, {name})` throws
- Overrides take `($, original)`, `original` is `undefined` for a new rule, new rules append, the start rule changes only by overriding the first
- `conflicts`, `inline`, `supertypes` receive `GrammarSymbol` arrays, `extras`, `precedences`, `externals`, `reserved` sets receive rule objects, `word` receives none
- `base.grammar` objects are shared by reference, a walk copies (`{...rule}`) and copies pass `normalize`
- `seq` and `choice` hold one member unflattened, a one-member `choice` of a string warns, a hidden empty-matching rule fails
- `$.name` of an undefined rule throws in `normalize`, `alias(x, $.undefined)` accepts it and creates a named kind, `externals` take any name
- `console.log` is safe, only the last stdout line reads as JSON

Prepare passes run in order:
1. Validate precedences: `Undeclared precedence 'x' in rule 'y'`, `Conflicting orderings for precedences 'a' and 'b'`
2. Intern symbols: a start rule stays visible, supertypes turn hidden
3. Extract tokens: strings, regexes, and `token()` hoist to tokens deduped by structure, a rule wholly one token used once becomes that token
4. Expand repeats: identical bodies share one auxiliary rule, a hidden rule topped by `repeat1` drops out of `inline`, a nullable repeat at end of input fails
5. Flatten: nested `choice` becomes a product of productions, `eof()` survives as a last step alone and reduces only on end of input
6. Reject unit recursion: `Grammar contains an indirectly recursive rule: a -> b -> a`
7. Expand tokens to NFAs
8. Extract default aliases: a symbol always aliased takes its most frequent alias globally, context aliases are absent inside `ERROR`, default ones stay
9. Process inlines: a rule matching empty fails, the outer alias and field copy onto every inserted step

- `--no-parser` runs every pass and stops before tables, `parser.c` depends on `--abi` and a `tree-sitter.json` `metadata.version` found walking up parents
- `generate --output <dir> <grammar.json>` writes `parser.c`, `node-types.json`, and `tree_sitter/`, no `grammar.json`
- `TREE_SITTER_JS_RUNTIME` names a runtime, `native` fails on CommonJS upstream grammars and on packages it cannot resolve

Debug flags and logs:

| [INDEX] | [COMMAND]                                     | [PRINTS]                                                              |
| :-----: | :-------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `generate --log`                              | Each keyword and recovery-token decision with its conflict            |
|  [02]   | `generate --report-states-for-rule '*'`       | Per rule state counts, items, and lookaheads, help's `-` a summary    |
|  [03]   | `generate --json-summary`                     | `{diagnostics, error}` on stderr, one conflict per run                |
|  [04]   | `generate --disable-optimizations`            | Table without state merging, the gap is the merge's gain              |
|  [05]   | `parse --no-ranges --cst`                     | Tree with unnamed tokens, `MISSING: ";"` the plain form hides         |
|  [06]   | `parse -d`                                    | `lex_external state:<row>`, `lexed_lookahead`, `recover_with_missing` |
|  [07]   | `parse -D`                                    | `log.html` with stack graphs                                          |
|  [08]   | `parse --edits "<row>,<col> <del> <text>" -d` | Node reuse after an edit                                              |
|  [09]   | `test -p <root>`                              | Corpus cases under `<root>/test/corpus`, `-u` writes trees            |

- Corpus delimiters are `===` of length 3 or more around a name, `---` before the tree, attributes (`:skip`, `:error`, `:cst`) sit below the name
- `tree-sitter.json` requires `grammars` and `metadata`, each grammar `name` and `scope`, `external-files` joins the loader's recompile check

## [03]-[EXTENSION]

Overrides edit `original`, a restated rule misses each later upstream change, installed sources stay unedited:
- Selection goes by predicate (string value, symbol name, field name), positional reads (`members[1]`) change meaning after an upgrade
- Transforms compose at one point, an explicit override under a transformed key discards the transform unless it receives its result
- Interface files (`.d.ts`, `.pyi`, `.swiftinterface`) drop bodies, one edit makes the body optional in every body-bearing rule, a missed form swallows the rest of the file
- Rules unreachable from the start rule, `word`, `extras`, `reserved`, or a nonterminal external drop silently with their whole `conflicts` set and `precedences` row, the row loss failing as `Undeclared precedence`
- `externals` names matching a rule keep it as the lexer's fallback when the scanner returns false, a renamed external aliased to the kind drops it
- `alias(seq(a, b), $.x)` aliases each member into sibling `x` nodes, aliasing a hidden rule yields one node
- `field('x', seq(a, b))` and a field over a hidden `seq` rule label every child, tokens included, and mark the field `multiple`
- Aliases onto an existing kind share its id, `{kind: binary}` returns real and aliased nodes, restricted copies stay invisible to rules
- Rules in both `inline` and `supertypes` lose the supertype, extensions filter `supertypes` by the base's `inline`
- Visible-named supertypes vanish from trees (TSX `statement`, `expression`, Java `primary_expression`, C# `lvalue_expression`, Python `pattern`)
- Aliasing a frequent leaf to a rare kind at one role gives rules and outlines a cheap candidate kind
- Start rules accepting a lone expression (`choice(original, prec(-1, $._expression))`) parse pattern snippets with no recovery cost

Restricted copies narrow a position upstream makes over-permissive:

```js
const {expr, try_expr} = base.grammar.rules;
const [operator] = try_expr.content.members.filter(({type}) => type === 'STRING');
const atoms = expr.members.filter(({name}) => name !== 'call' && name !== 'try_expr');
export default grammar(base, {
    name: 'block_headed',
    conflicts: ($, original) => [...original, [$.expr, $._headed_try]],
    rules: {
        if_stmt: ($, original) => seq(...original.members.map((part) => (part.name === 'condition' ? field('condition', choice(part.content, alias($._headed_try, $.try_expr))) : part))),
        _headed_try: ($) => seq(operator, field('operand', choice(...atoms, alias($._headed_try, $.try_expr)))),
    },
});
```

Base `call: prec(2, seq(field('callee', $.expr), field('trailing', $.block)))` sits beside `if_stmt`, `{` reads as a trailing block:

```text
base if try b { }      (ERROR (try_expr operand: (call callee: (id) trailing: (block))))
ext  if try b { }      (if_stmt condition: (try_expr operand: (id)) then: (block))
ext  if try try b { }  (if_stmt condition: (try_expr operand: (try_expr operand: (id))) then: (block))
ext  if b { }          (if_stmt condition: (id) then: (block))
```

Generated grammars take ordered walks over rule objects, with no table of names:
1. Keywords rendered as named nodes become unnamed aliases
2. Rules with one symbol per alternative become hidden supertypes
3. Operator sequences take `left`, `operator`, `right`, or `operator`, `operand`
4. Remaining single references to hidden rules take a field from the rule name, a visible rule's kind already states its role
5. Optional wrappers absent from `conflicts` join `inline`

Walk 1 decides pattern matching: an unnamed terminal compares kind alone and a named one compares text, `SELECT $A;` under `alias(/select/i, 'SELECT')` matches `select b;`. In its form `rules` holds the base's rule objects and `named(name)` tests a keyword rule:

```js
const rewrite = (rule, apply) => apply({...rule, ...(rule.members && {members: rule.members.map((part) => rewrite(part, apply))}), ...(rule.content && {content: rewrite(rule.content, apply)})});
const keyword = (rule) => (rule.type === 'SYMBOL' && named(rule.name) ? alias(rules[rule.name], rules[rule.name].value.toLowerCase()) : rule);
export default grammar(base, {
    name: 'walked',
    rules: Object.fromEntries(Object.keys(rules).filter((name) => !named(name)).map((name) => [name, (_, original) => rewrite(original, keyword)])),
});
```

After the walk `kw_as` stops appearing as a node and `col_id` holds `"as"`.

## [04]-[CONFLICTS]

Generation stops at the first conflict, each message names its fork:

```text
Unresolved conflict for symbol sequence:

  identifier  •  ';'  …

Possible interpretations:

  1:  (expr  identifier)  •  ';'  …
  2:  (type  identifier)  •  ';'  …

Possible resolutions:

  1:  Specify a higher precedence in `expr` than in the other rules.
  2:  Specify a higher precedence in `type` than in the other rules.
  3:  Add a conflict for these rules: `expr`, `type`
```

- `•` marks the dot, `)  •` reduces, `•` inside parentheses shifts, annotations read `(precedence: …, associativity: …)` and `(reduces only at end of input)`
- Rule lists appear under precedence only with more than one parent, associativity only when precedences tied

Static precedence decides at generation:
- Step precedence governs the gap after a step, `seq($.e, '+', prec.left(1, $.e))` works, `seq(prec.left(1, $.e), '+', $.e)` does nothing
- Reduce/reduce resolves eagerly by precedence, shift/reduce compares the step before the dot, associativity decides a tie all reducing rules agree on
- Named precedence against a nonzero integer compares `Equal`, `prec.left(1, …)` beside `prec.left('mul', …)` generates silently and parses `a+b*c` as `(a+b)*c`
- `precedences` lists accept rule symbols, `[[$.mul, $.add]]` orders whole rules
- Static precedence resolves before `conflicts` reads, a set whose items precedence decides forks nothing and arrives as an `unnecessary conflicts` warning, each such set goes

Conflict entries fork at runtime and match exactly:
- `conflicts` entries equal the sorted, deduped parent set, repeat auxiliaries resolve to their parents, `[a, t]` does not cover `[a, t, u]`, one new arm invalidates an entry
- Conflict sets come from `--json-summary`, rerun until generation passes
- Repeat-only conflicts resolve on their own through `is_repetition`

Forks resolve at a later reduce popping more than one path, in order:
1. Lower error cost
2. Higher dynamic precedence: largest magnitude on a production's path, subtrees sum children (`1+2+2` beats `3`, `1+1` loses to it)
3. Lower symbol id, which follows rule declaration order and not `choice` order, reordering rules changes a tie

- `prec.dynamic` on a unit arm of a hidden rule does nothing, it decides on a competing rule or multi-step production
- Start rule arms take no `prec.dynamic`, accept rebuilds the root from its children's sum, the competing non-root rule takes the opposite sign
- Versions merge on equal state, position, error cost, and external state
- Stacks cap at 6 versions and drop the least promising silently, stacked ambiguities at one position lose readings
- Forks stop node reuse, reductions during them mark fragile, a conflict set at a frequent position costs incremental parsing

Generic against comparison under the generator's sets, the completed form winning:

```js
export default grammar(base, {
    name: 'generic',
    conflicts: ($, original) => [...original, ...reported.map((set) => set.map((name) => $[name]))],
    rules: {
        call: ($, original) => {
            const parts = original.content.members;
            const callee = parts.find(({name}) => name === 'function');
            return prec(original.value, seq(field('function', choice(callee.content, $.generic)), ...parts.filter((part) => part !== callee)));
        },
        generic: ($) => prec.dynamic(1, seq(field('name', $.id), '<', field('argument', $.type_name), '>')),
        type_name: ($) => $.id,
    },
});
```

```bash
tree-sitter generate --json-summary grammar.js 2>&1 | jq -c '.error.BuildTables.Conflict.possible_resolutions[]? | .AddConflict.symbols // empty'
```

```text
base f<a>(x);  (binary left: (binary left: (id) right: (id)) right: (paren (id)))
ext  f<a>(x);  (call function: (generic name: (id) argument: (type_name (id))) (args (id)))
ext  f<a;      (binary left: (id) right: (id))
ext  f<a>b;    (binary left: (binary left: (id) right: (id)) right: (id))
```

## [05]-[TABLE_COST]

`tree-sitter generate` fails at 65,535 actions with `Parse table action count <n> exceeds maximum value of 65535`, above that with ``State count `<n>` exceeds the max value 65535.``, and actions reach it first:
- Upstream grammars hold about 2 actions per state, a last `ts_parse_actions` index in `src/parser.c` reads the count
- Each fragment and hole position adds actions, a proxy omitting them understates the count
- Dense rows go to states with more than `min(64, symbols/2)` entries plus states 0 and 1, each dense row costs `SYMBOL_COUNT` × 2 bytes

Generation time and memory follow distinct tokens ahead of state count, each keyword arm adds a token:
- `optional`s multiply per production, a `seq` of `n` holds `2^n` productions, `repeat(choice(...))` over the same words holds one per word
- Factoring a `choice` inline generates byte-identical tables, a hidden rule equal ones, a hidden rule holding fields adds production ids and states
- Item identity holds precedence at the dot, dynamic precedence, completed aliases and fields, and remaining steps, each difference can split states
- Hidden children with fields put preceding symbols into item identity
- Minimizing removes a hidden unit reduction with no alias, field, or supertype role, those wrappers are free
- State merging refuses a new external or lexically conflicting token, external-heavy states merge less

Keyword categories under identifier rules (`ColId: IDENT | unreserved_keyword`) set a generated grammar's table cost, each member joining the closure of every state reaching the rule. A name needs no arm where its keyword form is invalid, identifier rules replace their category arms with computed ones:
- Sets start from identifier rules accepting no other identifier rule's whole set, holding their categories less wide ones (`unreserved_keyword`)
- Arms belong where a keyword's own form and that identifier open in one LR(0) state
- Closure walks over rule objects find those states with no table, keyword, category, and identifier rules stay closed
- Arms make their keyword valid in each state holding the rule, whose identifier rules take each keyword they accept or refuse it, until no set grows
- Each identifier rule's arms sit in one hidden rule aliased to the identifier kind, each member aliased to its spelling

## [06]-[LEXING]

Lexing runs before parse precedence and considers only tokens valid in the parse state:
- Ties resolve through `token(prec(n, …))`, longest match, string over regex, then rule order, `prec` outside `token` never reaches the lexer, `prec(5, token('ab'))` included
- Implicit precedence is string 2, regex 0, `token.immediate` +1, named precedence is ignored in tokens
- Token precedence drops a longer transition once a higher token completes, `token(prec(1, 'ab'))` on `abc` gives `ab` and an `ERROR` over `c`
- Longest match beats parse context when both tokens are valid (`<<` over `<`), a scanner token with lookahead separates them
- Token identity is structural, respellings (`[ab]`, `[a-b]`) add competing tokens where the earlier always wins and the later rule goes dead

Regex dialect:
- Grammar `\w`, `\s`, `\d` (and `\W`, `\S`, `\D`) rewrite textually to `[0-9A-Za-z_]`, `[\t-\r ]`, `[0-9]` under any flag, `RustRegex` included, an escaped backslash too (`/q\\w/` rejects `q\w`)
- `regex_syntax` parses patterns, `\0`, an unescaped `[` in a class, and JS surrogate pairs are rejected, `\p{XID_Start}` and `\p{Pattern_White_Space}` compile with no flag
- Anchors, `\b`, look-around, and back-references fail as `Assertions are not supported`, `new RustRegex('<pattern>')` takes Rust syntax with inline flags
- Non-greedy quantifiers read as greedy (`/<.*?>/` spans `<a> b>`), first-close forms take a scanner
- Regex literals keep `i` and drop `u` and `v`, other flags report a diagnostic, `i` folds without `ſ` or the Kelvin sign
- Regexes matching empty pass `generate` and hang the parser (`/a*/` on `b`)
- Unnamed `token.immediate(/x/)` inside a rule prints no node, it reads through a rule or alias

Keywords under `word` come from a capture the generator proves:
- Candidates open with a letter or `_`, match a string `word` matches, match nothing `word` rejects, and beat `word` by token precedence or an earlier token index
- Candidates shadowing another or changing conflict status drop, keywords first written in a rule after `word` take `token(prec(1, …))`
- One regex overlapping `word` and no candidate can exclude every keyword (`/while/i` beside `/[a-z]+/`)
- Respelling splits a literal, `'x'`, `token(prec(1, 'x'))`, and `token.immediate('x')` are separate tokens under one kind, only one stays a keyword
- Runtime lexes `word` first and reruns a keyword lexer, a keyword applies on identical end and a valid action or reserved status
- `letx` stays one identifier and `let inside` keeps `inside` whole, without `word` it gives `in` and an `ERROR` over `side`
- Keywords collapse to `word` in lex-state token sets and multiply no lex states, they sit in a separate `ts_lex_keywords`
- Keywords arriving invalid with `word` valid relabel at runtime, `parse -d` logs `switch from_keyword`, a reserved word reads as a name there (PostgreSQL `CREATE TABLE select (a int)`)
- Grammars without `word` (Swift) hold no keyword table and no `reserved`

```js
word: ($) => $.identifier,
rules: {
    source_file: ($) => repeat($._s),
    _s: ($) => choice($.let, $.ref),
    let: ($) => seq('let', $.identifier, '=', $.identifier, ';'),
    ref: ($) => seq($.identifier, ';'),
    identifier: () => /[a-z]+/,
},
```

```text
'let let = let;'  0:4 - 0:7 identifier `let`   0:10 - 0:13 identifier `let`
'let;'            ERROR  "let" ";"     both valid at statement start, the keyword wins
'letter;'         ref  identifier `letter`
```

Reserved sets refuse a word the state would otherwise take:
- First `reserved` set applies in every state, `reserved(<set>, $.identifier)` names another where its step is the `word` token, inlining drops it from spliced steps
- States take the highest-indexed set among items whose next step is the word, a set reaching only states that hold an item of its own production
- Entries must exist as tokens (`Reserved word 'for' must be a token`), `--abi 14` drops sets silently
- Excluding keyword text from an identifier regex lexes it as adjacent identifiers (`i`, `f`), `reserved` sets keep the word whole
- Contextual words join `_reserved_identifier` (Java, TSX, Rust, C#), `keyword_identifier` (Python), `_contextual_simple_identifier` (Swift)

Extras apply between tokens of every production, `token.immediate` and the end of a nonterminal extra excepted:
- Whitespace comes from the language's specification (Rust `Pattern_White_Space`), identifier classes from its lexer
- Inline extras prefix every non-immediate token's NFA and yield no node, an inline pattern equal to a token becomes that symbol
- Named extras add an action to every parse state and yield a node
- Nonterminal extras need unambiguous endings (`Extra rules must have unambiguous endings`), an extra matching empty warns
- Nonterminal extras multiply generation, nested comments take a same-named external with the internal token as its fallback
- Optional final terminators take `choice(';', eof())` where a document ends at end of input, before a host closer an unterminated copy per statement

## [07]-[SCANNERS]

External scanners run first at each position where the parse state holds an external token, before the generated lexer skips extras:
1. `deserialize` from the last external token, with `(NULL, 0)` when none exists, it must reset state
2. `scan`, a false return resets position and falls to the generated lexer
3. `serialize` only when a token is found
4. Generated lexing fails, the mode switches to the error state and repeats, one unrecognized character skips per loop with the scanner re-entered

- Scan count exceeds token count, `create` runs per parse and `destroy` at reset, all five functions link, `create` can return `NULL`
- `valid_symbols` selects ownership before consuming input, scanners cannot backtrack, nothing resets between calls a wrapper makes inside one `scan`
- `result_symbol` resets to 0 at start, `advance` returns without moving at end of input, `while (lookahead != X) advance` hangs
- `advance(lexer, true)` resets the token start even mid-token, skipping goes before a token starts
- `mark_end` fixes the token end before lookahead reads further, with no call the token ends at the final position
- `mark_end` before `skip` makes a zero-width token at the mark, the whitespace becomes the next token's padding (Python `_newline`, TSX `_automatic_semicolon`)
- `eof` decides end of input, `lookahead` 0 can be a valid character
- Bytes read count toward lookahead and widen which edits invalidate the token, `get_column` (code points less a BOM) adds column-shifting edits
- `log` writes under a logger alone and shows in `parse -d`

Recovery and zero-width tokens:
- Row 1 of `ts_external_scanner_states` marks every pure external valid, an external naming an internal token holds it only if that token enters the error state
- Unused sentinels anywhere in `externals` detect recovery (Rust `_error_sentinel`, Bash and TSX `__error_recovery`), a sentinel stays a pure external
- Impossible valid pairs detect it with no sentinel (Python `STRING_CONTENT && INDENT`)
- Zero-width tokens with no state change drop during recovery, before the parser advances past an error, and when marked extra
- Externals in `extras` hold row validity in every state, the scanner runs at every token and that bit holds no state
- Literals in `externals` give the scanner their validity to read and are never emitted, a false return lets the lexer produce them (TSX `||`, Python `except`)

State decides parse cost:
- Stateful scanners deserialize per scan, equal serialized state is required for stack merge and node reuse
- State above 24 bytes copies to the heap per external leaf, one edit changing state for every later token loses reuse of all of them
- `serialize` writes into a 1024-byte buffer (`TREE_SITTER_SERIALIZATION_BUFFER_SIZE`), the length check compiles out under `NDEBUG`, overflow corrupts the lexer silently, deep nesting bounds what it stores
- State belongs in a scanner only when the close depends on the open form and the parse stack cannot hold it

```js
extras: () => [/[ \t\r\n]/],
externals: ($) => [$._eos, $.error_sentinel],
rules: {
    source_file: ($) => repeat($.stmt),
    stmt: ($) => seq($.id, optional($.num), $._eos),
    id: () => /[a-z]+/,
    num: () => /[0-9]+/,
},
```

```c
bool tree_sitter_lines_external_scanner_scan(void *payload, TSLexer *lexer, const bool *valid) {
    if (valid[ERROR_SENTINEL] || !valid[EOS]) return false;
    while (lexer->lookahead == ' ' || lexer->lookahead == '\t') lexer->advance(lexer, true);
    if (lexer->eof(lexer) || lexer->lookahead == '\n' || lexer->lookahead == ';') {
        if (lexer->lookahead == ';') lexer->advance(lexer, false);
        lexer->mark_end(lexer);
        lexer->result_symbol = EOS;
        return true;
    }
    return false;
}
```

`parse -d` over `a 1\nb;\nc` and a malformed `a 1 2`, the row number naming each state's valid set, a scanner with no sentinel logging `ignore_empty_external_token` in recovery:

```text
lex_external state:2, row:0, column:3
lexed_lookahead sym:_eos, size:0
lex_external state:2, row:1, column:1
lexed_lookahead sym:_eos, size:1
detect_error lookahead:num
lex_external state:1, row:0, column:5
```

`generate` writes growable array macros to `src/tree_sitter/array.h` beside `parser.h`, hand-kept arrays restate them.

Upstream scanners show the forms a language's lexical state takes:

| [INDEX] | [GRAMMAR] | [STATE]                                            | [TECHNIQUE]                                                         |
| :-----: | :-------- | :------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | TSX, JS   | None, template nesting sits on the parse stack     | `_template_chars` valid only inside, ASI scans past comments        |
|  [02]   | Python    | Indent stack, delimiter flags, interpolation flag  | Tab counts 8, dedent suppressed in brackets, deferred past comments |
|  [03]   | C#        | Interpolation stack of dollar, brace, quote counts | `LAMBDA_PAREN_OPEN` runs its own unbounded lookahead                |
|  [04]   | Bash      | Heredoc stack with delimiter text                  | `_concat` zero-width between adjacent word parts                    |
|  [05]   | Rust      | One opening hash count                             | `float_literal` external so `1.max()` and `1..2` back out           |
|  [06]   | Swift     | One raw hash count, no stack                       | Newline scan lexes the next operator, `_implicit_semi` refused      |
|  [07]   | XML       | Tag-name stack                                     | Mismatch returns false, `ERROR` root, `_ErroneousETag` unreachable  |
|  [08]   | Postgres  | None, a 64-byte local tag inside one call          | `$tag$…$tag$` scanned whole                                         |
|  [09]   | PL/pgSQL  | None, one external per terminator set              | Parse state picks the terminator, fragments alias to opaque text    |
|  [10]   | SQLite    | None                                               | `malformed_*` nodes for bad literals, clean ones refuse             |

Each stateful row truncates or loses its stack at the buffer: Python caps delimiters at 255, C# returns 0, XML caps names at 255 bytes. Swift's single count breaks a raw string inside a raw interpolation.

Extension scanners wrap one upstream scanner by including its source:
- Each upstream function it wraps renames to `<name>_upstream_<fn>` before the include, the rest keep their ABI names
- Renames go through `push_macro`/`pop_macro` or function-like macros, an `#undef` erases a command-line rename
- Function-like renames need parentheses at each own definition (`bool (tree_sitter_<name>_external_scanner_scan)(…)`), an unparenthesized form re-expands into the rename
- Upstream `deserialize` reading `const char *` sign-extends bytes above `0x7f`, a wrapper storing high bits renames it and reads `const unsigned char *`
- Appended externals continue upstream's token enum after its last member, order and counts derived from `externals` in the generated `src/grammar.json`
- `isw*` follows the process locale, the include sees tests over code point spans the build writes from `word` (XML `WORD_START` and `WORD_CHARACTER` in `grammar.h`)
- Stateless upstream scanners need no added allocation or serialized state
- Continuation keywords of a Swift newline scan sit in the scanner's tables, a new one enters there

## [08]-[ERRORS]

Costs select among recoveries, 500 per recovery, +110 per missing token, +100 per skipped tree, 30 per line, 1 per byte:
- `handle_error` first tries one `MISSING` token whose next state can reduce on the lookahead, lowest symbol first, externals included
- Printable `MISSING` leaves come from the parse table, limited per state to lookaheads its external lex state lets the scanner emit
- Failing that, the parser pushes the error state and recovers to a prior state or skips, junk characters print as `(ERROR (ERROR))` with text
- Inside `ERROR` no field labels exist, a rule reading fields misses nodes there

Included ranges back ast-grep injections:
- `ts_parser_set_included_ranges` refuses unordered or overlapping ranges, an empty list means the whole input
- `advance` hops to the next range mid-token, a token can span a gap, `eof` is true only past the last range
- `mark_end` at a range start ends at the previous range's end, `get_column` snaps to the first included byte on its line
- Changed ranges block node reuse, the CLI holds no flag for ranges

## [09]-[COMPOSITION]

Hole expressions belong to the host language's own parse, merging a host grammar repeats that parse for a larger table, a hole scans as opaque opening and closing tokens. Hosts injecting plain source take no composition.

```js
const forms = [{opening: '${', closing: '}'}];
export default grammar(base, {
    name: 'composite',
    externals: ($, original) => [...original, $._hole, $._hole_end],
    conflicts: ($, original) => [...original, [$.substitution]],
    rules: {
        substitution: ($) => prec.dynamic(1, repeat1(seq($._hole, $._hole_end))),
        _constants: () => constants({HOLE_FORMS: forms.map(({opening, closing}) => `{${JSON.stringify(opening)}, '${closing}'}`).join(', ')}),
        source_file: ($, original) => choice(original, seq('`', choice(base.grammar.rules.source_file, $._statement, $._expression), '`')),
        _name: ($, original) => choice(original, prec(1, $.substitution)),
    },
});
```

Holes span an opening and a closing external token, so one `MISSING` token recovery inserts never completes a hidden `substitution`. The `[$.substitution]` conflict folds adjacent holes (`${a} ${b}`) into one substitution that slots read as one value, `prec.dynamic` on `substitution` giving each hole its node only where the arms reading them apart tie on static precedence and fork.

Rules read a hole by kind and the host path through injection, byte offsets stay absolute:

```yaml
languageInjections:
    - hostLanguage: typescript
      injected: composite
      rule:
          kind: template_string
          pattern: $CONTENT
          inside: {kind: arguments, inside: {kind: call_expression, has: {field: function, regex: '^q$'}}}
```

```text
{"ruleId":"hole","text":"${b + 1}","file":"a.ts","language":"composite","bytes":{"start":20,"end":28}}
```

Grammar side:
- Hosts declare `documents` of string or regex `opening` and `closing`, and one `hole` of string delimiters
- Hole scans count the opening's last character against a one-character `closing`
- Hidden `_hole` and `_hole_end` are the last externals, the dialect's token enum and `valid_symbols` stay as upstream states them, `substitution` is the one visible kind
- Host forms reach C through the hidden `_constants` rule of name and value pairs, `constants()` writes it, generation drops it, `src/grammar.json` keeps it for the build's `grammar.h`
- `source_file` takes one arm opening on any host's document opener and closing on any closer around the dialect's `source_file` and fragment arms
- Fragment arms name upstream rules and open behind a host's delimiter, restated fragments lose upstream forms
- Fragments led by another fragment alias their rule with the leading optional mandatory
- Tails no upstream rule names (a `RETURNING` list) become rules by splitting arms at their shared prefix and suffix around the new rule
- Hole slots, found by parsing real templates with each hole replaced by a marker, widen a whole rule (`choice(original, prec(<level>, $.substitution))`) or its named fields and symbols
- Arms reading a slotted rule after a name (`func_name Sconst`) take the rule's original choice, a slot there reading every `name ${hole}` as that arm
- Whole-rule levels count the slotted rules a rule reaches, outer rules win a hole their inner rule accepts
- Named slots rank above every whole-rule level, a list reaching a slot takes the hole in place of the list, labeled by the field the list holds
- Positions reading a hole as one kind against a whole-rule slot take a hidden `_<kind>_hole: prec(<level>, $.substitution)` aliased to that kind, an inline `alias(prec(…))` adds no production and loses
- Holes standing for a run of production members (`` `UPDATE t ${set} WHERE a` ``) take an arm copying the production with that run replaced
- Operator holes take infix and prefix arms one static level below the loosest operator, a whole-rule operator slot shifts every later hole
- Overlapping arms resolve to one tree whatever the site meant (PostgreSQL `` `a + 1` `` as `sortby_list`, `` `${x}` `` as `columnDef`), a rule reads the printed tree
- Slots accept a hole only as the construct it stands for, a cast tail taking a trailing-clause hole prints a wrong tree in place of a failure
- Documents injected as plain text hold whole statements alone, languages nested inside one stay opaque, embedding at grammar level is the one route

Scanner side, one wrapper over the dialect scanner:
- Dialect `create`, `destroy`, `serialize`, `deserialize` take the composite's ABI names by `-D`, its `scan` becomes a `dialect_scan` symbol, the wrapper owns `scan` alone
- Separate translation units keep shared static names (`advance`, `skip`, `TokenType`) apart
- `valid_symbols[<hole>]` false hands the call to the dialect, true skips ASCII whitespace and the first form sharing the lookahead decides it
- Openings that fail partway (`$` before a dollar-quote tag) replay to the dialect through a wrapper lexer serving the consumed bytes ahead of the source
- Hole scans walk delimiters past strings, nested template holes, and comments, a delimiter inside a regex literal counts
- Opening tokens end before the closing delimiter, closing tokens hold it and a glued word tail (`${t}_log`)
- Glued words that are whole dialect keywords stay apart (`${opt}INDEX`), `HOLE_WORDS` holding each word of every finite token `word` also matches, the set keyword extraction takes

## [10]-[PACKAGES]

Automata run while `grammar.js` evaluates, libraries hold the resulting patterns, and CLI and NAPI load one compiled grammar:

| [INDEX] | [WORK]             | [PACKAGE]                                                  | [REPLACES]               |
| :-----: | :----------------- | :--------------------------------------------------------- | :----------------------- |
|  [01]   | Grammar base       | `tree-sitter-<language>` `grammar.js`, `src/`, no binding  | Copied rules             |
|  [02]   | Language algebra   | `refa` `DFA.fromIntersection`, `complement`, `prefixes`    | Regex string surgery     |
|  [03]   | Overlap decision   | `refa` `isDisjointWith`, `getIntersectionWords`            | Literal special cases    |
|  [04]   | Pattern output     | `refa` `toRegex` through a Rust printer                    | Concatenated regex text  |
|  [05]   | Character classes  | `refa` `CharSet`                                           | Range arithmetic         |
|  [06]   | Unicode properties | `@unicode/unicode-<version>` `ranges.mjs`                  | Transcribed range tables |
|  [07]   | JavaScript regexes | `refa` `JS.Parser`, `@eslint-community/regexpp` for an AST | Regex source parsers     |
|  [08]   | Rust regexes       | `regex_syntax` `ast` spans, `hir::translate` per class     | Regex source parsers     |
|  [09]   | Regex facts        | `regexp-ast-analysis`, no automaton built                  | Walks over regex source  |

- Native `\p{…}` passes the Rust parser, a plain Unicode class needs no `refa`, algebra over it does, a printed `\p{ID_Start}` runs thousands of characters

`regex_syntax` reads both sides of a class comparison:
- Rule regexes parse through `ast::parse::Parser`, a `Visitor` tracks each group's `(?i)` and `(?u)` flags, `TranslatorBuilder` translates each class under them with `utf8(false)` keeping byte classes
- Grammar tokens take the generator's `\w`, `\s`, `\d` rewrite before that parse, `i` sets `case_insensitive`, and only `word` and `extras` tokens cross
- Token HIR lowers to a `refa` AST, a minimal DFA's initial-state transitions give identifier start, final-to-final transitions the identifier class
- `extras` tokens with every transition inside their opening characters give the whitespace class, comment tokens hold others and state none

`refa` automata:
- `complement`, `minimize`, `prefixes`, `suffixes`, `reverse` mutate and return void, DFA holds no `suffixes`, `reverse`, `union`, `append`, `fromRegex`, NFA no `complement` or `minimize`
- `maxCharacter` is 65535 without `u` and 1114111 with it, mixing throws `MaxCharacterError`
- `isDisjointWith` takes NFA, DFA, and ENFA mixed, subset is `isDisjointWith(a, complement(b))`, equality minimizes both and calls `structurallyEqual`
- `getIntersectionWords` is lazy and shortest-first, one witness costs one word
- No quotient exists: `withIsFinal` over a DFA marks states whose suffix language meets `R`, `FAIterators.withInitial` builds each test
- `NFA.fromRegex` throws on any assertion, lookaround exclusion does not reduce, `parser` option `assertions: 'ignore'` drops them and changes the language
- `NFA.fromRegex` takes a hand-built AST (`[{type: 'Concatenation', elements: […]}]`), a `STRING`/`SEQ`/`CHOICE`/`REPEAT` walk over a rule object lowers to it
- `CharSet.union` takes unsorted overlapping ranges, `fromCharacters` throws unless sorted
- Every default factory caps at 10,000 nodes, `[ab]*a[ab]{13}` throws `TooManyNodesError`, `DFA.nodeFactory` is unlimited, `infinityThreshold` shortens counted repeats in an NFA
- `toRegex` holds its own 10,000 cap, `{maxNodes: 5e6}` raises it for a long keyword exclusion, an NFA prints where its DFA throws
- `isDisjointWith(a, b, maxNodes)` throws over its limit, `instanceof` catches it, the constructor name is minified

`JS.Parser` reads JavaScript alone:
- `(?i)`, `(?x)`, and `(?i:a)` reject in every mode, `[[:alpha:]]` rejects under `u`, `[a&&b]` parses as literal characters under `u`
- `/…/v` literals throw on `maxCharacter`, `JS.parseUnicodeSet(element, flags)` takes a regexpp AST and handles `[\p{L}--[a-z]]` and `\q{abc|d}`
- Bundled regexpp is older than `@eslint-community/regexpp` and holds older Unicode, `JS.Parser.fromAst` takes an external AST and keeps the bundled tables
- Without `u`, `\p{L}` silently reads as characters, regexpp under `strict` throws, `u` and `v` together throw

`JS.toLiteral` prints JavaScript, Rust rejects four of its forms (`\0` for NUL, an unescaped `[` in a class, `[^]` and `[]`, surrogate endpoints):
- `fastCharacters: true` stops `\s`, `\d`, and `.`, which change language under tree-sitter's textual rewrite, and leaves the four forms, any negated source class still prints `\0`
- Automata free of those forms print directly, `{flags: {unicode: true, unicodeSets: false, ignoreCase: false}, fastCharacters: true}` gives a `PATTERN` value and flags
- Flags template pins case folding, `/[aA]/u` without `ignoreCase: false` gives `{source: 'A', flags: 'iu'}`
- `/k/iu` prints `[KkK]` and `/s/iu` adds `ſ`, `/k/i` and tree-sitter's `i` exclude them
- Unicode tables disagree: runtime and `@unicode` latest agree, `refa` holds an older table, `regex_syntax` another

Printers over the `toRegex()` AST write every other automaton in forms Rust accepts, a keyword-excluding identifier takes its region through one:

```js
const OPTIONS = {maxCharacter: 0x10ffff};
const nfa = (literal) => NFA.fromRegex(JS.Parser.fromLiteral(literal).parse().expression, OPTIONS);
const point = (code) => `\\u{${code.toString(16)}}`;
const set = ({ranges}) => `[${ranges.map(({min, max}) => (min === max ? point(min) : `${point(min)}-${point(max)}`)).join('')}]`;
const times = ({min, max}) => (max === Infinity ? `{${min},}` : `{${min},${max}}`);
const branches = (list) => list.map(({elements}) => elements.map(element).join('')).join('|');
const element = (node) => (node.type === 'CharacterClass' ? set(node.characters) : `(?:${branches(node.alternatives)})${node.type === 'Quantifier' ? times(node) : ''}`);
const scalars = (fa) => NFA.fromTransitionIterator(FAIterators.mapOut(fa.transitionIterator(), (out) => new Map([...out].map(([to, chars]) => [to, chars.without({min: 0xd800, max: 0xdfff})]).filter(([, chars]) => !chars.isEmpty))), OPTIONS);
const rust = (fa) => branches(scalars(fa).toRegex({maxNodes: 5e6}).alternatives);
const allowed = DFA.fromWords(['alpha', 'beta'].map(Words.fromStringToUnicode), OPTIONS);
allowed.complement();
const region = DFA.fromIntersection(nfa(/[a-z_][a-z0-9_]*/u), allowed);
region.minimize();
```

Regions reject `alpha` and accept `alph`, `a`, `alphas`, `gamma`, every proper prefix of a keyword stays valid, they fit a check or class and no `word`-lexed identifier.

Widening an upstream class keeps its token shape, a transition map over every state:

```js
const nonAscii = CharSet.fromRange(0x10ffff, {min: 0x80, max: 0x10ffff});
const widened = DFA.fromTransitionIterator(FAIterators.mapOut(nfa(/[a-z][a-z0-9]*/u).transitionIterator(), (out) => new Map([...out].map(([to, chars]) => [to, chars.union(nonAscii)]))), OPTIONS);
widened.minimize();
```

Prefix collision between an operator and a trivia opener, and a scanner-free operator token excluding that opener:

```js
const shortest = (left, right) => {
    for (const word of getIntersectionWords(left, right)) return Words.fromUnicodeToString(word);
    return null;
};
const opening = (literal) => {
    const fa = nfa(literal);
    fa.append(NFA.all(OPTIONS));
    return fa;
};
const leading = (literal) => {
    const fa = nfa(literal);
    fa.prefixes();
    fa.withoutEmptyWord();
    return fa;
};
shortest(nfa(/[-+*]+/u), opening(/--/u)); // '--', the operator opens with the opener
shortest(nfa(/[-+*]+/u), leading(/--/u)); // '-', the operator is a prefix of the opener
isDisjointWith(nfa(/[-+*]+/u), opening(/#/u)); // true
const opener = nfa(/--|\/\*/u);
opener.prepend(NFA.all(OPTIONS));
opener.append(NFA.all(OPTIONS));
const openerFree = DFA.fromFA(opener);
openerFree.complement();
const token = DFA.fromIntersection(nfa(/[-+*\/]+/u), openerFree);
token.minimize(); // accepts -, +-, */, rejects --, /*, +--
```

`regexp-ast-analysis` answers over a regexpp AST and its flags in the JavaScript dialect alone, classes assumed non-empty, first-character overlap is no language overlap (`ab` and `ac`).

`@unicode/unicode-<version>` holds `<Category>/<Name>/{ranges,code-points,symbols,regex}.mjs`, ESM with no `exports` map:
- `ranges.mjs` default-exports `{begin, end, length}` with an exclusive `end`, `CharSet.empty(MAX).union(ranges.map(({begin, end}) => ({min: begin, max: end - 1})))`
- `regex.mjs` spells astral characters as surrogate pairs and fails as a grammar pattern

## [11]-[BUILDS]

Native builds take grammar sources, scanner implementations, and their imports as inputs:
- Targets derive from the transitive imports of each `grammar.js`, a hand-kept list restates them
- Grammars compile their own `scanner.c`, else inherit `src/scanner.c` of each imported upstream grammar holding externals and no import
- Composed grammars add the wrapper scanner, empty `externals` in the generated `grammar.json` means no scanner
- Grammar targets sit in their own task-graph project, a task hashes every target of its project, a shared project rebuilds on an unrelated change
- `tree-sitter build -o <lib> <dir>` compiles one grammar root with `src/scanner.c` and `src/grammar.json`, per-unit defines take one build system over every grammar

Task hashes hold global files (runner config, ignore files) no grammar reads, expensive steps key on content below the task:
- `tree-sitter generate --no-parser` writes `grammar.json` and `node-types.json` with no table
- Generator version and `grammar.json` key `parser.c`, `tree-sitter generate <grammar.json>` fills a missing key with no second evaluation
- Libraries compile in each task from the stored parser, a few seconds against generation's minute for C# and Postgres
- Each process works in its own directory and renames into the store and onto published paths, equal keys race to equal content with no lock, a rename onto a held key fails and the held parser serves
- Stored parsers stay until a language's `parser/` folder is removed, no build deletes a key another process may read
- Overwriting a mapped library can crash running scans, a rename leaves the old inode to its mappers
- Libraries compile with hidden visibility and export `tree_sitter_<name>` alone
- Compatibility flags follow the compiler frontend and scope to affected scanner units, GNU `-fno-strict-aliasing` for `tree_sitter/array.h` casts
- Package links resolve to versioned source and include paths, a path change invalidates cached native objects

## [12]-[NODE_SHAPES]

Shapes read from the configured library, first command prints a tree with unnamed tokens, second a kind's contract, third each parent accepting a kind:

```bash
tree-sitter parse --no-ranges --cst -l <lib> --lang-name <name> <file>
jq -c '.[] | select(.type == "<kind>") | {fields, children, subtypes}' <src>/node-types.json
jq -r --arg kind <kind> '.[] | select([(.fields // {})[].types[], (.children.types // [])[]] | any(.type == $kind)) | .type' <src>/node-types.json
```

Contract keys:
- `multiple: true` marks a repeated field (Java `declarator`, Python `subscript`), `named: false` under a field a token field (Swift `declaration_kind`, TypeScript `operator`) that keywords test through `pattern` or `regex`
- Hidden `_name` rules hold no kind, `kind: _name` fails the load, their children's fields appear on the parent with quantities appended per production
- `fields` lists anonymous types, `children` named kinds alone, `required` and `multiple` union across productions, `subtypes` marks a supertype and `root: true` the start rule

Wrapper rules hide capture identity: `$A = $A;` over `lhs` and `rhs` wrappers finds nothing, capturing the inner `id` matches, hiding chain rules removes the gap.

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

Upstream grammars parse these forms where the language's parser rejects them or reads them otherwise, configured extensions override every form except Python `except*`, Bash tests, Rust prefixed strings, and Swift `each(x)`:

| [INDEX] | [GRAMMAR] | [SOURCE]                          | [TREE]                                                   |
| :-----: | :-------- | :-------------------------------- | :------------------------------------------------------- |
|  [01]   | C#        | `a is 0f \|\| b is 0f`            | `is_pattern_expression` holding `binary_expression`      |
|  [02]   | C#        | `g(a * b)`                        | `argument(declaration_expression(type: pointer_type))`   |
|  [03]   | C#        | `foreach (O<V> d in x)`           | `foreach_statement(left: binary_expression)`             |
|  [04]   | C#        | `(a) + b`, `public union Pet(…);` | `cast_expression`, `local_function_statement`            |
|  [05]   | Java      | `case null, default`              | `switch_label(null_literal, identifier)`                 |
|  [06]   | Java      | Top-level `int x = 1;`            | `local_variable_declaration`                             |
|  [07]   | Java      | `o instanceof String _`           | `instanceof_expression(right: type_identifier, name: …)` |
|  [08]   | Python    | `except* A, B:`                   | Repeated `value` fields                                  |
|  [09]   | Bash      | `coproc N { cat; }`, `{fd}>f`     | Split `command`s, `concatenation` with no `descriptor`   |
|  [10]   | Bash      | `[[ a == b ]]`, `[ a = b ]`       | `right: extglob_pattern`, `right: word`                  |
|  [11]   | Rust      | `f128`, `2f16`                    | `type_identifier`, `ERROR`, literal rules stop at `f64`  |
|  [12]   | Rust      | `s..*o`                           | `binary_expression(left: range_expression)`              |
|  [13]   | Rust      | `c"x"`, `b"y"`                    | `string_literal` with the prefix in text                 |
|  [14]   | Swift     | `if try b { }`, `switch try b {`  | `ERROR`, the prefix rule takes `{` as a trailing closure |
|  [15]   | Swift     | `each(x)`                         | `value_parameter_pack`                                   |
|  [16]   | Swift     | `defer { close() }`               | `call_expression(simple_identifier, call_suffix)`        |
|  [17]   | Swift     | `/* x */` opening the next line   | `ERROR`, the newline scan reads `/` as an operator       |
|  [18]   | TSX       | ``sql<Row>`x` ``                  | `binary_expression(binary_expression, template_string)`  |
|  [19]   | TSX       | `F.d<<A>() => A>(x)`              | `binary_expression`                                      |
|  [20]   | TSX       | `import("x").Foo<T>`              | `MISSING`                                                |
|  [21]   | SQLite    | `REINDEX EXPRESSIONS`             | `reindex_statement(target: qualified_table_name)`        |
|  [22]   | Postgres  | `a + 1`                           | `a_expr(a_expr(a_expr_prec(c_expr(columnref(ColId(…`     |

Configured shapes by grammar, extensions included:
- C# `generic_name` children hold no fields, `variable_declarator`, `using_variable_declarator`, and `parameter` hold `value`, `verbatim_string_literal` a `content` child
- Python `comparison_operator` is flat where `binary_operator` left-nests, strings split into `string_start`, `string_content`, `string_end`
- Rust doc comments read `line_comment(outer: outer_doc_comment_marker, doc: doc_comment)`, `token_tree` drops operator leaves from named children
- Swift holds no supertypes, enums hold `enum_class_body`, extensions `class_declaration name: user_type`, `function_declaration` `name` is `multiple`, and declarations name `type_name`, `function_name`, `case_name`, `init_keyword`, `deinit_keyword`, `subscript_keyword`, `referenceable_operator`
- XML `Attribute` holds `Name` and `value: AttText`, `AttValue` sits under `DefaultDecl` alone, `EntityValue` holds `value: EntityText`, `extras` is empty with whitespace in `_S`, and `word` is `Name` over XML 1.0 name ranges
- Postgres holds keywords anonymous, few `opt_*` wrappers, `a_expr`, `b_expr`, and `joined_table` operand and operator fields, a lone hidden child named by its rule, `$$…$$` at `AexprConst(dollar_quoted_string)`
- TOML single-line strings alias a `string_content` per run between escapes, multi-line ones one holding escapes, escape tests take `stopBy: end`
