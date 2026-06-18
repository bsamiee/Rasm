# [API_CATALOGUE] write-excel-file

`write-excel-file` generates `.xlsx` workbooks from JavaScript data without an Office runtime. The default export `writeXlsxFile` is overloaded: a raw `SheetData` (`Cell[][]`) plus `SheetOptions`, an array of plain objects plus `SheetOptionsForObjects` carrying a `columns` schema, or an array of `Sheet` for multi-sheet output. Cells are either a bare `Value` (`String | Date | Number | Boolean`), `null`/`undefined`, or a `CellObject` carrying a typed `value`, a `type` constructor or `'Formula'`, an output `format`, and the full `CellStyleProperties` (fonts, fills, borders, alignment, spans, rotation). `getSheetData` converts an object array plus `Column[]` into `SheetData`. The `node` entry returns a `ReturnType` with `toBuffer`, `toStream`, and `toFile`; environment is selected through the `/node`, `/browser`, and `/universal` subpath exports. Sheet-level features cover sticky rows/columns, conditional formatting, and embedded images, with a low-level `Feature` hook for raw XML transformation.

---

## [1]-[PACKAGE_SURFACE]

The entry contents are identical across `/node`, `/browser`, and `/universal`; the `FileContent` and `ReturnType` types differ per environment.

```ts
// write-excel-file — node entry
export type Image = ImageType<FileContent>

// Single sheet (raw rows)
declare function writeXlsxFile(sheetData: SheetData, sheetOptions?: SheetOptions<FileContent>, options?: Options<FileContent>): ReturnType
// Single sheet (objects + columns)
declare function writeXlsxFile<Object>(objects: Object[], sheetOptions: SheetOptionsForObjects<Object, FileContent>, options?: Options<FileContent>): ReturnType
// Multiple sheets
declare function writeXlsxFile(sheets: Sheet<FileContent>[], options?: Options<FileContent>): ReturnType
export default writeXlsxFile

export function getSheetData<Object>(objects: Object[], columns: Column<Object>[]): SheetData

export type { Sheet, SheetOptions, Value, Cell, CellObject, Row, SheetData, Options, Column, Feature }
```

The `node` environment resolves `FileContent = Stream | Buffer | Blob` and the result `ReturnType` exposes `toBuffer`/`toStream`/`toFile`; browser/universal resolve `FileContent` and the result to environment-native equivalents.

---

## [2]-[CELL_TYPES]

A cell is a styled `CellObject`, a bare scalar `Value`, or empty. `Value` constructors double as `type` discriminators; `'Formula'` marks a formula string. `Row` and `SheetData` are the raw matrix form.

```ts
// write-excel-file — SheetData.d.ts
type Value = String | Date | Number | Boolean
type CellType<Value> = Constructor<Value> | 'Formula' // StringConstructor | DateConstructor | NumberConstructor | BooleanConstructor | 'Formula'

interface CellProperties extends CellStyleProperties { format?: string }
interface CellObject extends CellProperties {
  value?: Value
  type?: CellType<Value>
}
type Cell = CellObject | Value | null | undefined
type Row = Cell[]
type SheetData = Row[]
```

---

## [3]-[CELL_STYLE]

`CellStyleProperties` extends the universal style set (fonts, fills, borders) with layout-only fields (alignment, spans, indent, wrap, rotation). The universal subset is shared with conditional formatting rules.

```ts
// write-excel-file — CellStyleProperties.d.ts
type FontWeight = 'bold'
type FontStyle = 'italic'
type Color = string
type FillPatternStyle =
  'darkDown' | 'darkGray' | 'darkGrid' | 'darkHorizontal' | 'darkTrellis' | 'darkUp' | 'darkVertical' |
  'gray0625' | 'gray125' | 'lightDown' | 'lightGray' | 'lightGrid' | 'lightHorizontal' | 'lightTrellis' |
  'lightUp' | 'lightVertical' | 'mediumGray'
type BorderStyle =
  'hair' | 'dotted' | 'dashDotDot' | 'dashDot' | 'dashed' | 'thin' | 'mediumDashDotDot' | 'slantDashDot' |
  'mediumDashDot' | 'mediumDashed' | 'medium' | 'double' | 'thick'
type TextDecoration =
  | { strikethrough?: boolean }
  | { strikethrough?: boolean; underline: true }
  | { strikethrough?: boolean; doubleUnderline: true }

interface CellStylePropertiesUniversal {
  fontFamily?: string; fontSize?: number; fontWeight?: FontWeight; fontStyle?: FontStyle
  textDecoration?: TextDecoration; textColor?: Color; backgroundColor?: Color
  fillPatternStyle?: FillPatternStyle; fillPatternColor?: Color
  borderColor?: Color; borderStyle?: BorderStyle
  leftBorderColor?: Color; leftBorderStyle?: BorderStyle
  rightBorderColor?: Color; rightBorderStyle?: BorderStyle
  topBorderColor?: Color; topBorderStyle?: BorderStyle
  bottomBorderColor?: Color; bottomBorderStyle?: BorderStyle
}
interface CellStyleProperties extends CellStylePropertiesUniversal {
  align?: 'left' | 'center' | 'right'
  alignVertical?: 'top' | 'center' | 'bottom'
  height?: number; columnSpan?: number; rowSpan?: number
  indent?: number; wrap?: boolean; textRotation?: number
}
```

---

## [4]-[COLUMN_SCHEMA]

`Column<Object>` is the object-mode schema: a `header` cell, a `cell` projector mapping each object to a `Cell`, and an optional `width`. `getSheetData` materializes `SheetData` from objects plus columns for reuse outside `writeXlsxFile`.

```ts
// write-excel-file — getSheetData.d.ts, SheetOptions.d.ts
interface SheetOptionsColumn { width?: number }
interface Column<Object> extends SheetOptionsColumn {
  header?: Cell
  cell: (object: Object, objectIndex: number) => Cell
}

declare function getSheetData<Object>(objects: Object[], columns: Column<Object>[]): SheetData
```

---

## [5]-[SHEET_OPTIONS]

`SheetOptions` carries per-sheet layout plus the sticky-pane, conditional-formatting, and image feature parameters. `SheetOptionsForObjects` requires the `columns` schema. `Sheet` bundles `data` with a sheet's options for multi-sheet writes. `Options` is workbook-level.

```ts
// write-excel-file — SheetOptions.d.ts, Sheet.d.ts, Options.d.ts
interface SheetOptions<FileContent>
  extends StickyRowsOrColumnsParameters, ConditionalFormattingParameters, ImagesParameters<FileContent> {
  sheet?: string
  orientation?: 'landscape'
  showGridLines?: boolean
  rightToLeft?: boolean
  zoomScale?: number
  dateFormat?: string
  columns?: SheetOptionsColumn[]
}
interface SheetOptionsForObjects<Object, FileContent> extends SheetOptions<FileContent> {
  columns: Column<Object>[]
}
interface Sheet<FileContent> extends SheetOptions<FileContent> { data: SheetData }
interface Options<FileContent> {
  fontFamily?: string
  fontSize?: number
  features?: Feature<FileContent>[]
}
```

---

## [6]-[SHEET_FEATURES]

Sticky panes, conditional formatting, and images mix into `SheetOptions`. Conditional formatting matches a cell range against a formula or operator condition and applies a universal-style override; images anchor `ImageType` content to a cell.

```ts
// write-excel-file — features/*.d.ts
interface StickyRowsOrColumnsParameters { stickyColumnsCount?: number; stickyRowsCount?: number }

interface ImagesParameters<FileContent> { images?: ImageType<FileContent>[] }
interface ImageType<FileContent> {
  content: FileContent; contentType: string
  width: number; height: number; dpi: number
  anchor: { row: number; column: number }
  offsetX?: number; offsetY?: number; title?: string; description?: string
}

type ConditionalFormattingCondition =
  | { formula: string }
  | { operator: '=' | '!='; value: string }
  | { operator: '<' | '<=' | '>' | '>=' | '=' | '!='; value: number }
  | { operator: '...'; value: number; value2: number }
interface ConditionalFormatting {
  cellRange: { from: { row: number; column: number }; to: { row: number; column: number } }
  condition: ConditionalFormattingCondition
  style: Exclude<CellStylePropertiesUniversal, 'fontFamily' | 'fontSize'>
}
interface ConditionalFormattingParameters { conditionalFormatting?: ConditionalFormatting[] }
```

---

## [7]-[FEATURE_HOOK]

`Feature` is the low-level escape hatch for transforming the raw OOXML parts (`insert`/`transform`/`transformElementAttributes`) or writing new files. The node `ReturnType` is the serialization rail.

```ts
// write-excel-file — Feature.d.ts, node/ReturnType.d.ts, node/FileContent.d.ts
interface Feature<FileContent> {
  files?: {
    transform?: {
      '[Content_Types].xml'?: FeatureTransformFile<FileContent>
      '_rels/.rels'?: FeatureTransformFile<FileContent>
      'xl/styles.xml'?: FeatureTransformFile<FileContent>
      'xl/workbook.xml'?: FeatureTransformFile<FileContent>
      'xl/_rels/workbook.xml.rels'?: FeatureTransformFile<FileContent>
      'xl/worksheets/sheet{id}.xml'?: FeatureTransformFileRelatedToSpecificSheet<FileContent>
      'xl/worksheets/_rels/sheet{id}.xml.rels'?: FeatureTransformFileRelatedToSpecificSheet<FileContent>
      'xl/drawings/drawing{id}.xml'?: FeatureTransformFileRelatedToSpecificSheet<FileContent>
      'xl/drawings/_rels/drawing{id}.xml.rels'?: FeatureTransformFileRelatedToSpecificSheet<FileContent>
    }
    write?: { files?: (sheetsOptions: SheetOptions<FileContent>[], properties: { read(path: string): FileContent | string | undefined }) => Record<string, FileContent | string> | undefined }
  }
}

type FileContent = Stream | Buffer | Blob // node entry
interface ReturnType {
  toBuffer: () => Promise<Buffer>
  toStream: (() => Promise<Readable>) | ((writableStream: Writable) => Promise<void>)
  toFile: (filePath: string) => Promise<void>
}
```

---

## [8]-[IMPLEMENTATION_LAW]

[OVERLOAD_DISPATCH]:
- `writeXlsxFile` dispatches on its first argument: `Cell[][]` is raw rows, `Object[]` requires `SheetOptionsForObjects` with a `columns` schema, and `Sheet[]` writes multiple sheets. The single-object overload demands `columns`; the raw overload makes `sheetOptions` optional.
- `getSheetData` is the standalone object-to-matrix conversion using the same `Column` schema, for callers that need `SheetData` without serializing.

[CELL_MODEL]:
- A cell is a bare `Value`, `null`/`undefined` (empty), or a `CellObject`. `type` declares the cell's data kind via a constructor (`String`, `Number`, `Date`, `Boolean`) or `'Formula'`; `format` controls the displayed number/date format string; the remaining `CellStyleProperties` fields style and span the cell.

[OUTPUT_RAIL]:
- The node entry returns `ReturnType`; pick `toBuffer` for in-memory bytes, `toFile` for a path, or `toStream` for piping (either returning a `Readable` or accepting a `Writable`). Browser/universal entries resolve `FileContent` and the result to environment-native forms.

[FEATURES]:
- Sticky panes, conditional formatting, and images are declared per-sheet inside `SheetOptions`. Conditional formatting forbids `fontFamily`/`fontSize` in its style override. The `Feature` hook is the raw-XML extension surface for parts not covered by the typed options; reserve it for behavior the structured options cannot express.
