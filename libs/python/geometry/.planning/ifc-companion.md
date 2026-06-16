# [PY_GEOMETRY_IFC_COMPANION]

The persistent IfcOpenShell tessellation daemon — the load-bearing cross-boundary owner. `IfcCompanion` drives IFC bytes plus deflection/tolerance into GLB + semantic XML/JSON through `IfcConvert` and the `ifcopenshell.geom.iterator`, hosted by the runtime `ServerHost` speaking the EXISTING C# `ComputeService`/`ArtifactSync` gRPC contract over the remote-lane `TRANSPORT_AXIS` UDS/InProcess leg. Output is content-addressed via runtime `ContentIdentity` reproducing the C# `InterchangeIdentity` XxHash128 seed with deflection/tolerance folded into the cache key; the 64 KiB ArtifactSync framing with per-frame Crc32 + whole-artifact XxHash128 transcribes the C# contract field-for-field.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | DAEMON    | the tessellation daemon, the warm pool, the content-addressed output |

## [2]-[DAEMON]

- Owner: `IfcCompanion` — the boundary capsule hosting the `ifcopenshell.geom.iterator` over a command queue; `TessellationRequest` the IFC-bytes + `IdentityPolicy` carrier keyed by `ContentIdentity`; `MeshResult` the per-element GLB + semantic projection.
- Entry: `IfcCompanion.tessellate` opens an `ifcopenshell.file` from bytes, builds `geom.settings()` from the deflection/tolerance policy, iterates over `geom.iterator(settings, model, num_threads)` collecting per-element verts/faces/materials into a GLB, and keys the result through `ContentIdentity.key`; `IfcCompanion.semantic` projects the model header/units/schema into the semantic XML/JSON; `IfcCompanion.warm` pre-loads the OCCT kernel to amortize the 200-400ms cold start.
- Auto: `geom.settings()` knobs `mesher-linear-deflection` and `mesher-angular-deflection` bind from the `IdentityPolicy`, so a re-tessellation at identical settings keys identically and crosses the wire by reference (cache hit); `geometry_library='opencascade'` selects the kernel; the iterator runs over `num_threads` from the `LanePolicy` capacity.
- Receipt: a tessellation contributes a `Receipt.emitted` row through `ReceiptContributor` carrying the content-key, element count, triangle count, and elapsed; a cache hit contributes a `Receipt.admitted` row instead of re-iterating.
- Packages: `ifcopenshell` (`open`/`geom.settings`/`geom.iterator`/`geom.create_shape`/`guess_format`), runtime (`ServerHost`/`ContentIdentity`/`LanePolicy`/`ReceiptContributor`/`RuntimeRail`).
- Growth: a new tessellation knob is one field folded into the `IdentityPolicy` and the `geom.settings()` bind; a new kernel is one `geometry_library` row; zero new surface.
- Boundary: the daemon mints no transport, channel, or wire vocabulary — it speaks the existing C# `ComputeService`/`ArtifactSync` contract through the runtime `ServerHost`; the GLB the companion returns is the same shape the C# SharpGLTF import reads; the IFC semantic graph the C# `IfcSemanticModel` projects in-process is never re-derived here — the companion produces only the tessellated geometry and the lightweight header/units the managed surface cannot tessellate; a per-request process spawn (rejecting the warm pool), an in-process managed-IFC tessellator, and a path-keyed cache are the deleted forms. This owner is `SPIKE` on the companion-floor + live-server probe.

```python signature
import ifcopenshell
import ifcopenshell.geom
from msgspec import Struct

from rasm.runtime.content_identity import CANONICAL_POLICY, ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.observability import ReceiptContributor
from rasm.runtime.rails_resilience import RuntimeRail, boundary


class TessellationRequest(Struct, frozen=True):
    ifc_bytes: bytes
    policy: IdentityPolicy = CANONICAL_POLICY
    num_threads: int = 4


class MeshResult(Struct, frozen=True):
    content_key: ContentKey
    glb: bytes
    element_count: int
    triangle_count: int


class IfcCompanion:
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
        return boundary("ifc.tessellate", lambda: self._run(request))

    def _run(self, request: TessellationRequest) -> MeshResult:
        model = ifcopenshell.file.from_string(request.ifc_bytes.decode())
        it = ifcopenshell.geom.iterator(self._settings(request.policy), model, request.num_threads)
        glb, elements, triangles = _collect_glb(it)
        return MeshResult(ContentIdentity.key("glb", glb, request.policy), glb, elements, triangles)
```

## [3]-[RESEARCH]

- [WARM_POOL]: the `ifcopenshell.geom.iterator` warm-pool reuse pattern amortizing the 200-400ms Python+OCCT cold start, the GLB assembly from per-element `ShapeElementType` verts/faces (the `_collect_glb` body), and the 64 KiB ArtifactSync `FrameEdge` framing with per-frame Crc32 + whole-artifact XxHash128 are proven against the C# `ArtifactSync` descriptors once the floor/lock-scope decision admits the sub-3.13 environment (suite TASKLOG); the `geom.settings().set` knob names confirm against `.api/api-ifcopenshell.md`.
