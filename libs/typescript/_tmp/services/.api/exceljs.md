# [API_CATALOGUE] exceljs

`exceljs` is the XLSX/CSV spreadsheet engine — `Workbook` root, `Worksheet`/`Row`/`Column`/`Cell` model, the full style/value/formula algebra, data validations, the discriminated conditional-formatting rule family, Excel tables, defined names, images, worksheet protection, and both buffered and streaming XLSX IO (`stream.xlsx.WorkbookWriter`/`WorkbookReader`). In `services` it is never a standalone reader/writer: it is the `"xlsx"` arm of the `persistence/object#OBJECT_STORE` `AssetCodec` `Match.value(format)` fan-out — a `Promise`/Node-stream library lifted at the Effect boundary (`Effect.tryPromise` for buffered, `NodeStream.fromWritable`/`toReadable` for streamed), emitting a `Uint8Array`/`Readable` that `ObjectStore.put` content-addresses into S3. Every raw `wb.xlsx.*` call lives behind that codec seam; a bare Promise or a hand-rolled zip/XML assembly is the named defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `exceljs`
- package: `exceljs` (4.4.0, MIT, © Guyon Roche)
- module format: CommonJS (`main: excel.js`), self-typed via the bundled ambient `index.d.ts` (`types`) — default import `import ExcelJS from "exceljs"` under `esModuleInterop`, then `new ExcelJS.Workbook()` / `new ExcelJS.stream.xlsx.WorkbookWriter(opts)`; no `@types/exceljs`
- runtime target: `node` (a `scope:node` dependency) — buffered IO returns a Node `Buffer`, streaming rides `node:stream` `Readable`/`Writable` and the `archiver` zip pipeline (`node:zlib`); `Buffer`/`node:stream`/`archiver` bind it to the durable interior, never the browser bundle
- asset: the `Workbook` root, the `Worksheet`/`Row`/`Column`/`Cell` model, the style/value/formula algebra, the `DataValidation`/`ConditionalFormattingRule`/`Table` families, defined names, images, worksheet protection, and the `stream.xlsx.WorkbookWriter`/`WorkbookReader` streaming pair
- consumer: `persistence/object#OBJECT_STORE` — the `AssetCodec` `xlsx` arm (`new Uint8Array(await wb.xlsx.writeBuffer())` → `ObjectStore.put`)
- rail: asset-codec / spreadsheet (the `xlsx` arm of the `persistence/object#OBJECT_STORE` `AssetCodec` union)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and worksheet
`Workbook` is the sole root and IO entry; `Worksheet` owns rows/columns/cells and the per-sheet feature surfaces, with `Xlsx`/`Csv` the per-format IO sub-services.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Workbook`            | class         | root model + IO entry (`xlsx`/`csv` sub-services, `addImage`, `definedNames`, `properties`, `views`) |
|  [02]   | `Worksheet`           | interface     | one sheet — rows/columns/cells, tables, validations, CF, protection, views, autoFilter |
|  [03]   | `Xlsx` / `Csv`        | interface     | per-format IO sub-service (`readFile`/`read`/`load`/`writeFile`/`writeBuffer`/`write`) |
|  [04]   | `WorkbookProperties`  | interface     | `date1904`, `filterPrivacy`, `defaultFont`, `calcProperties`     |
|  [05]   | `WorksheetProperties` | interface     | `tabColor`, `outlineLevelCol/Row`, `defaultRowHeight`, `defaultColWidth`, `showGridLines` |
|  [06]   | `WorkbookView`        | interface     | window position, active tab, visibility                          |
|  [07]   | `WorksheetView`       | union         | `WorksheetViewNormal \| WorksheetViewFrozen \| WorksheetViewSplit` — discriminated on `state`, shared shape in `WorksheetViewCommon` |
|  [08]   | `WorksheetState`      | type          | `'visible' \| 'hidden' \| 'veryHidden'`                          |
|  [09]   | `WorkbookModel` / `WorksheetModel` / `RowModel` / `CellModel` | interface | serialization shapes (the `.model` round-trip surface) |

[PUBLIC_TYPE_SCOPE]: row, column, cell, value, formula
`Row`/`Column`/`Cell` are 1-based get-or-create handles; `CellValue` is the tagged value union and `ValueType`/`FormulaType`/`ErrorValue` its discriminants.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------------------------------------ | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Row` (`extends Style`)                     | interface     | `values`/`getCell`/`eachCell`/`commit`/`height`/`hidden`/`outlineLevel`/`number` |
|  [02]   | `Column`                                    | interface     | `header`/`key`/`width`/`hidden`/`outlineLevel`/`eachCell`/`values`/`letter` |
|  [03]   | `Cell` (`extends Style, Address`)           | interface     | `value`/`text`/`formula`/`result`/`type`/`effectiveType`/`dataValidation`/`note`/`master`/`merge`/`name(s)` |
|  [04]   | `CellValue`                                 | union         | `null \| number \| string \| boolean \| Date \| CellErrorValue \| CellRichTextValue \| CellHyperlinkValue \| CellFormulaValue \| CellSharedFormulaValue \| undefined` |
|  [05]   | `RowValues`                                 | type          | `CellValue[] \| { [key: string]: CellValue } \| undefined \| null` — array (column-positional) or keyed (column-`key`) |
|  [06]   | `ValueType`                                 | enum          | `Null(0)`, `Merge(1)`, `Number(2)`, `String(3)`, `Date(4)`, `Hyperlink(5)`, `Formula(6)`, `SharedString(7)`, `RichText(8)`, `Boolean(9)`, `Error(10)` — the `cell.type` discriminant |
|  [07]   | `FormulaType`                               | enum          | `None(0)`, `Master(1)`, `Shared(2)` — master vs shared-formula axis |
|  [08]   | `ErrorValue`                                | enum          | `#N/A`/`#REF!`/`#NAME?`/`#DIV/0!`/`#NULL!`/`#VALUE!`/`#NUM!` — the `CellErrorValue.error` domain |
|  [09]   | `CellFormulaValue` / `CellSharedFormulaValue` / `CellRichTextValue` / `CellHyperlinkValue` / `CellErrorValue` | interface | the tagged `CellValue` payload shapes |

[PUBLIC_TYPE_SCOPE]: style algebra
`Style` is the six-slot bag — `numFmt`/`font`/`alignment`/`protection`/`border`/`fill` — spread onto every `Cell`/`Row`/`Column`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Style`                         | interface     | the six-slot style bag — `numFmt`/`font`/`alignment`/`protection`/`border`/`fill` (spread onto every `Cell`/`Row`/`Column`) |
|  [02]   | `Font`                          | interface     | `name`/`size`/`bold`/`italic`/`underline`/`strike`/`color`/`family`/`scheme` |
|  [03]   | `Alignment`                     | interface     | `horizontal`/`vertical`/`wrapText`/`textRotation`/`indent`/`readingOrder` (`ReadingOrder` enum) |
|  [04]   | `Fill`                          | union         | `FillPattern \| FillGradientAngle \| FillGradientPath` — `FillPatterns` names + `GradientStop[]` |
|  [05]   | `Border` / `Borders` / `BorderStyle` | interface/type | one edge `{ style, color }`; `Borders` = top/left/bottom/right/diagonal; `BorderStyle` = `'thin' \| 'dotted' \| 'medium' \| …` |
|  [06]   | `Color`                         | interface     | `{ argb?: string; theme?: number; tint?: number; indexed?: number }` |
|  [07]   | `Protection`                    | interface     | `{ locked; hidden }` (per-cell, effective only under sheet `protect`) |

[PUBLIC_TYPE_SCOPE]: validation, conditional formatting, tables, protection, defined names
`DataValidation` and `ConditionalFormattingRule` are single discriminated shapes that own their whole spaces by `type`, never a per-variant class; `Table`/`WorksheetProtection`/`DefinedNamesModel` complete the feature vocabulary.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `DataValidation`                | interface     | one parameterized rule — `type: 'list' \| 'whole' \| 'decimal' \| 'date' \| 'textLength' \| 'custom'`, `operator: DataValidationOperator`, `formulae`, prompt/error text; the whole validation space is this one shape, never a per-type class |
|  [02]   | `ConditionalFormattingRule`     | union         | `Expression \| CellIs \| Top10 \| AboveAverage \| ColorScale \| IconSet \| ContainsText \| TimePeriod \| DataBar` — one discriminated `type` union (`ConditionalFormattingBaseRule` shared) owns the entire CF space |
|  [03]   | `ConditionalFormattingOptions`  | interface     | `{ ref: string; rules: ConditionalFormattingRule[] }` — the single `addConditionalFormatting` payload |
|  [04]   | `Cvfo` + `CfvoTypes`/`IconSetTypes`/`CellIsOperators`/`ContainsTextOperators`/`TimePeriodTypes` | interface/type | the CF value-object + the bounded vocabularies the rule variants discriminate over |
|  [05]   | `Table` / `TableProperties` / `TableColumnProperties` / `TableStyleProperties` | interface | Excel table — `name`/`ref`/`headerRow`/`totalsRow`/`columns[]`; `TableStyleProperties.theme` = the `'TableStyleMedium2' \| …` enumerated theme axis |
|  [06]   | `WorksheetProtection`           | interface     | the 16-flag permission bag (`selectLockedCells`/`formatCells`/`insertRows`/`deleteColumns`/`sort`/`autoFilter`/`pivotTables`/…) + `spinCount` |
|  [07]   | `AutoFilter`                    | type          | `string \| { from: Location; to: Location }` — filter range |
|  [08]   | `DataValidationOperator`        | type          | `'between' \| 'notBetween' \| 'equal' \| 'greaterThan' \| …` |
|  [09]   | `Comment` / `CommentMargins` / `CommentProtection` / `CommentEditAs` / `RichText` | interface/type | threaded cell note payloads (`cell.note = string \| Comment`) |
|  [10]   | `DefinedNamesModel`             | interface     | serialized named-range map (behind `Workbook.definedNames`) |

[PUBLIC_TYPE_SCOPE]: page setup, images, streaming IO
Print/media/streaming shapes; `stream.xlsx.WorkbookWriter`/`WorkbookReader` are the bounded-memory `extends Workbook` variants.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `PageSetup`                                 | interface     | orientation/margins/`paperSize` (`PaperSize` const enum)/print area/fit-to-page/header-footer margins |
|  [02]   | `HeaderFooter`                              | interface     | odd/even/first-page header & footer strings |
|  [03]   | `Image` / `Anchor` (`IAnchor`) / `ImageRange` / `ImagePosition` / `ImageHyperlinkValue` | interface/class | media payload (`extension`, `base64?`/`filename?`/`buffer?`) + tl/br cell-anchor positioning |
|  [04]   | `stream.xlsx.WorkbookWriter`                | class         | streaming XLSX writer (`extends Workbook`) — row-at-a-time flush, `commit(): Promise<void>` |
|  [05]   | `stream.xlsx.WorkbookReader`                | class         | streaming XLSX reader (`extends Workbook`) — `read(): Promise<void>`, async-iterable worksheet/row events |
|  [06]   | `XlsxWriteOptions` (`extends stream.xlsx.WorkbookWriterOptions`) | interface | `stream?`/`filename?`/`useSharedStrings?`/`useStyles?`/`zip?` (`ArchiverZipOptions`) |
|  [07]   | `XlsxReadOptions` / `CsvReadOptions` / `CsvWriteOptions` | interface | per-format read/write knobs (CSV `parserOptions`/`formatterOptions`/`dateFormats`/`map`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Workbook construction and buffered IO — the sink-kind axis
Ingress/egress discriminate on where the bytes go (`readFile`/`read`/`load`, `writeFile`/`writeBuffer`/`write`), never a renamed operation; `writeBuffer()` is the `ObjectStore.put` feed.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `new Workbook()`                            | constructor    | empty workbook (creator/lastModifiedBy/created assignable) |
|  [02]   | `wb.xlsx.readFile(path)` / `.read(stream)` / `.load(buffer)` | read | XLSX ingress — one method per source kind (file/stream/`Buffer`) |
|  [03]   | `wb.xlsx.writeFile(path)` / `.writeBuffer()` / `.write(stream)` | write | XLSX egress — file / in-memory `Buffer` / pipe-to-`Writable`; `writeBuffer()` is the `ObjectStore.put` feed |
|  [04]   | `wb.csv.readFile(path)` / `.read(stream)` / `.writeFile(path)` / `.writeBuffer()` / `.write(stream)` | read/write | CSV ingress/egress mirror (prefer `papaparse` for pure CSV; `exceljs` CSV only when one codec must span both formats) |
|  [05]   | `wb.model` (get/set)                        | serialize      | full `WorkbookModel` round-trip for cross-process transfer |

[ENTRYPOINT_SCOPE]: Workbook sheet + media + names
Sheet lifecycle, workbook-level image embedding (`addImage → id`, placed per-sheet by range), and the `definedNames` named-range surface.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `wb.addWorksheet(name?, options?)`          | factory        | add + return a `Worksheet` (options carry `properties`/`pageSetup`/`views`/`headerFooter`/`state`) |
|  [02]   | `wb.getWorksheet(indexOrName?)`             | lookup         | `Worksheet \| undefined` by 1-based id or name           |
|  [03]   | `wb.removeWorksheet(indexOrName)` / `wb.eachSheet(cb)` | mutate/iterate | drop a sheet / fold every sheet             |
|  [04]   | `wb.addImage(img)` → id; `ws.addImage(id, range)` | media    | embed once at the workbook, place per-sheet by `ImageRange`/`ImagePosition` |
|  [05]   | `wb.definedNames` (`DefinedNames`)          | names          | `add(locStr, name)`/`getRanges(name)`/`forEach`/`remove` named-range surface |

[ENTRYPOINT_SCOPE]: Worksheet — row/column/cell + table/validation/CF/protection
The per-sheet write surface: schema columns, row add/insert/splice, cell access, merges, tables, one-entry CF/validation, and sheet protection.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `ws.columns = Column[]`                     | schema         | assign header/key/width columns once; `addRow(keyedObject)` then maps by `key` |
|  [02]   | `ws.addRow(data, style?)` / `ws.addRows(rows, style?)` | row add | append one/many from array (positional) or object (keyed); returns `Row`/`Row[]` |
|  [03]   | `ws.insertRow(pos, v, style?)` / `ws.insertRows(pos, vs, style?)` / `ws.duplicateRow(n, count, insert)` | row insert | positional insert + row duplication |
|  [04]   | `ws.spliceRows(start, count, …insert)` / `ws.spliceColumns(start, count, …insert)` | splice | array-style cut/insert of rows/columns (shifts adjust merges/keys) |
|  [05]   | `ws.getRow(i)` / `ws.getRows(start, len)` / `ws.eachRow(opt?, cb)` / `ws.getSheetValues()` | row access | get-or-create 1-based row(s); iterate (optionally including empty); dump all `RowValues` |
|  [06]   | `ws.getColumn(indexOrKey)` / `ws.getColumnKey(key)` | column access | by letter, 1-based number, or defined `key` |
|  [07]   | `ws.getCell(r, c?)` / `row.getCell(indexOrKey)` | cell access | get-or-create cell by address (`"B2"`) or `(row, col)`; set `.value`/`.dataValidation`/`.note`/`.style` |
|  [08]   | `ws.mergeCells(range)` / `ws.unMergeCells(range)` | layout   | merge/unmerge a rectangle (top-left becomes `master`) |
|  [09]   | `ws.addTable(TableProperties)` → `Table` / `ws.getTable(name)` / `ws.getTables()` / `ws.removeTable(name)` | table | Excel table with themed style + typed columns |
|  [10]   | `ws.addConditionalFormatting(ConditionalFormattingOptions)` / `ws.removeConditionalFormatting(ref?)` | CF | one parameterized rule-set entry over the `ConditionalFormattingRule` union |
|  [11]   | `ws.protect(password, WorksheetProtection)` → `Promise<void>` / `ws.unprotect()` | protection | sheet lock honoring per-cell `Protection.locked` |
|  [12]   | `ws.autoFilter = AutoFilter` / `ws.views = WorksheetView[]` / `ws.state` / `ws.pageSetup` / `ws.headerFooter` | layout | filter range, frozen/split panes, visibility, print setup |

[ENTRYPOINT_SCOPE]: streaming writer/reader — the large-export path
The bounded-memory path: `row.commit()` per row, `ws.commit()` per sheet, `writer.commit()` for the archive; skipping a commit silently drops rows.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `new stream.xlsx.WorkbookWriter({ stream? \| filename?, useSharedStrings?, useStyles?, zip? })` | streaming write | bounded-memory writer over a `Writable` or file |
|  [02]   | `ws = writer.addWorksheet(name)`; `row = ws.addRow(data)`; `row.commit()` | streaming row | flush a row and release it from memory |
|  [03]   | `ws.commit()` then `writer.commit(): Promise<void>` | streaming finalize | seal each sheet, then the archive — omitting either silently drops rows |
|  [04]   | `new stream.xlsx.WorkbookReader(input, options)`; `await reader.read()` / `for await (const ws of reader)` | streaming read | async-iterable worksheet/row events for large ingest |

## [04]-[IMPLEMENTATION_LAW]

[EXCEL_TOPOLOGY]:
- namespace `exceljs`; `Workbook` is the sole root; `xlsx`/`csv` are per-format IO sub-services (`wb.xlsx` / `wb.csv`); `stream.xlsx.WorkbookWriter`/`WorkbookReader` are the bounded-memory variants.
- row/column/cell indices are 1-based throughout; `getRow`/`getColumn`/`getCell` are get-or-create.
- the write/read surface is a sink/source-kind axis, not a method family: `readFile`/`read`/`load` discriminate file vs `Stream` vs `Buffer`; `writeFile`/`writeBuffer`/`write` discriminate file vs `Buffer` vs `Writable`. Pick by where the bytes go, never by a renamed operation.
- `DataValidation` and `ConditionalFormattingRule` are single discriminated shapes — the entire validation and CF spaces are one `type`-keyed union each, driven by data (`formulae`/`rules`), never a per-variant class. A new rule kind is a union arm, not a new method.
- streaming law: `WorkbookWriter` must `row.commit()` per row (or accept whole-sheet buffering), then `ws.commit()` per sheet, then `writer.commit()` for the archive; skipping a commit silently drops rows.
- images embed once at the workbook (`wb.addImage → id`) and place per-sheet (`ws.addImage(id, range)`); `CellFormulaValue.result` holds the last file-calculated value and is not recomputed on write.

[LOCAL_ADMISSION]:
- Use `xlsx.writeBuffer()` (→ `Uint8Array`) for the codec fan-out and S3 upload; `writeFile()` only for a genuine disk artifact. Never return a raw `Buffer` past the codec seam — normalize to `Uint8Array`.
- Use `stream.xlsx.WorkbookWriter` behind `Effect.acquireRelease` for exports whose row count is unbounded; the finalizer owns `commit()`. Never call `writeBuffer()` on a multi-hundred-MB export — it materializes the whole archive.
- Guard formula injection: a leading `=`/`+`/`-`/`@` in a user-supplied string cell is an Excel formula on open. Neutralize it at the codec boundary (prefix guard) exactly as the CSV arm sets `papaparse`'s `escapeFormulae: true`; a spreadsheet is an execution surface, not inert data.
- Set `ws.columns = [{ header, key }]` before `addRow(keyedObject)` so keyed rows map deterministically; a keyed `addRow` with no column `key` silently no-ops that field.
- `useSharedStrings: true` shrinks string-heavy exports; `useStyles: true` is required for streamed styled cells — both default `false` on the streaming writer.

[STACKING]:
- Effect boundary: `exceljs` is a `Promise`/callback library lifted at one edge — `Effect.tryPromise({ try: () => wb.xlsx.writeBuffer(), catch: (cause) => new AssetTransferFault({ format: "xlsx", stage: "encode", cause }) })` is the buffered encode, selected by `Match.value(format).pipe(Match.when("csv", …), Match.when("xlsx", …), Match.when("pdf", …), Match.when("archive", …), Match.exhaustive)` so `exceljs` is the `"xlsx"` arm of a total dispatch, never a standalone call; `writeBuffer()` returns a Node `Buffer` normalized to `Uint8Array` (`new Uint8Array(await wb.xlsx.writeBuffer())`) for the wire.
- `persistence/object#OBJECT_STORE` (`ObjectStore`): the `AssetCodec` fan-out is `encode: ObjectStore["encode"] = (format) => (rows) => …`; the `"xlsx"` `Uint8Array` flows straight into `ObjectStore.put`, which content-addresses it (`ObjectKey`, `XxHash128` seed-zero, the `interchange#CONTENT_KEY_PARITY` regime) into the `@effect-aws/client-s3` `S3Service`. `exceljs` never touches S3 — it produces bytes; `ObjectStore` owns the sink.
- `@effect/platform-node` `NodeStream`/`NodeSink` (`.api/effect-platform-node.md`): for exports too large to buffer, `NodeStream.fromWritable(() => new stream.xlsx.WorkbookWriter({ stream: passthrough }))` bridges the writer's `Writable` into an Effect `Sink`, and `NodeStream.fromReadable` bridges `WorkbookReader`/the passthrough back into a `Stream<Uint8Array>` for S3 multipart upload — the same bridge the store uses for `sharp` image streams. `Effect.acquireRelease(openWriter, (w) => Effect.promise(() => w.commit()))` guarantees the finalizing `commit()` on every exit path.
- Ingest mirror: `Schema.decodeUnknown(RowSchema)` over each `RowValues` from `WorkbookReader`/`getSheetValues()` parses untyped cells into a branded domain record at the boundary (parse, never trust), folding malformed rows onto the same `AssetTransferFault` rail.
- Sibling `AssetCodec` arms: `exceljs` (`xlsx`) sits beside `papaparse` (`csv`, `.api/papaparse.md` `unparse`), `jspdf` (`pdf`, `.api/jspdf.md` `output("arraybuffer")`), and `jszip` (`archive`, `.api/jszip.md` `generateAsync`/`generateNodeStream`) under the one `Schema.Literal("csv","xlsx","pdf","archive")` discriminant folded by `Match.exhaustive`, all sharing the `AssetTransferFault { stage: "encode" }` rail and the `ObjectStore` sink; `sharp` (`.api/sharp.md`) is the in-pipeline image transform on the same `put` (failing onto `ObjectFault`, not `AssetTransferFault`), not a fifth export literal. A bundled multi-sheet export composes `exceljs` output into a `jszip` archive entry (`file(path, bytes)`) for one streamed `ObjectStore.put`.

[RAIL_LAW]:
- Package: `exceljs`
- Owns: XLSX/CSV read/write, the workbook/worksheet/row/column/cell model, the style/value/formula algebra, data validations, the conditional-formatting rule union, Excel tables, defined names, images, worksheet protection, and buffered + streaming IO — as the `"xlsx"` arm of the `persistence/object#OBJECT_STORE` `AssetCodec`
- Accept: `Buffer`/`Uint8Array`/file path/Node `Stream` for read and write; row data as positional array or column-`key` object; one `DataValidation`/`ConditionalFormattingOptions`/`TableProperties`/`WorksheetProtection` payload per feature
- Reject: a bare `Promise` past the `AssetCodec` seam (must be `Effect.tryPromise` under the `format` `Match`), hand-rolled XLSX zip/XML assembly, a direct `fast-csv`/`archiver` call when this package is admitted, a per-rule-type validation/CF class where the discriminated union owns the space, and an unescaped user string cell (formula-injection surface)
