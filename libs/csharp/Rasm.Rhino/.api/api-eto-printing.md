# [RASM_RHINO_API_ETO_PRINTING]

`Eto.Forms` printing owns document output: `PrintDocument` is the paginated render job the two dialogs present, the settings types configure, and `Taskbar` mirrors, held apart from the interactive control tree and ambient runtime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms`
- package: `Eto.Forms` — host-provided, resolved from the Rhino host assembly set, not a central `PackageReference`
- assembly: `Eto`
- namespace: `Eto.Forms`
- asset: the `Eto` assembly the Rhino host loads; the `macOS`, `WinForms`, and `Wpf` platform handlers back the same print surface
- rail: eto-printing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, presentation, and settings
- namespace: `Eto.Forms`
- rail: eto-printing

| [INDEX] | [SYMBOL]               | [KIND]            | [CAPABILITY]                                                      |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `PrintDocument`        | render job        | paginated document with lifecycle callbacks and `PageCount`       |
|  [02]   | `PrintDialog`          | dialog            | OS printer, copy-count, and page-range chooser over a document    |
|  [03]   | `PrintPreviewDialog`   | dialog            | on-screen preview render of a `PrintDocument`                     |
|  [04]   | `PrintPageEventArgs`   | event args        | per-page render context — `Graphics`, `PageSize`, `CurrentPage`   |
|  [05]   | `PageSettings`         | settings          | page geometry — size, margins, orientation                        |
|  [06]   | `PrintSettings`        | settings          | job configuration — copies, range, collation, printer selection   |
|  [07]   | `Taskbar`              | static projection | OS taskbar/dock progress projection                               |
|  [08]   | `TaskbarProgressState` | enum              | `None`/`Progress`/`Indeterminate`/`Error`/`Paused` progress modes |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle
- rail: eto-printing

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]              | [CAPABILITY]                             |
| :-----: | :---------------------------- | :------------------------ | :--------------------------------------- |
|  [01]   | `PrintDocument` ctor          | `()`                      | an empty paginated job                   |
|  [02]   | `PrintDocument` ctor          | `(Control)`               | a job rendering a control's visual       |
|  [03]   | `PrintDocument.PageCount`     | `get/set → int`           | declared page count                      |
|  [04]   | `PrintDocument.PrintSettings` | `get/set → PrintSettings` | binds the job-configuration inputs       |
|  [05]   | `PrintDocument.Print`         | `()`                      | run the render silently                  |
|  [06]   | `PrintDocument.OnPrinting`    | `(EventArgs)`             | job-start override                       |
|  [07]   | `PrintDocument.OnPrintPage`   | `(PrintPageEventArgs)`    | render one page into the page `Graphics` |
|  [08]   | `PrintDocument.OnPrinted`     | `(EventArgs)`             | job-complete override                    |
|  [09]   | `PrintDocument.Printing`      | event                     | observes job start                       |
|  [10]   | `PrintDocument.PrintPage`     | event                     | observes each page render                |
|  [11]   | `PrintDocument.Printed`       | event                     | observes job completion                  |

[ENTRYPOINT_SCOPE]: presentation and progress
- rail: eto-printing

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                     | [CAPABILITY]                     |
| :-----: | :------------------------------ | :----------------------------------------------- | :------------------------------- |
|  [01]   | `PrintDialog.ShowDialog`        | `(Control parent, PrintDocument) → DialogResult` | present the printer chooser      |
|  [02]   | `PrintDialog.AllowSelection`    | `get/set → bool`                                 | offer the host-selection option  |
|  [03]   | `PrintDialog.AllowPageRange`    | `get/set → bool`                                 | offer the page-range option      |
|  [04]   | `PrintPreviewDialog` ctor       | `(PrintDocument)`                                | build a preview over a document  |
|  [05]   | `PrintPreviewDialog.Document`   | `get → PrintDocument`                            | the previewed document           |
|  [06]   | `PrintPreviewDialog.ShowDialog` | `(Window parent) → DialogResult`                 | present the on-screen preview    |
|  [07]   | `Taskbar.SetProgress`           | `(TaskbarProgressState state, float = 0f)`       | project job progress onto the OS |

[ENTRYPOINT_SCOPE]: page and job settings
- rail: eto-printing

| [INDEX] | [SURFACE]                                       | [CAPABILITY]                              |
| :-----: | :---------------------------------------------- | :---------------------------------------- |
|  [01]   | `PageSettings.PrintableArea { get; }`           | reads printer-resolved page bounds        |
|  [02]   | `PrintSettings.MaximumPageRange { get; set; }`  | bounds the selectable page interval       |
|  [03]   | `PrintSettings.SelectedPageRange { get; set; }` | carries the chosen page interval          |
|  [04]   | `PrintSettings.PrintSelection { get; set; }`    | selects all, host selection, or page span |
|  [05]   | `PrintSettings.Orientation { get; set; }`       | selects portrait or landscape geometry    |
|  [06]   | `PrintSettings.Copies { get; set; }`            | sets copy cardinality                     |
|  [07]   | `PrintSettings.Collate { get; set; }`           | selects copy collation                    |
|  [08]   | `PrintSettings.Reverse { get; set; }`           | selects reverse page order                |

## [04]-[IMPLEMENTATION_LAW]

[PIPELINE_LAW]:
- Construction is empty or over a `Control` whose rendered visual is the page source. Rendering runs `OnPrinting` → `OnPrintPage` per page → `OnPrinted`, and `OnPrintPage` receives a `PrintPageEventArgs` carrying the `Graphics` (`api-eto-drawing`) the page paints into, so a page draws with the identical primitive set a `Drawable` uses on screen.
- `Print` runs the job silently against the configured printer; `PrintDialog.ShowDialog` gates the same job behind the OS chooser; `PrintPreviewDialog` renders it to screen without committing to hardware.

[PROGRESS_LAW]:
- `Taskbar.SetProgress` stands independent of the document and the dialogs: it projects a completion fraction under a `TaskbarProgressState` onto the OS taskbar or dock, the ambient signal beside a long render, never inside the page pipeline.

[STACKING]:
- `LanguageExt`(`.api/api-languageext`): a print job composes as an effect — `PrintDocument.Print` and the `On*` callbacks fold into an `Eff<A>`/`IO<A>` pipeline whose per-page render is a step, and each `ShowDialog` result folds to `Fin<A>` so a cancelled `DialogResult` rides a typed rail. `PrintDocument` is resource-scoped through the `use` rail, its construction and disposal bracketing one scope; `Option<A>` lifts a null `PrintSettings` so an unconfigured job is `None`.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): `TaskbarProgressState` binds as a `[SmartEnum]` routed by generated `Switch`/`Map`, so a progress transition dispatches over the closed vocabulary; `DialogResult` binds the same way where a dialog outcome drives downstream dispatch.

[LOCAL_ADMISSION]:
- `Eto.Forms` printing is host-provided and never re-declared; a Rasm owner internalizes document output behind one canonical rail so downstream code composes a print effect and a page-render callback, never a raw `PrintDocument` lifecycle, a stringy dialog-result branch, or a hand-threaded taskbar update.

[RAIL_LAW]:
- Package: `Eto.Forms`
- Owns: `PrintDocument` lifecycle and page callbacks, `PrintDialog`/`PrintPreviewDialog` presentation, `PageSettings`/`PrintSettings`, `Taskbar` progress projection
- Accept: paginated document rendering, printer and preview presentation, page geometry and job configuration, ambient taskbar/dock progress
- Reject: interactive windows and file dialogs (`api-eto-forms`), the page `Graphics` primitive set (`api-eto-drawing`), ambient application dispatch and clock (`api-eto-runtime`), and leaking a raw `PrintDocument` or `DialogResult` branch past the owning rail
