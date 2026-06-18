# [UI_OBSERVATION_ROUTES]

The read-only dashboard routes over the `projection` folds — leaf subscribers that read and never emit. `EvidenceTimelineRoute` renders receipt envelopes in HLC order with the `SkewBand` confidence interval, `BenchmarkRoute` renders only when fingerprint-gated, and `CollectorPanel` reads the telemetry collector. These are data-dashboard surfaces, distinct in kind from the render-engine surfaces. Each route subscribes to its fold only through the `binding/atom-binding.md` `AtomBinding`; dashboards read the collector while instrumentation belongs to `platform`.

## [1]-[INDEX]

One cluster: `OBSERVATION_ROUTES` owns the evidence-timeline, benchmark, and collector dashboards.

## [2]-[OBSERVATION_ROUTES]

- Owner: the read-only observation routes — `EvidenceTimelineRoute`, `BenchmarkRoute`, `CollectorPanel` — each a thin subscriber over a `projection` fold or the telemetry collector holding no domain state.
- Cases: `EvidenceTimelineRoute` carries receipt envelopes whole and renders them in HLC order with the `SkewBand` confidence interval from the `projection` `EvidenceFeed`, so a browser dashboard renders "within +/-N ms confidence" without recomputing the HLC fold; `BenchmarkRoute` renders only when fingerprint-gated by the host-fingerprint shape on `interchange` `decode-rail#TS_PROJECTION`, so an unverifiable claim never displays as verified; `CollectorPanel` reads the telemetry collector. The timeline composes the `@tanstack/react-virtual` virtualizer for the receipt stream.
- Entry: dashboards sit on the evidence fold and the receipt store and read, never emit; the read-versus-emit split is explicit — dashboards read the collector while instrumentation belongs to `platform`; the routes subscribe to their fold only through the one `binding/atom-binding.md` `AtomBinding`; a `Result`-bearing fold renders through the binding's `Result.builder` chain so the loading/success/failure arms render uniformly.
- Packages: `react`, `react-dom`, `@tanstack/react-virtual`, `@tanstack/react-table`, `@effect/opentelemetry`, `effect`.
- Growth: a new observation route lands as one route module over an existing store; a new telemetry panel reads the same collector, never a parallel surface.
- Boundary: a benchmark claim displayed without the fingerprint gate is the named defect; `CollectorPanel` crosses no wire contract of its own and references no telemetry wire type; the routes never re-decode a value the `interchange` rail admitted; `@effect/opentelemetry` is used strictly as a collector reader and a route that emits a span is the named defect.

```ts contract
interface BenchmarkRoute {
  readonly render: (claim: BenchmarkClaimWire, host: Option.Option<HostFingerprintWire>) => Option.Option<React.ReactElement>;
}

const stampLine = (f: HostFingerprintWire): string =>
  Array.fromIterable(Object.entries(f.stamps)).pipe(
    Array.sort(Order.mapInput(Order.string, ([k]: readonly [string, string]) => k)),
    Array.map(([k, v]) => `${k}=${v}`),
    Array.join(";"),
    (stamps) => `${f.os}|${f.arch}|${f.processors}|${stamps}`,
  );

const renderBenchmark = (
  claim: BenchmarkClaimWire,
  host: Option.Option<HostFingerprintWire>,
): Option.Option<React.ReactElement> =>
  Option.match(host, {
    onNone: () => Option.none(),
    onSome: (current) =>
      stampLine(claim.fingerprint) === stampLine(current)
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

The `stampLine` projection reproduces the C#-owned fingerprint identity bit-identically from the decoded `HostFingerprintWire` shape — the ordinal stamp-line over `{ os, arch, processors, stamps }` sorted by key — so a benchmark verified in any runtime is verifiable here; the staleness comparison reads only the decoded wire shape and the wire carries no `machineKey`.
