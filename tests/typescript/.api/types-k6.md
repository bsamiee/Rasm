# [@types/k6] — typed k6 load-script authoring; the k6 binary is a runner fact, never a JS dependency

[PACKAGE_SURFACE]:
- package: `@types/k6` · version `2.0.0` · license `MIT` (DefinitelyTyped)
- module: type-only — ships ZERO `.js` (0 runtime files); `index.d.ts` + `global.d.ts` + ambient `declare module 'k6/*'` submodule declarations only.
- asset: ambient module declarations for `k6`, `k6/http`, `k6/metrics`, `k6/execution`, `k6/options`, `k6/ws`, `k6/websockets`, `k6/net/grpc`, `k6/html`, `k6/crypto`, `k6/encoding`, `k6/data`, `k6/timers`, `k6/browser`, `k6/secrets`, `k6/experimental/*`; `global.d.ts` adds `__ENV`/`__VU`/`__ITER`/`open`.
- runtime: NONE in node. The k6 binary is a Go program embedding the `goja`/`sobek` JS runtime; it — not node — executes the load script. Node never imports `k6`; this package types the SCRIPT the binary runs.
- plane: `plane:dev` — the `tests/typescript/e2e` load-driver, beside `@playwright/test`. It cannot leak into a bundle (there is nothing to import at runtime), so the `tests/typescript/_architecture` purity audit holds by construction.
- rail: load-script authoring types / e2e-gauge input contract.

`@types/k6` is the authoring-type surface for the k6 half of the `tests/typescript/e2e` home. It contributes no runtime and composes into NO Effect rail — a k6 script runs in the k6 binary's own JS runtime, outside node and outside the Effect world entirely. What this package buys is a fully typed load script: a typed `export default` VU body, typed `k6/http` calls, typed custom `Metric`s, and — the payload that matters — a typed `Options` object whose `scenarios`, `thresholds`, and executor shapes are the load profile AS DATA. The `Scenario.executor` field is a seven-arm discriminated union: one `scenarios` map owns every load shape (constant/ramping VUs, arrival-rate, per-VU iterations), discriminated by an `executor` string — a new load pattern is a row, never a new mechanism. The gauge does not import k6; it SPAWNS the binary (`@effect/platform` `Command`) against a typed script and parses the threshold verdict from the exported summary. This is the load twin of `@playwright/test`'s browser driver: both are subprocess gauges, neither is a runtime import.

## [01]-[LIFECYCLE_AND_GLOBALS]

[PUBLIC_TYPE_SCOPE]: the k6 execution lifecycle — the author writes `export default` (the per-iteration VU body) plus optional `setup`/`teardown`; k6 calls them. The root `k6` module supplies the in-body verbs; `global.d.ts` supplies the ambient VU/env globals.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                                 |
| :-----: | :---------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `export default function`     | author-typed   | the VU iteration body; k6 runs it `vus × iterations` times   |
|  [02]   | `check<VT>(val, sets, tags?)` | function       | boolean assertions on a value; records the check-rate metric |
|  [03]   | `group<RT>(name, fn)`         | function       | label a block of requests as one named group in the summary  |
|  [04]   | `sleep(t)` / `fail(err?)`     | function       | pace a VU (seconds) / abort the iteration with a message     |
|  [05]   | `randomSeed(int)`             | function       | deterministic per-VU randomness for a reproducible load      |
|  [06]   | `__ENV` / `__VU` / `__ITER`   | ambient global | env map, 1-based VU id, 0-based iteration counter            |
|  [07]   | `open(path, mode?)`           | ambient global | read a fixture file at init time (corpus / payload seed)     |

```ts signature
// index.d.ts + global.d.ts — Checkers is a name→predicate record; a check is one row, discriminated by description.
export function check<VT>(val: VT, sets: Checkers<VT>, tags?: object): boolean
export function group<RT>(name: string, fn: () => RT): RT
export function sleep(t: number): void
export function fail(err?: string): never
interface Checkers<VT> { [description: string]: (val: VT) => boolean }   // { "status is 200": r => r.status === 200 }
declare var __ENV: { [name: string]: string }; declare var __VU: number; declare var __ITER: number
```

## [02]-[LOAD_PROFILE_AS_DATA]

`Options` is the load profile expressed as ONE declarative object — the config-as-data twin of the Stryker `MutationScoreThresholds` (`stryker-mutator-vitest-runner.md` [03]). `thresholds` is the pass/fail gate (a metric name → assertion list); `scenarios` is the executor map. `Scenario.executor` collapses every load shape into one discriminated union — there is no `RampingVusScenario` class, only a `ramping-vus` row.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                                                             |
| :-----: | :---------------- | :------------------ | :----------------------------------------------------------------------- |
|  [01]   | `Options`         | interface           | the whole run config: `vus`/`duration`/`stages`/`scenarios`/`thresholds` |
|  [02]   | `Threshold`       | union               | `string \| ObjectThreshold` — a metric pass rule (e.g. `p(95)<200`)      |
|  [03]   | `ObjectThreshold` | interface           | `{ threshold; abortOnFail?; delayAbortEval? }` — fail-fast gate          |
|  [04]   | `Scenario`        | union (7 executors) | `executor` discriminates the load shape; the whole roster is one type    |
|  [05]   | `Stage`           | interface           | `{ duration; target }` — a ramping segment for `stages`/`ramping-*`      |

```ts signature
// options/index.d.ts — ONE Options object; scenarios is executor-discriminated; thresholds is the gate.
interface Options {
  vus?: number; duration?: string; iterations?: number
  stages?: Stage[]                                         // ramping VU segments
  scenarios?: { [name: string]: Scenario }                // named load shapes, executor-discriminated
  thresholds?: { [name: string]: Threshold[] }            // metric-name → pass rules; the e2e gate
  tags?: { [name: string]: string }; summaryTrendStats?: string[]; systemTags?: string[]
  rps?: number; discardResponseBodies?: boolean; noConnectionReuse?: boolean
  dns?: { … }; hosts?: { [h: string]: string }; tlsAuth?: Certificate[]; insecureSkipTLSVerify?: boolean
  ext?: { [name: string]: CollectorOptions }; throw?: boolean
}
type Threshold = string | ObjectThreshold
interface ObjectThreshold { threshold: string; abortOnFail?: boolean; delayAbortEval?: string }
// The executor collapse — seven rows, one union, discriminated by `executor`:
type Executor =
  | "shared-iterations" | "per-vu-iterations" | "constant-vus" | "ramping-vus"
  | "constant-arrival-rate" | "ramping-arrival-rate" | "externally-controlled"
```

## [03]-[HTTP_CLIENT]

[PUBLIC_TYPE_SCOPE]: `k6/http` — the request surface, RT-generic so the response body type narrows to `text` / `binary` / `none`. One `request<RT>` verb-generalizes; the named verbs are its rows; `batch` fires a typed request map; `asyncRequest` is the promise mirror.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `get`/`post`/`put`/`patch`/`del`/`head`/`options`/`request` | function      | RT-generic verbs; `request(method, …)` generalizes    |
|  [02]   | `asyncRequest<RT>`                                          | function      | `Promise<RefinedResponse<RT>>` concurrent mirror      |
|  [03]   | `batch<Q>(requests)`                                        | function      | parallel typed request map → `BatchResponses<Q>`      |
|  [04]   | `RefinedResponse<RT>` / `Response`                          | interface     | `status`/`body`/`headers`/`timings`/`json()`; RT body |
|  [05]   | `url` / `expectedStatuses`                                  | function      | URL tagged-template / status-set predicate            |

```ts signature
// http/index.d.ts — RT threads through: RefinedResponse<'text'>.body is string, <'binary'> is ArrayBuffer.
export function request<RT extends ResponseType | undefined>(method: string, url: string, body?: RequestBody, params?: RefinedParams<RT>): RefinedResponse<RT>
export function asyncRequest<RT extends ResponseType | undefined>(method: string, url: string, body?: RequestBody, params?: RefinedParams<RT>): Promise<RefinedResponse<RT>>
export function batch<Q extends BatchRequests>(requests: Q): BatchResponses<Q>
interface Response {
  status: number; status_text: string; body: ResponseBody; headers: { [name: string]: string }
  timings: { duration: number; waiting: number; connecting: number; sending: number; receiving: number }
  error: string; error_code: number
  json(selector?: string): JSONValue
}
type ResponseType = "binary" | "none" | "text"
```

## [04]-[METRICS_AND_EXECUTION]

Custom metrics feed thresholds BY NAME — a `new Trend("my_op_ms")` becomes the `thresholds["my_op_ms"]` key. `Metric` is the abstract base; `Counter`/`Gauge`/`Rate`/`Trend` are its four rows. `k6/execution` exposes the live VU/scenario/test state.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]  | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------ | :------------- | :---------------------------------------------------------------- |
|  [01]   | `Metric` → `Counter`/`Gauge`/`Rate`/`Trend` | class          | `new Trend(name, isTime?)`; `.add(value, tags?)`                  |
|  [02]   | `execution.vu` / `.scenario`                | live state     | `vu.idInTest`/`vu.iterationInScenario`/`scenario.iterationInTest` |
|  [03]   | `execution.instance` / `.test`              | live state     | instance VU count; `test.abort(reason?)` hard-stops the run       |
|  [04]   | `k6/data` `SharedArray`                     | class          | memory-shared read-only fixture corpus across VUs                 |
|  [05]   | `k6/ws`                                     | ambient module | legacy WebSocket protocol driver                                  |
|  [06]   | `k6/websockets`                             | ambient module | Promise-based WebSocket driver                                    |
|  [07]   | `k6/net/grpc`                               | ambient module | gRPC protocol driver                                              |
|  [08]   | `k6/browser`                                | ambient module | browser-automation driver                                         |

```ts signature
// metrics/index.d.ts + execution/index.d.ts — the metric name is the threshold key; execution.test.abort is the panic.
export abstract class Metric { constructor(name: string, isTime?: boolean); add(value: number | boolean, tags?: { [name: string]: string }): void }
export class Trend extends Metric {}  export class Counter extends Metric {}  export class Rate extends Metric {}  export class Gauge extends Metric {}
export const test: { abort(input?: string): void }
export const vu: { idInInstance: number; idInTest: number; iterationInInstance: number; iterationInScenario: number }
```

## [05]-[INTEGRATION]

[STACK: the e2e gauge SPAWNS the binary — it does not import k6] — this is the load twin of the container gauges, but the boundary is different: k6 runs OUTSIDE node, so there is no runtime import and no Effect Layer for the script. The `tests/typescript/e2e` suites drive the binary through the same subprocess rail the branch uses everywhere — `@effect/platform` `Command.make("k6", "run", "--summary-export=<f>", scriptPath).pipe(Command.env({ … }))` under `NodeContext.layer` (`effect-platform.md` / `effect-platform-node.md`, `host/exec/process.ts`), captures `.exitCode`/`.string`, and treats a non-zero exit (a breached `abortOnFail` threshold) as the gauge failure. `@types/k6` types the SCRIPT the binary runs; `@effect/platform` `Command` drives the BINARY; the two never share a runtime.

[STACK: the summary JSON is the receipt — `effect/Schema` validates the OUTPUT, never the script] — the only Effect-side composition is on the k6 OUTPUT: the `--summary-export` / `--out json` payload is decoded by an `effect/Schema` at the gauge boundary into a typed threshold-verdict receipt, folded into the gauge's pass/fail exactly like the Stryker JSON reporter output (`stryker-mutator-vitest-runner.md` [04]). Do NOT model the k6 script itself as an Effect program — it is authored against these ambient types and executed by goja; the Effect rail begins at the spawned process boundary and the parsed summary, not inside the load script.

[STACK: config-as-data parallel across the gauges] — `Options.thresholds` (`{ [metric]: Threshold[] }`) is the same shape-of-config the mutation gauge uses for `MutationScoreThresholds` and the container harness uses for image/wait rows: the gauge floors live as declarative data the driver reads, one type per concern, discriminated by row. `@playwright/test` is the sibling e2e driver for browser flows; k6 owns load — both are `tests/typescript/e2e` subprocess drivers, neither is a `plane:runtime` import.

## [06]-[RAIL_LAW]

- Owns: the authoring types for a k6 load script — the lifecycle verbs (`check`/`group`/`sleep`), the `Options` load profile (executor-discriminated `scenarios` + metric-keyed `thresholds`), the RT-generic `k6/http` client, custom `Metric`s, and the ambient VU/env globals.
- Accept: a typed `export default` VU body + typed `Options`; `thresholds` as the declarative e2e gate with `abortOnFail` for fail-fast; `scenarios` executor rows for load shape; custom `Trend`/`Rate` metrics feeding thresholds by name; `SharedArray` + `open` for a fixture corpus; `__ENV` for run parameterization from the spawning `Command.env`.
- Reject: `import`ing `k6` or any `k6/*` from a `plane:runtime` folder (there is no runtime — it resolves to nothing in node and is meaningless outside the binary); modeling the load script as an Effect program (it runs in goja, not the Effect runtime); asserting the k6 result inside node without going through the spawned-binary summary (the script's return value never crosses into node); hardcoding a target host instead of `__ENV`.
- Boundary: this package is `.d.ts` only — it types the script and contributes zero bytes to any bundle or runtime. The Effect world starts at the `@effect/platform` `Command` that spawns `k6` and ends at the `effect/Schema`-decoded summary; everything between runs in the k6 binary. A k6 script is not a vitest spec and not an Effect: it is an input artifact to a subprocess gauge.
