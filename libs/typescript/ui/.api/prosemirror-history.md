# [TS_UI_API_PROSEMIRROR_HISTORY]

`prosemirror-history` owns undo: the `history(config)` plugin folds every transaction's steps into rope-backed undo and redo branches, groups adjacent changes by time and adjacency, and rebases its branches over remote steps so undo never reverts another client's work. `undo`/`redo` are plain `Command` values a keymap binds, and the depth readers drive control enablement.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `prosemirror-history`
- package: `prosemirror-history` (MIT)
- module: `type: module`, `sideEffects: false`, one `.` entry with dual `import`/`require` conditions and bundled `.d.ts`/`.d.cts`
- runtime: pure state folding; the branch store is an immutable rope, so a deep history costs no copy per edit
- depends: `prosemirror-state`, `prosemirror-transform`, `prosemirror-view` for types, and `rope-sequence` for the branch store
- rail: `view/content` — the undo plane over the transaction stream

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one configuration interface; the plugin's own field stays private and is reached through the depth readers.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `HistoryOptions` | interface     | `{depth?, newGroupDelay?}` — retention and grouping |

- `depth` defaults to 100 events, discarding the oldest beyond it; `newGroupDelay` defaults to 500 milliseconds.
- `HistoryOptions` is declared but not exported; annotate the config structurally or inline it at the `history(...)` call.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the plugin, the commands, and the readers.

| [INDEX] | [SURFACE]                                   | [SHAPE] | [CAPABILITY]                                                |
| :-----: | :------------------------------------------ | :------ | :---------------------------------------------------------- |
|  [01]   | `history({depth, newGroupDelay}) -> Plugin` | static  | folds steps into undo and redo branches                     |
|  [02]   | `undo` / `redo`                             | static  | `Command` values that revert or replay one event and scroll |
|  [03]   | `undoNoScroll` / `redoNoScroll`             | static  | the same verbs without the scroll-into-view request         |
|  [04]   | `undoDepth(state)` / `redoDepth(state)`     | static  | the available event counts, for control enablement          |
|  [05]   | `closeHistory(tr) -> Transaction`           | static  | seals the current event so later steps start a new one      |
|  [06]   | `isHistoryTransaction(tr) -> boolean`       | static  | reports that the history plugin produced the transaction    |

- `undoDepth`/`redoDepth` are declared returning `any`; they yield a number and a call site annotates it.
- `closeHistory` returns the same transaction for chaining, marking a boundary rather than clearing the stacks.
- `isHistoryTransaction` is what a fold uses to skip re-deriving a fact from a replayed change.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `history()` keeps one plugin state field over the transaction stream: each transaction's steps append to the undo branch with their inverted forms, and `undo` pops one event, applies the inversions, and pushes the result onto the redo branch. Nothing outside the field stores history, so a state clone carries its own.
- Events group by adjacency and time: consecutive changes within `newGroupDelay` that touch adjoining positions fold into one undo step, while a non-adjacent change always opens a new group. `closeHistory(tr)` forces a boundary — the mechanism for making a programmatic edit separately undoable.
- Transactions opt out by metadata: `tr.setMeta("addToHistory", false)` keeps a change out of the undo branch entirely, which is how a remote change, a decoration refresh, or a derived edit stays outside the user's undo.
- Remote changes rebase the branches rather than entering them: steps arriving through the collaborative fold map the stored inversions forward, so `undo` reverts this client's own last event and never another client's work.
- Redo clears on new input: any user change after an undo drops the redo branch, matching the linear undo model users expect.
- `undo` and `redo` request a scroll into view; `undoNoScroll`/`redoNoScroll` are the same verbs for a caller that owns scrolling, such as a viewport-stable programmatic revert.

[STACKING]:
- `prosemirror-state`(`.api/prosemirror-state.md`): the plugin is one `StateField` folded from every transaction, keyed off `tr.isGeneric`, `tr.time`, and the `"addToHistory"` metadata `tr.setMeta` writes; `undo`/`redo` are that package's `Command` alias.
- `prosemirror-transform`(`.api/prosemirror-transform.md`): the branch stores each step's `Step.invert(doc)` against the pre-step document `Transform.docs` retains, and rebasing maps those inversions through `tr.mapping`.
- `prosemirror-keymap`(`.api/prosemirror-keymap.md`): `keymap({"Mod-z": undo, "Shift-Mod-z": redo, "Mod-y": redo})` mounts the verbs; the plugin ships no bindings of its own.
- `prosemirror-collab`(`.api/prosemirror-collab.md`): remote steps arriving through `receiveTransaction` rebase the history branches, so the two plugins compose with no coordination — both fold the same transaction stream.
- `prosemirror-inputrules`(`.api/prosemirror-inputrules.md`): `undoInputRule` reverts the last applied rule and chains ahead of `undo` on Backspace, so one press undoes the autoformat rather than the typing.
- `react-aria-components`(`.api/react-aria-components.md`): an undo control's `isDisabled` reads `undoDepth(view.state) === 0` and its `onPress` runs `undo(view.state, view.dispatch)`, both from the same state value.
- `core/state/fold`: `Fold` owns lawful keyed folds with ordinal replay; a fold derived from editor changes skips a transaction where `isHistoryTransaction(tr)` holds, so an undo replays the document without re-emitting derived facts.
- within-lib `view/content`: the editor page mounts one `history()` per editor with its retention policy declared, binds the verbs through the keymap, and marks every programmatic edit either `addToHistory: false` or `closeHistory` by intent.

[LOCAL_ADMISSION]:
- Mount exactly one `history()` plugin per editor and let the field own every undo fact; a parallel snapshot stack beside it diverges on the first rebase.
- Mark a programmatic or derived edit with `tr.setMeta("addToHistory", false)`, and seal a boundary with `closeHistory(tr)` where an edit must be separately undoable.
- Read `undoDepth`/`redoDepth` for control enablement rather than tracking counts alongside.
- Bind the verbs through a `keymap` plugin at the precedence the document class requires.

[RAIL_LAW]:
- Package: `prosemirror-history`
- Owns: the undo plane — the `history({depth, newGroupDelay})` plugin folding steps into rope-backed undo and redo branches with time-and-adjacency grouping and collaborative rebasing; the `undo`/`redo` and `undoNoScroll`/`redoNoScroll` command values; the `undoDepth`/`redoDepth` readers; the `closeHistory(tr)` boundary; and the `isHistoryTransaction(tr)` provenance test
- Accept: one plugin instance per editor with declared retention, verbs bound through `keymap`, `addToHistory: false` metadata on derived and remote edits, `closeHistory` for a deliberate boundary, depth readers behind control enablement, and `isHistoryTransaction` as the guard on any fold derived from editor changes
- Reject: a snapshot stack or document-copy history beside the plugin, a second `history()` instance in one plugin roster, a programmatic edit landing in the user's undo unmarked, a hand-tracked undo counter where the depth readers answer, and an undo path that reverts a remote client's change
