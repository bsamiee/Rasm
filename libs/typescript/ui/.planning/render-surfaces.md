# [UI_RENDER_SURFACES]

One page owns the leaf render surfaces — the read-only observation routes `EvidenceTimelineRoute`/`BenchmarkRoute`/`CollectorPanel`, the 2D cartographic `GeoSeriesSurface` over one `GeoSeriesLayer` union owner, and the 3D `GlbViewport` WebGL mesh render leaf over the `interchange` `ArtifactFrameRail` GLB byte-stream. Observation routes are leaf React subscribers at the SAME altitude as the atom binding — read-only views built ON the binding, not a separate domain — the geo surface is one more render leaf over the `interchange` `GeometryRail` projection, and the GLB viewport is one render leaf over the `ArtifactFrameRail` content-addressed blob whose mesh-decode seam is BLOCKED on the upstream `remote-lane#TS_PROJECTION` promotion of the `GeometryPayload(mesh)` descriptor and consumes that generated shape BY REFERENCE, never a re-authored branch-side wire struct. The routes sit on the `projection` folds and read, never emit; dashboards read the collector while instrumentation belongs to `platform/platform-substrate.md`. The page owns no wire cluster directly and consumes the receipt, evidence, snapshot-geometry, and mesh-tensor shapes only through stores and the rails.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                 |
| :-----: | :-------------- | :--------------------------------------------------------------------- |
|   [1]   | RENDER_SURFACES | the read-only routes and the one GeoSeriesLayer geo surface             |
|   [2]   | GLB_VIEWPORT    | the WebGL mesh render leaf over the ArtifactFrameRail GLB byte stream   |
|   [3]   | RESEARCH        | the residual renderer-backend probe and the meshlet/cluster-LOD ambition |

## [2]-[RENDER_SURFACES]

- Owner: the read-only observation routes — `EvidenceTimelineRoute`, `BenchmarkRoute`, `CollectorPanel` — and `GeoSeriesSurface`, the 2D cartographic surface over the single `GeoSeriesLayer` union; each route is a thin subscriber over a `projection` fold or the telemetry collector holding no domain state, and the geo surface owns the renderer and never a second decode.
- Cases: `EvidenceTimelineRoute` carries receipt envelopes whole and renders them in HLC order with the `SkewBand` confidence interval from the `projection` `EvidenceFeed`, so a browser dashboard renders "within +/-N ms confidence" without recomputing the HLC fold; `BenchmarkRoute` renders only when fingerprint-gated by the host-fingerprint shape on `receipts-and-benchmarks.md#TS_PROJECTION`, so an unverifiable claim never displays as verified; `CollectorPanel` reads the telemetry collector; `GeoSeriesSurface` dispatches the renderer total over the `GeoSeriesLayer` union — the base case draws pan-zoom-style-spec cartography over the maplibre `Map` substrate, the overlay case draws the geometry family keyed by `featureKind` as one `GeoJsonLayer` in the `@deck.gl/react` `DeckGL` layer set composited by the `overlayMode` discriminant (the `DeckGL` component reverse-controls the maplibre `Map` as its child base map, or the layer set renders standalone over a free `MapViewState`). The four prior loose aliases (`MapSubstrate`, `OverlayMode`, `GeometryFeatureKind`, `GeoSeriesComposition`) are deleted and folded into the one union owner — relocating spam is not collapsing it.
- Entry: dashboards sit on the evidence fold and the receipt store and read, never emit; the read-versus-emit split is explicit — dashboards read the collector while instrumentation belongs to `platform/platform-substrate.md`; the routes subscribe to their fold only through the one `binding.md` `AtomBinding`; `GeoSeriesSurface` sources its geometry only through the `interchange` `GeometryRail` decoded to the GeoJSON projection on `snapshot-codecs.md#TS_PROJECTION`, and the maplibre `Map` instance is held as an `Effect.acquireRelease` resource bound under the `platform/host-runtime.md` `BrowserPlatform`, never a free React ref.
- Packages: `react` for the route components, `@tanstack/react-virtual` for the virtualized timeline (composed through the component-system), `@effect/opentelemetry` used strictly as a collector reader, `maplibre-gl` for the `Map` base-map substrate, `@deck.gl/core` for the `Deck`/`MapViewState` view state, `@deck.gl/layers` for the `GeoJsonLayer` overlay constructor, `@deck.gl/react` for the `DeckGL` component that reverse-controls the maplibre `Map` as its child base map, and `effect` for the `Schema.Literal` union and the `acquireRelease` resource.
- Growth: a new observation route lands as one route module over an existing store; a new telemetry panel reads the same collector; a new geometry-feature kind lands as one `featureKind` literal, a new overlay mode as one `overlayMode` literal, a new base substrate as one union case, never a parallel surface.
- Boundary: a benchmark claim displayed without the fingerprint gate is the named defect; `CollectorPanel` crosses no wire contract of its own and references no telemetry wire type; the routes never re-decode a value the `interchange` rail admitted; the four geo aliases are collapsed into one union and never restated as parallel const objects; a second decode of the geometry beside `GeometryRail` is the named defect; the 3D GLB viewport is the sibling `GLB_VIEWPORT` cluster on this page over the `ArtifactFrameRail` blob and the promoted mesh `#TS_PROJECTION` — it never re-decodes the GLB bytes beside the rail and never reaches a C# geometry interior.

```ts contract
const GeoSeriesLayer = Schema.Union(
  Schema.Struct({
    _tag: Schema.Literal("base"),
    substrate: Schema.Literal("maplibre-base"),
    styleSpec: Schema.Unknown,
    viewState: Schema.Unknown,
  }),
  Schema.Struct({
    _tag: Schema.Literal("overlay"),
    substrate: Schema.Literal("deckgl-overlay"),
    featureKind: Schema.Literal("point", "path", "polygon", "mesh-projection"),
    overlayMode: Schema.Literal("interleaved", "overlaid"),
  }),
);
type GeoSeriesLayer = Schema.Schema.Type<typeof GeoSeriesLayer>;

const renderLayer = (layer: GeoSeriesLayer, geojson: GeoJSON.FeatureCollection): StyleSpecification | LayersList =>
  Match.value(layer).pipe(
    Match.tag("base", (b) => b.styleSpec as StyleSpecification),
    Match.tag("overlay", (o) => [
      new GeoJsonLayer({ id: o.featureKind, data: geojson, pickable: true, stroked: true, filled: true }),
    ]),
    Match.exhaustive,
  );

interface BenchmarkRoute {
  readonly render: (claim: BenchmarkClaimWire, host: Option.Option<HostFingerprintWire>) => Option.Option<React.ReactElement>;
}

// stampLine reproduces the upstream HostFingerprint.StampLine() projection VERBATIM — the
// receipts-and-benchmarks#TS_PROJECTION shape carries no machineKey; identity IS the ordinal
// stamp-line over { os, arch, processors, stamps } sorted by key, matching BenchmarkClaim.Stale().
const stampLine = (f: HostFingerprintWire): string =>
  ReadonlyArray.fromIterable(Object.entries(f.stamps)).pipe(
    ReadonlyArray.sort(Order.mapInput(Order.string, ([k]: readonly [string, string]) => k)),
    ReadonlyArray.map(([k, v]) => `${k}=${v}`),
    ReadonlyArray.join(";"),
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

## [3]-[GLB_VIEWPORT]

- [BLOCKED] PRECONDITION: the mesh-decode entry point is UNDONE until the C# `remote-lane#TS_PROJECTION` cluster promotes `GeometryPayload(mesh)`/`MeshTensor` out of `[PROTO_VOCABULARY]` into the projection fence — today that cluster carries only `StreamKind`/`MethodShape`/`TransportCapabilityWire`/`TransportFramingWire`/`FaultDetailWire`/`ArtifactFrameWire` and the five service shapes, with NO mesh wire type, so `decodeMeshView` consumes the generated `GeometryPayload` oneof descriptor BY REFERENCE and a branch-side `MeshTensorWire` re-authoring the proto interior is the named boundary defect. The `region-map/seam-splits.md` four-end DAG names this the single true blocker; until it clears, this cluster's backend/draw/camera owners are authored but the decode seam stays a blocked-on-upstream reference. The GLB BYTE layer is unblocked — `interchange` `ArtifactFrameRail` already reassembles the content-addressed `ArtifactBlob` over the existing fence; only the mesh-TENSOR projection (the typed view over the blob bytes) is deferred.
- Owner: `GlbViewport`, the WebGL mesh render leaf consuming the content-addressed GLB artifact byte-stream the `interchange` `ArtifactFrameRail` reassembles into one `ArtifactBlob` — the same content-key blob the C# `remote-lane#TS_PROJECTION` `Solve`/`Generate`/`SubtreeFetch` server-stream delivers and the Python `libs/python/compute` IFC->GLB two-hop companion re-enters over the identical remote lane. `RendererBackend`, the `Schema.Literal` renderer-backend axis over the four engine rows; `ViewportResource`, the `Effect.acquireRelease`-managed GL context bound under the `platform/host-runtime.md` `BrowserPlatform`, never a free React ref or a raw `WebGLRenderingContext` leaked past its scope; `MeshDraw`, the in-memory `MeshView`->draw fold over the GENERATED `GeometryPayload(mesh)` descriptor (decoded by reference once the upstream fence promotes it; `vertices` `bytes` as Float32 N×3, `faces` `bytes` as Uint32 F×3), never a C# geometry interior, never a hand-minted wire struct, and never a second decode beside the `ArtifactFrameRail` blob. The viewport is one render leaf at the SAME altitude as the atom binding, holding no domain state: it reads the blob through the rail, decodes the mesh-tensor view, uploads one GPU resource per content key, and disposes on scope exit.
- Cases: `RendererBackend` dispatches the upload-and-draw total over four engine rows folded INTO the `acquireBackend` `Match` arms — `three` (the `WebGLRenderer` + `BufferGeometry` baseline whose `draw` calls `setAttribute("position", new THREE.BufferAttribute(view.vertices, 3))` + `setIndex(new THREE.BufferAttribute(view.indices, 1))`), `babylon` (the `Engine` whose `draw` builds a `VertexData` with `positions`/`indices` and calls `applyToMesh` on the owned `Mesh`), `model-viewer` (the declarative `<model-viewer>` custom element whose `draw` sets the `src` to an object-URL `Blob` over the GLB bytes, the zero-GL-handle read-only row), and `webgpu` (the `GPUDevice` whose `draw` runs `createBuffer`/`writeBuffer` for the vertex+index sets, the meshlet path reserved for the cluster-LOD ambition); the engine literal owns acquire AND draw in one arm so a backend swap is one literal value, never a parallel viewport and never a sibling `threeResource`/`babylonResource`/`webgpuResource`/`modelViewerResource` factory. `MeshDraw` folds the decoded `GeometryPayload(mesh)` into one `MeshView` (`vertices: Float32Array`, `indices: Uint32Array`, `vertexCount`, `faceCount`), and the draw fold matches the `RendererBackend` literal total inside each acquire arm. `ViewportCamera` is one render-surface-local `RoleBehavior<CameraProps, CameraState>` row on the component-system `core`/`gesture` leaf — a `RoleBehavior` ROW, never a sibling behavior type — whose pointer-gesture intents fold through the `CameraGesture` `Data.TaggedEnum` (`orbit`/`pan`/`dolly`/`frame`) under one `Match.tagsExhaustive` wired INTO the `behavior` so a `CameraGesture` tag drives `CameraState` through the binding, never an orphaned fold.
- Entry: `GlbViewport.mount(blob, backend)` acquires the GL context as an `Effect.acquireRelease` scoped resource, decodes the mesh view through the generated `GeometryPayload` descriptor, uploads one GPU buffer set per `contentKey`, and registers the render loop as an `Effect.forkScoped` fiber torn down LIFO on scope exit; the `ArtifactBlob` arrives only through the `interchange` `ArtifactFrameRail` (the viewport sources no wire contract of its own and re-decodes nothing the rail admitted); the camera state reads and writes only through the `binding.md` `AtomBinding`, never a second state binding; the heavy mesh-tensor decode and the XxHash128 content-key verify already ran off the main thread under the `platform` `DecodeWorkerPool` at the rail seam, so the viewport receives a verified blob and never re-hashes. The `FaultDetail` fault family is the `interchange` `codec-rails.md#FAULT_FAMILY` owner imported by reference — never re-declared on-page.
- Packages: `three` for the `WebGLRenderer`/`BufferGeometry`/`GLTFLoader` baseline, `@babylonjs/core` for the `Engine`/`VertexData` row, `@google/model-viewer` for the declarative read-only embed, `@webgpu/types` for the `GPUDevice`/`GPUBuffer` meshlet types, `react-aria`/`react-stately` for the `core`/`gesture` `RoleBehavior` pointer contract the `ViewportCamera` row composes (never re-authored), and `effect` for the `Schema.Literal` backend axis, the `acquireRelease` resource, the `Data.TaggedEnum` gesture fold, and the `Match.exhaustive` draw dispatch. Each package is admitted-on-precondition: the WebGL upload/draw/camera owners are authored against the in-memory `MeshView` now, but the mesh-DECODE seam stays a blocked-on-upstream reference to the generated `GeometryPayload` descriptor until the C# `remote-lane#TS_PROJECTION` fence promotes it — admitting the decode before that fence exists is the named premature-admission defect.
- Growth: a new engine lands as one `RendererBackend` literal carrying its acquire arm and draw arm, never a parallel viewport; a new geometry payload kind (the `GeometryPayload` `point_cloud`/`voxel` oneof arms) lands as one `MeshView` sibling case on the same decode fold keyed by the wire `kind` discriminant, never a second viewport surface; a new camera intent lands as one `CameraGesture` tag; a new render mode (wireframe, normals, LOD) lands as one `MeshDraw` parameter row, never a sibling renderer.
- Boundary: the viewport reads ONLY the generated `GeometryPayload(mesh)` descriptor (consumed by reference once promoted upstream) and the `ArtifactFrameRail` `ArtifactBlob` — re-authoring a branch-side `MeshTensorWire`, reaching a C# geometry interior, or a NetTopologySuite/RhinoCommon host projection is the named defect; a second decode of the GLB bytes beside the `ArtifactFrameRail` is the named defect; a GL context held as a free React ref instead of an `Effect.acquireRelease` resource is the named defect — the context, the buffers, and the render-loop fiber all release LIFO on scope exit; the camera is one `RoleBehavior` row on the `core`/`gesture` leaf and a parallel viewport-gesture handler type is the named defect; `model-viewer` is the zero-GL-handle read-only embed row and never holds a `WebGLRenderingContext` the scope must release; the `webgpu` row is the cluster-LOD meshlet ambition and degrades to the `three` row when `navigator.gpu` is absent, the degrade folded through the same backend literal, never a second feature-detect branch.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { ArtifactBlob } from "@rasm/ts/interchange/codec-rails";
import type { GeometryPayload } from "@rasm/ts/gen/remote_lane_pb"; // generated descriptor; mesh oneof arm pending the upstream remote-lane#TS_PROJECTION promotion
import type { FocusManager, RoleBehavior } from "@rasm/ts/ui/component-system";
import { BrowserPlatform } from "@rasm/ts/platform/host-runtime";
import { FaultDetail } from "@rasm/ts/interchange/codec-rails"; // FAULT_FAMILY owner — constructor namespace AND error channel; never re-declared here

// --- [MODELS] --------------------------------------------------------------------------
const RendererBackend = Schema.Literal("three", "babylon", "model-viewer", "webgpu");
type RendererBackend = Schema.Schema.Type<typeof RendererBackend>;

// MeshView is the IN-MEMORY decoded geometry the draw arms upload — NOT a wire shape. No branch-side
// MeshTensorWire exists: the wire type is the generated GeometryPayload(mesh) oneof arm, consumed by
// reference once remote-lane#TS_PROJECTION promotes it. decodeMeshView takes the oneof envelope from
// the start (not a concrete tensor), so the point_cloud/voxel oneof arms land as sibling Match cases
// on this one fold (RESEARCH [POINT_CLOUD_VOXEL]), never a second decode entry point.
interface MeshView {
  readonly vertices: Float32Array;
  readonly indices: Uint32Array;
  readonly vertexCount: number;
  readonly faceCount: number;
}

// [BLOCKED] decodeMeshView discriminates the GENERATED GeometryPayload oneof by its `kind` case. The
// `mesh` arm is the only one fenced here; `point_cloud`/`voxel` re-enter as sibling cases on this same
// Match once their oneof arms ride a draw row. Each typed view is constructed subarray-safe: the wire
// `vertices`/`faces` Uint8Array may carry a non-zero byteOffset against a pooled/transferred buffer, so
// the view is taken over the EXACT [byteOffset, byteOffset+byteLength) window — never `bytes.buffer`
// re-viewed whole — guaranteeing no read past a sibling allocation in the worker-pool sink.
const f32View = (b: Uint8Array, count: number): Float32Array =>
  new Float32Array(b.buffer, b.byteOffset, count * 3); // byteOffset honored; the DecodeWorkerPool transfer contract guarantees 4-byte alignment of the artifact sink
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
// The RendererBackend literal owns acquire AND draw in ONE arm — no sibling threeResource/babylonResource/
// webgpuResource/modelViewerResource factories. Each arm acquireReleases its engine handle and returns a
// ViewportResource whose `draw` body IS the per-backend GL upload this page exists to author.
const acquireBackend = (
  canvas: HTMLCanvasElement,
  blob: ArtifactBlob,
  backend: RendererBackend,
): Effect.Effect<ViewportResource, FaultDetail, Scope.Scope> =>
  Match.value(backend).pipe(
    Match.when("three", () =>
      Effect.acquireRelease(
        Effect.sync(() => new THREE.WebGLRenderer({ canvas, antialias: true })),
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
    Match.when("babylon", () =>
      Effect.acquireRelease(
        Effect.sync(() => new BABYLON.Engine(canvas, true)),
        (engine) => Effect.sync(() => engine.dispose()),
      ).pipe(Effect.map((engine) => {
        const scene = new BABYLON.Scene(engine);
        const target = new BABYLON.Mesh("glb", scene);
        new BABYLON.ArcRotateCamera("cam", 0, 0.6, 1, BABYLON.Vector3.Zero(), scene).attachControl(canvas, true);
        return {
          canvas,
          draw: (mesh) => Effect.sync(() => {
            const data = new BABYLON.VertexData();
            data.positions = mesh.vertices;
            data.indices = mesh.indices;
            data.applyToMesh(target, true);
            scene.render();
          }),
          resize: (w, h) => Effect.sync(() => engine.setSize(w, h)),
        } satisfies ViewportResource;
      }))),
    Match.when("webgpu", () =>
      // navigator.gpu absence and a null adapter are typed FaultDetail.HopFault on the rail — never a
      // `?? raise()` throw nor a `a!` non-null assertion. catchAll degrades to the `three` literal arm.
      Effect.acquireRelease(
        Effect.flatMap(
          Effect.fromNullable(navigator.gpu).pipe(
            Effect.mapError(() => FaultDetail.HopFault({ code: "webgpu-unavailable", evidence: {} })),
          ),
          (gpu) =>
            Effect.tryPromise({
              try: () => gpu.requestAdapter(),
              catch: () => FaultDetail.HopFault({ code: "webgpu-adapter-failed", evidence: {} }),
            }).pipe(
              Effect.flatMap((adapter) =>
                Effect.fromNullable(adapter).pipe(
                  Effect.mapError(() => FaultDetail.HopFault({ code: "webgpu-no-adapter", evidence: {} })),
                )),
              Effect.flatMap((adapter) =>
                Effect.tryPromise({
                  try: () => adapter.requestDevice(),
                  catch: () => FaultDetail.HopFault({ code: "webgpu-device-failed", evidence: {} }),
                })),
            ),
        ),
        (device) => Effect.sync(() => device.destroy()),
      ).pipe(
        Effect.map((device) => {
          const ctx = canvas.getContext("webgpu") as GPUCanvasContext;
          ctx.configure({ device, format: navigator.gpu.getPreferredCanvasFormat(), alphaMode: "premultiplied" });
          return {
            canvas,
            draw: (mesh) => Effect.sync(() => {
              const vbuf = device.createBuffer({ size: mesh.vertices.byteLength, usage: GPUBufferUsage.VERTEX | GPUBufferUsage.COPY_DST });
              const ibuf = device.createBuffer({ size: mesh.indices.byteLength, usage: GPUBufferUsage.INDEX | GPUBufferUsage.COPY_DST });
              device.queue.writeBuffer(vbuf, 0, mesh.vertices);
              device.queue.writeBuffer(ibuf, 0, mesh.indices);
            }),
            resize: (w, h) => Effect.sync(() => { canvas.width = w; canvas.height = h; }),
          } satisfies ViewportResource;
        }),
        Effect.catchAll(() => acquireBackend(canvas, blob, "three")),
      )),
    Match.when("model-viewer", () =>
      Effect.acquireRelease(
        Effect.sync(() => URL.createObjectURL(new Blob([blob.bytes as BlobPart], { type: "model/gltf-binary" }))),
        (src) => Effect.sync(() => URL.revokeObjectURL(src)),
      ).pipe(Effect.map((src) => {
        const el = document.createElement("model-viewer") as HTMLElement & { src: string };
        el.setAttribute("camera-controls", "");
        el.src = src;
        canvas.replaceWith(el); // zero-GL-handle read-only embed: no WebGLRenderingContext to release
        return {
          canvas,
          draw: () => Effect.void, // the <model-viewer> element renders the GLB object-URL itself
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
    const payload = yield* platform.decodePool.geometryOf(blob); // GeometryPayload oneof off the main thread; the XxHash128 verify already ran at the rail seam
    const view = yield* decodeMeshView(payload);
    yield* resource.draw(view).pipe(Effect.forkScoped);
    return resource;
  });

// --- [GROUPS] --------------------------------------------------------------------------
type CameraGesture = Data.TaggedEnum<{
  readonly orbit: { readonly dx: number; readonly dy: number };
  readonly pan: { readonly dx: number; readonly dy: number };
  readonly dolly: { readonly delta: number };
  readonly frame: { readonly bounds: MeshView };
}>;
const CameraGesture = Data.taggedEnum<CameraGesture>();

interface CameraState {
  readonly azimuth: number;
  readonly elevation: number;
  readonly distance: number;
  readonly target: readonly [number, number, number];
}

const centroidOf = (bounds: MeshView): readonly [number, number, number] =>
  ReadonlyArray.range(0, bounds.vertexCount - 1).pipe(
    ReadonlyArray.reduce([0, 0, 0] as const, (acc, i) => [
      acc[0] + bounds.vertices[i * 3],
      acc[1] + bounds.vertices[i * 3 + 1],
      acc[2] + bounds.vertices[i * 3 + 2],
    ] as const),
    (sum) => [sum[0] / bounds.vertexCount, sum[1] / bounds.vertexCount, sum[2] / bounds.vertexCount] as const,
  );

const radiusOf = (bounds: MeshView): number => {
  const c = centroidOf(bounds);
  return ReadonlyArray.range(0, bounds.vertexCount - 1).pipe(
    ReadonlyArray.reduce(0, (max, i) =>
      Math.max(max, Math.hypot(
        bounds.vertices[i * 3] - c[0],
        bounds.vertices[i * 3 + 1] - c[1],
        bounds.vertices[i * 3 + 2] - c[2],
      ))),
  );
};

const applyGesture = (state: CameraState, gesture: CameraGesture): CameraState =>
  Match.value(gesture).pipe(
    Match.tagsExhaustive({
      orbit: (g) => ({ ...state, azimuth: state.azimuth + g.dx, elevation: state.elevation + g.dy }),
      pan: (g) => ({ ...state, target: [state.target[0] + g.dx, state.target[1] + g.dy, state.target[2]] as const }),
      dolly: (g) => ({ ...state, distance: Math.max(0.01, state.distance * (1 + g.delta)) }),
      frame: (g) => ({ azimuth: 0, elevation: 0.6, distance: radiusOf(g.bounds) * 2.5, target: centroidOf(g.bounds) }),
    }),
  );

// viewportCamera wires applyGesture INTO the RoleBehavior: a CameraGesture tag drives CameraState through
// the AtomBinding cell, so the gesture fold is never orphaned. `state` carries the current cell value and
// `dispatch` folds an incoming intent forward, closing the gesture→state→binding loop.
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
    focus: { focus: () => {}, focusFirst: () => {}, focusLast: () => {} } satisfies FocusManager,
  }),
};
```

## [4]-[RESEARCH]

- [RENDERER_BACKEND]: the live-host renderer-backend selection is the residual probe — `three` is the settled baseline row, `model-viewer` the zero-GL-handle read-only embed, and the `webgpu` meshlet/cluster-LOD path enters only when `navigator.gpu` admits a `GPUDevice` on the target host and the GLB tessellation density justifies the cluster-LOD upload over the `BufferGeometry` baseline; the degrade-to-`three` arm is folded through the same backend literal so the probe never spawns a second viewport. The upstream mesh `#TS_PROJECTION` promotion of `GeometryPayload(mesh)`/`MeshTensor` is the BLOCKING precondition, NOT discharged — the four-end cross-branch precondition-DAG (recorded in `region-map/seam-splits.md`, line 50) names `(a)` the C# `remote-lane#TS_PROJECTION` mesh-shape promotion out of `[PROTO_VOCABULARY]` as the single true blocker, and the upstream cluster does NOT yet carry the mesh wire type; `(b)` C# `interchange.md` SharpGLTF/GeometryGym pinning is the only end discharged; `(c)` the Python `libs/python/compute` IFC->GLB two-hop companion is observed, not forced. Until `(a)` physically lands the promoted shape in the projection fence, the TS mesh-decode seam stays a blocked-on-upstream reference to the generated descriptor and `GlbViewport` is a deferred horizon (region-map `owner-symbols.md` records it as `[REFINEMENT_HORIZON; no leaf this turn]`), never a discharged spike — cross-page self-attestation of discharge is not evidence. The GLB byte/artifact layer was never on this DAG — `interchange` `ArtifactFrameRail` reassembles the content-addressed GLB bytes today; the backend/draw/camera owners above are authored against the in-memory `MeshView`, but the WebGL packages are admitted-on-precondition and the decode itself remains gated on `(a)`.
- [POINT_CLOUD_VOXEL]: the `GeometryPayload` `point_cloud`/`voxel` oneof arms re-enter the `MeshDraw` fold as `MeshView` sibling cases keyed by the wire `kind` discriminant — the `PointCloudTensor` (count×channels Float32 view) and `VoxelTensor` (NCHW dims) decode to a point-sprite draw and a voxel-instanced draw on the same `RendererBackend` axis, observed-only growth of one decode case per oneof arm, never a second viewport.
