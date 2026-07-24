# [TS_RUNTIME_API_JSPDF]

`jsPDF` mints one mutable PDF document a fluent draw/paginate/graphics-state API builds, emitting bytes through one polymorphic `output` a format token discriminates. `runtime/src/work/report.ts` internalizes it once: a document folds inside one `Effect.sync`, `output("arraybuffer")` crosses to `Uint8Array` a single time, and those bytes route to `FileSystem`, a jszip entry, or a nodemailer attachment under a durable report activity.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jspdf`
- package: `jspdf` (MIT)
- module: dual — ESM (`dist/jspdf.es.min.js`) and a Node build (`dist/jspdf.node.min.js`); `exports` maps `.` and `./dist/*`; named exports (`jsPDF`, `GState`, `ShadingPattern`, AcroForm classes) beside `export default jsPDF`
- runtime: isomorphic with a real Node build — the drawing API and `output("arraybuffer")` render headless; pure-JS, no native addon, CPU-bound, so a large document offloads off the request path. `html()` (html2canvas), `save()`, and `output("blob"\|"bloburi"\|"pdfobjectnewwindow")` need a DOM and never enter a Node job
- rail: document egress; internalized at `runtime/src/work/report.ts`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the document root, its construction/security/metadata/text/image/table/viewer policy records, and the interactive, vector, and canvas-emulation types; each option struct's field roster rides the token lines below the grid.

| [INDEX] | [SYMBOL]                                                                | [TYPE_FAMILY]    | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `jsPDF`                                                                 | document         | mutable builder; `output` egress            |
|  [02]   | `jsPDFOptions`                                                          | construct policy | constructor policy values                   |
|  [03]   | `EncryptionOptions`                                                     | security policy  | at-rest protection; passwords `Redacted`    |
|  [04]   | `DocumentProperties`                                                    | metadata         | via `setDocumentProperties`                 |
|  [05]   | `TextOptionsLight` / `TextOptions`                                      | text policy      | per-`text` layout options                   |
|  [06]   | `ImageOptions` / `ImageProperties` / `ImageCompression` / `ImageFormat` | image            | `addImage`; format/compression policy       |
|  [07]   | `Font` / `PageInfo`                                                     | receipt          | font + page-object evidence                 |
|  [08]   | `TableConfig` / `CellConfig` / `TableRowData`                           | table policy     | `table`/`cell` structured-data layout       |
|  [09]   | `AcroFormField`                                                         | form field       | PDF-form fields via `addField`              |
|  [10]   | `Annotation` / `TextWithLinkOptions`                                    | annotation       | `createAnnotation` + `textWithLink` targets |
|  [11]   | `Outline` / `OutlineItem` / `OutlineOptions`                            | bookmark         | sectioned bookmark tree                     |
|  [12]   | `ViewerPreferencesInput`                                                | viewer policy    | reader-behavior hints                       |
|  [13]   | `ShadingPattern` / `TilingPattern` / `GState` / `Matrix`                | vector           | gradients, tiling, opacity, transforms      |
|  [14]   | `Context2d` / `Gradient` / `RGBAData`                                   | canvas emulation | HTML-canvas context for canvas porting      |

- `jsPDFOptions`: `orientation` `unit` `format` `compress` `precision` `floatPrecision` `filters` `encryption` `putOnlyUsedFonts` `userUnit`; `unit` one of `pt`/`px`/`mm`/`cm`/`in`/`em`/`ex`/`pc`, `format` a named string or `number[]`.
- `EncryptionOptions`: `userPassword` `ownerPassword` `userPermissions`; permissions from `print`/`modify`/`copy`/`annot-forms`.
- `DocumentProperties`: `title` `subject` `author` `keywords` `creator`.
- `TextOptions`: `align` `angle` `baseline` `charSpace` `lineHeightFactor` `maxWidth` `renderingMode`, RTL flags.
- `ImageOptions`: format `PNG`/`JPEG`/`WEBP`, compression `NONE`/`FAST`/`MEDIUM`/`SLOW`; `getImageProperties` reads dimensions and colorspace.
- `Font` / `PageInfo`: `getFont`/`getFontList` evidence; `getCurrentPageInfo`/`getPageInfo` page-object refs for cross-page links.
- `AcroFormField` factories `doc.AcroForm.TextField()`/`CheckBox`/`ComboBox`/`RadioButton`/`ListBox`/`PushButton`/`PasswordField`/`EditBox`; each carries `x`/`y`/`width`/`height`/`fieldName`/`value`/`readOnly`/`required`.
- `TableConfig`: `printHeaders` `margins` `headerBackgroundColor`, `rowStart`/`cellStart` hooks.
- `Annotation` kinds `text`/`freetext`/`link` with `bounds`/`contents`.
- `ViewerPreferencesInput`: `HideToolbar` `FitWindow` `DisplayDocTitle` `Duplex` `PrintScaling` `NumCopies`.
- `Matrix` ops `multiply`/`inversed`/`decompose`/`applyToPoint`; `Context2d` ops `fillRect`/`arc`/`bezierCurveTo`/`drawImage`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, draw, measure, paginate, set graphics state, and emit — every surface a `doc.*` method on the built document, its member families expanded in the token lines below.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]  | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `new jsPDF(options?)`                                  | construct       | one `Effect.sync`; `unit`/`format`/`encryption` policy   |
|  [02]   | `doc.output("arraybuffer")` -> `ArrayBuffer`           | node egress     | durable-job bytes -> `Uint8Array` -> `FileSystem`/jszip  |
|  [03]   | `doc.output(t, opts?)`                                 | browser egress  | DOM-only `Blob`/`URL`/`Window`; `browser/transport` only |
|  [04]   | `doc.text(text, x, y, options?)`                       | text + measure  | placement; `splitTextToSize` wraps `maxWidth`            |
|  [05]   | `doc.rect` / `circle` / `line` / `path`                | vector draw     | primitive + path drawing surface, chainable              |
|  [06]   | `doc.addPage(format?, orientation?)`                   | paginate        | report loop appends pages, stamps headers/footers        |
|  [07]   | `doc.setFont` / `setFontSize` / `addFont`              | graphics state  | fluent state setters; `addFont` embeds a custom TTF      |
|  [08]   | `doc.table(x, y, data, headers, config)` / `cell(...)` | tabular         | structured-table primitive; cell hooks, auto-sizing      |
|  [09]   | `doc.addImage(data, format, x, y, w, h, ...)`          | image           | Node-safe bytes; `alias` dedups a logo                   |
|  [10]   | `doc.addSvgAsImage(svg, x, y, w, h, ...)`              | svg             | rasterize an SVG string, no `html()` DOM worker          |
|  [11]   | `doc.setDocumentProperties` / `setCreationDate`        | metadata        | catalog metadata, pinned date; `setLanguage`             |
|  [12]   | `doc.addField(field)` / `doc.AcroForm.TextField()`     | interactive     | form fields, annotations, links, print, embedded JS      |
|  [13]   | `doc.outline.add(parent, title, { pageNumber })`       | bookmark        | build the navigation tree for a sectioned report         |
|  [14]   | `jsPDF.API` / `jsPDF.version` / `doc.internal.events`  | plugin registry | plugin on the prototype; `PubSub` cross-cut hooks        |
|  [15]   | `doc.setCreationDate(fixed)` + `jsPDFOptions.compress` | reproducibility | pin creation date + compression -> byte-identical bytes  |

- `output` browser tokens `"blob"`/`"bloburi"`/`"datauristring"`/`"pdfobjectnewwindow"`.
- measure `getTextWidth`/`getTextDimensions`/`splitTextToSize`.
- vector `rect`/`roundedRect`/`circle`/`ellipse`/`line`/`lines`/`triangle`/`path`; path ops `moveTo`/`lineTo`/`curveTo`/`fill`/`stroke`.
- pagination `setPage`/`insertPage`/`deletePage`/`movePage`/`getNumberOfPages`.
- graphics `setTextColor`/`setDrawColor`/`setFillColor`/`setLineWidth`/`setLineDashPattern`/`setLineCap`/`setLineJoin`/`setGState`/`saveGraphicsState`/`restoreGraphicsState`.
- tabular `setTableHeaderRow` pins the header row.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `output` is one polymorphic egress: the format token selects the return (`"arraybuffer"` -> `ArrayBuffer`, `"blob"` -> `Blob`, `"datauristring"` -> `string`, `"pdfobjectnewwindow"` -> `Window`). A Node job pins the `arraybuffer` arm; the `Blob`/`URL`/`Window`/`save()` arms stay browser-only.
- Every draw/state method returns `jsPDF`, so the whole document folds inside one `Effect.sync`; only its `Uint8Array` crosses the boundary, once.
- Paging is a measured fold: `getTextDimensions`/`splitTextToSize`/`getTextWidth` drive layout over decoded rows, and the loop appends `addPage` when running `y` exceeds `internal.pageSize.getHeight()`.
- Bytes are reproducible: `setCreationDate(fixed)` and `compress` make equal rows emit equal bytes, so the report `XxHash128` content key stays stable for cache dedupe.
- Encryption passwords ride `Redacted`, unwrapped only at construction; `userPermissions` is a bounded policy set on the document.
- `jsPDF.API` is the extension seam: repeated document furniture registers once on the prototype, never re-drawn per call site.

[STACKING]:
- `effect` (`../../.api/effect.md`): `Effect.sync` wraps the build-and-`output("arraybuffer")`; `Schema.decodeUnknown` types rows before layout; `Array.reduce`/`Effect.forEach` fold rows into pages; `Config.redacted` carries encryption passwords; `Effect.withSpan` tags page count and byte size.
- `@effect/platform` (`../../.api/effect-platform.md`): the `Uint8Array` lands via `FileSystem.writeFile` or `FileSystem.sink`; the wire arm is `HttpBody.uint8Array(bytes, "application/pdf")`; `makeTempDirectoryScoped` scopes a multi-file render.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): the Node build composes under `NodeContext.layer`; `NodeStream.toReadable` turns bytes into the `Readable` a nodemailer attachment expects; a heavy render offloads to a `WorkerPool` worker.
- `jszip` (`./jszip.md`) / `nodemailer` (`./nodemailer.md`): the PDF `Uint8Array` becomes a jszip entry or a nodemailer `{ content, contentType: "application/pdf" }` attachment — jspdf mints bytes, the siblings transport them.
- `exceljs` (`./exceljs.md`) / `papaparse` (`./papaparse.md`): output-format peers — one decoded row set renders to PDF, XLSX, or CSV under one output-format policy row, never a forked pipeline.
- `@effect/workflow` (`./effect-workflow.md`): a render is a durable `Activity` under a `Schedule` budget; reproducible bytes make it idempotent, so a replay regenerates the identical document with no duplicate artifact.

[LOCAL_ADMISSION]:
- `output("arraybuffer")` -> `Uint8Array` is the only egress in a Node job; `save()`/`html()`/`output("blob"\|"bloburi")` stay in `browser`.
- Build inside one `Effect.sync` and cross once at `output`; the mutable `jsPDF` never threads the `Effect` rail.
- `addImage` takes `Uint8Array`/`RGBAData`/data-URI bytes; a DOM `HTMLImageElement`/`HTMLCanvasElement` source never enters a Node job.
- `EncryptionOptions` passwords ride `Config.redacted`, `setCreationDate`/`compress` pin for byte stability, and shared furniture registers once on `jsPDF.API`.

[RAIL_LAW]:
- Package: `jspdf`
- Owns: programmatic headless PDF rendering — the fluent draw/paginate/graphics-state API, the polymorphic `output` egress, measured layout, structured `table`/`cell`, `addImage` from bytes, encryption + metadata, AcroForm/outline/annotation structure, vector patterns/`GState`/`Matrix`, and the `jsPDF.API` registry
- Accept: an `Effect.sync` build with one `output("arraybuffer")` -> `Uint8Array` crossing, `Schema`-typed rows, measured paging, byte-reproducible construction, `Redacted` passwords, bytes routed to `FileSystem`/`HttpBody`/jszip/nodemailer
- Reject: `save()`/`html()`/`Blob`/`bloburi` egress in a Node job, the mutable document crossing the `Effect` boundary, hardcoded paging arithmetic, DOM image sources headless, inline passwords, per-call-site re-drawing of shared furniture
