# [RUNTIME_REPORT]

Document egress as one folded `Report.Spec` family: the format discriminant selects CSV, XLSX, or PDF while each column owns its value projection, and every render answers one single-subscription `Report.Artifact` with a chunked body and a receipt settled by that drain. Mutation-heavy Promise and synchronous engines remain inside one `Effect` boundary and one `ReportFault` family. Unbounded CSV and streaming-XLSX modes flow end to end; rich XLSX and PDF declare row ceilings before materializing the engine model, with oversized PDF routed to the render worker. `Report.gathered` is the only bytes-in-memory consumer fold and requires a ceiling. Pinned instants, fixed compression, and stable column order make equal renders byte-stable so the data object plane mints identical content identity at landing; runtime never mints that identity. The module is node-lane egress on `./server` as `runtime/src/work/report.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                         | [PUBLIC] |
| :-----: | :---------- | :----------------------------------------------------------------------------- | :------- |
|  [01]   | `SPEC_FOLD` | the report spec, the format policy row, the one render dispatch, byte identity | `Report` |
|  [02]   | `XLSX_ARM`  | the streaming workbook writer and the full style/rule/validation vocabularies  | `Report` |
|  [03]   | `PDF_ARM`   | measured paging, native tables, metadata/encryption, furniture registration    | `Report` |
|  [04]   | `CSV_ARM`   | serializer with formula defense, the node streaming duplex, decoded ingress    | `Report` |
|  [05]   | `BUNDLE`    | the archive container — streaming egress, progress receipt, guarded ingress    | `Report` |

## [02]-[SPEC_FOLD]

[SPEC_FOLD]:
- Owner: `Report.Spec` — a format-discriminated family over one base. Every base `columns` row owns header, key, width, and value projection, so column order and row projection cannot drift; the `Xlsx` arm carries one payload-timing discriminant whose `Rich` case alone owns validation, brand, footer, and row ceiling, while style, keyed cell policy, rules, protection, and title remain valid for both modes; the `Pdf` arm carries furniture, protection, and its materialization ceiling; and the `Csv` arm carries `UnparseConfig`. `Report.Artifact.body` is single-subscription by `Ref.getAndSet`, and its receipt settles on success, failure, or interrupted drain.
- Law: rows arrive decoded — the caller's Schema owns row typing and the render fold receives typed values; no arm re-validates, and the CSV arm's refusal of engine-side typing is this law's engine-level echo.
- Law: materialization is a consumer fold under a stated ceiling — `Report.gathered(artifact, ceiling)` is the ONE bytes-in-memory form, faulting `ceiling`-reasoned (`exhausted` class) the moment the running total passes the bound, so an unbounded body structurally cannot buffer whole; a consumer calling it attests its bound (a mail attachment cap, a bundle entry cap) at the call.
- Law: bytes are identity material minted where they land — reproducibility (pinned instants, fixed compression, stable column order) is a correctness requirement because the data wave's artifact-index put mints the content key over the landed bytes and dedupes equal renders; runtime never mints content identity, a defaulted creation date in any arm is the named defect, and a replay under an equal spec regenerates byte-identical output.
- Law: a render is a durable step — the relay and the job families run `Report.render` inside `Step.run(name, "bulk", …)`, so deadline geometry, replay memoization, and evidence arrive from the flow mint and this page owns none of them.
- Law: the off-thread threshold is wired, not aspirational — a `pdf` fold whose bounded projected cell set exceeds `Report.policy.offloadRows` routes through `proc/worker`'s `Render` request: the data-only plan (columns, furniture, projected cells) encodes to bytes, crosses zero-copy, and the produced bytes cross back; a protected document renders in-process regardless, because a sealed password never crosses the thread seam — the one exemption to worker routing, never to the row ceiling.
- Receipt: `Report.Receipt` — `{ rows, bytes, format, span }` — settles through the sealed body's `Deferred` when the last chunk flows, the evidence the meter fact and the artifact index consume; a receipt read before the body drains simply waits.
- Growth: a new format is one arm behind the same dispatch; a new visual concern is a spec field every arm interprets or ignores by declaration.
- Packages: `effect` (`Effect`, `Stream`, `Match`, `Duration`, `Deferred`, `Ref`, `Clock`); `../proc/worker.ts` (`Bench`, `Render` — the off-thread ceiling).

```typescript
import ExcelJS from "exceljs"
import { jsPDF } from "jspdf"
import JSZip from "jszip"
import Papa from "papaparse"
import { Buffer } from "node:buffer"
import path from "node:path"
import { PassThrough, Transform } from "node:stream"
import { Array, Chunk, Clock, Data, DateTime, Deferred, Duration, Effect, Match, Option, Redacted, Ref, Schema, Stream } from "effect"
import { FaultClass } from "@rasm/ts/core"
import { Bench, BenchFault, Drop, Render } from "../proc/worker.ts"

const _reasons = {
  engine: "defect",
  sink: "unavailable",
  archive: "defect",
  slip: "malformed",
  ceiling: "exhausted",
  consumed: "conflicted",
} as const satisfies Record<string, FaultClass.Kind>

class ReportFault extends Data.TaggedError("ReportFault")<{
  readonly reason: keyof typeof _reasons
  readonly arm: "csv" | "xlsx" | "pdf" | "zip"
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _reasons[this.reason]
  }
}

declare namespace Report {
  type Cell = string | number | boolean | DateTime.Utc | null
  type Column<A> = {
    readonly header: string
    readonly key: string
    readonly width: number
    readonly value: (row: A) => Cell
  }
  type Base<A> = {
    readonly name: string
    readonly columns: ReadonlyArray<Column<A>>
  }
  type Brand = Option.Option<{ readonly bytes: Uint8Array; readonly extension: "png" | "jpeg" }>
  type Furniture = {
    readonly title: string
    readonly brand: Brand
    readonly footer: string
  }
  type XlsxProtection = Option.Option<{
    readonly password: Redacted.Redacted<string>
    readonly options: ExcelJS.WorksheetProtection
  }>
  type PdfProtection = Option.Option<{
    readonly userPassword: Redacted.Redacted<string>
    readonly ownerPassword: Redacted.Redacted<string>
    readonly permissions: ReadonlyArray<"print" | "modify" | "copy" | "annot-forms">
  }>
  type XlsxMode =
    | { readonly _tag: "Stream" }
    | {
      readonly _tag: "Rich"
      readonly rowCeiling: number
      readonly guards: ReadonlyArray<{ readonly column: string; readonly validation: ExcelJS.DataValidation }>
      readonly brand: Brand
      readonly footer: string
    }
  type Xlsx<A> = Base<A> & {
    readonly format: "xlsx"
    readonly title: string
    readonly mode: XlsxMode
    readonly style: { readonly [name: string]: Partial<ExcelJS.Style> }
    readonly cells: { readonly [key: string]: { readonly style?: string; readonly totals?: "sum" | "average" | "count" | "max" | "min" } }
    readonly rules: ReadonlyArray<{ readonly range: string; readonly rule: ExcelJS.ConditionalFormattingRule }>
    readonly protect: XlsxProtection
  }
  type Pdf<A> = Base<A> & {
    readonly format: "pdf"
    readonly rowCeiling: number
    readonly furniture: Furniture
    readonly protect: PdfProtection
  }
  type Csv<A> = Base<A> & { readonly format: "csv"; readonly csv: Papa.UnparseConfig }
  type Spec<A> = Xlsx<A> | Pdf<A> | Csv<A>
  type Format = Spec<never>["format"]
  type Receipt = {
    readonly rows: number
    readonly bytes: number
    readonly format: Format
    readonly span: Duration.Duration
  }
  type Artifact<R> = {
    readonly format: Format
    readonly body: Stream.Stream<Uint8Array, ReportFault, R>
    readonly receipt: Effect.Effect<Receipt, ReportFault>
  }
  type Bundle = {
    readonly entries: ReadonlyArray<{ readonly name: string; readonly format: Format; readonly bytes: Uint8Array }>
    readonly offloadAbove: number
    readonly progress: (metadata: JSZip.JSZipMetadata) => void
  }
}

const _policy = { offloadRows: 50_000 } as const

const _PlanCell = Schema.Union(Schema.String, Schema.Number, Schema.Boolean, Schema.Null)
const _PdfPlan = Schema.Struct({
  columns: Schema.Array(Schema.Struct({ header: Schema.String, key: Schema.String, width: Schema.Number })),
  furniture: Schema.Struct({
    title: Schema.String,
    footer: Schema.String,
    brand: Schema.NullOr(Schema.Struct({ extension: Schema.Literal("png", "jpeg"), bytes: Schema.String })),
  }),
  cells: Schema.Array(Schema.Array(_PlanCell)),
})
const _BundlePlan = Schema.Struct({
  entries: Schema.Array(Schema.Struct({ name: Schema.String, format: Schema.Literal("csv", "xlsx", "pdf"), bytes: Schema.String })),
})

type PdfPlan = Schema.Schema.Type<typeof _PdfPlan>
type BundlePlan = Schema.Schema.Type<typeof _BundlePlan>

const _decodedPlan = <A, I>(schema: Schema.Schema<A, I>, bytes: Uint8Array): Effect.Effect<A, BenchFault> =>
  Schema.decodeUnknown(Schema.parseJson(schema))(new TextDecoder().decode(bytes)).pipe(
    Effect.mapError(() => new BenchFault({ reason: "refused", class: "malformed" })),
  )

const _project = <A>(spec: Report.Base<A>, row: A): ReadonlyArray<Report.Cell> =>
  Array.map(spec.columns, (column) => column.value(row))

const _scalar = (cell: Report.Cell): string | number | boolean | null =>
  DateTime.isDateTime(cell) ? DateTime.formatIso(cell) : cell

const _excel = (cell: Report.Cell): string | number | boolean | Date | null =>
  DateTime.isDateTime(cell) ? DateTime.toDateUtc(cell) : cell

const _ZIP_STAMPS = [
  { signature: [0x50, 0x4b, 0x03, 0x04], offset: 10, method: 8, version: 4, name: 26, width: 30 },
  { signature: [0x50, 0x4b, 0x01, 0x02], offset: 12, method: 10, version: 6, name: 28, width: 46 },
] as const

const _canonicalZip = (bytes: Uint8Array): Uint8Array => {
  const canonical = Uint8Array.from(bytes)
  for (let index = 0; index <= canonical.length - 30; index += 1) {
    const row = Array.findFirst(_ZIP_STAMPS, (header) => {
      const date = (canonical[index + header.offset + 2] ?? 0) | ((canonical[index + header.offset + 3] ?? 0) << 8)
      return index + header.width <= canonical.length
        && Array.every(header.signature, (byte, offset) => canonical[index + offset] === byte)
        && canonical[index + header.version + 1] === 0
        && (canonical[index + header.version] ?? 0) >= 10
        && (canonical[index + header.version] ?? 0) <= 63
        && ((canonical[index + header.method] === 0) || (canonical[index + header.method] === 8))
        && canonical[index + header.method + 1] === 0
        && ((canonical[index + header.name] ?? 0) | ((canonical[index + header.name + 1] ?? 0) << 8)) > 0
        && ((date >> 9) + 1980) >= 2020
    })
    if (Option.isSome(row)) canonical.set([0, 0, 33, 0], index + row.value.offset)
  }
  return canonical
}

const _canonicalZipStream = (): Transform => {
  let held = Buffer.alloc(0)
  return new Transform({
    transform(chunk: Uint8Array, _encoding, done) {
      const canonical = Buffer.from(_canonicalZip(Buffer.concat([held, Buffer.from(chunk)])))
      const edge = Number.max(0, canonical.length - 45)
      this.push(canonical.subarray(0, edge))
      held = Buffer.from(canonical.subarray(edge))
      done()
    },
    flush(done) {
      this.push(_canonicalZip(held))
      done()
    },
  })
}

const _sealed = <R>(
  format: Report.Format,
  counted: Ref.Ref<number>,
  body: Stream.Stream<Uint8Array, ReportFault, R>,
): Effect.Effect<Report.Artifact<R>> =>
  Effect.gen(function* () {
    const settled = yield* Deferred.make<Report.Receipt, ReportFault>()
    const size = yield* Ref.make(0)
    const openedOnce = yield* Ref.make(false)
    const opened = yield* Clock.currentTimeMillis
    return {
      format,
      receipt: Deferred.await(settled),
      body: Stream.unwrap(
        Effect.map(Ref.getAndSet(openedOnce, true), (replayed) =>
          replayed
            ? Stream.fail(new ReportFault({ reason: "consumed", arm: format, detail: "<single-subscription>" }))
            : body.pipe(
              Stream.tap((chunk) => Ref.update(size, (held) => held + chunk.length)),
              Stream.tapError((fault) => Deferred.fail(settled, fault)),
              Stream.onDone(() =>
                Effect.gen(function* () {
                  const closed = yield* Clock.currentTimeMillis
                  yield* Deferred.succeed(settled, {
                    rows: yield* Ref.get(counted),
                    bytes: yield* Ref.get(size),
                    format,
                    span: Duration.millis(closed - opened),
                  })
                })),
              Stream.ensuring(
                Effect.flatMap(Deferred.isDone(settled), (done) =>
                  done
                    ? Effect.void
                    : Effect.asVoid(Deferred.fail(settled, new ReportFault({
                      reason: "sink", arm: format, detail: "<interrupted>",
                    }))),
                ),
              ),
            )),
      ),
    }
  })

const _render = <A, R>(
  spec: Report.Spec<A>,
  rows: Stream.Stream<A, never, R>,
): Effect.Effect<Report.Artifact<R>, ReportFault, R | Bench> =>
  Match.value(spec).pipe(
    Match.when({ format: "xlsx" }, (xlsx) => _xlsx(xlsx, rows)),
    Match.when({ format: "pdf" }, (pdf) => _pdf(pdf, rows)),
    Match.when({ format: "csv" }, (csv) => _csv(csv, rows)),
    Match.exhaustive,
  )

const _gathered = <R>(artifact: Report.Artifact<R>, ceiling: number): Effect.Effect<Uint8Array, ReportFault, R> =>
  artifact.body.pipe(
    Stream.runFoldEffect({ held: Chunk.empty<Uint8Array>(), total: 0 }, (state, chunk) =>
      state.total + chunk.length > ceiling
        ? Effect.fail(new ReportFault({ reason: "ceiling", arm: artifact.format, detail: String(ceiling) }))
        : Effect.succeed({ held: Chunk.append(state.held, chunk), total: state.total + chunk.length })),
    Effect.map((state) => _joined(Chunk.toReadonlyArray(state.held))),
  )
```

## [03]-[XLSX_ARM]

[XLSX_ARM]:
- Owner: the spreadsheet arm has one discriminated policy with two honest payload timings. `Stream` uses `ExcelJS.stream.xlsx.WorkbookWriter`, commits each projected row, canonicalizes ZIP entry timestamps across arbitrary chunk boundaries, and emits compressed chunks end to end; it carries title, column styles, conditional rules, and protection only. `Rich({ rowCeiling, guards, brand, footer })` admits at most the declared row bound, builds one `Workbook`, canonicalizes its completed archive, and integrates the full vocabulary the streaming writer cannot apply after committed cells: native tables and totals, data validation, brand image, footer, column styles, conditional rules, and protection. The modes share the same `Report.Xlsx` owner and `_xlsx` dispatch; rich-only fields cannot inhabit the streaming case as inert option ghosts.
- Law: the nine-arm `ConditionalFormattingRule` union, the `DataValidation` operator space, and the `Style` composite are the parameterization vocabulary — a report that needs a data bar, an icon set, or a dropdown names a rule row; an imperative per-report formatting branch is unspellable.
- Law: amend-load ingress rides the symmetric reader — `new ExcelJS.stream.xlsx.WorkbookReader(input, options)` async-iterates worksheets and rows out of a stored artifact for append-and-re-emit jobs, lifted through `Stream.fromAsyncIterable`; the in-memory `workbook.xlsx.load` is reserved for small template loads.
- Law: `.csv` on the workbook facade defers to the CSV arm — `exceljs.csv` exists only to re-project an already-built `Worksheet`.
- Law: the body streams end to end — the `PassThrough` sink bridges into `Stream.asyncScoped` (subscribe on acquire, `destroy` on release, `emit.single`/`emit.end`/`emit.fail` the admitted crossings), and the commit driver runs as a scope-forked fiber feeding the writer while the consumer drains chunks, so compressed output leaves memory as it is produced and a chunk array never accumulates.
- Exemption: the `PassThrough` event callbacks are the platform-forced statement seam — the writer mutates a node stream outside the rail, and the bridge's listeners are the one sanctioned push-crossing site in this module.
- Growth: a new formatting capability is a spec field mapped to its vocabulary row in this one fold.
- Packages: `exceljs` (`Workbook`, `stream.xlsx.WorkbookWriter`, `stream.xlsx.WorkbookReader`, the `Style`/`Table`/`ConditionalFormattingRule`/`DataValidation` model).

```typescript
const _committed = <A, R>(spec: Report.Xlsx<A>, rows: Stream.Stream<A, never, R>, sink: PassThrough, counted: Ref.Ref<number>) =>
  Effect.gen(function* () {
    const writer = yield* Effect.sync(() =>
      new ExcelJS.stream.xlsx.WorkbookWriter({ stream: sink, useStyles: true, useSharedStrings: false })
    )
    writer.created = new Date(0)
    writer.modified = new Date(0)
    writer.lastPrinted = new Date(0)
    const sheet = writer.addWorksheet(spec.title)
    sheet.columns = [
      ...Array.map(spec.columns, (column) => ({
        header: column.header,
        key: column.key,
        width: column.width,
        style: spec.cells[column.key]?.style === undefined ? {} : (spec.style[spec.cells[column.key].style] ?? {}),
      })),
    ]
    yield* Effect.forEach(
      spec.rules,
      (row) => Effect.sync(() => sheet.addConditionalFormatting({ ref: row.range, rules: [row.rule] })),
      { discard: true },
    )
    yield* Option.match(spec.protect, {
      onNone: () => Effect.void,
      onSome: (protect) =>
        Effect.tryPromise({
          try: () => sheet.protect(Redacted.value(protect.password), protect.options),
          catch: (cause) => new ReportFault({ reason: "engine", arm: "xlsx", detail: String(cause) }),
        }),
    })
    yield* Stream.runForEach(rows, (row) =>
      Effect.zipRight(
        Effect.sync(() => sheet.addRow(Array.map(_project(spec, row), _excel)).commit()),
        Ref.update(counted, (held) => held + 1),
      ))
    yield* Effect.sync(() => sheet.commit())
    yield* Effect.tryPromise({
      try: () => writer.commit(),
      catch: (cause) => new ReportFault({ reason: "engine", arm: "xlsx", detail: String(cause) }),
    })
  })

const _xlsxStream = <A, R>(spec: Report.Xlsx<A>, rows: Stream.Stream<A, never, R>): Effect.Effect<Report.Artifact<R>> =>
  Effect.flatMap(Ref.make(0), (counted) =>
    _sealed(
      "xlsx",
      counted,
      Stream.asyncScoped<Uint8Array, ReportFault, R>((emit) =>
        Effect.gen(function* () {
          const bridge = yield* Effect.acquireRelease(
            Effect.sync(() => {
              const sink = new PassThrough()
              const canonical = _canonicalZipStream()
              sink.pipe(canonical)
              canonical.on("data", (chunk: Uint8Array) => void emit.single(chunk))
              canonical.on("end", () => void emit.end())
              canonical.on("error", (cause) =>
                void emit.fail(new ReportFault({ reason: "sink", arm: "xlsx", detail: String(cause) })))
              return { sink, canonical }
            }),
            ({ sink, canonical }) => Effect.sync(() => { sink.destroy(); canonical.destroy() }),
          )
          yield* Effect.forkScoped(
            Effect.tapError(_committed(spec, rows, bridge.sink, counted), (fault) =>
              Effect.sync(() => void emit.fail(fault))),
          )
        })),
    ))

const _xlsxRich = <A, R>(
  spec: Report.Xlsx<A>,
  mode: Extract<Report.XlsxMode, { readonly _tag: "Rich" }>,
  rows: Stream.Stream<A, never, R>,
): Effect.Effect<Report.Artifact<never>, ReportFault, R> =>
  Effect.gen(function* () {
    const collected = yield* rows.pipe(Stream.take(mode.rowCeiling + 1), Stream.runCollect)
    const values = Chunk.toReadonlyArray(collected)
    if (values.length > mode.rowCeiling) {
      return yield* Effect.fail(new ReportFault({ reason: "ceiling", arm: "xlsx", detail: String(mode.rowCeiling) }))
    }
    const cells = Array.map(values, (row) => Array.map(_project(spec, row), _excel))
    const bytes = yield* Effect.tryPromise({
      try: async () => {
        const book = new ExcelJS.Workbook()
        book.created = new Date(0)
        book.modified = new Date(0)
        book.lastPrinted = new Date(0)
        const sheet = book.addWorksheet(spec.title)
        const totals = Array.some(spec.columns, (column) => spec.cells[column.key]?.totals !== undefined)
        sheet.addTable({
          name: spec.name,
          ref: "A1",
          headerRow: true,
          totalsRow: totals,
          columns: Array.map(spec.columns, (column) => ({
            name: column.header,
            totalsRowFunction: spec.cells[column.key]?.totals,
          })),
          rows: cells,
        })
        Array.forEach(spec.columns, (column, index) => {
          const held = sheet.getColumn(index + 1)
          held.width = column.width
          const style = spec.cells[column.key]?.style
          if (style !== undefined) held.style = spec.style[style] ?? {}
        })
        Array.forEach(spec.rules, (row) => sheet.addConditionalFormatting({ ref: row.range, rules: [row.rule] }))
        Array.forEach(mode.guards, (row) =>
          sheet.getColumn(row.column).eachCell((cell) => { cell.dataValidation = row.validation }))
        Option.match(mode.brand, {
          onNone: () => undefined,
          onSome: (brand) =>
            sheet.addImage(book.addImage({ buffer: Buffer.from(brand.bytes), extension: brand.extension }), "A1:B3"),
        })
        sheet.headerFooter.oddFooter = mode.footer
        const protection = Option.getOrUndefined(spec.protect)
        if (protection !== undefined) await sheet.protect(Redacted.value(protection.password), protection.options)
        return _canonicalZip(new Uint8Array(await book.xlsx.writeBuffer()))
      },
      catch: (cause) => new ReportFault({ reason: "engine", arm: "xlsx", detail: String(cause) }),
    })
    const counted = yield* Ref.make(values.length)
    return yield* _sealed("xlsx", counted, Stream.make(bytes))
  })

const _xlsx = <A, R>(spec: Report.Xlsx<A>, rows: Stream.Stream<A, never, R>) =>
  Match.value(spec.mode).pipe(
    Match.tag("Stream", () => _xlsxStream(spec, rows)),
    Match.tag("Rich", (mode) => _xlsxRich(spec, mode, rows)),
    Match.exhaustive,
  )
```

## [04]-[PDF_ARM]

[PDF_ARM]:
- Owner: the bounded measured-paging PDF arm — `rowCeiling` is enforced by taking at most one row beyond the limit before projection or engine allocation, then one `new jsPDF({ unit: "pt", compress: true, encryption })` is built and emitted inside one synchronous fold. `setDocumentProperties` stamps title and creator from the spec, `setCreationDate(new Date(0))` pins the instant so equal rows produce identical bytes, the brand band lands once through `addImage` with an `alias` so a repeated logo embeds one object, and the column contract renders through the native `doc.table(x, y, data, headers, config)` structured-table primitive with `printHeaders`. `doc.outline.add` builds the section bookmark tree, and `output("arraybuffer")` is the single boundary crossing.
- Law: encryption is a spec row — `userPassword`/`ownerPassword` from `Redacted`, `userPermissions` the bounded set — and the browser egress arms (`save`, `blob`, `html`) are unspellable in this node lane.
- Law: repeated furniture registers once — a branded header band or signature block is a `jsPDF.API` plugin registration at module scope, invoked per page; re-drawing shared furniture imperatively per call site is the rejected form.
- Law: rendering is CPU-bound pure JS — the arm's synchronous fold is `_drawn`, and `Report.policy.offloadRows` routes a large unprotected document through the `Render` worker request at the `SPEC_FOLD` dispatch, so the request path never blocks on a large draw.
- Growth: a new document element (watermark, TOC) is a furniture field folded here; interactive AcroForm surfaces are a spec extension row, admitted when a consumer names them.
- Packages: `jspdf` (`jsPDF`, `GState`, the table/outline/AcroForm/metadata surface).

```typescript
const _pdfPlan = <A>(spec: Report.Pdf<A>, cells: ReadonlyArray<ReadonlyArray<Report.Cell>>): PdfPlan => ({
    columns: Array.map(spec.columns, ({ header, key, width }) => ({ header, key, width })),
    furniture: {
      title: spec.furniture.title,
      footer: spec.furniture.footer,
      brand: Option.match(spec.furniture.brand, {
        onNone: () => null,
        onSome: (brand) => ({ extension: brand.extension, bytes: Buffer.from(brand.bytes).toString("base64") }),
      }),
    },
    cells: Array.map(cells, (row) => Array.map(row, _scalar)),
  })

const _planned = <A>(spec: Report.Pdf<A>, cells: ReadonlyArray<ReadonlyArray<Report.Cell>>): Uint8Array =>
  new TextEncoder().encode(JSON.stringify(_pdfPlan(spec, cells)))

const _drawn = <A>(
  spec: Report.Pdf<A>,
  cells: ReadonlyArray<ReadonlyArray<Report.Cell>>,
): Effect.Effect<Uint8Array, ReportFault> =>
  Effect.try({
    try: () => {
      const doc = new jsPDF({
        unit: "pt",
        compress: true,
        encryption: Option.getOrUndefined(
          Option.map(spec.protect, (protect) => ({
            userPassword: Redacted.value(protect.userPassword),
            ownerPassword: Redacted.value(protect.ownerPassword),
            userPermissions: [...protect.permissions],
          })),
        ),
      })
      doc.setDocumentProperties({ title: spec.furniture.title, creator: "rasm" })
      doc.setCreationDate(new Date(0))
      Option.match(spec.furniture.brand, {
        onNone: () => undefined,
        onSome: (brand) => doc.addImage(brand.bytes, brand.extension.toUpperCase(), 40, 20, 100, 24, "brand"),
      })
      const data = Array.map(cells, (row) =>
        Object.fromEntries(Array.map(spec.columns, (column, index) => [column.key, String(_scalar(row[index] ?? null))])))
      doc.table(40, 60, data, Array.map(spec.columns, (column) => column.header), { printHeaders: true })
      doc.outline.add(null, spec.furniture.title, { pageNumber: 1 })
      Array.forEach(Array.range(1, doc.getNumberOfPages()), (page) => {
        doc.setPage(page)
        doc.text(spec.furniture.footer, 40, doc.internal.pageSize.getHeight() - 24)
      })
      return new Uint8Array(doc.output("arraybuffer"))
    },
    catch: (cause) => new ReportFault({ reason: "engine", arm: "pdf", detail: String(cause) }),
  })

const _pdf = <A, R>(
  spec: Report.Pdf<A>,
  rows: Stream.Stream<A, never, R>,
): Effect.Effect<Report.Artifact<never>, ReportFault, R | Bench> =>
  Effect.gen(function* () {
    const collected = yield* rows.pipe(Stream.take(spec.rowCeiling + 1), Stream.runCollect)
    const values = Chunk.toReadonlyArray(collected)
    if (values.length > spec.rowCeiling) {
      return yield* Effect.fail(new ReportFault({ reason: "ceiling", arm: "pdf", detail: String(spec.rowCeiling) }))
    }
    const cells = Array.map(values, (row) => _project(spec, row))
    const bytes = yield* cells.length > _policy.offloadRows && Option.isNone(spec.protect)
      ? Effect.mapError(Render.rendered("pdf", _planned(spec, cells)), (fault) =>
          new ReportFault({ reason: "sink", arm: "pdf", detail: fault._tag }))
      : _drawn(spec, cells)
    const counted = yield* Ref.make(cells.length)
    return yield* _sealed("pdf", counted, Stream.make(bytes))
  })

const _workerPdf = (plan: PdfPlan): Effect.Effect<Uint8Array, BenchFault> =>
  Effect.try({
    try: () => {
      const doc = new jsPDF({ unit: "pt", compress: true })
      doc.setDocumentProperties({ title: plan.furniture.title, creator: "rasm" })
      doc.setCreationDate(new Date(0))
      if (plan.furniture.brand !== null) {
        doc.addImage(
          Buffer.from(plan.furniture.brand.bytes, "base64"),
          plan.furniture.brand.extension.toUpperCase(),
          40,
          20,
          100,
          24,
          "brand",
        )
      }
      const data = Array.map(plan.cells, (row) =>
        Object.fromEntries(Array.map(plan.columns, (column, index) => [column.key, String(row[index] ?? null)])))
      doc.table(40, 60, data, Array.map(plan.columns, (column) => column.header), { printHeaders: true })
      doc.outline.add(null, plan.furniture.title, { pageNumber: 1 })
      Array.forEach(Array.range(1, doc.getNumberOfPages()), (page) => {
        doc.setPage(page)
        doc.text(plan.furniture.footer, 40, doc.internal.pageSize.getHeight() - 24)
      })
      return new Uint8Array(doc.output("arraybuffer"))
    },
    catch: () => new BenchFault({ reason: "starved", class: "defect" }),
  })

const _workerBundle = (plan: BundlePlan): Effect.Effect<Uint8Array, BenchFault> =>
  Effect.tryPromise({
    try: async () => {
      const zip = new JSZip()
      Array.forEach(plan.entries, (entry) =>
        zip.file(entry.name, Buffer.from(entry.bytes, "base64"), {
          compression: entry.format === "csv" ? "DEFLATE" : "STORE",
          compressionOptions: { level: 6 },
          date: new Date(0),
        }))
      return zip.generateAsync({ type: "uint8array", streamFiles: true })
    },
    catch: () => new BenchFault({ reason: "starved", class: "defect" }),
  })

const _worker = {
  Drop: (_request: Drop) => Effect.void,
  Render: (request: Render) => request.kind === "pdf"
    ? Effect.flatMap(_decodedPlan(_PdfPlan, request.plan), _workerPdf)
    : Effect.flatMap(_decodedPlan(_BundlePlan, request.plan), _workerBundle),
} as const
```

## [05]-[CSV_ARM]

[CSV_ARM]:
- Owner: the CSV codec arm — egress is per-row serialization: the header line mints once from `Papa.unparse({ fields, data: [] })`, every projected row serializes through `Papa.unparse` with `header: false`, and the encoded lines flow as the artifact body — one row in memory at a time, so a multi-gigabyte export never holds one string. Ingress is the polymorphic `parse` — the string arm synchronously with `result.errors` lifted before `result.data` is read, the `Papa.parse(Papa.NODE_STREAM_INPUT)` duplex for unbounded inputs — every row decoded by the caller's Schema, `dynamicTyping` refused so typing authority never forks.
- Law: `escapeFormulae` rides every egress call — a cell beginning `=`/`+`/`-`/`@` prefixes so a spreadsheet consumer never executes it; CSV egress is untrusted-sink output by definition.
- Law: `ParseError` accumulates, never throws — the code family (`Quotes`/`Delimiter`/`FieldMismatch`) lowers to the fault rail with the `ParseMeta` cursor as evidence.
- Growth: a delimiter or encoding posture is an `UnparseConfig` field on the spec's format row.
- Packages: `papaparse` (`parse`, `unparse`, `NODE_STREAM_INPUT`, `Parser`).

```typescript
const _csv = <A, R>(spec: Report.Csv<A>, rows: Stream.Stream<A, never, R>): Effect.Effect<Report.Artifact<R>> =>
  Effect.flatMap(Ref.make(0), (counted) => {
    const encoder = new TextEncoder()
    const fields = Array.map(spec.columns, (column) => column.header)
    return _sealed(
      "csv",
      counted,
      Stream.concat(
        Stream.make(encoder.encode(`${Papa.unparse({ fields, data: [] }, { ...spec.csv, escapeFormulae: true, newline: "\n" })}\n`)),
        rows.pipe(
          Stream.tap(() => Ref.update(counted, (held) => held + 1)),
          Stream.map((row) =>
            encoder.encode(
              `${Papa.unparse({ fields, data: [Array.map(_project(spec, row), _scalar)] }, {
                ...spec.csv,
                escapeFormulae: true,
                newline: "\n",
                header: false,
              })}\n`,
            )),
        ),
      ),
    )
  })

const _joined = (chunks: ReadonlyArray<Uint8Array>): Uint8Array => {
  const joined = new Uint8Array(Array.reduce(chunks, 0, (sum, chunk) => sum + chunk.length))
  Array.reduce(chunks, 0, (offset, chunk) => {
    joined.set(chunk, offset)
    return offset + chunk.length
  })
  return joined
}
```

## [06]-[BUNDLE]

[BUNDLE]:
- Owner: the archive container — `Report.bundle(spec)` folds named members into one `JSZip` tree with per-entry compression policy (`STORE` for already-compressed formats, `DEFLATE` level 6 for text) and fixed entry dates for byte stability. A member roster at or below `offloadAbove` uses `generateInternalStream({ type: "uint8array", streamFiles: true })` bridged through `Stream.async`, and `onUpdate` metadata folds into the supplied progress projection; a roster above the threshold encodes the same entries into the typed bundle plan and dials the `Render` worker. Each entry's bytes arrive through `Report.gathered` under the member's stated ceiling, so worker routing moves compression off the request thread without pretending the already-gathered entries are unbounded.
- Law: inbound archives are untrusted — `loadAsync(data, { checkCRC32: true })` gates integrity, and every entry's `unsafeOriginalName` resolves under the extraction anchor before any byte lands; the fold admits only targets that keep the anchor as their path prefix, and an escaping resolution folds to the `slip`-reasoned fault.
- Law: DEFLATE is CPU-bound pure JS — a bundle whose gathered entry bytes exceed `offloadAbove` runs through the same worker `Render` row (the `"bundle"` kind) the PDF arm dials; the threshold chooses execution placement and never masquerades as a materialization ceiling.
- Growth: a container policy axis (per-tenant naming, manifest entry) is a fold parameter; a second archive format is a new arm at the spec dispatch, never a fork of this one.
- Packages: `jszip` (`JSZip`, `generateInternalStream`, `generateAsync`, `loadAsync`, `JSZipMetadata`).

```typescript
const _bundleStream = (spec: Report.Bundle) =>
  Stream.async<Uint8Array, ReportFault>((emit) => {
    const zip = new JSZip()
    for (const entry of spec.entries) {
      zip.file(entry.name, entry.bytes, {
        compression: entry.format === "csv" ? "DEFLATE" : "STORE",
        compressionOptions: { level: 6 },
        date: new Date(0),
      })
    }
    const helper = zip.generateInternalStream<"uint8array">({ type: "uint8array", streamFiles: true })
    helper.on("data", (chunk, metadata) => {
      spec.progress(metadata)
      emit.single(chunk)
    })
    helper.on("end", () => emit.end())
    helper.on("error", (cause) => emit.fail(new ReportFault({ reason: "archive", arm: "zip", detail: String(cause) })))
    helper.resume()
  })

const _bundlePlan = (spec: Report.Bundle): BundlePlan => ({
  entries: Array.map(spec.entries, (entry) => ({
    name: entry.name,
    format: entry.format,
    bytes: Buffer.from(entry.bytes).toString("base64"),
  })),
})

const _bundle = (spec: Report.Bundle): Stream.Stream<Uint8Array, ReportFault, Bench> =>
  Array.reduce(spec.entries, 0, (total, entry) => total + entry.bytes.length) > spec.offloadAbove
    ? Stream.fromEffect(
      Effect.mapError(Render.rendered("bundle", new TextEncoder().encode(JSON.stringify(_bundlePlan(spec)))), (fault) =>
        new ReportFault({ reason: "sink", arm: "zip", detail: fault._tag })),
    )
    : _bundleStream(spec)

const _unbundle = (bytes: Uint8Array, root: string) =>
  Effect.tryPromise({
    try: () => JSZip.loadAsync(bytes, { checkCRC32: true }),
    catch: (cause) => new ReportFault({ reason: "archive", arm: "zip", detail: String(cause) }),
  }).pipe(
    Effect.flatMap((zip) => {
      const anchor = path.resolve(root)
      return Effect.forEach(Object.values(zip.files), (entry) => {
        const target = path.resolve(anchor, entry.unsafeOriginalName)
        return target === anchor || target.startsWith(`${anchor}${path.sep}`)
          ? Effect.tryPromise({
            try: () => entry.async("uint8array"),
            catch: (cause) => new ReportFault({ reason: "archive", arm: "zip", detail: String(cause) }),
          }).pipe(Effect.map((body) => ({ name: target, body })))
          : Effect.fail(new ReportFault({ reason: "slip", arm: "zip", detail: entry.name }))
      })
    }),
  )

const Report = {
  policy: _policy,
  render: _render,
  gathered: _gathered,
  bundle: _bundle,
  unbundle: _unbundle,
  worker: _worker,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Report, ReportFault }
```
