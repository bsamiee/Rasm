# [RASM_GRASSHOPPER_API_ETO_FORMS]

`Eto.Forms` raises every GH2-hosted panel inside the Rhino process. The control base, layout owners, window and dialog hierarchy, grid and cell families, and command surface are the branch construction spine this boundary composes unchanged; the rows below are the masked and stepped field family, the exclusive option group, the rich-text buffer contract, and the tree hit-test and drop-target models the GH2 panel and canvas add beyond it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto` — Grasshopper2 panel partition
- package: `Eto` (the cross-platform Eto.Forms UI framework, host-provided by RhinoWIP) (BSD-3-Clause)
- assembly: `Eto` (`Eto.dll`)
- namespace: `Eto.Forms`
- asset: host-provided — RhinoWIP ships `Eto.dll` under `RhCore.framework/Versions/A/Resources`; no NuGet admission
- rail: native UI

## [02]-[PUBLIC_TYPES]

- Registers the `Eto.Forms` construction spine (`libs/csharp/.api/api-eto-forms.md`): `Control` and its event families, the text, value, choice, command, and display roster, the container set, `Grid`/`GridView`/`TreeGridView` with the cell family, the four layout owners, the window, dialog, and chooser hierarchy, and the popup-menu and `Command` surface carry their construction there and this boundary composes that spelling; the rows below are the widgets and models this partition adds beyond it.

[PUBLIC_TYPE_SCOPE]: masked and stepped fields

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `MaskedTextBox<T>`     | control       | format-masked text over a typed provider       |
|  [02]   | `MaskedTextStepper<T>` | control       | format-masked stepper over a typed provider    |
|  [03]   | `Stepper`              | control       | bare up and down increment affordance          |
|  [04]   | `TextStepper`          | control       | text field carrying an increment affordance    |
|  [05]   | `NumericUpDown`        | control       | numeric field carrying an increment affordance |

[PUBLIC_TYPE_SCOPE]: exclusive option group and rich-text buffer

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------- | :------------ | :--------------------------------------------- |
|  [01]   | `RadioButtonList` | control       | mutually-exclusive option group over one store |
|  [02]   | `ITextBuffer`     | interface     | range formatting and rich-text document IO     |

[PUBLIC_TYPE_SCOPE]: tree hit-test and drop-target models

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `TreeGridCell`         | model         | resolved hit test carrying item, column, and kind              |
|  [02]   | `TreeGridViewDragInfo` | model         | drop-target descriptor carrying item, parent, and insert index |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tree hit-test and drop-target resolution

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]           |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `TreeGridView.GetCellAt(PointF) -> TreeGridCell`                  | instance | hit-test resolution    |
|  [02]   | `TreeGridView.GetDragInfo(DragEventArgs) -> TreeGridViewDragInfo` | instance | drop-target resolution |

- `TreeGridCell`: `Item` `Column` `ColumnIndex` `Type`; `TreeGridViewDragInfo`: `Item` `Parent` `Position` `InsertIndex`.
- `ITextBuffer`: `SetBold` `SetItalic` `SetFont` `SetForeground` `SetBackground` `Insert` `Delete` `Clear` `Load` `Save(Stream, RichTextAreaFormat)`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A masked field owns its own format contract: `MaskedTextBox<T>` and `MaskedTextStepper<T>` take a typed provider, so a format is a provider value and never a validation ladder beside a plain text field.
- Increment is an affordance, not a widget kind: `Stepper` is the bare affordance and `TextStepper`/`NumericUpDown` are the fields that carry it, so a stepped variant of an existing field composes the affordance rather than forking the field roster.
- Tree interaction resolves through the two model returns alone — `GetCellAt` answers what the pointer is over and `GetDragInfo` answers where a drop lands, so a canvas drag reads one descriptor rather than reconstructing the target from coordinates.

[STACKING]:
- `api-eto-forms`(`libs/csharp/.api/api-eto-forms.md`): the registered construction spine every panel composes — a `Panel`/`Scrollable` root holds one layout owner and the layout holds the field, data-view, and container roster, with the rows here seated among them.
- `api-eto-binding`(`libs/csharp/.api/api-eto-binding.md`): every field and view exposes its `*Binding`, the seam the branch-tier binding rail fuses to a `DataContext`.
- `api-eto-drawing`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-drawing.md`): the registered `Drawable` paint seam hands its context to the drawing surface for owner-drawn content.
- `api-eto-runtime`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-runtime.md`): dialog presentation, control invalidation, and cross-thread mutation marshal through the registered application singleton.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): a masked field's typed provider and a bounded field value are `[ValueObject<T>]` owners the control binding reads and writes; the tree hit-test kind is a `[SmartEnum]` case the drag policy dispatches on.
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): `Optional(view.GetCellAt(point))` null-gates a hit test into `Option<TreeGridCell>` and a drop-target read folds to `Fin<TreeGridViewDragInfo>` before a canvas commits the move.

[LOCAL_ADMISSION]:
- A panel subclasses a registered control or composes the registered roster directly; a new control capability lands as a subclass or a composition, never a wrapper renaming a host member or a re-implemented native widget.
- A format-masked or increment-carrying field takes the rows here; a hand-rolled mask parser or spinner pair beside them is the deleted form.
- Boundary faults lower onto the LanguageExt rail.

[RAIL_LAW]:
- Partition: `Eto.Forms` Grasshopper2 panel boundary — masked and stepped fields, exclusive option group, rich-text buffer, tree hit-test and drop-target models
- Owns: the widgets and interaction models the GH2 panel adds over the registered branch spine
- Accept: format-masked entry, increment affordances, an exclusive option group over one store, rich-text range formatting, tree hit-test and drop-target resolution
- Reject: a re-tabling of the branch construction spine, a hand-rolled mask parser or spinner pair, a tree drop target reconstructed from coordinates, immediate 2D painting (`api-eto-drawing`), and platform-handler selection (`api-eto-platform`)
