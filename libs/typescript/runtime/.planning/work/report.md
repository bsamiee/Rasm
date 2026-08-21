# [RUNTIME_REPORT]

Document egress as one folded `Report.Spec` family: the format discriminant selects CSV, XLSX, or PDF while each column owns its value projection, and every render answers one single-subscription `Report.Artifact` with a chunked body and a settled receipt that drain fills. One closed modality roster — CSV, XLSX, PDF, ZIP — anchors the spec discriminant, the fault's arm column, the archive plan's entry column, and the off-thread request kind, and one row table beside it owns per-modality placement and compression. Mutation-heavy Promise and synchronous engines remain inside one `Effect` boundary and one `ReportFault` family. Unbounded CSV and streaming-XLSX modes flow end to end; rich XLSX and PDF declare row ceilings before materializing the engine model, with oversized PDF routed to the render worker. `Report.gathered` is the only bytes-in-memory consumer fold and requires a ceiling. Pinned instants, fixed compression, and stable column order make equal renders byte-stable so the data object plane mints identical content identity at landing; runtime never mints that identity. The module is node-lane egress on `./server` as `runtime/src/work/report.ts`.

## [01]-[INDEX]

- [02]-[SPEC_FOLD]: the modality roster and its row table, the report spec, the one render dispatch, the settled receipt, byte identity; `Report`.
- [03]-[XLSX_ARM]: the streaming workbook writer, the name-capable amend-load reader, the full style/rule/validation vocabularies; `Report`.
- [04]-[PDF_ARM]: measured paging, native tables, metadata/encryption, furniture registration; `Report`.
- [05]-[CSV_ARM]: serializer with formula defense, the node streaming duplex, decoded ingress; `Report`.
- [06]-[BUNDLE]: the archive container — streaming egress, progress receipt, guarded ingress; `Report`.

## [02]-[SPEC_FOLD]

[SPEC_FOLD]:
- Owner: `Report.Spec` — a format-discriminated family over one base. Every base `columns` row owns header, key, width, and value projection, so column order and row projection cannot drift; the `Xlsx` arm carries one payload-timing discriminant whose `Rich` case alone owns validation, brand, footer, and row ceiling, while style, keyed cell policy, rules, protection, and title remain valid for both modes; the `Pdf` arm carries furniture, protection, and its materialization ceiling; and the `Csv` arm carries `UnparseConfig`. `Report.Artifact.body` is single-subscription by `Ref.getAndSet`, and its receipt settles on success, failure, or interrupted drain.
- Law: rows arrive decoded — the caller's Schema owns row typing and the render fold receives typed values; no arm re-validates, and the CSV arm's refusal of engine-side typing is this law's engine-level echo.
- Law: materialization is a consumer fold under a stated ceiling — `Report.gathered(artifact, ceiling)` is the ONE bytes-in-memory form, faulting `ceiling`-reasoned (`exhausted` class) the moment the running total passes the bound, so an unbounded body structurally cannot buffer whole; a consumer calling it attests its bound (a mail attachment cap, a bundle entry cap) at the call.
- Law: bytes are identity material minted where they land — reproducibility (pinned instants, fixed compression, stable column order) is a correctness requirement because the data wave's artifact-index put mints the content key over the landed bytes and dedupes equal renders; runtime never mints content identity, a defaulted creation date in any arm is the named defect, and a replay under an equal spec regenerates byte-identical output.
- Law: a render is a durable step — the relay and the job families run `Report.render` inside `Step.run(name, "bulk", …)`, so deadline geometry, replay memoization, and evidence arrive from the flow mint and this page owns none of them.
- Law: placement is a modality row, never a caller knob — the row states the threshold, the unit it measures, and the `proc/worker` request kind together, so an arm with no `offload` column cannot reach the pool and the two thresholds that stood as a module constant and a bundle field are one column. A `pdf` fold whose bounded projected cell set passes that threshold routes through the `Render` request: the data-only plan (columns, furniture, projected cells) encodes to bytes, crosses zero-copy, and the produced bytes cross back; a protected document renders in-process regardless, because a sealed password never crosses the thread seam — the one exemption to worker routing, never to the row ceiling.
- Receipt: `Report.Rendered` — `entity#SETTLED_RECEIPT`'s spine carrying this producer's own `{ rows, bytes, format }` evidence — settles through the sealed body's `Deferred` when the last chunk flows, the evidence the meter fact and the artifact index consume; a receipt read before the body drains simply waits. The partition reads `empty` on a zero-row render and `whole` otherwise, provenance names the spec, and the content key stays absent because runtime mints no content identity.
- Growth: a new format is one arm behind the same dispatch; a new visual concern is a spec field every arm interprets or ignores by declaration.
- Packages: `effect` (`Effect`, `Stream`, `Duration`, `Deferred`, `Ref`, `Clock`, `DateTime`); `../proc/worker.ts` (`Bench`, `Render` — the off-thread crossing); `./entity.ts` (`Settled` — the receipt spine).

```typescript signature
// named-only tree: the package ships no default export, so the namespace import is the one admissible spelling
import * as ExcelJS from "exceljs"
import { jsPDF } from "jspdf"
import JSZip from "jszip"
import Papa from "papaparse"
import { Buffer } from "node:buffer"
import path from "node:path"
import { PassThrough, Transform } from "node:stream"
import { Array, Chunk, Clock, DateTime, Deferred, Duration, Effect, Match, Option, Redacted, Ref, Schema, Stream } from "effect"
import { Fault } from "@rasm/ts/core"
import { Bench, BenchFault, Drop, Render } from "../proc/worker.ts"
import { Settled } from "./entity.ts"

// ONE closed production-modality roster. Four spellings of this set stood apart before — the spec union's `format`
// field, the fault's own `arm` literal, the worker request's `kind`, and the bundle plan's entry column — and each
// one now reads this anchor, so a fifth modality lands as one tuple member and every table breaks at compile time.
const _MODALITIES = ["csv", "xlsx", "pdf", "zip"] as const
const _Format = Schema.Literal(..._MODALITIES)
const _Measure = Schema.Literal("bytes", "rows")

// The per-case policy that stood as loose constants and hand-written ternaries.
//  `offload` is the WHOLE off-thread declaration as one option — the measured threshold, the unit it measures, and
//  the `proc/worker` request kind that crossing carries — so the three arrive together or not at all and an arm
//  cannot dial a pool the roster gave it no request kind for. It collapses the module-wide `offloadRows` constant
//  and the caller-supplied bundle threshold onto one column, because placement is this owner's policy: the caller
//  states a materialization ceiling on its own spec, never where the work runs.
//  `compression` is the archive policy a member of this modality takes inside a bundle — already-compressed
//  containers store, text deflates — read at both the streaming and the off-thread archive folds.
const _MODALITY: { readonly [K in Report.Format]: Report.Modality } = {
  csv: { compression: "DEFLATE", offload: Option.none() },
  xlsx: { compression: "STORE", offload: Option.none() },
  pdf: { compression: "STORE", offload: Option.some({ above: 50_000, kind: "pdf", unit: "rows" }) },
  zip: { compression: "STORE", offload: Option.some({ above: 8_388_608, kind: "zip", unit: "bytes" }) },
}

// Every reason's subject names the ARM it refused on, closed against the modality roster, so a fault carries a
// modality a fold reads back rather than a free word beside a free string. The two structural refusals declare their
// own coordinates instead of rendering them into text: a breached bound carries the number and the unit it measured,
// and an escaping archive entry carries the path and the anchor it left.
const _Refused = Schema.Struct({ arm: _Format, detail: Schema.String })

const _family = Fault.Class.family(["engine", "sink", "archive", "slip", "ceiling", "consumed"] as const, {
  engine: Fault.Class.row({
    class: "defect",
    leg: "render",
    detail: _Refused,
    render: ({ arm, detail }) => `the ${arm} engine refused — ${detail}`,
  }),
  sink: Fault.Class.row({
    class: "unavailable",
    leg: "body",
    detail: _Refused,
    render: ({ arm, detail }) => `the ${arm} body stopped flowing — ${detail}`,
  }),
  archive: Fault.Class.row({
    class: "defect",
    leg: "archive",
    detail: _Refused,
    render: ({ arm, detail }) => `the ${arm} container refused — ${detail}`,
  }),
  slip: Fault.Class.row({
    class: "malformed",
    leg: "archive",
    detail: Schema.Struct({ anchor: Schema.NonEmptyString, entry: Schema.NonEmptyString }),
    render: ({ anchor, entry }) => `archive entry ${entry} resolves outside the extraction anchor ${anchor}`,
  }),
  ceiling: Fault.Class.row({
    class: "exhausted",
    leg: "ceiling",
    detail: Schema.Struct({ arm: _Format, bound: Schema.Int, unit: _Measure }),
    render: ({ arm, bound, unit }) => `the ${arm} arm was handed more than ${bound} ${unit}`,
  }),
  consumed: Fault.Class.row({
    class: "conflicted",
    leg: "body",
    detail: Schema.Struct({ arm: _Format }),
    render: ({ arm }) => `the ${arm} body is single-subscription and already drained`,
  }),
})

class ReportFault extends Schema.TaggedError<ReportFault>()("ReportFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

// The settled-work spine with this producer's own evidence column: rows and bytes are what a meter fact and an
// artifact index read, and the stamp pair, the concern partition, the provenance join, and the warning band arrive
// from `entity#SETTLED_RECEIPT` rather than being restated as a second receipt vocabulary no consumer could join.
class Rendered extends Settled.extend<Rendered>("Report.Rendered")({
  evidence: Schema.Struct({ bytes: Schema.Int, format: _Format, rows: Schema.Int }),
}) {}

// The placement read both offloadable arms take: the row's own threshold against the arm's own measure, answering the
// request kind that crossing carries and nothing wherever the work stays on this thread.
const _crossing = (arm: Report.Format, measured: number): Option.Option<Bench.Kind> =>
  Option.map(Option.filter(_MODALITY[arm].offload, (row) => measured > row.above), (row) => row.kind)

// The worker's own faults, the wire decode, and the pool transport all answer `message`, so one projection re-keys
// every crossing refusal onto the arm that dialled it and no site spells the tag alone.
const _sank = (arm: Report.Format) => (fault: { readonly message: string }): ReportFault =>
  new ReportFault({ case: { reason: "sink", arm, detail: fault.message } })

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
  type Format = (typeof _MODALITIES)[number]
  type Measure = typeof _Measure.Type
  type Modality = {
    readonly compression: "DEFLATE" | "STORE"
    readonly offload: Option.Option<{ readonly above: number; readonly kind: Bench.Kind; readonly unit: Measure }>
  }
  // The row-fed subset: `zip` folds an already-gathered entry roster and takes no row stream, so the render dispatch
  // is total over exactly the modalities a stream can drive and the archive keeps its own entry.
  type Fed = Spec<never>["format"]
  type Rendered = InstanceType<typeof Rendered>
  type Artifact<R> = {
    readonly format: Format
    readonly body: Stream.Stream<Uint8Array, ReportFault, R>
    readonly receipt: Effect.Effect<Rendered, ReportFault>
  }
  type Sheet = {
    readonly ordinal: number
    readonly name: Option.Option<string>
    readonly state: Option.Option<ExcelJS.WorksheetState>
  }
  type Bundle = {
    readonly entries: ReadonlyArray<{ readonly name: string; readonly format: Format; readonly bytes: Uint8Array }>
    readonly progress: (metadata: JSZip.JSZipMetadata) => void
  }
}

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
  entries: Schema.Array(Schema.Struct({ name: Schema.String, format: _Format, bytes: Schema.String })),
})

type PdfPlan = Schema.Schema.Type<typeof _PdfPlan>
type BundlePlan = Schema.Schema.Type<typeof _BundlePlan>

// The worker-side plan decode: `class` is DERIVED at the mint, so this refusal states its reason and its evidence and
// cannot stamp a class the reason contradicts; the parse diagnostic that was discarded whole rides the row's subject.
const _decodedPlan = <A, I>(schema: Schema.Schema<A, I>, kind: Bench.Kind, bytes: Uint8Array): Effect.Effect<A, BenchFault> =>
  Schema.decodeUnknown(Schema.parseJson(schema))(new TextDecoder().decode(bytes)).pipe(
    Effect.mapError((issue) =>
      new BenchFault({ case: { reason: "refused", request: "Render", detail: `${kind} plan — ${String(issue)}` } })),
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
  name: string,
  counted: Ref.Ref<number>,
  body: Stream.Stream<Uint8Array, ReportFault, R>,
): Effect.Effect<Report.Artifact<R>> =>
  Effect.gen(function* () {
    const settled = yield* Deferred.make<Report.Rendered, ReportFault>()
    const size = yield* Ref.make(0)
    const openedOnce = yield* Ref.make(false)
    const opened = yield* Clock.currentTimeMillis
    return {
      format,
      receipt: Deferred.await(settled),
      body: Stream.unwrap(
        Effect.map(Ref.getAndSet(openedOnce, true), (replayed) =>
          replayed
            ? Stream.fail(new ReportFault({ case: { reason: "consumed", arm: format } }))
            : body.pipe(
              Stream.tap((chunk) => Ref.update(size, (held) => held + chunk.length)),
              Stream.tapError((fault) => Deferred.fail(settled, fault)),
              Stream.onDone(() =>
                Effect.gen(function* () {
                  const closed = yield* Clock.currentTimeMillis
                  const rows = yield* Ref.get(counted)
                  yield* Deferred.succeed(
                    settled,
                    new Rendered({
                      evidence: { bytes: yield* Ref.get(size), format, rows },
                      // A render either wrote its whole declared column set or wrote nothing at all: this producer
                      // separates no partial landing, so it states `whole` and forfeits the band rather than minting
                      // a fourth word, and a zero-row render is the `empty` arm the spine already names.
                      partition: rows === 0 ? "empty" : "whole",
                      // Runtime mints no content identity, so the produced id is the spec's own name and the data
                      // wave's artifact-index put is what mints a key over the landed bytes.
                      provenance: { consumed: [], produced: name },
                      warnings: [],
                      at: yield* DateTime.now,
                      span: Duration.millis(closed - opened),
                    }),
                  )
                })),
              Stream.ensuring(
                Effect.flatMap(Deferred.isDone(settled), (done) =>
                  done
                    ? Effect.void
                    : Effect.asVoid(Deferred.fail(settled, new ReportFault({
                      case: { reason: "sink", arm: format, detail: "the drain was interrupted before the last chunk" },
                    }))),
                ),
              ),
            )),
      ),
    }
  })

// The render dispatch is the roster's own mapped record over the row-fed cases: a modality without an arm and an arm
// without a modality are both compile errors at this declaration, where the `Match.when` ladder over a foreign
// `format` field proved neither and spent an `exhaustive` check re-deriving what the union already stated. Each cell
// is an arrow rather than a bare reference because the arms are declared in their own sections below this one, so
// the table names them at CALL time and its declaration order stays the section order.
const _arms: {
  readonly [K in Report.Fed]: <A, R>(
    spec: Extract<Report.Spec<A>, { readonly format: K }>,
    rows: Stream.Stream<A, never, R>,
  ) => Effect.Effect<Report.Artifact<R>, ReportFault, R | Bench>
} = {
  csv: (spec, rows) => _csv(spec, rows),
  pdf: (spec, rows) => _pdf(spec, rows),
  xlsx: (spec, rows) => _xlsx(spec, rows),
}

const _render = <A, R>(
  spec: Report.Spec<A>,
  rows: Stream.Stream<A, never, R>,
): Effect.Effect<Report.Artifact<R>, ReportFault, R | Bench> => _arms[spec.format](spec as never, rows)

const _gathered = <R>(artifact: Report.Artifact<R>, ceiling: number): Effect.Effect<Uint8Array, ReportFault, R> =>
  artifact.body.pipe(
    Stream.runFoldEffect({ held: Chunk.empty<Uint8Array>(), total: 0 }, (state, chunk) =>
      state.total + chunk.length > ceiling
        ? Effect.fail(new ReportFault({ case: { reason: "ceiling", arm: artifact.format, bound: ceiling, unit: "bytes" } }))
        : Effect.succeed({ held: Chunk.append(state.held, chunk), total: state.total + chunk.length })),
    Effect.map((state) => _joined(Chunk.toReadonlyArray(state.held))),
  )
```

## [03]-[XLSX_ARM]

[XLSX_ARM]:
- Owner: the spreadsheet arm has one discriminated policy with two honest payload timings. `Stream` uses `ExcelJS.stream.xlsx.WorkbookWriter`, commits each projected row, canonicalizes ZIP entry timestamps across arbitrary chunk boundaries, and emits compressed chunks end to end; it carries title, column styles, conditional rules, and protection only. `Rich({ rowCeiling, guards, brand, footer })` admits at most the declared row bound, builds one `Workbook`, canonicalizes its completed archive, and integrates the full vocabulary the streaming writer cannot apply after committed cells: native tables and totals, data validation, brand image, footer, column styles, conditional rules, and protection. The modes share the same `Report.Xlsx` owner and `_xlsx` dispatch; rich-only fields cannot inhabit the streaming case as inert option ghosts.
- Law: the nine-arm `ConditionalFormattingRule` union, the `DataValidation` operator space, and the `Style` composite are the parameterization vocabulary — a report that needs a data bar, an icon set, or a dropdown names a rule row; an imperative per-report formatting branch is unspellable.
- Law: amend-load ingress rides the symmetric reader — `Report.amend(input, options)` mints `new ExcelJS.stream.xlsx.WorkbookReader(input, options)` and lifts its worksheet generator through `Stream.fromAsyncIterable`, each `WorksheetReader` flattening into its own row generator, so an append-and-re-emit job reads a stored artifact one row at a time and the sheet never materializes; the in-memory `workbook.xlsx.load` is reserved for small template loads.
- Law: the reader's per-part policy is the ingress's memory contract, not a default — `worksheets: "emit"` streams the rows, `sharedStrings: "cache"` and `styles: "cache"` retain exactly what a row needs to resolve its text and format, and every other part stays `ignore`; caching a part no row reads holds a huge stored artifact on the heap for the length of the read, and emitting shared strings hands the caller a part it must reassemble itself.
- Law: the amend coordinate is name-capable — `Report.Sheet` carries the read `ordinal` beside an `Option`-carried `name` and `state`, so a job addresses a stored artifact by the sheet the workbook declares and falls back to position only where no name exists; the reader seats a synthesized `Sheet<id>` placeholder at construction and the workbook registry overwrites `id`, `name`, and `state` together only when a rel resolves the zip entry, so the parsed numeric `sheetId` left behind is the one sound proof the declared name landed and an unresolved read answers `Option.none()` rather than handing a caller a placeholder to filter on.
- Law: that coordinate is a declaration gap the augmentation closes — the shipped `WorksheetReader` declares none of the three members its constructor and the registry match both assign, so one `declare module "exceljs"` block beside this engine declares them at their verified runtime types, `id` widening to `number | string` because the unresolved path keeps the zip path's captured digits; every downstream read composes the corrected surface and no call site casts.
- Law: `.csv` on the workbook facade defers to the CSV arm — `exceljs.csv` exists only to re-project an already-built `Worksheet`.
- Law: the body streams end to end — the `PassThrough` sink bridges into `Stream.asyncScoped` (subscribe on acquire, `destroy` on release, `emit.single`/`emit.end`/`emit.fail` the admitted crossings), and the commit driver runs as a scope-forked fiber feeding the writer while the consumer drains chunks, so compressed output leaves memory as it is produced and a chunk array never accumulates.
- Exemption: the `PassThrough` event callbacks are the platform-forced statement seam — the writer mutates a node stream outside the rail, and the bridge's listeners are the one sanctioned push-crossing site in this module.
- Growth: a new formatting capability is a spec field mapped to its vocabulary row in this one fold.
- Packages: `exceljs` (`Workbook`, `stream.xlsx.WorkbookWriter`, `stream.xlsx.WorkbookReader`, `WorkbookStreamReaderOptions`, `WorksheetReader` with the locally declared `id`/`name`/`state` coordinate, `WorksheetState`, the `Style`/`Table`/`ConditionalFormattingRule`/`DataValidation` model); `effect` (`Stream.fromAsyncIterable`, `Stream.zipWithIndex`, `Option.fromNullable`).

```typescript signature
// the three members the shipped `WorksheetReader` omits, each traced to the machine that installs it: the
// constructor assigns `id` from the zip path and `name` as the `Sheet<id>` placeholder, and the workbook
// registry match reassigns `id`, `name`, and `state` from the sheet row its rel resolves
declare module "exceljs" {
  namespace stream.xlsx {
    interface WorksheetReader {
      readonly id: number | string
      readonly name: string
      readonly state?: WorksheetState
    }
  }
}

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
          catch: (cause) => new ReportFault({ case: { reason: "engine", arm: "xlsx", detail: String(cause) } }),
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
      catch: (cause) => new ReportFault({ case: { reason: "engine", arm: "xlsx", detail: String(cause) } }),
    })
  })

const _xlsxStream = <A, R>(spec: Report.Xlsx<A>, rows: Stream.Stream<A, never, R>): Effect.Effect<Report.Artifact<R>> =>
  Effect.flatMap(Ref.make(0), (counted) =>
    _sealed(
      "xlsx",
      spec.name,
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
                void emit.fail(new ReportFault({ case: { reason: "sink", arm: "xlsx", detail: String(cause) } })))
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
      return yield* Effect.fail(new ReportFault({ case: { reason: "ceiling", arm: "xlsx", bound: mode.rowCeiling, unit: "rows" } }))
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
      catch: (cause) => new ReportFault({ case: { reason: "engine", arm: "xlsx", detail: String(cause) } }),
    })
    const counted = yield* Ref.make(values.length)
    return yield* _sealed("xlsx", spec.name, counted, Stream.make(bytes))
  })

const _xlsx = <A, R>(spec: Report.Xlsx<A>, rows: Stream.Stream<A, never, R>) =>
  Match.value(spec.mode).pipe(
    Match.tag("Stream", () => _xlsxStream(spec, rows)),
    Match.tag("Rich", (mode) => _xlsxRich(spec, mode, rows)),
    Match.exhaustive,
  )

const _amended = (
  input: string | NodeJS.ReadableStream,
  options: Partial<ExcelJS.stream.xlsx.WorkbookStreamReaderOptions> = {
    worksheets: "emit",
    sharedStrings: "cache",
    styles: "cache",
  },
): Stream.Stream<{ readonly sheet: Report.Sheet; readonly row: ExcelJS.Row }, ReportFault> =>
  Stream.unwrap(
    Effect.map(
      Effect.sync(() => new ExcelJS.stream.xlsx.WorkbookReader(input, options)),
      // two nested generators, one lift each: the workbook yields sheet readers and each sheet reader yields rows,
      // so a stored artifact of any size crosses the seam a row at a time and never lands on the heap whole
      (reader) =>
        Stream.flatMap(
          Stream.map(
            Stream.zipWithIndex(
              Stream.fromAsyncIterable(reader, (cause) =>
                new ReportFault({ case: { reason: "engine", arm: "xlsx", detail: String(cause) } })),
            ),
            // the coordinate mints once per sheet, never per row; the registry match replaced the constructor's
            // captured zip digits with the workbook's parsed `sheetId`, so a numeric `id` proves the declared name
            // landed and a surviving string leaves the ordinal as the only coordinate the read can honestly offer
            ([held, ordinal]): readonly [ExcelJS.stream.xlsx.WorksheetReader, Report.Sheet] => [
              held,
              {
                ordinal,
                name: typeof held.id === "number" ? Option.some(held.name) : Option.none(),
                state: Option.fromNullable(held.state),
              },
            ],
          ),
          ([held, sheet]) =>
            Stream.map(
              Stream.fromAsyncIterable(held, (cause) =>
                new ReportFault({ case: { reason: "engine", arm: "xlsx", detail: String(cause) } })),
              (row) => ({ sheet, row }),
            ),
        ),
    ),
  )
```

## [04]-[PDF_ARM]

[PDF_ARM]:
- Owner: the bounded measured-paging PDF arm — `rowCeiling` is enforced by taking at most one row beyond the limit before projection or engine allocation, then one `new jsPDF({ unit: "pt", compress: true, encryption })` is built and emitted inside one synchronous fold. `setDocumentProperties` stamps title and creator from the spec, `setCreationDate(new Date(0))` pins the instant so equal rows produce identical bytes, the brand band lands once through `addImage` with an `alias` so a repeated logo embeds one object, and the column contract renders through the native `doc.table(x, y, data, headers, config)` structured-table primitive with `printHeaders`. `doc.outline.add` builds the section bookmark tree, and `output("arraybuffer")` is the single boundary crossing.
- Law: encryption is a spec row — `userPassword`/`ownerPassword` from `Redacted`, `userPermissions` the bounded set — and the browser egress arms (`save`, `blob`, `html`) are unspellable in this node lane.
- Law: repeated furniture registers once — a branded header band or signature block is a `jsPDF.API` plugin registration at module scope, invoked per page; re-drawing shared furniture imperatively per call site is the rejected form.
- Law: rendering is CPU-bound pure JS — the arm's synchronous fold is `_drawn`, and the `pdf` modality row routes a large unprotected document through the `Render` worker request, so the request path never blocks on a large draw.
- Growth: a new document element (watermark, TOC) is a furniture field folded here; interactive AcroForm surfaces are a spec extension row, admitted when a consumer names them.
- Packages: `jspdf` (`jsPDF`, `GState`, the table/outline/AcroForm/metadata surface).

```typescript signature
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
    catch: (cause) => new ReportFault({ case: { reason: "engine", arm: "pdf", detail: String(cause) } }),
  })

const _pdf = <A, R>(
  spec: Report.Pdf<A>,
  rows: Stream.Stream<A, never, R>,
): Effect.Effect<Report.Artifact<never>, ReportFault, R | Bench> =>
  Effect.gen(function* () {
    const collected = yield* rows.pipe(Stream.take(spec.rowCeiling + 1), Stream.runCollect)
    const values = Chunk.toReadonlyArray(collected)
    if (values.length > spec.rowCeiling) {
      return yield* Effect.fail(new ReportFault({ case: { reason: "ceiling", arm: "pdf", bound: spec.rowCeiling, unit: "rows" } }))
    }
    const cells = Array.map(values, (row) => _project(spec, row))
    // A sealed password never crosses the thread seam, so protection withdraws the crossing rather than lowering the
    // threshold: the row still owns placement, and the exemption is stated where it is taken.
    const bytes = yield* Option.match(Option.isNone(spec.protect) ? _crossing("pdf", cells.length) : Option.none(), {
      onNone: () => _drawn(spec, cells),
      onSome: (kind) => Effect.mapError(Render.rendered(kind, _planned(spec, cells)), _sank("pdf")),
    })
    const counted = yield* Ref.make(cells.length)
    return yield* _sealed("pdf", spec.name, counted, Stream.make(bytes))
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
    catch: (cause) => new BenchFault({ case: { reason: "starved", request: "Render", detail: `pdf draw — ${String(cause)}` } }),
  })

const _workerBundle = (plan: BundlePlan): Effect.Effect<Uint8Array, BenchFault> =>
  Effect.tryPromise({
    try: async () => {
      const zip = new JSZip()
      Array.forEach(plan.entries, (entry) =>
        zip.file(entry.name, Buffer.from(entry.bytes, "base64"), {
          compression: _MODALITY[entry.format].compression,
          compressionOptions: { level: 6 },
          date: new Date(0),
        }))
      return zip.generateAsync({ type: "uint8array", streamFiles: true })
    },
    catch: (cause) => new BenchFault({ case: { reason: "starved", request: "Render", detail: `zip deflate — ${String(cause)}` } }),
  })

// The plan handlers are a mapped record over the worker's OWN request roster, so a kind landing without a handler is
// a compile error here — the ternary's `else` silently answered the wrong plan codec for every kind but one.
const _plans: { readonly [K in Bench.Kind]: (plan: Uint8Array) => Effect.Effect<Uint8Array, BenchFault> } = {
  pdf: (plan) => Effect.flatMap(_decodedPlan(_PdfPlan, "pdf", plan), _workerPdf),
  zip: (plan) => Effect.flatMap(_decodedPlan(_BundlePlan, "zip", plan), _workerBundle),
}

const _worker = {
  Drop: (_request: Drop) => Effect.void,
  Render: (request: Render) => _plans[request.kind](request.plan),
} as const
```

## [05]-[CSV_ARM]

[CSV_ARM]:
- Owner: the CSV codec arm — egress is per-row serialization: the header line mints once from `Papa.unparse({ fields, data: [] })`, every projected row serializes through `Papa.unparse` with `header: false`, and the encoded lines flow as the artifact body — one row in memory at a time, so a multi-gigabyte export never holds one string. Ingress is the polymorphic `parse` — the string arm synchronously with `result.errors` lifted before `result.data` is read, the `Papa.parse(Papa.NODE_STREAM_INPUT)` duplex for unbounded inputs — every row decoded by the caller's Schema, `dynamicTyping` refused so typing authority never forks.
- Law: `escapeFormulae` rides every egress call — a cell beginning `=`/`+`/`-`/`@` prefixes so a spreadsheet consumer never executes it; CSV egress is untrusted-sink output by definition.
- Law: `ParseError` accumulates, never throws — the code family (`Quotes`/`Delimiter`/`FieldMismatch`) lowers to the fault rail with the `ParseMeta` cursor as evidence.
- Growth: a delimiter or encoding posture is an `UnparseConfig` field on the spec's format row.
- Packages: `papaparse` (`parse`, `unparse`, `NODE_STREAM_INPUT`, `Parser`).

```typescript signature
const _csv = <A, R>(spec: Report.Csv<A>, rows: Stream.Stream<A, never, R>): Effect.Effect<Report.Artifact<R>> =>
  Effect.flatMap(Ref.make(0), (counted) => {
    const encoder = new TextEncoder()
    const fields = Array.map(spec.columns, (column) => column.header)
    return _sealed(
      "csv",
      spec.name,
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
- Owner: the archive container — `Report.bundle(spec)` folds named members into one `JSZip` tree, each entry taking the compression its own modality row states (`STORE` for already-compressed containers, `DEFLATE` level 6 for text) beside fixed entry dates for byte stability. A member roster at or below the `zip` row's byte threshold uses `generateInternalStream({ type: "uint8array", streamFiles: true })` bridged through `Stream.async`, and `onUpdate` metadata folds into the supplied progress projection; a roster above it encodes the same entries into the typed bundle plan and dials the `Render` worker. Each entry's bytes arrive through `Report.gathered` under the member's stated ceiling, so worker routing moves compression off the request thread without pretending the already-gathered entries are unbounded.
- Law: inbound archives are untrusted — `loadAsync(data, { checkCRC32: true })` gates integrity, and every entry's `unsafeOriginalName` resolves under the extraction anchor before any byte lands; the fold admits only targets that keep the anchor as their path prefix, and an escaping resolution folds to the `slip`-reasoned fault.
- Law: DEFLATE is CPU-bound pure JS — a bundle whose gathered entry bytes pass the `zip` row's threshold runs through the same worker `Render` request the PDF arm dials, under the `zip` request kind its own row names; the threshold chooses execution placement and never masquerades as a materialization ceiling.
- Growth: a container policy axis (per-tenant naming, manifest entry) is a fold parameter; a second archive format is a new arm at the spec dispatch, never a fork of this one.
- Packages: `jszip` (`JSZip`, `generateInternalStream`, `generateAsync`, `loadAsync`, `JSZipMetadata`).

```typescript signature
const _bundleStream = (spec: Report.Bundle) =>
  Stream.async<Uint8Array, ReportFault>((emit) => {
    const zip = new JSZip()
    for (const entry of spec.entries) {
      zip.file(entry.name, entry.bytes, {
        compression: _MODALITY[entry.format].compression,
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
    helper.on("error", (cause) =>
      emit.fail(new ReportFault({ case: { reason: "archive", arm: "zip", detail: String(cause) } })))
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
  Option.match(_crossing("zip", Array.reduce(spec.entries, 0, (total, entry) => total + entry.bytes.length)), {
    onNone: () => _bundleStream(spec),
    onSome: (kind) =>
      Stream.fromEffect(
        Effect.mapError(Render.rendered(kind, new TextEncoder().encode(JSON.stringify(_bundlePlan(spec)))), _sank("zip")),
      ),
  })

const _unbundle = (bytes: Uint8Array, root: string) =>
  Effect.tryPromise({
    try: () => JSZip.loadAsync(bytes, { checkCRC32: true }),
    catch: (cause) => new ReportFault({ case: { reason: "archive", arm: "zip", detail: String(cause) } }),
  }).pipe(
    Effect.flatMap((zip) => {
      const anchor = path.resolve(root)
      return Effect.forEach(Object.values(zip.files), (entry) => {
        const target = path.resolve(anchor, entry.unsafeOriginalName)
        return target === anchor || target.startsWith(`${anchor}${path.sep}`)
          ? Effect.tryPromise({
            try: () => entry.async("uint8array"),
            catch: (cause) => new ReportFault({ case: { reason: "archive", arm: "zip", detail: String(cause) } }),
          }).pipe(Effect.map((body) => ({ name: target, body })))
          : Effect.fail(new ReportFault({ case: { reason: "slip", anchor, entry: entry.name } }))
      })
    }),
  )

const Report = {
  amend: _amended,
  render: _render,
  gathered: _gathered,
  bundle: _bundle,
  unbundle: _unbundle,
  worker: _worker,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Report, ReportFault }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
