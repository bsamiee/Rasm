# [RASM_RHINO_API_ETO_FORMS]

`Eto.Forms` builds every native surface the Rhino host embeds. The control base, layout owners, window and dialog hierarchy, grid and cell families, and popup-menu and command surface are the branch construction spine this boundary composes unchanged; the rows below are the calendar and document-tab widgets, the node tree, the application menu-bar and toolbar chrome, the `Eto.Forms.ThemedControls` custom-drawn family, and `Eto.Threading` main-thread identity that this host boundary alone reaches; the data-binding rail registers at the branch tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms` — Rhino host-boundary partition
- package: `Eto.Forms` (host-provided; bound in-place from the Rhino-loaded `Eto.dll`, never a second NuGet admission) (BSD-3-Clause)
- assembly: `Eto.dll` (Rhino `RhCore` framework)
- namespace: `Eto.Forms`, `Eto.Forms.ThemedControls`, `Eto.Threading`
- rail: native-ui

## [02]-[PUBLIC_TYPES]

- Registers the `Eto.Forms` construction spine (`libs/csharp/.api/api-eto-forms.md`): `Control` and its event families, the text, value, choice, command, and display roster, the container set, `Grid`/`GridView`/`TreeGridView` with the cell family, the four layout owners, the window, dialog, and chooser hierarchy, and the popup-menu and `Command` surface carry their construction there and this boundary composes that spelling; the rows below are the widgets, chrome, and rails this partition adds beyond it.

[PUBLIC_TYPE_SCOPE]: calendar, document tabs, and node tree

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :---------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Calendar`        | value input   | month-grid date selection with a min/max range   |
|  [02]   | `DocumentControl` | container     | closable, reorderable document-tab host          |
|  [03]   | `DocumentPage`    | container     | one closable document tab over a content control |
|  [04]   | `TreeView`        | tree          | node tree over `ITreeItem`                       |
|  [05]   | `ITreeItem`       | contract      | node contract the tree binds                     |

[PUBLIC_TYPE_SCOPE]: application menu bar and toolbar chrome

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------------ | :------------ | :-------------------------------------- |
|  [01]   | `MenuBar`           | menu          | top-level application menu              |
|  [02]   | `CheckMenuItem`     | menu item     | checkable menu entry                    |
|  [03]   | `RadioMenuItem`     | menu item     | radio-grouped menu entry                |
|  [04]   | `SeparatorMenuItem` | menu item     | menu divider                            |
|  [05]   | `ToolBar`           | toolbar       | control toolbar over `ToolItem` entries |
|  [06]   | `ButtonToolItem`    | tool item     | invoking toolbar button                 |
|  [07]   | `CheckToolItem`     | tool item     | toggle toolbar button                   |
|  [08]   | `DropDownToolItem`  | tool item     | toolbar button carrying a dropdown menu |
|  [09]   | `SeparatorToolItem` | tool item     | toolbar divider                         |

[PUBLIC_TYPE_SCOPE]: data binding

- Registers the `Eto.Forms` data-binding rail (`libs/csharp/.api/api-eto-binding.md`): `IndirectBinding<T>`, `DirectBinding<T>`, `BindableBinding<T,TValue>`, `DualBinding<T>`, `DualBindingMode`, and the `Bind`/`BindDataContext` fluent entry carry their algebra there; this boundary composes them and adds no binding carrier.

[PUBLIC_TYPE_SCOPE]: themed dialogs, editors, and thread identity

`Eto.Forms.ThemedControls` mints the custom-drawn, cross-platform-uniform family; its `Themed*Handler` backend classes register through the platform-handler seam (`api-eto-platform.md`), never a widget-construction row. `Eto.Threading.Thread` is the managed thread abstraction carrying main-thread identity.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `ThemedMessageBox`       | dialog        | themed modal message box with arbitrary result-typed buttons    |
|  [02]   | `ThemedPropertyGrid`     | control       | themed reflected property editor over one or many bound objects |
|  [03]   | `ThemedCollectionEditor` | control       | themed add and remove editor over a homogeneous collection      |
|  [04]   | `ThemedSpinnerMode`      | enum          | themed-spinner glyph shape (`Line`/`Circle`)                    |
|  [05]   | `ThemedSpinnerDirection` | enum          | themed-spinner rotation (`Clockwise`/`CounterClockwise`)        |
|  [06]   | `Thread`                 | thread        | managed thread with `IsMain`/`MainThread` main-thread identity  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: calendar, document tabs, and command projection

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Calendar.MinDate { get; set; }` / `Calendar.MaxDate { get; set; }`     | property | bound the selectable dates                |
|  [02]   | `DocumentControl.Pages / SelectedIndex / AllowReordering { get; set; }` | property | closable, reorderable tab host state      |
|  [03]   | `DocumentPage.Content / Text / Closable { get; set; }`                  | property | one closable document tab                 |
|  [04]   | `Command.CreateMenuItem() -> MenuItem`                                  | instance | project the command into one menu item    |
|  [05]   | `Command.CreateToolItem() -> ToolItem`                                  | instance | project the command into one toolbar item |

[ENTRYPOINT_SCOPE]: themed controls and thread identity

| [INDEX] | [OWNER]                  | [SURFACE]                                             |
| :-----: | :----------------------- | :---------------------------------------------------- |
|  [01]   | `ThemedMessageBox`       | `AddButton`; result, text, alignment, image           |
|  [02]   | `ThemedPropertyGrid`     | selection, categories, description, refresh, change   |
|  [03]   | `ThemedCollectionEditor` | `DataStore`, `ElementType`, `ExtraContent`            |
|  [04]   | `Thread`                 | action lifecycle, main and current identity, liveness |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Binding is bidirectional through `IndirectBinding<T>` under a `DualBindingMode`; `Convert`/`Child`/`AfterDelay` chain the transform, and `BindDataContext` reuses one binding graph across every data item swapped into `DataContext`, so a generator-shaped UI layer folds each widget and region as a row rather than a hand-wired call site.
- One registered `Command` projects into both a menu item and a tool item, so a command row drives the menu bar, the popup menu, and the toolbar from a single enablement and shortcut definition.
- `DocumentControl` owns closable-tab document hosting where `TabControl` owns fixed pages; the two never merge, and a closable page is a `DocumentPage`.
- Themed controls are backend classes, not construction rows: a themed message box, property grid, or collection editor registers its `Themed*Handler` at the platform seam and the widget then constructs like any other.

[STACKING]:
- `api-eto-forms`(`../../.api/api-eto-forms.md`): the registered construction spine every screen composes; this boundary adds no widget the spine already carries and re-tables none.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): a `[SmartEnum]` owns the closed control-kind, cell-kind, layout-strategy, and dialog-outcome vocabularies a generator-shaped UI layer folds to rows, and a `[Union]` owns the discriminated screen-element tree; the generated `Switch`/`Map` drives construction dispatch instead of a hand-written control-type ladder.
- `LanguageExt.Core`(`../../.api/api-languageext.md`): `Fin<A>` rails modal outcomes and chooser results, cancellation a `Fail` rather than a null sentinel; `Option<A>` carries the nullable scale flags and optional selection; `Eff<A>` wraps drag and native-attach effects; `Seq<A>` is the child-collection carrier a layout region folds over.
- `Wacton.Unicolour`(`../../.api/api-unicolour.md`): the canonical colour value behind the registered colour picker and chooser; the paint-edge colour maps to and from `Unicolour` (`api-eto-drawing.md`), keeping theme ramps and perceptual selection in the perceptual model.
- `api-eto-platform`(`api-eto-platform.md`): native hosting, the native-parent attach and detach pair, and the style re-application seam cross into the platform-handler boundary, and the `Themed*Handler` backend classes register there.
- `api-rhino-ui`(`api-rhino-ui.md`): a document-owned Rhino window, native styling, and semi-modal presentation come from the host bridge; this construction surface supplies the control tree the bridge presents.

[LOCAL_ADMISSION]:
- Eto is admitted from the Rhino-loaded `Eto.dll`; this boundary references that instance so its widgets share the host application, dispatcher, and platform handler, and a second copy never enters through NuGet.
- A screen is built once from generated element rows against the registered construction surface and the rails here; `Eto.Forms.*` types stay behind the UI owner and downstream code composes screen definitions rather than raw widget calls.
- `Eto.Threading.Thread` stays subordinate to the Rhino host marshal owner (`api-rhino-ui.md`); an Eto-level main-thread test never replaces the host marshal seam.

[RAIL_LAW]:
- Partition: `Eto.Forms` Rhino host boundary — calendar and document-tab widgets, the node tree, application menu-bar and toolbar chrome, the themed control family, and managed thread identity; the data-binding rail rides the registered branch owner
- Owns: the widgets, chrome, and rails this host boundary adds over the registered branch spine
- Accept: date and document-tab construction, node-tree binding, menu-bar and toolbar chrome from one command row, control-to-model binding through `DataContext`, a themed message box, property grid, or collection editor
- Reject: a re-tabling of the branch construction spine, immediate 2D painting (`api-eto-drawing.md`), platform-handler and native-hosting selection plus the `Themed*Handler` backends (`api-eto-platform.md`), document-owned Rhino windows and panels (`api-rhino-ui.md`), and leaking `Eto.Forms.*` types past the UI owner
