# [TS_UI_API_PROSEMIRROR_SCHEMA_LIST]

`prosemirror-schema-list` owns list structure on both halves: three `NodeSpec` rows and `addListNodes` fold ordered lists, bullet lists, and list items into a schema's node map, and six command factories carry the wrap, split, lift, and sink verbs that structure needs. Those commands assume the item content shape `addListNodes` documents, so schema and behaviour ship as one admission.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: this package exports no type of its own; the specs are `NodeSpec` values and the commands the `Command` alias, both declared upstream.

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `NodeSpec` | interface     | the row shape the three exported specs satisfy                 |
|  [02]   | `Command`  | type alias    | `(state, dispatch?, view?) => boolean`, shared with every verb |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the schema half — three spec rows and the fold that seats them.

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------------------------- | :------ | :----------------------------------------------------------- |
|  [01]   | `orderedList`                                                | static  | `NodeSpec` rendering `<ol>`; `order` attribute defaults to 1 |
|  [02]   | `bulletList`                                                 | static  | `NodeSpec` rendering `<ul>`                                  |
|  [03]   | `listItem`                                                   | static  | `NodeSpec` rendering `<li>`                                  |
|  [04]   | `addListNodes(OrderedMap<NodeSpec>, itemContent, listGroup)` | static  | returns the map widened with the three rows                  |

- `addListNodes` seats them as `"ordered_list"`, `"bullet_list"`, and `"list_item"`; the command factories take whatever `NodeType` a caller passes, so a renamed node stays workable while the standard names keep the ecosystem's own rules aligned.
- Exported specs carry no `content` or `group` of their own — `addListNodes` supplies `itemContent` and `listGroup`, so seating them by hand means supplying both.
- `itemContent` shapes like `"paragraph block*"` or `"paragraph (ordered_list | bullet_list)*"` are what the commands below assume; a different shape leaves them declining.

[ENTRYPOINT_SCOPE]: the command half — the structural verbs list editing needs.

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `wrapInList(NodeType, attrs) -> Command`                                      | static  | wraps the selection in a list of that type  |
|  [02]   | `wrapRangeInList(Transaction \| null, NodeRange, NodeType, attrs) -> boolean` | static  | transaction-level form; `null` queries only |
|  [03]   | `splitListItem(NodeType, itemAttrs) -> Command`                               | static  | splits textblock and list item together     |
|  [04]   | `splitListItemKeepMarks(NodeType, itemAttrs) -> Command`                      | static  | the same split preserving the stored marks  |
|  [05]   | `liftListItem(NodeType) -> Command`                                           | static  | lifts the item out of its wrapping list     |
|  [06]   | `sinkListItem(NodeType) -> Command`                                           | static  | sinks the item into an inner list           |

- `wrapRangeInList` returns a boolean and writes onto the transaction it is given, which is the seam for composing list wrapping inside a larger command.
- Every factory takes the `NodeType` from the caller's own schema, so a document class with renamed list nodes binds the same verbs.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- List structure is schema data: the three specs are ordinary `NodeSpec` rows and `addListNodes(nodes, itemContent, listGroup)` folds them into the `OrderedMap` a `SchemaSpec.nodes` accepts. Nesting, item content, and group membership are all decided by `itemContent` and `listGroup`, not by the commands.
- Commands are the structural half of that same decision: `splitListItem`, `liftListItem`, and `sinkListItem` assume an item whose content starts with a textblock and admits nested lists. Passing a different `itemContent` leaves them declining rather than corrupting, which is why both halves land in one admission.
- Every verb is a factory over a `NodeType`: the package hardcodes no node name at the command layer, so a renamed list node binds the same verbs and a second list family — a task list, a definition list — is another bound instance instead of a fork.
- `wrapRangeInList` is the composable core beneath `wrapInList`: given a transaction it writes the wrapping, given `null` it answers admissibility only, so a larger command folds list wrapping into its own transaction without dispatching twice.
- These verbs are ordinary `Command` values and compose by fallback: Enter chains `splitListItem(itemType)` ahead of `splitBlock`, Tab and Shift-Tab bind `sinkListItem`/`liftListItem`, and Backspace reaches `liftListItem` before the base join verbs.

[STACKING]:
- `prosemirror-model`(`.api/prosemirror-model.md`): `addListNodes` returns the `OrderedMap<NodeSpec>` that package's `SchemaSpec.nodes` takes, and every command factory consumes a `NodeType` off `schema.nodes`; the specs' `toDOM`/`parseDOM` rows drive `DOMSerializer.fromSchema` and `DOMParser.fromSchema` with no extra registration.
- `prosemirror-transform`(`.api/prosemirror-transform.md`): the verbs answer admissibility through `findWrapping`, `liftTarget`, and `canSplit`, then emit steps via `Transform.wrap`, `lift`, and `split`, so each declines cleanly where the content expression forbids the change.
- `prosemirror-commands`(`.api/prosemirror-commands.md`): the list verbs chain ahead of the base ones — `chainCommands(splitListItem(itemType), splitBlock)` on Enter is the canonical composition, and `liftListItem` precedes `joinBackward` on Backspace.
- `prosemirror-keymap`(`.api/prosemirror-keymap.md`): the verbs mount as bindings in a keymap ordered ahead of `keymap(baseKeymap)`.
- `prosemirror-inputrules`(`.api/prosemirror-inputrules.md`): the list autoformats are `wrappingInputRule` instances over these node types, the ordered form deriving its `order` attribute from the match.
- within-lib `view/content`: the editor page folds `addListNodes` into its schema build with an explicit `itemContent` and `listGroup`, then binds the six verbs against the resulting `NodeType` values in one command roster.

[LOCAL_ADMISSION]:
- Seat list nodes with `addListNodes(nodes, itemContent, listGroup)` and declare `itemContent` in the shape the commands assume; hand-written list specs beside these are the rejected shape.
- Bind the verbs against the schema's own `NodeType` values and chain them ahead of the base commands rather than reimplementing split, lift, or sink.
- Compose list wrapping inside a larger command with `wrapRangeInList(tr, range, type, attrs)` instead of dispatching a nested command.
- Land a second list family as another bound instance of these factories over its own node types.
