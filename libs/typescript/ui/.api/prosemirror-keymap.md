# [TS_UI_API_PROSEMIRROR_KEYMAP]

`prosemirror-keymap` owns key-to-command binding: `keymap(bindings)` turns one plain record of key names to `Command` values into a `Plugin` whose `handleKeyDown` dispatches them, and `keydownHandler(bindings)` exposes the same matcher for a caller that owns its own handler. Key names normalize modifiers and resolve `Mod-` per platform, so one binding table serves every host.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `prosemirror-keymap`
- package: `prosemirror-keymap` (MIT)
- module: `type: module`, `sideEffects: false`, one `.` entry with dual `import`/`require` conditions and bundled `.d.ts`/`.d.cts`
- runtime: browser input — matches against `KeyboardEvent.key` and `keyCode`, and reads the platform to resolve `Mod-`
- depends: `prosemirror-state` for `Plugin`/`Command`, `w3c-keyname` for the key-name normalization table
- rail: `view/content` — the key-binding plane over the command roster

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: this package exports no type of its own; both surfaces take one structural binding record whose values are the `Command` alias `prosemirror-state`(`.api/prosemirror-state.md`) declares.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `{[key: string]: Command}` | interface     | the binding table; keys are normalized key names |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two surfaces — one plugin, one bare handler.

| [INDEX] | [SURFACE]                                              | [SHAPE] | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------------- | :------ | :----------------------------------------------------------------- |
|  [01]   | `keymap(bindings) -> Plugin`                           | static  | a plugin whose `props.handleKeyDown` runs the matched command      |
|  [02]   | `keydownHandler(bindings) -> (view, event) => boolean` | static  | the matcher alone, for a caller-owned `handleKeyDown` or node view |

- Every binding receives `(state, dispatch, view)` — the view rides along outside the command protocol, which is what lets a binding reach the DOM directly.
- Both surfaces return `true` when a binding claimed the key, which is what stops the event from reaching lower-precedence plugins and the browser.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Key names are a normalized grammar: zero or more modifier prefixes followed by one key identifier drawn from `KeyboardEvent.key`. Modifiers are `Shift-`/`s-`, `Alt-`/`a-`, `Ctrl-`/`c-`/`Control-`, and `Cmd-`/`m-`/`Meta-`, given in any order; `Mod-` resolves to `Cmd-` on Mac and `Ctrl-` elsewhere, and `Space` aliases `" "`. Lowercase letters name the bare key and uppercase letters imply shift, so a shift-produced character never takes an explicit `Shift-` prefix.
- Precedence is plugin order: several keymaps coexist and the earliest in the plugin array dispatches first, stopping at the first binding returning `true`. Schema-specific keymaps mount ahead of `keymap(baseKeymap)`, and one key covering several document shapes chains its fallbacks with `chainCommands` inside a single binding.
- `keymap` holds no state: it contributes `handleKeyDown` alone, so mounting, reconfiguring, and unmounting a keymap costs nothing and a binding table swaps by `EditorState.reconfigure`.
- `keydownHandler` is the same matcher without the plugin wrapper — the shape a node view or a foreign input surface uses to honour editor bindings inside DOM the view does not own.

[STACKING]:
- `prosemirror-commands`(`.api/prosemirror-commands.md`): `keymap(baseKeymap)` is the boot binding at the lowest precedence, and every custom binding composes that package's verbs with `chainCommands` rather than reimplementing them.
- `prosemirror-state`(`.api/prosemirror-state.md`): the returned `Plugin` contributes `props.handleKeyDown` and nothing else; the binding values are that package's `Command` alias.
- `prosemirror-history`(`.api/prosemirror-history.md`): `keymap({"Mod-z": undo, "Shift-Mod-z": redo, "Mod-y": redo})` is the standard mount, the history plugin itself carrying no bindings.
- `prosemirror-schema-list`(`.api/prosemirror-schema-list.md`): list bindings mount ahead of the base table — `chainCommands(splitListItem(itemType), splitBlock)` on Enter, `liftListItem`/`sinkListItem` on Shift-Tab and Tab.
- `prosemirror-gapcursor`(`.api/prosemirror-gapcursor.md`): the gap-cursor plugin mounts its own arrow-key keymap internally, so its plugin order relative to a custom arrow binding decides which wins.
- `prosemirror-view`(`.api/prosemirror-view.md`): `keydownHandler` is what a `NodeView` calls from its own listener so a widget's inner DOM still honours the editor's bindings while `stopEvent` keeps the rest out.
- `cmdk`(`.api/cmdk.md`): the palette owns its own list keyboard while the editor keeps its bindings; the palette mounts outside `view.dom`, so no key reaches both machines.
- within-lib `view/content`: the editor page declares one binding table per document class, ordering schema-specific keymaps ahead of `baseKeymap` and naming each shortcut's command as the same value the toolbar dispatches.

[LOCAL_ADMISSION]:
- Declare bindings as one data record per document class and mount them as `keymap(...)` plugins ordered by precedence; a hand-written `handleKeyDown` with a key-comparison ladder is the rejected shape.
- Write `Mod-` for the primary modifier so one table serves both platforms.
- Cover a key's several document shapes with `chainCommands` inside one binding rather than by splitting the binding across keymaps.
- Reach for `keydownHandler` only inside DOM the editor view does not own.

[RAIL_LAW]:
- Package: `prosemirror-keymap`
- Owns: key-to-command binding — the normalized key-name grammar with its modifier aliases and platform-resolved `Mod-`, `keymap(bindings)` producing a stateless `handleKeyDown` plugin, and `keydownHandler(bindings)` exposing the matcher for foreign DOM
- Accept: one binding record per document class mounted as an ordered `keymap` plugin, `Mod-` for the primary modifier, `chainCommands` inside a binding for per-key fallback, `keymap(baseKeymap)` at the lowest precedence, and `keydownHandler` inside a node view's own listener
- Reject: a hand-written key-comparison ladder in `handleKeyDown`, a platform branch where `Mod-` resolves, an explicit `Shift-` on a shift-produced character, a binding that duplicates a command body instead of naming the command value, and a second keymap minted to hold what a `chainCommands` fallback covers
