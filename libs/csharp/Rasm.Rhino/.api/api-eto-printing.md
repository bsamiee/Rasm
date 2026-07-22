# [RASM_RHINO_API_ETO_PRINTING]

`Eto.Forms` document output is the print pipeline the Rhino host binds, and this catalog owns it as a surface distinct from interactive dialogs and runtime dispatch. `PrintDocument` is the paginated job: it declares a `PageCount`, raises `OnPrinting`/`OnPrintPage`/`OnPrinted` across the render lifecycle, and drives the render either silently through `Print` or interactively. `PrintDialog` presents the OS printer-and-range chooser over a document; `PrintPreviewDialog` renders the same document to an on-screen preview. `PageSettings` and `PrintSettings` carry the page geometry and job configuration a document renders under. `Taskbar` projects job progress onto the OS taskbar or dock through `SetProgress` and the `TaskbarProgressState` vocabulary — the ambient completion signal that rides beside a long render. Interactive windows and file dialogs are `api-eto-forms`; the `Graphics` surface a page callback paints into is `api-eto-drawing`; ambient application dispatch is `api-eto-runtime`.

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

`PrintDocument` is the render job and the anchor every other type composes with; the two dialogs present it, and the two settings types configure it. `Taskbar` is the independent ambient-progress projection.

| [INDEX] | [SYMBOL]               | [KIND]            | [CAPABILITY]                                                      |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `PrintDocument`        | render job        | paginated document with lifecycle callbacks and `PageCount`       |
|  [02]   | `PrintDialog`          | dialog            | OS printer, copy-count, and page-range chooser over a document    |
|  [03]   | `PrintPreviewDialog`   | dialog            | on-screen preview render of a `PrintDocument`                     |
|  [04]   | `PrintPageEventArgs`   | event args        | the per-page render context passed to `OnPrintPage`               |
|  [05]   | `PageSettings`         | settings          | page geometry — size, margins, orientation                        |
|  [06]   | `PrintSettings`        | settings          | job configuration — copies, range, collation, printer selection   |
|  [07]   | `Taskbar`              | static projection | OS taskbar/dock progress projection                               |
|  [08]   | `TaskbarProgressState` | enum              | `None`/`Progress`/`Indeterminate`/`Paused`/`Error` progress modes |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle
- rail: eto-printing

`PrintDocument` constructs empty or over a `Control` whose visual is the page source; `Print` runs the render silently; the three `On*` callbacks bracket the job and paginate it, with `OnPrintPage` receiving the `Graphics` to paint each page.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]           | [CAPABILITY]                             |
| :-----: | :-------------------------- | :--------------------- | :--------------------------------------- |
|  [01]   | `PrintDocument` ctor        | `()`                   | an empty paginated job                   |
|  [02]   | `PrintDocument` ctor        | `(Control)`            | a job rendering a control's visual       |
|  [03]   | `PrintDocument.PageCount`   | `get/set → int`        | declared page count                      |
|  [04]   | `PrintDocument.Print`       | `()`                   | run the render silently                  |
|  [05]   | `PrintDocument.OnPrinting`  | `(EventArgs)`          | job-start lifecycle callback             |
|  [06]   | `PrintDocument.OnPrintPage` | `(PrintPageEventArgs)` | render one page into the page `Graphics` |
|  [07]   | `PrintDocument.OnPrinted`   | `(EventArgs)`          | job-complete lifecycle callback          |
|  [08]   | `PrintDocument.Printing`    | event                  | observes job-start lifecycle             |
|  [09]   | `PrintDocument.PrintPage`   | event                  | observes each page render callback       |
|  [10]   | `PrintDocument.Printed`     | event                  | observes job completion                  |

[ENTRYPOINT_SCOPE]: presentation and progress
- rail: eto-printing

`PrintDialog` and `PrintPreviewDialog` present the same `PrintDocument`; `Taskbar.SetProgress` projects the render's completion fraction under a `TaskbarProgressState`.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                     | [CAPABILITY]                     |
| :-----: | :------------------------------ | :----------------------------------------------- | :------------------------------- |
|  [01]   | `PrintDialog.ShowDialog`        | `(Control parent, PrintDocument) → DialogResult` | present the printer chooser      |
|  [02]   | `PrintPreviewDialog` ctor       | `(PrintDocument)`                                | build a preview over a document  |
|  [03]   | `PrintPreviewDialog.Document`   | `get → PrintDocument`                            | the previewed document           |
|  [04]   | `PrintPreviewDialog.ShowDialog` | `(Window parent) → DialogResult`                 | present the on-screen preview    |
|  [05]   | `Taskbar.SetProgress`           | `(TaskbarProgressState state, float = 0f)`       | project job progress onto the OS |

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
- `PrintDocument` is the one render job every entrypoint composes with: a dialog presents it, a preview wraps it, the settings configure it, and the taskbar mirrors its progress. Construction is empty or over a `Control` whose rendered visual is the page source. The render lifecycle is `OnPrinting` → `OnPrintPage` per page → `OnPrinted`; `OnPrintPage` receives a `PrintPageEventArgs` carrying the `Graphics` (`api-eto-drawing`) the page paints into, so a page is drawn with the identical primitive set a `Drawable` uses on screen.
- `Print` runs the job silently against the configured printer; `PrintDialog.ShowDialog` gates the same job behind the OS printer, copy-count, and range chooser; `PrintPreviewDialog` renders it to screen without committing to hardware. `PageSettings` and `PrintSettings` are the geometry and job-configuration inputs a document renders under.

[PROGRESS_LAW]:
- `Taskbar.SetProgress` is independent of the document and the dialogs: it projects a completion fraction under a `TaskbarProgressState` onto the OS taskbar or dock, the ambient signal for a long render that runs beside — never inside — the page pipeline.

[STACKING]:
- `LanguageExt`(`.api/api-languageext`): a print job composes as an effect — `PrintDocument.Print` and the `On*` callbacks fold into an `Eff<A>`/`IO<A>` pipeline whose per-page render is a step, and the `PrintDialog.ShowDialog`/`PrintPreviewDialog.ShowDialog` result folds to `Fin<A>` (a cancelled `DialogResult` is a typed rail, never a control-flow branch). The `PrintDocument` is resource-scoped through the `use` rail so a job's construction and disposal bracket exactly one scope. `Option<A>` lifts an unselected printer or a null `PrintSettings` so an unconfigured job is `None`, not a null probe.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): `TaskbarProgressState` binds as a `[SmartEnum]` routed by generated `Switch`/`Map`, so a progress transition dispatches over the closed vocabulary rather than a raw enum compare; `DialogResult` binds the same way where the print dialogs' outcome drives downstream dispatch.

[LOCAL_ADMISSION]:
- `Eto.Forms` printing is host-provided and never re-declared; a Rasm owner internalizes document output behind one canonical rail so downstream code composes a print effect and a page-render callback, never a raw `PrintDocument` lifecycle, a stringy dialog-result branch, or a hand-threaded taskbar update.

[RAIL_LAW]:
- Package: `Eto.Forms`
- Owns: `PrintDocument` lifecycle and page callbacks, `PrintDialog`/`PrintPreviewDialog` presentation, `PageSettings`/`PrintSettings`, `Taskbar` progress projection
- Accept: paginated document rendering, printer and preview presentation, page geometry and job configuration, ambient taskbar/dock progress
- Reject: interactive windows and file dialogs (`api-eto-forms`), the page `Graphics` primitive set (`api-eto-drawing`), ambient application dispatch and clock (`api-eto-runtime`), and leaking a raw `PrintDocument` or `DialogResult` branch past the owning rail
