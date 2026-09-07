# [MATCHING]

Structural search runs on the MCP tools with inline YAML rules. Patterns are valid code under the language's tree-sitter grammar with whole-node metavariables: `$VAR` one named node, `$$VAR` one named or unnamed node (including operators and keywords), `$$$MULTI` lazy zero-or-more without backtracking, `$_` and `$_NAME` non-capturing. Smart matching skips unnamed target nodes: the less a pattern specifies, the more it matches. Specify only what the query fixes.

Each search runs in sequence:
1. When the query fits one AST node, run `find_code` with the pattern and a bounded `max_results`
2. For a structural query, start from the most specific positive rule, refine relationally, then filter captures
3. For an unknown node kind, run `dump_syntax_tree` with `format=cst` on one top-level node, or `format=pattern` for a mis-parsing pattern
4. Multi-statement snippets take `ast-grep run -l <lang> -p '<code>' --debug-query=cst`, the tree prints on stderr before exit 8
5. Prove with `test_match_code_rule` on the matching snippet, then on the non-matching one, and return to the tree on a miss
6. On a failed rule call, inspect `printf '%s' '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $?`
7. Run `find_code_by_rule` with bounded `max_results`, `output_format=json` when captures or ranges feed the next step

The proof and the search fold three outcomes into one failure, and the exit code of the `--stdin` scan separates them:
- `[]` with 0 is no match, 8 prints the parse error of a rule the binary rejects, and 1 with the JSON is an `error` diagnostic
- The matching snippet run first proves the rule parses, and the failure on the non-matching one reads as no match
- Both probes take severity omitted or below `error`, durable diagnostics use `error`, and on-demand rewrites use `off`
- The search tools run `--json=stream` over the path as given
- `language` follows the `languageGlobs` entry of `sgconfig.yml`, a `.ts` file mapped to `tsx` matches under `tsx`
- Read oversized results from the response's named file with `jq -r .result <file>`, and bound context with `max_results`
- `languageGlobs` reach every path the command names, outside the root included, and a run from outside the tree parses `.ts` as `typescript`
- Inline rules bind local `utils` and never `utilDirs`, even under `--config`, and a global util proves under a scratch `sgconfig.yml`
- `scan -r <file>` refuses `--filter`, one file proves by `-r` alone and a tree rule by `scan --filter '^<id>$' <file>`
- `--stdin` takes one rule through `-r` or `--inline-rules`, and a project config holding more rules exits 3

```yaml
id: <query-id>
language: <language>
utils:
  <util-id>:                                        # Define once, reference via matches, recurse through has/inside, never through a composite
    any: [ { kind: <kind-a> }, { kind: <kind-b> } ]
rule:
  all:                                              # Explicit all: capture order matters, the defining pattern comes first
    - pattern: <code with $VAR and $$$ARGS>
    - has: { pattern: <sub-pattern>, stopBy: end }  # stopBy picks the axis: neighbor pins depth, end opens it, a rule bounds the walk
    - not: { inside: { kind: <kind>, stopBy: end } }
constraints:
  <VAR>: { regex: '<rust-regex>' }                  # Post-rule filter, a full rule object on a single-node capture, a $$$ name parses and never runs
```

- Kind-only structure uses ESQuery in `kind`: `<a> > <b>`, `<a> <b>`, `<a> + <b>`, `<a> ~ <b>`, `<a>:has(> <b>)`, `:not(<b>)`, `:is(<a>,<b>)`
- `run -k '<selector>' -l <lang>` runs a selector without a rule file, `:nth-child(2n+1 of <b>)` counts named siblings of one shape
- `:nth-last-child(1 of <kind>)` selects the last matching sibling, combine with `:nth-child` for kind-only counts
- The rightmost compound is the subject, `<a> > <b>` matches `<b>`, and `:has(> <a> > <b>)` matches nothing because `<b>` is then the direct child
- `pattern` and `kind` never combine to reparse, a wrong-kind pattern takes `pattern: { context: <full-code>, selector: <kind> }`
- Prefix name searches capture whole nodes with `$NAME($$$)` and `constraints: NAME: { regex: '^<prefix>' }`, and `use$HOOK` is no metavariable
- `$$$` before a node ends at the first sibling the node fits, `f($$$H, { $$$P })` misses `f(x, { a }, { b })` and `f($$$H, { $$$P }, $$$T)` hits it
- `$$$` after a comma needs the comma under `smart` and `cst`, `f($A, $$$R)` misses `f(x)` and hits `f(x,)`, and `ast` down binds `R` empty on `f(x)`
- Rules need a kind set from `pattern` or `kind`, `regex`, `range`, `nthChild`, or `not` alone aborts with a missing-kind error
- Empty `run` results read their exit code, 1 is no match, 8 a rejected pattern (a lone `$$$VAR`), and 0 with a warning an `ERROR` root
- Rule objects are unordered `all`s, the keys apply atomic, composite, then relational, and `all:` keeps its list order
- The first clause naming `$VAR` binds it and later ones re-match it, a binding a later clause needs puts both under `all:`
- Rules test one node, `has: {all: [<a>, <b>]}` demands one child with both shapes, one `has` per required child
- `strictness` per value: `cst` skips nothing, `smart` skips unnamed target nodes and comments, `ast` skips unnamed nodes on both sides, no comment
- `relaxed` skips unnamed nodes on both sides and comments, `signature` skips like `relaxed` and matches leaves by kind alone, `template` drops kinds
- Skipped comments face a leaf, a metavariable, or the match end, and a comment before a compound node (a statement, a call) fails every value
- `signature` over files keeps the fixed-text prefilter of the walk, `a = b` finds `a = y` and misses `x = y`, and `--stdin` matches both
- `range: {start: {line, column}, end: {line, column}}` (zero-based, end exclusive) pins a rule to the node an external report names
- `find_code` returns `No matches found` for an `ERROR` pattern, a missing path, and a `language` no glob maps, `ast-grep run` separates them
- On Windows the server runs `ast-grep.cmd` through a shell, and a call holding `$` or parentheses takes the CLI form
- `template` needs a `kind` beside it, and a comment inside the code takes `kind` with relational rules over a lower strictness

## [01]-[PRECISION]

Choose the smallest predicate that distinguishes the correction family from its near misses.

- Anchor `regex` where the contract requires it, unanchored substring matching finds `Exemption` inside `NoExemptionHere`
- `regex` is a Rust regex with no look-around or back-reference, `|` inside a name escapes, and `(?i)` sets flags inline
- `kind` and `pattern` restrict the candidate nodes by kind, `regex` restricts none, and `matches: <param>` prunes only beside a `kind`
- Captures unify by default, the same `$VAR` across clauses proves sameness, `not:` on a rebound pattern proves difference, `$_VAR` skips both
- `has` binds `$VAR` to the earliest matching child and a later clause rejecting that child fails the rule, the narrower `has` precedes the wider
- Unification compares nodes, a name wrapped in another kind never unifies, `has: {field: name, pattern: $VAR}` reaches the identifier
- Named leaves unify by text, an `identifier` capture re-matches a `shorthand_property_identifier` and a `property_identifier` of its text
- Strings unify by text under one quote style, and a string with an `escape_sequence` child unifies by structure, `"\nstoreDir"` with `"\ncacheDir"`
- Captures bound on one node re-match their text inside a later `not: {has: ...}`, the form for a fact every sibling repeats
- `nthChild: <n>` counts named siblings, comments the block holds included, and `{position: <n>, ofRule: <rule>, reverse: true}` counts from the end
- `nthChild: {position: 1, ofRule: {not: {kind: comment}}}` counts semantic slots without comments, including statements and type arguments
- Pattern bodies match by containment, `not: {has: {nthChild: 2}}` on the container proves exactly one child
- `nthChild: 1` with `nthChild: {position: 1, reverse: true}` under `all` proves the only child from the child's side
- Rules that count, bound, or measure a structure match the role a node plays: its parent kind, its field, and what it declares
- Kind chains count every node of the kind, and each shape with the kind and without the role (a thunk, an initializer) is a valid case
- Match one list element through `pattern: $ITEM` and guard its container structurally, or transform the `$$$` capture through a rewriter
- Two `$$$` around one element never backtrack, the element takes `pattern: $ITEM` with `inside: {pattern: <list with $$$>, stopBy: end}`
- `stopBy` forms: default neighbor for direct relations, `end` for the whole axis, a rule for a bounded walk that includes the stop node
- On a relational miss, inspect the owner distance and choose `neighbor`, a stopper, or `end` for the intended relation
- Same-kind stoppers (`stopBy: {kind: <owner-kind>}`) pin the nearest enclosing owner, and bound a `has` walk where the grammar nests (C `case`)
- Closure stoppers on `has` (`stopBy: {kind: <function-kind>}`, or `any:` of them) keep an inner function's `return` from satisfying the outer owner
- `stopBy: {not: {any: [<kinds>]}}` walks up while every ancestor is an allowed kind, the stopper stated as the allowed set
- Aliases of a callee resolve through `inside: {kind: <scope>, stopBy: end, has: {pattern: $ALIAS = <callee>, stopBy: end}}`, `$ALIAS` bound first
- Patterns a grammar update can reshape take `kind` with `field` and `regex` on the name, the longer form keeps matching
- `inside: {not: <shape>, stopBy: end}` matches when any ancestor lacks the shape, nearly every node, absence is `not: {inside: {<shape>}}` at `end`
- `field:` binds the final relation and survives `stopBy: end` (callee vs argument, key vs value), field names read off the `dump_syntax_tree` cst
- Relational objects hold a rule key beside `field` and `stopBy`, `has: {field: <role>}` alone aborts on `Rule must have one positive matcher`
- The positive-matcher abort names `utils` when the relation sits in a util, and `has: {field: <role>, kind: <kind>}` is the parsing form
- Only `has` and `inside` accept `field:`, and an `any:` branch under them refuses it as `unknown field`
- `has: {field: <role>, stopBy: end}` tests the field node, then its whole subtree, `has: {stopBy: end}` without a field skips the target node
- `has: {field: <role>}` tests the first child with the field alone, a repeated field (`argument`) takes `nthChild` or a fieldless `has`
- `has: {field: <role>, stopBy: <rule>}` tests the field node and its direct children alone, `stopBy: end` beside `field` walks its whole subtree
- `precedes` and `follows` walk the sibling list alone, unnamed siblings included, and reject `field`
- Nested `follows` under `stopBy: end` count earlier siblings, three levels fire on the third counted sibling with no arithmetic
- `constraints:` needs no kind set, negative and relational capture guards belong there
- `has` visits unnamed children, restrict named children with `not: {has: {pattern: $_, not: <allowed>}}`
- Child restrictions cover direct children only, the inside of an allowed member (an `elif` arm, a nested body) takes its own arm
- Constraints run after the whole rule, a capture guard cannot narrow a `not:`, the negation matches everything and the rule fails silently
- Marker exemptions bind structurally: a comment `precedes: { kind: <body> }` marks its owner, a mark on a descendant proves nothing
- Ordering rules bind statement nodes, an expression pattern alone has no statement siblings, wrap `precedes`/`follows` in `context`/`selector`
- Self-nesting shapes (a chain of arms, a call nested as its own first argument) report once through `not: {inside: <the nesting position>}`
- `kind` alone defines no metavariable, a fixable capture pairs `kind` with `pattern: $NODE`
- `expandStart`/`expandEnd` extend the fix range to the first sibling matching the sub-rule, the adjacent one by default and any under `stopBy: end`
- `expandStart`/`expandEnd` misses or nodes without siblings retain the matched range, and a valueless key fails parsing
- Metavariables bound in `all`, relational rules, and the matching `any:` branch export to `fix`, `message`, and `transform`
- Failed `any:` branches bind nothing, and the next branch binds the same name afresh
- `any:` keeps the first arm that matches structurally and `constraints` retries no other, the wrapped pattern precedes the unwrapped one
- Bind captures required by `fix` or `constraints` in each applicable `any:` arm, using local utils when the branches share bindings
- `strictness: signature` matches shape while keeping capture identity, the form for a duplicate-shape search and useless as a name ban
- Count real-code matches before registering a rule, refine overbroad predicates, and reserve semantic conditions for language tooling
- `dump_syntax_tree` decides wrapping and field names, an `ERROR` construct is unenforceable, and its statement's pattern still matches it
- Bash rules preserve unsupported syntax at the match and edit boundaries, while supported commands elsewhere remain eligible
- Use the language parser for supported syntax the tree-sitter grammar cannot read, and limit structural selection to nodes it parses

Shadowing of an alias resolves through language tooling.
