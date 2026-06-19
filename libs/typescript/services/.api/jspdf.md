# [API_CATALOGUE] jspdf

`jspdf` supplies client-side PDF document generation: the `jsPDF` class owns the document lifecycle (`addPage`, `deletePage`, `save`, `output`), drawing primitives (`rect`, `circle`, `ellipse`, `line`, `text`), image embedding (`addImage`, `addSvgAsImage`), font management (`addFont`, `setFont`, `setFontSize`), and plugin-extended capabilities including AcroForm fields, HTML-to-PDF rendering, table layout, and Context2D canvas API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jspdf`
- package: `jspdf`
- module: `jspdf`
- asset: `jsPDF` class, AcroForm classes, pattern/gradient types, option interfaces
- rail: document-generation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and config family
- rail: document-generation

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :------------------- | :------------ | :--------------------------------------- |
|  [01]   | `jsPDF`              | class         | PDF document owner; all operations chain |
|  [02]   | `jsPDFOptions`       | interface     | constructor options                      |
|  [03]   | `PageInfo`           | interface     | current page metadata                    |
|  [04]   | `Font`               | interface     | font descriptor                          |
|  [05]   | `DocumentProperties` | interface     | PDF metadata fields                      |
|  [06]   | `EncryptionOptions`  | interface     | 128-bit RC4 or AES encryption config     |
|  [07]   | `Point`              | interface     | `{ x, y }` coordinate                    |
|  [08]   | `Rectangle`          | interface     | `Point` extended with `w`, `h`           |

[PUBLIC_TYPE_SCOPE]: image and text family
- rail: document-generation

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :----------------- | :------------ | :--------------------------------------- |
|  [01]   | `ImageOptions`     | interface     | `addImage` options-form input            |
|  [02]   | `ImageProperties`  | interface     | introspected image metadata              |
|  [03]   | `ImageCompression` | string union  | `"NONE" \| "FAST" \| "MEDIUM" \| "SLOW"` |
|  [04]   | `ImageFormat`      | string union  | supported image format tokens            |
|  [05]   | `ColorSpace`       | string union  | color space tokens                       |
|  [06]   | `TextOptions`      | interface     | text alignment, baseline, render options |
|  [07]   | `TextOptionsLight` | interface     | lighter text option subset               |

[PUBLIC_TYPE_SCOPE]: AcroForm field family
- rail: document-generation

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]               |
| :-----: | :---------------------- | :------------- | :------------------- |
|  [01]   | `AcroFormField`         | abstract class | base form field      |
|  [02]   | `AcroFormChoiceField`   | class          | choice field base    |
|  [03]   | `AcroFormListBox`       | class          | list box field       |
|  [04]   | `AcroFormComboBox`      | class          | combo box field      |
|  [05]   | `AcroFormEditBox`       | class          | editable combo box   |
|  [06]   | `AcroFormButton`        | class          | button field base    |
|  [07]   | `AcroFormPushButton`    | class          | push button          |
|  [08]   | `AcroFormRadioButton`   | class          | radio button         |
|  [09]   | `AcroFormCheckBox`      | class          | check box            |
|  [10]   | `AcroFormTextField`     | class          | text input field     |
|  [11]   | `AcroFormPasswordField` | class          | password input field |

[PUBLIC_TYPE_SCOPE]: graphics and pattern family
- rail: document-generation

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------- | :------------ | :---------------------------- |
|  [01]   | `GState`             | class         | graphics state object         |
|  [02]   | `Matrix`             | interface     | transformation matrix         |
|  [03]   | `Pattern`            | interface     | fill pattern base             |
|  [04]   | `ShadingPattern`     | class         | axial or radial gradient fill |
|  [05]   | `ShadingPatternType` | string union  | `"axial" \| "radial"`         |
|  [06]   | `TilingPattern`      | class         | tiling fill pattern           |
|  [07]   | `Gradient`           | interface     | gradient descriptor           |
|  [08]   | `PatternData`        | interface     | fill pattern data shape       |
|  [09]   | `RGBAData`           | interface     | raw RGBA pixel buffer         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle
- rail: document-generation

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `new jsPDF(options?: jsPDFOptions)`                  | constructor    | options-form construction         |
|  [02]   | `new jsPDF(orientation?, unit?, format?, compress?)` | constructor    | positional construction           |
|  [03]   | `addPage(format?, orientation?)`                     | page control   | appends a new page                |
|  [04]   | `deletePage(targetPage)`                             | page control   | removes a page by 1-based index   |
|  [05]   | `insertPage(beforePage)`                             | page control   | inserts page before given index   |
|  [06]   | `movePage(targetPage, beforePage)`                   | page control   | reorders a page                   |
|  [07]   | `setPage(pageNumber)`                                | page control   | sets current active page          |
|  [08]   | `getNumberOfPages()`                                 | page query     | total page count                  |
|  [09]   | `getCurrentPageInfo()`                               | page query     | active page metadata              |
|  [10]   | `save(filename?, options?)`                          | output         | browser download or promise       |
|  [11]   | `output(type?, options?)`                            | output         | buffer, blob, data URI, or string |
|  [12]   | `setDocumentProperties(properties)`                  | metadata       | title, author, subject, etc.      |

[ENTRYPOINT_SCOPE]: drawing primitives
- rail: document-generation

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]               |
| :-----: | :------------------------------------------------ | :------------- | :------------------- |
|  [01]   | `text(text, x, y, options?, transform?)`          | draw           | text at position     |
|  [02]   | `rect(x, y, w, h, style?)`                        | draw           | rectangle            |
|  [03]   | `circle(x, y, r, style?)`                         | draw           | circle               |
|  [04]   | `ellipse(x, y, rx, ry, style?)`                   | draw           | ellipse              |
|  [05]   | `line(x1, y1, x2, y2, style?)`                    | draw           | straight line        |
|  [06]   | `triangle(x1,y1,x2,y2,x3,y3, style?)`             | draw           | triangle             |
|  [07]   | `roundedRect(x, y, w, h, rx, ry, style?)`         | draw           | rounded rectangle    |
|  [08]   | `path(lines?, style?)`                            | draw           | arbitrary path       |
|  [09]   | `lines(lines, x, y, scale?, style?, closed?)`     | draw           | polyline or polygon  |
|  [10]   | `moveTo(x, y)` / `lineTo(x, y)`                   | path           | path cursor movement |
|  [11]   | `curveTo(x1,y1,x2,y2,x3,y3)`                      | path           | cubic Bezier segment |
|  [12]   | `clip(rule?)` / `close()` / `stroke()` / `fill()` | path           | path finalization    |

[ENTRYPOINT_SCOPE]: font and color
- rail: document-generation

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `addFont(postScriptName, id, fontStyle, fontWeight?, …)`       | font           | registers a font by name/URL     |
|  [02]   | `setFont(fontName, fontStyle?, fontWeight?)`                   | font           | sets active font                 |
|  [03]   | `setFontSize(size)`                                            | font           | sets point size                  |
|  [04]   | `getFontList()`                                                | font           | all registered fonts             |
|  [05]   | `getTextDimensions(text, options?)`                            | font           | `{ w, h }` measurement           |
|  [06]   | `splitTextToSize(text, maxlen, options?)`                      | font           | word-wrapped line array          |
|  [07]   | `setDrawColor(ch1, …)` / `setFillColor(…)` / `setTextColor(…)` | color          | sets active draw/fill/text color |
|  [08]   | `setLineWidth(width)`                                          | color          | sets stroke width                |

[ENTRYPOINT_SCOPE]: image and plugin surface
- rail: document-generation

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `addImage(imageData, format, x, y, w, h, …)` | image          | embeds raster image by format string |
|  [02]   | `addImage(imageData, x, y, w, h, …)`         | image          | embeds raster image, format inferred |
|  [03]   | `addImage(options: ImageOptions)`            | image          | options-form image embed             |
|  [04]   | `addSvgAsImage(svg, x, y, w, h, alias?, …)`  | image          | renders SVG string as image          |
|  [05]   | `html(src, options?)`                        | html plugin    | renders HTML to PDF via html2canvas  |
|  [06]   | `table(x, y, data, headers, config)`         | cell plugin    | tabular layout                       |
|  [07]   | `addField(field: AcroFormField)`             | acroform       | adds interactive form field          |

## [04]-[IMPLEMENTATION_LAW]

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
