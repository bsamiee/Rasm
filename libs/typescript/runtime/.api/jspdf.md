# [TS_RUNTIME_API_JSPDF]

`jspdf` is the programmatic PDF engine `runtime/src/work/report.ts` drives to render a report document from typed rows and emit its bytes. Its surface is one mutable `jsPDF` document built by a fluent, chainable drawing API — `text`/`rect`/`line`/`circle`/`addImage`/`table`/`cell` place content, `addPage`/`setPage`/`insertPage`/`deletePage`/`movePage` manage pagination, `setFont`/`setFontSize`/`setTextColor`/`setDrawColor` set graphics state, and measurement (`getTextWidth`/`getTextDimensions`/`splitTextToSize`) drives layout — and one polymorphic `output` that discriminates on a format token: `output("arraybuffer")` returns the `ArrayBuffer` that is the Node durable-job egress, while `output("blob")`/`output("bloburi")`/`save()`/`html()` are browser paths that require a DOM. The document carries encryption (`userPassword`/`ownerPassword`/`userPermissions`), metadata (`setDocumentProperties`, `setCreationDate`, `setLanguage`), interactive structure (AcroForm fields, `outline`, `link`, `textWithLink`, `autoPrint`, `viewerPreferences`), and full vector primitives (paths, `ShadingPattern`/`TilingPattern` gradients, `GState` opacity, `Matrix` transforms, a `Context2d` canvas emulation). The engine is synchronous and imperative; the owner internalizes it once: a `jsPDF` is built inside one `Effect.sync` fold, `output("arraybuffer")` crosses to `Uint8Array` once, and those bytes flow to `FileSystem.writeFile`, a nodemailer attachment, or a jszip entry under a durable report activity.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jspdf`
- package: `jspdf` (MIT, © James Hall / yWorks); declarations bundled (`types/index.d.ts`, `declare module "jspdf"`)
- module format: dual — ESM (`module: dist/jspdf.es.min.js`) and a dedicated Node build (`main: dist/jspdf.node.min.js`); `exports` map exposes `.` and `./dist/*`; named exports (`jsPDF`, `GState`, `ShadingPattern`, AcroForm classes) plus `export default jsPDF`
- runtime target: isomorphic with a real Node build — the programmatic drawing API and `output("arraybuffer")` run headless. The `html()` worker (html2canvas), `save()` download, and `output("blob"\|"bloburi"\|"pdfobjectnewwindow")` require a DOM and never enter a Node durable job
- dependencies: `@babel/runtime`, `fflate` (PDF stream compression), `fast-png`; no native addon — rendering is pure-JS and CPU-bound, so a large document belongs off the request path
- rail: document egress (folder-tier; internalized once at `runtime/src/work/report.ts`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the document, its construction policy, and egress evidence
- rail: boundaries
- The document root plus its construction, security, metadata, text, image, and receipt policy records; each option struct's field roster rides the keyed list below the grid.

| [INDEX] | [SYMBOL]                                                                | [TYPE_FAMILY]    | [CONSUMER]                                 |
| :-----: | :---------------------------------------------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `jsPDF`                                                                 | document         | mutable builder; `output` egress           |
|  [02]   | `jsPDFOptions`                                                          | construct policy | constructor policy values                  |
|  [03]   | `EncryptionOptions`                                                     | security policy  | at-rest protection; passwords `Redacted`   |
|  [04]   | `DocumentProperties`                                                    | metadata         | metadata via `setDocumentProperties`       |
|  [05]   | `TextOptionsLight` / `TextOptions`                                      | text policy      | per-`text` layout options                  |
|  [06]   | `ImageOptions` / `ImageProperties` / `ImageCompression` / `ImageFormat` | image            | `addImage` form; format/compression policy |
|  [07]   | `Font` / `PageInfo`                                                     | receipt          | font + page-object evidence                |

- `jsPDFOptions` fields: `orientation`, `unit` (`pt`/`mm`/`in`/…), `format` (named or `number[]`), `compress`, `precision`/`floatPrecision`, `filters`, `encryption`, `putOnlyUsedFonts`, `userUnit`.
- `EncryptionOptions` fields: `userPassword`, `ownerPassword`, `userPermissions` (`print`/`modify`/`copy`/`annot-forms`).
- `DocumentProperties` fields: `title`/`subject`/`author`/`keywords`/`creator`.
- `TextOptions` fields: `align`, `angle`, `baseline`, `charSpace`, `lineHeightFactor`, `maxWidth`, `renderingMode`, RTL flags.
- `ImageOptions`: `PNG`/`JPEG`/`WEBP` format + `NONE`/`FAST`/`MEDIUM`/`SLOW` compression; `getImageProperties` reads dimensions/colorspace.
- `Font` / `PageInfo`: `getFont`/`getFontList` evidence; `getCurrentPageInfo`/`getPageInfo` page-object refs for cross-page links.

[PUBLIC_TYPE_SCOPE]: interactive structure and vector graphics
- rail: system-apis
- The table, form-field, annotation, bookmark, viewer, vector, and canvas-emulation records; each type's field/subtype/op roster rides the keyed list below the grid.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]    | [CONSUMER]                                             |
| :-----: | :------------------------------------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `TableConfig` / `CellConfig` / `TableRowData`            | table policy     | `table`/`cell` structured-data layout                  |
|  [02]   | `AcroFormField`                                          | form field       | interactive PDF-form fields via `addField`             |
|  [03]   | `Annotation` / `TextWithLinkOptions`                     | annotation       | `createAnnotation` + `textWithLink` navigation targets |
|  [04]   | `Outline` / `OutlineItem` / `OutlineOptions`             | bookmark         | `outline.add(parent, title, { pageNumber })`           |
|  [05]   | `ViewerPreferencesInput`                                 | viewer policy    | reader-behavior hints                                  |
|  [06]   | `ShadingPattern` / `TilingPattern` / `GState` / `Matrix` | vector           | gradients, tiling fills, opacity, affine transforms    |
|  [07]   | `Context2d` / `Gradient` / `RGBAData`                    | canvas emulation | HTML-canvas drawing context for porting canvas code    |

- `AcroFormField` types: `TextField`/`CheckBox`/`ComboBox`/`RadioButton`/`ListBox`/`PushButton`/`PasswordField`; each carries `x`/`y`/`width`/`height`, `fieldName`, `value`, `readOnly`/`required`.
- `TableConfig` fields: `printHeaders`, `margins`, `headerBackgroundColor`, `rowStart`/`cellStart` hooks.
- `Annotation` kinds: `text`/`freetext`/`link` with `bounds`/`contents`.
- `ViewerPreferencesInput` fields: `HideToolbar`/`FitWindow`/`DisplayDocTitle`/`Duplex`/`PrintScaling`/`NumCopies`.
- `Matrix` ops: `multiply`/`inversed`/`decompose`/`applyToPoint`; `Context2d` ops: `fillRect`/`arc`/`bezierCurveTo`/`drawImage`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, draw, paginate, and emit
- rail: boundaries
- Every surface is a `doc.*` method on the built document; the browser tokens and the drawing, pagination, and graphics-state families carry their full member rosters in the keyed list below the grid.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `new jsPDF(options?)`                                  | construct      | one `Effect.sync`; `unit`/`format`/`encryption` policy        |
|  [02]   | `doc.output("arraybuffer")` → `ArrayBuffer`            | node egress    | durable-job bytes → `Uint8Array` → `FileSystem`/jszip         |
|  [03]   | `doc.output(t, opts?)`                                 | browser egress | DOM-only `Blob`/`URL`/`Window`; `browser/transport` only      |
|  [04]   | `doc.text(text, x, y, options?)`                       | text + measure | placement + measure; `splitTextToSize` wraps `maxWidth`       |
|  [05]   | `doc.rect` / `circle` / `line` / `path`                | vector draw    | primitive + path drawing surface, chainable                   |
|  [06]   | `doc.addPage(format?, orientation?)`                   | paginate       | report loop appends pages, stamps headers/footers per page    |
|  [07]   | `doc.setFont` / `setFontSize` / `addFont`              | graphics state | fluent state setters; `addFont` embeds a custom TTF           |
|  [08]   | `doc.table(x, y, data, headers, config)` / `cell(...)` | tabular        | structured-table primitive; cell hooks, auto-sizing over rows |

- `output(t)` browser tokens: `"blob"`/`"bloburi"`/`"datauristring"`/`"pdfobjectnewwindow"`.
- Text measure: `getTextWidth`/`getTextDimensions`/`splitTextToSize`.
- Vector primitives: `rect`/`roundedRect`/`circle`/`ellipse`/`line`/`lines`/`triangle`/`path`; path ops `moveTo`/`lineTo`/`curveTo`/`fill`/`stroke`.
- Pagination: `setPage`/`insertPage`/`deletePage`/`movePage`/`getNumberOfPages`.
- Graphics setters: `setTextColor`/`setDrawColor`/`setFillColor`/`setLineWidth`/`setLineDashPattern`/`setLineCap`/`setLineJoin`/`setGState`/`saveGraphicsState`/`restoreGraphicsState`; `setLineDashPattern` rules dashed separators, `save`/`restoreGraphicsState` scope a style block.
- Tabular: `setTableHeaderRow` pins the header row.

[ENTRYPOINT_SCOPE]: images, metadata, interactivity, and plugin registration
- rail: system-apis

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]  | [CONSUMER]                                                |
| :-----: | :--------------------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `doc.addImage(data, format, x, y, w, h, …)`                | image           | Node-safe bytes; `alias` dedups a logo                    |
|  [02]   | `doc.addSvgAsImage(svg, x, y, w, h, …)`                    | svg             | rasterize an SVG string, no `html()` DOM worker           |
|  [03]   | `doc.setDocumentProperties` / `setCreationDate`            | metadata        | catalog metadata, pinned date; language via `setLanguage` |
|  [04]   | `doc.addField(field)` / `doc.AcroForm.TextField()`         | interactive     | form fields, annotations, links, print, embedded JS       |
|  [05]   | `doc.outline.add(parent, title, { pageNumber })`           | bookmark        | build the navigation tree for a sectioned report          |
|  [06]   | `jsPDF.API` / `jsPDF.version` / `doc.internal.events`      | plugin registry | plugin on the prototype; `PubSub` cross-cut hooks         |
|  [07]   | `doc.setCreationDate(fixedDate)` + `jsPDFOptions.compress` | reproducibility | pin creation date + compression → byte-identical bytes    |

## [04]-[IMPLEMENTATION_LAW]

[JSPDF_TOPOLOGY]:
- `output` is one polymorphic egress, not a method per format: a format token selects the return — `"arraybuffer"` → `ArrayBuffer`, `"blob"` → `Blob`, `"datauristring"` → `string`, `"pdfobjectnewwindow"` → `Window`. The Node durable job pins `output("arraybuffer")`; the `Blob`/`URL`/`Window`/`save()` arms are browser-only and never appear in a `work` job.
- The Node build is real: `main` is `dist/jspdf.node.min.js`, so the programmatic API renders headless. The single browser-only capability is `html()` (html2canvas needs a DOM) and the download/`Blob` egress arms — the owner builds documents from the drawing/`table`/`addImage`-bytes API, never from `html()`, inside `work`.
- The document is built by a fluent fold: every draw/state method returns `jsPDF` for chaining, and the whole build runs inside one `Effect.sync` because rendering is synchronous and self-contained. The mutable document never crosses the `Effect` boundary — only its `Uint8Array` output does, once.
- Layout is measured, not guessed: `getTextDimensions`/`splitTextToSize`/`getTextWidth` drive a pure layout computation over the decoded rows, and the page loop appends `addPage` when the running `y` exceeds `internal.pageSize.getHeight()`. Paging is a fold over measured content, not imperative cursor arithmetic scattered through the body.
- Bytes are reproducible for the content-key cache: pinning `setCreationDate(fixed)` and `compress` makes identical rows produce identical bytes, so a report's `XxHash128` content key is stable and the artifact index can dedupe — the same discipline the IFC GLB cache uses.
- Encryption passwords are `Redacted`: `EncryptionOptions.userPassword`/`ownerPassword` arrive from `Config.redacted`, unwrapped only at construction. `userPermissions` is a bounded set (`print`/`modify`/`copy`/`annot-forms`), a policy value on the document.
- `jsPDF.API` is the extension seam: a reusable rendering capability (a branded header band, a signature block) registers once on the prototype through `jsPDF.API`, not re-implemented per call site — the plugin registry is the owner's collapse point for repeated document furniture.

[STACKS_WITH]:
- `effect` (`../../.api/effect.md`): `Effect.sync` wraps the whole build-and-`output("arraybuffer")`; `Schema.decodeUnknown` types the report rows before layout; `Effect.forEach`/`Array.reduce` fold rows into pages; `Config.redacted`/`Redacted` carry encryption passwords; `Effect.withSpan` tags the render with page count and byte size; the `XxHash128` content key over the bytes rides `kernel`'s `ContentKey` mint for the artifact cache.
- `@effect/platform` (`../../.api/effect-platform.md`): the `Uint8Array` document lands via `FileSystem.writeFile(path, bytes)` or streams through `FileSystem.sink`; egress over the wire is `HttpBody.uint8Array(bytes, "application/pdf")`; `makeTempDirectoryScoped` scopes a working directory around a multi-file render.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): jspdf's Node build composes headless under `NodeContext.layer`; `NodeStream.toReadable` turns the byte output into the `Readable` a nodemailer attachment or an HTTP body expects; a heavy render offloads to a `Worker` behind the `WorkerPool` Tag since pure-JS rendering is CPU-bound.
- `jszip` (`./jszip.md`) / `nodemailer` (`./nodemailer.md`): the PDF `Uint8Array` becomes a jszip entry in a multi-artifact bundle or a nodemailer `{ content: bytes, contentType: "application/pdf" }` attachment — jspdf produces bytes, the sibling owners transport them.
- `exceljs` (`./exceljs.md`) / `papaparse` (`./papaparse.md`): the report output-format peers — the same decoded rows render to PDF (`jspdf`), XLSX (`exceljs`), or CSV (`papaparse`) selected by one output-format policy row, never a forked pipeline per format.
- `@effect/workflow` (`./effect-workflow.md`): a report render is a durable `Activity` with a `Schedule` budget; the reproducible bytes make the activity idempotent, so a replay after a sink failure regenerates the identical document without a duplicate artifact.

[LOCAL_ADMISSION]:
- Use `output("arraybuffer")` → `Uint8Array` as the only egress in a Node durable job; never `save()`, `html()`, or `output("blob"\|"bloburi")` outside `browser`.
- Use one `Effect.sync` fold to build the document and cross the boundary once at `output`; never thread the mutable `jsPDF` through the `Effect` rail or `await` a render step.
- Use `getTextDimensions`/`splitTextToSize` to drive a measured paging fold; never hardcode line counts or cursor offsets per report type.
- Use `addImage` with `Uint8Array`/`RGBAData`/data-URI bytes; never a DOM `HTMLImageElement`/`HTMLCanvasElement` source in a Node job.
- Use `Config.redacted` for `EncryptionOptions` passwords and pin `setCreationDate`/`compress` for reproducible bytes; never inline a password or leave the creation date defaulted where a content-key cache depends on byte stability.
- Use `jsPDF.API` to register repeated document furniture once; never re-draw the same branded header/footer imperatively at every call site.

[RAIL_LAW]:
- Package: `jspdf`
- Owns: programmatic PDF rendering — the fluent drawing/pagination/graphics-state API, the polymorphic `output` egress, measured layout (`splitTextToSize`/`getTextDimensions`), structured `table`/`cell`, `addImage` from bytes, document encryption + metadata, interactive AcroForm/outline/annotation structure, vector patterns/`GState`/`Matrix`, and the `jsPDF.API` plugin registry
- Accept: an `Effect.sync` build fold with a single `output("arraybuffer")` → `Uint8Array` crossing, `Schema`-typed rows, measured paging, byte-reproducible construction for the content-key cache, `Redacted` encryption passwords, bytes routed to `FileSystem`/`HttpBody`/jszip/nodemailer, plugins registered on `jsPDF.API`
- Reject: `save()`/`html()`/`Blob`/`bloburi` egress in a Node job, the mutable document crossing the `Effect` boundary, hardcoded paging arithmetic, DOM image sources headless, inline encryption passwords, defaulted creation dates where byte stability matters, per-call-site re-drawing of shared furniture
