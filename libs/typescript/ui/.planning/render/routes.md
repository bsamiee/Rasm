# [UI_ROUTES]

The read-only dashboard routes over the `projection` folds — leaf subscribers that read and never emit. `EvidenceTimelineRoute` renders receipt envelopes in HLC order and draws the load-bearing `SkewBand` confidence-interval band — a `Match` over the `projection/causality/vector#VERSION_VECTOR` `CausalVerdict` where the `ConcurrentUncertain` arm draws the "within +/-N ms" band and the concurrent-uncertain pair marking over the `@tanstack/react-virtual` row set, reading the verdict from `causalStore` and the radius from `projection/causality/skew#SKEW_BAND` `SkewInterval.radiusMs` by reference, never re-deriving HLC order or the band; `BenchmarkRoute` renders only when fingerprint-gated, and `CollectorPanel` reads the telemetry collector. These are data-dashboard surfaces, distinct in kind from the render-engine surfaces. Each route subscribes to its fold only through the `binding/atom.md` `AtomBinding`; dashboards read the collector while instrumentation belongs to `platform`.

## [01]-[INDEX]

- [01]-[OBSERVATION_ROUTES]: the `EvidenceTimelineRoute` HLC-ordered receipt timeline with `CausalVerdict` `SkewBand` confidence rendering, the `BenchmarkRoute` fingerprint-gated dashboard, and the `CollectorPanel` telemetry read surface.

## [02]-[OBSERVATION_ROUTES]

- Owner: the read-only observation routes — `EvidenceTimelineRoute`, `BenchmarkRoute`, `CollectorPanel` — each a thin subscriber over a `projection` fold or the telemetry collector holding no domain state.
- Cases: `EvidenceTimelineRoute` carries receipt envelopes whole and renders them in HLC order, and the band render is a total `Match` over the `vector#VERSION_VECTOR` `CausalVerdict` each `causalStore` cell tags onto its row — the `Before`/`After`/`Equal` arms draw the row in its definite ordered slot, the `Concurrent` arm marks a concurrent pair, and the `ConcurrentUncertain` arm draws the "within +/-`radiusMs` ms" band and the concurrent-uncertain pair marking, reading the verdict from `causalStore` and the radius from `skew#SKEW_BAND` `SkewInterval.radiusMs` by reference so the browser dashboard renders honest interval confidence without recomputing the HLC fold or re-deriving the band as an inline midpoint comparison; `BenchmarkRoute` renders only when fingerprint-gated by comparing the decoded `HostFingerprintWire.fingerprintId` identity the C# fingerprint owner already computed and stamped onto the wire against the current host's decoded identity, so an unverifiable claim never displays as verified and no browser-side identity reconstruction can drift from the C# stamp; `CollectorPanel` reads the telemetry collector. The timeline composes the `@tanstack/react-virtual` virtualizer for the receipt stream and the band decorates the virtualized rows.
- Entry: dashboards sit on the evidence fold and the receipt store and read, never emit; the read-versus-emit split is explicit — dashboards read the collector while instrumentation belongs to `platform`; the routes subscribe to their fold only through the one `binding/atom.md` `AtomBinding`; a `Result`-bearing fold renders through the binding's `Result.builder` chain so the loading/success/failure arms render uniformly.
- Packages: `react`, `react-dom`, `@tanstack/react-virtual`, `@tanstack/react-table`, `@effect/opentelemetry`, `effect`.
- Growth: a new observation route lands as one route module over an existing store; a new telemetry panel reads the same collector, never a parallel surface; a new causal-verdict band posture lands as one `Match` arm over the `CausalVerdict` family, never a parallel ordering surface or an inline midpoint comparison.
- Boundary: a benchmark claim displayed without the fingerprint gate is the named defect; a browser-side reconstruction of the fingerprint identity string (a hand-rolled stamp-line over `{ os, arch, processors, stamps }`) beside the decoded `HostFingerprintWire.fingerprintId` the C# owner already stamped is the named cross-language drift defect — the identity is computed once at the C# fingerprint owner and crosses as one decoded field, never re-minted on this leaf; a `ui`-side re-derivation of HLC order or the skew band (an inline midpoint comparison beside the `bandsOverlap` the `vector#VERSION_VECTOR` `skewVerdict` already composed) is the named defect — this leaf renders the settled `CausalVerdict` and `SkewInterval.radiusMs` and re-derives nothing; `CollectorPanel` crosses no wire contract of its own and references no telemetry wire type; the routes never re-decode a value the `interchange` rail admitted; `@effect/opentelemetry` is used strictly as a collector reader and a route that emits a span is the named defect.

```ts contract
import type { BenchmarkClaimWire, HostFingerprintWire } from "@rasm/ts/interchange";
import type { CausalCell } from "@rasm/ts/projection/causality/vector";
import { Match, Option } from "effect";
import { useVirtualizer } from "@tanstack/react-virtual";
import * as React from "react";

// --- [SERVICES] --------------------------------------------------------------------------

interface BenchmarkRoute {
  readonly render: (claim: BenchmarkClaimWire, host: Option.Option<HostFingerprintWire>) => Option.Option<React.ReactElement>;
}

const renderBenchmark = (
  claim: BenchmarkClaimWire,
  host: Option.Option<HostFingerprintWire>,
): Option.Option<React.ReactElement> =>
  Option.match(host, {
    onNone: () => Option.none(),
    onSome: (current) =>
      claim.fingerprint.fingerprintId === current.fingerprintId
        ? Option.some(React.createElement(BenchmarkCard, { claim, host: current, verified: true }))
        : Option.none(),
  });

const BenchmarkCard: React.FC<{
  readonly claim: BenchmarkClaimWire;
  readonly host: HostFingerprintWire;
  readonly verified: boolean;
}> = ({ claim, host, verified }) =>
  React.createElement(
    "section",
    { "data-verified": verified, "aria-label": `benchmark ${claim.family}/${claim.route}` },
    React.createElement("data", { value: claim.median }, `median ${claim.median}`),
    React.createElement("data", { value: claim.p95 }, `p95 ${claim.p95}`),
    React.createElement("span", null, `${host.os} ${host.arch} ×${host.processors}`),
  );

// --- [OPERATIONS] ------------------------------------------------------------------------

// The band render is a total `Match` over the settled `CausalVerdict` each `causalStore` cell
// tags onto its row — the `ConcurrentUncertain` arm draws the "within +/-`radiusMs` ms" band
// and the concurrent-uncertain marking, reading the radius from `SkewInterval.radiusMs` BY
// REFERENCE. No HLC order or band is re-derived here; the verdict is the projection fold's.
// `CausalCell` carries both the `verdict` and the held `band: SkewInterval` the projection
// fold tagged onto the op — the row reads the radius from `cell.band.radiusMs`, never a parallel
// skew field, so the band is single-sourced on the cell the `causalStore` fold owns.
type EvidenceRow = {
  readonly id: string;
  readonly label: string;
  readonly cell: CausalCell;
};

const bandRender = (row: EvidenceRow): React.ReactElement =>
  Match.value(row.cell.verdict).pipe(
    Match.tag("ConcurrentUncertain", () =>
      React.createElement(
        "div",
        { "data-causal": "concurrent-uncertain", "aria-label": `causally ambiguous within ±${row.cell.band.radiusMs} ms` },
        React.createElement("span", { className: "evidence-row" }, row.label),
        React.createElement("span", { className: "skew-band", style: { width: `${row.cell.band.radiusMs * 2}px` } }),
      )),
    Match.tag("Concurrent", () =>
      React.createElement("div", { "data-causal": "concurrent" }, React.createElement("span", { className: "evidence-row" }, row.label))),
    Match.orElse(() =>
      React.createElement("div", { "data-causal": "ordered" }, React.createElement("span", { className: "evidence-row" }, row.label))),
  );

// `EvidenceTimelineRoute` virtualizes the HLC-ordered rows and decorates each with its band.
const EvidenceTimeline: React.FC<{ readonly rows: ReadonlyArray<EvidenceRow> }> = ({ rows }) => {
  const parentRef = React.useRef<HTMLDivElement>(null);
  const virtualizer = useVirtualizer({
    count: rows.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 44,
  });
  return React.createElement(
    "div",
    { ref: parentRef, className: "evidence-timeline", role: "log", "aria-label": "evidence timeline" },
    React.createElement(
      "div",
      { style: { height: `${virtualizer.getTotalSize()}px`, position: "relative" } },
      ...virtualizer.getVirtualItems().map((item) =>
        React.createElement(
          "div",
          {
            key: rows[item.index]!.id,
            "data-index": item.index,
            ref: virtualizer.measureElement,
            style: { position: "absolute", top: 0, left: 0, width: "100%", transform: `translateY(${item.start}px)` },
          },
          bandRender(rows[item.index]!),
        ),
      ),
    ),
  );
};
```

The gate compares the decoded `HostFingerprintWire.fingerprintId` the C# fingerprint owner computes and stamps onto the wire — the identity crosses as one settled field, never reconstructed in the browser, so a benchmark verified in any runtime is verifiable here without a TS-side stamp-line that could drift from the C# ordering, separator, or escaping; the staleness comparison reads only the decoded wire field and the wire carries no `machineKey`.

The `EvidenceTimelineRoute` band render reads the settled `vector#VERSION_VECTOR` `CausalVerdict` from each `CausalCell` and the `skew#SKEW_BAND` `SkewInterval.radiusMs` by reference through `binding/atom.md` `AtomBinding`; the `ConcurrentUncertain` arm of the total `Match` draws the "within +/-`radiusMs` ms" band and the concurrent-uncertain pair marking over the `@tanstack/react-virtual` rows. The `bandsOverlap` widening that produces `ConcurrentUncertain` is composed by the `version-vector` `skewVerdict`, not re-derived here as an inline midpoint comparison — this leaf renders the verdict and the radius the projection fold already settled.
