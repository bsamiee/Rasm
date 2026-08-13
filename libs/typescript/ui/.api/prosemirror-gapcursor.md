# [TS_UI_API_PROSEMIRROR_GAPCURSOR]

`prosemirror-gapcursor` owns the cursor position no text can hold: `GapCursor` is a registered `Selection` subclass pointing at a gap beside an unselectable block, and `gapCursor()` captures the clicks and arrow motion that land there. Without it a document ending in a block node — a table, an image, a viewer embed — leaves no place to type, and the plugin's own stylesheet is what makes the cursor visible.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `prosemirror-gapcursor`
- package: `prosemirror-gapcursor` (MIT)
- module: `type: module`, `.` entry and a `./style/gapcursor.css` subpath; `sideEffects` names that stylesheet alone
- runtime: browser input — the plugin binds its own keymap and click handling and draws through a widget decoration
- depends: `prosemirror-state`, `prosemirror-model`, `prosemirror-view`, `prosemirror-keymap`
- rail: `view/content` — the gap-selection plane beside block nodes

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one selection class; the plugin takes no configuration.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :---------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `GapCursor` | class         | `Selection` whose `$anchor` and `$head` both name the gap |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the plugin and the selection it mints.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `gapCursor() -> Plugin`                                                     | static   | captures clicks and arrow motion into gaps |
|  [02]   | `new GapCursor(ResolvedPos)`                                                | ctor     | gap selection at one resolved position     |
|  [03]   | `GapCursor.map(doc, Mappable)` / `eq(Selection)` / `content()` / `toJSON()` | instance | `Selection` duties at a gap                |

- `gapCursor()` takes no options; where the cursor may appear is decided per node, not per plugin.
- `GapCursor.content()` returns an empty slice.
- `GapCursor` registers itself under the `"gapcursor"` selection JSON id at import, so a serialized state holding one rehydrates through `Selection.fromJSON` as soon as the module loads.
- `gapCursor()` renders the cursor as an element with class `ProseMirror-gapcursor`, invisible until `style/gapcursor.css` or an equivalent rule loads.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Gap positions exist wherever the document holds no selectable text: before or after a leaf or unselectable block, between two block nodes, and at a document edge that begins or ends with one. `gapCursor()` captures arrow motion and clicks near such a position and mints a `GapCursor` there so typing has a landing place.
- `GapCursor` is a real selection, not a decoration: it participates in `Selection.map`, serializes through the registered `"gapcursor"` id, and any command reading `state.selection` sees it. Commands branching only on `TextSelection` and `NodeSelection` therefore decline silently once this plugin is mounted, which is the case to cover when adding it to an existing editor.
- Placement is a per-node decision read off the spec: the plugin consults `allowGapCursor` on a node's spec to override its default judgement. `NodeSpec` carries an open index signature, so that key compiles without a declared augmentation and stays an untyped escape hatch a document class sets deliberately.
- `gapCursor()` mounts its own arrow keymap internally, so a custom arrow binding's precedence relative to it in the plugin array decides which claims the key.
- Visibility is the caller's: the drawn element carries only the `ProseMirror-gapcursor` class, so a build that skips `style/gapcursor.css` renders a working but invisible cursor.

[STACKING]:
- `prosemirror-state`(`.api/prosemirror-state.md`): `GapCursor extends Selection` and registers through `Selection.jsonID("gapcursor", GapCursor)`, so `EditorState.toJSON`/`fromJSON` round-trips a document whose selection sits in a gap.
- `prosemirror-view`(`.api/prosemirror-view.md`): the plugin draws through a widget decoration and reads clicks through the view's own handlers, so its stylesheet loads beside `prosemirror-view/style/prosemirror.css`.
- `prosemirror-keymap`(`.api/prosemirror-keymap.md`): the plugin mounts an internal keymap for arrow motion; a document class with its own arrow bindings orders them by intent in the plugin array.
- `prosemirror-model`(`.api/prosemirror-model.md`): `NodeSpec.allowGapCursor` rides the spec's open index signature, and the plugin's default judgement reads `isLeaf`, `isTextblock`, and `selectable` off the compiled `NodeType`.
- `prosemirror-commands`(`.api/prosemirror-commands.md`): a command chain that must act at a gap tests for a `GapCursor` selection before falling through to the text-selection verbs.
- within-lib `view/content`: any document class admitting block-level embeds — a viewer node, a table, a chart — mounts `gapCursor()` and declares `allowGapCursor` on the specs whose default judgement is wrong.

[LOCAL_ADMISSION]:
- Mount `gapCursor()` in every document class holding a leaf or unselectable block node, and load `prosemirror-gapcursor/style/gapcursor.css` with it.
- Handle `GapCursor` in any command that branches on the selection class; a two-way branch over text and node selections declines at a gap.
- Override placement with `allowGapCursor` on the owning `NodeSpec` rather than by filtering selections after the fact.

[RAIL_LAW]:
- Package: `prosemirror-gapcursor`
- Owns: the gap-selection plane — the `GapCursor` selection class registered under the `"gapcursor"` JSON id, the `gapCursor()` plugin capturing clicks and arrow motion into gap positions, the per-node `allowGapCursor` spec override, and the `ProseMirror-gapcursor` render class with its shipped stylesheet
- Accept: one `gapCursor()` plugin per document class holding block-level embeds, the package stylesheet loaded with it, `allowGapCursor` declared on the specs needing a non-default answer, and commands that recognize a `GapCursor` selection
- Reject: a document class with unselectable block nodes and no gap-cursor plugin, the plugin mounted without its stylesheet, a selection branch covering only text and node selections, and a post-hoc selection filter where an `allowGapCursor` spec row decides
