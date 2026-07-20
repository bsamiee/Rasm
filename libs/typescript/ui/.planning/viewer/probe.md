# [UI_PROBE]

The one render-evidence owner: benchmark and receipt are two lanes of a single discipline — capture a local truth with the render engines' own counters, pair it with the wire-decoded claim, and render the comparison as operator evidence that never gates, never faults, never retries. The benchmark lane folds deck's `DeckMetrics` sink and the frame-loop timer into one bounded seed fold, mirrors the host fingerprint locally so divergent numbers carry their divergence context, and joins claim rows against local rows BY LABEL into a comparison board. The receipt lane captures a deterministic fixed-extent framebuffer through the renderer's async readback, delegates the hash to the branch's one content mint, and compares structurally against the wire `RenderReceipt` — both proofs render side by side. Claims arrive already identity-gated (`Claim.admit` at `core/interchange/codec` refuses a foreign-host claim before this module sees it), and the local fingerprint exists to explain deltas, never to admit or refuse. The module is `ui/viewer/src/probe.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                          | [PUBLIC] |
| :-----: | :-------------- | :------------------------------------------------------------------------------ | :------- |
|  [01]   | `METRIC_FOLD`   | the local capture — deck metrics sink and frame timing as one bounded seed fold | `Probe`  |
|  [02]   | `HOST_MIRROR`   | the local host-fingerprint capture mirroring the wire's fields                  | `Probe`  |
|  [03]   | `CLAIM_BOARD`   | the claim-versus-local label-keyed join and its display rows                    | `Probe`  |
|  [04]   | `CAPTURE_FOLD`  | the deterministic framebuffer capture and the kernel hash delegate              | `Probe`  |
|  [05]   | `EVIDENCE_ROWS` | tone tables, bounded verdict history, the never-a-gate law                      | `Probe`  |

## [02]-[METRIC_FOLD]

[METRIC_FOLD]:
- Owner: `Probe.rows` — the local capture: deck's `_onMetrics` callback is the GPU-side sink carrying the FULL shipped `DeckMetrics` roster — timing (`fps`, `gpuTime`/`gpuTimePerFrame`, `cpuTime`/`cpuTimePerFrame`, `pickTime`, `setPropsTime`, `updateAttributesTime`), counters (`framesRedrawn`, `pickCount`, `pickLayersCount`, `updateAttributesCount`, `layersCount`, `drawLayersCount`, `updateLayersCount`), and the `gpuMemory` split (`bufferMemory`, `textureMemory`, `renderbufferMemory`) — and the three renderer's `info` counter surface (`render.frameCalls`/`drawCalls`/`triangles`, `memory.geometries`/`textures`/`total`) lands beside it in one merged sample per tick; the window's projections run as ONE seed fold — raw sums accumulate in a single `Chunk.reduce` pass, timing means project at read, a cumulative counter windows as its latest sample, a memory gauge windows as its peak — so a new statistic (a p95 seed, a compute-pass counter) is one seed field and one row, never a second traversal; each captured quantity lands as the SAME metric row shape the wire claim carries — `label`/`value`/`unit` derives from `Claim` itself — so comparison is a keyed join, not a shape adaptation.
- Packages: `@deck.gl/core` (`DeckMetrics` via the `_onMetrics` prop on the surface's overlay); `three` (`WebGPURenderer["info"]` — the render/compute/memory counter surface through `three/webgpu`); `@rasm/ts/core` (`Claim` — the metric row shape IS its vocabulary); `effect` (`Chunk`, `Number`, `pipe`).
- Law: probes are passive — metric capture never alters render behavior (no forced redraws, no `_animate` flips for measurement's sake); an idle viewport reports idle numbers truthfully.
- Law: the tick assembles the sample — the deck sink caches its last payload, the frame loop reads `renderer.info` inside its own tick (the loop is already the timing source, no second RAF), and a scene-owned loop flips `info.autoReset` false with `info.reset()` closing each tick so per-frame counters stay frame-true.
- Law: windows are policy rows — sample count and projection kind live in one `as const` record; a per-metric bespoke window is the named defect.
- Law: the trace renders as a live series, never table rows — `Probe.aligned(trace)` projects the rolling window into the aligned columns `view/chart#SERIES_SURFACE` streams through `setData` (the metric board is a chart cohort synced by one key), and the summary rows feed the claim board; a metric timeline rendered through `view/table` is the named defect.
- Law: residency metrics tap the scene broadcast — `scene#RESIDENCY_GRAFT`'s surplus arrival lane feeds an arrival-rate row and `Glb.Loop.refusals` feeds the refusal-rate row beside it, both in the same trace shape — broadcast taps, never a second port subscription; the taps ride `system/hook` registry rows (`rasm.ui.scene.residency`), so probe boards, history capture, and the app bridge consume one rail.
- Boundary: `Deck`/renderer acquisition is `geo`/`scene`'s — the sinks arrive as wiring parameters; React tree, vitals, and long-frame evidence is `system/vital`'s lane rendered on this same board through the shared row shape; render-vital emission to the OTel spine is the runtime plane's, fed by an app-composed `system/hook` tap over these local rows, so this probe stays a display surface and mints no instrument.

```typescript
import type { DeckMetrics } from "@deck.gl/core"
import type { Claim } from "@rasm/ts/core"
import { Chunk, Number, pipe } from "effect"
import type { WebGPURenderer } from "three/webgpu"

type Metric = Claim["metrics"][number]

const _WINDOW = { samples: 120 } as const

type _Info = WebGPURenderer["info"]

declare namespace Probe {
  type Sample = {
    readonly deck: DeckMetrics
    readonly draw: Pick<_Info["render"], "frameCalls" | "drawCalls" | "triangles">
    readonly held: Pick<_Info["memory"], "geometries" | "textures" | "total">
  }
  type Trace = Chunk.Chunk<Probe.Sample>
}

type _Sums = {
  readonly count: number
  readonly fps: number
  readonly gpu: number
  readonly cpu: number
  readonly pick: number
  readonly gpuFrame: number
  readonly cpuFrame: number
  readonly props: number
  readonly attrs: number
  readonly redrawn: number
  readonly picks: number
  readonly layers: number
  readonly drawnLayers: number
  readonly updatedLayers: number
  readonly mem: number
  readonly buffers: number
  readonly texBytes: number
  readonly targets: number
  readonly calls: number
  readonly draws: number
  readonly tris: number
  readonly geometries: number
  readonly textures: number
  readonly total: number
}

const _SEED: _Sums = {
  count: 0, fps: 0, gpu: 0, cpu: 0, pick: 0, gpuFrame: 0, cpuFrame: 0, props: 0, attrs: 0,
  redrawn: 0, picks: 0, layers: 0, drawnLayers: 0, updatedLayers: 0,
  mem: 0, buffers: 0, texBytes: 0, targets: 0,
  calls: 0, draws: 0, tris: 0, geometries: 0, textures: 0, total: 0,
}

const _stepped = (acc: _Sums, sample: Probe.Sample): _Sums => ({
  count: acc.count + 1,
  fps: acc.fps + sample.deck.fps,
  gpu: acc.gpu + sample.deck.gpuTime,
  cpu: acc.cpu + sample.deck.cpuTime,
  pick: acc.pick + sample.deck.pickTime,
  gpuFrame: acc.gpuFrame + sample.deck.gpuTimePerFrame,
  cpuFrame: acc.cpuFrame + sample.deck.cpuTimePerFrame,
  props: acc.props + sample.deck.setPropsTime,
  attrs: acc.attrs + sample.deck.updateAttributesTime,
  redrawn: sample.deck.framesRedrawn, // a cumulative counter windows as its latest sample, never a mean
  picks: sample.deck.pickCount,
  layers: sample.deck.layersCount,
  drawnLayers: sample.deck.drawLayersCount,
  updatedLayers: sample.deck.updateLayersCount,
  mem: Number.max(acc.mem, sample.deck.gpuMemory), // a gauge windows as its peak
  buffers: Number.max(acc.buffers, sample.deck.bufferMemory),
  texBytes: Number.max(acc.texBytes, sample.deck.textureMemory),
  targets: Number.max(acc.targets, sample.deck.renderbufferMemory),
  calls: acc.calls + sample.draw.frameCalls,
  draws: acc.draws + sample.draw.drawCalls,
  tris: acc.tris + sample.draw.triangles,
  geometries: sample.held.geometries, // residency counts window as their latest sample
  textures: sample.held.textures,
  total: Number.max(acc.total, sample.held.total),
})

const _observe = (trace: Probe.Trace, sample: Probe.Sample): Probe.Trace =>
  Chunk.takeRight(Chunk.append(trace, sample), _WINDOW.samples)

const _rows = (trace: Probe.Trace): ReadonlyArray<Metric> =>
  pipe(Chunk.reduce(trace, _SEED, _stepped), (sums) =>
    sums.count === 0
      ? [] // an empty window carries no rows — a zero-sample mean is fabricated evidence
      : [
          { label: "fps", value: sums.fps / sums.count, unit: "1/s" },
          { label: "gpu-time", value: sums.gpu / sums.count, unit: "ms" },
          { label: "cpu-time", value: sums.cpu / sums.count, unit: "ms" },
          { label: "gpu-time-frame", value: sums.gpuFrame / sums.count, unit: "ms" },
          { label: "cpu-time-frame", value: sums.cpuFrame / sums.count, unit: "ms" },
          { label: "pick-time", value: sums.pick / sums.count, unit: "ms" },
          { label: "setprops-time", value: sums.props / sums.count, unit: "ms" },
          { label: "attr-update-time", value: sums.attrs / sums.count, unit: "ms" },
          { label: "frames-redrawn", value: sums.redrawn, unit: "1" },
          { label: "pick-count", value: sums.picks, unit: "1" },
          { label: "layers", value: sums.layers, unit: "1" },
          { label: "layers-drawn", value: sums.drawnLayers, unit: "1" },
          { label: "layers-updated", value: sums.updatedLayers, unit: "1" },
          { label: "gpu-memory", value: sums.mem, unit: "bytes" },
          { label: "buffer-memory", value: sums.buffers, unit: "bytes" },
          { label: "texture-memory", value: sums.texBytes, unit: "bytes" },
          { label: "renderbuffer-memory", value: sums.targets, unit: "bytes" },
          { label: "render-calls", value: sums.calls / sums.count, unit: "1" },
          { label: "draw-calls", value: sums.draws / sums.count, unit: "1" },
          { label: "triangles", value: sums.tris / sums.count, unit: "1" },
          { label: "geometries-resident", value: sums.geometries, unit: "1" },
          { label: "textures-resident", value: sums.textures, unit: "1" },
          { label: "renderer-memory", value: sums.total, unit: "bytes" },
        ])

const _aligned = (trace: Probe.Trace): readonly [Float64Array, Float64Array, Float64Array, Float64Array] =>
  pipe(Chunk.toReadonlyArray(trace), (samples) => [
    Float64Array.from(samples, (_, rank) => rank),
    Float64Array.from(samples, (sample) => sample.deck.fps),
    Float64Array.from(samples, (sample) => sample.deck.gpuTime),
    Float64Array.from(samples, (sample) => sample.deck.cpuTime),
  ] as const)
```

## [03]-[HOST_MIRROR]

[HOST_MIRROR]:
- Owner: `Probe.host` — the local fingerprint mirroring the wire's `HostFingerprint` fields: `print` is the app's own identity host value handed in as a parameter — the probe never mints identity; `cores` reads `navigator.hardwareConcurrency`; `runtime` reads the user-agent brand; `machine`/`arch` read the WebGPU adapter info (`vendor`/`architecture` — the adapter probe already ran at `scene`'s backend selection, so the info arrives as an `Option` parameter from the renderer row, never a second `requestAdapter`); absent WebGPU, `Option.none` folds to the declared `"<unavailable>"` literal rather than fabricated values.
- Packages: `@rasm/ts/core` (`Claim` — `Claim.Host` IS `HostFingerprint`); `effect` (`Option`, `pipe`); `@webgpu/types` (ambient adapter-info shape).
- Law: the local fingerprint NEVER gates — the identity gate lives at the codec (`Claim.admit` against `AppIdentity`); this probe exists to display divergence context (different GPU, fewer cores) beside metric deltas.
- Law: fingerprint capture is once-per-session — the value is stable for the process lifetime and its atom pins through `Atom.keepAlive`, so the registry's idle TTL never re-runs the capture.

```typescript
import { Claim } from "@rasm/ts/core"
import { Number, Option, pipe } from "effect"

const _host = (
  print: string,
  adapter: Option.Option<{ readonly vendor: string; readonly architecture: string }>,
): Claim.Host =>
  pipe(
    Option.getOrElse(adapter, () => ({ vendor: "<unavailable>", architecture: "<unavailable>" })),
    (info) =>
      new Claim.Host({
        print,
        machine: info.vendor,
        arch: info.architecture,
        cores: Number.max(1, globalThis.navigator.hardwareConcurrency),
        runtime: globalThis.navigator.userAgent,
      }),
  )
```

## [04]-[CLAIM_BOARD]

[CLAIM_BOARD]:
- Owner: `Probe.board` — the comparison fold: join the admitted claim's metric rows with the local rows BY LABEL (a keyed record join — labels present on only one side render as one-sided evidence, never dropped), compute the signed delta per joined row, and pair the claim's fingerprint with the local one for the divergence header; the result is one board value the panel renders — claim column, local column, delta column, host context.
- Law: units agree or the row is incomparable — a label joined across differing units renders both values with no delta, because a silent unit conversion is fabricated science.
- Law: display formats through `Format` number rows (`system/intl`); tones key off delta sign through the `[6]` table.
- Boundary: claims arrive already admitted; persisting local runs as new claims is app egress through wire encode.

```typescript
import { Array, HashMap, HashSet } from "effect"

declare namespace Probe {
  type BoardRow = {
    readonly label: string
    readonly claimed: Option.Option<Metric>
    readonly local: Option.Option<Metric>
    readonly delta: Option.Option<number>
  }
}

const _board = (claim: Claim, local: ReadonlyArray<Metric>): ReadonlyArray<Probe.BoardRow> => {
  const mine = HashMap.fromIterable(Array.map(local, (row) => [row.label, row] as const))
  const named = HashSet.fromIterable(Array.map(claim.metrics, (row) => row.label))
  return Array.appendAll(
    Array.map(claim.metrics, (row) => {
      const held = HashMap.get(mine, row.label)
      return {
        label: row.label,
        claimed: Option.some(row),
        local: held,
        delta: Option.flatMap(held, (near) => (near.unit === row.unit ? Option.some(near.value - row.value) : Option.none())),
      }
    }),
    Array.filterMap(local, (row) =>
      HashSet.has(named, row.label)
        ? Option.none()
        : Option.some({ label: row.label, claimed: Option.none<Metric>(), local: Option.some(row), delta: Option.none<number>() })),
  )
}
```

## [05]-[CAPTURE_FOLD]

[CAPTURE_FOLD]:
- Owner: `Probe.capture` — the capture discipline as one fold: render into a fixed-extent target (capture never reads the live swap chain — DPR and resize break determinism), await the settled frame (`compileAsync` before first capture; capture runs after the residency fold quiesces), read pixels through `renderer.readRenderTargetPixelsAsync(target, 0, 0, width, height, buffer)` (the WebGPU-safe async readback; a synchronous read stalls the pipeline and is the named defect), and delegate the octets to the mint delegate — the branch's content mint has exactly the delegation sites `core/value/contentKey` enumerates, so the delegate arrives as a parameter the composition satisfies from the runtime worker's mint site, and this module carries no hash code.
- Packages: `three` (the render-target family, the async readback — members verified against the shipped runtime); `@rasm/ts/core` (`ContentKey`, `RenderReceipt`); `effect` (`DateTime`, `Effect`, `Equal`).
- Law: capture parameters are a policy row — extent, target format, and the settle predicate live in one `as const` record; a capture with ad-hoc parameters produces an incomparable hash and is the named defect.
- Law: the comparison is structural — the local key and the receipt's key compare through `Equal.equals` on the brand; the verdict is `{ view, expected, actual, matched, at }` — a plain data row beside the wire receipt's own C#-computed `matched`/`at`, so the operator reads both proofs.
- Law: the MRT/post chain feeding a G-buffer capture is the same fold with a different target row — the WebGPU arm's `PostProcessing` pipeline with `three/tsl`'s `mrt({ … })` names the targets, and no second capture fold exists.
- Growth: hashing graduates to the GPU when readback dominates — a `typegpu` reduction kernel over the capture buffer on the scene-published device (`tgpu.initFromDevice`, `scene#BACKEND_SELECT`'s seam) feeds the same mint delegate; the delegate signature never changes, so the ladder is invisible to consumers.
- Law: the capture rail is woven — `Effect.withSpan("rasm.ui.probe.capture")` names the readback-mint-compare trip with the view as span attribute and log annotation, so a capture correlates with the residency and pivot spans on the app bridge; the verdict stays display evidence and no metric exists here, because a mismatch is never a fault and never a series.
- Boundary: the wire receipt's decode is the codec's; renderer and scene arrive as parameters from `scene`; verdict transport to any journal is app egress.

```typescript
import type { ContentKey, RenderReceipt } from "@rasm/ts/core"
import { DateTime, Effect, Equal } from "effect"

const _CAPTURE = { width: 1024, height: 1024 } as const

declare namespace Probe {
  type Readback = (width: number, height: number) => Effect.Effect<Uint8Array>
  type Verdict = {
    readonly view: string
    readonly expected: ContentKey
    readonly actual: ContentKey
    readonly matched: boolean
    readonly at: DateTime.Utc
  }
}

const _capture = (
  view: string,
  readback: Probe.Readback,
  mint: (octets: Uint8Array) => Effect.Effect<ContentKey>,
  receipt: RenderReceipt,
): Effect.Effect<Probe.Verdict> =>
  Effect.gen(function* () {
    const pixels = yield* readback(_CAPTURE.width, _CAPTURE.height)
    const actual = yield* mint(pixels)
    const at = yield* DateTime.now
    return {
      view,
      expected: receipt.key,
      actual,
      matched: Equal.equals(actual, receipt.key),
      at,
    }
  }).pipe(
    Effect.withSpan("rasm.ui.probe.capture", { attributes: { "probe.view": view } }),
    Effect.annotateLogs({ view }),
  )
```

## [06]-[EVIDENCE_ROWS]

[EVIDENCE_ROWS]:
- Owner: `Probe.tone` — the verdict and delta presentation vocabulary: matched renders on the success tone, mismatched on the danger tone WITH both keys shown (the `:x32` spelling the kernel brand carries); delta rows tone by sign; the wire receipt's own fields render beside the local verdict; stamps format through `Format.instant`.
- Law: a mismatch is never a fault — no channel carries it, no retry fires; the verdict row IS the deliverable, and escalation is an operator decision outside this plane. Layout drift reports from `panel#LAYOUT_SOLVE` land on this same board as evidence rows under the same law.
- Law: verdict history is a bounded fold — the last N verdicts per view ride a `Chunk`-backed atom (append, take-right, the `[2]` window policy shape); evidence accumulates without unbounded memory.
- Law: evidence copies through the port — the copy-evidence affordance writes `Probe.line(row)` (one serializer over board row and verdict) through the `Clipboard` Tag (`system/primitive#CLIPBOARD_PORT`); `navigator.clipboard` in an evidence row is the named defect.
- Boundary: badge and row primitives are `system/primitive` recipes; the claim board renders `view/table` rows while metric timelines render `view/chart` series — evidence picks its surface by shape.

```typescript
import { DateTime, Option, Predicate } from "effect"

const _tone = {
  matched: { tone: "success" },
  mismatched: { tone: "danger" },
  faster: { tone: "success" },
  slower: { tone: "danger" },
  incomparable: { tone: "neutral" },
} as const

const _line = (row: Probe.BoardRow | Probe.Verdict): string =>
  Predicate.hasProperty(row, "matched")
    ? `${row.view} expected=${row.expected} actual=${row.actual} matched=${row.matched} at=${DateTime.formatIso(row.at)}`
    : [
        row.label,
        Option.match(row.claimed, { onNone: () => "claim=-", onSome: (held) => `claim=${held.value}${held.unit}` }),
        Option.match(row.local, { onNone: () => "local=-", onSome: (held) => `local=${held.value}${held.unit}` }),
        Option.match(row.delta, { onNone: () => "delta=-", onSome: (delta) => `delta=${delta}` }),
      ].join(" ")

declare namespace Probe {
  type Shape = {
    readonly window: typeof _WINDOW
    readonly extent: typeof _CAPTURE
    readonly observe: typeof _observe
    readonly rows: typeof _rows
    readonly aligned: typeof _aligned
    readonly host: typeof _host
    readonly board: typeof _board
    readonly capture: typeof _capture
    readonly tone: typeof _tone
    readonly line: typeof _line
  }
}

const Probe: Probe.Shape = {
  window: _WINDOW,
  extent: _CAPTURE,
  observe: _observe,
  rows: _rows,
  aligned: _aligned,
  host: _host,
  board: _board,
  capture: _capture,
  tone: _tone,
  line: _line,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Probe }
```
