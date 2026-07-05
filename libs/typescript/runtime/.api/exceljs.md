# [exceljs] — the `.xlsx` workbook model + constant-memory streaming writer behind `work/report` durable jobs

`exceljs` is the spreadsheet owner `work/report` composes for `.xlsx` egress: a full workbook model (worksheets, rows, columns, cells, styles, tables, conditional formatting, data validations, images, defined names, page setup) plus two IO surfaces — an in-memory `Workbook` (`readFile`/`load`/`writeBuffer`) and a CONSTANT-MEMORY streaming pair (`stream.xlsx.WorkbookWriter` commits worksheet-by-worksheet without holding the document in RAM; `stream.xlsx.WorkbookReader` async-iterates rows out of a huge input). Its API is Promise-based and mutation-heavy, so the boundary law is fixed: `work/report` wraps every terminal (`writeBuffer`/`writeFile`/`commit`) in `Effect.tryPromise` onto a `Data.TaggedError` rail, drives the streaming writer as an `effect/Stream` sink under `Effect.acquireRelease`, and treats a report as a PARAMETERIZED spec (column defs + style rows + a row projection) folded by one `renderWorkbook`, never a per-report imperative builder. The produced `Buffer` is the shared deliver artifact `jszip` bundles and `nodemailer` attaches, and `.xlsx` is one arm of the report output-format policy peered with `jspdf` (PDF) and `papaparse` (CSV) over the same decoded rows — one format row, never a forked pipeline per format.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `exceljs`
- package: `exceljs`
- version: `4.4.0`
- license: `MIT`
- deps: `archiver` + `jszip` (OOXML zip), `saxes` + `unzipper` (XML parse), `fast-csv` (the `.csv` surface), `dayjs`, `readable-stream`, `tmp`, `uuid`
- runtime: node/bun — the streaming `WorkbookWriter`/`WorkbookReader`, `archiver`, and `readable-stream` are node-only; a prebuilt browser bundle (`dist/exceljs.min.js`) exists but the `work/report` durable lane is a node runner, so treat exceljs as node-lane egress
- effect-peer: none — exceljs is a plain library; `effect`/`@effect/platform` (`.api/effect.md`, `.api/effect-platform.md`) wrap it at the `work/report` boundary
- catalog-verdict: KEEP for `.xlsx` — the one workbook-model owner; `.csv` DEFERS to `papaparse` (`runtime/.api/papaparse.md`), the admitted CSV owner, so `exceljs.csv` (fast-csv) is reserved for re-projecting an existing `Worksheet`
- entry: `import ExcelJS from "exceljs"` (CJS default) or the `exceljs` types; `Workbook`, `stream.xlsx.WorkbookWriter`/`WorkbookReader`, and the model interfaces are the surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the workbook/worksheet document model
- rail: document
- `Workbook` owns metadata, worksheets, defined names, and the two IO facades; `Worksheet` owns the row/column/cell grid plus tables and formatting. `work/report` never subclasses these — it drives them behind a spec.

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :--------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `Workbook`                                            | document root   | `work/report` — one workbook per report job; carries `.xlsx`/`.csv` facades and metadata |
|  [02]   | `Worksheet`                                           | sheet grid      | one sheet per report section; the row/column/cell/table owner |
|  [03]   | `Row` / `Column` / `Cell`                            | grid cell       | `Row extends Style`, `Cell extends Style, Address` — style rides the cell, no parallel style map |
|  [04]   | `Table` / `TableProperties` / `TableColumnProperties` | structured table | native Excel Table with header/totals rows; `totalsRowFunction` a policy value per column |
|  [05]   | `DefinedNames` / `DefinedNamesRanges`                | named range     | cross-sheet named ranges for formula-driven report templates  |
|  [06]   | `WorkbookModel` / `WorksheetModel` / `CellModel`     | serializable model | the plain-object projections behind `workbook.model` for content-key hashing |

[PUBLIC_TYPE_SCOPE]: the cell value + style vocabularies — parameterization DATA
- rail: document/policy
- Every report variation is one of these closed vocabularies, not a new code path: `CellValue` is the value union, `Style` composes `Font`/`Fill`/`Border`/`Alignment`, and the formatting/validation unions carry the whole rule space. A report template names style rows and rule values; it never branches per report.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :---------------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `CellValue` = number\|string\|bool\|Date\|richtext\|hyperlink\|formula\|error\|null | value union | the discriminated cell payload; `ValueType`/`FormulaType`/`ErrorValue` enums tag it |
|  [02]   | `Style` (`Font`/`Fill`/`Border`/`Alignment`/`Protection`/`numFmt`) | style record | one style record per cell/row/column; `numFmt` the format string |
|  [03]   | `Fill` = `FillPattern` \| `FillGradientAngle` \| `FillGradientPath` | fill union   | pattern vs gradient as a value, not a method |
|  [04]   | `ConditionalFormattingRule` (9 arms: `CellIs`/`ColorScale`/`IconSet`/`DataBar`/`Top10`/`AboveAverage`/`ContainsText`/`TimePeriod`/`Expression`) | rule union | the whole conditional-format space as data; a report picks arms |
|  [05]   | `DataValidation` (`list`/`whole`/`decimal`/`date`/`textLength`/`custom`) | validation | dropdown/range guards; the operator is a policy field |
|  [06]   | `PageSetup` / `HeaderFooter` / `WorksheetView` / `WorksheetProtection` | layout policy | print/freeze/split/protect settings as declarative records |
|  [07]   | `Image` / `Anchor` / `ImageRange`                           | media           | logo/chart embedding; `Workbook.addImage` returns the shared id |

[PUBLIC_TYPE_SCOPE]: the streaming IO options
- rail: boundaries/streaming
- The constant-memory writer/reader options; `WorkbookWriter` is the default for durable reports whose row count is unbounded.

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :------------------------------------------------------------ | :-------------- | :------------------------------------------------------------- |
|  [01]   | `stream.xlsx.WorkbookWriterOptions` (`stream`\|`filename`, `useSharedStrings`, `useStyles`, `zip`) | writer config | route output to an `effect/Stream` sink or a `FileSystem` path; `useStyles` trades memory for formatting |
|  [02]   | `stream.xlsx.WorkbookStreamReaderOptions` (`worksheets`/`sharedStrings`/`hyperlinks`/`styles`/`entries`: `emit`\|`cache`\|`ignore`) | reader config | per-part cache/emit policy for the async-iterated read of a huge input |
|  [03]   | `XlsxReadOptions` / `XlsxWriteOptions` / `JSZipGeneratorOptions` | IO options     | in-memory read/write tuning; `XlsxWriteOptions` extends the writer options |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `.xlsx` in-memory IO — the effectful boundary
- rail: boundaries
- Every IO terminal is a `Promise`; `work/report` wraps it in `Effect.tryPromise` → `ReportRenderError`. Prefer `writeBuffer()` and hand the `Buffer` to the platform `FileSystem`/object storage over `writeFile` (which reaches `node:fs` directly and bypasses the platform rail).

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `workbook.xlsx.writeBuffer(options?): Promise<Buffer>`            | egress         | `work/report` — the preferred terminal; the `Buffer` is the shared deliver artifact |
|  [02]   | `workbook.xlsx.write(stream, options?): Promise<void>`           | egress         | pipe directly to an object-store upload stream               |
|  [03]   | `workbook.xlsx.load(buffer, options?): Promise<Workbook>`        | ingress        | re-open a stored `.xlsx` for amend-and-re-emit report jobs   |
|  [04]   | `workbook.xlsx.readFile(path)` / `.read(stream)`                 | ingress        | template load; `read(stream)` stays off the `node:fs` path   |

[ENTRYPOINT_SCOPE]: constant-memory streaming — the durable-report default
- rail: boundaries/streaming
- `WorkbookWriter` commits per worksheet/row so an unbounded report never materializes in RAM. It is a scoped resource: acquire the writer, drain a row `Stream` through `addRow(...).commit()`, release with `workbook.commit()`. `WorkbookReader` is async-iterable for the symmetric huge-input read.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------ | :------------- | :-------------------------------------------------------- |
|  [01]   | `new stream.xlsx.WorkbookWriter({ stream\|filename, useSharedStrings, useStyles, zip })` | streaming write | the bounded-memory writer; wrap construction+`commit` in `Effect.acquireRelease` |
|  [02]   | `writer.addWorksheet(name)` → `worksheet.addRow(row).commit()`            | streaming row  | the `effect/Stream.runForEach` sink; each committed row leaves memory |
|  [03]   | `writer.commit(): Promise<void>`                                          | finalize       | the release step; flushes shared strings, styles, and the zip central directory |
|  [04]   | `new stream.xlsx.WorkbookReader(input, options)` — `for await (ws of reader)` → `for await (row of ws)` | streaming read | async-iterate a huge `.xlsx`; `WorksheetReader` yields `Row`s without a full parse |

[ENTRYPOINT_SCOPE]: worksheet authoring — the parameterized grid surface
- rail: document
- The row/column/cell/table/format calls a report template drives from a spec. `columns` is the header/key/width declaration; `addRow` accepts an object keyed by those column keys.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------ | :------------- | :-------------------------------------------------------- |
|  [01]   | `worksheet.columns = [{ header, key, width, style }]`                     | schema         | the column contract; `addRow` maps object fields by `key` |
|  [02]   | `worksheet.addRow(data)` / `.addRows(rows)` / `.insertRow` / `.duplicateRow` | rows        | the row projection sink; `data` is a keyed object or positional array |
|  [03]   | `worksheet.getCell(ref)` / `.mergeCells(range)` / `.getColumn(key)`       | cell access    | targeted cell/merge edits; `mergeCells` carries the overload family for range forms |
|  [04]   | `worksheet.addTable(props): Table` / `.getTable` / `.removeTable`         | table          | native Excel Table with typed columns + totals functions   |
|  [05]   | `worksheet.addConditionalFormatting(cf)` / `.getCell(x).dataValidation = v` | formatting   | the rule/validation vocabularies applied as data           |
|  [06]   | `worksheet.protect(password, options)` / `.addImage(id, range)` / `.autoFilter` | decorate  | sheet protection, embedded media, filter ranges            |
|  [07]   | `workbook.addWorksheet(name, options)` / `.addImage(img): number`        | assemble       | the workbook builder; `addImage` returns the shared media id |

## [04]-[IMPLEMENTATION_LAW]

[REPORT_TOPOLOGY]:
- exceljs is the engine, `effect` is the boundary: every IO call returns a `Promise` and mutates the workbook, so `work/report` owns one `renderWorkbook(spec, rows): Effect<Buffer, ReportRenderError>` that wraps the terminal in `Effect.tryPromise`. Domain code never `await`s exceljs directly.
- a report is a spec, not a builder: the column defs, style rows, table props, conditional-format rules, and data validations are DATA; `renderWorkbook` folds a `ReportSpec` (columns + styles + a `(row) => CellValue[]` projection) over a row source. A new report is a spec value, never a new function — the `Style`/`Table`/`ConditionalFormattingRule`/`DataValidation` unions ARE the parameterization vocabulary.
- streaming is the default for unbounded rows: `stream.xlsx.WorkbookWriter` under `Effect.acquireRelease` (construct = acquire, `workbook.commit()` = release) drains a row `effect/Stream` through `worksheet.addRow(row).commit()`, so a million-row report holds constant memory. The in-memory `Workbook` is reserved for small, style-rich, or amend-load reports.
- output is content-addressed: the produced `Buffer` hashes through the one `core/value/identity` `XxHash128` seed-zero mint for the report artifact content key; `work/report` never mints its own id, and a re-render with an equal `idempotencyKey` reuses the stored artifact.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `Effect.tryPromise` wraps IO onto a `Data.TaggedError` rail; `Effect.acquireRelease` scopes the streaming writer; `Stream.runForEach` drives row egress; `Match` dispatches the `CellValue`/`ConditionalFormattingRule` unions in the projection.
- `@effect/platform` (`.api/effect-platform.md`): produce a `Buffer` via `writeBuffer()` and write it through the `FileSystem` Tag or an object-store upload — never `workbook.xlsx.writeFile`, which bypasses the runtime-portable platform rail. The row source is a `@effect/sql` cursor or a `data/read/query` stream on `effect/Stream`.
- `@effect/workflow` (`runtime/.api/effect-workflow.md`): a report is one `Activity.make({ name, execute: renderWorkbook })` — idempotent (keyed on the report request), retryable under `interruptRetryPolicy`, resumable past a crash without re-rendering. The `work/report` durable job is that activity inside a `deliver` workflow.
- `jspdf` + `papaparse` (`runtime/.api/jspdf.md`, `runtime/.api/papaparse.md`): the report output-format peers — the same decoded rows render to PDF (`jspdf`), XLSX (`exceljs`), or CSV (`papaparse`) selected by one output-format policy row, never a forked pipeline per format. XLSX is exceljs's arm; `.csv` (fast-csv) is REJECTED for standalone tabular CSV since `papaparse` is the admitted CSV owner — use `exceljs.csv` only to re-project an existing `Worksheet` model.
- `jszip` + `nodemailer` (`runtime/.api/jszip.md`, `runtime/.api/nodemailer.md`): the shared `deliver` egress channels that transport the format-arm bytes — the `writeBuffer()` `Buffer` feeds `jszip` for multi-artifact bundling and `nodemailer` as a mail attachment; exceljs produces the spreadsheet bytes, the container and mail owners carry them.

[LOCAL_ADMISSION]:
- Wrap every exceljs terminal in `Effect.tryPromise` onto a tagged report-error rail; never `await workbook.xlsx.writeBuffer()` in domain code.
- Use `stream.xlsx.WorkbookWriter` under `Effect.acquireRelease` for unbounded reports; never build a full `Workbook` in memory for a large row set.
- Drive the grid from a `ReportSpec` folded by one `renderWorkbook`; never branch a per-report imperative builder or duplicate style construction.
- Emit a `Buffer` and write it through `@effect/platform` `FileSystem`/object storage; never `writeFile` to `node:fs` from a domain effect.
- Reach for `papaparse` for CSV; use `exceljs.csv` only against an existing `Worksheet`.

[RAIL_LAW]:
- Package: `exceljs`
- Owns: the `.xlsx` workbook/worksheet/row/column/cell document model, the style/value/table/conditional-format/data-validation vocabularies, defined names, page setup, embedded images, the in-memory `Workbook` IO facade, and the constant-memory `stream.xlsx.WorkbookWriter`/`WorkbookReader`
- Accept: exceljs behind a `renderWorkbook(spec, rows)` effect boundary, the streaming writer as an `effect/Stream` sink under `acquireRelease`, reports expressed as `ReportSpec` data, `writeBuffer` output routed through the platform `FileSystem`, the `Buffer` as the shared deliver artifact for `jszip`/`nodemailer`, XLSX as one arm of the report output-format policy peered with `jspdf`/`papaparse`
- Reject: a bare `await` on an exceljs `Promise`, a full in-memory workbook for an unbounded report, a per-report imperative builder or duplicated style code, `workbook.xlsx.writeFile` to `node:fs` from a domain effect, `exceljs.csv` where `papaparse` owns the CSV concern, exceljs on a browser lane
