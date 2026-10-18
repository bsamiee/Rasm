# [GREP]

Write native InDesign expressions without JavaScript RegExp delimiters or flags. Patterns select characters; search scope and formatting criteria remain separate decisions.

## [01]-[SEARCH]

Choose the operation by the behavior that must persist:

| [INDEX] | [NEED]                 | [OPERATION] | [PERSISTENCE]                  |
| :-----: | :--------------------- | :---------- | :----------------------------- |
|  [01]   | Edit existing text     | Find/Change | Text or formatting changes     |
|  [02]   | Format as text changes | GREP style  | Character formatting rule      |
|  [03]   | Repeat search criteria | Saved query | Application search preferences |

Use Find/Change to search or replace existing text, including matches spanning paragraph returns. GREP styles apply character formatting within each paragraph and leave contents unchanged. Generated list markers belong to paragraph list formatting and remain outside searchable story characters.

Choose selected text, a story, a document, or all open documents according to the intended extent. Story searches include threaded and overset text. For a table cell or footnote, use its native text range rather than outer-story character positions.

Set inclusion options for parent pages, footnotes, hidden layers, and locked layers or stories. Locked-layer and locked-story inclusion permits finding only. Hidden conditional text remains excluded. Use pattern modifiers for case sensitivity and search options for Kana and character-width sensitivity. Search direction changes match order.

Use Find Format to constrain pattern matches or search by formatting without a pattern. GREP styles select by expression alone. Use Change Format to apply formatting without replacing matched text. For repeated text in different paragraph styles, restrict Find Format to the intended style before changing it. Choose native style and color definitions from their owning document; names can repeat across groups.

Save reusable search criteria as a query. Queries retain patterns, replacements, formatting, and inclusion options in application preferences. Choose search scope for each run. Loading a query restores stored criteria; review those criteria before changing only the expression.

## [02]-[MATCHED_TEXT]

Use lookahead `(?=…)` or `(?!…)` to require or exclude following text without consuming it. Use fixed-width lookbehind `(?<=…)` or `(?<!…)` for preceding text. For decimals after `USD `:

```text
(?<=USD\x{20})\d+\.\d{2}
```

In `USD 125.50; EUR 80.00; USD 2.00`, matches are `125.50` and `2.00`. Currency labels remain outside the match.

For unequal lookbehind alternatives, replace `(?<=\$|USD)` with `(?:(?<=\$)|(?<=USD))`; use `(?<!\$)(?<!USD)` for their negation. Alternatives inside one lookbehind must have equal fixed widths.

When preceding text has variable length, consume it and use `\K` to exclude that prefix from the match:

```text
(?:Map|Figure|Table)\h+\K\d+
```

In `Map 3; Figure   12; Table 42; Map X`, matches are `3`, `12`, and `42`. Replacing with `[$0]` produces `Map [3]; Figure   [12]; Table [42]; Map X`. Character formatting applies to digits alone. Keep `\K` at top level: enclosing a working keep-out expression in a group can invalidate it. Consuming a prefix cannot replace negative lookbehind.

## [03]-[BOUNDARIES]

Distinguish `\r` paragraph returns from `\n` forced line breaks. Use `\h` for horizontal spacing, `\v` for vertical spacing, and `\s` for both. Dot excludes paragraph returns until `(?s)` enables dot-all mode; dot can match native markers. Use `\R` for a native line-break sequence.

Use `^` and `$` for paragraph boundaries, `\A` or `(?-m)^` for story start, `\Z` or `\z` for story end, and `\G` for the native match-position anchor. Word boundaries `\b`, word characters `\w`, and digits `\d` follow native Unicode behavior.

When an alternation needs a boundary in one branch, use a lookaround. Native `^` or `$` combined with alternatives can miss matches depending on branch order. For paragraph-initial `a` or any `bc`:

```text
(?<![^\r])a|bc
```

For `c` before a paragraph return or any `x`:

```text
c(?=\r)|x
```

Pattern `c(?=\r)` excludes a final `c` without a following return.

Use `+` instead of `*` when repeating characters that must be present. Zero-length matches are insertion points: `(?<=A)(?=B)` with replacement `-` changes `AB` to `A-B`. Pattern `^` can match an empty story.

## [04]-[CHARACTERS]

Match aggregate categories with full property names: `\p{Letter}`, `\p{Number}`, `\p{Punctuation}`, `\p{Symbol}`, and `\p{Mark}`. Single-letter forms differ: `\p{L}` matches lowercase letters, `\p{S}` whitespace, and `\p{P}`, `\p{N}`, and `\p{M}` nothing.

Select a subcategory when the distinction matters:
- Match uppercase, lowercase, or other letters with `\p{Lu}`, `\p{Ll}`, or `\p{Lo}`
- Match decimal digits, letter numbers, or other numbers with `\p{Nd}`, `\p{Nl}`, or `\p{No}`
- Match currency or mathematical symbols with `\p{Sc}` or `\p{Sm}`
- Match nonspacing marks or space separators with `\p{Mn}` or `\p{Zs}`

For decomposed `é`, `\p{Letter}` selects `e` and `\p{Mn}` selects the combining acute accent U+0301. Use `\X` for a combining-character sequence. Character categories describe encoded text rather than glyph appearance.

Capital `P` negates a property. Keep property escapes outside bracket classes: replace `[a-z\p{Lu}]` with `(?:[a-z]|\p{Lu})`, or `(?![a-z]|\p{Lu})(?s:.)` for its negation. Inside brackets, property escapes are read as literal characters.

POSIX classes provide `[[:alpha:]]`, `[[:upper:]]`, `[[:lower:]]`, `[[:punct:]]`, and `[[:word:]]`; `[[:unicode:]]` selects code points above U+00FF. Use equivalence class `[[=e=]]` for native equivalent-character matching.

For a Unicode character, use `\x{2014}`, `\x{1F600}`, or `\N{EM DASH}`. Use one full code point for astral characters; paired surrogate escapes match nothing. Pattern `\u` means uppercase letters, not a Unicode escape. Script properties `\p{Han}`, `\p{Script=Han}`, and `\p{IsArabic}` match nothing; express intended code-point ranges in classes.

Use `~a` to find an anchored-object marker, `~#` for any page-number marker, and `~N` for a current-page-number marker. Replacing marker text does not create its referenced object.

## [05]-[GROUPS_AND_MODIFIERS]

Use `(?:…)` when grouping needs no capture. Capture `(…)` for backreferences or replacement; named `(?<name>…)` pairs with `\k<name>`. Branch-reset `(?|…)` reuses capture positions across alternatives, and `(?(1)…)` selects by capture participation. Possessive repetition such as `a++` and atomic groups `(?>…)` prevent internal backtracking.

Set case matching with `(?i)`, `(?-i)`, or scoped `(?i:…)`. Quote literal pattern text with `\Q…\E`. For a readable expression, use `(?x)` with whitespace and comments; escape meaningful spaces with `\x{20}`. To select the space between a day number and a named month:

```text
(?x) (?<=\d) \x{20} (?= (?:January|February) \b )
```

Add intended month names to the alternation. Replacement `~S` changes only that separator to a nonbreaking space.

## [06]-[REPLACEMENTS]

Use replacement `$0` for matched text and `$1` through `$9` for captures. References consume one digit: `$10` inserts capture 1 followed by `0`. Replacement `\1`, `$&`, `${1}`, `\E`, and `$$` insert literal text. Use `\U`, `\u`, and `\L` to uppercase, capitalize, or lowercase following replacement text.

Choose insertion codes by the character needed:
- Paragraph return: `\r` or `~b`; forced line break: `\n`; tab: `\t`
- Nonbreaking hyphen: `~~`; bullet: `~8`; em dash: `~_` or `\x{2014}`
- Nonbreaking spaces: `~s` for U+202F, `~S` for U+00A0; thin space: `~<` for U+2009
- Literal dollar sign: `\$`; literal backslash: `\\`

Combine replacements with Change Format when contents and formatting must change together. For pattern `Figure (\d+)`, replacement `Map $1` retains captured digits while changing the label; Change Format applies to replacement text.

## [07]-[DIAGNOSIS]

When a pattern misses intended text, inspect that passage, scope, inclusion options, and Find Format criteria. Distinguish paragraph returns, forced line breaks, spaces, combining marks, and generated markers before changing the expression.

Invalid expressions can return no matches without an error, and GREP styles accept invalid patterns. Neither successful rule creation nor an empty search proves an expression valid. Judge matching behavior against intended text and boundaries in InDesign.
