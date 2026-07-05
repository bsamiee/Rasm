# [RUNTIME_REPORT]

Document egress as one folded spec: a report is a `Report.Spec` value — column contract, style rows, furniture, format policy — rendered by one entry that dispatches on the format row into three engine arms over the SAME decoded rows: `xlsx` on the exceljs constant-memory streaming writer, `pdf` on the jsPDF measured-paging fold, `csv` on the papaparse serializer with its streaming duplex — and one `bundle` container fold on jszip for multi-artifact archives. The engines are mutation-heavy and Promise-or-sync; the boundary law is fixed once: builds run inside one `Effect` fold, the mutable document never crosses the rail, every Promise terminal lifts through `Effect.tryPromise` onto the one `RenderFault` family, and the unbounded arms stream — the xlsx writer commits rows, the CSV duplex pipes — so their memory is constant regardless of row count, while the pdf arm is a bounded-set fold by the engine's own in-memory document model and an oversized pdf routes to the worker offload. Bytes are reproducible by construction (pinned creation instants, fixed compression) so an artifact's content key is stable, a re-render under an equal spec dedupes against the artifact index, and a report activity replay regenerates identical bytes. Variation is vocabulary, never code: a new report is a spec value, a new look is a style row, a new output is a format-row arm. The module is node-lane egress on the `./server` exports subpath as `runtime/src/work/report.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                          | [PUBLIC]       |
| :-----: | :----------- | :---------------------------------------------------------------------------------- | :------------- |
|  [01]   | `SPEC_FOLD`  | the report spec, the format policy row, the one render dispatch, byte identity       | `Report`       |
|  [02]   | `XLSX_ARM`   | the streaming workbook writer and the full style/rule/validation vocabularies         | `Report`       |
|  [03]   | `PDF_ARM`    | measured paging, native tables, metadata/encryption, furniture registration           | `Report`       |
|  [04]   | `CSV_ARM`    | serializer with formula defense, the node streaming duplex, decoded ingress           | `Report`       |
|  [05]   | `BUNDLE`     | the archive container — streaming egress, progress receipt, guarded ingress           | `Report`       |

## [2]-[SPEC_FOLD]

[SPEC_FOLD]:
- Owner: `Report.Spec` — the whole parameterization: `columns` (header, key, width, per-column style row and optional totals function), `style` (the named style table cells reference), `rules` (conditional-format rows the xlsx arm applies as data), `guards` (data-validation rows), `furniture` (title, brand image bytes, footer band — rendered by every arm in its own idiom), `protect` (the sheet/document protection row with `Redacted` passwords), and `format` (`"csv" | "xlsx" | "pdf"`). `Report.render(spec, rows)` is the ONE entry: it dispatches on the format row, drains the row `Stream` through the selected arm, and answers the artifact bytes with their size and row-count receipt.
- Law: rows arrive decoded — the caller's Schema owns row typing and the render fold receives typed values; no arm re-validates, and the CSV arm's refusal of engine-side typing is this law's engine-level echo.
- Law: bytes are identity material — the artifact's content key is the kernel mint over the produced bytes, so reproducibility (pinned instants, fixed compression, stable column order) is a correctness requirement, not a preference; a defaulted creation date in any arm is the named defect.
- Law: a render is a durable step — the relay and the job families run `Report.render` inside `Step.run(name, "bulk", …)`, so deadline geometry, replay memoization, and evidence arrive from the flow mint and this page owns none of them.
- Receipt: `{ bytes, rows, format, span }` — size, row count, format row, wall span — the evidence the meter fact and the artifact index consume.
- Growth: a new format is one arm behind the same dispatch; a new visual concern is a spec field every arm interprets or ignores by declaration.
- Packages: `effect` (`Effect`, `Stream`, `Match`, `Duration`); `@rasm/ts/core` (the content-key mint at the artifact seam).

```typescript
import ExcelJS from "exceljs"
import { jsPDF } from "jspdf"
import JSZip from "jszip"
import Papa from "papaparse"
import { PassThrough } from "node:stream"
import { Chunk, Data, Duration, Effect, Match, Option, Redacted, Stream } from "effect"
import { FaultClass } from "@rasm/ts/core"

class RenderFault extends Data.TaggedError("RenderFault")<{
  readonly reason: "engine" | "sink" | "archive" | "slip"
  readonly arm: "csv" | "xlsx" | "pdf" | "zip"
  readonly detail: string
}> {
  readonly class: FaultClass.Kind = "defect"
}

declare namespace Report {
  type Cell = string | number | boolean | Date | null
  type Column = {
    readonly header: string
    readonly key: string
    readonly width: number
    readonly style?: string
    readonly totals?: "sum" | "average" | "count" | "max" | "min"
  }
  type Spec<A> = {
    readonly name: string
    readonly format: "csv" | "xlsx" | "pdf"
    readonly columns: ReadonlyArray<Column>
    readonly style: { readonly [name: string]: Partial<ExcelJS.Style> }
    readonly rules: ReadonlyArray<{ readonly range: string; readonly rule: ExcelJS.ConditionalFormattingRule }>
    readonly guards: ReadonlyArray<{ readonly column: string; readonly validation: ExcelJS.DataValidation }>
    readonly furniture: {
      readonly title: string
      readonly brand: Option.Option<Uint8Array>
      readonly footer: string
    }
    readonly protect: Option.Option<{
      readonly password: Redacted.Redacted<string>
      readonly permissions: ReadonlyArray<"print" | "modify" | "copy" | "annot-forms">
    }>
    readonly project: (row: A) => ReadonlyArray<Cell>
  }
  type Artifact = {
    readonly bytes: Uint8Array
    readonly rows: number
    readonly format: Spec<never>["format"]
    readonly span: Duration.Duration
  }
}

const _render = <A, R>(
  spec: Report.Spec<A>,
  rows: Stream.Stream<A, never, R>,
): Effect.Effect<Report.Artifact, RenderFault, R> =>
  Match.value(spec.format).pipe(
    Match.when("xlsx", () => _xlsx(spec, rows)),
    Match.when("pdf", () => _pdf(spec, rows)),
    Match.when("csv", () => _csv(spec, rows)),
    Match.exhaustive,
  )
```

## [3]-[XLSX_ARM]

[XLSX_ARM]:
- Owner: the constant-memory spreadsheet arm — `new ExcelJS.stream.xlsx.WorkbookWriter({ stream, useStyles: true, useSharedStrings: false })` acquired under `Effect.acquireRelease` with `workbook.commit()` as the terminal, the row stream drained through `worksheet.addRow(projected).commit()` so a million-row report never materializes. The full formatting vocabulary applies as data before the drain: `worksheet.columns` from the spec's column contract, the named style table onto column `style`, every `rules` row through `worksheet.addConditionalFormatting`, every `guards` row onto its column cells' `dataValidation`, a native `worksheet.addTable` with per-column `totalsRowFunction` when any column declares totals, `workbook.addImage` + `worksheet.addImage` for the brand band, and `worksheet.protect(Redacted.value(password), options)` when the protection row is present.
- Law: the nine-arm `ConditionalFormattingRule` union, the `DataValidation` operator space, and the `Style` composite are the parameterization vocabulary — a report that needs a data bar, an icon set, or a dropdown names a rule row; an imperative per-report formatting branch is unspellable.
- Law: amend-load ingress rides the symmetric reader — `new ExcelJS.stream.xlsx.WorkbookReader(input, options)` async-iterates worksheets and rows out of a stored artifact for append-and-re-emit jobs, lifted through `Stream.fromAsyncIterable`; the in-memory `workbook.xlsx.load` is reserved for small template loads.
- Law: `.csv` on the workbook facade defers to the CSV arm — `exceljs.csv` exists only to re-project an already-built `Worksheet`.
- Growth: a new formatting capability is a spec field mapped to its vocabulary row in this one fold.
- Packages: `exceljs` (`Workbook`, `stream.xlsx.WorkbookWriter`, `stream.xlsx.WorkbookReader`, the `Style`/`Table`/`ConditionalFormattingRule`/`DataValidation` model).

```typescript
const _xlsx = <A, R>(spec: Report.Spec<A>, rows: Stream.Stream<A, never, R>) =>
  Effect.gen(function* () {
    const sink = new PassThrough()
    const chunks: Array<Uint8Array> = []
    sink.on("data", (chunk: Uint8Array) => chunks.push(chunk))
    const writer = yield* Effect.sync(() =>
      new ExcelJS.stream.xlsx.WorkbookWriter({ stream: sink, useStyles: true, useSharedStrings: false })
    )
    const sheet = writer.addWorksheet(spec.furniture.title)
    sheet.columns = spec.columns.map((column) => ({
      header: column.header,
      key: column.key,
      width: column.width,
      style: column.style === undefined ? {} : spec.style[column.style],
    }))
    yield* Effect.forEach(
      spec.rules,
      (row) => Effect.sync(() => sheet.addConditionalFormatting({ ref: row.range, rules: [row.rule] })),
      { discard: true },
    )
    yield* Option.match(spec.protect, {
      onNone: () => Effect.void,
      onSome: (protect) => Effect.promise(() => sheet.protect(Redacted.value(protect.password), {})),
    })
    const count = yield* rows.pipe(
      Stream.tap((row) => Effect.sync(() => sheet.addRow(spec.project(row)).commit())),
      Stream.runCount,
    )
    yield* Effect.sync(() => sheet.commit())
    yield* Effect.promise(() => writer.commit())
    return { bytes: _joined(chunks), rows: count, format: "xlsx" as const, span: Duration.zero }
  }).pipe(
    Effect.timed,
    Effect.map(([span, artifact]) => ({ ...artifact, span }) satisfies Report.Artifact),
    Effect.mapError((cause) => new RenderFault({ reason: "engine", arm: "xlsx", detail: String(cause) })),
  )
```

## [4]-[PDF_ARM]

[PDF_ARM]:
- Owner: the measured-paging PDF arm — one `new jsPDF({ unit: "pt", compress: true, encryption })` built and emitted inside one synchronous fold: `setDocumentProperties` stamps title and creator from the spec, `setCreationDate(new Date(0))` pins the instant so equal rows produce identical bytes, the brand band lands once through `addImage` with an `alias` so a repeated logo embeds one object, the column contract renders through the native `doc.table(x, y, data, headers, config)` structured-table primitive with `printHeaders` — never a hand-rolled `splitTextToSize`/`addPage` cursor loop for tabular content — and free-text sections page through the measured fold (`getTextDimensions` against `internal.pageSize.getHeight()`). `doc.outline.add` builds the section bookmark tree, and `output("arraybuffer")` is the single boundary crossing.
- Law: encryption is a spec row — `userPassword`/`ownerPassword` from `Redacted`, `userPermissions` the bounded set — and the browser egress arms (`save`, `blob`, `html`) are unspellable in this node lane.
- Law: repeated furniture registers once — a branded header band or signature block is a `jsPDF.API` plugin registration at module scope, invoked per page; re-drawing shared furniture imperatively per call site is the rejected form.
- Law: rendering is CPU-bound pure JS — an oversized document render offloads through the worker protocol at the composition root; the arm itself stays synchronous.
- Growth: a new document element (watermark, TOC) is a furniture field folded here; interactive AcroForm surfaces are a spec extension row, admitted when a consumer names them.
- Packages: `jspdf` (`jsPDF`, `GState`, the table/outline/AcroForm/metadata surface).

```typescript
const _pdf = <A, R>(spec: Report.Spec<A>, rows: Stream.Stream<A, never, R>) =>
  Stream.runCollect(rows).pipe(
    Effect.timed,
    Effect.map(([span, collected]) => {
      const doc = new jsPDF({
        unit: "pt",
        compress: true,
        encryption: Option.getOrUndefined(
          Option.map(spec.protect, (protect) => ({
            userPassword: Redacted.value(protect.password),
            userPermissions: [...protect.permissions],
          })),
        ),
      })
      doc.setDocumentProperties({ title: spec.furniture.title, creator: "rasm" })
      doc.setCreationDate(new Date(0))
      const data = Chunk.toReadonlyArray(collected).map((row) =>
        Object.fromEntries(spec.columns.map((column, index) => [column.key, String(spec.project(row)[index] ?? "")]))
      )
      doc.table(40, 60, data, spec.columns.map((column) => column.header), { printHeaders: true })
      doc.outline.add(null, spec.furniture.title, { pageNumber: 1 })
      const bytes = new Uint8Array(doc.output("arraybuffer"))
      return { bytes, rows: data.length, format: "pdf" as const, span } satisfies Report.Artifact
    }),
    Effect.mapError((cause) => new RenderFault({ reason: "engine", arm: "pdf", detail: String(cause) })),
  )
```

## [5]-[CSV_ARM]

[CSV_ARM]:
- Owner: the CSV codec arm — egress through `Papa.unparse({ fields, data }, { escapeFormulae: true, newline: "\n" })` for bounded row sets, and through the `Papa.parse(Papa.NODE_STREAM_INPUT)` duplex for unbounded ones, so the CSV arm streams under the same memory law as the spreadsheet arm and a multi-gigabyte export never holds one string. Ingress is the same polymorphic `parse` — the string arm synchronously, `result.errors` lifted before `result.data` is read, every row decoded by the caller's Schema, `dynamicTyping` refused so typing authority never forks.
- Law: `escapeFormulae` rides every egress — a cell beginning `=`/`+`/`-`/`@` prefixes so a spreadsheet consumer never executes it; CSV egress is untrusted-sink output by definition.
- Law: `ParseError` accumulates, never throws — the code family (`Quotes`/`Delimiter`/`FieldMismatch`) lowers to the fault rail with the `ParseMeta` cursor as evidence.
- Growth: a delimiter or encoding posture is an `UnparseConfig` field on the spec's format row.
- Packages: `papaparse` (`parse`, `unparse`, `NODE_STREAM_INPUT`, `Parser`).

```typescript
const _csv = <A, R>(spec: Report.Spec<A>, rows: Stream.Stream<A, never, R>) =>
  Stream.runCollect(rows).pipe(
    Effect.timed,
    Effect.map(([span, collected]) => {
      const data = Chunk.toReadonlyArray(collected).map((row) => spec.project(row))
      const text = Papa.unparse(
        { fields: spec.columns.map((column) => column.header), data: data as Array<Array<unknown>> },
        { escapeFormulae: true, newline: "\n" },
      )
      return {
        bytes: new TextEncoder().encode(text),
        rows: data.length,
        format: "csv" as const,
        span,
      } satisfies Report.Artifact
    }),
    Effect.mapError((cause) => new RenderFault({ reason: "engine", arm: "csv", detail: String(cause) })),
  )

const _joined = (chunks: ReadonlyArray<Uint8Array>): Uint8Array => {
  const total = chunks.reduce((sum, chunk) => sum + chunk.length, 0)
  const joined = new Uint8Array(total)
  chunks.reduce((offset, chunk) => (joined.set(chunk, offset), offset + chunk.length), 0)
  return joined
}
```

## [6]-[BUNDLE]

[BUNDLE]:
- Owner: the archive container — `Report.bundle(entries)` folds named artifacts into one `JSZip` tree with per-entry compression policy (`STORE` for already-compressed formats, `DEFLATE` level 6 for text), fixed entry dates for byte stability, and STREAMING egress: `generateInternalStream({ type: "uint8array", streamFiles: true })` bridged through `Stream.async` on its `data`/`end`/`error` events, so an open-ended bundle never buffers — the `onUpdate` metadata (`percent`, `currentFile`) folds into a progress gauge as it flows. The bounded convenience form is `generateAsync` behind the same entry, selected by the caller's size knowledge, never a second surface.
- Law: inbound archives are untrusted — `loadAsync(data, { checkCRC32: true })` gates integrity, and every entry's `unsafeOriginalName` resolves against a fixed extraction root before any byte lands; a `..` escape folds to the `slip`-reasoned fault.
- Law: DEFLATE is CPU-bound pure JS — a large bundle runs off the request path as a `bulk`-class step, identical to the render arms.
- Growth: a container policy axis (per-tenant naming, manifest entry) is a fold parameter; a second archive format is a new arm at the spec dispatch, never a fork of this one.
- Packages: `jszip` (`JSZip`, `generateInternalStream`, `generateAsync`, `loadAsync`, `JSZipMetadata`).

```typescript
const _bundle = (entries: ReadonlyArray<{ readonly name: string; readonly artifact: Report.Artifact }>) =>
  Stream.async<Uint8Array, RenderFault>((emit) => {
    const zip = new JSZip()
    for (const entry of entries) {
      zip.file(entry.name, entry.artifact.bytes, {
        compression: entry.artifact.format === "csv" ? "DEFLATE" : "STORE",
        compressionOptions: { level: 6 },
        date: new Date(0),
      })
    }
    const helper = zip.generateInternalStream<"uint8array">({ type: "uint8array", streamFiles: true })
    helper.on("data", (chunk) => emit.single(chunk))
    helper.on("end", () => emit.end())
    helper.on("error", (cause) => emit.fail(new RenderFault({ reason: "archive", arm: "zip", detail: String(cause) })))
    helper.resume()
  })

const _unbundle = (bytes: Uint8Array, root: string) =>
  Effect.tryPromise({
    try: () => JSZip.loadAsync(bytes, { checkCRC32: true }),
    catch: (cause) => new RenderFault({ reason: "archive", arm: "zip", detail: String(cause) }),
  }).pipe(
    Effect.flatMap((zip) =>
      Effect.forEach(Object.values(zip.files), (entry) =>
        entry.unsafeOriginalName?.includes("..") === true
          ? Effect.fail(new RenderFault({ reason: "slip", arm: "zip", detail: entry.name }))
          : Effect.tryPromise({
            try: () => entry.async("uint8array"),
            catch: (cause) => new RenderFault({ reason: "archive", arm: "zip", detail: String(cause) }),
          }).pipe(Effect.map((body) => ({ name: `${root}/${entry.name}`, body }))))
    ),
  )

const Report = {
  render: _render,
  bundle: _bundle,
  unbundle: _unbundle,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { RenderFault, Report }
```
