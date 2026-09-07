# [REWRITING]

Extend a proven match with a replacement template. Use `transform` for derived text and `rewriters` for structural changes within captures. Use the bindings for computed values, edits selected from external type information, or coordinated replacements beyond one node.

## [01]-[TEMPLATES]

`fix` replaces the matched node with unparsed text. Captures substitute anywhere in the template, without checking syntax or precedence:

- Fixes fire behind a guard stack: every unfixable variant (guards, discards, exports, valueless members) is a `not:` arm before the template
- Undefined metavariables fail the rule load under `scan` and substitute empty under `run -r`
- Declared but unmatched metavariables substitute empty in `fix` and as a `rewrite()` source
- `$VARName` means `$VARN` followed by `ame`, append uppercase text through a `replace` transform
- Parenthesize replacements and their captures where the surrounding operator, member access, return, or arrow body requires it
- Statement templates include their required `;`, replacing a statement without its terminator can drop it
- Multiline templates indent relative to the matched column and preserve the relative indentation of substituted captures
- Transform output removes the source line's indentation, a source starting deeper than the match can lose required indentation

For multiline statement changes, rewrite the captured statement itself and substitute that output. Test the result inside a nested block, excluding semicolon-separated statements and inline suites when expansion changes their ownership. Preserve comments and directives when reconstructing a node, or match their comment nodes explicitly when changing them is the intended correction.

When changing a callback contract, preserve observable arity and argument reads, and update parameter and return types together. Select the outer result constructor rather than a matching call nested in its value.

## [02]-[TRANSFORMS]

Name each output without `$` and reference its source with `$`. Dependent transforms use an earlier output as their source, ending in one `fix: $RESULT`.

| [INDEX] | [OPERATION] | [FORM] |
| :-----: | :----- | :----- |
| [01] | Replace text | `NEW: replace($OLD, replace=<regex>, by=<text>)` |
| [02] | Slice text | `NEW: substring($OLD, startChar=1, endChar=-1)` |
| [03] | Convert case | `NEW: convert($OLD, toCase=snakeCase)` |
| [04] | Rewrite nodes | `NEW: rewrite($OLD, rewriters=[<id>])` |

String forms use keyword arguments after the source. Omit an unused keyword, retaining the comma when no keyword remains. Use the object form when a regex or replacement contains commas or quotes that the string parser splits:

```yaml
transform:
  SUFFIX:
    replace:
      source: $NAME
      replace: '^(.*)$'
      by: '${1}Name'
```

`replace` uses Rust regex captures in its own `by` field, with `$1` or `${NAME}` and `$$` for a literal dollar. A matching rule's `regex` supplies no captures to the fix. A replacement that consumes a character needed by the next match can require another pass, rewrite the corresponding nodes when that expresses the change directly.

`substring` counts Unicode characters, with an inclusive start, exclusive end, and negative indices from the end. Slicing source text does not decode escaped string values. Use the language parser when the replacement needs the cooked value.

`convert` accepts `lowerCase`, `upperCase`, `capitalize`, `camelCase`, `snakeCase`, `kebabCase`, and `pascalCase`. Separator-sensitive cases accept `separatedBy: [underscore]` or the needed subset of `dash`, `dot`, `space`, `slash`, `underscore`, and `caseChange`. Select separators from the naming contract instead of rebuilding case conversion with regexes.

For a separator conditional on a nonempty capture, consume the entire capture, including newlines:

```yaml
transform:
  SEP: replace($$$REST, replace='(?s)^.+$', by=', ')
fix: 'call(newArgument$SEP$$$REST)'
```

`^.+` alone leaves later lines in the separator and duplicates source text. Prefixing with `by: ', $0'` instead preserves the matched text and the remainder intentionally.

## [03]-[REWRITERS]

Define rewriters once in the document's `rewriters` list and select them by id in `rewrite()`. Each requires `id`, `rule`, and `fix`, and can declare `constraints`, `transform`, and `utils`.

- Traverse the captured root and its descendants, including each node of a `$$$` capture
- Try rewriters in listed order at each node, the first match replaces its subtree and prevents descendant matches in that pass
- Keep unmatched text with `joinBy` omitted, including separators and comments between rewritten nodes
- Set `joinBy` to aggregate replacement strings, unmatched nodes disappear while their matching descendants can still contribute
- Bind list filters to direct members of the captured list through `inside: {pattern: $LIST, kind: <list-kind>}`, the kind alone admits nested values
- Put membership in the outer `rule` or `constraints`, rewriters do not suppress a diagnostic when no subrule changes its text

Use separate outputs over the same capture for conditional method names and bodies, or to partition a list into groups. Order specific rewriters before general ones that consume their arguments. An object-body rewriter can parenthesize `kind: object` before a general expression rewriter.

Rewriters read captured syntax trees and enclosing rule captures, but do not reparse transformed strings as new input. They can use enclosing local utilities, but not enclosing transform outputs. Local captures, transforms, and utilities do not export to another rewriter or the outer rule. Recursive calls retain enclosing captures and reset local captures. Re-match the subtree's values in the rewriter that uses them.

Recursion descends through captured children. Capture a nested expression as `$CHILD` or type arguments as `$$$TYPES`, and invoke the recursive rewriter on those captures. `inside` reads the node's real ancestors, including ancestors outside the captured root. Selecting a source because it is inside the outer construct can select the same node again and overflow the stack.

```yaml
rewriters:
  - id: unwrap
    rule: {pattern: unwrap($CHILD)}
    transform:
      INNER: rewrite($CHILD, rewriters=[unwrap])
    fix: $INNER
```

Define the owning API's behavior before treating any wrapper as removable. Rewriters remain local to their document. Share repeated predicates through utilities and copy required rewriters into their consuming rules.

## [04]-[EDIT_RANGES]

`FixConfig` uses `template`, `expandStart`, and `expandEnd`. Expansion rules accept `stopBy` to reach surrounding syntax. Match the exact separator or whitespace node that belongs to the correction, and retain comments and adjacent statements.

```yaml
fix:
  template: ''
  expandEnd: {regex: '^,$'}
```

Deletion needs cases for first, middle, last, and sole members, trailing separators, and intervening comments. Select the preceding separator when the language requires it for a final member.

Use `replacementOffsets` to apply a CLI-generated replacement. The diagnostic `range.byteOffset` covers the matched node and can exclude expanded text. Overlapping fixes do not compose, a whole-program replacement can consume every inner correction. Compose inner rewriters into the program replacement when inserting a required import, or coordinate disjoint edits through the API.

Titled alternatives describe different fixes for the same finding:

```yaml
fix:
  - {title: '<choice>', template: '<replacement>'}
  - {title: '<other choice>', template: '<other replacement>'}
```

`--json` and `-U` select the first fix, and `-i` offers all choices. Put the intended unattended correction first. Keep independent rules in separate files. Inline multi-document scans can resolve overlapping fixes by rule id, use an explicit sequence when one correction depends on another.

## [05]-[CUSTOM_LANGUAGES]

With `expandoChar`, patterns spell metavariables using that character (`_VAR`, `___VAR`), while fix templates and transform sources use `$`. Constraint keys omit the prefix, and patterns inside constraints use `expandoChar`.

For XML, bind whole nodes. `<Name>_TEXT</Name>` captures content, but `_ID` inside a quoted `AttValue` is literal text. `AttValue` includes its quotes, match each permitted delimiter explicitly when the value can contain the other quote. Whole-element patterns constrain the attribute count, and an attribute name without its value parses as `ERROR`.

Select parsed attributes and metadata before changing an element. Whole-element text replacement can also alter quoted attribute values and comments. Names ending in `Dir` or `Root` do not establish a trailing separator, and file suffixes do not establish import timing or ownership.

Capture the whole element with `pattern: _E` under its kind when transforming its attribute list. Derive edits from the start tag and preserve quoted values containing `>`. Inter-element whitespace is `CharData`, delete adjacent blank text with `expandEnd: {kind: CharData, regex: '^\s*$'}`.

## [06]-[APPLICATION]

1. Establish the intended edit set through structural search and inspect match counts and files
2. Add the replacement and test exact fixed text, syntax, and behavior required by the correction
3. Preview with `ast-grep scan --inline-rules '<yaml>' <paths>`, which prints diffs without writing
4. Apply with `-U`, inspect the written diff, and search for remaining intended matches
5. Apply another pass only when nested matches or injected regions remain and the previous pass changed the source
6. Format the affected files and check the resulting code with the owning language tools

`scan --json` emits matches and replacement data without writing files, even with `-U`. Use text output when applying fixes through the CLI.

`run -p '<pattern>' -r '<template>'` supports a direct pattern rewrite. `scan --inline-rules` supports configured fixes, transforms, rewriters, and alternatives. `run -U` requires `-r`. `run` exits 1 for no matches and 0 for a hit, while `scan` exits 1 for error-severity findings. `run` ignores suppression comments that `scan` honors.

For a stream, `<producer> | ast-grep scan --inline-rules '<yaml>' --stdin -U` emits rewritten source on stdout. Use one fixing rule per stream pass. Nested matches apply outermost first. Injected host updates can write only the last matching region in a pass while reporting every match, inspect actual source changes instead of treating `Applied N` as completion. Stop when a pass changes no source.

Interactive mode accepts `y`, skips with `n`, accepts the rest with `a`, and exits with `q` retaining accepted changes. `e` opens `$EDITOR` and skips the current diff, and Tab cycles alternative fixes.

When only part of a correction is mechanically determined, limit the rewrite to that subset. Retain residual diagnostics only for independently proven violations. Use semantic tooling or the API for type, binding, or behavior conditions that syntax cannot establish.
