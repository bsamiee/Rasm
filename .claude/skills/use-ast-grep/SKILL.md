---
name: use-ast-grep
description: "Use when outlining, searching, or rewriting code with ast-grep or writing a rule, covering matching and rule configuration"
---

# [AST_GREP]

MCP tools answer one-off matches and syntax trees, the CLI runs project scans, outlines, rewrites, and execution limits. Search rules stay inline, durable rules sit under `ruleDirs`.

[REFERENCES]:
- [01]-[RULE_BUILDING](references/rule-building.md): Rule diagnostics and rewrites
- [02]-[RULE_HARDENING](references/rule-hardening.md): Existing rule's coverage, shared shapes, and recursion
- [03]-[OUTLINE](references/outline.md): Reading outlines and writing the extractor catalogs behind them
- [04]-[CONFIG](references/config.md): Project discovery, custom languages, trust, globs, stdin, exit codes, and injections
- [05]-[PERFORMANCE](references/performance.md): Scan and outline cost
- [06]-[GRAMMARS](references/grammars.md): Choosing, extending, and building the parsers custom languages load
- [07]-[NAPI](references/napi.md): Parsing and rewriting inside a Node tool through `@ast-grep/napi`

[TEMPLATES]:
- [01]-[RULE](templates/rule.yml): Clause order of a durable rule

## [01]-[MATCHING]

Patterns are valid code under the language's tree-sitter grammar with whole-node metavariables:
- `$VAR` one named node, `$$VAR` one named or unnamed node (operators and keywords included)
- `$$$MULTI` lazy zero-or-more without backtracking, `$_` and `$_NAME` non-capturing
- `$$$` before a node ends at the first fitting sibling, `f($$$H, { $$$P }, $$$T)` matches `f(x, { a }, { b })`
- Adjacent `$$$A, $$$B` loses a node on `f(a, b, c)`, `A` binds empty and `B` starts at the separator
- `$$$A` holds separators, comments, and a trailing comma as nodes, transforms read its full source span
- `"$A"` binds string content, the metavariable test reads any node's whole text, a string holding an escape has more children and misses

Compilation peels single-child wrappers, the pattern's root decides the match range:
- `a` selects the identifier, `a;` the `expression_statement`, `pass` the anonymous token inside `pass_statement`
- Patterns peeling to one anonymous token match the token `kind` refuses and supply a kind set, `{pattern: pass, inside: {kind: pass_statement}}`
- Wrong-kind patterns take `pattern: {context: <full-code>, selector: <kind>}`, `run` takes `-p <full-code> --selector <kind>`
- `selector` takes one kind name and selects its first pre-order node, defining metavariables inside it alone, a lone selected metavariable matches its kind
- Patterns with a parse error beside a `kind` load and never match, `run` alone reports them at zero matches, without `kind` the load fails

Strictness decides which nodes matching skips, the levels hold no total order:

| [INDEX] | [LEVEL]     | [SKIPS]                                                                          |
| :-----: | :---------- | :------------------------------------------------------------------------------- |
|  [01]   | `cst`       | Nothing                                                                          |
|  [02]   | `smart`     | Unnamed target nodes, grammar extras, every target child past the pattern's last |
|  [03]   | `ast`       | Unnamed nodes on both sides                                                      |
|  [04]   | `relaxed`   | Unnamed nodes on both sides, grammar extras                                      |
|  [05]   | `signature` | Text of leaves, matched by kind alone, capture identity kept                     |
|  [06]   | `template`  | Kinds, matched by text alone, a `kind` sits beside it                            |

- `import a` matches `import a, b` under `smart` alone
- `smart`, `cst`, `template` require the comma before `$$$`, `f($A, $$$R)` matches `f(x,)` and misses `f(x)`, `ast`, `relaxed`, `signature` bind `R` empty
- Root `$X` binds a comment under `smart`, the skip test reads the grammar's `extras`, `relaxed` skips every extra
- `run` filters files by the pattern's longest literal whatever the strictness, `--strictness signature` misses files lacking it where `scan` finds them

Patterns debug through `--debug-query`, `pattern` prints the compiled pattern with its peeled root, `cst` and `ast` a tree, `sexp` an S-expression:
- MCP `dump_syntax_tree` errors on multi-statement snippets, `ast-grep run -l <lang> -p '<code>' --debug-query=cst` prints their tree on stderr before exit 8

`regex` is a Rust regex with Unicode `\w`, `\s`, `\d`, `\b`, no look-around or back-reference, and `(?i)` flags inline, a class over source characters restates a grammar token a `kind` test states:
- Identifier classes come from the grammar's `word` rule, else the rule `name` fields hold most often (Swift `simple_identifier`)
- Whitespace classes come from the language's lexical whitespace, `extras` where it states that set (Swift `[\t-\r ]` under `\s+`, XML `_S` under none)
- Unicode Perl classes state no grammar fact, `(?-u:\s)` reads ASCII
- ASCII tokens take `[0-9A-Za-z_]`, `[\t\n\v\f\r ]`, `[0-9]`, and `(?-u:\b)`, an ASCII class over a Unicode identifier token (SQL, Java) misses names
- Negated ASCII classes (`[^\t\n\v\f\r ]`) match every non-ASCII character
- `(?x)` in a `|-` block scalar holds one alternative per line with `\ ` for a space inside a phrase, `(?-x:...)` keeps a phrase verbatim

Identifier tokens hold Unicode classes per grammar, JavaScript and TypeScript accepting `\u` escapes:

| [INDEX] | [LANGUAGE] | [START]                 | [CONTINUE]                                                                                     |
| :-----: | :--------- | :---------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | Rust       | `(r#)?[_\p{XID_Start}]` | `[_\p{XID_Continue}]*`                                                                         |
|  [02]   | Python     | `[_\p{XID_Start}]`      | `[_\p{XID_Continue}]*`                                                                         |
|  [03]   | JS, TS     | Continue set, no digit  | No C0 control, `\p{Zs}`, ASCII punctuation but `$` `_`, U+200B, U+2028, U+2029, U+2060, U+FEFF |

## [02]-[RULES]

Rule documents load through one strict deserializer:
- Rule ids come from file names, idless documents of a multi-document file load as `<stem>-<index>`, every util declares `id`
- Rule and util ids share one namespace across languages, a category checked in more than one names each file `<category>-<language>`, in one the category alone
- Invalid rule files fail project rule loading, a repeated id as `Duplicate rule id`, unknown rule-document keys fail deserialization
- Repeated `not:` or `has:` keys in one map fail the load as `duplicate mapping key: <key>`, `not: {any: [<guards>]}` or an `all:` holds both
- Kind lists inside a flow map are quoted (`{kind: 'block_mapping_pair, flow_pair'}`), an unquoted second kind reads as a key
- Unset `severity` is `hint`, it prints `help[<id>]` and exits 0, `off` removes the rule
- `metadata` is a mapping printed under `--include-metadata` alone, `url` reaches LSP `code_description` alone

Rule maps conjoin `pattern`, `kind`, `regex`, `nthChild`, `range`, `all`, `any`, `not`, `matches`, then `inside`, `has`, `precedes`, `follows` whatever the YAML key order, a cheap relation beside a costly `any`, `not`, or `matches` runs first only from an ordered `all`:
- Positive `kind`, `pattern`, `matches`, or `nthChild.ofRule` predicates can derive candidate kinds
- Scan rules of `regex`, `range`, `not`, `inside`, or unfiltered `nthChild` alone, utils under them included, fail with `Rule must specify a set of AST kinds to match`
- `pattern: $X` alone supplies no kind set, `matches` inherits kinds from its selected util
- Kindless `any:` arms remove the kind set, a sibling `kind` restores it and alone the rule fails to load, `all:` narrows to the intersection of its items
- Empty kind intersections and `any: []` load and never match, `all: []` fails the load

`kind` holds a selector language:
- Combinators `,`, `>`, a space, `+`, `~` and `:has()`, `:not()`, `:is()`, `:nth-child(An+B of S)`, `:nth-last-child()` compile to matchers
- `:first-child`, `*`, `.x`, `[x]`, field selectors, and an anonymous token (`then`) fail the load with `Cannot parse rule`
- `:has(> a b)` tests `b` as the direct child, `+` reads the adjacent sibling tokens included, `number + identifier` misses `[1, b]` where `~` holds
- Selectors add secondary labels as written relations do
- Supertype kinds (`expression`, `statement`) load and match nothing, a rule lists their `subtypes`
- `kind: ERROR` matches, parse failures otherwise silence a rule at exit 0 and read as conforming code
- `kind: MISSING` fails the load, an inserted token keeps its kind as a zero-width leaf `regex: '^$'` matches, beside an exemption for kinds zero-width when valid

Positions:
- `nthChild` is 1-based and takes `An+B` strings, it counts named siblings and comments, a slot takes `ofRule` naming its kind or excluding every comment kind
- `nthChild` never matches the root, `reverse` takes the object form beside `position`
- `ofRule` requires the target to match it as well, its metavariable binds the target alone
- `range` is exact on both endpoints, lines and columns are 0-based, columns count characters, column is optional

Relations test one node along one axis:
- `has: {all: [<a>, <b>]}` demands one child with both shapes, one `has` per required child
- Relations retain the first qualifying node without later constraint retry, select nearest declarations before testing values
- Relational objects hold a rule key beside `field` and `stopBy`, `has: {field: <role>}` alone fails with `Rule must have one positive matcher`
- `stopBy` default `neighbor` reads direct relations, a rule `stopBy` ends the walk at the first node it matches, that node included
- `stopBy: {not: {kind: W}}` walks through a chain of `W` and stops at the first other node, a climb through parentheses alone
- `stopBy` rules start fresh captures and discard bindings, relation targets retain caller captures
- `stopBy: end` walks a candidate's whole axis (`has` the subtree, `inside` every ancestor, `follows` and `precedes` every sibling on that side)
- Walks stop where their fact ends (a nested function for `return`, depth, and locals, a module for imports), wider walks cost time and false findings
- `field:` binding names the final relation and survives `stopBy: end` (callee vs argument, key vs value)
- `has` with `field` and a rule `stopBy` reads the field child and its children alone and misses `((b))`, `has: {field: <role>, any: [<target>, {kind: <wrapper>, has: {stopBy: <stop>, <target>}}]}` descends
- Unnamed token fields (`operator`) take `has: {field: <role>, pattern: $$_X}`
- `has` and `inside` with `field` test a repeated field's first child alone, later children take a fieldless relation with kind and direct parent
- Default `follows` and `precedes` read the adjacent sibling, a comment or unnamed token (`,`) between siblings takes a `stopBy`
- Stopped `follows` and `precedes` walks locate the candidate by its start byte, under an error-recovered parent they can yield no sibling
- `has` visits unnamed children and comments, restrict named children with `not: {has: {pattern: $_, not: {any: [<allowed>, {kind: <comment kinds>}]}}}`

Captures bind in clause order and export from what matched:
- `not` guards over a capture sit after its binding clause in `all:`, the first clause naming `$VAR` binds any candidate in place of testing equality
- Repeated `$VAR` stores the last matching node and range, `$_VAR` skips capture and comparison
- Repeated `$VAR` compares text when either side has no named children whatever the kinds (property `y` equals identifier `y`)
- Repeated `$VAR` over named children needs the same kind and child count recursively, whitespace is ignored, comments count (`(a)` differs from `a`)
- `not` reads current captures and discards new bindings, double negation still exports none
- Captures re-match their text in a later `not: {has: ...}` or `not: {inside: ...}` stopping at owner kinds, the form for a fact every sibling repeats
- Metavariables bound in `all`, relational rules, and the matching `any:` arm export to `fix`, `message`, and `transform`
- Failed `any:` arms bind nothing, the next arm binds the same name again
- Optional captures take an `any:` whose last arm always matches, a template over the unbound capture renders empty
- `any:` keeps the first structurally matching arm with no `constraints` retry, the wrapped pattern precedes the unwrapped one

Constraints test single captures after the rule matches, in unspecified order, `$$$` captures pass unfiltered:
- Constraint rules need no kind set, see rule captures, and export their own, `{A: {not: {has: {pattern: $K, stopBy: end}}}}` relates capture `A` to capture `K`
- `constraints: {A: {pattern: 'g($Z)'}}` exports `$Z` to `fix`
- Keys take no `$`, a transform output is rejected as undefined, a capture unbound in the chosen `any:` arm passes

## [03]-[UTILS]

- Global utils are named `<package>-<shape>` and read `id`, `language`, `arguments`, `rule`, `constraints`, `utils`, and `transform`, other keys load unread
- Global util captures stay private both ways, a util's `$NAME` binds afresh untested against the caller's, `fix: $P` over one fails as undefined
- No language check runs between a rule and its util, kind ids compile under the util's language and compare against the caller's nodes
- Project `utilDirs` load for directory scans, `scan -r`, and `--inline-rules`, stdin included, `-r` and `--inline-rules` load no project rule directory
- Kind sets bind at the calling rule's whole, a kindless util under `all` beside `kind` loads and matches, a parameter slot contributes none
- Parameterized utils take `arguments` at the global level alone, a local `'wrap(NAME)'` exits 8, arguments are mandatory, a string `matches: <id>` of a parameterized util exits 8
- Rules call a parameterized util as `matches: {<util-id>: {<arg>: <rule>}}`, each argument a rule, calls under one `matches` combine as `all`
- Load order and cycle checks follow composite calls, constraints, and local util definitions, relational children add no edge
- Parameterized calls directly under a relation fail as `Rule <id> is not defined` where load order reaches the caller first, a local util holding one orders it
- Composite calls of that util in the same document (root `matches`, under `all` or `not`) order the load and keep the relational call
- Plain relational references resolve lazily and lose the kind pre-check, explicit kinds beside them keep it
- String `matches: <name>` resolves a parameter, then a local util, then a global util
- Local utilities read caller captures
- Every local util compiles whether called or not, an invalid kind in an uncalled one fails the load
- Uncalled local utils' variables count as defined, a `fix` reading them prints empty
- Argument rules start without caller captures, `{pattern: $X}` binding the first candidate a util's relation visits, a name test calls the util at that name
- Argument captures export from a util `rule`, an argument read in `constraints` or a nested call exports none
- Local references passed as arguments do not advertise their bindings for export
- Non-terminating relation recursion aborts with a stack overflow at exit 134 and no output

Parameterized call exporting argument captures, with a recursive util consuming strict descendants through `has`, each util in its own `utilDirs` file:

```yaml
id: call-with
language: javascript
arguments: [CALLEE, ARG]
rule:
    kind: call_expression
    all:
        - has: {field: function, matches: CALLEE}
        - has: {field: arguments, has: {matches: ARG}}
---
id: wrapped-number
language: javascript
rule:
    any:
        - kind: number
        - kind: parenthesized_expression
          has: {matches: wrapped-number}
---
id: no-numeric-send
language: javascript
severity: error
message: $NAME takes $VALUE
rule:
    matches:
        call-with:
            CALLEE: {regex: '^send', pattern: $NAME}
            ARG: {matches: wrapped-number, pattern: $VALUE}
fix: $NAME(String($VALUE))
```

Over `send(1)`, `sendAll(((2)))`, `send(x)`, `send(g(3))`, `other(1)`:

```text
send(1) -> send(String(1)) | send takes 1
sendAll(((2))) -> sendAll(String(((2)))) | sendAll takes ((2))
```

## [04]-[FIX]

`fix` replaces the whole matched node with a template, `transform` derives text, and `rewriters` change structure inside captures:
- Children a pattern never named go with the match, `if (a) b;` with fix `X` turns `if (a) b; else c;` into `X`, `strictness: ast` or a `not: {has: ...}` guards it
- Templates are unparsed text, captures substitute anywhere in them without a syntax or precedence check
- Undefined metavariables fail the rule load under `scan` and substitute empty under `run -r`
- Literal `$NAME` comes from a `replace` with `by: $$NAME`
- Statement templates include their `;`, a template without the terminator drops it
- Fixes indent captured and transformed multiline text to the source line with no raw mode
- `q($S)` over `f($S)` adds that indent inside a multiline string, a match narrowed to the changed token keeps its contents
- Later lines of a multi-line fix indent by the source line's leading spaces, tabs count as zero
- Overlapping fixes never compose, a `program`-level fix blocks every other fix
- `-U` applies only the outer of nested same-rule matches per pass, `f(f(1))` becomes `g(f(1))`, a rerun reaches the inner one

Template names read greedily over `[A-Z0-9_]`:
- `$A_x` and `$A1` fail the load as undefined `A_` and `A1`, `$Ax` prints the capture then `x`
- Sigils fit a binding's arity, `$A` on a `$$$A` capture, `$$$A` on a single capture, and `$$$T` on a transform output load and print nothing
- `message` substitutes captures and transform outputs with no load check, an unknown name prints empty, `note` and label `message` stay literal
- `by: '$1x'` reads a group named `1x` and prints empty, `${1}x` holds the group apart

Transforms name outputs without `$` and read their source with `$`:
- Order is topological, declaration order is irrelevant, any output serves any template
- Capture and output sharing a name fail as `Transform var <name> has already defined`, one reading itself as `cyclic dependency`
- Transforms over an unbound capture store empty text, a chained `\A` replace fires, a prefix belongs on the transform reading the capture
- `NEW: replace($OLD, replace=<regex>, by=<text>)` replaces text
- `NEW: substring($OLD, startChar=1, endChar=-1)` uses constant character indexes, negatives count from the end
- `NEW: convert($OLD, toCase=snakeCase, separatedBy=[underscore])` converts case over the named separators
- `NEW: rewrite($OLD, rewriters=[<id>])` rewrites nodes
- String forms split on the value's own quote character, `by="x,y"` holds, `'it''s'` fails, object forms (`replace: {source: $OLD, replace: <regex>, by: <text>}`) take any value
- String-form values decode as YAML after quote splitting, double quotes take `\\d`
- `replace` reads Rust regex captures in its `by` field as `$1` or `${NAME}`, with `$$` for a literal dollar

Rewriters sit in the document `rewriters` list and hold `id`, `rule`, and `fix`, optional `constraints`, `transform`, and `utils`, and accept kindless field predicates:
- Traversal covers the captured root and its descendants, each node of a `$$$` capture included, anonymous tokens through a kindless `regex` (`{regex: '^,$'}`)
- Rewriters try listed rules at every captured node and descendant, keep each node's first match, then discard overlapping edits
- `joinBy` omitted keeps unmatched text, separators and comments between rewritten nodes included, `joinBy` set joins the replacements alone
- No match returns the source with `joinBy` omitted and empty with it set, a filtering rewriter sets `joinBy`
- Rewriter matching starts from the enclosing rule's bindings, an outer capture name adds an equality test, a distinct name reads every node
- Rewriters read captured syntax trees, enclosing rule captures, and enclosing local utils, no transformed string or transform output
- Nested edits compose only through a rewriter's own `transform`, `f(g(h(1)))` gives `<g>`, a `rewrite` over `$X` inside it gives `<g <h 1>>`
- One relational key alone reports the related node, `{has: {kind: identifier}}` rewrites `[a, b]` to `[X, b]`, `{kind: array, has: ...}` gives `X`
- Rewriter lists over a leaf capture act as if/else over text, the fix reads outer captures, one rewriter with `joinBy: ""` gives an optional segment
- Undefined rewriter ids load and return the source unchanged when the document holds no `rewriters:` key, with the key they fail as `Undefined rewriter`
- Rewriter `utils` share the document's namespace, one util name in the rule and a rewriter fails as `Duplicate rule id`
- Edits reaching outside the captured slice panic with `joinBy` absent (`expandEnd`, `expandStart`, a lone `inside`), `joinBy` takes a path with no slicing
- That panic exits 101 under `--stdin`, on a file it kills a worker thread at exit 0 with `[]`
- Worker panics drop other rules' findings at random over many files, a count that varies between runs points at one

Filtering rewriters split a list by member shape:

```yaml
id: split-args
language: javascript
severity: error
message: split $NAME
rule: {pattern: $NAME($$$ARGS)}
rewriters:
    - {id: keep-numbers, rule: {kind: number, pattern: $V}, fix: $V}
    - {id: keep-names, rule: {kind: identifier, pattern: $V}, fix: $V}
transform:
    NUMBERS: {rewrite: {source: $$$ARGS, rewriters: [keep-numbers], joinBy: ', '}}
    NAMES: {rewrite: {source: $$$ARGS, rewriters: [keep-names], joinBy: ', '}}
fix: "$NAME([$NUMBERS], [$NAMES])"
```

```text
mix(1, a, 2, /* c */ b) -> mix([1, 2], [a, b])
mix(a)                  -> mix([], [a])
```

`fix: {template, expandStart, expandEnd}` widens the replaced range to the sibling the sub-rule matches, a miss keeps the matched range:
- Expansion stops at the first sibling the sub-rule matches, the adjacent one by default, under `stopBy: end` the first match past others (`not: {regex: '^=$'}` reaching a value past `=`)
- Directions apply independently, one comma rule on both removes a comma each side, a sub-rule reads captures, `field` beside them is ignored
- `fix` as a list of `{title, template}` offers alternatives, `-U` and `--json` take the first and `-i` offers each

List member removal takes `expandEnd` only when no comma precedes, one fix covers every position:

```yaml
id: drop-arg
language: javascript
severity: error
message: drop x
rule: {kind: identifier, regex: '^x$', inside: {kind: arguments}}
fix:
    template: ''
    expandStart: {regex: '^,$'}
    expandEnd: {regex: '^,$', not: {follows: {regex: '^,$', stopBy: end}}}
```

```text
f(a, x, b);  ->  f(a, b);
f(a, x);     ->  f(a);
f(x, b);     ->  f( b);
f(x);        ->  f();
f(a, x,);    ->  f(a,);
```

Labels:
- Default labels are the primary plus one secondary per relational match, nested included, any `labels:` key replaces them all, an unbound label drops
- `labels` highlights captured AST ranges, transformed text has no source range, local util captures can label a match and global ones cannot

## [05]-[SUPPRESSION]

`ast-grep-ignore` comments read node kinds containing `comment` with the marker as their first alphabetic text:
- Leading words cancel the marker, leading punctuation does not
- No colon suppresses every rule, `ast-grep-ignore foo` and `ast-grep-ignore-foo` included
- Comma-separated items keep their first token, `q because, p` suppresses `q` and `p`, prose after an id is allowed
- Leading comments cover exactly the next line keyed to match start lines, stacked comments and a blank-line gap break the cover
- Trailing comments cover their sibling's start line, a node inside a multi-line statement stays reported
- First-line comments followed by a blank line or nothing cover the file

## [06]-[MCP]

ast-grep-mcp runs one CLI process per tool call and hands the rule or pattern through unchanged:

| [INDEX] | [TOOL]                 | [COMMAND]                                                  | [RETURNS]                           |
| :-----: | :--------------------- | :--------------------------------------------------------- | :---------------------------------- |
|  [01]   | `dump_syntax_tree`     | `run --pattern <code> --lang <l> --debug-query=<format>`   | Tree the CLI prints on stderr       |
|  [03]   | `find_code`            | `run --pattern <code> [--lang <l>] --json=stream <folder>` | Text or match objects               |
|  [04]   | `find_code_by_rule`    | `scan --inline-rules <yaml> --json=stream <folder>`        | Text or match objects               |

- `severity: error` in a rule's YAML fails `scan`-backed tools on any match, the CLI exits 1 and the server raises it
- `format` takes `pattern`, `cst`, or `ast`, `pattern` finds a metavariable that parsed as a different node
- `AST_GREP_PATH` holds an executable or a command prefix split without a shell, the server appends the subcommand after it
- Wrappers at that path place `--custom-languages allow` after the subcommand
- `AST_GREP_CONFIG` or `--config` passes one `sgconfig.yml` to every call, custom names in the `language` description come from that file
- Nonzero exits other than `1` raise the CLI's stderr, `run` treats exit `1` as no match, a missing path reports `No matches found`
- `max_results` limits returned matches after the process finishes, each call's new process reloads config and grammar content without a restart
