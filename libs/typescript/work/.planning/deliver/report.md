# [WORK_REPORT]

A report is a spec folded over decoded rows, and the output format is one policy row — never a forked pipeline: `Report.render(spec, rows, format)` dispatches one mapped handler record whose `csv` arm serializes through `Papa.unparse` with the formula-injection guard always on, whose `xlsx` arm folds the column contract into an in-memory `Workbook` and emits `writeBuffer` bytes, and whose `pdf` arm builds one `jsPDF` document inside a single synchronous fold with measured paging and the creation date pinned so equal rows produce byte-identical output. Unbounded rows take the streaming arm: the constant-memory `WorkbookWriter` under `Effect.acquireUseRelease`, each committed row leaving memory, `commit` as the release. Multi-artifact jobs bundle through one `JSZip` tree fold with compression as a generator policy value. Every terminal crosses the `Effect` boundary exactly once, every engine mutation dies inside its fold, and the produced `Uint8Array` is the shared deliver artifact — a mail attachment, a relay payload, a store object whose content key the kernel mint owns. A per-report imperative builder, a bare `await` on an engine promise, `dynamicTyping`, an unguarded CSV cell, and a DOM egress arm in this node lane are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                          |
| :-----: | :-------------- | :--------------------------------------------------------------------------------- |
|  [01]   | [FORMAT_ROWS]   | the report spec, the format vocabulary, the three render arms, the fault family     |
|  [02]   | [STREAM_BUNDLE] | the constant-memory spreadsheet stream and the archive bundle fold                  |

## [2]-[FORMAT_ROWS]

[FORMAT_ROWS]:
- Owner: `Report` — the document-egress owner. `Report.Spec` is the parameterization: a title, the column contract (`key`, `header`, `width` rows), and the pinned mint instant that makes bytes reproducible. Rows arrive as flat string records already decoded by the caller's Schema — the engines never type the interior, which is why `dynamicTyping` is refused on the CSV side and why every cell reaches an engine as a string the spec already proved. `Report.render(spec, rows, format)` is the one entry: the format literal indexes the handler record, and each arm returns the same `Uint8Array` currency.
- Law: the format rows carry the whole per-format policy — `csv` (`text/csv`, `escapeFormulae` unconditionally: an exported cell opening `=`/`+`/`-`/`@` is prefixed because CSV egress is untrusted-sink output), `xlsx` (its media type and the shared-strings toggle), `pdf` (unit, page format, margin, line height, compression on) — so a format difference is a column read, never a branch in a body.
- Law: the pdf fold is measured, not guessed — the fold seeds the cursor under the title, `splitTextToSize` wraps each joined row against the content width, and the page breaks when the measured height would cross `internal.pageSize.getHeight()` minus the margin — an `Array.reduce` over rows whose accumulator is the cursor, with `addPage` as the overflow arm inside the marked engine kernel; hardcoded line counts are the deleted spelling.
- Law: reproducibility is construction policy — `setCreationDate(spec.minted)` and `compress: true` pin the pdf bytes, the CSV and workbook folds are deterministic over their inputs, so one spec plus equal rows yields one content key and the artifact index dedupes re-renders; the key itself is minted by the kernel `ContentKey` owner at the store seam, never here.
- Law: the fault family follows the folder convention — `render` (an engine refused the fold; `invalid`, terminal), `stream` (the streaming sink failed mid-flight; `unavailable`, re-drives), `bundle` (the archive fold failed; `defect`) — deterministic render work never re-drives, which the class column states once.
- Exemption: the pdf and xlsx arms are marked engine kernels — the document builders are mutation-forced platform surfaces, statements live inside the `try` thunks, and only the detached `Uint8Array` leaves.
- Boundary: row decoding is the caller's Schema at its own seam; artifact transport is `deliver/mail.md`, `deliver/relay.md`, and the store object owner; XLSX re-projection of an existing worksheet is the only sanctioned `exceljs.csv` use, and standalone CSV is always this page's `papaparse` arm.
- Entry: `Report.render(spec, rows, "xlsx")` inside a `Step.run` body — a report is one durable activity keyed by its request, replayed from the recorded artifact after a crash.
- Growth: a new output format is one format row plus one handler arm; a new spec axis (a footer, a brand band) is one `Spec` field every arm reads.
- Packages: `exceljs` (`Workbook`), `jspdf` (`jsPDF`), `papaparse` (`Papa.unparse`), `effect` (`Array`, `DateTime`, `Effect`, `Schema`, `Types`), `@rasm/ts/kernel` (`FaultClass`).

```typescript
import ExcelJS from "exceljs"
import { jsPDF } from "jspdf"
import Papa from "papaparse"
import { type FaultClass } from "@rasm/ts/kernel"
import { Array, DateTime, Effect, Schema, type Types } from "effect"

const _formats = ["csv", "xlsx", "pdf"] as const
const _rows = {
  csv: { media: "text/csv", escape: true },
  xlsx: { media: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", shared: true },
  pdf: { media: "application/pdf", unit: "pt", page: "a4", margin: 40, line: 14 },
} as const

const _policy = {
  render: { class: "invalid" },
  stream: { class: "unavailable" },
  bundle: { class: "defect" },
} as const

class _Fault extends Schema.TaggedError<_Fault>()("ReportFault", {
  reason: Schema.Literal("render", "stream", "bundle"),
  title: Schema.String,
  detail: Schema.String,
}) {
  get policy(): (typeof _policy)[_Fault["reason"]] {
    return _policy[this.reason]
  }
  get class(): FaultClass.Kind {
    return _policy[this.reason].class
  }
  override get message(): string {
    return `<report:${this.reason}> ${this.title} ${this.detail}`
  }
}

declare namespace Report {
  type Format = keyof typeof _rows
  type Column = { readonly key: string; readonly header: string; readonly width: number }
  type Row = Record<string, string>
  type Spec = {
    readonly title: string
    readonly columns: Array.NonEmptyReadonlyArray<Column>
    readonly minted: DateTime.Utc
  }
  type Fault = _Fault
  type Reason = keyof typeof _policy
  type _Rows<T extends Record<Format, { readonly media: string }> = typeof _rows> = T
  type _Faults<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _policy> = T
}

const _utf8 = new TextEncoder()

const _faulted = (reason: Report.Reason, spec: Report.Spec) => (cause: unknown): _Fault =>
  new _Fault({ reason, title: spec.title, detail: String(cause) })

const _cells = (spec: Report.Spec, row: Report.Row): ReadonlyArray<string> =>
  Array.map(spec.columns, (column) => row[column.key] ?? "")

const _csv = (spec: Report.Spec, rows: ReadonlyArray<Report.Row>): Effect.Effect<Uint8Array, _Fault> =>
  Effect.try({
    try: () =>
      _utf8.encode(
        Papa.unparse(
          {
            fields: Array.map(spec.columns, (column) => column.header),
            data: Array.map(rows, (row) => [..._cells(spec, row)]),
          },
          { escapeFormulae: _rows.csv.escape },
        ),
      ),
    catch: _faulted("render", spec),
  })

const _xlsx = (spec: Report.Spec, rows: ReadonlyArray<Report.Row>): Effect.Effect<Uint8Array, _Fault> =>
  Effect.tryPromise({
    try: async () => {
      const workbook = new ExcelJS.Workbook()
      const sheet = workbook.addWorksheet(spec.title)
      sheet.columns = spec.columns.map((column) => ({ header: column.header, key: column.key, width: column.width }))
      sheet.addRows(rows.map((row) => ({ ...row })))
      return new Uint8Array(await workbook.xlsx.writeBuffer())
    },
    catch: _faulted("render", spec),
  })

const _pdf = (spec: Report.Spec, rows: ReadonlyArray<Report.Row>): Effect.Effect<Uint8Array, _Fault> =>
  Effect.try({
    try: () => {
      const document = new jsPDF({ unit: _rows.pdf.unit, format: _rows.pdf.page, compress: true })
      document.setCreationDate(new Date(DateTime.formatIso(spec.minted)))
      document.setFontSize(10)
      document.text(spec.title, _rows.pdf.margin, _rows.pdf.margin)
      const width = document.internal.pageSize.getWidth() - _rows.pdf.margin * 2
      const floor = document.internal.pageSize.getHeight() - _rows.pdf.margin
      Array.reduce(rows, _rows.pdf.margin + _rows.pdf.line, (cursor, row) => {
        const lines: ReadonlyArray<string> = document.splitTextToSize(_cells(spec, row).join("  "), width)
        const overflow = cursor + lines.length * _rows.pdf.line > floor
        if (overflow) document.addPage()
        const opened = overflow ? _rows.pdf.margin : cursor
        document.text([...lines], _rows.pdf.margin, opened + _rows.pdf.line)
        return opened + lines.length * _rows.pdf.line
      })
      return new Uint8Array(document.output("arraybuffer"))
    },
    catch: _faulted("render", spec),
  })

const _RENDER: {
  readonly [K in Report.Format]: (spec: Report.Spec, rows: ReadonlyArray<Report.Row>) => Effect.Effect<Uint8Array, _Fault>
} = { csv: _csv, xlsx: _xlsx, pdf: _pdf }

const _render = (
  spec: Report.Spec,
  rows: ReadonlyArray<Report.Row>,
  format: Report.Format,
): Effect.Effect<Uint8Array, _Fault> => _RENDER[format](spec, rows)
```

## [3]-[STREAM_BUNDLE]

[STREAM_BUNDLE]:
- Owner: the unbounded arm and the container fold, assembled with the render dispatch into the one exported `Report`. `Report.stream(spec, rows, target)` drains a row `Stream` through the constant-memory `WorkbookWriter`: construction against the target path is the acquisition, every element lands as `addRow(row).commit()` so memory holds one row, and `workbook.commit()` is the release that flushes shared strings and the zip directory — teardown running on success, failure, and interruption alike. `Report.bundle(spec, parts)` folds named artifacts into one `JSZip` tree and serializes once with `generateAsync` pinned to `uint8array` under the `DEFLATE` policy row.
- Law: the streaming writer is the default past the in-memory ceiling — a million-row export holds constant memory while the in-memory `Workbook` arm stays for small, style-rich documents; the split is the caller's row-bound knowledge, and the fault classifies `stream` so a torn sink re-drives under the kernel gate while a render refusal never does.
- Law: compression is a policy value — `{ compression: "DEFLATE", compressionOptions: { level: 6 } }` on the generator options, never a code path per codec; DEFLATE is CPU-bound pure JS, so a large bundle belongs inside a `bulk`-budget step, off any interactive path.
- Law: the bundle is a container, never a producer — parts arrive as already-rendered `{ name, bytes }` values from the format arms, the tree fold runs inside the single `generateAsync` crossing, and no archive state leaks past it.
- Law: a release failure is a defect — the writer's `commit` resolves through `Effect.orDie` because the primary outcome must survive teardown, the release-channel law applied to the flushing writer.
- Boundary: writing the artifact to durable storage and minting its content key are the store object owner's; inbound archive reading (zip-slip validation, lazy entry access) is an ingress concern no deliver page owns.
- Entry: `Report.stream(spec, rows, { path })` for unbounded exports; `Report.bundle(spec, parts)` for multi-artifact jobs.
- Growth: a new stream sink modality is one `target` field; a new container policy axis is one generator-option field.
- Packages: `exceljs` (`stream.xlsx.WorkbookWriter`), `jszip` (`JSZip`), `effect` (`Effect`, `Stream`).

```typescript
import JSZip from "jszip"
import { Stream } from "effect"

const _stream = <E, R>(
  spec: Report.Spec,
  rows: Stream.Stream<Report.Row, E, R>,
  target: { readonly path: string },
): Effect.Effect<void, E | _Fault, R> =>
  Effect.acquireUseRelease(
    Effect.try({
      try: () => {
        const writer = new ExcelJS.stream.xlsx.WorkbookWriter({
          filename: target.path,
          useSharedStrings: _rows.xlsx.shared,
        })
        const sheet = writer.addWorksheet(spec.title)
        sheet.columns = spec.columns.map((column) => ({ header: column.header, key: column.key, width: column.width }))
        return { writer, sheet }
      },
      catch: _faulted("stream", spec),
    }),
    ({ sheet }) =>
      Stream.runForEach(rows, (row) =>
        Effect.try({ try: () => void sheet.addRow({ ...row }).commit(), catch: _faulted("stream", spec) })),
    ({ writer }) => Effect.orDie(Effect.tryPromise({ try: () => writer.commit(), catch: _faulted("stream", spec) })),
  )

const _bundle = (
  spec: Report.Spec,
  parts: Array.NonEmptyReadonlyArray<{ readonly name: string; readonly bytes: Uint8Array }>,
): Effect.Effect<Uint8Array, _Fault> =>
  Effect.tryPromise({
    try: () =>
      Array.reduce(parts, new JSZip(), (tree, part) => tree.file(part.name, part.bytes)).generateAsync({
        type: "uint8array",
        compression: "DEFLATE",
        compressionOptions: { level: 6 },
      }),
    catch: _faulted("bundle", spec),
  })

declare namespace Report {
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly formats: typeof _formats
      readonly render: typeof _render
      readonly stream: typeof _stream
      readonly bundle: typeof _bundle
    }
  >
}

const Report: Report.Shape = { ..._rows, formats: _formats, render: _render, stream: _stream, bundle: _bundle }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Report }
```
