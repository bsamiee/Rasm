# [TS_UI_API_PROSEMIRROR_STATE]

`prosemirror-state` owns the editor value: one immutable `EditorState` carries the document, the selection, the stored marks, and every plugin's own field, and `state.apply(tr)` is the sole transition. `Transaction` extends `Transform` with selection, stored marks, and a metadata channel; `Plugin`/`PluginKey` mint the extension unit that contributes a state field, view props, and transaction filters as one value.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the plugin protocol — one value declaring a state field, view props, a view lifecycle, and transaction hooks.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :------------------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `PluginSpec<PluginState>` | interface     | `props`, `state`, `key`, `view`, `filterTransaction`, `appendTransaction`      |
|  [02]   | `StateField<T>`           | interface     | `init(config, instance)`, `apply(tr, value, old, new)`, `toJSON?`, `fromJSON?` |
|  [03]   | `PluginView`              | type alias    | `{update?(view, prevState), destroy?()}` — the imperative side arm             |
|  [04]   | `EditorStateConfig`       | interface     | `{schema?, doc?, selection?, storedMarks?, plugins?}`                          |
|  [05]   | `Command`                 | type alias    | `(state, dispatch?, view?) => boolean`                                         |

- `PluginSpec` carries an open `[key: string]: any` index, so a plugin publishes extra fields other plugins read off `plugin.spec`.
- `EditorStateConfig` requires exactly one of `schema` or `doc`; `doc` supplies its own schema and makes `schema` redundant.
- `Command` is declared here, not in `prosemirror-commands` — every command-shaped value in the ecosystem types against this alias.

[PUBLIC_TYPE_SCOPE]: the selection protocol — an abstract class and the bookmark that survives a document change.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `Selection`         | class         | abstract base; `eq`, `map`, `toJSON` are the subclass duties   |
|  [02]   | `SelectionRange`    | class         | one `$from`/`$to` pair; a selection carries `ranges`           |
|  [03]   | `SelectionBookmark` | interface     | `{map(mapping), resolve(doc)}` — document-free position memory |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the state value and its transitions.

| [INDEX] | [SURFACE]                                                              | [SHAPE]           | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `EditorState.create(EditorStateConfig)`                                | static            | builds the first state; runs plugin `init`  |
|  [02]   | `EditorState.apply(Transaction) -> EditorState`                        | instance          | one transition; runs filters and appends    |
|  [03]   | `EditorState.applyTransaction(Transaction)`                            | instance          | `{state, transactions}` — the appended list |
|  [04]   | `EditorState.tr -> Transaction`                                        | property          | mints a fresh transaction over the document |
|  [05]   | `EditorState.doc` / `selection` / `storedMarks` / `schema` / `plugins` | property          | the carried facts                           |
|  [06]   | `EditorState.reconfigure({plugins})`                                   | instance          | swaps plugins, keeping same-key fields      |
|  [07]   | `EditorState.toJSON(pluginFields)` / `fromJSON(config, json, fields)`  | instance / static | round trip including named plugin fields    |
|  [08]   | `new Plugin(PluginSpec)` / `plugin.getState(state)`                    | ctor / instance   | mints the extension unit; reads its field   |
|  [09]   | `new PluginKey(name)` / `key.get(state)` / `key.getState(state)`       | ctor / instance   | reaches a plugin's instance and field       |

- `EditorState.apply` runs every `filterTransaction` first; one `false` drops the transaction whole and returns the unchanged state.
- `appendTransaction` runs to a fixed point after the applied transaction, so a plugin that always appends loops the state machine.
- `EditorState.toJSON` emits `{doc, selection}`, adding `storedMarks` and one entry per name in `pluginFields`; only a field declaring `toJSON`/`fromJSON` survives the round trip.
- `PluginKey` is required for cross-plugin reads and appears once per key name; two plugins sharing one key collide at `EditorState.create`.

[ENTRYPOINT_SCOPE]: the transaction — one `Transform` widened with selection, marks, metadata, and scroll intent.

| [INDEX] | [SURFACE]                                                            | [SHAPE]             | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------- | :------------------ | :---------------------------------------- |
|  [01]   | `Transaction.selection` / `setSelection(Selection)` / `selectionSet` | property / instance | mapped selection, setter, and intent flag |
|  [02]   | `Transaction.storedMarks` / `setStoredMarks(Mark[])`                 | property / instance | the marks a next typed character inherits |
|  [03]   | `Transaction.ensureMarks(Mark[])` / `addStoredMark(Mark)`            | instance            | incremental stored-mark edits             |
|  [04]   | `Transaction.removeStoredMark(Mark \| MarkType)`                     | instance            | drops one stored mark or a whole type     |
|  [05]   | `Transaction.replaceSelection(Slice)` / `replaceSelectionWith(Node)` | instance            | selection-aware replacement               |
|  [06]   | `Transaction.deleteSelection()` / `insertText(text, from, to)`       | instance            | the two common editing shorthands         |
|  [07]   | `Transaction.setMeta(key, value)` / `getMeta(key)`                   | instance            | the plugin instruction channel            |
|  [08]   | `Transaction.scrollIntoView()` / `scrolledIntoView`                  | instance / property | requests and reports scroll intent        |
|  [09]   | `Transaction.setTime(number)` / `time` / `isGeneric`                 | instance / property | grouping inputs the history plugin reads  |

- `Transaction.replaceSelectionWith(node, inheritMarks)` defaults `inheritMarks` to `true`, so the inserted node picks up the marks at the cursor.
- `Transaction` inherits every `Transform` mutator (`replace`, `delete`, `wrap`, `addMark`, `setNodeMarkup`, …), each returning `this`, so a command chains steps on one transaction and dispatches it once.
- `getMeta` returns `any` and is untyped by construction; the key is a string, a `Plugin`, or a `PluginKey`, and only a `PluginKey` keeps the channel collision-free.
- `isGeneric` is true only while no non-`addToHistory` metadata rides the transaction — the history plugin's grouping test.

[ENTRYPOINT_SCOPE]: selection classes — three registered kinds and the extension point.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `TextSelection.create(doc, anchor, head)` / `between($anchor, $head, bias)`   | static   | the cursor and ranged text selection          |
|  [02]   | `TextSelection.$cursor`                                                       | property | resolved position when the selection is empty |
|  [03]   | `NodeSelection.create(doc, from)` / `isSelectable(node)`                      | static   | selects a node; honours `NodeSpec.selectable` |
|  [04]   | `new AllSelection(doc)`                                                       | ctor     | the whole-document selection                  |
|  [05]   | `Selection.near($pos, bias)` / `findFrom($pos, dir, textOnly)`                | static   | the nearest valid selection to a position     |
|  [06]   | `Selection.atStart(doc)` / `atEnd(doc)`                                       | static   | document-boundary selections                  |
|  [07]   | `Selection.fromJSON(doc, json)` / `Selection.jsonID(id, class)`               | static   | rehydrates and registers a selection kind     |
|  [08]   | `Selection.map(doc, Mappable)` / `eq(Selection)` / `content()`                | instance | the abstract duties every subclass implements |
|  [09]   | `Selection.getBookmark()` / `bookmark.map(mapping)` / `bookmark.resolve(doc)` | instance | position memory surviving a document change   |
|  [10]   | `Selection.replace(tr, Slice)` / `replaceWith(tr, Node)`                      | instance | writes the selection's own replacement        |

- `Selection.toJSON` emits `{type: "text", anchor, head}`, `{type: "node", anchor}`, or `{type: "all"}`; a new subclass registers its own `type` string with `Selection.jsonID` before any state serializes it.
- `Selection.visible` defaults true and marks whether the browser draws the native selection; a custom selection drawing its own cursor sets it false.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- State is a persistent value, never a mutable object: `EditorState.create` builds the first one and `apply` returns a new one, so an old state stays valid, comparable, and replayable. Every editing operation is `state.tr`, a chain of transform steps, and one `apply`.
- Plugins are the only extension unit and their order is their precedence: `plugins` order fixes prop lookup order, field initialization order, and `filterTransaction`/`appendTransaction` order. `reconfigure({plugins})` swaps the set while preserving any field whose plugin key survives.
- Each plugin's state field is a fold: `init(config, instance)` seeds it and `apply(tr, value, oldState, newState)` folds each transaction into the next value. That field is the plugin's only storage — module-level mutable state escapes replay, undo, and multi-editor mounting.
- `PluginKey` is the addressing scheme: `key.getState(state)` reads a field from anywhere, `key.get(state)` reaches the plugin instance, and `tr.setMeta(key, value)` addresses the plugin's transaction channel under a key no other plugin reuses.
- Transactions carry intent beside changes: the selection maps through the transform automatically unless `setSelection` overrides it, stored marks survive an empty-selection edit, `setMeta` carries plugin instructions, and `scrollIntoView()` requests the view scroll — all facts a plugin reads while folding.
- `filterTransaction` vetoes and `appendTransaction` extends: a filter returning `false` drops the transaction entirely, and appended transactions run to a fixed point, each seeing the state produced by the previous round.

[STACKING]:
- `prosemirror-model`(`.api/prosemirror-model.md`): `EditorState.create({schema})` builds its document with `schema.topNodeType.createAndFill()`, `Selection` subclasses hold `ResolvedPos` pairs from `Node.resolve`, `storedMarks` is the `readonly Mark[]` `ResolvedPos.marksAcross` computes, and `EditorState.fromJSON({schema}, json)` decodes the document through `Node.fromJSON`.
- `prosemirror-transform`(`.api/prosemirror-transform.md`): `Transaction extends Transform`, so `tr.steps`, `tr.docs`, and `tr.mapping` are the transform's own; a plugin field maps its stored positions with `tr.mapping.map(pos)` inside `StateField.apply`, and `tr.docChanged` gates that work.
- `prosemirror-view`(`.api/prosemirror-view.md`): `PluginSpec.props` is an `EditorProps` record the view merges by plugin order, `PluginSpec.view(view)` returns the `PluginView` the editor drives, and `DirectEditorProps.dispatchTransaction` is where an application intercepts `apply`.
- `prosemirror-history`(`.api/prosemirror-history.md`): the history plugin is a `StateField` folding `tr.steps` into rope-backed branches, keyed off `tr.isGeneric`, `tr.time`, and the `"addToHistory"` metadata `tr.setMeta` writes.
- `prosemirror-keymap`(`.api/prosemirror-keymap.md`) + `prosemirror-commands`(`.api/prosemirror-commands.md`): both packages produce and consume the `Command` alias declared here — `keymap(bindings)` returns a `Plugin` whose `props.handleKeyDown` dispatches them.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): the editor state rides one atom — `dispatchTransaction` folds `state.apply(tr)` into the registry and the React tree reads the derived atom, keeping one writer over the document while the view stays the imperative DOM owner.
- `core/state/fold`: `Fold` owns lawful keyed folds and ordinal replay; a `StateField.apply` is that same fold shape at the editor grain, and a durable editor fact projects out of the field into the branch fold rather than living twice.
- within-lib `view/content`: the editor page declares one plugin roster per document class, one `PluginKey` per field, and dispatches every mutation as a `Command` — a direct `view.state` write outside `dispatch` is unreachable by design.

[LOCAL_ADMISSION]:
- Store editor-scoped state in a plugin `StateField` keyed by a `PluginKey`; a module-level cache beside the editor breaks replay and a second mounted editor.
- Shape every mutation as a `Command` — `(state, dispatch?, view?) => boolean` — with the query arm honoured, so a disabled control asks the same function that performs the edit.
- Address the plugin metadata channel with a `PluginKey`, never a bare string.
- Register a custom `Selection` subclass with `Selection.jsonID` before serializing any state that can hold it.
