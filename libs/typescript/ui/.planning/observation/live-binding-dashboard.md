# [UI_LIVE_BINDING_DASHBOARD]

The live-wire binding-studio observation leaf over the decoded `csharp:Rasm.AppHost/live-wire#TS_PROJECTION` binding-status, coerced-value, and write-receipt wire — a per-binding connect/subscribe/stale/fault health table, the source-versus-canonical unit coercion read off `CoercedValueWire`, and the write-back disposition rendered as a literal-discriminated outcome off `WriteReceiptWire`. `LiveBindingDashboard` decodes the settled `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire` shapes the C# `Rasm.AppHost` live-wire spine mints, folds the binding-status changefeed into one `@tanstack/react-table` row model virtualized through `@tanstack/react-virtual` for a high-cardinality binding set, and reads-only — every binding-status key is the decoded smart-enum string verbatim. The leaf is a data-dashboard surface distinct in kind from the render-engine surfaces; it subscribes to its fold only through the `binding/atom-binding.md` `AtomBinding` and re-mints no binding-status vocabulary.

## [1]-[INDEX]

One cluster: `LIVE_BINDING_DASHBOARD` owns the binding-status table, the coercion column, and the write-disposition projection.

## [2]-[LIVE_BINDING_DASHBOARD]

- Owner: `LiveBindingDashboard`, the read-only binding-studio leaf over the decoded live-wire feed; `BindingStateKey`/`ExternalTransportKey`/`BindingDirectionKey`, the `Schema.Literal` vocabularies decoding the C# `BindingState`/`ExternalTransport`/`BindingDirection` smart-enum string keys verbatim; the `@tanstack/react-table` column model over the `BindingStatusWire` row joined to its latest `CoercedValueWire` and `WriteReceiptWire`; and the `@tanstack/react-virtual` virtualizer over the binding row set. The dashboard renders each binding as one table row carrying its transport, lifecycle state, direction, last-good freshness, the source-versus-canonical coercion, and the write-back disposition.
- Cases: each binding's lifecycle state keys off the decoded `BindingStateKey` string — `connecting`/`subscribed`/`polling`/`stale`/`faulted` rendered as the row's health badge, a keyed-domain vocabulary lookup over the state string, never a `Match` chain restating one state per arm; the coercion column reads `CoercedValueWire` `canonical`/`canonicalUnit` beside `sourceUnit` so a divergence between the source-declared and suite-canonical units surfaces in the SPA, and the `sourceAt` extended-ISO timestamp renders source freshness against host time; the write-back disposition reads the `WriteReceiptWire.disposition` literal-discriminated union — `acknowledged`/`rejected`/`rolled-back`/`coalesced` rendered by its `kind`, the rejection fault string and the rolled-back prior value carried per arm — under one `Match.value` fold total over the disposition kind; a subscribe-shaped transport (OPC-UA, MQTT) streams its reactive sequence into the live cell and a poll-shaped transport refreshes on its cadence, both rendered uniformly as the same row shape because the dashboard reads the decoded status, never the transport mechanism.
- Entry: the leaf subscribes through the `binding/atom-binding.md` `AtomBinding` to a `projection` live cell folding the binding-status changefeed — one `feed-stores/live-cells#LIVE_CELLS` `FeedKind` row keyed by `BindingStatusWire.bindingId`, the latest status, coerced value, and write receipt per binding the live cell; the write receipt reconstructs through the existing `projection` `ReceiptEnvelopeWire`; the table reads the live cell through the binding's `Result.builder` chain so the loading/success/failure arms render uniformly; the leaf reads and never emits — it dials no transport and folds no binding-status enum of its own.
- Packages: `react`, `react-dom`, `@tanstack/react-table`, `@tanstack/react-virtual`, `@effect-atom/atom`, `effect`; no transport package on this card.
- Growth: a new binding field is one column on the decoded `BindingStatusWire` rendered as one `ColumnDef` row, never a second wire vocabulary; a new write disposition is one literal arm on the `WriteReceiptWire.disposition` union breaking the disposition fold at compile time; a new transport or lifecycle state is one `ExternalTransportKey`/`BindingStateKey` literal breaking its vocabulary lookup; a new dashboard panel reads the same live cell, never a parallel surface.
- Boundary: a branch-side binding-status enum or a second binding-status vocabulary beside the C# `BindingStatusWire` is the named cross-language drift defect — the binding-status, transport, direction, and write-disposition vocabularies are minted once in C# and decoded here as the smart-enum strings verbatim; a second decode of the live-wire feed beside the `interchange` rail is the named defect; a transport dial from the dashboard (a write-back issued from the SPA, a binding subscribe opened in the browser) is the named defect — the dashboard reads and never emits, the write-back being the C# `WriteBackSurface` owner; a `BindingState` decoded by ordinal instead of the smart-enum string is the named seam violation; a hand-rolled three-state loading match beside the `binding/atom-binding.md` `Result.builder` chain is the named defect; a `useState` table-state object holding the binding rows instead of the `@tanstack/react-table` row model over the decoded feed is the named defect.

```ts contract
import type { BindingStatusWire, CoercedValueWire, WriteReceiptWire } from "@rasm/interchange";
import type { ColumnDef } from "@tanstack/react-table";
import { createColumnHelper, flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import { useVirtualizer } from "@tanstack/react-virtual";
import { Match, Option, Schema } from "effect";
import * as React from "react";

const BindingStateKey = Schema.Literal("connecting", "subscribed", "polling", "stale", "faulted");
type BindingStateKey = Schema.Schema.Type<typeof BindingStateKey>;

const ExternalTransportKey = Schema.Literal("opc-ua", "modbus", "mqtt", "serial", "rest", "graphql", "spreadsheet", "erp-plm");
type ExternalTransportKey = Schema.Schema.Type<typeof ExternalTransportKey>;

const HEALTH_OF = {
  connecting: "pending",
  subscribed: "healthy",
  polling: "healthy",
  stale: "degraded",
  faulted: "unhealthy",
} as const satisfies Record<BindingStateKey, "pending" | "healthy" | "degraded" | "unhealthy">;

interface BindingRow {
  readonly status: BindingStatusWire;
  readonly coerced: Option.Option<CoercedValueWire>;
  readonly receipt: Option.Option<WriteReceiptWire>;
}

const dispositionLabel = (receipt: WriteReceiptWire): string =>
  Match.value(receipt.disposition).pipe(
    Match.discriminatorsExhaustive("kind")({
      acknowledged: (d) => `ack @ ${d.sourceAck}`,
      rejected: (d) => `rejected: ${d.fault}`,
      "rolled-back": (d) => `rolled back → ${d.priorValue}`,
      coalesced: (d) => `coalesced ← ${d.foldedInto}`,
    }),
  );

const coercionLabel = (coerced: CoercedValueWire): string =>
  `${coerced.canonical} ${coerced.canonicalUnit} ← ${coerced.sourceUnit}`;

const column = createColumnHelper<BindingRow>();

const COLUMNS: ReadonlyArray<ColumnDef<BindingRow, string>> = [
  column.accessor((row) => row.status.bindingId, { id: "binding", header: "binding" }),
  column.accessor((row) => row.status.transport, { id: "transport", header: "transport" }),
  column.accessor((row) => HEALTH_OF[row.status.state], { id: "health", header: "health" }),
  column.accessor((row) => row.status.direction, { id: "direction", header: "direction" }),
  column.accessor((row) => row.status.lastGoodAt ?? "never", { id: "fresh", header: "last good" }),
  column.accessor((row) => Option.match(row.coerced, { onNone: () => "—", onSome: coercionLabel }), { id: "coercion", header: "coercion" }),
  column.accessor((row) => Option.match(row.receipt, { onNone: () => "—", onSome: dispositionLabel }), { id: "write", header: "write-back" }),
] as ReadonlyArray<ColumnDef<BindingRow, string>>;

const LiveBindingDashboard: React.FC<{ readonly rows: ReadonlyArray<BindingRow>; readonly scrollRef: React.RefObject<HTMLDivElement> }> = ({ rows, scrollRef }) => {
  const table = useReactTable({ data: rows as Array<BindingRow>, columns: COLUMNS as Array<ColumnDef<BindingRow, string>>, getCoreRowModel: getCoreRowModel() });
  const model = table.getRowModel();
  const virtual = useVirtualizer({
    count: model.rows.length,
    getScrollElement: () => scrollRef.current,
    estimateSize: () => 36,
    overscan: 12,
  });
  return React.createElement(
    "div",
    { ref: scrollRef, role: "grid", "aria-label": "live binding studio", style: { overflow: "auto" } },
    React.createElement("div", { style: { height: virtual.getTotalSize() } },
      virtual.getVirtualItems().map((item) => {
        const row = model.rows[item.index];
        return React.createElement(
          "div",
          { key: row.id, role: "row", "data-health": HEALTH_OF[row.original.status.state], ref: virtual.measureElement, "data-index": item.index },
          row.getVisibleCells().map((cell) =>
            React.createElement("span", { key: cell.id, role: "gridcell" }, flexRender(cell.column.columnDef.cell, cell.getContext()))),
        );
      })),
  );
};
```

The `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire` mirror the C# `csharp:Rasm.AppHost/live-wire#TS_PROJECTION` records field-for-field — the `state` is the smart-enum `BindingStateKey` the `HEALTH_OF` `as const satisfies Record` maps to the health badge, the `transport`/`direction` cross as their smart-enum string keys, the `lastGoodAt` as the extended-ISO instant or null, the coercion as the `canonical`/`canonicalUnit`/`sourceUnit` triple, and the write disposition as the `kind`-discriminated union the `dispositionLabel` fold reads total under `Match.discriminatorsExhaustive("kind")`. The binding-status table is the `@tanstack/react-table` `createColumnHelper`/`useReactTable`/`getCoreRowModel`/`flexRender` row model the `tanstack-react-table.md` catalogue carries, virtualized through the `@tanstack/react-virtual` `useVirtualizer`/`getVirtualItems`/`getTotalSize`/`measureElement` surface so a host with thousands of industrial-edge bindings renders one virtualized table, never a per-binding DOM node. The write receipt reconstructs through the existing `projection` `ReceiptEnvelopeWire` and the live cell folds through the `feed-stores/live-cells#LIVE_CELLS` keyed fold; the dashboard reads the cell through the `binding/atom-binding.md` `Result.builder` chain.

RESEARCH [BINDING_FEED_ROW]: the `projection` live-cell row carrying the binding-status changefeed is one `feed-stores/live-cells#LIVE_CELLS` `FeedKind` row keyed by `BindingStatusWire.bindingId` folding the `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire` union against `csharp:Rasm.AppHost/live-wire#TS_PROJECTION`, and the `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire` `Schema.Class` decode rows land at `interchange/codecs/decode-rail#DECODE_RAIL` under the AppHost json-stj `ReceiptEnvelopeWire` anchor on `interchange/contracts/wire-inventory#WIRE_INVENTORY` — but the `wire-inventory` map carries no `csharp:Rasm.AppHost/live-wire` row today (its four AppHost anchors are lifecycle-and-drain, health-and-degradation, support-bundles, and runtime-ports) and `decode-rail` registers no live-wire `Schema.Class` rows, so the `@rasm/interchange` `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire` imports do not yet resolve and this surface stays BLOCKED on (1) the upstream `Rasm.AppHost/live-wire` `[7]-[TS_PROJECTION]` promoting the binding-status/coerced-value/write-receipt shapes into the projection fence, (2) a live-wire wire-inventory row binding that anchor to the json-stj codec, (3) the matching `decode-rail` rows, and (4) the `projection` live cell folding the changefeed. The producer-side discriminants are settled — the `BindingStateKey`/`ExternalTransportKey` smart-enum literals, the `HEALTH_OF` vocabulary, and the `dispositionLabel`/`coercionLabel` folds transcribe the C# `BindingState`/`ExternalTransport` smart-enum keys and the `WriteBack` disposition arms verbatim, and the `@tanstack/react-table`/`@tanstack/react-virtual` surface is catalogue-verified — only the consumer-side interchange landing and the AppHost projection fence are the open residuals.
