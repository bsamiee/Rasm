# [TS_UI_API_PROSEMIRROR_COMMANDS]

`prosemirror-commands` owns the schema-neutral editing verbs: each export is a `Command` value — `(state, dispatch?, view?) => boolean` — that answers whether it applies and performs the edit only when `dispatch` is given. `chainCommands` folds fallbacks into one verb, `autoJoin` post-processes a command's result, and `baseKeymap` is the platform-resolved binding table every editor starts from.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: this package exports no type of its own — every value here is a `Command` or a `{[key: string]: Command}` binding table, both declared by `prosemirror-state`(`.api/prosemirror-state.md`).

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Command`                  | type alias    | `(state, dispatch?, view?) => boolean` — re-used, not re-declared |
|  [02]   | `{[key: string]: Command}` | interface     | the keymap binding table the three base keymaps satisfy           |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: deletion and joining — the verbs backspace and delete chain through.

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------- | :------ | :-------------------------------------------------------------- |
|  [01]   | `deleteSelection`                                | static  | removes a non-empty selection                                   |
|  [02]   | `joinBackward` / `joinForward`                   | static  | reduces the boundary at a textblock start / end                 |
|  [03]   | `joinTextblockBackward` / `joinTextblockForward` | static  | the narrowed forms joining textblocks only                      |
|  [04]   | `selectNodeBackward` / `selectNodeForward`       | static  | selects the neighbouring node where the schema forbids deletion |
|  [05]   | `joinUp` / `joinDown`                            | static  | joins the selected block with its sibling above / below         |
|  [06]   | `autoJoin(Command, isJoinable)`                  | static  | rejoins nodes a command left adjacent                           |

- `autoJoin` reads `isJoinable` as a predicate or a type-name list.

[ENTRYPOINT_SCOPE]: block structure — splitting, lifting, wrapping, and retyping.

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :----------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `splitBlock` / `splitBlockKeepMarks` | static  | splits the parent block, resetting or keeping stored marks     |
|  [02]   | `splitBlockAs(splitNode)`            | static  | mints a split variant choosing the new block's `{type, attrs}` |
|  [03]   | `lift` / `liftEmptyBlock`            | static  | lifts a block out of its parent / lifts an empty textblock     |
|  [04]   | `wrapIn(NodeType, attrs)`            | static  | wraps the selection in a node of that type                     |
|  [05]   | `setBlockType(NodeType, attrs)`      | static  | retypes the selected textblocks                                |
|  [06]   | `createParagraphNear`                | static  | inserts an empty paragraph beside a selected block node        |
|  [07]   | `newlineInCode` / `exitCode`         | static  | inserts a newline inside a `code` node / escapes past it       |

[ENTRYPOINT_SCOPE]: marks and selection movement.

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `toggleMark(MarkType, attrs, options)`        | static  | adds or removes a mark across the selection    |
|  [02]   | `selectParentNode` / `selectAll`              | static  | widens the selection outward / to the document |
|  [03]   | `selectTextblockStart` / `selectTextblockEnd` | static  | moves the cursor to the textblock's edges      |

- `toggleMark` options: `removeWhenPresent` (default `true`) decides the mixed-range direction, `enterInlineAtoms` (default `true`) reaches inside covered inline atoms, `includeWhitespace` (default `false`) extends over leading and trailing whitespace.
- `toggleMark` on an empty selection writes `storedMarks` instead of a range, so the next typed character carries the mark.

[ENTRYPOINT_SCOPE]: composition and the base keymaps.

| [INDEX] | [SURFACE]                        | [SHAPE] | [CAPABILITY]                                                              |
| :-----: | :------------------------------- | :------ | :------------------------------------------------------------------------ |
|  [01]   | `chainCommands(...Command[])`    | static  | runs each until one returns `true`                                        |
|  [02]   | `baseKeymap`                     | static  | the platform-resolved table — `macBaseKeymap` on Mac, else `pcBaseKeymap` |
|  [03]   | `pcBaseKeymap` / `macBaseKeymap` | static  | the explicit platform tables, for a caller pinning behaviour              |

- Each base table binds Enter, Mod-Enter, Backspace, Delete, and Mod-a to chained deletion, join, and split commands; extend by spreading a table into a higher-precedence `keymap` rather than by mutating it.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every command carries a query arm and an action arm in one function: called with `state` alone it answers whether it applies and touches nothing; called with `dispatch` it builds a transaction and dispatches it. Toolbar enablement and click handling therefore call the same value, and a command that mutates without `dispatch` breaks every caller that asks first.
- Commands compose by fallback: `chainCommands(a, b, c)` runs each until one claims the event, which is how one key binding covers several document shapes. Base keymaps already chain this way, so a custom binding extends the chain rather than reimplementing the fallback.
- Parameterized commands are factories: `wrapIn`, `setBlockType`, `toggleMark`, `splitBlockAs`, and `autoJoin` take schema values or policy and return a `Command`, so a schema-specific verb is a bound instance of a general one.
- Commands take the third `view` argument only where the DOM answers better than the document — `joinBackward` and `joinForward` consult `EditorView.endOfTextblock` for bidi-aware boundaries. Keymaps pass the view through; a headless dispatch omits it and accepts the coarser answer.
- These verbs are schema-neutral by construction: they consult `NodeSpec` flags — `code`, `defining`, `isolating`, `selectable` — and the content expressions rather than node names, so retuning behaviour is a spec-row edit and never a command fork.
- `autoJoin` wraps rather than replaces: it runs the inner command and, where the resulting transform leaves two joinable nodes adjacent, appends the join to the same transaction.

[STACKING]:
- `prosemirror-state`(`.api/prosemirror-state.md`): every export here is that package's `Command` alias; each command builds on `state.tr`, sets a selection with `tr.setSelection`, and hands the transaction to the caller's `dispatch`.
- `prosemirror-transform`(`.api/prosemirror-transform.md`): the structural verbs answer admissibility with `liftTarget`, `findWrapping`, `canSplit`, and `canJoin`, then emit steps through `Transform.lift`, `wrap`, `split`, `join`, and `setBlockType` — which is exactly how the query arm stays free of mutation.
- `prosemirror-keymap`(`.api/prosemirror-keymap.md`): `keymap(baseKeymap)` is the standard boot binding, mounted at the lowest precedence so a schema-specific keymap earlier in the plugin list wins.
- `prosemirror-schema-list`(`.api/prosemirror-schema-list.md`): its list commands are the same `Command` shape and chain ahead of these — `chainCommands(splitListItem(itemType), splitBlock)` on Enter is the canonical composition.
- `prosemirror-model`(`.api/prosemirror-model.md`): `wrapIn`, `setBlockType`, and `toggleMark` take `NodeType`/`MarkType` values straight off `schema.nodes`/`schema.marks`, so a schema edit rebinds the toolbar with no command change.
- `react-aria-components`(`.api/react-aria-components.md`): a toolbar button's `isDisabled` reads `!command(view.state)` and its `onPress` runs `command(view.state, view.dispatch, view)`, keeping one command value behind both arms of the control.
- within-lib `view/content`: the editor page declares one command roster per document class, chains schema-specific verbs ahead of these, and exposes each to both the keymap and the toolbar as one value.

[LOCAL_ADMISSION]:
- Compose these verbs before writing a new one; a hand-rolled transaction that duplicates `splitBlock`, `toggleMark`, or `joinBackward` forfeits the schema-flag handling they already carry.
- Bind a toolbar control's enabled state and its action to the same command value, calling it without `dispatch` for the query.
- Extend a key binding by chaining ahead of the base table in a higher-precedence `keymap`; mutating `baseKeymap` is the rejected shape.
- Pass the `view` argument through from a keymap so boundary detection stays bidi-aware.
