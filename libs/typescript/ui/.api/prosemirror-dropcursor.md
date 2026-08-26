# [TS_UI_API_PROSEMIRROR_DROPCURSOR]

`prosemirror-dropcursor` owns the drop-target indicator: `dropCursor(options)` draws a cursor at the position a dragged slice lands, tracking the pointer through the drag and clearing on drop or leave. Placement respects the schema — the cursor appears only where the drop is admissible — and a node opts its interior out through the `disableDropCursor` spec key this package declares.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one options interface and one module augmentation this package contributes to the schema surface.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `DropCursorOptions`          | interface     | `{color?, width?, class?}` — appearance only                           |
|  [02]   | `NodeSpec.disableDropCursor` | interface     | the augmentation: `boolean \| (view, {pos, inside}, event) => boolean` |

- `DropCursorOptions` is declared but not exported; annotate structurally or inline the object at the `dropCursor(...)` call.
- `color` defaults to `black` and takes `false` to drop the inline colour so a `class` rule owns appearance; `width` defaults to 1 pixel.
- Importing this package augments `NodeSpec` globally through a `declare module "prosemirror-model"` block, so `disableDropCursor` becomes a typed spec key everywhere in the compilation.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the one exported surface.

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------- | :------ | :-------------------------------------------------------- |
|  [01]   | `dropCursor({color, width, class}) -> Plugin` | static  | tracks the drag and draws the cursor at the drop position |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `dropCursor` layers presentation over the view's own drag handling: it watches drag events, resolves the pointer to a drop position, and draws an indicator there. It performs no drop — the view applies the change, so removing the plugin costs the indicator and nothing else.
- Placement is schema-aware: the cursor appears where the dragged slice can legally land, so an inadmissible target shows nothing and the user reads the constraint from the absence.
- Opting out is a spec row: `NodeSpec.disableDropCursor` takes `true` to suppress the cursor inside that node or a `(view, {pos, inside}, event) => boolean` predicate for a position-dependent answer — the shape a node whose interior a foreign renderer owns declares.
- Appearance is inline by default and overridable by class: `color: false` and a `class` move the whole appearance into the design system's own stylesheet, which is how the indicator picks up the token colour scale instead of hardcoded black.

[STACKING]:
- `prosemirror-view`(`.api/prosemirror-view.md`): the plugin reads the drag through the view and positions its element off `EditorView.coordsAtPos`; `EditorView.dragging` carries the `{slice, move}` the admissibility test consults.
- `prosemirror-model`(`.api/prosemirror-model.md`): the `declare module "prosemirror-model"` augmentation adds `disableDropCursor` to `NodeSpec`, so a node declares its interior policy on the same row that declares its content and DOM mapping.
- `prosemirror-transform`(`.api/prosemirror-transform.md`): `dropPoint(doc, pos, slice)` is the admissibility rule behind where the cursor may appear, so indicator and drop agree by construction.
- `prosemirror-state`(`.api/prosemirror-state.md`): the plugin is one `Plugin` holding its own tracking state and contributing view props; it dispatches nothing.
- `@use-gesture/react`(`.api/use-gesture-react.md`): the branch's gesture layer owns pointer gestures outside the editor; a drag into `view.dom` hands off to the view's native drag pipeline, so the two never track one pointer at once.
- `class-variance-authority`(`.api/class-variance-authority.md`): `color: false` and a `class` from the one `cn` fold style the indicator on the token scale rather than the package default.
- within-lib `view/content`: any document class admitting draggable block nodes mounts `dropCursor()` with the branch's own indicator class, and declares `disableDropCursor` on the specs whose interiors reject drops.

[LOCAL_ADMISSION]:
- Mount `dropCursor()` in every document class with draggable nodes, styling it through `color: false` and a `class` on the token scale.
- Declare `disableDropCursor` on the owning `NodeSpec` where a node's interior rejects drops, rather than filtering drag events.
- Keep drop behaviour on the view and its `handleDrop` prop; this plugin owns the indicator alone.
