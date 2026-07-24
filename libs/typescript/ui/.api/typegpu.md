# [TS_UI_API_TYPEGPU]

`typegpu` owns typed standalone GPGPU compute — data-parallel kernels that live outside a three scene. One `TgpuRoot` owns the `GPUDevice` and every resource, the `d.*` schema types each buffer as its own WGSL layout with `d.Infer<T>` deriving the TS value, and kernels author as TS that the build transform lowers to WGSL. Scene-resident compute routes to `three/tsl`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `typegpu`
- package: `typegpu` (MIT)
- module: ESM, `sideEffects: false`; subpath exports `.` (the `tgpu` root), `./data` (the `d` schema namespace), `./std` (WGSL builtins as TS), `./common`
- runtime: browser WebGPU; the shipped `.d.ts` binds `GPUDevice`/`GPUBuffer`/`GPUFeatureName` as ambient globals that resolve only with `@webgpu/types` in the consumer tsconfig `types` array
- depends: `unplugin-typegpu` rewrites `'use gpu'` and `tgpu.fn` bodies to WGSL across every major bundler; `typed-binary` and `tinyest` carry buffer serialization and the embedded TS→WGSL AST
- plane: `plane:runtime` (W4 `ui`, `scope:viewer`)
- rail: standalone typed GPGPU compute

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the root, its typed resources, and the resolution-time composition plane

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :-------------------- | :------------ | :---------------------------------- |
|  [01]   | `TgpuRoot`            | interface     | owns the device and all resources   |
|  [02]   | `TgpuBuffer<T>`       | interface     | schema-typed buffer with read/write |
|  [03]   | `TgpuUniform<T>`      | interface     | uniform-usage buffer view           |
|  [04]   | `TgpuMutable<T>`      | interface     | read-write storage view             |
|  [05]   | `TgpuReadonly<T>`     | interface     | read-only storage view              |
|  [06]   | `TgpuComputePipeline` | interface     | dispatchable compute pipeline       |
|  [07]   | `TgpuBindGroupLayout` | interface     | named schema-typed bindings         |
|  [08]   | `TgpuBindGroup`       | interface     | layout bound to concrete resources  |
|  [09]   | `TgpuSlot<T>`         | interface     | resolution-time injection point     |
|  [10]   | `TgpuLazy<T>`         | interface     | memoized resolution-time derivation |
|  [11]   | `TgpuAccessor<T>`     | interface     | slot-backed in-kernel value         |

[SCHEMA_TYPE_SCOPE]: `typegpu/data` (`import * as d from 'typegpu/data'`) — the WGSL memory-layout vocabulary enforced at the type level

| [INDEX] | [FAMILY]  | [MEMBERS]                                                                     | [ROLE]                            |
| :-----: | :-------- | :---------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | scalar    | `d.f32` `d.f16` `d.u32` `d.i32` `d.bool`                                      | element types                     |
|  [02]   | vector    | `d.vec2f`/`3f`/`4f` `vec2u`/`3u`/`4u` `vec2i`/`3i`/`4i` `vec2h`/`3h`/`4h`     | constructor and schema node       |
|  [03]   | matrix    | `d.mat2x2f` `d.mat3x3f` `d.mat4x4f`                                           | column-major layout               |
|  [04]   | composite | `d.struct({...})` `d.arrayOf(T, n)` `d.atomic(d.u32\|d.i32)`                  | host-shareable buffer shapes      |
|  [05]   | loose     | `d.disarrayOf` `d.unstruct`                                                   | packed non-host-shareable layout  |
|  [06]   | attribute | `d.size(n, T)` `d.align(n, T)` `d.location(n, T)` `d.builtin` `d.interpolate` | explicit layout and IO decoration |
|  [07]   | inference | `d.Infer<T>`                                                                  | schema-to-TS typing seam          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: root construction and schema-typed resource factories

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------- |
|  [01]   | `tgpu.init(InitOptions?) -> Promise<TgpuRoot>`                      | factory  | request device and build root  |
|  [02]   | `tgpu.initFromDevice({device}) -> TgpuRoot`                         | factory  | adopt an existing `GPUDevice`  |
|  [03]   | `TgpuRoot.createBuffer(T, Infer<T>\|GPUBuffer?) -> TgpuBuffer<T>`   | instance | schema-typed buffer            |
|  [04]   | `TgpuRoot.createUniform(T, Infer<T>?) -> TgpuUniform<T>`            | instance | uniform resource               |
|  [05]   | `TgpuRoot.createMutable(T, Infer<T>?) -> TgpuMutable<T>`            | instance | read-write storage             |
|  [06]   | `TgpuRoot.createReadonly(T, Infer<T>?) -> TgpuReadonly<T>`          | instance | read-only storage              |
|  [07]   | `TgpuRoot.createComputePipeline(descriptor) -> TgpuComputePipeline` | instance | compute dispatch               |
|  [08]   | `TgpuRoot.createBindGroup(layout, entries) -> TgpuBindGroup`        | instance | bind resources to a layout     |
|  [09]   | `TgpuRoot.createTexture(props) -> TgpuTexture`                      | instance | storage or sampled texture     |
|  [10]   | `TgpuRoot.createSampler(props) -> TgpuFixedSampler`                 | instance | texture sampler                |
|  [11]   | `TgpuRoot.createQuerySet(type, count) -> TgpuQuerySet`              | instance | timestamp or occlusion queries |
|  [12]   | `TgpuRoot.unwrap(resource) -> GPU*`                                 | instance | expose the raw WebGPU handle   |
|  [13]   | `TgpuRoot.destroy()`                                                | instance | release device and resources   |
|  [14]   | `TgpuRoot.enabledFeatures: ReadonlySet<GPUFeatureName>`             | property | negotiated device features     |
|  [15]   | `TgpuBuffer.read() -> Promise<Infer<T>>`                            | instance | async readback to a TS value   |
|  [16]   | `TgpuBuffer.write(Infer<T>)`                                        | instance | upload a TS value              |

[ENTRYPOINT_SCOPE]: TS-authored kernels and their WGSL resolution

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `tgpu.fn(argSchemas, returnSchema?)(impl)`                               | factory | typed shader-function shell          |
|  [02]   | `tgpu.computeFn({workgroupSize, in?})(impl)`                             | factory | compute entry point                  |
|  [03]   | `tgpu.resolve(items[]) -> string`                                        | static  | lower resolvables to WGSL            |
|  [04]   | `tgpu.resolve({externals, template}) -> string`                          | static  | resolve a WGSL template              |
|  [05]   | `tgpu.resolveWithContext(...) -> {code, usedBindGroupLayouts, catchall}` | static  | resolve for hand-assembled pipelines |

- `'use gpu'`: marks a plain function for JS→WGSL transpilation — it runs on CPU under test and lowers to WGSL when resolved, and a `tgpu.fn` body transpiles the same way under `unplugin-typegpu`.

[ENTRYPOINT_SCOPE]: the resolution-time composition plane and binding declarations

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `tgpu.slot(default?) -> TgpuSlot<T>`                            | factory | resolution-time injection point  |
|  [02]   | `tgpu.lazy(() => T) -> TgpuLazy<T>`                             | factory | memoized derivation              |
|  [03]   | `tgpu.accessor(T, default?) -> TgpuAccessor<T>`                 | factory | slot-backed kernel value         |
|  [04]   | `tgpu.mutableAccessor(T, default?) -> TgpuMutableAccessor<T>`   | factory | mutable slot-backed value        |
|  [05]   | `tgpu.const(T, value) -> TgpuConst<T>`                          | factory | module-scope WGSL constant       |
|  [06]   | `tgpu.privateVar(T, init?) -> TgpuVar<'private', T>`            | factory | private address-space variable   |
|  [07]   | `tgpu.workgroupVar(T) -> TgpuVar<'workgroup', T>`               | factory | workgroup address-space variable |
|  [08]   | `tgpu.comptime(fn) -> TgpuComptime`                             | factory | resolution-time constant fold    |
|  [09]   | `tgpu.unroll(iterable) -> iterable`                             | factory | unroll a for-loop iterable       |
|  [10]   | `tgpu.bindGroupLayout({...}) -> TgpuBindGroupLayout`            | factory | named schema-typed bindings      |
|  [11]   | `tgpu.vertexLayout((n) => T, stepMode?) -> TgpuVertexLayout<T>` | factory | vertex-buffer layout             |

[ENTRYPOINT_SCOPE]: `typegpu/std` mirrors WGSL builtins as typed TS with dual CPU/GPU semantics

| [INDEX] | [FAMILY]   | [MEMBERS]                                                                                 | [ROLE]                  |
| :-----: | :--------- | :---------------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | arithmetic | `add` `sub` `mul` `div` `dot` `cross` `normalize` `length` `distance` `reflect` `refract` | vector and scalar math  |
|  [02]   | common     | `clamp` `mix` `step` `smoothstep` `min` `max`                                             | clamp and interpolation |
|  [03]   | math       | `abs` `sign` `floor` `ceil` `fract` `sqrt` `pow` `exp` `log`                              | scalar math             |
|  [04]   | trig       | `sin` `cos` `tan` `atan2`                                                                 | trigonometry            |
|  [05]   | atomic     | `atomicAdd` `atomicSub` `atomicLoad` `atomicStore` `atomicMax` `atomicMin`                | atomic memory ops       |
|  [06]   | barrier    | `workgroupBarrier` `storageBarrier` `textureBarrier`                                      | execution barriers      |
|  [07]   | texture    | `textureSample` `textureLoad` `textureStore` `textureDimensions`                          | texture access          |
|  [08]   | control    | `select` `discard`                                                                        | branch-free control     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every kernel resolves to WGSL through `tgpu.resolve`; the `d.*` schema is the sole source of buffer layout and `d.Infer<T>` the sole source of a buffer's TS type, so a hand-written shader string or a numeric bind index never appears.
- `TgpuRoot` owns the `GPUDevice` and every resource; a WebGPU-absent environment never constructs a root, so `navigator.gpu` presence gates admission and the CPU/worker path is the degrade arm.
- Scene residency picks the compute altitude: a kernel with a three-scene consumer resolves as TSL, a kernel with no scene consumer resolves as typegpu — one concern at two altitudes, never two engines on one kernel.

[STACKING]:
- `webgpu-types`(`.api/webgpu-types.md`): the `.d.ts` binds `GPUDevice`/`GPUBuffer`/`GPUFeatureName` as ambient globals `@webgpu/types` resolves, and `root.enabledFeatures` reads the same `GPUSupportedFeatures` set the viewer tsconfig `types` entry already admits for `three/webgpu`.
- `three`(`.api/three.md`): scene-resident compute is `three/tsl` — `Fn(...)().compute(count)` dispatched via `renderer.compute`/`computeAsync` over the TSL vocabulary (`instancedArray`, `instanceIndex`, `storage`, `textureStore`, `uniform`, `uniformArray`, `deltaTime`, `time`, `Loop`, `If`, `workgroupBarrier`, `workgroupArray`, `subgroupAdd`, `atomicAdd`); a shared device joins at `tgpu.initFromDevice({device})` and `root.unwrap(buffer)` exposes the raw `GPUBuffer`, one memory space with zero readback round-trips.
- `effect-atom-atom-react`(`.api/effect-atom-atom-react.md`): `tgpu.init()` and `root.destroy()` bracket a `Scope`-owned adapter, `buffer.read()`'s promise crosses the async seam, and kernel results land in atoms, so a component never touches a `GPUBuffer` and a missing feature is a typed refusal at the adapter.
- `deck.gl-core`(`.api/deck.gl-core.md`): deck.gl and three own render while typegpu owns compute-without-a-scene; a typegpu kernel prepares buffers a deck.gl layer or three geometry consumes, never growing a render pass.
- within-lib: the `ui` viewer folds root acquisition, capability probing, and kernel dispatch behind one boundary adapter, so a component consumes atom-delivered results rather than a raw device or buffer.

[LOCAL_ADMISSION]:
- `scope:viewer` project-local, like every WebGPU surface; the tsconfig `types` entry serving `three/webgpu` admits the ambient `GPU*` graph this package rides.
- Plain-TS kernel bodies require the `unplugin-typegpu` build transform recognizing `'use gpu'` and `tgpu.fn` bodies; absent the transform, generate WGSL through `tgpu.resolve`.
- Admission gates on `navigator.gpu` presence and `root.enabledFeatures` membership, with the CPU/worker path as the degrade arm.
- Render authoring — `createRenderPipeline` and the `root['~unstable']` render and render-bundle surface — stays out of scope; render belongs to three and deck.gl.

[RAIL_LAW]:
- Package: `typegpu`
- Owns: standalone typed WebGPU compute — root and device lifecycle, schema-typed buffer/uniform/mutable/readonly resources, the `d.*` layout vocabulary, TS-authored kernels, WGSL resolution, named bind-group layouts, the slot/lazy/accessor composition plane, and `typegpu/std` builtins.
- Accept: `d.struct`/`d.arrayOf` schemas with `d.Infer` as the one typing seam; plain-TS kernel bodies under `unplugin-typegpu`; `createComputePipeline` dispatch; `initFromDevice` plus `unwrap` at the three seam; a `Scope`-bracketed root behind a boundary adapter; capability-gated admission with a CPU/worker degrade arm.
- Reject: hand-written WGSL where `tgpu.fn`/`resolve` own generation; numeric bind-index bookkeeping beside `bindGroupLayout`; scene-resident compute authored here instead of TSL; render-pass authoring; a second device or root beside an adoptable one; ungated construction where `navigator.gpu` may be absent.
