# [API_CATALOGUE] jspdf

`jspdf` is the programmatic PDF codec the `persistence/object#OBJECT_STORE` `AssetCodec` `pdf` arm rides: one `jsPDF` instance owns the document (`addPage`/`setPage`/`output`/`save`), the vector primitives (`rect`/`circle`/`line`/`triangle`/`path`/`lines` + the `moveTo`/`lineTo`/`curveTo` cursor), text and font control, `addImage` raster embedding, and a plugin surface reaching AcroForm interactive fields, the full Context2D canvas API, document outlines, annotations, graphics-state stacks, tiling/shading patterns, form XObjects, and a virtual font filesystem. `output(type)` is one polymorphic entry whose return type is discriminated by the type literal (string / `ArrayBuffer` / `Blob` / `URL` / `Window` / boolean), and every draw primitive carries one `style` discriminant (`"S"|"F"|"DF"|"FD"|null`) selecting stroke/fill/both. In the `services` node tier only the programmatic path is live: `new jsPDF()` → draw/`table` → `output("arraybuffer")` → `Uint8Array` → streamed `ObjectStore.put`; the `html()`/`addSvgAsImage`/`loadFile`/DOM-image paths need a browser DOM and are structurally dead here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jspdf`
- package: `jspdf` (target 4.2.1, MIT, © James Hall / yWorks GmbH)
- module format: ESM + UMD dist; self-typed at `types/index.d.ts` (`declare module "jspdf"`, default + named `jsPDF` export) — no `@types/jspdf`; pure-JS, zero native ABI
- runtime target: browser-first, node-capable — the programmatic build (`new jsPDF`, draw, `output`, `addImage(Uint8Array)`) runs in node; DOM-bound paths (`html`, `addSvgAsImage`, `loadFile`, `addImage(HTMLImageElement|HTMLCanvasElement)`) require a browser
- excluded peers: the `html()` plugin needs `html2canvas` + a DOM and `addSvgAsImage` needs `canvg` — neither is admitted in the workspace catalog, so both are inert in `services`
- asset: the `jsPDF` class, the `AcroForm*`/`Context2d`/`ShadingPattern`/`TilingPattern`/`GState`/`Matrix`/`Outline` families, the option/descriptor interfaces, the static `jsPDF.API` plugin registry
- consumer: `persistence/object#OBJECT_STORE` — the `AssetCodec` `pdf` literal (`doc.table(...)` + `output("arraybuffer")` → `ObjectStore.put`)
- rail: asset-codec / document-generation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, config, and page family
`jsPDF` is the single document owner; every operation returns `jsPDF` for chaining. `jsPDFOptions` carries construction (unit/format/compression/precision/encryption); `PageInfo`/`Font`/`DocumentProperties` are introspection shapes.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `jsPDF`              | class         | the document; all draw/page/output operations chain on the instance         |
|  [02]   | `jsPDFOptions`       | interface     | `orientation`/`unit`/`format`/`compress`/`precision`/`filters`/`encryption`/`putOnlyUsedFonts`/`floatPrecision`/`hotfixes` |
|  [03]   | `EncryptionOptions`  | interface     | `{ userPassword?, ownerPassword?, userPermissions?: ("print"\|"modify"\|"copy"\|"annot-forms")[] }` — a constructor option, not a `setEncryption` call |
|  [04]   | `DocumentProperties` | interface     | `title`/`subject`/`author`/`keywords`/`creator` metadata                     |
|  [05]   | `PageInfo`           | interface     | `{ objId, pageNumber, pageContext }` current-page metadata                    |
|  [06]   | `Font`               | interface     | resolved font descriptor (`id`/`fontName`/`fontStyle`/`postScriptName`/…)     |
|  [07]   | `Point` / `Rectangle` | interface    | `{ x, y }` / `{ x, y, w, h }` geometry                                        |

[PUBLIC_TYPE_SCOPE]: image, text, and table family
`ImageFormat`/`ImageCompression`/`ColorSpace` are the closed raster vocabularies; `TextOptionsLight` (align/baseline/`renderingMode`/`maxWidth`/`charSpace`) is the text-render axis; `TableConfig`/`CellConfig` drive the cell plugin the `pdf` codec calls.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `ImageOptions` / `ImageProperties`    | interface     | options-form `addImage` input / introspected image metadata        |
|  [02]   | `ImageFormat`                         | string union  | `"PNG"\|"JPEG"\|"JPG"\|"WEBP"\|"GIF89a"\|"BMP"\|"TIFF"\|"RGBA"\|…`  |
|  [03]   | `ImageCompression`                    | string union  | `"NONE"\|"FAST"\|"MEDIUM"\|"SLOW"`                                  |
|  [04]   | `ColorSpace`                          | string union  | `"DeviceRGB"\|"DeviceGray"\|"DeviceCMYK"\|"Indexed"\|"Pattern"\|…`  |
|  [05]   | `RGBAData`                            | interface     | `{ data: Uint8ClampedArray, width, height }` raw pixel buffer       |
|  [06]   | `TextOptions` / `TextOptionsLight`    | interface     | text placement + `align`/`baseline`/`renderingMode`/`maxWidth`/`angle`/`charSpace` |
|  [07]   | `TableConfig` / `CellConfig` / `TableRowData` / `TableCellData` | interface | cell-plugin layout, header, and per-row/cell hooks |

[PUBLIC_TYPE_SCOPE]: AcroForm interactive-field family
The `AcroForm*` hierarchy is a genuine inheritance chain, but construction goes through the one `jsPDF.AcroForm` factory (`doc.AcroForm.TextField()`), not per-class `new`. Field kind is a factory row, then `addField` attaches it.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `AcroFormField`                                                | abstract class | base field — position, `fieldName`, font, `readOnly`/`required` |
|  [02]   | `AcroFormChoiceField` → `AcroFormListBox` / `AcroFormComboBox` / `AcroFormEditBox` | class chain | option-list fields (`getOptions`/`addOption`/`combo`/`edit`) |
|  [03]   | `AcroFormButton` → `AcroFormPushButton` / `AcroFormRadioButton` / `AcroFormCheckBox` | class chain | button fields; `AcroFormRadioButton.createOption` → `AcroFormChildClass` |
|  [04]   | `AcroFormTextField` / `AcroFormPasswordField`                  | class          | text input (`multiline`/`maxLength`/`comb`/`password`) |
|  [05]   | `AcroFormChildClass`                                           | class          | a radio member (`optionName`/`appearanceState: "On"\|"Off"`) |

[PUBLIC_TYPE_SCOPE]: graphics-state, pattern, and canvas family
`GState` (opacity), `Matrix` (affine transform with `multiply`/`inversed`/`decompose`), and the `ShadingPattern`/`TilingPattern` fill objects compose the advanced-API drawing model; `Context2d` is the full canvas-2D surface `doc.context2d`/`doc.canvas.getContext()` exposes.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `GState`                                          | class+iface   | `{ opacity?, "stroke-opacity"? }` graphics state                |
|  [02]   | `Matrix`                                          | interface     | affine matrix — `multiply`/`inversed`/`decompose`/`applyToPoint` |
|  [03]   | `Pattern` / `PatternData`                         | interface     | fill-pattern base / `fill(pattern)` data shape                  |
|  [04]   | `ShadingPattern` / `ShadingPatternType` / `ShadingPatterStop` | class+union | axial/radial gradient fill (`"axial"\|"radial"`, stops)     |
|  [05]   | `TilingPattern`                                   | class         | repeating tiling fill (boundingBox/xStep/yStep)                 |
|  [06]   | `Context2d` / `Gradient`                          | interface     | canvas-2D API (`arc`/`bezierCurveTo`/`fillText`/`drawImage`/`createLinearGradient`/…) |
|  [07]   | `Outline` / `OutlineItem` / `Annotation` / `TextWithLinkOptions` | interface | document bookmarks and link/annotation descriptors        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle and output — `output(type)` is the one polymorphic egress
`output` overloads on the type literal to return the right shape; `arraybuffer` is the node-tier serialization, `blob`/`bloburi`/`*newwindow` are browser sinks, bare `save` is a browser download (or a `Promise` with `returnPromise`).

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `new jsPDF(options?)` / `new jsPDF(orientation?, unit?, format?, compress?)` | constructor | options-form or positional construction              |
|  [02]   | `addPage(format?, orientation?)` / `insertPage(before)` / `deletePage(n)` / `movePage(target, before)` / `setPage(n)` | page control | 1-based page tree mutation and active-page select |
|  [03]   | `getNumberOfPages()` / `getCurrentPageInfo()` / `getPageInfo(n)`     | page query     | page count and per-page metadata                     |
|  [04]   | `output(): string`                                                  | output         | raw PDF string                                       |
|  [05]   | `output("arraybuffer"): ArrayBuffer` / `output("blob"): Blob`       | output         | binary bytes — `arraybuffer` is the node serialization |
|  [06]   | `output("datauristring"\|"dataurlstring", { filename? }): string` / `output("bloburi"\|"bloburl"): URL` | output | data-URI / blob-URL forms |
|  [07]   | `output("dataurlnewwindow"\|"pdfobjectnewwindow"\|"pdfjsnewwindow", { filename? }): Window` | output | browser viewer sinks |
|  [08]   | `save(filename, { returnPromise: true }): Promise<void>` / `save(filename?): jsPDF` | output | browser download; promise arity for await |
|  [09]   | `setDocumentProperties(props)` / `setDisplayMode(zoom, layout?, pmode?)` / `viewerPreferences(opts)` / `addMetadata(xml, ns?)` | metadata | doc info, initial view, viewer prefs, XMP |

[ENTRYPOINT_SCOPE]: vector primitives and path — the `style` discriminant owns stroke/fill
Every primitive takes an optional `style` (`"S"` stroke, `"F"` fill, `"DF"`/`"FD"` both, `null` no-op); the path builder pairs `moveTo`/`lineTo`/`curveTo` with `stroke`/`fill`/`clip` terminators.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `rect` / `roundedRect` / `circle` / `ellipse` / `line` / `triangle` (…, `style?`) | draw | closed/open primitives with the `style` discriminant |
|  [02]   | `path(lines?, style?)` / `lines(lines, x, y, scale?, style?, closed?)` | draw         | arbitrary path / polyline-polygon                    |
|  [03]   | `moveTo` / `lineTo` / `curveTo(x1..x3,y3)`                           | path           | cursor movement and cubic Bézier segments            |
|  [04]   | `stroke` / `fill(pattern?)` / `fillStroke` / `fillEvenOdd` / `clip("evenodd"?)` / `clipEvenOdd` / `close` / `discardPath` | path | fill-rule-aware path finalization |
|  [05]   | `saveGraphicsState()` / `restoreGraphicsState()` / `addGState(key, GState)` / `setGState(g)` | gstate | push/pop the opacity+transform state |
|  [06]   | `setCurrentTransformationMatrix(m)` / `Matrix(a..f)` / `matrixMult(m1, m2)` | transform | CTM and affine matrix construction               |
|  [07]   | `addShadingPattern(key, p)` / `beginTilingPattern(p)` / `endTilingPattern(key, p)` / `beginFormObject`/`endFormObject`/`doFormObject` | pattern/xobject | gradient/tiling fills and reusable form XObjects |

[ENTRYPOINT_SCOPE]: text, font, and color
Fonts register through `addFont` (built-in Helvetica/Courier/Times need none); `setDrawColor`/`setFillColor`/`setTextColor` overload on string vs numeric channels; the split/measure helpers own word-wrap and metrics.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `text(text, x, y, options?: TextOptionsLight, transform?)`          | draw           | placed text with align/baseline/`renderingMode`/`maxWidth` |
|  [02]   | `addFont(postScriptName\|URL, id, fontStyle, fontWeight?, encoding?)` / `setFont(name, style?, weight?)` / `setFontSize(pt)` | font | register / select font; `getFont`/`getFontList`/`getFontSize` |
|  [03]   | `getTextDimensions(text, opts?)` / `getTextWidth(text)` / `splitTextToSize(text, maxlen, opts?)` | font | measure and word-wrap                             |
|  [04]   | `setDrawColor` / `setFillColor` / `setTextColor` (`ch1: string` \| `ch1..ch4: number`) | color | active draw/fill/text color; `getDrawColor`/`getFillColor`/`getTextColor` |
|  [05]   | `setLineWidth` / `setLineCap` / `setLineJoin` / `setLineDashPattern` / `setLineMiterLimit` / `setLineHeightFactor` | color | stroke geometry |
|  [06]   | `setLanguage(langCode)` / `setR2L(bool)` / `setCharSpace(n)`         | text           | script/direction/spacing controls                   |

[ENTRYPOINT_SCOPE]: plugin surface — image, table, form, annotation, outline, vfs
`table` is the cell-plugin the `pdf` codec calls; `addImage` overloads on explicit-vs-inferred format vs options-form; `addFileToVFS` is the node-safe font-load seam (no `loadFile` XHR).

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `addImage(imageData, format, x, y, w, h, alias?, compression?, rotation?)` | image    | explicit-format raster embed (node: `string`/`Uint8Array`/`RGBAData`) |
|  [02]   | `addImage(imageData, x, y, w, h, …)` / `addImage(options: ImageOptions)` | image      | format-inferred / options-form embed; `getImageProperties(data)` |
|  [03]   | `table(x, y, data, headers: string[] \| CellConfig[], config: TableConfig)` | cell plugin | tabular layout — the `pdf` `AssetCodec` entry |
|  [04]   | `cell(...)` / `cellAddPage()` / `setTableHeaderRow(cfg)` / `setHeaderFunction(fn)` | cell plugin | low-level cell flow                            |
|  [05]   | `addField(field: AcroFormField)` + `AcroForm.{TextField,CheckBox,ComboBox,RadioButton,PushButton,ListBox,…}()` | acroform | interactive form-field factory + attach |
|  [06]   | `createAnnotation(a: Annotation)` / `link(x, y, w, h, opts)` / `textWithLink(text, x, y, opts)` | annotation | link/annotation regions |
|  [07]   | `outline.add(parent, title, { pageNumber })`                        | outline        | PDF bookmark tree                                    |
|  [08]   | `addFileToVFS(name, content)` / `getFileFromVFS(name)` / `existsFileInVFS(name)` | vfs        | virtual filesystem for embedded fonts (node-safe)   |
|  [09]   | `context2d` / `canvas.getContext(): Context2d`                      | canvas         | full canvas-2D drawing API                           |
|  [10]   | `addJS(js)` / `autoPrint(opts?)` / `putTotalPages(expr)`            | plugin         | embedded JS, auto-print, `{total_pages}` substitution |

## [04]-[IMPLEMENTATION_LAW]

[JSPDF_TOPOLOGY]:
- One `jsPDF` instance is one document; `addPage` extends, `setPage` retargets, numbering is 1-based. Every draw/config method returns the instance for chaining, so a document is one fluent expression terminated by `output`/`save`.
- `output` is a single overloaded surface: the return type follows the type literal — `"arraybuffer"` → `ArrayBuffer`, `"blob"` → `Blob`, `"bloburi"` → `URL`, `"datauristring"` → `string`, `"*newwindow"` → `Window`, bare → `string`. Never an `outputBuffer`/`outputBlob` family.
- Fonts: the three cores (`Helvetica`/`Courier`/`Times`) need no registration; a custom font is `addFileToVFS(name, base64Ttf)` then `addFont(name, id, style)` — the VFS path is the node-safe mechanism (`loadFile` is browser XHR). Advanced drawing (`ShadingPattern`, form XObjects, CTM) requires `advancedAPI(body)` mode.
- `internal` (`events` PubSub, `scaleFactor`, `pageSize`, `getEncryptor`) is the private plugin substrate — read for plugin authoring only, never mutated from domain code.

[LOCAL_ADMISSION]:
- `jsPDFOptions.unit` defaults to `"mm"` over `"pt"|"px"|"in"|"cm"|"ex"|"em"|"pc"`; `format` is a named size (`"a4"`) or a `[w, h]` array in the chosen unit; `compress`/`precision`/`floatPrecision` tune output size.
- In node, `addImage` accepts `string` (data URI) / `Uint8Array` / `RGBAData` only — `HTMLImageElement`/`HTMLCanvasElement` inputs and `addSvgAsImage` require a DOM; raster upstream through `sharp` to `Uint8Array` instead.
- Encryption is the `jsPDFOptions.encryption` construction option (`userPassword`/`ownerPassword`/`userPermissions`), applied at document creation, not a post-hoc call.

[STACKING]:
- Effect boundary: the programmatic build is synchronous, folded at `Effect.try` (not `tryPromise`) into the `persistence/object#OBJECT_STORE` `AssetTransferFault` rail (`format: "pdf", stage: "encode"`); `output("arraybuffer")` → `new Uint8Array(buf)` → `Stream.make` → `ObjectStore.put`. The `save(...)` browser-download surface is never reached server-side.
- sharp raster: `sharp(input).toFormat("png").toBuffer()` (`.api/sharp.md`) → `Uint8Array` → `addImage(bytes, "PNG", x, y, w, h)` composites a processed image into the PDF without a DOM `HTMLImageElement`.
- jszip bundling: multiple `output("arraybuffer")` documents become `file(path, bytes)` entries in a `jszip` archive (`.api/jszip.md`) for a multi-document export streamed as one `ObjectStore.put`.
- nodemailer attachment: `output("arraybuffer")` bytes ride a `nodemailer` `Mail.Options.attachments` entry (`{ filename, content: Buffer }`, `.api/nodemailer.md`) so a notification carries the generated report; content-address and OTLP-progress stacking mirror the other codecs.
- The `html()` / `addSvgAsImage` plugins stay unwired: `html2canvas`/`canvg` are absent from the workspace catalog and both need a DOM, so HTML-to-PDF is a browser-only capability documented for completeness, not a `services` rail.

[RAIL_LAW]:
- package: `jspdf`
- owns: programmatic PDF document construction, vector/text/image drawing, AcroForm/Context2D/outline/annotation plugins, and the type-discriminated `output` egress
- accept: one `jsPDF` per document chained to a single `output("arraybuffer")`; the `style` discriminant for every primitive; `addFileToVFS`+`addFont` for custom fonts; `sharp`-produced `Uint8Array` for `addImage`
- reject: hand-rolled PDF byte assembly; direct `jsPDF.internal` mutation in domain code; the `html()`/`addSvgAsImage`/`loadFile` DOM paths in the node tier; an `outputBuffer`/`outputBlob` method family where `output(type)` discriminates
