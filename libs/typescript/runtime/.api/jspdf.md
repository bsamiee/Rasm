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

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------ |:---------------- |:--------------------------------------------------------------------- |
| [01] | `jsPDF` | document | the mutable builder — the chainable drawing/pagination/graphics API and the `output` egress; one instance per durable report job |
| [02] | `jsPDFOptions` | construct policy | `orientation`, `unit` (`pt`/`mm`/`in`/…), `format` (named or `number[]`), `compress`, `precision`/`floatPrecision`, `filters`, `encryption`, `putOnlyUsedFonts`, `userUnit` |
| [03] | `EncryptionOptions` | security policy | `userPassword`, `ownerPassword`, `userPermissions` (`print`/`modify`/`copy`/`annot-forms`) — at-rest document protection; passwords held as `Redacted` |
| [04] | `DocumentProperties` | metadata | `title`/`subject`/`author`/`keywords`/`creator` — the report's catalog metadata set once via `setDocumentProperties` |
| [05] | `TextOptionsLight` / `TextOptions` | text policy | `align`, `angle`, `baseline`, `charSpace`, `lineHeightFactor`, `maxWidth`, `renderingMode`, RTL flags — the per-`text` layout options |
| [06] | `ImageOptions` / `ImageProperties` / `ImageCompression` / `ImageFormat` | image | the `addImage` object form and its `PNG`/`JPEG`/`WEBP` format + `NONE`/`FAST`/`MEDIUM`/`SLOW` compression policy; `getImageProperties` reads dimensions/colorspace |
| [07] | `Font` / `PageInfo` | receipt | `getFont`/`getFontList` font evidence; `getCurrentPageInfo`/`getPageInfo` page-object references for cross-page links |

[PUBLIC_TYPE_SCOPE]: interactive structure and vector graphics
- rail: system-apis

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------ |:---------------- |:--------------------------------------------------------------------- |
| [01] | `TableConfig` / `CellConfig` / `TableRowData` | table policy | the `table`/`cell` structured-data layout — `printHeaders`, `margins`, `headerBackgroundColor`, `rowStart`/`cellStart` hooks; the tabular-report primitive |
| [02] | `AcroFormField` (`TextField`/`CheckBox`/`ComboBox`/`RadioButton`/`ListBox`/`PushButton`/`PasswordField`) | form field | interactive PDF-form fields added via `addField`; `x`/`y`/`width`/`height`, `fieldName`, `value`, `readOnly`/`required` |
| [03] | `Annotation` / `TextWithLinkOptions` | annotation | `createAnnotation` (`text`/`freetext`/`link` with `bounds`/`contents`) and `textWithLink` navigation targets |
| [04] | `Outline` / `OutlineItem` / `OutlineOptions` | bookmark | `outline.add(parent, title, { pageNumber })` — the document navigation tree for a multi-section report |
| [05] | `ViewerPreferencesInput` | viewer policy | `HideToolbar`/`FitWindow`/`DisplayDocTitle`/`Duplex`/`PrintScaling`/`NumCopies` — reader-behavior hints |
| [06] | `ShadingPattern` / `TilingPattern` / `GState` / `Matrix` | vector | axial/radial gradients, tiling fills, opacity `GState`, and affine `Matrix` transforms (`multiply`/`inversed`/`decompose`/`applyToPoint`) for rich vector content |
| [07] | `Context2d` / `Gradient` / `RGBAData` | canvas emulation | the HTML-canvas-compatible drawing context (`fillRect`/`arc`/`bezierCurveTo`/`drawImage`) for porting canvas rendering code |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, draw, paginate, and emit
- rail: boundaries

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------------------------------------- |:-------------- |:--------------------------------------------------------------- |
| [01] | `new jsPDF(options?)` | construct | the report document; built inside one `Effect.sync` fold with `unit`/`format`/`encryption` as policy values |
| [02] | `doc.output("arraybuffer")` → `ArrayBuffer` | node egress | the durable-job byte path — `Effect.sync` → `new Uint8Array(ab)` → `FileSystem`/attachment/jszip entry |
| [03] | `doc.output(t, opts?)` — `t`: `"blob"`/`"bloburi"`/`"datauristring"`/`"pdfobjectnewwindow"` | browser egress | DOM-only variants (`Blob`/`URL`/data-URI/`Window`); flagged non-Node — `browser/transport` only |
| [04] | `doc.text(text, x, y, options?)` / `getTextWidth` / `getTextDimensions` / `splitTextToSize` | text + measure | content placement with layout measurement; `splitTextToSize` wraps to `maxWidth`, the paging loop's width oracle |
| [05] | `doc.rect`/`roundedRect`/`circle`/`ellipse`/`line`/`lines`/`triangle`/`path` + `moveTo`/`lineTo`/`curveTo`/`fill`/`stroke` | vector draw | the primitive + path drawing surface; chainable, folded over the layout model |
| [06] | `doc.addPage(format?, orientation?)` / `setPage` / `insertPage` / `deletePage` / `movePage` / `getNumberOfPages` | paginate | multi-page management; the report loop appends pages and stamps headers/footers per page |
| [07] | `doc.setFont`/`setFontSize`/`addFont`/`setTextColor`/`setDrawColor`/`setFillColor`/`setLineWidth`/`setLineDashPattern`/`setLineCap`/`setLineJoin`/`setGState`/`saveGraphicsState`/`restoreGraphicsState` | graphics state | fluent state setters; `addFont` embeds a custom TTF, `setLineDashPattern` rules dashed separators, `save`/`restoreGraphicsState` scope a style block |
| [08] | `doc.table(x, y, data, headers, config)` / `doc.cell(...)` / `setTableHeaderRow` | tabular | the structured-table primitive for a data report — headers, cell hooks, auto-sizing over decoded rows |

[ENTRYPOINT_SCOPE]: images, metadata, interactivity, and plugin registration
- rail: system-apis

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------------------------------------- |:-------------- |:--------------------------------------------------------------- |
| [01] | `doc.addImage(data, format, x, y, w, h, alias?, compression?, rotation?)` / `getImageProperties` | image | `data` accepts a `Uint8Array`/`RGBAData`/data-URI — the Node-safe image path (bytes, not a DOM `<img>`); `alias` dedups a repeated logo |
| [02] | `doc.addSvgAsImage(svg, x, y, w, h, …)` | svg | rasterize an SVG string into the document without the `html()` DOM worker |
| [03] | `doc.setDocumentProperties(props)` / `setCreationDate` / `setLanguage(code)` / `setDisplayMode` / `setR2L` | metadata | catalog metadata, deterministic creation date (reproducible bytes), language + reading direction |
| [04] | `doc.addField(field)` / `doc.AcroForm.TextField()` … / `createAnnotation` / `link` / `textWithLink` / `autoPrint` / `addJS` | interactive | form fields, annotations, hyperlinks, print-on-open, embedded JS — the interactive-report surface |
| [05] | `doc.outline.add(parent, title, { pageNumber })` | bookmark | build the navigation tree for a sectioned report |
| [06] | `jsPDF.API` (`static`) / `jsPDF.version` / `doc.internal.events` (`PubSub`) | plugin registry | register a reusable rendering plugin once on the prototype; the `PubSub` event bus for cross-cut hooks |
| [07] | `doc.setCreationDate(fixedDate)` + `jsPDFOptions.compress` | reproducibility | pin the creation date and compression so the same rows produce byte-identical output for a content-key cache |

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
