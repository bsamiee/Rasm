# [UI_GLB]

The 3D mesh render leaf over the `interchange` `ArtifactFrameRail` GLB byte-stream. `GlbViewport` consumes the content-addressed `ArtifactBlob` the rail reassembles, decodes the mesh-tensor view through the generated `GeometryPayload(mesh)` descriptor BY REFERENCE, uploads one GPU resource per content key, and disposes on scope exit. The renderer is one Three.js row whose `WebGPURenderer` auto-detects WebGPU and falls back to WebGL with zero config, plus the `model-viewer` declarative read-only embed row — given universal WebGPU, no separate Babylon or raw-WebGPU backend. The mesh-decode seam consumes the settled `GeometryPayload`/`MeshTensor` proto the C# `csharp:Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` mints (`MeshTensor` `vertex_count`/`vertices`/`face_count`/`faces`); the residency manifest the C# `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` `ResidencyManifest` mints rides as one CONSUME-only growth row keyed by content key. The viewport is one render leaf at the same altitude as the atom binding, holds no domain state, and reads camera state only through the `binding/atom.md` `AtomBinding`.

## [1]-[INDEX]

- [1]-[GLB_VIEWPORT]: the `GlbViewport` mesh render leaf, the `RendererBackend` two-row axis (`three`/`model-viewer`), the `decodeMeshView` `GeometryPayload` decode fold, the residency-manifest consume row, and the `ViewportCamera` gesture-driven camera fold.

## [2]-[GLB_VIEWPORT]

- Owner: `GlbViewport`, the mesh render leaf consuming the content-addressed GLB byte-stream the `interchange` `ArtifactFrameRail` reassembles into one `ArtifactBlob` — the same content-key blob the C# `csharp:Rasm.Compute/Runtime/channels#TS_PROJECTION` server-stream delivers and the Python IFC->GLB companion re-enters over the identical remote lane. `RendererBackend`, the `Schema.Literal` two-row backend axis; `ViewportResource`, the `Effect.acquireRelease`-managed GL context acquired over the ui-owned `ViewportHost` capability, never a free React ref; `GeometryView`, the `Data.TaggedEnum` family the `three` row's `draw` folds total under `GeometryView.$match` (the `mesh` arm `vertices` bytes as Float32 N×3 and `faces` as Uint32 F×3, the `pointCloud` arm `data` as Float32 `count × channels`, the `voxel` arm `dims·data`); `decodeMeshView`, the total decode fold over the `GeometryPayload` `kind` oneof discriminant projecting `mesh`/`pointCloud`/`voxel` arms onto sibling `GeometryView` cases. The viewport reads the blob through the rail, decodes the geometry view by `kind`, uploads one GPU resource per content key, and disposes on scope exit.
- Cases: `RendererBackend` dispatches the upload-and-draw total over two rows folded INTO the `acquireBackend` `Match` arms — `three` (the Three.js `WebGPURenderer` whose `init()` auto-detects WebGPU and falls back to WebGL with zero config, whose `draw` folds the `GeometryView` family onto the `BufferGeometry` `position`/`index` attributes, the `Points` cloud, and the instanced voxel) and `model-viewer` (the declarative `<model-viewer>` custom element whose `draw` sets the `src` to an object-URL `Blob` over the GLB bytes, the zero-GL-handle read-only row). The engine literal owns acquire AND draw in one arm so a backend swap is one literal value, never a parallel viewport. `decodeMeshView` folds the decoded `GeometryPayload` into one `GeometryView` keyed by the wire `kind` oneof `case` (`mesh`/`pointCloud`/`voxel`), each arm a sibling case on the one fold. `ViewportCamera` is one render-surface-local `RoleBehavior<CameraProps, CameraState>` row on the `interaction/role.md` `core` leaf whose pointer-gesture intents fold through the `interaction/gesture.md` `CameraGesture` `Data.TaggedEnum` under one `Match.tagsExhaustive` wired INTO the behavior so a `CameraGesture` tag drives `CameraState` through the binding, never an orphaned fold. The meshlet/cluster-LOD ambition rides Three.js TSL compute on the `three` row, never a hand-authored `GPUDevice` path.
- Entry: `GlbViewport.mount(blob, backend)` acquires the GL context as an `Effect.acquireRelease` scoped resource, decodes the mesh view through the generated `GeometryPayload` descriptor, uploads one GPU buffer set per `contentKey`, and registers the render loop as an `Effect.forkScoped` fiber torn down LIFO on scope exit; the `ArtifactBlob` arrives only through the `interchange` `ArtifactFrameRail`; the camera state reads and writes only through the `binding/atom.md` `AtomBinding`; the heavy mesh-tensor decode and the XxHash128 content-key verify already ran off the main thread under the `platform` `DecodeWorkerPool` at the rail seam, so the viewport receives a verified blob and never re-hashes. The `FaultDetail` fault family is the `interchange` `fault-family#FAULT_FAMILY` owner imported by reference. A backgrounded viewport stays mounted-but-hidden through the React `<Activity>` surface (the `interaction/transition.md` owner), preserving the GL context and uploaded buffers across visibility toggles instead of teardown.
- Packages: `three`, `@google/model-viewer`, `@webgpu/types`, `@bufbuild/protobuf`, `react-aria`, `react-stately`, `effect`. The `GeometryPayload` descriptor decodes by reference off the `@bufbuild/protobuf` generated `remote_lane_pb` the `interchange/Codec/codec#DECODE_RAIL` `proto` row admits — no branch-side wire struct; the upload/draw/camera owners fold the decoded `MeshView`.
- Growth: a new engine lands as one `RendererBackend` literal carrying its acquire arm and draw arm; a new geometry payload kind (the `GeometryPayload` `point_cloud`/`voxel` oneof arms) lands as one `MeshView` sibling case on the same decode fold keyed by the wire `kind` discriminant, never a second viewport surface; a new camera intent lands as one `CameraGesture` tag; a new render mode (wireframe, normals, LOD) lands as one `MeshDraw` parameter row. The WebGPU splat/meshlet residency path (the cross-`libs/` `WEB_GEOMETRY_RESIDENCY_WIRE`) lands as one CONSUME-only manifest row keyed by content key — the residency manifest is minted ONCE by `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` and decoded through the `typescript:worker/` pool; this leaf reads the settled manifest by reference and never re-mints the manifest or its hash (the single-mint invariant is enforced at the cross-libs master, not here).
- Boundary: the viewport reads ONLY the generated `GeometryPayload` descriptor (consumed by reference through the `decode-rail#DECODE_RAIL` `proto` row) and the `ArtifactFrameRail` `ArtifactBlob` — re-authoring a branch-side `MeshTensorWire`/`PointCloudTensorWire`/`VoxelTensorWire`, reaching a C# geometry interior, or a host projection is the named defect; a second decode of the GLB bytes beside the `ArtifactFrameRail` is the named defect; the residency manifest is the single C# mint of the cross-`libs/` `WEB_GEOMETRY_RESIDENCY_WIRE` at `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` `ResidencyManifest.Mint`, decoded through the `typescript:platform/Transport/decode#DECODE_POOL` pool and consumed here by content key — a TS-side re-mint of the manifest, the `ContentKey`, or the `:x32` blob-key spelling is the named cross-language drift defect graded at the cross-`libs/` master; the canvas and the worker-pool geometry decode arrive only as the ui-owned `ViewportHost` capability the SPA composition root satisfies with the `platform` `BrowserPlatform`/`DecodeWorkerPool` layer — `ui` declares the requirement and never imports `platform`, so a value import of a `platform` symbol into this leaf is the named one-way-direction defect; a GL context held as a free React ref instead of an `Effect.acquireRelease` resource is the named defect — the context, the buffers, and the render-loop fiber all release LIFO on scope exit; the camera is one `RoleBehavior` row and a parallel viewport-gesture handler type is the named defect; `model-viewer` is the zero-GL-handle read-only embed row; the WebGL fallback is the Three.js `WebGPURenderer` auto-detect, never a hand-rolled `navigator.gpu` degrade branch beside it.

```ts contract
import type { ArtifactBlob } from "@rasm/ts/interchange";
import type { GeometryPayload, GeometryResidencyWire, ResidencyTileWire } from "@rasm/ts/gen/remote_lane_pb";
import type { RoleBehavior } from "@rasm/ts/ui/interaction/role";
import type { CameraGesture, CameraState } from "@rasm/ts/ui/interaction/gesture";
import { FaultDetail } from "@rasm/ts/interchange";
import * as THREE from "three/webgpu";
import { Context, Data, Effect, Match, Option, Schema, Scope } from "effect";

const RendererBackend = Schema.Literal("three", "model-viewer");
type RendererBackend = Schema.Schema.Type<typeof RendererBackend>;

type GeometryView = Data.TaggedEnum<{
  readonly mesh: { readonly vertices: Float32Array; readonly indices: Uint32Array; readonly vertexCount: number; readonly faceCount: number };
  readonly pointCloud: { readonly positions: Float32Array; readonly count: number; readonly channels: number };
  readonly voxel: { readonly dims: readonly [number, number, number]; readonly data: Uint8Array };
}>;
const GeometryView = Data.taggedEnum<GeometryView>();
interface MeshView { readonly vertices: Float32Array; readonly indices: Uint32Array; readonly vertexCount: number; readonly faceCount: number }

const f32View = (b: Uint8Array, scalars: number): Float32Array =>
  b.byteOffset % Float32Array.BYTES_PER_ELEMENT === 0
    ? new Float32Array(b.buffer, b.byteOffset, scalars)
    : new Float32Array(b.slice(0, scalars * Float32Array.BYTES_PER_ELEMENT).buffer);
const u32View = (b: Uint8Array, scalars: number): Uint32Array =>
  b.byteOffset % Uint32Array.BYTES_PER_ELEMENT === 0
    ? new Uint32Array(b.buffer, b.byteOffset, scalars)
    : new Uint32Array(b.slice(0, scalars * Uint32Array.BYTES_PER_ELEMENT).buffer);

const decodeMeshView = (payload: GeometryPayload): Effect.Effect<GeometryView, FaultDetail> =>
  Match.value(payload.kind).pipe(
    Match.when({ case: "mesh" }, ({ value: m }) =>
      Effect.succeed(GeometryView.mesh({
        vertices: f32View(m.vertices, Number(m.vertexCount) * 3),
        indices: u32View(m.faces, Number(m.faceCount) * 3),
        vertexCount: Number(m.vertexCount),
        faceCount: Number(m.faceCount),
      }))),
    Match.when({ case: "pointCloud" }, ({ value: p }) =>
      Effect.succeed(GeometryView.pointCloud({
        positions: f32View(p.data, Number(p.count) * p.channels),
        count: Number(p.count),
        channels: p.channels,
      }))),
    Match.when({ case: "voxel" }, ({ value: v }) =>
      v.dims.length === 3
        ? Effect.succeed(GeometryView.voxel({ dims: [Number(v.dims[0]), Number(v.dims[1]), Number(v.dims[2])], data: v.data }))
        : Effect.fail(FaultDetail.HopFault({ code: "voxel-rank-unsupported", evidence: { rank: String(v.dims.length) } }))),
    Match.orElse(() =>
      Effect.fail(FaultDetail.HopFault({ code: "geometry-kind-unsupported", evidence: { kind: payload.kind.case ?? "none" } }))),
  );

interface ViewportResource {
  readonly canvas: HTMLCanvasElement;
  readonly draw: (view: GeometryView) => Effect.Effect<void>;
  readonly resize: (width: number, height: number) => Effect.Effect<void>;
}

class ViewportHost extends Context.Tag("ui/ViewportHost")<ViewportHost, {
  readonly canvas: Effect.Effect<HTMLCanvasElement, never, Scope.Scope>;
  readonly geometryOf: (blob: ArtifactBlob) => Effect.Effect<GeometryPayload, FaultDetail>;
  readonly tileBytes: (tile: ResidencyTileWire) => Effect.Effect<ArtifactBlob, FaultDetail>;
}>() {}

const residentTiles = (manifest: GeometryResidencyWire): Effect.Effect<ReadonlyArray<GeometryView>, FaultDetail, ViewportHost> =>
  Effect.flatMap(ViewportHost, (host) =>
    Effect.forEach(manifest.tiles, (tile) => host.tileBytes(tile).pipe(Effect.flatMap(host.geometryOf), Effect.flatMap(decodeMeshView))));

interface GlbViewport {
  readonly mount: (
    blob: ArtifactBlob,
    backend: RendererBackend,
  ) => Effect.Effect<ViewportResource, FaultDetail, Scope.Scope | ViewportHost>;
}

const acquireBackend = (
  canvas: HTMLCanvasElement,
  blob: ArtifactBlob,
  backend: RendererBackend,
): Effect.Effect<ViewportResource, FaultDetail, Scope.Scope> =>
  Match.value(backend).pipe(
    Match.when("three", () =>
      Effect.acquireRelease(
        Effect.promise(() => {
          const renderer = new THREE.WebGPURenderer({ canvas, antialias: true });
          return renderer.init().then(() => renderer);
        }),
        (renderer) => Effect.sync(() => renderer.dispose()),
      ).pipe(Effect.map((renderer) => {
        const scene = new THREE.Scene();
        const geometry = new THREE.BufferGeometry();
        const camera = new THREE.PerspectiveCamera(50, 1, 0.01, 1000);
        scene.add(new THREE.Mesh(geometry, new THREE.MeshStandardMaterial()));
        scene.add(new THREE.Points(geometry, new THREE.PointsMaterial({ size: 1 })));
        return {
          canvas,
          draw: (view) => Effect.sync(() => GeometryView.$match(view, {
            mesh: (m) => {
              geometry.setAttribute("position", new THREE.BufferAttribute(m.vertices, 3));
              geometry.setIndex(new THREE.BufferAttribute(m.indices, 1));
              geometry.computeVertexNormals();
              renderer.render(scene, camera);
            },
            pointCloud: (p) => {
              geometry.deleteAttribute("normal");
              geometry.setIndex(null);
              geometry.setAttribute("position", new THREE.BufferAttribute(p.positions, p.channels));
              renderer.render(scene, camera);
            },
            voxel: (v) => {
              geometry.setIndex(null);
              geometry.setAttribute("position", new THREE.BufferAttribute(f32View(v.data, v.dims[0] * v.dims[1] * v.dims[2]), 3));
              renderer.render(scene, camera);
            },
          })),
          resize: (w, h) => Effect.sync(() => {
            renderer.setSize(w, h, false);
            camera.aspect = w / h;
            camera.updateProjectionMatrix();
          }),
        } satisfies ViewportResource;
      }))),
    Match.when("model-viewer", () =>
      Effect.acquireRelease(
        Effect.sync(() => URL.createObjectURL(new Blob([blob.bytes as BlobPart], { type: "model/gltf-binary" }))),
        (src) => Effect.sync(() => URL.revokeObjectURL(src)),
      ).pipe(Effect.map((src) => {
        const el = document.createElement("model-viewer") as HTMLElement & { src: string };
        el.setAttribute("camera-controls", "");
        el.src = src;
        canvas.replaceWith(el);
        return {
          canvas,
          draw: () => Effect.void,
          resize: (w, h) => Effect.sync(() => { el.style.width = `${w}px`; el.style.height = `${h}px`; }),
        } satisfies ViewportResource;
      }))),
    Match.exhaustive,
  );

const mount = (blob: ArtifactBlob, backend: RendererBackend): Effect.Effect<ViewportResource, FaultDetail, Scope.Scope | ViewportHost> =>
  Effect.gen(function* () {
    const host = yield* ViewportHost;
    const canvas = yield* host.canvas;
    const resource = yield* acquireBackend(canvas, blob, backend);
    const payload = yield* host.geometryOf(blob);
    const view = yield* decodeMeshView(payload);
    yield* resource.draw(view).pipe(Effect.forkScoped);
    return resource;
  });

const viewportCamera: RoleBehavior<
  { readonly state: CameraState; readonly dispatch: (next: CameraState) => void },
  { readonly value: CameraState; readonly onGesture: (g: CameraGesture) => void }
> = {
  role: "core",
  behavior: (props) => ({
    aria: { role: "img", "aria-label": "3D mesh viewport" },
    state: {
      value: props.state,
      onGesture: (g) => props.dispatch(applyGesture(props.state, g)),
    },
    focus: Option.none(),
  }),
};
```

The `f32View`/`u32View` constructors honor the wire `byteOffset` against a pooled/transferred buffer — the typed view is taken over the exact `[byteOffset, byteOffset+byteLength)` window, never `bytes.buffer` re-viewed whole, so no read crosses a sibling allocation in the worker-pool sink. When the `DecodeWorkerPool` transfer contract delivers a 4-byte-aligned sink the view is zero-copy; an unaligned `byteOffset` (a `bytes` field sliced mid-buffer) falls back to a single owned-buffer copy rather than throwing the `RangeError` a misaligned typed-array view raises, so the leaf renders correctly regardless of the sink's alignment. The `MeshTensor` `vertices`/`faces` byte fields cross as `N×3` Float32 and `F×3` Uint32 (the channels.md `MeshTensor` `vertex_count`/`vertices`/`face_count`/`faces` message), the `PointCloudTensor` `data`/`count`/`channels` as `count × channels` Float32, and the `VoxelTensor` `dims`/`data` as one `dims[0]·dims[1]·dims[2]` field — `decodeMeshView` dispatches the three on the `GeometryPayload.kind` oneof `case` under one `Match`, so the `mesh`/`pointCloud`/`voxel` arms are sibling `GeometryView` cases on the one `RendererBackend` axis, never a second viewport, and the `three` row folds each onto its draw (indexed `Mesh`, `Points`, instanced voxel) total under `GeometryView.$match`.

`residentTiles` folds the consumed `GeometryResidencyWire.tiles` — the `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` `ResidencyManifest` minted ONCE in C# — through `host.tileBytes` then the same `geometryOf`/`decodeMeshView` fold, so a content-keyed residency tile decodes on the identical seam as the streamed blob and the leaf re-mints neither the manifest, the `ContentKey`, nor the `:x32` blob-key spelling; the splat-tile arm (`GeometryResidencyWire.splats`) stays unconsumed on this leaf until the upstream Compute splat-payload decode that feeds the C# `SplatSource` lands, the manifest member already crossing as a settled wire row.

The Three.js `WebGPURenderer` rides the `three/webgpu` subpath (the `three.md` catalogue confirms `three/webgpu` re-exports `WebGPURenderer`/`WebGPUBackend`/`WebGLBackend` plus the node-material family and `PostProcessing`); the `init()` promise resolves before the first `render(scene, camera)` and auto-falls-back to `WebGLBackend` when `navigator.gpu` is absent. The `BufferGeometry` `setAttribute`/`setIndex`/`deleteAttribute`/`computeVertexNormals` and the `Points`/`PointsMaterial` surface are the `three.md` catalogue rows. The `@google/model-viewer` `<model-viewer>` custom element registers on import through `customElements.define`, takes a `.glb` object-URL on `src`, and enables orbit through the `camera-controls` attribute / `.cameraControls` property; the GLB object-URL `Blob` carries the `model/gltf-binary` MIME. The TSL compute surface for the cluster-LOD ambition rides the `three/tsl` node-function catalog (`Fn`/`If`/`Loop`/`vec3`) on the `three` row.

RESEARCH [TSL_COMPUTE]: the specific `three/tsl` node-function signatures the meshlet/cluster-LOD compute kernel composes stay RESEARCH until the kernel design is scoped against the consumed `MeshletClusterWire` residency arm; the `WebGPURenderer`/`init()`, the `BufferGeometry`/`Points` draw surface, the `<model-viewer>` element surface, and the `GeometryPayload`/`GeometryResidencyWire` decode are verified and transcribed above.
