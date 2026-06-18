# [API_CATALOGUE] jspdf

`jspdf` supplies client-side PDF document generation: the `jsPDF` class owns the document lifecycle (`addPage`, `deletePage`, `save`, `output`), drawing primitives (`rect`, `circle`, `ellipse`, `line`, `text`), image embedding (`addImage`, `addSvgAsImage`), font management (`addFont`, `setFont`, `setFontSize`), and plugin-extended capabilities including AcroForm fields, HTML-to-PDF rendering, table layout, and Context2D canvas API.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jspdf`
- package: `jspdf`
- module: `jspdf`
- asset: `jsPDF` class, AcroForm classes, pattern/gradient types, option interfaces
- rail: document-generation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and config family
- rail: document-generation

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :------------------- | :------------ | :--------------------------------------- |
|   [1]   | `jsPDF`              | class         | PDF document owner; all operations chain |
|   [2]   | `jsPDFOptions`       | interface     | constructor options                      |
|   [3]   | `PageInfo`           | interface     | current page metadata                    |
|   [4]   | `Font`               | interface     | font descriptor                          |
|   [5]   | `DocumentProperties` | interface     | PDF metadata fields                      |
|   [6]   | `EncryptionOptions`  | interface     | 128-bit RC4 or AES encryption config     |
|   [7]   | `Point`              | interface     | `{ x, y }` coordinate                    |
|   [8]   | `Rectangle`          | interface     | `Point` extended with `w`, `h`           |

[PUBLIC_TYPE_SCOPE]: image and text family
- rail: document-generation

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :----------------- | :------------ | :--------------------------------------- |
|   [1]   | `ImageOptions`     | interface     | `addImage` options-form input            |
|   [2]   | `ImageProperties`  | interface     | introspected image metadata              |
|   [3]   | `ImageCompression` | string union  | `"NONE" \| "FAST" \| "MEDIUM" \| "SLOW"` |
|   [4]   | `ImageFormat`      | string union  | supported image format tokens            |
|   [5]   | `ColorSpace`       | string union  | color space tokens                       |
|   [6]   | `TextOptions`      | interface     | text alignment, baseline, render options |
|   [7]   | `TextOptionsLight` | interface     | lighter text option subset               |

[PUBLIC_TYPE_SCOPE]: AcroForm field family
- rail: document-generation

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]               |
| :-----: | :---------------------- | :------------- | :------------------- |
|   [1]   | `AcroFormField`         | abstract class | base form field      |
|   [2]   | `AcroFormChoiceField`   | class          | choice field base    |
|   [3]   | `AcroFormListBox`       | class          | list box field       |
|   [4]   | `AcroFormComboBox`      | class          | combo box field      |
|   [5]   | `AcroFormEditBox`       | class          | editable combo box   |
|   [6]   | `AcroFormButton`        | class          | button field base    |
|   [7]   | `AcroFormPushButton`    | class          | push button          |
|   [8]   | `AcroFormRadioButton`   | class          | radio button         |
|   [9]   | `AcroFormCheckBox`      | class          | check box            |
|  [10]   | `AcroFormTextField`     | class          | text input field     |
|  [11]   | `AcroFormPasswordField` | class          | password input field |

[PUBLIC_TYPE_SCOPE]: graphics and pattern family
- rail: document-generation

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------- | :------------ | :---------------------------- |
|   [1]   | `GState`             | class         | graphics state object         |
|   [2]   | `Matrix`             | interface     | transformation matrix         |
|   [3]   | `Pattern`            | interface     | fill pattern base             |
|   [4]   | `ShadingPattern`     | class         | axial or radial gradient fill |
|   [5]   | `ShadingPatternType` | string union  | `"axial" \| "radial"`         |
|   [6]   | `TilingPattern`      | class         | tiling fill pattern           |
|   [7]   | `Gradient`           | interface     | gradient descriptor           |
|   [8]   | `PatternData`        | interface     | fill pattern data shape       |
|   [9]   | `RGBAData`           | interface     | raw RGBA pixel buffer         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle
- rail: document-generation

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `new jsPDF(options?: jsPDFOptions)`                  | constructor    | options-form construction         |
|   [2]   | `new jsPDF(orientation?, unit?, format?, compress?)` | constructor    | positional construction           |
|   [3]   | `addPage(format?, orientation?)`                     | page control   | appends a new page                |
|   [4]   | `deletePage(targetPage)`                             | page control   | removes a page by 1-based index   |
|   [5]   | `insertPage(beforePage)`                             | page control   | inserts page before given index   |
|   [6]   | `movePage(targetPage, beforePage)`                   | page control   | reorders a page                   |
|   [7]   | `setPage(pageNumber)`                                | page control   | sets current active page          |
|   [8]   | `getNumberOfPages()`                                 | page query     | total page count                  |
|   [9]   | `getCurrentPageInfo()`                               | page query     | active page metadata              |
|  [10]   | `save(filename?, options?)`                          | output         | browser download or promise       |
|  [11]   | `output(type?, options?)`                            | output         | buffer, blob, data URI, or string |
|  [12]   | `setDocumentProperties(properties)`                  | metadata       | title, author, subject, etc.      |

[ENTRYPOINT_SCOPE]: drawing primitives
- rail: document-generation

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]               |
| :-----: | :------------------------------------------------ | :------------- | :------------------- |
|   [1]   | `text(text, x, y, options?, transform?)`          | draw           | text at position     |
|   [2]   | `rect(x, y, w, h, style?)`                        | draw           | rectangle            |
|   [3]   | `circle(x, y, r, style?)`                         | draw           | circle               |
|   [4]   | `ellipse(x, y, rx, ry, style?)`                   | draw           | ellipse              |
|   [5]   | `line(x1, y1, x2, y2, style?)`                    | draw           | straight line        |
|   [6]   | `triangle(x1,y1,x2,y2,x3,y3, style?)`             | draw           | triangle             |
|   [7]   | `roundedRect(x, y, w, h, rx, ry, style?)`         | draw           | rounded rectangle    |
|   [8]   | `path(lines?, style?)`                            | draw           | arbitrary path       |
|   [9]   | `lines(lines, x, y, scale?, style?, closed?)`     | draw           | polyline or polygon  |
|  [10]   | `moveTo(x, y)` / `lineTo(x, y)`                   | path           | path cursor movement |
|  [11]   | `curveTo(x1,y1,x2,y2,x3,y3)`                      | path           | cubic Bezier segment |
|  [12]   | `clip(rule?)` / `close()` / `stroke()` / `fill()` | path           | path finalization    |

[ENTRYPOINT_SCOPE]: font and color
- rail: document-generation

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `addFont(postScriptName, id, fontStyle, fontWeight?, …)`       | font           | registers a font by name/URL     |
|   [2]   | `setFont(fontName, fontStyle?, fontWeight?)`                   | font           | sets active font                 |
|   [3]   | `setFontSize(size)`                                            | font           | sets point size                  |
|   [4]   | `getFontList()`                                                | font           | all registered fonts             |
|   [5]   | `getTextDimensions(text, options?)`                            | font           | `{ w, h }` measurement           |
|   [6]   | `splitTextToSize(text, maxlen, options?)`                      | font           | word-wrapped line array          |
|   [7]   | `setDrawColor(ch1, …)` / `setFillColor(…)` / `setTextColor(…)` | color          | sets active draw/fill/text color |
|   [8]   | `setLineWidth(width)`                                          | color          | sets stroke width                |

[ENTRYPOINT_SCOPE]: image and plugin surface
- rail: document-generation

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `addImage(imageData, format, x, y, w, h, …)` | image          | embeds raster image by format string |
|   [2]   | `addImage(imageData, x, y, w, h, …)`         | image          | embeds raster image, format inferred |
|   [3]   | `addImage(options: ImageOptions)`            | image          | options-form image embed             |
|   [4]   | `addSvgAsImage(svg, x, y, w, h, alias?, …)`  | image          | renders SVG string as image          |
|   [5]   | `html(src, options?)`                        | html plugin    | renders HTML to PDF via html2canvas  |
|   [6]   | `table(x, y, data, headers, config)`         | cell plugin    | tabular layout                       |
|   [7]   | `addField(field: AcroFormField)`             | acroform       | adds interactive form field          |

## [4]-[IMPLEMENTATION_LAW]

[JSPDF_TOPOLOGY]:
- One `jsPDF` instance owns one document; call `addPage` to extend and `setPage` to switch the active drawing target.
- All drawing operations affect the current active page; page numbering is 1-based throughout.
- `output("blob")` returns a `Blob`; `output("arraybuffer")` returns an `ArrayBuffer`; `save(filename, { returnPromise: true })` returns `Promise<void>`.
- `addImage` accepts `string` (data URI or file path in Node), `HTMLImageElement`, `HTMLCanvasElement`, `Uint8Array`, or `RGBAData`; compression defaults to `"SLOW"`.
- Fonts must be registered with `addFont` before use; the built-in core fonts (`"Helvetica"`, `"Courier"`, `"Times"`) need no registration.
- `html(src, options)` returns `HTMLWorker extends Promise<any>`; it depends on `html2canvas` being available in the bundle.

[LOCAL_ADMISSION]:
- `jsPDFOptions.unit` defaults to `"mm"`; supported values are `"pt"`, `"px"`, `"in"`, `"mm"`, `"cm"`, `"ex"`, `"em"`, `"pc"`.
- `jsPDFOptions.format` accepts named page sizes (e.g. `"a4"`) or `[width, height]` arrays in the chosen unit.
- AcroForm fields are created via the factory methods on `jsPDF.AcroForm` and added with `addField`.

[RAIL_LAW]:
- Package: `jspdf`
- Owns: PDF document construction, drawing, images, fonts, form fields, HTML rendering
- Accept: one `jsPDF` instance per document; chain all operations before calling `save` or `output`
- Reject: hand-rolled PDF byte construction; direct manipulation of `jsPDF.internal` in domain code
