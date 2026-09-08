# [REWRITING]

`fix` replaces a proven match with a template, `transform` derives text, and `rewriters` change structure inside captures.

## [01]-[TEMPLATES]

Templates are unparsed text, captures substitute anywhere in them without a syntax or precedence check:
- Unfixable variants (guards, discards, exports, valueless members) are `not:` arms before the template
- Undefined metavariables fail the rule load under `scan` and substitute empty under `run -r`
- Declared unmatched metavariables substitute empty in `fix` and as a `rewrite()` source
- `$ITEMName` means `$ITEMN` followed by `ame`, append uppercase text through a `replace` transform
- Lowercase names after `$` (`$schema`) are no metavariable and stay literal in `fix`
- Parenthesize replacements and their captures where the surrounding operator, member access, return, or arrow body requires it
- Statement templates include their `;`, a template without the terminator drops it
- Multiline templates indent relative to the matched column and keep the relative indentation of substituted captures
- Transform output removes the source line's indentation, a source starting deeper than the match loses required indentation
- Substituted heredoc delimiters move from column 0 to the template's column and stop closing the heredoc, a multi-line string stays as written
- Multiline statement changes rewrite the captured statement and substitute that output
- Exclude semicolon-separated statements and inline suites when expansion changes their ownership
- Reconstructed nodes keep comments and directives, a correction changing them matches their comment nodes

## [02]-[TRANSFORMS]

Outputs are named without `$` and read their source with `$`. Dependent transforms take an earlier output as source and end in one `fix: $RESULT`.

| [INDEX] | [OPERATION]   | [FORM]                                           |
| :-----: | :------------ | :----------------------------------------------- |
|  [01]   | Replace text  | `NEW: replace($OLD, replace=<regex>, by=<text>)` |
|  [02]   | Slice text    | `NEW: substring($OLD, startChar=1, endChar=-1)`  |
|  [03]   | Convert case  | `NEW: convert($OLD, toCase=snakeCase)`           |
|  [04]   | Rewrite nodes | `NEW: rewrite($OLD, rewriters=[<id>])`           |

String forms take keyword arguments after the source, unused keywords go and the comma stays when none remains. Object form holds a regex or replacement with commas or quotes the string parser splits:

```yaml
transform:
  SUFFIX:
    replace:
      source: $NAME
      replace: '^(.*)$'
      by: '${1}Name'
```

- `replace` reads Rust regex captures in its `by` field as `$1` or `${NAME}`, with `$$` for a literal dollar
- Rule `regex` supplies no captures to the fix
- Replacements consuming a character the next match needs require another pass, rewriting the nodes states the change directly
- `substring` counts Unicode characters, inclusive start, exclusive end, negative indices from the end
- Slicing source text decodes no escaped string value, a replacement needing the cooked value reads the language parser
- `convert` takes `lowerCase`, `upperCase`, `capitalize`, `camelCase`, `snakeCase`, `kebabCase`, or `pascalCase`
- `separatedBy` takes the subset of `dash`, `dot`, `space`, `slash`, `underscore`, and `caseChange` the naming contract states, in place of a regex

Separators conditional on a nonempty capture consume the whole capture, newlines included:

```yaml
transform:
  SEP: replace($$$REST, replace='(?s)^.+$', by=', ')
fix: 'call(newArgument$SEP$$$REST)'
```

`^.+` alone leaves later lines in the separator and duplicates source text. `by: ', $0'` keeps the matched text after the separator.

## [03]-[REWRITERS]

Rewriters sit in the document `rewriters` list, `rewrite()` selects them by id. Each holds `id`, `rule`, and `fix`, with optional `constraints`, `transform`, and `utils`:
- Traversal covers the captured root and its descendants, each node of a `$$$` capture included
- Rewriters run in listed order at each node, the first match replaces its subtree and blocks descendant matches in that pass
- `joinBy` omitted keeps unmatched text, separators and comments between rewritten nodes included
- `joinBy` set joins replacement strings, unmatched nodes drop and their matching descendants contribute
- List filters bind direct members through `inside: {pattern: $LIST, kind: <list-kind>}`, the kind alone accepts nested values
- Membership sits in the outer `rule` or `constraints`, a rewriter with no changed text suppresses no diagnostic
- Rewriters matching no node yield the source unchanged, one matching a descendant alone rewrites it inside that text
- Separate outputs over one capture serve conditional method names and bodies, or partition a list into groups
- Specific rewriters precede general ones consuming their arguments, an object-body rewriter parenthesizes `kind: object` first
- Rewriters read captured syntax trees, enclosing rule captures, and enclosing local utilities, no transformed string or transform output
- Local captures, transforms, and utilities export to no other rewriter or outer rule, a rewriter re-matches the subtree values it uses
- Recursive calls keep enclosing captures and reset local captures
- Recursion descends through captured children, a nested expression as `$CHILD` or type arguments as `$$$TYPES`, with the rewriter invoked on them
- `inside` reads real ancestors, those outside the captured root included, a source selected by containment recurses on itself
- Rewriters stay local to their document, consuming rules hold a copy of the rewriters they need

```yaml
rewriters:
  - id: unwrap
    rule: {pattern: unwrap($CHILD)}
    transform:
      INNER: rewrite($CHILD, rewriters=[unwrap])
    fix: $INNER
```

## [04]-[EDIT_RANGES]

`FixConfig` holds `template`, `expandStart`, and `expandEnd`:
- `expandStart` and `expandEnd` extend the fix range to the first sibling matching the sub-rule, adjacent by default and any under `stopBy: end`
- Misses and nodes without siblings keep the matched range
- Valueless keys fail parsing
- Expansion matches the exact separator or whitespace node of the correction and keeps comments and adjacent statements

```yaml
fix:
  template: ''
  expandEnd: {regex: '^,$'}
```

- Deletion cases cover first, middle, last, and sole members, trailing separators, and intervening comments
- Final members take the preceding separator where the language requires it
- Unnamed tokens (the `,` between JSON pairs) block a `kind: pair` expansion, `stopBy: {kind: pair}` reaches across to that pair
- YAML member deletion with a comment before the comma stops at the comment and leaves a leading comma, the case keeps both or excludes the shape
- `range.byteOffset` covers the matched node and can exclude expanded text
- Overlapping fixes never compose, a `program`-level fix blocks every other fix
- Required imports compose inner rewriters into the program replacement
- Disjoint edits coordinate through the API

Titled alternatives are different fixes for one finding:

```yaml
fix:
  - {title: '<choice>', template: '<replacement>'}
  - {title: '<other choice>', template: '<other replacement>'}
```

- `--json` and `-U` take the first fix, `-i` offers every choice, the unattended correction sits first
- Inline multi-document scans resolve overlapping fixes by the lower rule id, a dependent correction takes an explicit sequence

## [05]-[CUSTOM_LANGUAGES]

- Under `expandoChar`, patterns spell metavariables with that character (`_VAR`, `___VAR`), fix templates and transform sources use `$`
- Constraint keys omit the prefix, patterns inside constraints use `expandoChar`
- XML patterns bind whole nodes, `<Name>_TEXT</Name>` captures content
- `_ID` inside a quoted `AttValue` is literal text
- `AttValue` includes its quotes, a value that can hold the other quote matches each delimiter
- Whole-element patterns fix the attribute count
- Attribute names without a value parse as `ERROR`
- Whole-element text replacement alters quoted attribute values and comments, element changes select parsed attributes
- Attribute list transforms capture the element as `pattern: _E` under its kind, derive edits from the start tag, and keep quoted values holding `>`
- Inter-element whitespace is `CharData` under `content`, a `Comment` inside a value splits its `CharData`
- Adjacent blank text deletes through `expandEnd: {kind: CharData, regex: '^\s*$'}`

## [06]-[APPLICATION]

1. Find the edit set through structural search and inspect match counts and files
2. Add the replacement and test exact fixed text, syntax, and behavior the correction requires
3. Preview with `ast-grep scan --inline-rules '<yaml>' <paths>`, printing diffs without writing
4. Apply with `-U`, inspect the written diff, and search for remaining intended matches
5. When nested matches or injected regions remain after a source-changing pass, run another pass
6. Format the affected files and check the code with the owning language tools

- `scan --json` emits matches and replacement data and writes no file, `-U` included, CLI application takes text output
- `run -p '<pattern>' -r '<template>'` rewrites by a direct pattern, `run -U` requires `-r`
- `scan --inline-rules` runs configured fixes, transforms, rewriters, and alternatives
- `run` ignores the suppression comments `scan` honors
- `<producer> | ast-grep scan --inline-rules '<yaml>' --stdin -U` emits rewritten source on stdout
- Nested matches apply outermost first
- Injected host updates can write the last matching region alone and report every match, the source diff proves completion, `Applied N` nothing
