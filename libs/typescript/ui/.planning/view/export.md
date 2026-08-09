# [UI_EXPORT]

Export owns every way rendered state leaves the browser: one source family, one format vocabulary, one serializer matrix whose shape makes an illegal pair a compile error, one content-minted parcel, and one capability port the browser composition satisfies. Every surface keeps its own engine and hands this owner a handle; this owner keeps no engine, opens no view, and spells no native picker. Module: `ui/src/view/export.ts`.

## [01]-[INDEX]

- [02]-[FORMAT_LAW]: the format row table, the parcel owner, the content mint delegate, the fault family; `Export`, `ExportFault`.
- [03]-[SERIALIZER_MATRIX]: the source family, the per-source admitted-format matrix, the raster-encode port; `Export`, `Raster`.
- [04]-[EGRESS_PORT]: the delivery capability Tag, the route vocabulary, the streaming lane; `Egress`.

## [02]-[FORMAT_LAW]

[FORMAT_LAW]:
- Owner: `Export` — one owner whose members are the octet fold, the parcel mint, and the delivery dispatch; a format is a `Export.format` row carrying `mime`, `extension`, and `binary`, so a new format is one row and the filename, the media type, and the text-versus-octets encode step all derive from it. A `mime` literal beside a call site, or a filename assembled by concatenation, restates a column the row already carries.
- Owner: `Export.Parcel` — the Schema owner every export lands as: the resolved name, the format key, the content key the mint delegate answers, and the held octets. `filename` and `mime` are getters projecting the format row, so the two facts a delivery needs are derived from the parcel alone and no consumer re-reads the table.
- Law: the content mint is a delegate; `mint` maps octets to `Digest.Key<"content">`, and this module carries no hash implementation.
- Law: octets are the one currency — every serializer answers `Uint8Array`, text formats crossing through the encoder at the seam, so the parcel, the digest, and the port all speak one shape and no arm carries a `string | Uint8Array` union downstream.
- Law: the trip is woven at the owner — `Effect.withSpan("rasm.ui.export.parcel")` carries the format and the source tag as span attributes and log annotations, and the two convention rows mount once (`exportParcels` fanned on the format and source axes, `exportSize` pricing parcel octets on the format axis), so parcel count, size, and encode latency reach the app bridge with zero collector import; the format key is bounded and rides the tag axis, while the parcel name is identifier-grade and stays log material.
- Packages: `@rasm/ts/core` (`Digest`, `Convention`, `Fault.Class`); `effect` (`Schema`, `Effect`, `Data`, `Match`, `Record`).
- Boundary: `view/chart` owns every perspective view bracket and hands this owner a read; `viewer/scene` owns the renderer and hands this owner an element; `viewer/probe` owns capture readback and hands this owner a pixel band; `system/cache` owns OPFS residency and this owner owns egress alone — a byte that stays in the browser is the cache's, a byte that leaves is this page's.
- Growth: a new format is one row with its column in the matrix; a new source is one case with its row of serializers — never a second export surface beside this one.

```typescript
import { Convention, Digest, Fault } from "@rasm/ts/core"
import { Effect, Schema, type Types } from "effect"

const _formats = ["arrow", "csv", "json", "svg", "png", "glb", "text"] as const

const _formatRows = {
  arrow: { mime: "application/vnd.apache.arrow.file", extension: "arrow", binary: true },
  csv: { mime: "text/csv", extension: "csv", binary: false },
  json: { mime: "application/json", extension: "json", binary: false },
  svg: { mime: "image/svg+xml", extension: "svg", binary: false },
  png: { mime: "image/png", extension: "png", binary: true },
  glb: { mime: "model/gltf-binary", extension: "glb", binary: true },
  text: { mime: "text/plain", extension: "txt", binary: false },
} as const

declare namespace Export {
  type Formats = typeof _formats
  type Format = keyof typeof _formatRows
  type FormatRow = { readonly mime: string; readonly extension: string; readonly binary: boolean }
  type Mint = (octets: Uint8Array) => Effect.Effect<Digest.Key<"content">>
  // Two row columns already decide the encodable-raster subset — an `image/*` media type carried as octets — so a new
  // raster row admits the encode port with no second roster, and `svg` stays out on its text column
  type Raster = { readonly [F in Format]: (typeof _formatRows)[F] extends { readonly mime: `image/${string}`; readonly binary: true } ? F : never }[Format]
  type _Rows<T extends Record<Formats[number], FormatRow> = typeof _formatRows> = T // row guard: a missing format or a malformed column fails at the declaration with zero widening
  type _Keys<K extends Formats[number] = Format> = K // key guard: a format row outside the tuple fails here
}

// One row per reason carrying the core kind alone: severity, blame, retryability, and quarantine stay the core
// Fault.Class row table's, so a local rank or retry column would fork the branch lattice per folder.
const _family = Fault.Class.family(["source-refused", "encode-failed", "egress-denied", "egress-absent"] as const, {
  "source-refused": { class: "invalid" },
  "encode-failed": { class: "malformed" },
  "egress-denied": { class: "denied" },
  "egress-absent": { class: "unavailable" },
})

class ExportFault extends Schema.TaggedError<ExportFault>()("ExportFault", {
  reason: _family.schema,
  parcel: Schema.String,
  detail: Schema.String,
}) {
  static readonly roster: typeof _family.reasons = _family.reasons // the metric word census reads the family's own ordered tuple
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<export:${this.reason}> ${this.parcel}: ${this.detail}`
  }
}

const _Format = Schema.Literal(..._formats) // the tuple spread holds the non-empty overload and keeps the exact literal set

class Parcel extends Schema.Class<Parcel>("Parcel")({
  name: Schema.NonEmptyString,
  format: _Format,
  key: Digest.Key.content,
  octets: Schema.Uint8ArrayFromSelf,
}) {
  get row(): Export.FormatRow {
    return _formatRows[this.format]
  }
  get mime(): string {
    return this.row.mime
  }
  get filename(): string {
    return `${this.name}.${this.row.extension}` // the suffix derives from the row: no call site concatenates one
  }
}

const _utf8 = new TextEncoder() // BOUNDARY ADAPTER: the text-to-byte crossing, mirror of the admission kernel's decoder seam

const _encoded = (text: string): Uint8Array => _utf8.encode(text)
```

## [03]-[SERIALIZER_MATRIX]

[SERIALIZER_MATRIX]:
- Owner: `Export.Source` — the closed source family, one case per surface that can leave the browser: `Pivot` carries the engine handle and its view config, `Figure` the detached Plot element, `Canvas` the uplot backing canvas, `Rows` the Arrow table a grid or a selection projects, `Scene` the `<model-viewer>` element, `Readback` the renderer pixel band with its extent, `Draft` a form payload beside the kernel Schema that admitted it, and `Lines` the evidence rows a probe board renders.
- Law: legality is the matrix's own shape, never a runtime refusal — `Export.Admits` names the formats each case admits and the serializer record is `{ readonly [T in Kind]: { readonly [F in Admits[T]]: … } }`, so `Export.octets` is one correlated generic indexed call and an illegal pair — a scene as csv, a draft as png — fails at the call site rather than answering a fault at runtime. A `legal(source, format)` predicate beside this matrix re-derives at runtime what the mapped contract already proves.
- Law: the pivot arms open no view — all three ride `Chart.snapshot`, whose `read` parameter is the only axis they vary (`to_arrow`, `to_csv`, `to_json`), so every perspective bracket on the branch lives at its owner and this page holds a serializer, not a lifetime.
- Law: tabular CSV is a pivot export, never a hand-rolled writer — `Rows` admits `arrow` alone because Arrow ships an IPC writer and no CSV writer, and the engine that already speaks CSV is perspective; a column-joining text fold beside `tableToIPC` restates a serializer the admitted engine owns, and the CSV of a client-modeled grid is the same rows loaded as a pivot origin.
- Law: `Plot.plot` answers `SVGSVGElement | HTMLElement` — a caption, title, or legend option wraps the svg in a figure — so the `svg` arm discriminates on the tag it holds and reaches the inner element when it is wrapped; an `as SVGSVGElement` here serializes a figure wrapper as if it were the drawing.
- Law: the raster arms split by who owns the pixels — a `<model-viewer>` element and a uplot backing canvas each answer their own live surface, while a three-owned renderer cannot be read off a live swap chain, so its plane arrives already read back through `viewer/probe#CAPTURE_FOLD`; a `canvas.toBlob` against the scene's own canvas is the named defect, because the drawing buffer is not preserved.
- Law: a read-back plane encodes OFF the document — `Raster` is the folder-declared encode capability the browser root satisfies by wrapping `runtime:browser/fetch#WIRE_PROTOCOL`'s `Imprint` request, whose transfer list MOVES the plane into the decode pool, so the frame that just rendered never pays the PNG encode; the satisfier's `PoolFault`, worker, and decode failures map onto this page's `encode-failed` at that one wrap, exactly as `[4]`'s `Egress` wrap does, because neither package imports the other and a fault class cannot cross the strata; live canvases keep their own encoders, since an element handle cannot cross a worker boundary, so the port carries planes alone.
- Law: the plane crossing the port is the hash preimage's own normalization — `Probe.packed` resolves row stride and origin into tightly packed top-left RGBA8, and the clamped view re-labels those bytes rather than copying them, so the exported image and the capture verdict read one buffer; a stride-blind `ImageData` construction shears a padded readback and inverts a bottom-left one.
- Law: a draft encodes through the Schema that admitted it — `Schema.parseJson(schema)` fuses the encode and the stringify into one codec, so the exported payload and the wire payload cannot skew; a second serialization shape beside the kernel Schema is the parallel-validator defect `view/form` already names.
- Packages: `apache-arrow` (`tableToIPC`, `RecordBatchWriter.throughDOM`); `@google/model-viewer` (`ModelViewerElement.exportScene`, `.toBlob`); `uplot` (the `ctx` readback — `u.ctx.canvas` is the only reach to the backing element, and it is a DOM canvas); `view/chart` (`Chart.snapshot`, `Chart.Pivot`); `viewer/probe` (`Probe.Readback`, `Probe.packed`); `effect` (`Context`, `Option`).
- Boundary: which config a pivot exports, which scale a figure inked, and which rows a grid selected are the owning surfaces' state; this matrix receives handles and answers octets. Where the encode RUNS is the composition root's — the port declares the crossing, the host names the pool.
- Growth: a new admitted format for an existing source is one key in that source's row and one entry in `Admits` — the mapped contract turns the missing serializer into a compile error at the record while every call site stays untouched; a lossy readback is one arm here, because the codec and quality axes already ride the port.

```typescript
import type { ModelViewerElement } from "@google/model-viewer"
import type { View, ViewConfigUpdate } from "@perspective-dev/client"
import { tableToIPC, type Table } from "apache-arrow"
import { Array, Context, Data, Effect, Option, Schema, type Scope } from "effect"
import type uPlot from "uplot"
import { Probe } from "../viewer/probe.ts"
import { Chart } from "./chart.ts"

declare namespace Export {
  type Source = Data.TaggedEnum<{
    Pivot: { readonly pivot: Chart.Pivot; readonly feed: string; readonly config: ViewConfigUpdate }
    Figure: { readonly figure: SVGSVGElement | HTMLElement }
    Canvas: { readonly chart: uPlot }
    Rows: { readonly table: Table }
    Scene: { readonly element: ModelViewerElement }
    Readback: { readonly readback: Probe.Readback; readonly width: number; readonly height: number }
    Draft: { readonly draft: unknown; readonly schema: Schema.Schema<unknown, string> }
    Lines: { readonly lines: ReadonlyArray<string> }
  }>
  type Kind = Export.Source["_tag"]
  type Case<T extends Export.Kind> = Extract<Export.Source, { readonly _tag: T }>
  type Admits = {
    readonly Pivot: "arrow" | "csv" | "json"
    readonly Figure: "svg"
    readonly Canvas: "png"
    readonly Rows: "arrow"
    readonly Scene: "glb" | "png"
    readonly Readback: "png"
    readonly Draft: "json"
    readonly Lines: "text"
  }
  type Serialize<T extends Export.Kind> = (source: Export.Case<T>) => Effect.Effect<Uint8Array, ExportFault, Raster | Scope.Scope>
  type Matrix = { readonly [T in Export.Kind]: { readonly [F in Export.Admits[T]]: Export.Serialize<T> } }
  type _Admits<A extends Record<Export.Kind, Export.Format> = { [T in Export.Kind]: Export.Admits[T] }> = A // key guard: an admitted format outside the vocabulary fails here
}

const _Source = Data.taggedEnum<Export.Source>()

// Raster declares this folder's encode capability and the browser root satisfies it over the decode pool, so an
// element handle never crosses the port and a plane never encodes on the thread that drew it. Callers pass the parcel
// because the refusal stamps where the octets fail, and the satisfier maps its own fault family onto this one.
class Raster extends Context.Tag("ui/Raster")<Raster, {
  readonly imprint: (
    parcel: string,
    pixels: ImageData,
    format: Export.Raster,
    quality: Option.Option<number>,
  ) => Effect.Effect<Uint8Array, ExportFault>
}>() {}

const _blob = (parcel: string, take: () => Promise<Blob>): Effect.Effect<Uint8Array, ExportFault> =>
  Effect.tryPromise({
    try: async () => new Uint8Array(await (await take()).arrayBuffer()),
    catch: (defect) => new ExportFault({ reason: "encode-failed", parcel, detail: String(defect) }),
  })

const _raster = (parcel: string, canvas: HTMLCanvasElement): Effect.Effect<Uint8Array, ExportFault> =>
  _blob(parcel, () =>
    // BOUNDARY ADAPTER: the DOM canvas encoder is callback-shaped and answers null on an unencodable surface
    new Promise<Blob>((settle, refuse) => canvas.toBlob((held) => (held === null ? refuse(new Error("<unencodable>")) : settle(held)), _formatRows.png.mime)))

// Encoding leaves the document with the plane: `Probe.packed` resolves stride and origin into the same tightly packed
// top-left RGBA8 the hash preimage takes, the clamped view re-labels those bytes with no copy, and the port's transfer
// list moves the buffer; the codec and quality axes ride the port, never this arm
const _drawn = (parcel: string, row: Export.Case<"Readback">): Effect.Effect<Uint8Array, ExportFault, Raster> =>
  Effect.flatMap(Raster, (port) =>
    Effect.flatMap(row.readback(row.width, row.height), (capture) =>
      port.imprint(
        parcel,
        new ImageData(new Uint8ClampedArray(Probe.packed(capture, row.width, row.height).buffer), row.width, row.height),
        "png",
        Option.none(),
      )))

const _svg = (parcel: string, figure: SVGSVGElement | HTMLElement): Effect.Effect<Uint8Array, ExportFault> =>
  Effect.map(
    // a caption, title, or legend option wraps the drawing in a figure: the wrapped case reaches the inner element
    Effect.fromNullable(figure instanceof SVGSVGElement ? figure : figure.querySelector("svg")).pipe(
      Effect.mapError(() => new ExportFault({ reason: "source-refused", parcel, detail: "<no-svg-root>" })),
    ),
    (root) => _encoded(new XMLSerializer().serializeToString(root)),
  )

const _text = (parcel: string, read: () => Promise<string>): Effect.Effect<Uint8Array, ExportFault> =>
  Effect.map(
    Effect.tryPromise({
      try: read,
      catch: (defect) => new ExportFault({ reason: "encode-failed", parcel, detail: String(defect) }),
    }),
    _encoded,
  )

const _snapshot = <A>(row: Export.Case<"Pivot">, read: (view: View) => Promise<A>) =>
  Effect.mapError(
    Chart.snapshot(row.pivot, row.feed, row.config, read), // every perspective bracket lives at its owner
    (fault) => new ExportFault({ reason: "source-refused", parcel: row.feed, detail: fault.message }),
  )

const _MATRIX: Export.Matrix = {
  Pivot: {
    arrow: (row) => Effect.map(_snapshot(row, (view) => view.to_arrow()), (held) => new Uint8Array(held)),
    csv: (row) => Effect.map(_snapshot(row, (view) => view.to_csv()), _encoded),
    json: (row) => Effect.map(_snapshot(row, (view) => view.to_json_string()), _encoded),
  },
  Figure: { svg: (row) => _svg("<figure>", row.figure) },
  Canvas: { png: (row) => _raster("<series>", row.chart.ctx.canvas) }, // u.ctx.canvas is the only reach to the backing element
  Rows: { arrow: (row) => Effect.succeed(tableToIPC(row.table, "file")) },
  Scene: {
    glb: (row) => _blob("<scene>", () => row.element.exportScene()),
    png: (row) => _blob("<scene>", () => row.element.toBlob()),
  },
  Readback: { png: (row) => _drawn("<readback>", row) },
  Draft: {
    json: (row) =>
      Effect.mapError(
        Effect.map(Schema.encode(row.schema)(row.draft), _encoded), // parseJson fuses encode and stringify: the exported payload cannot skew from the wire payload
        (fault) => new ExportFault({ reason: "encode-failed", parcel: "<draft>", detail: fault.message }),
      ),
  },
  Lines: { text: (row) => Effect.succeed(_encoded(Array.join(row.lines, "\n"))) },
}

const _octets = <T extends Export.Kind, F extends Export.Admits[T]>(
  source: Export.Case<T>,
  format: F,
): Effect.Effect<Uint8Array, ExportFault, Raster | Scope.Scope> =>
  // the mapped contract resolves the indexed access to one correlated signature: the per-case payload flows cast-free
  _MATRIX[source._tag][format](source)

// The two convention rows this plane owns — the counter fans on the bounded format and source axes, the histogram
// prices parcel octets; both tagged per emission, so one mounted handle serves every format row.
const _parcels = Convention.mount(Convention.metric.exportParcels)
const _size = Convention.mount(Convention.metric.exportSize)

const _parcel = <T extends Export.Kind, F extends Export.Admits[T]>(
  source: Export.Case<T>,
  format: F,
  name: string,
  mint: Export.Mint,
): Effect.Effect<Parcel, ExportFault, Raster | Scope.Scope> =>
  Effect.gen(function* () {
    const octets = yield* _octets(source, format)
    const key = yield* mint(octets) // the branch's content mint, delegated exactly as the capture fold takes it
    yield* Effect.withMetric(Effect.succeed(1), _parcels).pipe(
      Effect.tagMetrics({ [Convention.rasm.exportFormat]: format, [Convention.rasm.exportSource]: source._tag }),
    )
    yield* Effect.withMetric(Effect.succeed(octets.byteLength), _size).pipe(
      Effect.tagMetrics(Convention.rasm.exportFormat, format),
    )
    return new Parcel({ name, format, key, octets })
  }).pipe(
    Effect.withSpan("rasm.ui.export.parcel", { attributes: { "export.format": format, "export.source": source._tag } }),
    Effect.annotateLogs({ parcel: name }),
  )
```

## [04]-[EGRESS_PORT]

[EGRESS_PORT]:
- Owner: `Egress` — the folder-declared delivery capability Tag: `deliver(parcel, route)` writes a whole parcel and `open(parcel)` yields a `WritableStream` for a payload no buffer holds, both on the typed fault rail, declared HERE and satisfied at the browser composition root exactly as `system/primitive#CLIPBOARD_PORT` is. This folder never imports the platform package, so the capability travels the requirement channel and a proof substitutes a Layer.
- Law: the native surfaces stay outside this folder — the save picker is not declared in the platform lib at all, and `navigator.storage` is confined to its own runtime owner, so the port's contract is the whole ui-side truth and the composition root spells whichever native call its host carries. A ui page reaching for a picker, an anchor click, or a storage handle is the ungated-native-call defect, and a host carrying none of them satisfies the port with a Layer whose every route answers `egress-absent`.
- Law: the browser satisfier is the runtime egress value (`runtime:browser/persist#STORAGE_RESIDENCY`), wrapped member-for-member at the composition root — `deliver(parcel, false)` onto its save ladder, `deliver(parcel, true)` onto its share arm, `open` onto its picker-stream arm — with the satisfier's fault family mapped onto THIS page's reasons at the same wrap (`absent` → `egress-absent`, everything else → `egress-denied`), because neither package imports the other and a fault class cannot cross the strata; the Tag's shape and fault stay ui-owned, the natives stay runtime-owned, and the wrap is the only place both truths meet.
- Law: routes are a closed vocabulary with a handler row each — `Export.deliver(parcel, route)` dispatches `save` and `share` onto the `Egress` port and `clipboard` onto the `Clipboard` port `system/primitive` already declares, so text that belongs in the paste buffer never grows a second port and a binary parcel routed to the clipboard refuses at the row rather than at the host. A new route is one row and one handler; a `saveParcel`/`shareParcel` sibling pair is the arity twin this entrypoint deletes.
- Law: the object-URL lifecycle, wherever a composition uses one, is the bracket `viewer/scene` already states — acquire with `createObjectURL`, release with `revokeObjectURL` on the same scope; a leaked object URL pins the whole parcel in memory.
- Law: a payload past the buffer ceiling streams — `RecordBatchWriter.throughDOM()` answers a `{ writable, readable }` pair whose readable pipes straight into the port's writable, so an Arrow egress of arbitrary size never materializes one contiguous array; the buffered `Export.parcel` lane stays the default because it is the lane the content mint can digest, and the streaming lane names that it trades the key for the size.
- Packages: `effect` (`Context`, `Effect`, `Match`, `Scope`); `apache-arrow` (`RecordBatchWriter.throughDOM`); `system/primitive` (`Clipboard` — the text route's existing port).
- Boundary: what a route means to the operator — a download shelf, a share sheet, a journal POST — is the composition's; this page names the route, the parcel, and the refusal.
- Growth: a new delivery surface is one route row satisfied by the same port; a new port member is earned only by a capability the parcel shape cannot express.

```typescript
import { RecordBatchWriter, type Table } from "apache-arrow"
import { Context, Effect, Match, type Scope, type Types } from "effect"
import { Clipboard } from "../system/primitive.ts"

class Egress extends Context.Tag("ui/Egress")<Egress, {
  readonly deliver: (parcel: Parcel, share: boolean) => Effect.Effect<void, ExportFault>
  readonly open: (parcel: Parcel) => Effect.Effect<WritableStream<Uint8Array>, ExportFault, Scope.Scope>
}>() {}

const _routes = ["save", "share", "clipboard"] as const

declare namespace Export {
  type Routes = typeof _routes
  type Route = Routes[number]
  type Shape = Types.Simplify<{
    readonly Fault: typeof ExportFault
    readonly Parcel: typeof Parcel
    readonly Source: typeof _Source
    readonly format: typeof _formatRows // named, never spread: a format key and a member name share one space if they merge
    readonly formats: Export.Formats
    readonly routes: Export.Routes
    readonly octets: typeof _octets
    readonly parcel: typeof _parcel
    readonly deliver: typeof _deliver
    readonly piped: typeof _piped
  }>
}

const _deliver = (parcel: Parcel, route: Export.Route): Effect.Effect<void, ExportFault, Egress | Clipboard> =>
  Match.value(route).pipe(
    Match.when("clipboard", () =>
      parcel.row.binary
        ? Effect.fail(new ExportFault({ reason: "source-refused", parcel: parcel.name, detail: "<binary-to-clipboard>" }))
        : Effect.flatMap(Clipboard, (board) =>
          Effect.mapError(
            board.copy(new TextDecoder().decode(parcel.octets)), // BOUNDARY ADAPTER: the text route re-reads the one octet currency
            (fault) => new ExportFault({ reason: "egress-denied", parcel: parcel.name, detail: fault.message }),
          ))),
    Match.orElse((direct) => Effect.flatMap(Egress, (port) => port.deliver(parcel, direct === "share"))),
  )

const _piped = (
  name: string,
  format: Export.Format,
  key: Digest.Key<"content">,
  batches: Table,
): Effect.Effect<void, ExportFault, Egress | Scope.Scope> =>
  Effect.gen(function* () {
    const port = yield* Egress
    // the streaming lane trades the content key's coverage for size: the mint saw no octets, so the key names the source
    const writable = yield* port.open(new Parcel({ name, format, key, octets: new Uint8Array(0) }))
    const through = RecordBatchWriter.throughDOM<Record<string, never>>()
    yield* Effect.tryPromise({
      try: async () => {
        void through.writable.getWriter().write(batches)
        await through.readable.pipeTo(writable)
      },
      catch: (defect) => new ExportFault({ reason: "encode-failed", parcel: name, detail: String(defect) }),
    })
  })

const Export: Export.Shape = {
  Fault: ExportFault,
  Parcel,
  Source: _Source,
  format: _formatRows,
  formats: _formats,
  routes: _routes,
  octets: _octets,
  parcel: _parcel,
  deliver: _deliver,
  piped: _piped,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Egress, Export, ExportFault, Parcel, Raster }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
