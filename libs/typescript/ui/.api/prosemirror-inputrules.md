# [TS_UI_API_PROSEMIRROR_INPUTRULES]

`prosemirror-inputrules` owns typing-triggered transformation: an `InputRule` pairs a regular expression matched against the text before the cursor with a replacement string or a transaction-returning handler, `inputRules({rules})` mounts the roster as one plugin, and `undoInputRule` reverts the last applied rule on the next keystroke. Autoformatting is therefore a data row per rule, never a keystroke branch.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the rule value and the plugin's own field shape.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `InputRule`   | class         | one match plus its handler; `inCode` and `inCodeMark` are readable fields |
|  [02]   | `PluginState` | union         | `{transform, from, to, text} \| null` — the last applied rule, for undo   |

- `PluginState` is declared but not exported; `inputRules` returns `Plugin<PluginState>`, so the field type infers at the call site.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: rule construction and the two builders that cover the common structural cases.

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `new InputRule(RegExp, string \| handler, {undoable, inCode, inCodeMark})` | ctor    | one rule; `match` must end with `$`            |
|  [02]   | `wrappingInputRule(RegExp, NodeType, getAttrs, joinPredicate)`             | static  | wraps the textblock in that node type on match |
|  [03]   | `textblockTypeInputRule(RegExp, NodeType, getAttrs)`                       | static  | retypes the textblock on match                 |
|  [04]   | `inputRules({rules}) -> Plugin<PluginState>`                               | static  | mounts `rules`, a `readonly InputRule[]`       |
|  [05]   | `undoInputRule`                                                            | static  | `Command` reverting the last input's rule      |

- `InputRule` handlers take `(state, match, start, end) => Transaction | null`; returning `null` declines the match and lets the next rule try.
- String handlers replace the matched text, or the first capture group where the regexp has one.
- `getAttrs` takes `Attrs | null` or `(match: RegExpMatchArray) => Attrs | null`, so a rule derives attributes from its own capture groups.
- `wrappingInputRule`'s `joinPredicate` is `(match, nodeBefore) => boolean` and decides whether the new wrapper joins an identical sibling above; omitting it joins by default.
- Constructor options: `undoable` (default `true`) admits the rule to `undoInputRule`, `inCode` is `boolean | "only"` and defaults to skipping `code` nodes, `inCodeMark` defaults to `true`.

[ENTRYPOINT_SCOPE]: the shipped typographic rules — plain `InputRule` values composed into a roster.

| [INDEX] | [SURFACE]                              | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------- | :------ | :------------------------------------------------ |
|  [01]   | `emDash` / `ellipsis`                  | static  | double dash to em dash; three dots to an ellipsis |
|  [02]   | `smartQuotes`                          | static  | `readonly InputRule[]` of the four quote rules    |
|  [03]   | `openDoubleQuote` / `closeDoubleQuote` | static  | the double-quote halves, for a partial roster     |
|  [04]   | `openSingleQuote` / `closeSingleQuote` | static  | the single-quote halves, apostrophes included     |

- Spread `smartQuotes` into the rule array; passing the array itself as one rule silently disables the whole group.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Rules are data, matched in order: the plugin runs on text input, takes the text before the cursor in the current textblock, and tries each rule in roster order until one returns a transaction. Adding an autoformat is one row, and its priority is its index.
- Every regexp anchors at the end with `$` because it matches text ending at the cursor, and a structural rule anchors its start with `^` so it fires only at the textblock start.
- `inCode` and `inCodeMark` guard code context per rule, never globally: `inCode` defaults to skipping nodes with `NodeSpec.code`, `"only"` inverts that to fire exclusively inside them, and `inCodeMark` governs `MarkSpec.code` — so a rule inside a code block is a declared exception rather than a handler branch.
- `inputRules` remembers exactly one application: its field holds the last rule's transform and range, `undoInputRule` reverts it, and any further input clears the memory. Chaining `undoInputRule` ahead of `undo` on Backspace makes the first press cancel the autoformat.
- `wrappingInputRule` and `textblockTypeInputRule` build on the structural transform primitives, so a rule that a schema's content expression forbids declines instead of producing an invalid document.

[STACKING]:
- `prosemirror-model`(`.api/prosemirror-model.md`): the two builders take a `NodeType` off `schema.nodes` and their `getAttrs` returns the `Attrs` bag that type declares; `inCode`/`inCodeMark` read `NodeSpec.code` and `MarkSpec.code`.
- `prosemirror-transform`(`.api/prosemirror-transform.md`): `wrappingInputRule` composes `findWrapping` and `Transform.wrap` and joins through `Transform.join`, and `textblockTypeInputRule` emits `Transform.setBlockType`, so each declines where the step is inadmissible.
- `prosemirror-state`(`.api/prosemirror-state.md`): the plugin field is a `StateField` holding the last application, and every handler returns a `Transaction` built on `state.tr`.
- `prosemirror-view`(`.api/prosemirror-view.md`): the plugin contributes `props.handleTextInput`, so rules fire on real text input and never on programmatic replacement.
- `prosemirror-history`(`.api/prosemirror-history.md`): `chainCommands(undoInputRule, undo)` on Backspace makes the first press cancel the autoformat and the next enter the undo branch.
- `prosemirror-keymap`(`.api/prosemirror-keymap.md`): `undoInputRule` binds like any command, with the chain order deciding which verb claims Backspace.
- `prosemirror-schema-list`(`.api/prosemirror-schema-list.md`): the canonical list rules are `wrappingInputRule` instances over `bullet_list` and `ordered_list`, the ordered form deriving its `order` attribute from the match.
- within-lib `view/content`: the editor page declares one rule roster per document class as data ordered by priority, deriving each rule from the schema's own `NodeType` values.

[LOCAL_ADMISSION]:
- Land an autoformat as an `InputRule` row in the document class's roster; a `handleTextInput` handler branching on typed characters is the rejected shape.
- Anchor every pattern with `$`, and with `^` where the rule is structural.
- Reach for `wrappingInputRule`/`textblockTypeInputRule` before a hand-written handler; a bare handler duplicating a wrap or a retype loses the admissibility check.
- Spread `smartQuotes` into the roster and declare `inCode`/`inCodeMark` per rule where a code context needs a different answer.
