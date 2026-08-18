# [RASM_RHINO_API_ETO_FORMS]

`Eto.Forms` builds every native surface the Rhino host embeds. The control base, layout owners, window and dialog hierarchy, grid and cell families, and popup-menu and command surface are the branch construction spine this boundary composes unchanged; the rows below are the node tree, the themed-spinner enums, and `Eto.Threading` main-thread identity that this host boundary alone reaches — the calendar, document-tab, themed-control, and menu-bar/toolbar chrome rows seat at the branch tier the kernel `Interaction` plane composes; the data-binding rail registers at the branch tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms` — Rhino host-boundary partition
- package: `Eto.Forms` (host-provided; bound in-place from the Rhino-loaded `Eto.dll`, never a second NuGet admission) (BSD-3-Clause)
- assembly: `Eto.dll` (Rhino `RhCore` framework)
- namespace: `Eto.Forms`, `Eto.Forms.ThemedControls`, `Eto.Threading`
- rail: native-ui

## [02]-[PUBLIC_TYPES]

- Registers the `Eto.Forms` construction spine (`libs/csharp/.api/api-eto-forms.md`): `Control` and its event families, the text, value, choice, command, and display roster, the container set, `Grid`/`GridView`/`TreeGridView` with the cell family, the four layout owners, the window, dialog, and chooser hierarchy, and the popup-menu and `Command` surface carry their construction there and this boundary composes that spelling; the rows below are the widgets, chrome, and rails this partition adds beyond it.

[PUBLIC_TYPE_SCOPE]: node tree

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :---------- | :------------ | :--------------------------- |
|  [01]   | `TreeView`  | tree          | node tree over `ITreeItem`   |
|  [02]   | `ITreeItem` | contract      | node contract the tree binds |

[PUBLIC_TYPE_SCOPE]: data binding

- Registers the `Eto.Forms` data-binding rail (`libs/csharp/.api/api-eto-binding.md`): `IndirectBinding<T>`, `DirectBinding<T>`, `BindableBinding<T,TValue>`, `DualBinding<T>`, `DualBindingMode`, and the `Bind`/`BindDataContext` fluent entry carry their algebra there; this boundary composes them and adds no binding carrier.

[PUBLIC_TYPE_SCOPE]: themed dialogs, editors, and thread identity

The `Eto.Forms.ThemedControls` family registers at the branch tier (`libs/csharp/.api/api-eto-forms.md`) — the kernel `Interaction/control` composes it; the spinner enums and `Eto.Threading.Thread` main-thread identity stay this boundary's alone, and the `Themed*Handler` backends register through the platform-handler seam (`libs/csharp/Rasm.Rhino/.api/api-eto-platform.md`).

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `ThemedSpinnerMode`      | enum          | themed-spinner glyph shape (`Line`/`Circle`)                   |
|  [02]   | `ThemedSpinnerDirection` | enum          | themed-spinner rotation (`Clockwise`/`CounterClockwise`)       |
|  [03]   | `Thread`                 | thread        | managed thread with `IsMain`/`MainThread` main-thread identity |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: themed controls and thread identity

| [INDEX] | [OWNER]  | [SURFACE]                                             |
| :-----: | :------- | :---------------------------------------------------- |
|  [01]   | `Thread` | action lifecycle, main and current identity, liveness |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Binding is bidirectional through `IndirectBinding<T>` under a `DualBindingMode`; `Convert`/`Child`/`AfterDelay` chain the transform, and `BindDataContext` reuses one binding graph across every data item swapped into `DataContext`, so a generator-shaped UI layer folds each widget and region as a row rather than a hand-wired call site.
- One registered `Command` projects into both a menu item and a tool item, so a command row drives the menu bar, the popup menu, and the toolbar from a single enablement and shortcut definition.
- `DocumentControl` owns closable-tab document hosting where `TabControl` owns fixed pages; the two never merge, and a closable page is a `DocumentPage`.
- Themed controls are backend classes, not construction rows: a themed message box, property grid, or collection editor registers its `Themed*Handler` at the platform seam and the widget then constructs like any other.

[STACKING]:
- `libs/csharp/.api/api-eto-forms.md`: the registered construction spine every screen composes; this boundary adds no widget the spine already carries and re-tables none.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): a `[SmartEnum]` owns the closed control-kind, cell-kind, layout-strategy, and dialog-outcome vocabularies a generator-shaped UI layer folds to rows, and a `[Union]` owns the discriminated screen-element tree; the generated `Switch`/`Map` drives construction dispatch instead of a hand-written control-type ladder.
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): `Fin<A>` rails modal outcomes and chooser results, cancellation a `Fail` rather than a null sentinel; `Option<A>` carries the nullable scale flags and optional selection; `Eff<A>` wraps drag and native-attach effects; `Seq<A>` is the child-collection carrier a layout region folds over.
- `Wacton.Unicolour`(`libs/csharp/.api/api-unicolour.md`): the canonical colour value behind the registered colour picker and chooser; the paint-edge colour maps to and from `Unicolour` (`libs/csharp/Rasm.Rhino/.api/api-eto-drawing.md`), keeping theme ramps and perceptual selection in the perceptual model.
- `libs/csharp/Rasm.Rhino/.api/api-eto-platform.md`: native hosting, the native-parent attach and detach pair, and the style re-application seam cross into the platform-handler boundary, and the `Themed*Handler` backend classes register there.
- `libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md`: a document-owned Rhino window, native styling, and semi-modal presentation come from the host bridge; this construction surface supplies the control tree the bridge presents.

[LOCAL_ADMISSION]:
- Eto is admitted from the Rhino-loaded `Eto.dll`; this boundary references that instance so its widgets share the host application, dispatcher, and platform handler, and a second copy never enters through NuGet.
- A screen is built once from generated element rows against the registered construction surface and the rails here; `Eto.Forms.*` types stay behind the UI owner and downstream code composes screen definitions rather than raw widget calls.
- `Eto.Threading.Thread` stays subordinate to the Rhino host marshal owner (`libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md`); an Eto-level main-thread test never replaces the host marshal seam.

[RAIL_LAW]:
- Partition: `Eto.Forms` Rhino host boundary — the node tree, the themed-spinner enums, and managed thread identity; chrome, calendar, document-tab, and themed-control rows ride the registered branch owner beside the data-binding rail
- Owns: the widgets, chrome, and rails this host boundary adds over the registered branch spine
- Accept: node-tree binding, themed-spinner configuration, and main-thread identity reads subordinate to the host marshal owner
- Reject: a re-tabling of the branch construction spine, immediate 2D painting (`libs/csharp/Rasm.Rhino/.api/api-eto-drawing.md`), platform-handler and native-hosting selection plus the `Themed*Handler` backends (`libs/csharp/Rasm.Rhino/.api/api-eto-platform.md`), document-owned Rhino windows and panels (`libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md`), and leaking `Eto.Forms.*` types past the UI owner
