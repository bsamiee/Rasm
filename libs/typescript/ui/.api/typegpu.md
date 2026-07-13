# [TS_UI_API_TYPEGPU]

[PACKAGE_SURFACE]:
- package: `typegpu` · license `MIT`
- module: `type: module`, `sideEffects: false`; exports `.` (the `tgpu` root), `./data` (the `d` schema namespace), `./std` (WGSL builtins as TS), `./common` — no deeper subpath is public.
- asset: deps `typed-binary` (schema serialization), `tinyest` (embedded-AST for TS→WGSL), `tsover-runtime`; a `typegpu` bin ships alongside.
- build: `unplugin-typegpu` (peer `typegpu`) is the transform that lets kernel bodies be plain TS — it recognizes `'use gpu'`-directive functions and `tgpu.fn` bodies and rewrites them for WGSL generation; bundler subpaths `./vite` `./rollup` `./webpack` `./esbuild` `./rspack` `./rolldown` `./bun` `./babel`.
- types: the shipped `.d.ts` references `GPUDevice`/`GPUBuffer`/… as BARE AMBIENT globals — nothing resolves without `@webgpu/types` (`.api/webgpu-types.md`) in the consuming tsconfig `types` array; `scope:viewer` project-local, like every WebGPU surface.
- plane: `plane:runtime` (W4 `ui`, `scope:viewer`); rail: standalone GPGPU compute.

`typegpu` is the typed GPGPU owner for data-parallel kernels that live OUTSIDE a three scene — geometry post-processing, simulation steps, analytic folds over large buffers. Its design is one root object (`TgpuRoot`) owning the `GPUDevice` and every resource, a `d.*` schema vocabulary that types buffers end-to-end (the `msgspec`-for-VRAM move: the schema IS the layout, `d.Infer<T>` IS the TS type), and kernels authored as TS functions the build transform + `tgpu.resolve` lower to WGSL — no hand-written shader strings, no untyped bind indices. It carries NO recovery: WebGPU-absent environments never construct a root, so admission is capability-gated behind the same `navigator.gpu` probe the viewer already runs, with the CPU/worker path as the degrade arm. Compute that lives INSIDE a three scene stays on `three/tsl` — two altitudes of one concern, split by scene residency, never two engines on one kernel.

## [01]-[ROOT_AND_RESOURCES]

```ts signature
declare const tgpu: {
  init(options?: { adapter?: GPURequestAdapterOptions; device?: GPUDeviceDescriptor }): Promise<TgpuRoot>
  initFromDevice(options: { device: GPUDevice }): TgpuRoot            // sync — adopt a device another owner (three) already holds
}
interface TgpuRoot {
  readonly device: GPUDevice; readonly enabledFeatures: ReadonlySet<GPUFeatureName>
  createBuffer<T>(schema: T, initialOrGpuBuffer?: d.Infer<T> | GPUBuffer): TgpuBuffer<T>   // .$usage('uniform'|'storage'|'vertex'|'index'|'indirect') chains capability
  createUniform<T>(schema: T, initial?: d.Infer<T>): TgpuUniform<T>   // shorthand owners: uniform / mutable(read_write storage) / readonly(storage)
  createMutable<T>(schema: T, initial?: d.Infer<T>): TgpuMutable<T>
  createReadonly<T>(schema: T, initial?: d.Infer<T>): TgpuReadonly<T>
  createTexture(desc: unknown): unknown; createSampler(desc: unknown): unknown; createQuerySet(type: GPUQueryType, count: number): unknown
  createBindGroup(layout: TgpuBindGroupLayout, entries: Record<string, unknown>): TgpuBindGroup
  createComputePipeline(descriptor: unknown): TgpuComputePipeline     // STABLE on root; '~unstable'.withCompute is the deprecated legacy path
  unwrap(resource: unknown): GPUBuffer | GPUBindGroup | GPUComputePipeline | GPURenderPipeline | GPUTextureView | GPUSampler | GPUQuerySet
  destroy(): void                                                     // the teardown bracket — releases every root-owned resource
  '~unstable': { beginRenderPass; beginRenderBundleEncoder; createGuardedComputePipeline; flush; pipe; with; withCompute; withVertex }
}
// Buffer verbs: write(value) / read(): Promise<d.Infer<T>> / copyFrom(other) / clear() / partial patch / destroy(); $addFlags for raw usage bits.
```

Render-pipeline authoring (`withVertex`/`beginRenderPass`) is `'~unstable'` and out of scope here — rendering belongs to three/deck.gl; this catalog admits the compute lane.

## [02]-[SCHEMA_PLANE]

`typegpu/data` (`import * as d from 'typegpu/data'`) is the one layout vocabulary — WGSL memory rules (alignment, padding, host-shareability) enforced at the type level.

| [INDEX] | [FAMILY]   | [MEMBERS]                                                                       | [ROLE]                                 |
| :-----: | :--------- | :------------------------------------------------------------------------------ | :------------------------------------- |
|  [01]   | scalar     | `d.f32` `d.f16` `d.u32` `d.i32` `d.bool`                                        | element types                          |
|  [02]   | vector     | `d.vec2f`/`3f`/`4f` · `vec2u`/`3u`/`4u` · `vec2i`/`3i`/`4i` · `vec2h`/`3h`/`4h` | callable constructors AND schema nodes |
|  [03]   | matrix     | `d.mat2x2f` `d.mat3x3f` `d.mat4x4f`                                             | column-major WGSL layout               |
|  [04]   | composite  | `d.struct({...})` `d.arrayOf(type, n)` `d.atomic(d.u32 \| d.i32)`               | buffer shapes; ordered struct fields   |
|  [05]   | loose      | `d.disarrayOf` `d.unstruct`                                                     | packed non-host-shareable layouts      |
|  [06]   | attributes | `d.size(n, T)` `d.align(n, T)` `d.location(n, T)` `d.builtin` `d.interpolate`   | explicit layout/IO decoration          |
|  [07]   | inference  | `d.Infer<T>`                                                                    | schema → TS value type; typing seam    |

## [03]-[KERNELS_AND_RESOLUTION]

- `tgpu.fn(argSchemas, returnSchema?)(impl)` — a typed shader function shell; with the build transform active the body is plain TS (`tinyest`-parsed), callable from JS AND resolvable to WGSL.
- `'use gpu'` directive — marks a plain function for JS→WGSL transpilation (the only directive; there is no `'kernel'`); such a function runs on CPU for tests and on GPU when resolved.
- `tgpu.computeFn({ workgroupSize: number[], in?: IORecord })(impl)` — the compute entry point; `tgpu.vertexFn`/`tgpu.fragmentFn` exist for the out-of-scope render lane.
- `tgpu.resolve(items[])` / `tgpu.resolve({ externals, template })` → WGSL string; `tgpu.resolveWithContext(...)` → `{ code, usedBindGroupLayouts, catchall }` for hand-assembled pipelines.
- Composition vocabulary: `tgpu.slot` (resolution-time injection point), `tgpu.lazy` (memoized derivation — `derived` is its DEPRECATED alias, never author it), `tgpu.accessor`/`mutableAccessor`, `tgpu.const`, `tgpu.privateVar`/`tgpu.workgroupVar` (address-space variables), `tgpu.comptime`, `tgpu.unroll`.
- `tgpu.bindGroupLayout({...})` — named, schema-typed bindings replacing numeric index bookkeeping; `tgpu.vertexLayout` types the vertex-buffer side.
- `typegpu/std` mirrors WGSL builtins as typed TS (dual CPU/GPU semantics): arithmetic (`add`/`mul`/`dot`/`cross`/`normalize`/`length`/`distance`), trig, `clamp`/`mix`/`step`/`smoothstep`/`min`/`max`/`floor`/`pow`/`exp`/`log`, the full atomic family (`atomicAdd`/`atomicLoad`/`atomicStore`/`atomicMax`/…), barriers (`workgroupBarrier`/`storageBarrier`/`textureBarrier`), texture ops (`textureSample`/`textureLoad`/`textureStore`/`textureDimensions`), `select`/`discard`.

## [04]-[INTEGRATION]

[STACK: `@webgpu/types` (`.api/webgpu-types.md`)] — typegpu declares no dependency on it yet types against the ambient `GPU*` graph; the viewer tsconfig `types` entry that already serves `three/webgpu` is the same admission this package rides. Both stay `scope:viewer` project-local.

[BOUNDARY: `three/tsl` compute (`.api/three.md`)] — scene residency picks the altitude. Kernels whose inputs/outputs live in a three scene (instance transforms, particle state feeding materials) are TSL: `Fn(...)().compute(count)` dispatched via `renderer.compute`/`computeAsync`, with the verified TSL vocabulary (`instancedArray`, `instanceIndex`, `storage`, `textureStore`, `uniform`, `uniformArray`, `deltaTime`, `time`, `Loop`, `If`, `workgroupBarrier`, `workgroupArray`, `subgroupAdd`, `atomicAdd`). Standalone data-parallel kernels with no scene consumer are typegpu. Where the two must share a device, `tgpu.initFromDevice({ device })` adopts the renderer's device and `root.unwrap(buffer)` exposes the raw `GPUBuffer` at the seam — one device, one memory space, zero readback round-trips.

[STACK: the Effect rail + atom store (`.api/effect-atom-atom-react.md`)] — the root is a scoped resource: acquisition (`tgpu.init()`) and release (`root.destroy()`) bracket in a `Scope`-owned boundary adapter, `buffer.read()`'s promise crosses into the rail via the one async seam, and kernel results land in atoms — a component never touches a `GPUBuffer`. Capability probing (`navigator.gpu` absent, feature missing from `root.enabledFeatures`) is a typed refusal at the adapter, not a throw in a component.

[BOUNDARY: the GPU-viz siblings] — deck.gl (`.api/deck.gl-core.md`) and three own RENDER; typegpu owns compute-without-a-scene. A typegpu kernel may prepare buffers a deck.gl layer or three geometry consumes, but it never grows a render pass of its own (`withVertex`/`beginRenderPass` stay unadmitted).

## [05]-[RAIL_LAW]

- Owns: standalone typed WebGPU compute — root/device lifecycle, schema-typed buffer/uniform/mutable/readonly resources, `d.*` layout vocabulary, TS-authored kernels (`tgpu.fn`/`computeFn`/`'use gpu'`), WGSL resolution (`resolve`/`resolveWithContext`), named bind-group layouts, the slot/lazy/accessor composition plane, and `typegpu/std` builtins.
- Accept: `d.struct`/`d.arrayOf` schemas with `d.Infer` as the one typing seam; plain-TS kernel bodies under `unplugin-typegpu`; `root.createComputePipeline` for dispatch; `initFromDevice` + `unwrap` at the three seam; Scope-bracketed root lifecycle behind a boundary adapter; capability-gated admission with a CPU/worker degrade arm; exact pinning with export re-verification (pre-1.0 surface moves).
- Reject: hand-written WGSL strings where `tgpu.fn`/`resolve` own generation; numeric bind-index bookkeeping beside `bindGroupLayout`; the deprecated `tgpu['~unstable'].withCompute`/`derived` spellings; scene-resident compute authored here instead of TSL; any render-pass authoring; a second device/root beside an adoptable one; ungated construction where `navigator.gpu` may be absent.
