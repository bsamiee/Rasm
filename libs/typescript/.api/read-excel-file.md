# [API_CATALOGUE] read-excel-file

`read-excel-file` parses `.xlsx` workbooks into JavaScript without a full Office runtime. The default export `readXlsxFile` reads every sheet into `Sheet[]` (`{ sheet, data }`), `readSheet` reads a single sheet by index or name into raw `SheetData` (`(CellValue | null)[][]`), and a schema-bearing overload of `readSheet`/`parseSheetData` maps rows onto typed objects with column-driven validation. Parsing is environment-routed through subpath exports: `read-excel-file/node` (input `string | Stream | Blob | Buffer`), `read-excel-file/browser` and `read-excel-file/universal` (input `Blob | ArrayBuffer`), and `read-excel-file/web-worker`. Schema results are returned as a discriminated `{ objects } | { errors }` rather than thrown, and built-in value types (`String`, `Number`, `Date`, `Boolean`, `Integer`, `URL`, `Email`) plus custom parser functions drive per-cell coercion.

---

## [1]-[PACKAGE_SURFACE]

The entry contents are identical across `/node`, `/browser`, `/universal`, and `/web-worker`; only the imported `Input` type differs by environment.

```ts
// read-excel-file — node/universal entry
import type { Input } from './input.js' // node: string | Stream | Blob | Buffer ; universal/browser: Blob | ArrayBuffer

export default function readXlsxFile<ParsedNumber = number>(
  input: Input,
  options?: Options<ParsedNumber>
): Promise<Sheet<ParsedNumber>[]>

export function readSheet<ParsedNumber = number>(input: Input, sheet?: number | string, options?: Options<ParsedNumber>): Promise<SheetData<ParsedNumber>>
export function readSheet<ParsedNumber = number>(input: Input, options?: Options<ParsedNumber>): Promise<SheetData<ParsedNumber>>
export function readSheet<Object extends object, ColumnTitle extends string = string, Error extends ParseSheetDataError = ParseSheetDataError<ColumnTitle>, ParsedNumber = number>(
  input: Input,
  options: OptionsWithSchema<Object, ColumnTitle, ParsedNumber>
): Promise<ParseSheetDataResult<Object, ColumnTitle, Error>>

export function parseSheetData<Object extends object, ColumnTitle extends string = string, Error extends ParseSheetDataError = ParseSheetDataError<ColumnTitle>>(
  data: SheetData,
  schema: Schema<Object, ColumnTitle>,
  options?: ParseSheetDataOptions
): ParseSheetDataResult<Object, ColumnTitle, Error>

export type { CellValue, Row, SheetData, Sheet, ParseSheetDataResult, Schema }
export type { ParseSheetDataCustomType, String, Date, Number, Boolean, Integer, Email, URL }
export type { ParseSheetDataCustomTypeErrorMessage, ParseSheetDataCustomTypeErrorReason, ParseSheetDataError, ParseSheetDataValueRequiredError }
```

---

## [2]-[CELL_TYPES]

A workbook reads into `Sheet[]`; each sheet's `data` is a `SheetData` of nullable cell values. `CellValue` is the spreadsheet-native scalar union; `ParsedNumber` defaults to `number` and is overridable by the `parseNumber` option.

```ts
// read-excel-file — SheetData.d.ts, Sheet.d.ts
type CellValue<ParsedNumber = number> = string | ParsedNumber | boolean | typeof Date
type Row<ParsedNumber = number> = (CellValue<ParsedNumber> | null)[]
type SheetData<ParsedNumber = number> = Row<ParsedNumber>[]
type Sheet<ParsedNumber = number> = { sheet: string; data: SheetData<ParsedNumber> }
```

---

## [3]-[READ_OPTIONS]

`Options` controls raw reads; `OptionsWithSchema` adds the `schema` that switches `readSheet` to the typed-object overload.

```ts
// read-excel-file — Options.d.ts, OptionsWithSchema.d.ts
interface Options<ParsedNumber = number> {
  trim?: boolean
  parseNumber?: (string: string) => ParsedNumber
  dateFormat?: string
}
interface OptionsWithSchema<Object extends object, ColumnTitle extends string = string, ParsedNumber = number> extends Options<ParsedNumber> {
  schema: Schema<Object, ColumnTitle>
}
```

---

## [4]-[SCHEMA]

`Schema` maps each target object key to a `column` plus an optional `type`, `oneOf`, `required`, and `validate`; nested objects recurse via a child `schema`. Value `type` is a constructor (`String`, `Number`, `Date`, `Boolean`), a built-in parser (`Integer`, `URL`, `Email`), or a custom `(value: CellValue) => ParsedValue | undefined`.

```ts
// read-excel-file — parseSheetDataSchema.d.ts, parseSheetDataValueType.d.ts
type Schema<Object extends object, ColumnTitle extends string = string> = {
  [Key in keyof Object]: SchemaEntry<Key, Object, Object, ColumnTitle>
}
interface SchemaEntryForValue<Key extends keyof Object, Object extends object, TopLevelObject extends object, ColumnTitle extends string> {
  column: ColumnTitle
  type?: ParseSheetDataValueType<ParseSheetDataCustomType<Object[Key]>>
  oneOf?: Object[Key][]
  required?: boolean | ((row: TopLevelObject) => boolean)
  validate?(value: Object[Key]): void
}
interface SchemaEntryRecursive<Key extends keyof Object, Object extends object, TopLevelObject extends object, ColumnTitle extends string> {
  schema: Object[Key] extends object ? Schema<Object[Key], ColumnTitle> : never
  required?: boolean | ((row: TopLevelObject) => boolean)
}

type StringType = StringConstructor; type DateType = DateConstructor
type NumberType = NumberConstructor; type BooleanType = BooleanConstructor
declare function Integer(value: CellValue): number
declare function URL(value: CellValue): string
declare function Email(value: CellValue): string
type ParseSheetDataCustomType<ParsedValue> = (value: CellValue) => ParsedValue | undefined
type ParseSheetDataValueType<CustomType> =
  | StringType | DateType | NumberType | BooleanType
  | typeof Integer | typeof URL | typeof Email
  | CustomType
```

---

## [5]-[PARSE_RESULT]

Schema-mapped parsing returns a discriminated union: a success carries `objects` with `errors: undefined`; a failure carries `errors` with `objects: undefined`. `ParseSheetDataOptions` tunes missing-column, empty-cell, and array-splitting behavior.

```ts
// read-excel-file — parseSheetData.d.ts
type ParseSheetDataResult<Object extends object, ColumnTitle extends string, Error extends ParseSheetDataError = ParseSheetDataError<ColumnTitle>> =
  | { objects: Object[]; errors: undefined }
  | { objects: undefined; errors: Error[] }

interface ParseSheetDataOptions {
  propertyValueWhenColumnIsMissing?: any
  propertyValueWhenCellIsEmpty?: any
  transformEmptyArray?(arrayPropertyValue: never[], parameters: { path: string }): any
  transformEmptyObject?(object: Record<string, undefined | null>, parameters: { path?: string }): any
  arrayValueSeparator?: string
}
```

---

## [6]-[ERRORS]

Each error carries `row`, `column`, `columnIndex`, an `error` discriminant, an optional `reason`, the offending `value`, and the schema `type`. `'required'` errors carry `value: null | undefined`; all other errors carry a non-null `CellValue`. The exported `ParseSheetDataError` union spans every built-in and custom-type failure.

```ts
// read-excel-file — parseSheetDataError.d.ts
interface ParseSheetDataValueRequiredError<ColumnTitle extends string = string, CustomType extends ParseSheetDataCustomType<unknown> = never> {
  row: number; column: ColumnTitle; columnIndex: number
  error: 'required'; reason: undefined; value: null | undefined
  type?: ParseSheetDataValueType<CustomType>
}
// Base error discriminants (error / reason):
//   'not_a_boolean', 'not_a_date', 'out_of_bounds', 'not_a_string', 'invalid_number',
//   'not_a_number', 'not_an_integer', 'not_a_url', 'not_an_email', 'invalid' (reason 'syntax')
type ParseSheetDataError<
  ColumnTitle extends string = string,
  CustomType extends ParseSheetDataCustomType<unknown> = never,
  ErrorMessage extends ParseSheetDataCustomTypeErrorMessage<CustomType> = string,
  ErrorReason extends ParseSheetDataCustomTypeErrorReason<CustomType, ErrorMessage> = string | undefined
> =
  | ParseSheetDataBuiltInValueTypeError<ColumnTitle>
  | ParseSheetDataValueRequiredError<ColumnTitle, ParseSheetDataValueType<CustomType>>
  | ParseSheetDataArrayValueNotAStringError<ColumnTitle, ParseSheetDataValueType<CustomType>>
  | ParseSheetDataArrayValueSyntaxError<ColumnTitle, ParseSheetDataValueType<CustomType>>
  | ParseSheetDataErrorCustomType<ColumnTitle, ParseSheetDataValueType<CustomType>, ErrorMessage, ErrorReason>

type ParseSheetDataCustomTypeErrorMessage<CustomType extends ParseSheetDataCustomType<unknown>> = string
type ParseSheetDataCustomTypeErrorReason<CustomType extends ParseSheetDataCustomType<unknown>, ErrorMessage extends string> = string | undefined
```

---

## [7]-[IMPLEMENTATION_LAW]

[ENTRY_ROUTING]:
- Subpath exports route by environment: import from `read-excel-file/node` for filesystem/stream/Buffer input, `read-excel-file/browser` or `read-excel-file/universal` for `Blob | ArrayBuffer`, and `read-excel-file/web-worker` to run parsing off the main thread. The function surface is identical across all four; only `Input` widens.

[DISPATCH]:
- `readXlsxFile` yields all sheets; `readSheet` yields one (by 1-based index, by sheet name, or the default sheet). Supplying `schema` via `OptionsWithSchema` switches the return from `SheetData` to a typed `ParseSheetDataResult`.
- `parseSheetData` is the pure mapper: feed it already-read `SheetData` plus a `Schema` to convert rows to objects without re-reading the workbook.

[RESULT_RAIL]:
- Schema parsing never throws on bad cells. Narrow on `result.errors === undefined` (success) or iterate `result.errors` (failure). Each error pinpoints `row`/`column`/`columnIndex` and the failed `type`.
- `required` may be a predicate over the parsed row; `validate` throws inside the entry to flag a custom failure; `oneOf` constrains the value set; nested `schema` builds nested objects from flat columns.

[VALUE_TYPES]:
- The header column maps to a target key by the schema's `column` field, not by position. `type` defaults to `String` when omitted. Custom parsers returning `undefined` are treated as `null` (empty cell).
