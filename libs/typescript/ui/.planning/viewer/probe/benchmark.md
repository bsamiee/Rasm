# [UI_BENCHMARK]

`viewer/probe/benchmark.ts` renders the benchmark plane (AH:60): the wire-decoded `Claim` — measurement rows plus its embedded `HostFingerprint`, already identity-gated at `wire` (`Claim.admit` refuses a foreign-host claim before this module ever sees it) — displays beside a LOCAL capture: the same metric vocabulary measured on this host through the render engines' own counters (deck's `DeckMetrics` sink, the render-loop frame timer) and the host's own fingerprint probe. The comparison is a keyed fold over metric labels producing per-row deltas — evidence for the operator, never a pass/fail gate — and the local fingerprint's whole purpose is showing WHY numbers differ when they do.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                          |
| :-----: | :-------------- | :----------------------------------------------------------------- |
|   [1]   | `METRIC_PROBES` | the local metric capture — deck metrics sink, frame timing fold     |
|   [2]   | `HOST_PROBE`    | the local host-fingerprint capture mirroring the wire's fields      |
|   [3]   | `CLAIM_BOARD`   | the claim-vs-local comparison fold and its display rows             |

## [2]-[METRIC_PROBES]

- Owner: `Benchmark.metrics` — the local capture: deck's `_onMetrics` callback is the GPU-side sink (`DeckMetrics` — `fps`, `gpuTime`, `cpuTime`, `pickTime`, `gpuMemory`), folded into a bounded rolling window (a `Chunk`-backed accumulator with mean/peak projections — `computation`'s seed-fold law applied verbatim); the three loop contributes a frame-time row measured inside its own `setAnimationLoop` tick (delta between ticks — the loop is already the timing source, no second RAF); each captured quantity lands as the SAME metric row shape the wire claim carries — `{ label, value, unit }` — so comparison is a keyed join, not a shape adaptation.
- Packages: `@deck.gl/core` (`DeckMetrics` via the `_onMetrics` prop on the surface's overlay), `effect` (`Chunk`, the fold), `@rasm/ts/wire/vocab` (`Claim` — the metric row shape is its vocabulary).
- Law: probes are passive — metric capture never alters render behavior (no forced redraws, no `_animate` flips for measurement's sake); an idle viewport reports idle numbers truthfully.
- Law: windows are policy rows — sample count and projection kind (`mean`, `p95`, `peak`) are one `as const` record; a per-metric bespoke window is the named defect.
- Boundary: `Deck`/renderer acquisition is `viewer/geo/layers`/`viewer/scene/glb`'s — the sinks arrive as wiring parameters; React tree render cost (`Profiler`) is app-plane telemetry, not this probe.

```typescript
import type { DeckMetrics } from "@deck.gl/core"
import { Claim } from "@rasm/ts/wire/vocab"
import { Array, Chunk } from "effect"

const _WINDOW = { samples: 120 } as const

declare namespace Benchmark {
  type Metric = { readonly label: string; readonly value: number; readonly unit: string }
  type Trace = Chunk.Chunk<DeckMetrics>
}

const _observe = (trace: Benchmark.Trace, sample: DeckMetrics): Benchmark.Trace =>
  Chunk.takeRight(Chunk.append(trace, sample), _WINDOW.samples)

const _rows = (trace: Benchmark.Trace): ReadonlyArray<Benchmark.Metric> => {
  const samples = Chunk.toReadonlyArray(trace)
  const mean = (read: (m: DeckMetrics) => number): number =>
    samples.length === 0 ? 0 : samples.reduce((total, m) => total + read(m), 0) / samples.length
  return [
    { label: "fps", value: mean((m) => m.fps), unit: "1/s" },
    { label: "gpu-time", value: mean((m) => m.gpuTime), unit: "ms" },
    { label: "cpu-time", value: mean((m) => m.cpuTime), unit: "ms" },
    { label: "pick-time", value: mean((m) => m.pickTime), unit: "ms" },
  ]
}
```

## [3]-[HOST_PROBE]

- Owner: `Benchmark.host` — the local fingerprint mirroring the wire's `HostFingerprint` fields (`machine`, `arch`, `cores`, `runtime`): `cores` reads `navigator.hardwareConcurrency`; `runtime` reads the user-agent brand; `machine`/`arch` read the WebGPU `GPUAdapterInfo` (`vendor`/`architecture` — the adapter probe already ran at backend selection, so the info arrives as a parameter from the renderer row, never a second `requestAdapter`); absent WebGPU, the fields carry the declared `"<unavailable>"` literal rather than fabricated values.
- Law: the local fingerprint NEVER gates — the identity gate lives at `wire` (`Claim.admit` against `AppIdentity`); this probe exists to display divergence context (different GPU, fewer cores) beside metric deltas.
- Law: fingerprint capture is once-per-session — the value is stable for the process lifetime and rides a `keepAlive` atom.

```typescript
const _host = (adapter: { readonly vendor: string; readonly architecture: string } | null): Claim.Host =>
  new Claim.Host({
    machine: adapter?.vendor ?? "<unavailable>",
    arch: adapter?.architecture ?? "<unavailable>",
    cores: Math.max(1, globalThis.navigator.hardwareConcurrency),
    runtime: globalThis.navigator.userAgent,
  })
```

## [4]-[CLAIM_BOARD]

- Owner: `Benchmark.board` — the comparison fold: join the admitted claim's metric rows with the local rows BY LABEL (a keyed record join — labels present only on one side render as one-sided evidence, never dropped), compute the signed delta and ratio per joined row, and pair the claim's fingerprint with the local one for the divergence header; the result is one `Board` value the panel renders — claim column, local column, delta column, host context.
- Law: units must agree to compare — a label joined across differing units renders as incomparable evidence (both shown, no delta), because a silent unit conversion is fabricated science.
- Law: display formats through `intl/format` number rows (`compact` for times, `plain` for ratios); tones key off delta sign through one `as const` table.
- Boundary: claims arrive already admitted (`wire/codec/claim`'s gate); persisting local runs as new claims is app egress through `wire` encode.

```typescript
import { HashMap, Option } from "effect"

declare namespace Board {
  type Row = {
    readonly label: string
    readonly claimed: Option.Option<Benchmark.Metric>
    readonly local: Option.Option<Benchmark.Metric>
    readonly delta: Option.Option<number>
  }
}

const _board = (claim: Claim, local: ReadonlyArray<Benchmark.Metric>): ReadonlyArray<Board.Row> => {
  const mine = HashMap.fromIterable(Array.map(local, (row) => [row.label, row] as const))
  return Array.map(claim.metrics, (row) => {
    const held = HashMap.get(mine, row.label)
    return {
      label: row.label,
      claimed: Option.some({ label: row.label, value: row.value, unit: row.unit }),
      local: held,
      delta: Option.flatMap(held, (near) => (near.unit === row.unit ? Option.some(near.value - row.value) : Option.none())),
    }
  })
}

const Benchmark: {
  readonly window: typeof _WINDOW
  readonly observe: typeof _observe
  readonly rows: typeof _rows
  readonly host: typeof _host
  readonly board: typeof _board
} = {
  window: _WINDOW,
  observe: _observe,
  rows: _rows,
  host: _host,
  board: _board,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Benchmark }
```
