# [UI_PROBE]

Probe owns benchmark and render evidence. It derives canonical pixel identity from a fixed capture and compares it with the render arm in `Wire.EvidenceTimeline`.

## [01]-[INDEX]

- [02]-[METRIC_FOLD]: the local capture — deck and renderer counters through one bounded algebra; `Probe`.
- [03]-[HOST_MIRROR]: the local host-fingerprint capture mirroring the wire's fields; `Probe`.
- [04]-[CLAIM_BOARD]: the claim-versus-local label-keyed join and its display rows; `Probe`.
- [05]-[CAPTURE_FOLD]: the deterministic framebuffer capture and the kernel hash delegate; `Probe`.
- [06]-[EVIDENCE_ROWS]: tone tables, bounded verdict history, the never-a-gate law; `Probe`.

## [02]-[METRIC_FOLD]

[METRIC_FOLD]:
- Owner: `Probe.rows` projects Deck and renderer samples through one metric table.
- Law: probes are passive, and the existing scene tick supplies each sample.
- Law: `Vital.window`, `Vital.fold`, and `Vital.project` own bounded aggregation.
- Law: `_METRICS` derives the accumulator keys, output rows, and aligned chart series.
- Boundary: scene supplies samples, and runtime telemetry consumes the resulting rows.
- Packages: `@deck.gl/core`; `three/webgpu`; `@rasm/ts/core` (`Board`); `system/vital`; `effect`.

```typescript
import type { DeckMetrics } from "@deck.gl/core"
import { Board } from "@rasm/ts/core"
import { Array, Chunk, Option, Record, pipe } from "effect"
import type { WebGPURenderer } from "three/webgpu"
import { Vital } from "../../src/system/vital.ts"

type Metric = Board.Claim.Metric

type _Info = WebGPURenderer["info"]

declare namespace Probe {
  type Sample = {
    readonly deck: DeckMetrics
    readonly info: {
      readonly render: Pick<_Info["render"], "frameCalls" | "drawCalls" | "triangles" | "points" | "lines">
      readonly compute: Pick<_Info["compute"], "frameCalls">
      readonly memory: Pick<_Info["memory"], "geometries" | "textures" | "texturesSize" | "attributes" | "attributesSize" | "programs" | "renderTargets" | "readbackBuffers" | "uniformBuffers" | "total">
    }
  }
  type Trace = Chunk.Chunk<Probe.Sample>
}

type _Measure = { readonly label: Metric["label"]; readonly unit: Metric["unit"]; readonly projection: Vital.Projection; readonly read: (sample: Probe.Sample) => number }

const _DECK_TIMERS = {
  fps: { label: "fps", unit: "1/s", projection: "mean", read: (s: Probe.Sample) => s.deck.fps },
  gpu: { label: "gpu-time", unit: "ms", projection: "mean", read: (s: Probe.Sample) => s.deck.gpuTime },
  gpuFrame: { label: "gpu-time-frame", unit: "ms", projection: "mean", read: (s: Probe.Sample) => s.deck.gpuTimePerFrame },
  cpu: { label: "cpu-time", unit: "ms", projection: "mean", read: (s: Probe.Sample) => s.deck.cpuTime },
  cpuFrame: { label: "cpu-time-frame", unit: "ms", projection: "mean", read: (s: Probe.Sample) => s.deck.cpuTimePerFrame },
  pick: { label: "pick-time", unit: "ms", projection: "mean", read: (s: Probe.Sample) => s.deck.pickTime },
  props: { label: "setprops-time", unit: "ms", projection: "mean", read: (s: Probe.Sample) => s.deck.setPropsTime },
  attrs: { label: "attr-update-time", unit: "ms", projection: "mean", read: (s: Probe.Sample) => s.deck.updateAttributesTime },
} as const satisfies Record<string, _Measure>

const _DECK_COUNTERS = {
  redrawn: { label: "frames-redrawn", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.deck.framesRedrawn },
  picks: { label: "pick-count", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.deck.pickCount },
  pickedLayers: { label: "pick-layers", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.deck.pickLayersCount },
  attrUpdates: { label: "attr-updates", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.deck.updateAttributesCount },
  layers: { label: "layers", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.deck.layersCount },
  drawnLayers: { label: "layers-drawn", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.deck.drawLayersCount },
  updatedLayers: { label: "layers-updated", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.deck.updateLayersCount },
} as const satisfies Record<string, _Measure>

const _DECK_MEMORY = {
  gpuMemory: { label: "gpu-memory", unit: "By", projection: "peak", read: (s: Probe.Sample) => s.deck.gpuMemory },
  bufferMemory: { label: "buffer-memory", unit: "By", projection: "peak", read: (s: Probe.Sample) => s.deck.bufferMemory },
  textureMemory: { label: "texture-memory", unit: "By", projection: "peak", read: (s: Probe.Sample) => s.deck.textureMemory },
  renderbufferMemory: { label: "renderbuffer-memory", unit: "By", projection: "peak", read: (s: Probe.Sample) => s.deck.renderbufferMemory },
} as const satisfies Record<string, _Measure>

const _RENDERER_INFO = {
  renderCalls: { label: "render-calls", unit: "1", projection: "mean", read: (s: Probe.Sample) => s.info.render.frameCalls },
  drawCalls: { label: "draw-calls", unit: "1", projection: "mean", read: (s: Probe.Sample) => s.info.render.drawCalls },
  triangles: { label: "triangles", unit: "1", projection: "mean", read: (s: Probe.Sample) => s.info.render.triangles },
  points: { label: "points", unit: "1", projection: "mean", read: (s: Probe.Sample) => s.info.render.points },
  lines: { label: "lines", unit: "1", projection: "mean", read: (s: Probe.Sample) => s.info.render.lines },
  computeCalls: { label: "compute-calls", unit: "1", projection: "mean", read: (s: Probe.Sample) => s.info.compute.frameCalls },
  geometries: { label: "geometries-resident", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.info.memory.geometries },
  textures: { label: "textures-resident", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.info.memory.textures },
  attributes: { label: "attributes-resident", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.info.memory.attributes },
  programs: { label: "programs-resident", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.info.memory.programs },
  renderTargets: { label: "render-targets", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.info.memory.renderTargets },
  readbackBuffers: { label: "readback-buffers", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.info.memory.readbackBuffers },
  uniformBuffers: { label: "uniform-buffers", unit: "1", projection: "latest", read: (s: Probe.Sample) => s.info.memory.uniformBuffers },
  textureBytes: { label: "renderer-texture-memory", unit: "By", projection: "peak", read: (s: Probe.Sample) => s.info.memory.texturesSize },
  attributeBytes: { label: "renderer-attribute-memory", unit: "By", projection: "peak", read: (s: Probe.Sample) => s.info.memory.attributesSize },
  totalBytes: { label: "renderer-memory", unit: "By", projection: "peak", read: (s: Probe.Sample) => s.info.memory.total },
} as const satisfies Record<string, _Measure>

const _METRICS = { ..._DECK_TIMERS, ..._DECK_COUNTERS, ..._DECK_MEMORY, ..._RENDERER_INFO } as const
type _MetricKey = keyof typeof _METRICS

// the floor owns the whole window algebra — window bound, accumulating pass, and finishers all arrive from
// `system/vital`, so this page declares only WHAT it measures and HOW to read one sample
const _observe = (trace: Probe.Trace, sample: Probe.Sample, policy: Vital.Policy): Probe.Trace =>
  Vital.window(trace, [sample], policy.samples) // the branded floor cap, never a page literal

const _rows = (trace: Probe.Trace): ReadonlyArray<Metric> =>
  pipe(Vital.fold(trace, (sample) => Record.map(_METRICS, (row) => row.read(sample))), (window) =>
    window.count === 0
      ? [] // an empty window carries no rows — a zero-sample mean is fabricated evidence
      : Array.getSomes(
          Record.collect(_METRICS, (key, row) =>
            // a measure the window never saw emits nothing rather than a zero no sample produced
            Option.map(Record.get(window.parts, key), (held) => ({
              label: row.label,
              value: Vital.project[row.projection](held, window.count),
              unit: row.unit,
            })))))

const _aligned = (
  trace: Probe.Trace,
  series: Array.NonEmptyReadonlyArray<_MetricKey>,
): readonly [Float64Array, ...ReadonlyArray<Float64Array>] =>
  // the caller names its series off the one measure key space; the leading column is the sample rank
  pipe(Chunk.toReadonlyArray(trace), (samples) => [
    Float64Array.from(samples, (_, rank) => rank),
    ...Array.map(series, (key) => Float64Array.from(samples, _METRICS[key].read)),
  ] as const)
```

## [03]-[HOST_MIRROR]

[HOST_MIRROR]:
- Owner: `Probe.host` mirrors `Board.Claim.Host` from app identity, browser facts, and the scene's adapter result.
- Packages: `@rasm/ts/core` (`Board`); `effect`; `@webgpu/types`.
- Law: `Board.Claim.matches` compares the producer host print with `Identity.App`; probe renders divergence and never gates.
- Law: decoded support exports display beside claims without re-deriving their evidence.
- Law: host capture runs once per session.

```typescript
import { Number, Option, pipe } from "effect"
import type { Theme } from "../../src/system/token.ts"

const _host = (
  print: string,
  adapter: Option.Option<{ readonly vendor: string; readonly architecture: string }>,
): Board.Claim.Host =>
  pipe(
    Option.getOrElse(adapter, () => ({ vendor: "<unavailable>", architecture: "<unavailable>" })),
    (info) =>
      // Browsers expose no operating-system name through a stable surface, so `os` takes the same
      // declared-unavailable sentinel the absent adapter facts take, and `stamps` stays empty here
      // because every host fact this probe reaches already fills a column of its own.
      new Board.Claim.Host({
        print,
        machine: info.vendor,
        os: "<unavailable>",
        arch: info.architecture,
        processors: Number.max(1, globalThis.navigator.hardwareConcurrency),
        runtime: globalThis.navigator.userAgent,
        stamps: {},
      }),
  )
```

## [04]-[CLAIM_BOARD]

[CLAIM_BOARD]:
- Owner: `Probe.board` performs a full label-keyed join of claim and local metrics.
- Law: units must agree before a row receives a numeric delta.
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

const _board = (claim: Board.Claim, local: ReadonlyArray<Metric>): ReadonlyArray<Probe.BoardRow> => {
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
- Owner: `Probe.capture` normalizes one controlled RGBA8 readback and compares its canonical pixel hash with timeline evidence.
- Law: the preimage is UTF-8 version, little-endian width and height, then tightly packed top-left RGBA8 sRGB straight-alpha bytes.
- Law: capture compares only `pixels.hash`; `frameHash` identifies encoded artifact bytes and `drawHash` identifies draw attribution.
- Law: `Probe.packed` publishes that normalization, so the hash preimage and `view/export#SERIALIZER_MATRIX`'s readback arm read one buffer and a second repack forks the pixel identity.
- Boundary: scene supplies async readback, `Digest.mint` owns hashing, and `Wire` owns timeline decoding.

```typescript
import { Digest, Wire } from "@rasm/ts/core"
import { Array, DateTime, Effect, Equal, Option } from "effect"

const _PIXEL_VERSION = "rgba8-srgb-straight-top-left-v1" as const
const _CAPTURE = { width: 1024, height: 1024, version: _PIXEL_VERSION } as const

type Evidence = Wire.EvidenceTimeline["rows"][number]["envelope"]["payload"]
type RenderEvidence = Extract<Evidence, { readonly kind: "render" }>

const _isRender = (evidence: Evidence): evidence is RenderEvidence => evidence.kind === "render"

declare namespace Probe {
  type Pixels = {
    readonly rgba: Uint8Array
    readonly rowStride: number
    readonly origin: "top-left" | "bottom-left"
  }
  type Readback = (width: number, height: number) => Effect.Effect<Pixels>
  type Verdict = {
    readonly view: string
    readonly expected: Digest.Key<"content">
    readonly actual: Digest.Key<"content">
    readonly matched: boolean
    readonly at: DateTime.Utc
  }
}

const _packed = (capture: Probe.Pixels, width: number, height: number): Uint8Array => {
  const rowBytes = width * 4
  const packed = new Uint8Array(rowBytes * height)
  for (let row = 0; row < height; row += 1) {
    const source = capture.origin === "top-left" ? row : height - row - 1
    packed.set(capture.rgba.subarray(source * capture.rowStride, source * capture.rowStride + rowBytes), row * rowBytes)
  }
  return packed
}

const _preimage = (capture: Probe.Pixels, width: number, height: number): Uint8Array => {
  const version = new TextEncoder().encode(_PIXEL_VERSION)
  const pixels = _packed(capture, width, height)
  const preimage = new Uint8Array(version.length + 8 + pixels.length)
  preimage.set(version)
  const dimensions = new DataView(preimage.buffer, version.length, 8)
  dimensions.setInt32(0, width, true)
  dimensions.setInt32(4, height, true)
  preimage.set(pixels, version.length + 8)
  return preimage
}

const _render = (timeline: Wire.EvidenceTimeline, view: string): Option.Option<RenderEvidence> =>
  Array.findFirst(Array.filterMap(timeline.rows, (row) => _isRender(row.envelope.payload)
    ? Option.some(row.envelope.payload)
    : Option.none()), (receipt) => receipt.slot === view && receipt.pixels !== undefined)

const _capture = (
  view: string,
  readback: Probe.Readback,
  timeline: Wire.EvidenceTimeline,
): Effect.Effect<Option.Option<Probe.Verdict>> =>
  Option.match(_render(timeline, view), {
    onNone: () => Effect.succeed(Option.none()),
    onSome: (receipt) => Effect.gen(function* () {
    const identity = receipt.pixels!
    const capture = yield* readback(identity.width, identity.height)
    const actual = yield* Digest.mint("content", _preimage(capture, identity.width, identity.height))
    const at = yield* DateTime.now
    return Option.some({
      view,
      expected: identity.hash,
      actual,
      matched: Equal.equals(actual, identity.hash),
      at,
    })
  }).pipe(
    Effect.withSpan("rasm.ui.probe.capture", { attributes: { "probe.view": view } }),
    Effect.annotateLogs({ view }),
  )})
```

## [06]-[EVIDENCE_ROWS]

[EVIDENCE_ROWS]:
- Owner: `Probe.tone` maps verdict and delta evidence to presentation rows.
- Law: mismatches remain evidence, and `Vital.Policy.samples` bounds history.
- Boundary: `system/primitive` owns rendering and clipboard output.

```typescript
import { DateTime, Option, Predicate } from "effect"

const _tone = {
  matched: { tone: "success" },
  mismatched: { tone: "danger" },
  faster: { tone: "success" },
  slower: { tone: "danger" },
  incomparable: { tone: "neutral" },
} as const satisfies Record<string, { readonly tone: Theme.Tone }>

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
    readonly extent: typeof _CAPTURE
    readonly observe: typeof _observe
    readonly rows: typeof _rows
    readonly aligned: typeof _aligned
    readonly host: typeof _host
    readonly board: typeof _board
    readonly capture: typeof _capture
    readonly packed: typeof _packed
    readonly tone: typeof _tone
    readonly line: typeof _line
  }
}

const Probe: Probe.Shape = {
  extent: _CAPTURE,
  observe: _observe,
  rows: _rows,
  aligned: _aligned,
  host: _host,
  board: _board,
  capture: _capture,
  packed: _packed,
  tone: _tone,
  line: _line,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Probe }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
