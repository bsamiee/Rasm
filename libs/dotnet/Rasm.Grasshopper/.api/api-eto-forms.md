# [RASM_GRASSHOPPER_API_ETO_FORMS]

`Eto.Forms` raises every GH2-hosted panel inside the Rhino process. Control base, layout owners, window and dialog hierarchy, grid and cell families, and command surface are the branch construction spine this boundary composes unchanged; the rows below are the masked and stepped field family, the exclusive option group, the rich-text buffer contract, and the tree hit-test and drop-target models the GH2 panel and canvas add beyond it.

## [01]-[PUBLIC_TYPES]

- Registers the `Eto.Forms` construction spine (`libs/dotnet/.api/api-eto-forms.md`): `Control` and its event families, the text, value, choice, command, and display roster, the container set, `Grid`/`GridView`/`TreeGridView` with the cell family, the four layout owners, the window, dialog, and chooser hierarchy, and the popup-menu and `Command` surface carry their construction there and this boundary composes that spelling; the rows below are the widgets and models this partition adds beyond it.

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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tree hit-test and drop-target resolution

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]           |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `TreeGridView.GetCellAt(PointF) -> TreeGridCell`                  | instance | hit-test resolution    |
|  [02]   | `TreeGridView.GetDragInfo(DragEventArgs) -> TreeGridViewDragInfo` | instance | drop-target resolution |

- `TreeGridCell`: `Item` `Column` `ColumnIndex` `Type`; `TreeGridViewDragInfo`: `Item` `Parent` `Position` `InsertIndex`.
- `ITextBuffer`: `SetBold` `SetItalic` `SetFont` `SetForeground` `SetBackground` `Insert` `Delete` `Clear` `Load` `Save(Stream, RichTextAreaFormat)`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Masked field owns its own format contract: `MaskedTextBox<T>` and `MaskedTextStepper<T>` take a typed provider, so a format is a provider value and never a validation ladder beside a plain text field.
- Increment is an affordance, not a widget kind: `Stepper` is the bare affordance and `TextStepper`/`NumericUpDown` are the fields that carry it, so a stepped variant of an existing field composes the affordance rather than forking the field roster. `NumericUpDown` ships `[Obsolete]` — `NumericStepper` is the live spelling and the obsolete field never enters a fence.
- `MaskedTextStepper<T>` constructs bare or over `(IMaskedTextProvider<T>)` — no string-mask constructor exists, so it composes a typed provider, never `FixedMaskedTextProvider`'s string mask.
- Owner-drawn and templated cells: `DrawableCell`'s whole hook is its `Paint` event; `CustomCell` carries the `CreateCell`/`ConfigureCell`/`GetIdentifier`/`GetPreferredWidth` delegate slots beside `BeginEdit`/`CancelEdit`/`CommitEdit`/`Paint` events.
- `Grid.CommitEdit()`/`CancelEdit()` return `bool` — a refused commit is a readable verdict, never a silent no-op; `SegmentedButton.SelectedIndexes` is get/set over `IEnumerable<int>`, assignable from a collection expression.
- Tree interaction resolves through the two model returns alone — `GetCellAt` answers what the pointer is over and `GetDragInfo` answers where a drop lands, so a canvas drag reads one descriptor rather than reconstructing the target from coordinates.

[STACKING]:
- `api-eto-forms`(`libs/dotnet/.api/api-eto-forms.md`): the registered construction spine every panel composes — a `Panel`/`Scrollable` root holds one layout owner and the layout holds the field, data-view, and container roster, with the rows here seated among them.
- `api-eto-binding`(`libs/dotnet/.api/api-eto-binding.md`): every field and view exposes its `*Binding`, the seam the branch-tier binding rail fuses to a `DataContext`.
- `api-eto-drawing`(`libs/dotnet/Rasm.Grasshopper/.api/api-eto-drawing.md`): the registered `Drawable` paint seam hands its context to the drawing surface for owner-drawn content.
- `api-eto-runtime`(`libs/dotnet/Rasm.Grasshopper/.api/api-eto-runtime.md`): dialog presentation, control invalidation, and cross-thread mutation marshal through the registered application singleton.
- `api-thinktecture-runtime-extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): a masked field's typed provider and a bounded field value are `[ValueObject<T>]` owners the control binding reads and writes; the tree hit-test kind is a `[SmartEnum]` case the drag policy dispatches on.
- `api-languageext`(`libs/dotnet/.api/api-languageext.md`): `Optional(view.GetCellAt(point))` null-gates a hit test into `Option<TreeGridCell>` and a drop-target read folds to `Fin<TreeGridViewDragInfo>` before a canvas commits the move.

[LOCAL_ADMISSION]:
- Panel subclasses a registered control or composes the registered roster directly; a new control capability lands as a subclass or a composition, never a wrapper renaming a host member or a re-implemented native widget.
- Format-masked or increment-carrying field takes the rows here; a hand-rolled mask parser or spinner pair beside them is the deleted form.
- Boundary faults lower onto the LanguageExt rail.
