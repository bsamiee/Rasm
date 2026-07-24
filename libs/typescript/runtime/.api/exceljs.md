# [TS_RUNTIME_API_EXCELJS]

`exceljs` owns the `.xlsx` workbook document model — worksheets, the row/column/cell grid, the style/value/table/conditional-format/data-validation vocabularies, defined names, page setup, and embedded images — behind two IO surfaces: an in-memory `Workbook` and a constant-memory `stream.xlsx` writer/reader pair. `work/report` composes it as the sole `.xlsx` egress engine behind one `renderWorkbook` effect boundary, its Promise-based mutating API never awaited in domain code.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `exceljs`
- package: `exceljs` (MIT)
- module: CJS default export (`import ExcelJS from "exceljs"`); `Workbook`, `stream.xlsx.WorkbookWriter`/`WorkbookReader`, and the model interfaces are the surface
- runtime: node/bun — the streaming writer/reader, `archiver`, and `readable-stream` bind node, so `work/report` runs exceljs on its node egress lane, never a browser bundle
- rail: the `.xlsx` workbook-model and egress owner for `work/report`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the workbook/worksheet model; `Workbook` owns metadata + IO facades, `Worksheet` owns the grid, tables, and formatting, and style rides each cell (`Row extends Style`, `Cell extends Style, Address`) with no parallel style map.

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]      | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------- | :----------------- | :----------------------------- |
|  [01]   | `Workbook`                                            | document root      | one workbook per job           |
|  [02]   | `Worksheet`                                           | sheet grid         | grid and table owner           |
|  [03]   | `Row` / `Column` / `Cell`                             | grid cell          | style-bearing grid primitives  |
|  [04]   | `Table` / `TableProperties` / `TableColumnProperties` | structured table   | Excel Table with totals        |
|  [05]   | `DefinedNames` / `DefinedNamesRanges`                 | named range        | cross-sheet formula names      |
|  [06]   | `WorkbookModel` / `WorksheetModel` / `CellModel`      | serializable model | projection for content hashing |

[PUBLIC_TYPE_SCOPE]: the cell value and style vocabularies that parameterize a report as data.

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `CellValue`                                                            | value union   | discriminated cell payload         |
|  [02]   | `Style`                                                                | style record  | per cell/row/column styling        |
|  [03]   | `Fill`                                                                 | fill union    | pattern or gradient fill           |
|  [04]   | `ConditionalFormattingRule`                                            | rule union    | conditional-format space as data   |
|  [05]   | `DataValidation`                                                       | validation    | dropdown and range guards          |
|  [06]   | `PageSetup` / `HeaderFooter` / `WorksheetView` / `WorksheetProtection` | layout policy | print/freeze/split/protect records |
|  [07]   | `Image` / `Anchor` / `ImageRange`                                      | media         | embedded image placement           |

- `CellValue` arms: `number` `string` `bool` `Date` `richtext` `hyperlink` `formula` `error` `null`, tagged by `ValueType`/`FormulaType`/`ErrorValue`.
- `Style` fields: `Font` `Fill` `Border` `Alignment` `Protection` `numFmt`.
- `Fill` arms: `FillPattern` `FillGradientAngle` `FillGradientPath`.
- `ConditionalFormattingRule` arms: `cellIs` `colorScale` `iconSet` `dataBar` `top10` `aboveAverage` `containsText` `timePeriod` `expression`.
- `DataValidation` arms: `list` `whole` `decimal` `date` `textLength` `custom`.

[PUBLIC_TYPE_SCOPE]: the constant-memory streaming options; writer options carry `stream`/`filename`, `useSharedStrings`, `useStyles`, `zip`, and reader options gate `worksheets`/`sharedStrings`/`hyperlinks`/`styles`/`entries` per part as `emit`|`cache`|`ignore`.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `stream.xlsx.WorkbookWriterOptions`                              | writer config | route to a `Stream` sink or `FileSystem`  |
|  [02]   | `stream.xlsx.WorkbookStreamReaderOptions`                        | reader config | per-part cache/emit for huge input        |
|  [03]   | `XlsxReadOptions` / `XlsxWriteOptions` / `JSZipGeneratorOptions` | IO options    | `XlsxWriteOptions` extends writer options |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `.xlsx` in-memory IO — every terminal an instance `Promise` on `workbook.xlsx`, wrapped in `Effect.tryPromise` onto the report-error rail.

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                |
| :-----: | :-------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `writeBuffer(options?) -> Promise<Buffer>`    | instance | preferred egress terminal   |
|  [02]   | `write(stream, options?) -> Promise<void>`    | instance | pipe to object-store upload |
|  [03]   | `load(buffer, options?) -> Promise<Workbook>` | instance | reopen stored xlsx to amend |
|  [04]   | `readFile(path)` / `read(stream)`             | instance | template ingress            |

- `workbook.xlsx.writeFile`: reaches `node:fs` directly and bypasses the platform rail; route egress through `writeBuffer` and the platform `FileSystem`.
- `workbook.xlsx.read(stream)`: stays off `node:fs`, unlike `readFile`.

[ENTRYPOINT_SCOPE]: constant-memory streaming, the durable-report default — `WorkbookWriter` commits per worksheet and row under `Effect.acquireRelease` (construct = acquire, `commit()` = release), `WorkbookReader` async-iterates the symmetric huge-input read.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `new stream.xlsx.WorkbookWriter(options)`        | ctor     | bounded-memory writer, acquire      |
|  [02]   | `writer.addWorksheet(name).addRow(row).commit()` | instance | row sink; each row leaves memory    |
|  [03]   | `writer.commit() -> Promise<void>`               | instance | release; flush strings, styles, zip |
|  [04]   | `new stream.xlsx.WorkbookReader(input, options)` | ctor     | async-iterate a huge xlsx           |

[ENTRYPOINT_SCOPE]: worksheet authoring — the `worksheet.*` grid a template drives from a spec; the `workbook.addWorksheet`/`addImage` assembler builds at the workbook level, `columns` declares header/key/width, and `addRow` maps an object by column key.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `columns = [{ header, key, width, style }]`                         | property | column and width contract           |
|  [02]   | `addRow(data)` / `addRows` / `insertRow` / `duplicateRow`           | instance | keyed or positional row sink        |
|  [03]   | `getCell(ref)` / `mergeCells(range)` / `getColumn(key)`             | instance | cell and merge access               |
|  [04]   | `addTable(props) -> Table` / `getTable` / `removeTable`             | instance | Excel Table with totals             |
|  [05]   | `addConditionalFormatting(cf)` / `cell.dataValidation = v`          | instance | rules and validations as data       |
|  [06]   | `protect(password, options)` / `addImage(id, range)` / `autoFilter` | instance | sheet decoration                    |
|  [07]   | `workbook.addWorksheet(name, options)` / `addImage(img) -> number`  | instance | workbook assembler; shared media id |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- exceljs is the engine and `effect` is the boundary: every IO call returns a mutating `Promise`, so `work/report` owns one `renderWorkbook(spec, rows) -> Effect<Buffer, ReportRenderError>` wrapping each terminal in `Effect.tryPromise`, and domain code never awaits exceljs.
- A report is a spec, not a builder: column defs, style rows, table props, conditional-format rules, and data validations are data; `renderWorkbook` folds a `ReportSpec` (columns + styles + a `(row) -> CellValue[]` projection) over a row source, so a new report is a spec value and the `Style`/`Table`/`ConditionalFormattingRule`/`DataValidation` unions are the parameterization vocabulary.
- Streaming is the default for unbounded rows: `stream.xlsx.WorkbookWriter` under `Effect.acquireRelease` drains a row `Stream` through `addRow(row).commit()` at constant memory; the in-memory `Workbook` serves only small, style-rich, or amend-load reports.
- Output is content-addressed: the produced `Buffer` hashes through the one `core/value/identity` `XxHash128` seed-zero mint for the artifact content key, and an equal `idempotencyKey` reuses the stored artifact.

[STACKING]:
- `effect` (`.api/effect.md`): `Effect.tryPromise` wraps each IO terminal onto a `Data.TaggedError` rail, `Effect.acquireRelease` scopes the streaming writer, `Stream.runForEach` drives row egress, and `Match` dispatches the `CellValue`/`ConditionalFormattingRule` unions in the projection.
- `@effect/platform` (`.api/effect-platform.md`): `writeBuffer()` output writes through the `FileSystem` Tag or an object-store upload, and the row source is a `@effect/sql` cursor or a `data/read/query` `Stream`.
- `@effect/workflow` (`runtime/.api/effect-workflow.md`): a report is one `Activity.make({ name, execute: renderWorkbook })` — idempotent on the report request, retryable under `interruptRetryPolicy`, resumable past a crash without re-rendering.
- `jspdf` + `papaparse` (`runtime/.api/jspdf.md`, `runtime/.api/papaparse.md`): the output-format peers over the same decoded rows, one policy row selecting PDF, XLSX, or CSV; `papaparse` owns standalone CSV, so `exceljs.csv` reserves for re-projecting an existing `Worksheet`.
- `jszip` + `nodemailer` (`runtime/.api/jszip.md`, `runtime/.api/nodemailer.md`): the shared `deliver` channels — the `writeBuffer()` `Buffer` feeds `jszip` bundling and `nodemailer` attachment.
- `work/report` (within-library): folds every report through one `renderWorkbook` over a `ReportSpec` with the streaming writer as the `effect/Stream` sink, so a new report is a spec row and never a bespoke builder.

[RAIL_LAW]:
- Package: `exceljs`
- Owns: the `.xlsx` workbook/worksheet/row/column/cell model, the style/value/table/conditional-format/data-validation vocabularies, defined names, page setup, embedded images, the in-memory `Workbook` facade, and the constant-memory `stream.xlsx.WorkbookWriter`/`WorkbookReader`.
- Accept: exceljs behind a `renderWorkbook(spec, rows)` effect boundary, the streaming writer as an `effect/Stream` sink under `acquireRelease`, reports expressed as `ReportSpec` data, `writeBuffer` routed through the platform `FileSystem`, the `Buffer` as the shared deliver artifact, XLSX as one arm of the output-format policy.
- Reject: a bare `await` on an exceljs `Promise`, a full in-memory workbook for an unbounded report, a per-report imperative builder or duplicated style code, `workbook.xlsx.writeFile` to `node:fs` from a domain effect, `exceljs.csv` where `papaparse` owns CSV, exceljs on a browser lane.
