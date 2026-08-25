# [TS_UI_API_PROSEMIRROR_VIEW]

`prosemirror-view` owns the editable DOM: one `EditorView` renders an `EditorState` into a `contenteditable` element, reads user intent back through the DOM and the clipboard, and emits every change as a `Transaction` for the host to apply. `Decoration`/`DecorationSet` layer presentation over the document without touching it, and the `nodeViews`/`markViews` records hand a node's DOM to a caller-owned object.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the props system — one flat record every plugin and the direct caller contribute to, resolved by precedence.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `EditorProps<P>`     | interface     | every handler, transform, and render prop; `this` binds to `P`             |
|  [02]   | `DirectEditorProps`  | interface     | `EditorProps` plus required `state`, plus `plugins`, `dispatchTransaction` |
|  [03]   | `DOMEventMap`        | interface     | `HTMLElementEventMap` widened with an open string index                    |
|  [04]   | `ViewMutationRecord` | union         | `MutationRecord \| {type: "selection", target}`                            |

- `EditorProps` handler keys: `handleDOMEvents`, `handleKeyDown`, `handleKeyPress`, `handleTextInput`, `handleClick`/`handleClickOn`, `handleDoubleClick`/`handleDoubleClickOn`, `handleTripleClick`/`handleTripleClickOn`, `handlePaste`, `handleDrop`, `handleScrollToSelection`, `dragCopies`, `createSelectionBetween`.
- `EditorProps` clipboard keys: `domParser`, `clipboardParser`, `clipboardSerializer`, `clipboardTextSerializer`, `clipboardTextParser`, `transformPastedHTML`, `transformPastedText`, `transformPasted`, `transformCopied`.
- `EditorProps` render keys: `nodeViews`, `markViews`, `decorations`, `editable`, `attributes`, `scrollThreshold`, `scrollMargin`.
- Every handler returning `true` claims the event; `false` or `undefined` passes it to the next contributor by plugin order, the direct props winning first.

[PUBLIC_TYPE_SCOPE]: the node-view seam — a caller-owned object owning one node's DOM.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                                |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `NodeView`            | interface     | `dom`, `contentDOM?`, `update?`, `selectNode?`, `stopEvent?`, `ignoreMutation?`, `destroy?` |
|  [02]   | `NodeViewConstructor` | type alias    | `(node, view, getPos, decorations, innerDecorations) => NodeView`                           |
|  [03]   | `MarkView`            | interface     | `dom`, `contentDOM?`, `update?(mark)`, `ignoreMutation?`, `destroy?`                        |
|  [04]   | `MarkViewConstructor` | type alias    | `(mark, view, inline) => MarkView`                                                          |
|  [05]   | `DecorationSource`    | interface     | `map`, `forChild`, `forEachSet` — what a node view receives inward                          |
|  [06]   | `DecorationAttrs`     | type alias    | `{nodeName?, class?, style?, [attr]: string \| undefined}`                                  |

- `NodeView.update` returns `false` to force a full rebuild and `true` to claim the new node; `multiType` lets one view survive a node-type change.
- `getPos()` returns `number | undefined` — `undefined` once the node leaves the document, so every position read guards.
- Omitting `contentDOM` makes the node a leaf the view never renders into, which is the correct shape for a node whose children a foreign renderer owns.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the view lifecycle — construct, update, destroy, all imperative.

| [INDEX] | [SURFACE]                                                       | [SHAPE]             | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------- | :------------------ | :----------------------------------------- |
|  [01]   | `new EditorView(place, DirectEditorProps)`                      | ctor                | mounts the editable DOM at `place`         |
|  [02]   | `EditorView.update(DirectEditorProps)`                          | instance            | replaces the whole prop set                |
|  [03]   | `EditorView.setProps(Partial<DirectEditorProps>)`               | instance            | patches named props, keeping the rest      |
|  [04]   | `EditorView.updateState(EditorState)`                           | instance            | the state-only fast path                   |
|  [05]   | `EditorView.destroy()` / `isDestroyed`                          | instance / property | tears down listeners and views             |
|  [06]   | `EditorView.dispatch(Transaction)`                              | property            | routes to `dispatchTransaction`            |
|  [07]   | `EditorView.state` / `dom` / `editable` / `composing` / `props` | property            | rendered state, root element, input status |
|  [08]   | `EditorView.dragging`                                           | property            | `{slice, move}` during a live drag         |
|  [09]   | `EditorView.focus()` / `hasFocus()` / `root` / `updateRoot()`   | instance / property | focus control and the owning root          |
|  [10]   | `EditorView.someProp(name, f)`                                  | instance            | folds one prop across props then plugins   |

- `dispatch` is an instance field, not a prototype method, so it detaches safely and a host may reassign it.
- `place` takes a node, a callback, or `{mount: element}`; the mount form takes over an existing element instead of appending a new one — the shape a React `ref` container uses.
- `updateRoot()` re-reads the owning document after the view moves between a shadow root and the main tree.

[ENTRYPOINT_SCOPE]: DOM and coordinate queries — the bridge between document positions and screen space.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `EditorView.posAtCoords({left, top})`                          | instance | `{pos, inside}` or `null` outside the document     |
|  [02]   | `EditorView.coordsAtPos(pos, side)`                            | instance | the client rect a floating toolbar anchors to      |
|  [03]   | `EditorView.domAtPos(pos, side)` / `nodeDOM(pos)`              | instance | the DOM node and offset behind a position          |
|  [04]   | `EditorView.posAtDOM(node, offset, bias)`                      | instance | the document position behind a DOM node            |
|  [05]   | `EditorView.endOfTextblock(dir, state)`                        | instance | bidi-aware boundary test motion commands consume   |
|  [06]   | `EditorView.pasteHTML(html, event)` / `pasteText(text, event)` | instance | drives the paste pipeline programmatically         |
|  [07]   | `EditorView.serializeForClipboard(Slice)`                      | instance | `{dom, text, slice}` for a custom copy path        |
|  [08]   | `EditorView.dispatchEvent(Event)`                              | instance | feeds a synthetic event through the input handlers |

[ENTRYPOINT_SCOPE]: decorations — presentation layered over an untouched document.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `Decoration.widget(pos, WidgetConstructor, spec)`               | static   | inserts a DOM node the document never contains            |
|  [02]   | `Decoration.inline(from, to, DecorationAttrs, spec)`            | static   | styles an inline range                                    |
|  [03]   | `Decoration.node(from, to, DecorationAttrs, spec)`              | static   | styles one node's own element                             |
|  [04]   | `DecorationSet.create(doc, Decoration[])`                       | static   | builds the indexed set; `DecorationSet.empty` is the unit |
|  [05]   | `DecorationSet.map(Mapping, doc, {onRemove})`                   | instance | carries the set across a document change                  |
|  [06]   | `DecorationSet.add(doc, Decoration[])` / `remove(Decoration[])` | instance | persistent set edits                                      |
|  [07]   | `DecorationSet.find(start, end, predicate)`                     | instance | reads decorations back by range and `spec`                |
|  [08]   | `Decoration.from` / `to` / `spec`                               | property | the mapped range and the caller's own spec object         |

- `Decoration.widget` `spec` keys: `side` (before/after ordering at one position), `relaxedSide`, `marks`, `stopEvent`, `ignoreSelection`, `key`, `destroy`; a stable `key` keeps the widget's DOM across redraws.
- `Decoration.inline` `spec` keys `inclusiveStart`/`inclusiveEnd` decide whether text typed at an edge joins the decoration.
- `DecorationSet.map` drops a decoration whose range the change deleted and reports it through `onRemove`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `EditorView` owns its DOM outright: it renders `state.doc` into a `contenteditable` element, observes mutations, and redraws by diffing the new state against the old. Any outside mutation of that subtree is read as user input or clobbered on the next redraw, so a React tree renders only the container and never a child of `view.dom`.
- `dispatch` closes the transaction round trip, the whole contract: user input produces a `Transaction`, `view.dispatch(tr)` routes it to `dispatchTransaction`, and the host applies it and calls `updateState` with the result. Omitting `dispatchTransaction` installs the default that applies and updates in place, which forfeits interception.
- Props resolve by precedence, not by merge: `someProp` walks the direct props first and then each plugin in order, taking the first contributor that answers; handler props stop at the first `true`, while `decorations` and `attributes` accumulate across every contributor.
- Node views take ownership of a subtree: the constructor returns `dom` and optionally `contentDOM`, the view renders children into `contentDOM` and nothing else, `stopEvent` keeps foreign widget events out of the editor, and `ignoreMutation` keeps foreign DOM writes from being read as edits. Node views without `contentDOM` are leaves the caller renders entirely.
- Decorations never enter the document: they live in a plugin's `DecorationSet`, map through each transaction's `Mapping`, and are handed to `EditorProps.decorations`. Presentation that must survive serialization is a mark or an attribute in the schema instead.
- `style/prosemirror.css` carries load-bearing editing rules — `white-space: pre-wrap` on `.ProseMirror`, the `.ProseMirror-hideselection` caret suppression, the `.ProseMirror-selectednode` outline, and the `img.ProseMirror-separator` protection — so a build that omits it renders collapsed whitespace and an invisible node selection.

[STACKING]:
- `prosemirror-state`(`.api/prosemirror-state.md`): `DirectEditorProps.state` is the rendered value, `plugins` adds view-only plugins beside the state's own, `PluginSpec.props` contributes `EditorProps`, and `PluginSpec.view(view)` returns the `PluginView` this package drives through `update(view, prevState)` and `destroy()`.
- `prosemirror-model`(`.api/prosemirror-model.md`): `EditorProps.domParser`/`clipboardParser` take a `DOMParser` and `clipboardSerializer` a `DOMSerializer`, so paste and copy dialects are spec rows; any node without a `nodeViews` entry renders through its `NodeSpec.toDOM`.
- `prosemirror-transform`(`.api/prosemirror-transform.md`): `DecorationSet.map(mapping, doc)` consumes `tr.mapping` directly, which is how a decoration survives a remote edit.
- `prosemirror-gapcursor`(`.api/prosemirror-gapcursor.md`) + `prosemirror-dropcursor`(`.api/prosemirror-dropcursor.md`): both draw through this package's decoration and selection plumbing and ship their own cursor CSS, so their stylesheets load beside `style/prosemirror.css`.
- `react`(`.api/react.md`): the editor mounts in a `useEffect` against a container `ref` — construct on mount, `setProps`/`updateState` on change, `destroy()` in the cleanup — and the container renders with a stable identity so reconciliation never reaches inside. React-rendered node views create their own root inside the `NodeView.dom` the constructor returns and tear it down in `destroy`.
- `@floating-ui/react`(`.api/floating-ui-react.md`): a selection toolbar anchors to a virtual element built from `view.coordsAtPos(pos)`, keeping the floating layer outside `view.dom` where the view's redraw cannot reach it.
- `isomorphic-dompurify`(`.api/isomorphic-dompurify.md`): `EditorProps.transformPastedHTML` sanitizes foreign clipboard HTML through the one `sanitize` gate before `clipboardParser` maps it onto schema rules.
- within-lib `view/content`: the editor page owns one `EditorView` per document instance behind a React container, routes `dispatchTransaction` into the state fold, and declares node views only where a schema `toDOM` cannot express the render.

[LOCAL_ADMISSION]:
- Mount one `EditorView` imperatively against a stable container ref and destroy it in the same effect's cleanup; rendering React children inside `view.dom` is the rejected shape.
- Supply `dispatchTransaction` so every change routes through the application's own state owner before `updateState`.
- Layer non-document presentation as a `DecorationSet` in a plugin field mapped by `tr.mapping`; presentation that must persist becomes a schema mark instead.
- Load `prosemirror-view/style/prosemirror.css` with the editor; whitespace, hidden selection, and node-selection rendering depend on it.
