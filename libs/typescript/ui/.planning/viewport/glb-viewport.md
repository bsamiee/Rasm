# [UI_GLB_VIEWPORT]

The 3D mesh render leaf over the `interchange` `ArtifactFrameRail` GLB byte-stream. `GlbViewport` consumes the content-addressed `ArtifactBlob` the rail reassembles, decodes the mesh-tensor view through the generated `GeometryPayload(mesh)` descriptor BY REFERENCE, uploads one GPU resource per content key, and disposes on scope exit. The renderer is one Three.js row whose `WebGPURenderer` auto-detects WebGPU and falls back to WebGL with zero config, plus the `model-viewer` declarative read-only embed row — given universal WebGPU, no separate Babylon or raw-WebGPU backend. The mesh-DECODE seam is BLOCKED on the upstream C# wire promotion; the GLB byte layer is already unblocked. The viewport is one render leaf at the same altitude as the atom binding, holds no domain state, and reads camera state only through the `binding/atom-binding.md` `AtomBinding`.

## [1]-[INDEX]

One cluster: `GLB_VIEWPORT` owns the mesh render leaf, the renderer backend, and the camera fold.

## [2]-[GLB_VIEWPORT]

- [BLOCKED] PRECONDITION: the mesh-decode entry point is undone until the C# `interchange` `remote-lane#TS_PROJECTION` cluster promotes `GeometryPayload(mesh)`/`MeshTensor` out of the proto vocabulary into the projection fence; today that cluster carries no mesh wire type, so `decodeMeshView` consumes the generated `GeometryPayload` oneof descriptor BY REFERENCE and a branch-side `MeshTensorWire` re-authoring the proto interior is the named boundary defect. The GLB BYTE layer is unblocked — `interchange` `ArtifactFrameRail` already reassembles the content-addressed `ArtifactBlob`; only the mesh-TENSOR projection over the blob bytes is deferred. The backend/draw/camera owners are authored against the in-memory `MeshView` now, but the decode seam stays a blocked-on-upstream reference until the fence promotes the shape.
- Owner: `GlbViewport`, the mesh render leaf consuming the content-addressed GLB byte-stream the `interchange` `ArtifactFrameRail` reassembles into one `ArtifactBlob` — the same content-key blob the C# `remote-lane#TS_PROJECTION` server-stream delivers and the Python IFC->GLB companion re-enters over the identical remote lane. `RendererBackend`, the `Schema.Literal` two-row backend axis; `ViewportResource`, the `Effect.acquireRelease`-managed GL context bound under the `platform` `BrowserPlatform`, never a free React ref; `MeshDraw`, the in-memory `MeshView`->draw fold over the generated `GeometryPayload(mesh)` descriptor (`vertices` bytes as Float32 N×3, `faces` bytes as Uint32 F×3). The viewport reads the blob through the rail, decodes the mesh-tensor view, uploads one GPU resource per content key, and disposes on scope exit.
- Cases: `RendererBackend` dispatches the upload-and-draw total over two rows folded INTO the `acquireBackend` `Match` arms — `three` (the Three.js `WebGPURenderer` whose `init()` auto-detects WebGPU and falls back to WebGL with zero config, whose `draw` sets `BufferGeometry` `position`/`index` attributes over the `MeshView`) and `model-viewer` (the declarative `<model-viewer>` custom element whose `draw` sets the `src` to an object-URL `Blob` over the GLB bytes, the zero-GL-handle read-only row). The engine literal owns acquire AND draw in one arm so a backend swap is one literal value, never a parallel viewport. `MeshDraw` folds the decoded `GeometryPayload(mesh)` into one `MeshView` (`vertices: Float32Array`, `indices: Uint32Array`, `vertexCount`, `faceCount`). `ViewportCamera` is one render-surface-local `RoleBehavior<CameraProps, CameraState>` row on the `component-system/role-behavior.md` `core` leaf whose pointer-gesture intents fold through the `motion/gesture-algebra.md` `CameraGesture` `Data.TaggedEnum` under one `Match.tagsExhaustive` wired INTO the behavior so a `CameraGesture` tag drives `CameraState` through the binding, never an orphaned fold. The meshlet/cluster-LOD ambition rides Three.js TSL compute on the `three` row, never a hand-authored `GPUDevice` path.
- Entry: `GlbViewport.mount(blob, backend)` acquires the GL context as an `Effect.acquireRelease` scoped resource, decodes the mesh view through the generated `GeometryPayload` descriptor, uploads one GPU buffer set per `contentKey`, and registers the render loop as an `Effect.forkScoped` fiber torn down LIFO on scope exit; the `ArtifactBlob` arrives only through the `interchange` `ArtifactFrameRail`; the camera state reads and writes only through the `binding/atom-binding.md` `AtomBinding`; the heavy mesh-tensor decode and the XxHash128 content-key verify already ran off the main thread under the `platform` `DecodeWorkerPool` at the rail seam, so the viewport receives a verified blob and never re-hashes. The `FaultDetail` fault family is the `interchange` `codec-rails#FAULT_FAMILY` owner imported by reference. A backgrounded viewport stays mounted-but-hidden through the React `<Activity>` surface (the `motion/view-transitions.md` owner), preserving the GL context and uploaded buffers across visibility toggles instead of teardown.
- Packages: `three`, `@google/model-viewer`, `@webgpu/types`, `react-aria`, `react-stately`, `effect`. Each is admitted-on-precondition: the upload/draw/camera owners are authored against the in-memory `MeshView` now, but the mesh-DECODE seam stays a blocked-on-upstream reference until the C# `remote-lane#TS_PROJECTION` fence promotes the shape — admitting the decode before that fence exists is the named premature-admission defect.
- Growth: a new engine lands as one `RendererBackend` literal carrying its acquire arm and draw arm; a new geometry payload kind (the `GeometryPayload` `point_cloud`/`voxel` oneof arms) lands as one `MeshView` sibling case on the same decode fold keyed by the wire `kind` discriminant, never a second viewport surface; a new camera intent lands as one `CameraGesture` tag; a new render mode (wireframe, normals, LOD) lands as one `MeshDraw` parameter row.
- Boundary: the viewport reads ONLY the generated `GeometryPayload(mesh)` descriptor (consumed by reference once promoted upstream) and the `ArtifactFrameRail` `ArtifactBlob` — re-authoring a branch-side `MeshTensorWire`, reaching a C# geometry interior, or a host projection is the named defect; a second decode of the GLB bytes beside the `ArtifactFrameRail` is the named defect; a GL context held as a free React ref instead of an `Effect.acquireRelease` resource is the named defect — the context, the buffers, and the render-loop fiber all release LIFO on scope exit; the camera is one `RoleBehavior` row and a parallel viewport-gesture handler type is the named defect; `model-viewer` is the zero-GL-handle read-only embed row; the WebGL fallback is the Three.js `WebGPURenderer` auto-detect, never a hand-rolled `navigator.gpu` degrade branch beside it.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { ArtifactBlob } from "@rasm/ts/interchange/codec-rails";
import type { GeometryPayload } from "@rasm/ts/gen/remote_lane_pb";
import type { RoleBehavior } from "@rasm/ts/ui/component-system/role-behavior";
import type { CameraGesture, CameraState } from "@rasm/ts/ui/motion/gesture-algebra";
import { BrowserPlatform } from "@rasm/ts/platform/host-runtime";
import { FaultDetail } from "@rasm/ts/interchange/codec-rails";

// --- [MODELS] --------------------------------------------------------------------------
const RendererBackend = Schema.Literal("three", "model-viewer");
type RendererBackend = Schema.Schema.Type<typeof RendererBackend>;

interface MeshView {
  readonly vertices: Float32Array;
  readonly indices: Uint32Array;
  readonly vertexCount: number;
  readonly faceCount: number;
}

const f32View = (b: Uint8Array, count: number): Float32Array =>
  new Float32Array(b.buffer, b.byteOffset, count * 3);
const u32View = (b: Uint8Array, count: number): Uint32Array =>
  new Uint32Array(b.buffer, b.byteOffset, count * 3);

const decodeMeshView = (payload: GeometryPayload): Effect.Effect<MeshView, FaultDetail> =>
  Match.value(payload).pipe(
    Match.when({ kind: { case: "mesh" } }, ({ kind: { value: m } }) =>
      Effect.succeed<MeshView>({
        vertices: f32View(m.vertices, Number(m.vertexCount)),
        indices: u32View(m.faces, Number(m.faceCount)),
        vertexCount: Number(m.vertexCount),
        faceCount: Number(m.faceCount),
      })),
    Match.orElse((p) =>
      Effect.fail(FaultDetail.HopFault({ code: "geometry-kind-unsupported", evidence: { kind: p.kind.case ?? "none" } }))),
  );

// --- [SERVICES] ------------------------------------------------------------------------
interface ViewportResource {
  readonly canvas: HTMLCanvasElement;
  readonly draw: (mesh: MeshView) => Effect.Effect<void>;
  readonly resize: (width: number, height: number) => Effect.Effect<void>;
}

interface GlbViewport {
  readonly mount: (
    blob: ArtifactBlob,
    backend: RendererBackend,
  ) => Effect.Effect<ViewportResource, FaultDetail, Scope.Scope | BrowserPlatform>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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
        return {
          canvas,
          draw: (mesh) => Effect.sync(() => {
            geometry.setAttribute("position", new THREE.BufferAttribute(mesh.vertices, 3));
            geometry.setIndex(new THREE.BufferAttribute(mesh.indices, 1));
            geometry.computeVertexNormals();
            renderer.render(scene, camera);
          }),
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

const mount = (blob: ArtifactBlob, backend: RendererBackend): Effect.Effect<ViewportResource, FaultDetail, Scope.Scope | BrowserPlatform> =>
  Effect.gen(function* () {
    const platform = yield* BrowserPlatform;
    const canvas = yield* platform.canvas;
    const resource = yield* acquireBackend(canvas, blob, backend);
    const payload = yield* platform.decodePool.geometryOf(blob);
    const view = yield* decodeMeshView(payload);
    yield* resource.draw(view).pipe(Effect.forkScoped);
    return resource;
  });

// --- [GROUPS] --------------------------------------------------------------------------
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
    focus: { focus: () => {}, focusFirst: () => {}, focusLast: () => {} },
  }),
};
```

The `f32View`/`u32View` constructors honor the wire `byteOffset` against a pooled/transferred buffer — the typed view is taken over the exact `[byteOffset, byteOffset+byteLength)` window, never `bytes.buffer` re-viewed whole, so no read crosses a sibling allocation in the worker-pool sink. The `DecodeWorkerPool` transfer contract guarantees 4-byte alignment of the artifact sink. The `point_cloud`/`voxel` oneof arms re-enter the decode fold as `MeshView` sibling cases keyed by the wire `kind` discriminant — a `PointCloudTensor` point-sprite draw and a `VoxelTensor` voxel-instanced draw on the same `RendererBackend` axis — never a second viewport.

RESEARCH [WEBGPU_RENDERER]: the Three.js `WebGPURenderer` import path (`three/webgpu`), the `init()` promise contract, the TSL compute surface for cluster-LOD, and the `@google/model-viewer` custom-element `src`/`camera-controls` attributes are unverified; the `WebGPURenderer` constructor and `init()` signature stay RESEARCH until the mesh-wire promotion lands and the folder `.api/` catalogue carries the `three`/`@google/model-viewer`/`@webgpu/types` rows.
