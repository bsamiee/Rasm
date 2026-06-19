# [UI_OBSERVATION_ROUTES]

The read-only dashboard routes over the `projection` folds — leaf subscribers that read and never emit. `EvidenceTimelineRoute` renders receipt envelopes in HLC order with the `SkewBand` confidence interval, `BenchmarkRoute` renders only when fingerprint-gated, and `CollectorPanel` reads the telemetry collector. These are data-dashboard surfaces, distinct in kind from the render-engine surfaces. Each route subscribes to its fold only through the `binding/atom-binding.md` `AtomBinding`; dashboards read the collector while instrumentation belongs to `platform`.

## [1]-[INDEX]

One cluster: `OBSERVATION_ROUTES` owns the evidence-timeline, benchmark, and collector dashboards.

## [2]-[OBSERVATION_ROUTES]

- Owner: the read-only observation routes — `EvidenceTimelineRoute`, `BenchmarkRoute`, `CollectorPanel` — each a thin subscriber over a `projection` fold or the telemetry collector holding no domain state.
- Cases: `EvidenceTimelineRoute` carries receipt envelopes whole and renders them in HLC order with the `SkewBand` confidence interval from the `projection` `EvidenceFeed`, so a browser dashboard renders "within +/-N ms confidence" without recomputing the HLC fold; `BenchmarkRoute` renders only when fingerprint-gated by comparing the decoded `HostFingerprintWire.fingerprintId` identity the C# fingerprint owner already computed and stamped onto the wire against the current host's decoded identity, so an unverifiable claim never displays as verified and no browser-side identity reconstruction can drift from the C# stamp; `CollectorPanel` reads the telemetry collector. The timeline composes the `@tanstack/react-virtual` virtualizer for the receipt stream.
- Entry: dashboards sit on the evidence fold and the receipt store and read, never emit; the read-versus-emit split is explicit — dashboards read the collector while instrumentation belongs to `platform`; the routes subscribe to their fold only through the one `binding/atom-binding.md` `AtomBinding`; a `Result`-bearing fold renders through the binding's `Result.builder` chain so the loading/success/failure arms render uniformly.
- Packages: `react`, `react-dom`, `@tanstack/react-virtual`, `@tanstack/react-table`, `@effect/opentelemetry`, `effect`.
- Growth: a new observation route lands as one route module over an existing store; a new telemetry panel reads the same collector, never a parallel surface.
- Boundary: a benchmark claim displayed without the fingerprint gate is the named defect; a browser-side reconstruction of the fingerprint identity string (a hand-rolled stamp-line over `{ os, arch, processors, stamps }`) beside the decoded `HostFingerprintWire.fingerprintId` the C# owner already stamped is the named cross-language drift defect — the identity is computed once at the C# fingerprint owner and crosses as one decoded field, never re-minted on this leaf; `CollectorPanel` crosses no wire contract of its own and references no telemetry wire type; the routes never re-decode a value the `interchange` rail admitted; `@effect/opentelemetry` is used strictly as a collector reader and a route that emits a span is the named defect.

```ts contract
import type { BenchmarkClaimWire, HostFingerprintWire } from "@rasm/ts/interchange";
import { Option } from "effect";
import * as React from "react";

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
```

The gate compares the decoded `HostFingerprintWire.fingerprintId` the C# fingerprint owner computes and stamps onto the wire — the identity crosses as one settled field, never reconstructed in the browser, so a benchmark verified in any runtime is verifiable here without a TS-side stamp-line that could drift from the C# ordering, separator, or escaping; the staleness comparison reads only the decoded wire field and the wire carries no `machineKey`.
