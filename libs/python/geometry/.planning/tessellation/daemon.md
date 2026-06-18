# [PY_GEOMETRY_TESSELLATION_DAEMON]

The persistent IfcOpenShell tessellation daemon — the load-bearing cross-boundary owner. `TessellationDaemon` drives source bytes plus a deflection/tolerance policy into per-element GLB and a lightweight semantic header through the `ifcopenshell.geom.iterator` and the native `ifcopenshell.geom.serializers.gltf` serializer, hosted by the runtime `ServerHost` over the existing C# `ComputeService`/`ArtifactSync` gRPC contract. Output is content-addressed via runtime `ContentIdentity` reproducing the C# `InterchangeIdentity` XxHash128 seed with deflection/tolerance folded into the cache key, framed by the 64 KiB `ArtifactSync` leg with per-frame Crc32 and whole-artifact XxHash128.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[DAEMON]`: the tessellation request owner, the warm OCCT pool, and the native GLB serializer.

## [2]-[DAEMON]

- Owner: `TessellationDaemon` — the boundary capsule driving the `ifcopenshell.geom.iterator` over a command queue, the OCCT kernel kept warm across requests per `daemon#3-RESEARCH`; `TessellationRequest` the source-bytes + `SourceFormat` + `IdentityPolicy` carrier keyed by `ContentIdentity`; `MeshResult` the per-element GLB and the semantic header projection in one carrier, so one `tessellate` call emits both outputs by reference.
- Cases: `SourceFormat` rows `IFC` (the IfcOpenShell hybrid-CGAL+OCCT kernel over an `ifcopenshell.file`) and the `STEP`/`IGES` rows that route the OCCT B-rep hop on `cad#BRIDGE`; the daemon discriminates source-format by `match` and threads one tessellation queue rather than parallel daemons, the `IFC` arm opening through `ifcopenshell.file.from_string` and the CAD arms delegating the reader to the `step-bridge` owner before re-entering the shared GLB assembly. The IFC arm projects the model `schema_identifier` and `IfcProject` header into the semantic field; the CAD arms carry an empty header, since a B-rep model holds no IFC schema.
- Entry: `TessellationDaemon.tessellate` opens the model from bytes, binds `geom.settings()` from the deflection/tolerance policy, iterates `geom.iterator(settings, model, num_threads, geometry_library="hybrid-cgal-simple-opencascade")` feeding each `ShapeElementType` into the native `serializers.gltf` over a `serializers.buffer`, keys the finalized GLB through `ContentIdentity.of`, and folds the IFC `schema_identifier`/`IfcProject` header into the same `MeshResult` so the GLB and the semantic header cross the wire together; the warm-pool pre-load that amortizes the 200-400 ms OCCT cold start lands once its reuse pattern resolves on `daemon#3-RESEARCH`.
- Auto: `geom.settings()` knobs `mesher-linear-deflection` and `mesher-angular-deflection` bind from the `IdentityPolicy`, so re-tessellation at identical settings keys identically and crosses the wire by reference; the `serializers.gltf` lifecycle (`writeHeader`/`write`/`finalize`) assembles the GLB from the iterator's per-element `verts`/`faces`/`materials`/`material_ids` with zero temp-file I/O; the iterator runs over `num_threads` from the `LanePolicy` capacity, draining `iterator.get()` into `serializer.write` and folding the element/triangle counts in one pass.
- Receipt: a tessellation contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the content-key, element count, triangle count, and elapsed; a cache hit contributes an admitted-phase `Receipt.of` row instead of re-iterating.
- Packages: `ifcopenshell` (`open`/`file.from_string`/`guess_format`/`by_type`/`schema_identifier`/`geom.settings`/`geom.iterator`/`geom.serializers.gltf`/`geom.serializers.buffer`), runtime (`ServerHost`/`ContentIdentity`/`LanePolicy`/`ReceiptContributor`/`RuntimeRail`).
- Growth: a new tessellation knob is one field folded into the `IdentityPolicy` and the `geom.settings()` bind; a new CAD source is one `SourceFormat` row routing the `cad#BRIDGE` hop; zero new surface.
- Boundary: the daemon mints no transport, channel, or wire vocabulary — it speaks the existing C# `ComputeService`/`ArtifactSync` contract through the runtime `ServerHost`; the GLB it returns is the shape the C# `SharpGLTF` import reads; the IFC semantic graph the C# `IfcSemanticModel` projects in-process is never re-derived — the companion produces only the tessellated geometry and the lightweight header the managed surface cannot tessellate. A per-request process spawn, an in-process managed-IFC tessellator, and a path-keyed cache are the deleted forms.

```python signature
import ifcopenshell
import ifcopenshell.geom
from enum import StrEnum
from msgspec import Struct

from rasm.geometry.step_bridge.cad import StepBridge
from rasm.runtime.content_identity import CANONICAL_POLICY, ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.receipts import ReceiptContributor
from rasm.runtime.faults import RuntimeRail, boundary


class SourceFormat(StrEnum):
    IFC = "ifc"
    STEP = "step"
    IGES = "iges"


class TessellationRequest(Struct, frozen=True):
    source_bytes: bytes
    fmt: SourceFormat = SourceFormat.IFC
    policy: IdentityPolicy = CANONICAL_POLICY
    num_threads: int = 4


class MeshResult(Struct, frozen=True):
    content_key: ContentKey
    glb: bytes
    element_count: int
    triangle_count: int
    semantic: dict[str, str]


class TessellationDaemon:
    def __init__(self, contributor: ReceiptContributor) -> None:
        self._contributor = contributor

    @staticmethod
    def _settings(policy: IdentityPolicy) -> "ifcopenshell.geom.settings":
        s = ifcopenshell.geom.settings()
        s.set("mesher-linear-deflection", policy.deflection)
        s.set("mesher-angular-deflection", policy.angle_tolerance)
        s.set("weld-vertices", True)
        s.set("apply-default-materials", True)
        return s

    def tessellate(self, request: TessellationRequest) -> "RuntimeRail[MeshResult]":
        return boundary(f"tessellate.{request.fmt}", lambda: self._run(request))

    def _run(self, request: TessellationRequest) -> MeshResult:
        settings = self._settings(request.policy)
        match request.fmt:
            case SourceFormat.IFC:
                model = ifcopenshell.file.from_string(request.source_bytes.decode())
                projects = model.by_type("IfcProject")
                semantic = {"schema": model.schema_identifier, "project": (projects[0].Name or "") if projects else ""}
            case SourceFormat.STEP | SourceFormat.IGES:
                model = StepBridge.model(request.source_bytes, request.fmt)
                semantic = {}
        buffer = ifcopenshell.geom.serializers.buffer()
        serializer = ifcopenshell.geom.serializers.gltf(buffer, settings, ifcopenshell.geom.serializer_settings())
        serializer.writeHeader()
        iterator = ifcopenshell.geom.iterator(
            settings, model, request.num_threads, geometry_library="hybrid-cgal-simple-opencascade"
        )
        elements = triangles = 0
        if iterator.initialize():
            while True:
                shape = iterator.get()
                serializer.write(shape)
                elements += 1
                triangles += len(shape.geometry.faces) // 3
                if not iterator.next():
                    break
        serializer.finalize()
        glb = buffer.get_value()
        return MeshResult(ContentIdentity.of("glb", glb, request.policy), glb, elements, triangles, semantic)
```

## [3]-[RESEARCH]

- [SERIALIZER_LIFECYCLE]: the `geom.serializers.gltf(buffer, settings, serializer_settings)` constructor arity, the `serializers.buffer().get_value()` retrieval, and the `writeHeader`/`write`/`finalize` order against an iterator-fed `ShapeElementType` confirm against the branch `ifcopenshell` catalogue on the companion interpreter; the `iterator.initialize()`/`get()`/`next()` drain protocol and the per-shape `geometry.faces` triangle count confirm against the same catalogue.
- [WARM_POOL]: the `geom.iterator` warm-pool reuse pattern amortizing the 200-400 ms OCCT cold start, the multithreaded-iteration memory ceiling at high `num_threads`, and the 64 KiB `ArtifactSync` `FrameEdge` framing with per-frame Crc32 and whole-artifact XxHash128 prove against the C# `ArtifactSync` descriptors on the live companion server.
