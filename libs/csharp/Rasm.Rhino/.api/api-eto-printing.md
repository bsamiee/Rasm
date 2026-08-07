# [RASM_RHINO_API_ETO_PRINTING]

`Eto.Forms` printing owns document output, and the Rhino host boundary is its only reach in this branch — no sibling boundary prints, so this partition carries the whole surface rather than registering one. `PrintDocument` is the paginated render job the two dialogs present, the settings types configure, and `Taskbar` mirrors, held apart from the interactive control tree and ambient runtime.

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

| [INDEX] | [SYMBOL]               | [KIND]            | [CAPABILITY]                                                         |
| :-----: | :--------------------- | :---------------- | :------------------------------------------------------------------- |
|  [01]   | `PrintDocument`        | render job        | `Widget` paginated job with lifecycle callbacks, `Name`, `PageCount` |
|  [02]   | `PrintDialog`          | dialog            | `CommonDialog` printer, copy-count, and page-range chooser           |
|  [03]   | `PrintPreviewDialog`   | dialog            | `CommonDialog` on-screen preview render of a `PrintDocument`         |
|  [04]   | `PrintPageEventArgs`   | event args        | per-page render context — `Graphics`, `PageSize`, `CurrentPage`      |
|  [05]   | `PageSettings`         | settings          | printer-resolved `PrintableArea` alone; carries no margin or size    |
|  [06]   | `PrintSettings`        | settings          | job configuration — copies, ranges, selection, orientation, order    |
|  [07]   | `PageOrientation`      | enum              | `Portrait`/`Landscape` page geometry selector                        |
|  [08]   | `PrintSelection`       | enum              | `AllPages`/`Selection`/`SelectedPages` job-extent selector           |
|  [09]   | `Taskbar`              | static projection | OS taskbar/dock progress projection                                  |
|  [10]   | `TaskbarProgressState` | enum              | `None`/`Progress`/`Indeterminate`/`Error`/`Paused` progress modes    |

- `PageSettings` exposes one get-only `RectangleF PrintableArea` and nothing else; page size arrives per page on `PrintPageEventArgs.PageSize`, orientation on `PrintSettings.Orientation`, and margins exist nowhere on this surface — a margin is the boundary's own inset against the printable rectangle.
- Both page ranges are `Range<int>`, so an interval is one value, never a start/end pair a boundary must keep consistent.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle
- rail: eto-printing

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]              | [CAPABILITY]                             |
| :-----: | :---------------------------- | :------------------------ | :--------------------------------------- |
|  [01]   | `PrintDocument` ctor          | `()`                      | an empty paginated job                   |
|  [02]   | `PrintDocument` ctor          | `(Control)`               | a job rendering a control's visual       |
|  [03]   | `PrintDocument.Name`          | `get/set → string`        | the job name the OS print queue shows    |
|  [04]   | `PrintDocument.PageCount`     | `get/set → int`           | declared page count                      |
|  [05]   | `PrintDocument.PrintSettings` | `get/set → PrintSettings` | binds the job-configuration inputs       |
|  [06]   | `PrintDocument.Print`         | `()`                      | run the render silently                  |
|  [07]   | `PrintDocument.OnPrinting`    | `(EventArgs)`             | job-start override                       |
|  [08]   | `PrintDocument.OnPrintPage`   | `(PrintPageEventArgs)`    | render one page into the page `Graphics` |
|  [09]   | `PrintDocument.OnPrinted`     | `(EventArgs)`             | job-complete override                    |
|  [10]   | `PrintDocument.Printing`      | event                     | observes job start                       |
|  [11]   | `PrintDocument.PrintPage`     | event                     | observes each page render                |
|  [12]   | `PrintDocument.Printed`       | event                     | observes job completion                  |

[ENTRYPOINT_SCOPE]: presentation and progress
- rail: eto-printing

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                     | [CAPABILITY]                     |
| :-----: | :--------------------------------- | :----------------------------------------------- | :------------------------------- |
|  [01]   | `PrintDialog.ShowDialog`           | `(Control parent, PrintDocument) → DialogResult` | present the printer chooser      |
|  [02]   | `PrintDialog.PrintSettings`        | `get/set → PrintSettings`                        | the settings the chooser edits   |
|  [03]   | `PrintDialog.AllowSelection`       | `get/set → bool`                                 | offer the host-selection option  |
|  [04]   | `PrintDialog.AllowPageRange`       | `get/set → bool`                                 | offer the page-range option      |
|  [05]   | `PrintPreviewDialog` ctor          | `(PrintDocument)`                                | build a preview over a document  |
|  [06]   | `PrintPreviewDialog.Document`      | `get → PrintDocument`                            | the previewed document           |
|  [07]   | `PrintPreviewDialog.PrintSettings` | `get/set → PrintSettings`                        | the settings the preview renders |
|  [08]   | `PrintPreviewDialog.ShowDialog`    | `(Window parent) → DialogResult`                 | present the on-screen preview    |
|  [09]   | `Taskbar.SetProgress`              | `(TaskbarProgressState, float = 0f)`             | project job progress onto the OS |

- Each dialog takes a caller-supplied parent, so the parent is the Rhino-owned window the host bridge resolves (`libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md`), never a boundary-minted top-level window.
- `SetProgress` THROWS `ArgumentOutOfRangeException` for a fraction outside `0f..1f`; the fraction is validated at the boundary and never reaches the host unbounded.

[ENTRYPOINT_SCOPE]: page and job settings
- rail: eto-printing

| [INDEX] | [SURFACE]                                           | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `PageSettings.PrintableArea { get; } -> RectangleF` | reads printer-resolved page bounds            |
|  [02]   | `PrintSettings.MaximumPageRange { get; set; }`      | `Range<int>` bound on the selectable interval |
|  [03]   | `PrintSettings.SelectedPageRange { get; set; }`     | `Range<int>` chosen interval                  |
|  [04]   | `PrintSettings.PrintSelection { get; set; }`        | selects all, host selection, or page span     |
|  [05]   | `PrintSettings.Orientation { get; set; }`           | selects portrait or landscape geometry        |
|  [06]   | `PrintSettings.Copies { get; set; }`                | copy cardinality; host default `1`            |
|  [07]   | `PrintSettings.Collate { get; set; }`               | copy collation; host default `true`           |
|  [08]   | `PrintSettings.Reverse { get; set; }`               | selects reverse page order                    |

- `Copies` and `Collate` carry host `[DefaultValue]` attributes, so an unset job is already configured; a boundary default that restates them is a forged value, and a boundary that omits them inherits the host's.

## [04]-[IMPLEMENTATION_LAW]

[PIPELINE_LAW]:
- Construction is empty or over a `Control` whose rendered visual is the page source. Rendering runs `OnPrinting` → `OnPrintPage` per page → `OnPrinted`, and `OnPrintPage` receives a `PrintPageEventArgs` carrying the `Graphics` (`libs/csharp/.api/api-eto-drawing.md`) the page paints into, so a page draws with the identical primitive set a `Drawable` uses on screen.
- `Print` runs the job silently against the configured printer; `PrintDialog.ShowDialog` gates the same job behind the OS chooser; `PrintPreviewDialog` renders it to screen without committing to hardware.
- A control-sourced job brackets the native tree: `Print` attaches an unloaded source control to a native parent before the render and detaches it after, so the render is UI-thread-bound and the source control is exclusively held for the job's duration — a control already mounted elsewhere, or a job run off the host thread, breaks the pair the platform seam owns (`libs/csharp/Rasm.Rhino/.api/api-eto-platform.md`).
- Page count is declared, not discovered: `PageCount` is set before the render and the per-page callback receives `CurrentPage`, so pagination is the boundary's computation over the printable rectangle, never a host-driven enumeration.

[PROGRESS_LAW]:
- `Taskbar.SetProgress` stands independent of the document and the dialogs: it projects a completion fraction under a `TaskbarProgressState` onto the OS taskbar or dock, the ambient signal beside a long render, never inside the page pipeline.
- The OS projection and the Rhino in-app meter (`libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md` `StatusBar`) are one pair over one progress fact, not two owners: a long render publishes its fraction once and both sinks read it.

[STACKING]:
- `libs/csharp/.api/api-eto-drawing.md`: the page `Graphics` primitive set every `OnPrintPage` body composes, and the `RectangleF`/`SizeF` carriers the printable area and page size arrive as.
- `libs/csharp/.api/api-eto-forms.md`: `Control`, `Window`, and `DialogResult` are the carriers the source control and both dialogs receive; the control tree itself is authored there.
- `libs/csharp/Rasm.Rhino/.api/api-eto-platform.md`: the `AttachNative`/`DetachNative` pair a control-sourced `Print` brackets internally; a boundary never nests its own attach inside a print job.
- `libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md`: the Rhino host marshal owner and the document-owned parent window — a print job and both dialogs cross the host thread there first, and the parent a dialog receives is resolved through the bridge.
- `libs/csharp/Rasm.Rhino/.api/api-eto-runtime.md`: a background producer requesting a print marshals through the registered application dispatch before touching the job.
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): a print job composes as an effect — `PrintDocument.Print` and the `On*` callbacks fold into an `Eff<A>`/`IO<A>` pipeline whose per-page render is a step, and each `ShowDialog` result folds to `Fin<A>` so a cancelled `DialogResult` rides a typed rail. `PrintDocument` is resource-scoped through the `use` rail, its construction and disposal bracketing one scope; `Option<A>` lifts a null `PrintSettings` so an unconfigured job is `None`; the progress-fraction bound is a `Fin` guard, never a caught `ArgumentOutOfRangeException`.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `PageOrientation`, `PrintSelection`, `TaskbarProgressState`, and `DialogResult` bind as `[SmartEnum]` owners routed by generated `Switch`/`Map`; a `Range<int>` page interval binds as a `[ComplexValueObject]` so the bound and the selection validate against each other as one owner; the progress fraction binds as a `[ValueObject<float>]` carrying the host's `0f..1f` bound.

[LOCAL_ADMISSION]:
- `Eto.Forms` printing is host-provided and never re-declared; a Rasm owner internalizes document output behind one canonical rail so downstream code composes a print effect and a page-render callback, never a raw `PrintDocument` lifecycle, a stringy dialog-result branch, or a hand-threaded taskbar update.
- No sibling host boundary in this branch prints, so this partition is the whole surface rather than a registration over a branch owner; a second boundary reaching printing hoists this file to the branch tier rather than re-tabling it.

[RAIL_LAW]:
- Package: `Eto.Forms`
- Owns: `PrintDocument` lifecycle, name, and page callbacks, `PrintDialog`/`PrintPreviewDialog` presentation, `PageSettings`/`PrintSettings` with the orientation and selection vocabularies, `Taskbar` progress projection
- Accept: paginated document rendering, printer and preview presentation, page geometry and job configuration, ambient taskbar/dock progress
- Reject: interactive windows and file dialogs (`libs/csharp/.api/api-eto-forms.md`), the page `Graphics` primitive set (`libs/csharp/.api/api-eto-drawing.md`), ambient application dispatch and clock (`libs/csharp/Rasm.Rhino/.api/api-eto-runtime.md`), a boundary-minted parent window or a print job off the host thread (`libs/csharp/Rasm.Rhino/.api/api-rhino-ui.md`), a boundary-side margin or copy default restating a host `[DefaultValue]`, and leaking a raw `PrintDocument` or `DialogResult` branch past the owning rail
